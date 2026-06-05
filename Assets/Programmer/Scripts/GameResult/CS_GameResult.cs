/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ゲームリザルトの作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-04 | 初回作成
 */
using UnityEngine;

public class CS_GameResult : MonoBehaviour
{
    [Header("ゲームクリアのPrefab格納")][SerializeField] private GameObject gameClear;
    [Header("ゲーム失敗のPrefab格納")][SerializeField] private GameObject gameFailure;

    private CS_EndManager endManager;
    bool end = false;

    [Header("デバッグの確認時用")][SerializeField] public bool debugEnd;

    // Start is called before the first frame update
    void Start()
    {


        endManager = GameObject.Find("EndManager").GetComponent<CS_EndManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (debugEnd)
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                end = true;
            }

            if (end)
            {
                gameFailure.SetActive(true);
            }
            else
            {
                gameClear.SetActive(false);
                gameFailure.SetActive(false);
            }
        }
        else
        {
            if (endManager.read_IsEnd)
            {
                if (endManager.read_IsWin)
                {
                    gameClear.SetActive(true);
                }
                else
                {
                    gameFailure.SetActive(true);
                }
            }
            else
            {
                gameClear.SetActive(false);
                gameFailure.SetActive(false);
            }
        }
    }
}
