/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    選択バーの処理の作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-15 | 初回作成
 */
using UnityEngine;

public class CS_SelectBarMove : MonoBehaviour
{
    [Header("ボタンのリスト（上から順に格納してください）")][SerializeField] private GameObject[] buttonList;
    [Header("選択バー")][SerializeField] private GameObject selectBar;

    private CustomInputAction inputActions;
    private int currentButton = 0;

    // Start is called before the first frame update
    void Start()
    {
        inputActions = new CustomInputAction();
        inputActions.SelectBar.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        //現在選択しているボタンの移動処理
        if(inputActions.SelectBar.MoveUp.triggered)
        {
            currentButton--;
            if(currentButton < 0)
            {
                currentButton = buttonList.Length - 1;
            }
        }

        if(inputActions.SelectBar.MoveDown.triggered)
        {
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
            string sceneName = "";
            switch(currentButton)
            {
                case 0:
                    sceneName = "StageSelectScene";
                    break;
                case 1:
                    break;
                case 2:
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
            }
            GetComponent<CS_SceneTransition>().StartSceneTransition(sceneName);
        }
    }

    private void OnDestroy()
    {
        inputActions.SelectBar.Disable();
    }

}
