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
        inputActions = new CustomInputAction();
        inputActions.GameOver.Enable();
        inputActions.GameClear.MoveAxis.started += SelectInput;

        retryButtonImage = GameObject.Find("RetryButton").GetComponent<Image>();
        retryButtonImage.sprite = retryButtonSprite[currentButton];

        backTitleButtonImage = GameObject.Find("GameFailureBackTitleButton").GetComponent<Image>();
        backTitleButtonImage.sprite = backTitleButtonSprite[currentButton];
        backGroundPlaySE = GameObject.Find("SE").GetComponent<CS_BackGroundPlaySE>();

    }

    // Update is called once per frame
    void Update()
    {
        retryButtonImage.sprite = retryButtonSprite[currentButton];
        backTitleButtonImage.sprite = backTitleButtonSprite[currentButton];

        //決定ボタンでシーン遷移
        if (inputActions.GameOver.Decision.triggered)
        {
            string sceneName = sceneTransitionName[currentButton];
            sceneTransition.StartSceneTransition(sceneName);
        }
    }

    private void OnDestroy()
    {
        inputActions.GameOver.Disable();
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
