/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ゲームリザルトの作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-04 | 初回作成
 * 2026-06-15 | デバッグコマンド追加
 */
using UnityEngine;

public class CS_GameResult : MonoBehaviour
{
    [Header("ゲームクリアのPrefab格納")][SerializeField] private GameObject gameClear;
    //[Header("ゲーム失敗のPrefab格納")][SerializeField] private GameObject gameFailure;

    [Header("ゲーム失敗のカットシーンのPrefab格納")][SerializeField] private GameObject gameFailureCutScene;

    private CS_EndManager endManager;

    [Header("デバッグの確認時用")][SerializeField] public bool debugEnd;
    [Header("デバッグコマンド　GameClearへのキー")][SerializeField] public KeyCode gameClearKey;
    [Header("デバッグコマンド　GameFailureへのキー")][SerializeField] public KeyCode gameFailureKey;

    private CS_BackGroundPlaySE backGroundPlaySE;
    private bool startJingle = false;//ジングルが再生されたかどうか

    // Start is called before the first frame update
    void Start()
    {
        endManager = GameObject.Find("EndManager").GetComponent<CS_EndManager>();

        backGroundPlaySE = GameObject.Find("SE").GetComponent<CS_BackGroundPlaySE>();

        gameClear.SetActive(false);
        //gameFailure.SetActive(false);

        //gameFailureCutScene.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (debugEnd)
        {
            if(Input.GetKeyDown(gameClearKey))
            {
                gameClear.SetActive(true);
                if (!startJingle)
                {
                    backGroundPlaySE.PlaySE("WinJingle");
                    startJingle = true;
                }
            }

            if(Input.GetKeyDown(gameFailureKey))
            {
                GameObject gameObject = Instantiate(gameFailureCutScene);
                //gameFailureCutScene.SetActive(true);

                //gameFailure.SetActive(true);
                //if (!startJingle)
                //{
                //    backGroundPlaySE.PlaySE("LoseJingle");
                //    startJingle = true;
                //}
            }
        }
        else
        {
            if (endManager.read_IsEnd)
            {
                if (endManager.read_IsWin)
                {
                    gameClear.SetActive(true);
                    if (!startJingle)
                    {
                        backGroundPlaySE.PlaySE("WinJingle");
                        startJingle = true;
                    }
                }
                else
                {

                    GameObject gameObject = Instantiate(gameFailureCutScene);
                    //gameFailureCutScene.SetActive(true);
                    //gameFailure.SetActive(true);
                    //if (!startJingle)
                    //{
                    //    backGroundPlaySE.PlaySE("LoseJingle");
                    //    startJingle = true;
                    //}
                }
            }
        }
    }
}
