/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ゲーム失敗のボタンの処理の作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-22 | 初回作成
 * 2026-06-05 | バグの修正
 */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CS_GameFailureSelectButton : MonoBehaviour
{
    [Header("ボタンのリスト（左から順に格納してください）")][SerializeField] private GameObject[] buttonList;
    [Header("リトライのボタン画像")][SerializeField] private Sprite[] retryButtonSprite;
    [Header("タイトルへのボタン画像")][SerializeField] private Sprite[] backTitleButtonSprite;
    [Header("左から順に遷移するシーンの名前を格納")][SerializeField] private string[] sceneTransitionName;


    private Image retryButtonImage;
    private Image backTitleButtonImage;

    private CustomInputAction inputActions;
    private int currentButton = 0;
    private CS_BackGroundPlaySE backGroundPlaySE;

    [Header("フェード処理があるCanvasを格納")][SerializeField]private CS_SceneTransition sceneTransition;

    // Start is called before the first frame update
    void Start()
    {
        inputActions = CS_CustomInputActionManager.instance.customInputAction;
        inputActions.GameClear.MoveAxis.started += SelectInput;

        GameObject retryButtonObject = GameObject.Find("RetryButton");
        retryButtonImage = retryButtonObject != null ? retryButtonObject.GetComponent<Image>() : null;
        if (retryButtonImage != null) retryButtonImage.sprite = retryButtonSprite[currentButton];

        GameObject backTitleButtonObject = GameObject.Find("GameFailureBackTitleButton");
        backTitleButtonImage = backTitleButtonObject != null ? backTitleButtonObject.GetComponent<Image>() : null;
        if (backTitleButtonImage != null) backTitleButtonImage.sprite = backTitleButtonSprite[currentButton];

        GameObject seObject = GameObject.Find("SE");
        backGroundPlaySE = seObject != null ? seObject.GetComponent<CS_BackGroundPlaySE>() : null;

    }

    // Update is called once per frame
    void Update()
    {
        if (retryButtonImage != null) retryButtonImage.sprite = retryButtonSprite[currentButton];
        if (backTitleButtonImage != null) backTitleButtonImage.sprite = backTitleButtonSprite[currentButton];

        //決定ボタンでシーン遷移
        if (inputActions.GameOver.Decision.triggered)
        {
            string sceneName = sceneTransitionName[currentButton];
            sceneTransition.StartSceneTransition(sceneName);
        }
    }

    void SelectInput(InputAction.CallbackContext context)
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
                    currentButton = backTitleButtonSprite.Length - 1;
                }
            }
            else if (inputFloat < 0.0f)
            {
                backGroundPlaySE.PlaySE("Cusor");
                currentButton++;
                if (currentButton >= backTitleButtonSprite.Length)
                {
                    currentButton = 0;
                }
            }
        }
    }
}
