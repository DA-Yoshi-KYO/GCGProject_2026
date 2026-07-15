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

    private CustomInputAction inputActions;
    private int currentButton = 0;

    private CS_BackGroundPlaySE backGroundPlaySE;

    private Canvas tutorialConfirmationCanvas;

    // Start is called before the first frame update
    void Start()
    {
        inputActions = CS_CustomInputActionManager.instance.customInputAction;
        inputActions.TutorialConfirmation.MoveAxis.started += TutorialConfirmationButtonSelectInput;

        backGroundPlaySE = GameObject.Find("SE").GetComponent<CS_BackGroundPlaySE>();

        tutorialConfirmationCanvas = GetComponent<Canvas>();

        currentButton = 0;
        UpdateButtonTexture();
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

        //決定ボタンでシーン遷移
        if (inputActions.TutorialConfirmation.Decision.triggered)
        {
            //backGroundPlaySE.PlaySE("Decision");
            string sceneName = "";
            switch (currentButton)
            {
                case 0:
                    sceneName = sceneList[0];
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
                //backGroundPlaySE.PlaySE("Cusor");
                currentButton--;
                if (currentButton < 0)
                {
                    currentButton = buttonList.Length - 1;
                }

                UpdateButtonTexture();
            }
            else if (inputFloat < 0.0f)
            {
                //backGroundPlaySE.PlaySE("Cusor");
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
