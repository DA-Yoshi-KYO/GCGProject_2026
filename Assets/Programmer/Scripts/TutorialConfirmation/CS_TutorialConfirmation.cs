/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    チュートリアル確認画面の処理
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-07-8 | 初回作成
 */
using UnityEngine;
using UnityEngine.InputSystem;

public class CS_TutorialConfirmation : MonoBehaviour
{
    [Header("ボタンのリスト（右から順に格納してください）")][SerializeField] private CS_TitleButton[] buttonList;
    [Header("ボタンを押した際にシーン遷移するシーン名前（右から順に格納してください）")][SerializeField] private string[] sceneList;

    [Header("FadeCanvasのPrefab格納")][SerializeField] private GameObject fadeCanvas;
    [Header("MovieCanvasを格納")][SerializeField] private GameObject movieCanvas;

    [Header("背景の画像を格納")][SerializeField] private GameObject[] backGroundImage;

    private CustomInputAction inputActions;
    private int currentButton = 0;

    private CS_BackGroundPlaySE backGroundPlaySE;

    private Canvas tutorialConfirmationCanvas;

    private float uiTimer = 0.0f;
    [Header("UIアニメーションの時間間隔")][SerializeField] private float uiDuration;
    [Header("UIアニメーションの最小の大きさ")][SerializeField] private Vector3 minScale;
    [Header("UIアニメーションの最大の大きさ")][SerializeField] private Vector3 maxScale;
    private bool reversibleScale = false;//拡大・縮小を判定

    private bool endSound = false;

    // Start is called before the first frame update
    void Start()
    {
        inputActions = CS_CustomInputActionManager.instance.customInputAction;
        inputActions.TutorialConfirmation.MoveAxis.started += TutorialConfirmationButtonSelectInput;

        backGroundPlaySE = GameObject.Find("SE").GetComponent<CS_BackGroundPlaySE>();

        tutorialConfirmationCanvas = GetComponent<Canvas>();

        if (backGroundImage != null && backGroundImage.Length >= 2)
        {
            backGroundImage[0].SetActive(true);
            backGroundImage[1].SetActive(false);
        }

        currentButton = 0;
        UpdateButtonTexture();
        UpdateButtonScale();
    }

    private void OnDestroy()
    {
        inputActions.TutorialConfirmation.MoveAxis.started -= TutorialConfirmationButtonSelectInput;
    }

    // Update is called once per frame
    void Update()
    {
        if (enabled)
        {
            //アクティブを切り替える（不必要なため）
           movieCanvas.SetActive(false);
        }

        switch (CS_CustomInputActionManager.instance.currentInputType)
        {
            case CS_CustomInputActionManager.InputType.Gamepad:
                backGroundImage[0].SetActive(true);
                backGroundImage[1].SetActive(false);
                break;
            case CS_CustomInputActionManager.InputType.KeyboardMouse:
                backGroundImage[0].SetActive(false);
                backGroundImage[1].SetActive(true);
                break;
        }

        UpdateButtonScale();

        //決定ボタンでシーン遷移
        if (inputActions.TutorialConfirmation.Decision.triggered)
        {
            if (!endSound)
                backGroundPlaySE.PlaySE("Decision");
            string sceneName = "";
            switch (currentButton)
            {
                case 0:
                    sceneName = sceneList[0];
                    endSound = true;
                    fadeCanvas.GetComponent<CS_SceneTransition>().StartSceneTransition(sceneName);
                    tutorialConfirmationCanvas.sortingOrder = 4;
                    break;
                case 1:
                    sceneName = sceneList[1];
                    fadeCanvas.GetComponent<CS_SceneTransition>().StartSceneTransition(sceneName);
                    tutorialConfirmationCanvas.sortingOrder = 4;
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

    void TutorialConfirmationButtonSelectInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            float inputFloat = context.ReadValue<float>();

            //現在選択しているボタンの移動処理
            if (inputFloat > 0.0f)
            {
                if (!endSound)
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
                if (!endSound)
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

    //選択しているボタンの拡縮処理
    private void UpdateButtonScale()
    {
        for (int i = 0 ; i < buttonList.Length ; i++)
        {
            if (i == currentButton)
            {
                uiTimer += Time.deltaTime * 2.0f;
                float t = Easing.EaseInOutSine(uiTimer, uiDuration);

                if (!reversibleScale)
                {
                    buttonList[i].GetComponent<RectTransform>().localScale = Vector3.Lerp(minScale, maxScale, t);
                }
                else
                {
                    buttonList[i].GetComponent<RectTransform>().localScale = Vector3.Lerp(maxScale, minScale, t);
                }

                if (uiTimer > uiDuration)
                {
                    uiTimer = 0.0f;
                    if (!reversibleScale)
                        reversibleScale = true;
                    else
                        reversibleScale = false;
                }
            }
            else
            {
                buttonList[i].GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
        }
    }
}
