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
    [Header("ゲーム失敗のPrefab格納")][SerializeField] private GameObject gameFailure;

    [Header("ゲームクリアのカットシーンのPrefab格納")][SerializeField] private GameObject gameClearCutScene;
    [Header("ゲーム失敗のカットシーンのPrefab格納")][SerializeField] private GameObject gameFailureCutScene;
    [Header("ゲームクリアのカットシーンの座標")][SerializeField] private Transform gameClearCutScenePos;
    [Header("ゲーム失敗のカットシーンの座標")][SerializeField] private Transform gameFailureCutScenePos;

    private CS_EndManager endManager;

    [Header("デバッグの確認時用")][SerializeField] private bool debugEnd;
    [Header("失敗演出の設定（もし前の物の場合True）")][SerializeField]private bool isOldFailure;
    [Header("デバッグコマンド　GameClearへのキー")][SerializeField] public KeyCode gameClearKey;
    [Header("デバッグコマンド　GameFailureへのキー")][SerializeField] public KeyCode gameFailureKey;

    private CS_BackGroundPlayBGM backGroundPlayBGM;
    private bool startJingle = false;//ジングルが再生されたかどうか

    // Start is called before the first frame update
    void Start()
    {
        GameObject endManagerObject = GameObject.Find("EndManager");
        endManager = endManagerObject != null ? endManagerObject.GetComponent<CS_EndManager>() : null;

        GameObject seObject = GameObject.Find("BGM");
        backGroundPlayBGM = seObject != null ? seObject.GetComponent<CS_BackGroundPlayBGM>() : null;

        gameClear.SetActive(false);

        if (isOldFailure)
        {
            gameFailure.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (debugEnd)
        {
            if (Input.GetKeyDown(gameClearKey))
            {
                if (isOldFailure)
                {
                    gameClear.SetActive(true);
                    if (!startJingle)
                    {
                        backGroundPlayBGM.InGameCutScene(true);
                        startJingle = true;
                    }
                }
                else
                {
                    if (!startJingle)
                    {
                        GameObject gameObject = Instantiate(gameClearCutScene, gameClearCutScenePos.position, gameClearCutScenePos.rotation);
                        backGroundPlayBGM.InGameCutScene(true);
                        startJingle = true;
                    }
                }
            }
        }

        if (Input.GetKeyDown(gameFailureKey))
        {
            if (isOldFailure)
            {
                gameFailure.SetActive(true);
                if (!startJingle)
                {
                    backGroundPlayBGM.InGameCutScene(false);
                    startJingle = true;
                }
            }
            else
            {
                if (!startJingle)
                {
                    GameObject gameObject = Instantiate(gameFailureCutScene, gameFailureCutScenePos.position, gameFailureCutScenePos.rotation);
                    backGroundPlayBGM.InGameCutScene(false);
                    startJingle = true;
                }
            }
        }
        else
        {
            if (endManager.read_IsEnd)
            {
                if (endManager.read_IsWin)
                {
                    if (isOldFailure)
                    {
                        gameClear.SetActive(true);
                        if (!startJingle)
                        {
                            backGroundPlayBGM.InGameCutScene(true);
                            startJingle = true;
                        }
                    }
                    else
                    {
                        if (!startJingle)
                        {
                            GameObject gameObject = Instantiate(gameClearCutScene, gameClearCutScenePos.position, gameClearCutScenePos.rotation);
                            backGroundPlayBGM.InGameCutScene(true);
                            startJingle = true;
                        }
                    }
                }
                else
                {
                    if (isOldFailure)
                    {
                        gameFailure.SetActive(true);
                        if (!startJingle)
                        {
                            backGroundPlayBGM.InGameCutScene(false);
                            startJingle = true;
                        }
                    }
                    else
                    {
                        if (!startJingle)
                        {
                            GameObject gameObject = Instantiate(gameFailureCutScene, gameFailureCutScenePos.position, gameFailureCutScenePos.rotation);
                            backGroundPlayBGM.InGameCutScene(false);
                            startJingle = true;
                        }
                    }
                }
            }
        }
    }
}
