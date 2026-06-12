/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    足跡の生成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-27 | 初回作成
 * 2026-06-05 | バグの修正(斜めの地面を歩くときに地面に沿って出るように)
 */
using UnityEngine;

public class CS_FootPrint : MonoBehaviour
{
    [Header("足跡のPrefab")][SerializeField] public GameObject footPrintPrefab;//足跡のPrefab
    [Header("生成する時間の間隔")][SerializeField] public float createFootPrintDuration;//生成する時間の間隔
    [Header("生成した足跡を削除する時間")][SerializeField] public float destroyTime;//生成した足跡を削除する時間
    [Header("生成位置のX座標の調整")][SerializeField] public float footOffsetX;//生成位置のX座標の調整
    [Header("生成位置のY座標の調整")][SerializeField] public float spawnOffsetY = 1f;//生成位置のY座標の調整
 
    private bool rightFoot = true;//右足かどうか

    private CS_ObjectPool pool;

    private void Start()
    {
        pool = new CS_ObjectPool(footPrintPrefab);        
    }

    //足跡の生成関数
    public void SpawnFootprintAuto()
    {
        //生成する足跡が右か左かどうか
        if (rightFoot)
        {
            SpawnRightFootprint();
        }
        else
        {
            SpawnLeftFootprint();
        }

        rightFoot = !rightFoot;
    }

    //左足の生成位置
    public void SpawnLeftFootprint()
    {
        Vector3 pos = transform.position - transform.right * footOffsetX;
        SpawnFootprint(pos, transform.forward);
    }

    //右足の生成位置
    public void SpawnRightFootprint()
    {
        Vector3 pos = transform.position + transform.right * footOffsetX;
        SpawnFootprint(pos, transform.forward);
    }

    //生成処理
    private void SpawnFootprint(Vector3 pos, Vector3 forward)
    {
        //向きの調整
        float angleOffset;

        if (rightFoot)
        {
            angleOffset = 10.0f;
        }
        else
        {
            angleOffset = -10.0f;
        }

        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Euler(90, angleOffset, 0);

        Vector3 spawnPos = pos + Vector3.up * spawnOffsetY;

        //斜めの地面を歩く際の回転と位置の修正
        Ray ray = new Ray(spawnPos, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            spawnPos = hit.point;
            spawnPos.y += 0.01f;
            rotation = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(forward, hit.normal), //地面に沿ったforward
                hit.normal                                   //地面の法線
            ) * Quaternion.Euler(90, 0.0f, 0);

            //生成
            GameObject footPrintGameObject = pool.GetObject();
            footPrintGameObject.transform.position = spawnPos;
            footPrintGameObject.transform.rotation = rotation;

            //削除
            StartCoroutine(pool.DisableAfterTime(footPrintGameObject, destroyTime));
        }
        else
        {
            Debug.LogWarning("No ground" + "SpawnPos: " + spawnPos);
        }
    }
}
