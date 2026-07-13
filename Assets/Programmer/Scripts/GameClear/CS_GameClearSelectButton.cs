/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ゲームクリアのボタンの処理の作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-22 | 初回作成
 * 2026-06-05 | バグの修正
 */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CS_GameClearSelectButton : MonoBehaviour
{
    [Header("ボタンのリスト（左から順に格納してください）")][SerializeField] private GameObject[] buttonList;
    [Header("タイトルへのボタン画像")][SerializeField] private Sprite[] backTitleButtonSprite;
    [Header("ステージセレクトへのボタン画像")][SerializeField] private Sprite[] stageSelectButtonSprite;
    [Header("左から順に遷移するシーンの名前を格納")][SerializeField] private string[] sceneTransitionName;

    private Image backTitleButtonImage;
    private Image stageSelectButtonImage;

    private CustomInputAction inputActions;
    private int currentButton = 0;

    [Header("フェード処理があるCanvasを格納")][SerializeField] private CS_SceneTransition sceneTransition;
    private CS_BackGroundPlaySE backGroundPlaySE;

    // Start is called before the first frame update
    void Start()
    {
        inputActions = CS_CustomInputActionManager.instance.customInputAction;
        inputActions.GameClear.MoveAxis.started += SelectInput;

        GameObject backTitleButtonObject = GameObject.Find("GameClearBackTitleButton");
        if (backTitleButtonObject == null)
        {
            Debug.LogError("CS_GameClearSelectButton: GameClearBackTitleButtonが見つかりません。");
            return;
        }
        backTitleButtonImage = backTitleButtonObject.GetComponent<Image>();
        if (backTitleButtonImage != null) backTitleButtonImage.sprite = backTitleButtonSprite[currentButton];

        GameObject stageSelectButtonObject = GameObject.Find("StageSelectButton");
        if (stageSelectButtonObject != null)
        {
            stageSelectButtonImage = stageSelectButtonObject.GetComponent<Image>();
            if (stageSelectButtonImage != null) stageSelectButtonImage.sprite = stageSelectButtonSprite[currentButton];
        }

        GameObject seObject = GameObject.Find("SE");
        backGroundPlaySE = seObject != null ? seObject.GetComponent<CS_BackGroundPlaySE>() : null;
    }

    // Update is called once per frame
    void Update()
    {
        //現在選択しているボタンの移動処理
        if (backTitleButtonImage != null) backTitleButtonImage.sprite = backTitleButtonSprite[currentButton];
        if (stageSelectButtonImage != null) stageSelectButtonImage.sprite = stageSelectButtonSprite[currentButton];

        //決定ボタンでシーン遷移
        if (inputActions.GameClear.Decision.triggered)
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
                    currentButton = buttonList.Length - 1;
                }
            }
            else if (inputFloat < 0.0f)
            {
                backGroundPlaySE.PlaySE("Cusor");
                currentButton++;
                if (currentButton >= buttonList.Length)
                {
                    currentButton = 0;
                }
            }
        }
    }
}
