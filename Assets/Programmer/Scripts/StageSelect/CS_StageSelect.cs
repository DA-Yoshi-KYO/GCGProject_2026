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
        inputActions = CS_CustomInputActionManager.instance.customInputAction;
        
        GameObject backGroundObject = GameObject.Find("BackGround");
        if (backGroundObject == null)
        {
            Debug.LogError("CS_StageSelect: BackGroundオブジェクトが見つかりません。");
            return;
        }
        image = backGroundObject.GetComponent<Image>();
        if (image == null || imageBackGround == null || stageNumber - 1 < 0 || stageNumber - 1 >= imageBackGround.Length) return;
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
}
