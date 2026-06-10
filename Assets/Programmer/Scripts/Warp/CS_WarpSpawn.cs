/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ワープの生成処理作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-08 | 初回作成
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class CS_WarpSpawn : MonoBehaviour
{
    private Transform[] spawnPoints;//候補地点
    List<Transform> spawnPointsList;
    [Header("ワープの入り口用のPrefab")][SerializeField]public GameObject warpEntrancePrefab;
    [Header("ワープの出口用のPrefab")][SerializeField]public GameObject warpExitPrefab;
    [Header("ワープの数")][SerializeField] private int warpCount = 1;

    private GameObject currentEntrance;
    private GameObject currentExit;

    [SerializeField] public bool debug;

    // Start is called before the first frame update
    void Start()
    {
    }

    private void Update()
    {
        //部屋が作られていないと探せないためUpdate内で実装する

        if (debug)
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                SpawnWarp();
            }
        }
    }

    public void SpawnWarp()
    {
        //既存の入口・出口を削除
        if (currentEntrance != null) Destroy(currentEntrance);
        if (currentExit != null) Destroy(currentExit);

        //生成位置候補の処理
        ProcessGenerationCandidatePositions();

        spawnPointsList = new List<Transform>(spawnPoints);

        //シャッフル
        for (int i = spawnPointsList.Count - 1 ; i > 0 ; i--)
        {
            int r = Random.Range(0, i + 1);
            var prev = spawnPointsList[i];
            spawnPointsList[i] = spawnPointsList[r];
            spawnPointsList[r] = prev;
        }

        //作れる最大ペア数
        int maxWarp = Mathf.Min(warpCount, spawnPointsList.Count / 2);

        for (int i = 0 ; i < maxWarp ; i++)
        {
            //2つずつ取り出す
            Transform entrancePoint = spawnPointsList[i * 2];
            Transform exitPoint = spawnPointsList[i * 2 + 1];

            //入口生成
            GameObject entranceObj = Instantiate(
                warpEntrancePrefab,
                entrancePoint.position,
                entrancePoint.rotation
            );

            //出口生成
            GameObject exitObj = Instantiate(
                warpExitPrefab,
                exitPoint.position,
                exitPoint.rotation
            );

            //WarpPoint のリンク設定（双方向）
            CS_WarpPoint entranceWP = entranceObj.GetComponent<CS_WarpPoint>();
            CS_WarpPoint exitWP = exitObj.GetComponent<CS_WarpPoint>();

            entranceWP.targetPoint = exitWP;
            exitWP.targetPoint = entranceWP;
        }
    }

    //生成位置候補の処理
    public void ProcessGenerationCandidatePositions()
    {
        GameObject roomParent = GameObject.Find("RoomCreatePoints");
        Transform roomChild;
        if (roomParent == null)
            return;

        //初期部屋と宝部屋は探索しない
        int count = roomParent.transform.childCount - 2;
        spawnPoints = new Transform[count];

        for (int i = 1; i < roomParent.transform.childCount - 1 ;++i)
        {
            roomChild = roomParent.transform.GetChild(i);

            Transform warpObj = FindWarpObject(roomChild, "WarpCreatePos");

            if (warpObj == null)
                return;

            spawnPoints[i - 1] = warpObj;
        }
    }

    //再帰処理で探す
    public Transform FindWarpObject(Transform parent, string name)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(parent);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();

            foreach (Transform child in current)
            {
                if (child.name == name)
                    return child;

                queue.Enqueue(child);
            }
        }

        return null;
    }
}
