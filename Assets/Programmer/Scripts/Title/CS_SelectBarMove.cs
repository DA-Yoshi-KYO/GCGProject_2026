/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    選択バーの処理の作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-15 | 初回作成
 * 2026-05-17 | SE再生処理追加
 * 2026-06-15 | 修正
 */
using UnityEngine;
using UnityEngine.InputSystem;

public class CS_SelectBarMove : MonoBehaviour
{
    [Header("ボタンのリスト（上から順に格納してください）")][SerializeField] private CS_TitleButton[] buttonList;
    [Header("選択バー")][SerializeField] private GameObject selectBar;

    [Header("FadeCanvasのPrefab格納")][SerializeField] private GameObject fadeCanvas;

    [Header("Manualの画像を格納")][SerializeField] private GameObject[] manualImage;

    private CustomInputAction inputActions;
    private int currentButton = 0;

    private CS_BackGroundPlaySE backGroundPlaySE;

    [Header("GameStartButton押した際にシーン遷移する名前")][SerializeField] public string pressGameStartToSceneName;

    // Start is called before the first frame update
    void Start()
    {
        inputActions = CS_CustomInputActionManager.instance.customInputAction;
        inputActions.SelectBar.MoveAxis.started += TitleSelectInput;

        GameObject seObject = GameObject.Find("SE");
        backGroundPlaySE = seObject != null ? seObject.GetComponent<CS_BackGroundPlaySE>() : null;

        if (manualImage != null && manualImage.Length >= 2)
        {
            manualImage[0].SetActive(true);
            manualImage[1].SetActive(false);
        }

        currentButton = 0;
        UpdateButtonTexture();
    }

    // Update is called once per frame
    void Update()
    {
        if (Option.Instance == null) return;

        switch (CS_CustomInputActionManager.instance.currentInputType)
        {
            case CS_CustomInputActionManager.InputType.Gamepad:
                manualImage[0].SetActive(true);
                manualImage[1].SetActive(false);
                break;
            case CS_CustomInputActionManager.InputType.KeyboardMouse:
                manualImage[0].SetActive(false);
                manualImage[1].SetActive(true);
                break;
        }

        if (!Option.Instance.GetIsOptionUIActive())
        {
            inputActions.SelectBar.Enable();
        }
        else
        {
            inputActions.SelectBar.Disable();
        }

        //座標移動
        Vector3 selectBarPos = selectBar.GetComponent<RectTransform>().anchoredPosition;

        selectBarPos.y = buttonList[currentButton].gameObject.GetComponent<RectTransform>().anchoredPosition.y;

        selectBar.GetComponent<RectTransform>().anchoredPosition = selectBarPos;

        //決定ボタンでシーン遷移
        if (inputActions.SelectBar.Decision.triggered)
        {
            backGroundPlaySE.PlaySE("Decision");
            string sceneName = "";
            switch (currentButton)
            {
                case 0:
                    sceneName = pressGameStartToSceneName;
                    fadeCanvas.GetComponent<CS_SceneTransition>().StartSceneTransition(sceneName);
                    break;
                case 1:
                    Option.Instance.OpenOptionUI();
                    break;
                case 2:
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
            }
        }
    }

    private void UpdateButtonTexture()
    {
        for (int i = 0 ; i < buttonList.Length ; i++)
        {
            buttonList[i].ChangeTexture(i == currentButton);
        }
    }

    void TitleSelectInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            float inputFloat = context.ReadValue<float>();

            //現在選択しているボタンの移動処理
            if (inputFloat > 0.0f)
            {
                backGroundPlaySE.PlaySE("Cusor");
                currentButton--;
                if (currentButton < 0)
                {
                    currentButton = buttonList.Length - 1;
                }

                UpdateButtonTexture();
            }
            else if (inputFloat < 0.0f)
            {
                backGroundPlaySE.PlaySE("Cusor");
                currentButton++;
                if (currentButton >= buttonList.Length)
                {
                    currentButton = 0;
                }

                UpdateButtonTexture();
            }
        }
    }
}
