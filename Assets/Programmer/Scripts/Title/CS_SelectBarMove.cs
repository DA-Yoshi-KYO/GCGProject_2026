/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    選択バーの処理の作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-15 | 初回作成
 * 2026-05-17 | SE再生処理追加
 */
using UnityEngine;

public class CS_SelectBarMove : MonoBehaviour
{
    [Header("ボタンのリスト（上から順に格納してください）")][SerializeField] private GameObject[] buttonList;
    [Header("選択バー")][SerializeField] private GameObject selectBar;

    private CustomInputAction inputActions;
    private int currentButton = 0;

    private CS_BackGroundPlaySE backGroundPlaySE;

    // Start is called before the first frame update
    void Start()
    {
        inputActions = new CustomInputAction();
        inputActions.SelectBar.Enable();

        backGroundPlaySE = GameObject.Find("SE").GetComponent<CS_BackGroundPlaySE>();
    }

    // Update is called once per frame
    void Update()
    {
        //現在選択しているボタンの移動処理
        if(inputActions.SelectBar.MoveUp.triggered)
        {
            backGroundPlaySE.PlaySE("Cusor");
            currentButton--;
            if(currentButton < 0)
            {
                currentButton = buttonList.Length - 1;
            }
        }

        if(inputActions.SelectBar.MoveDown.triggered)
        {
            backGroundPlaySE.PlaySE("Cusor");
            currentButton++;
            if(currentButton >= buttonList.Length)
            {
                currentButton = 0;
            }
        }

        //座標移動
        Vector3 selectBarPos = selectBar.GetComponent<RectTransform>().anchoredPosition;
        
        selectBarPos.y = buttonList[currentButton].GetComponent<RectTransform>().anchoredPosition.y;

        selectBar.GetComponent<RectTransform>().anchoredPosition = selectBarPos;

        //決定ボタンでシーン遷移
        if(inputActions.SelectBar.Decision.triggered)
        {
            backGroundPlaySE.PlaySE("Decision");
            string sceneName = "";
            switch(currentButton)
            {
                case 0:
                    sceneName = "StageSelectScene";
                    GetComponent<CS_SceneTransition>().StartSceneTransition(sceneName);
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

    private void OnDestroy()
    {
        inputActions.SelectBar.Disable();
    }

}
