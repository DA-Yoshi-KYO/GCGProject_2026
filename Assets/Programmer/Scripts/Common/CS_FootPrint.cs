/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    足跡の生成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-27 | 初回作成
 */
using UnityEngine;
using System.Collections;

public class CS_FootPrint : MonoBehaviour
{
    [Header("足跡のPrefab")][SerializeField] public GameObject footPrintPrefab;//足跡のPrefab
    [Header("足跡を出すキャラクター")][SerializeField] public Transform character;//足跡を出すキャラクター
    [Header("生成する時間の間隔")][SerializeField] public float createFootPrintDuration;//生成する時間の間隔
    [Header("生成した足跡を削除する時間")][SerializeField] public float destroyTime;//生成した足跡を削除する時間
    [Header("生成位置のX座標の調整")][SerializeField] public float footOffsetX;//生成位置のX座標の調整
    [Header("生成位置のY座標の調整")][SerializeField] public float spawnOffsetY = 0.02f;//生成位置のY座標の調整
 
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
        Vector3 pos = character.position - character.right * footOffsetX;
        SpawnFootprint(pos, character.forward);
    }

    //右足の生成位置
    public void SpawnRightFootprint()
    {
        Vector3 pos = character.position + character.right * footOffsetX;
        SpawnFootprint(pos, character.forward);
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

        //生成
        GameObject footPrintGameObject = pool.GetObject();
        footPrintGameObject.transform.position = spawnPos;
        footPrintGameObject.transform.rotation = rotation;

        //削除
        StartCoroutine(pool.DisableAfterTime(footPrintGameObject, destroyTime));
    }
}
