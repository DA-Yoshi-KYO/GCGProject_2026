/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ステージセレクトシーンの処理の作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-19 | 初回作成
 */
using UnityEngine;
using UnityEngine.UI;

public class CS_StageSelect : MonoBehaviour
{
    [Header("ステージ1から順に画像を格納")][SerializeField] private Sprite[] imageBackGround;
    [Header("ステージ1から順に遷移するシーンの名前を格納")][SerializeField] private string[] sceneTransitionName;

    private Image image;

    private CustomInputAction inputActions;
    private int stageNumber = 1;

    // Start is called before the first frame update
    void Start()
    {
        inputActions = new CustomInputAction();
        inputActions.StageSelect.Enable();

        image = GameObject.Find("BackGround").GetComponent<Image>();
        image.sprite = imageBackGround[stageNumber - 1];
    }

    // Update is called once per frame
    void Update()
    {
        //現在選択しているボタンの移動処理
        if (inputActions.StageSelect.MoveLeft.triggered)
        {
            stageNumber--;
            if (stageNumber < 1)
            {
                stageNumber = imageBackGround.Length;
            }
            image.sprite = imageBackGround[stageNumber - 1];
        }

        if (inputActions.StageSelect.MoveRight.triggered)
        {
            stageNumber++;
            if (stageNumber > imageBackGround.Length)
            {
                stageNumber = 1;
            }
            image.sprite = imageBackGround[stageNumber - 1];
        }

        //決定ボタンでシーン遷移
        if (inputActions.StageSelect.Decision.triggered)
        {
            string sceneName = sceneTransitionName[stageNumber - 1];
            GetComponent<CS_SceneTransition>().StartSceneTransition(sceneName);
        }
    }

    private void OnDestroy()
    {
        inputActions.StageSelect.Disable();
    }
}
