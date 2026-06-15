/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    カットシーンビデオの再生する判定
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-15 | 初回作成
 */
using UnityEngine;

public class CS_CutSceneVideoTrigger : MonoBehaviour
{
    private GameObject cutSceneManager;
    [Header("再生する場面")][SerializeField] private string situation;

    private void Start()
    {
        cutSceneManager = GameObject.Find("CutSceneManager");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //再生するデータの場面の設定
            for (int i = 0 ; i < cutSceneManager.GetComponent<CS_CutSceneVideo>().cutScenedata.Length ; ++i)
            {
                if (situation == cutSceneManager.GetComponent<CS_CutSceneVideo>().cutScenedata[i].situation)
                {
                cutSceneManager.GetComponent<CS_CutSceneVideo>().setNumber = i;
                }
            }

        cutSceneManager.GetComponent<CS_CutSceneVideo>().PlayVideo();
        }
    }
}
