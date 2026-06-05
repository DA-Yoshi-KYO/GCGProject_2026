/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ゲーム失敗のボタンの処理の作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-22 | 初回作成
 * 2026-06-05 | バグの修正
 */
using UnityEngine;
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

    [Header("フェード処理があるCanvasを格納")][SerializeField]private CS_SceneTransition sceneTransition;

    // Start is called before the first frame update
    void Start()
    {
        inputActions = new CustomInputAction();
        inputActions.GameOver.Enable();

        retryButtonImage = GameObject.Find("RetryButton").GetComponent<Image>();
        retryButtonImage.sprite = retryButtonSprite[currentButton];

        backTitleButtonImage = GameObject.Find("GameFailureBackTitleButton").GetComponent<Image>();
        backTitleButtonImage.sprite = backTitleButtonSprite[currentButton];
    }

    // Update is called once per frame
    void Update()
    {
        //現在選択しているボタンの移動処理
        if (inputActions.GameOver.MoveLeft.triggered)
        {
            currentButton--;
            if (currentButton < 0)
            {
                currentButton = backTitleButtonSprite.Length - 1;
            }
        }

        if (inputActions.GameOver.MoveRight.triggered)
        {
            currentButton++;
            if (currentButton > backTitleButtonSprite.Length - 1)
            {
                currentButton = 0;
            }
        }

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
}
