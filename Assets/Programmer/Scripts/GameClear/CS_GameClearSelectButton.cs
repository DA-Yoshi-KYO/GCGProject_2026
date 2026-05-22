/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ゲームクリアのボタンの処理の作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-22 | 初回作成
 */
using UnityEngine;
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

    // Start is called before the first frame update
    void Start()
    {
        inputActions = new CustomInputAction();
        inputActions.GameClear.Enable();

        backTitleButtonImage = GameObject.Find("BackTitleButton").GetComponent<Image>();
        backTitleButtonImage.sprite = backTitleButtonSprite[currentButton];

        stageSelectButtonImage = GameObject.Find("StageSelectButton").GetComponent<Image>();
        stageSelectButtonImage.sprite = stageSelectButtonSprite[currentButton];
    }

    // Update is called once per frame
    void Update()
    {
        //現在選択しているボタンの移動処理
        if (inputActions.GameClear.MoveLeft.triggered)
        {
            currentButton--;
            if (currentButton < 0)
            {
                currentButton = backTitleButtonSprite.Length - 1;
            }
            backTitleButtonImage.sprite = backTitleButtonSprite[currentButton];
            stageSelectButtonImage.sprite = stageSelectButtonSprite[currentButton];
        }

        if (inputActions.GameClear.MoveRight.triggered)
        {
            currentButton++;
            if (currentButton > backTitleButtonSprite.Length - 1)
            {
                currentButton = 0;
            }
            backTitleButtonImage.sprite = backTitleButtonSprite[currentButton];
            stageSelectButtonImage.sprite = stageSelectButtonSprite[currentButton];
        }

        //決定ボタンでシーン遷移
        if (inputActions.GameClear.Decision.triggered)
        {
            string sceneName = sceneTransitionName[currentButton];
            GetComponent<CS_SceneTransition>().StartSceneTransition(sceneName);
        }
    }

    private void OnDestroy()
    {
        inputActions.GameClear.Disable();
    }
}
