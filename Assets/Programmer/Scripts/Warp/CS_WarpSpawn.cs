/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ワープの生成処理作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-08 | 初回作成
 */
using UnityEngine;

public class CS_Warp : MonoBehaviour
{
    public Transform[] spawnPoints;//候補地点
    public GameObject warpEntrancePrefab;
    public GameObject warpExitPrefab;

    private GameObject currentEntrance;
    private GameObject currentExit;

    public void SpawnWarpEntrance()
    {
        //既存の入口・出口を削除
        if (currentEntrance != null) Destroy(currentEntrance);
        if (currentExit != null) Destroy(currentExit);

        //入口をランダムに選ぶ
        int entranceIndex = Random.Range(0, spawnPoints.Length);

        //出口は入口以外から選ぶ
        int exitIndex;
        do
        {
            exitIndex = Random.Range(0, spawnPoints.Length);
        }
        while (exitIndex == entranceIndex);

        //入口生成
        currentEntrance = Instantiate(
            warpEntrancePrefab,
            spawnPoints[entranceIndex].position,
            spawnPoints[entranceIndex].rotation
        );

        //出口生成
        currentExit = Instantiate(
            warpExitPrefab,
            spawnPoints[exitIndex].position,
            spawnPoints[exitIndex].rotation
        );

        //WarpPoint のリンク設定
        CS_WarpPoint entranceWP = currentEntrance.GetComponent<CS_WarpPoint>();
        CS_WarpPoint exitWP = currentExit.GetComponent<CS_WarpPoint>();

        entranceWP.targetPoint = exitWP;//入口→出口（双方向）
        exitWP.targetPoint = entranceWP;//出口→入口（双方向）
    }

    // Start is called before the first frame update
    void Start()
    {
        SpawnWarpEntrance();
    }
}
