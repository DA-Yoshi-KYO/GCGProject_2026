/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ワープの生成処理作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-08 | 初回作成
 * 2026-06-11 | 隣同士の部屋にワープを作成しないように修正
 */
using System.Collections.Generic;
using UnityEngine;

public class CS_WarpSpawn : MonoBehaviour
{
    private Transform[] spawnPoints;//候補地点
    private List<Transform> spawnPointsList;//候補地点リスト
    [Header("ワープの入り口用のPrefab")][SerializeField]public GameObject warpEntrancePrefab;
    [Header("ワープの出口用のPrefab")][SerializeField]public GameObject warpExitPrefab;
    [Header("ワープの数")][SerializeField] private int warpCount = 1;
    [Header("RoomCreatePointの元データ")][SerializeField] private GameObject go_RoomCreatePointsFormer;

    private GameObject currentEntrance;
    private GameObject currentExit;
    private List<GameObject> lgo_warpWallObjects = new List<GameObject>();
    private List<GameObject[]> warpWallPairList = new List<GameObject[]>();

    // Start is called before the first frame update
    public void WarpPointStart()
    {
        CollectWarpWalls();
        CreateRandomWarpWallPairs();
    }

    private void Update()
    {
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
            List<Transform> remove = new List<Transform>(spawnPointsList);

            //入口生成
            Transform entrancePoint = remove[i];

            GameObject entranceObj = Instantiate(
                warpEntrancePrefab,
                entrancePoint.position,
                entrancePoint.rotation
            );

            //部屋の隣同士の情報取得して候補から外す
            CS_RoomCreatePoint roomCreatePoint = remove[i].gameObject.GetComponentInParent<CS_RoomCreatePoint>();

            Transform deletionCandidate;//削除候補
            List<Transform> deletionCandidateList = new List<Transform>();//削除候補のリスト格納
            Transform parent;

            deletionCandidateList.Add(remove[i]);

            CS_RoomMoveConnection roomMoveConnectionRight;
            if (roomCreatePoint.TryGetConnection(CSE_RoomDoorDirection.Right, out roomMoveConnectionRight))
            {
                parent = roomMoveConnectionRight.TargetCreatePoint.transform;
                for(int j = 0 ; j < remove.Count ; ++j)
                {
                    deletionCandidate = FindParentObject(remove[j], parent.gameObject.name);

                    if (deletionCandidate != null)
                    {
                        deletionCandidateList.Add(remove[j]);
                    }
                }
            }
            CS_RoomMoveConnection roomMoveConnectionLeft;
            if (roomCreatePoint.TryGetConnection(CSE_RoomDoorDirection.Left, out roomMoveConnectionLeft))
            {
                parent = roomMoveConnectionLeft.TargetCreatePoint.transform;
                for (int j = 0 ; j < remove.Count ; ++j)
                {
                    deletionCandidate = FindParentObject(remove[j], parent.gameObject.name);

                    if (deletionCandidate != null)
                    {
                        deletionCandidateList.Add(remove[j]);
                    }
                }
            }
            CS_RoomMoveConnection roomMoveConnectionFront;
            if (roomCreatePoint.TryGetConnection(CSE_RoomDoorDirection.Front, out roomMoveConnectionFront))
            {
                parent = roomMoveConnectionFront.TargetCreatePoint.transform;
                for (int j = 0 ; j < remove.Count ; ++j)
                {
                    deletionCandidate = FindParentObject(remove[j], parent.gameObject.name);

                    if (deletionCandidate != null)
                    {
                        deletionCandidateList.Add(remove[j]);
                    }
                }
            }
            CS_RoomMoveConnection roomMoveConnectionBack;
            if (roomCreatePoint.TryGetConnection(CSE_RoomDoorDirection.Back, out roomMoveConnectionBack))
            {
                parent = roomMoveConnectionBack.TargetCreatePoint.transform;
                for (int j = 0 ; j < remove.Count ; ++j)
                {
                    deletionCandidate = FindParentObject(remove[j], parent.gameObject.name);

                    if (deletionCandidate != null)
                    {
                        deletionCandidateList.Add(remove[j]);
                    }
                }
            }

            //まとめて削除
            foreach (var obj in deletionCandidateList)
            {
                remove.Remove(obj);
            }

            //出口生成
            Transform exitPoint = remove[i];

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

            //複数ワープをつくるために作られた場所は削除
            spawnPointsList.Remove(entrancePoint);
            spawnPointsList.Remove(exitPoint);
        }
    }

    //生成位置候補の処理
    public void ProcessGenerationCandidatePositions()
    {
        GameObject roomParent = GameObject.Find("RoomCreatePoints");

        if (roomParent == null)
        {
            Debug.LogError("[CS_WarpSpawn] RoomCreatePoints が見つかりません。", this);
            spawnPoints = new Transform[0];
            return;
        }

        List<Transform> foundSpawnPoints = new List<Transform>();

        // 初期部屋と宝部屋は探索しない
        for (int i = 1 ; i < roomParent.transform.childCount - 1 ; ++i)
        {
            Transform roomChild = roomParent.transform.GetChild(i);

            Transform warpObj = FindChildObject(roomChild, "WarpCreatePos");

            if (warpObj == null)
            {
                Debug.LogWarning(
                    "[CS_WarpSpawn] WarpCreatePos が見つかりません。Room : " + roomChild.name,
                    roomChild);

                continue;
            }

            foundSpawnPoints.Add(warpObj);
        }

        spawnPoints = foundSpawnPoints.ToArray();
    }

    //再帰処理で探す

    //子オブジェクトを探索する場合
    public Transform FindChildObject(Transform parent, string name)
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

    //親オブジェクトを探索する場合
    public Transform FindParentObject(Transform child, string name)
    {
        Transform current = child.parent;

        while (current.parent != null)
        {
            if (current.name == name)
                return current;

            current = current.parent;
        }

        return null;
    }

    /// <summary>
    /// go_RoomCreatePointsFormer の子階層から、TagがWarpWallのObjectを集めます。
    /// </summary>
    private void CollectWarpWalls()
    {
        lgo_warpWallObjects.Clear();

        if (go_RoomCreatePointsFormer == null)
        {
            Debug.LogError("[CS_WarpSpawn] go_RoomCreatePointsFormer が設定されていません。", this);
            return;
        }

        Transform[] childTransforms = go_RoomCreatePointsFormer.GetComponentsInChildren<Transform>(true);

        foreach (Transform childTransform in childTransforms)
        {
            if (childTransform == go_RoomCreatePointsFormer.transform)
            {
                continue;
            }

            if (childTransform.CompareTag("WarpWall"))
            {
                lgo_warpWallObjects.Add(childTransform.gameObject);
            }
        }

        Debug.Log("[CS_WarpSpawn] WarpWall の数 : " + lgo_warpWallObjects.Count, this);
    }

    /// <summary>
    /// WarpWall同士をランダムにペアにします。
    /// ペアに使われなかったWarpWallは無効化します。
    /// </summary>
    private void CreateRandomWarpWallPairs()
    {
        warpWallPairList.Clear();

        List<GameObject> candidateList = new List<GameObject>(lgo_warpWallObjects);
        candidateList.RemoveAll(obj => obj == null);

        // シャッフル
        for (int i = candidateList.Count - 1 ; i > 0 ; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            GameObject temp = candidateList[i];
            candidateList[i] = candidateList[randomIndex];
            candidateList[randomIndex] = temp;
        }

        int pairCount = Mathf.Min(warpCount, candidateList.Count / 2);

        HashSet<GameObject> usedWarpWalls = new HashSet<GameObject>();

        for (int i = 0 ; i < pairCount ; i++)
        {
            GameObject warpWallA = candidateList[i * 2];
            GameObject warpWallB = candidateList[i * 2 + 1];

            warpWallPairList.Add(new GameObject[] { warpWallA, warpWallB });

            usedWarpWalls.Add(warpWallA);
            usedWarpWalls.Add(warpWallB);

            SetWarpWallFlag(warpWallA, true);
            SetWarpWallFlag(warpWallB, true);

            Debug.Log(
                "[CS_WarpSpawn] WarpWallペア : "
                + warpWallA.name
                + " <-> "
                + warpWallB.name,
                this
            );
        }

        // ペアに使われなかったWarpWallを無効化
        foreach (GameObject warpWall in candidateList)
        {
            if (usedWarpWalls.Contains(warpWall))
            {
                continue;
            }

            SetWarpWallFlag(warpWall, false);

            Debug.Log(
                "[CS_WarpSpawn] 未使用WarpWallを無効化 : "
                + warpWall.name,
                this
            );
        }
    }

    /// <summary>
    /// WarpWallの有効/無効フラグを切り替えます。
    /// </summary>
    private void SetWarpWallFlag(GameObject warpWallObject, bool flag)
    {
        if (warpWallObject == null)
        {
            return;
        }

        CS_WarpWallSwitch warpWallSwitch = warpWallObject.GetComponent<CS_WarpWallSwitch>();

        if (warpWallSwitch == null)
        {
            Debug.LogWarning(
                "[CS_WarpSpawn] CS_WarpWallSwitch が付いていません : "
                + warpWallObject.name,
                warpWallObject
            );

            return;
        }

        warpWallSwitch.SetWarpWallFlag(flag);
    }
}
