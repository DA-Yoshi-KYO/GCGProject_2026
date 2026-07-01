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

    [Header("ワープの入り口用のPrefab")]
    [SerializeField] public GameObject warpEntrancePrefab;

    [Header("ワープの出口用のPrefab")]
    [SerializeField] public GameObject warpExitPrefab;

    [Header("ワープの数")]
    [SerializeField] private int warpCount = 1;

    [Header("RoomCreatePointの元データ")]
    [SerializeField] private GameObject go_RoomCreatePointsFormer;

    private GameObject currentEntrance;
    private GameObject currentExit;

    private List<GameObject> lgo_warpWallObjects = new List<GameObject>();
    private List<GameObject[]> warpWallPairList = new List<GameObject[]>();

    private List<GameObject> lgo_currentWarpObjects = new List<GameObject>();

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
        CollectWarpWalls();
        CreateRandomWarpWallPairs();
    }

    /// <summary>
    /// 現在生成されているワープオブジェクトを削除します。
    /// </summary>
    private void DestroyCurrentWarpObjects()
    {
        foreach (GameObject warpObject in lgo_currentWarpObjects)
        {
            if (warpObject != null)
            {
                Destroy(warpObject);
            }
        }

        lgo_currentWarpObjects.Clear();

        if (currentEntrance != null)
        {
            Destroy(currentEntrance);
            currentEntrance = null;
        }

        if (currentExit != null)
        {
            Destroy(currentExit);
            currentExit = null;
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
                {
                    return child;
                }

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
            {
                return current;
            }

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
    /// Room接続確認用の方向一覧です。
    /// </summary>
    private readonly CSE_RoomDoorDirection[] e_RoomDoorDirections =
    {
    CSE_RoomDoorDirection.Right,
    CSE_RoomDoorDirection.Left,
    CSE_RoomDoorDirection.Front,
    CSE_RoomDoorDirection.Back
};

    /// <summary>
    /// WarpWall同士をランダムにペアにします。
    /// 扉移動で繋がっているRoom同士はワープペアにしません。
    /// ペアに使われなかったWarpWallは無効化します。
    /// </summary>
    private void CreateRandomWarpWallPairs()
    {
        DestroyCurrentWarpObjects();

        warpWallPairList.Clear();

        if (warpEntrancePrefab == null)
        {
            Debug.LogError("[CS_WarpSpawn] warpEntrancePrefab が設定されていません。", this);
            return;
        }

        if (warpExitPrefab == null)
        {
            Debug.LogError("[CS_WarpSpawn] warpExitPrefab が設定されていません。", this);
            return;
        }

        List<GameObject> candidateList = new List<GameObject>(lgo_warpWallObjects);
        candidateList.RemoveAll(obj => obj == null);

        ShuffleWarpWallList(candidateList);

        int createdPairCount = 0;

        while (createdPairCount < warpCount && candidateList.Count >= 2)
        {
            GameObject warpWallA = candidateList[0];
            candidateList.RemoveAt(0);

            int pairTargetIndex = FindValidWarpWallPairIndex(warpWallA, candidateList);

            if (pairTargetIndex < 0)
            {
                SetWarpWallFlag(warpWallA, false);

                Debug.Log(
                    "[CS_WarpSpawn] 有効なワープ相手がないため無効化 : "
                    + warpWallA.name,
                    this);

                continue;
            }

            GameObject warpWallB = candidateList[pairTargetIndex];
            candidateList.RemoveAt(pairTargetIndex);

            warpWallPairList.Add(new GameObject[] { warpWallA, warpWallB });

            SetWarpWallFlag(warpWallA, true);
            SetWarpWallFlag(warpWallB, true);

            CreateWarpPointPair(warpWallA, warpWallB);

            createdPairCount++;

            Debug.Log(
                "[CS_WarpSpawn] WarpWallペア : "
                + warpWallA.name
                + " <-> "
                + warpWallB.name,
                this);
        }

        foreach (GameObject warpWall in candidateList)
        {
            SetWarpWallFlag(warpWall, false);

            Debug.Log(
                "[CS_WarpSpawn] 未使用WarpWallを無効化 : "
                + warpWall.name,
                this);
        }
    }

    /// <summary>
    /// WarpWall候補リストをシャッフルします。
    /// </summary>
    private void ShuffleWarpWallList(List<GameObject> candidateList)
    {
        for (int i = candidateList.Count - 1 ; i > 0 ; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            GameObject temp = candidateList[i];
            candidateList[i] = candidateList[randomIndex];
            candidateList[randomIndex] = temp;
        }
    }

    /// <summary>
    /// warpWallA とペアにできるWarpWallの番号を探します。
    /// 扉移動で繋がっているRoom同士は対象外にします。
    /// </summary>
    private int FindValidWarpWallPairIndex(
        GameObject warpWallA,
        List<GameObject> candidateList)
    {
        for (int i = 0 ; i < candidateList.Count ; i++)
        {
            GameObject warpWallB = candidateList[i];

            if (warpWallB == null)
            {
                continue;
            }

            if (IsInvalidWarpWallPair(warpWallA, warpWallB))
            {
                continue;
            }

            return i;
        }

        return -1;
    }

    /// <summary>
    /// ワープペアにしてはいけない組み合わせか判定します。
    /// </summary>
    private bool IsInvalidWarpWallPair(GameObject warpWallA, GameObject warpWallB)
    {
        CS_RoomCreatePoint roomCreatePointA = FindOwnerRoomCreatePoint(warpWallA);
        CS_RoomCreatePoint roomCreatePointB = FindOwnerRoomCreatePoint(warpWallB);

        if (roomCreatePointA == null || roomCreatePointB == null)
        {
            Debug.LogWarning(
                "[CS_WarpSpawn] WarpWallの親RoomCreatePointを取得できません。"
                + " / A : " + warpWallA.name
                + " / B : " + warpWallB.name,
                this);

            // RoomCreatePointが取れない場合は安全側でペア禁止にする
            return true;
        }

        // 同じRoom内のWarpWall同士はペアにしない
        if (roomCreatePointA == roomCreatePointB)
        {
            return true;
        }

        // AからBへ扉移動できるならペア禁止
        if (IsRoomConnectedToTarget(roomCreatePointA, roomCreatePointB))
        {
            return true;
        }

        // BからAへ扉移動できるならペア禁止
        if (IsRoomConnectedToTarget(roomCreatePointB, roomCreatePointA))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// WarpWallが所属しているRoomCreatePointを親階層から探します。
    /// </summary>
    private CS_RoomCreatePoint FindOwnerRoomCreatePoint(GameObject warpWallObject)
    {
        if (warpWallObject == null)
        {
            return null;
        }

        Transform current = warpWallObject.transform;

        while (current != null)
        {
            CS_RoomCreatePoint roomCreatePoint =
                current.GetComponent<CS_RoomCreatePoint>();

            if (roomCreatePoint != null)
            {
                return roomCreatePoint;
            }

            current = current.parent;
        }

        return null;
    }

    /// <summary>
    /// fromRoom から targetRoom に扉移動できるか確認します。
    /// </summary>
    private bool IsRoomConnectedToTarget(
        CS_RoomCreatePoint fromRoom,
        CS_RoomCreatePoint targetRoom)
    {
        if (fromRoom == null || targetRoom == null)
        {
            return false;
        }

        for (int i = 0 ; i < e_RoomDoorDirections.Length ; i++)
        {
            CSE_RoomDoorDirection direction = e_RoomDoorDirections[i];

            if (!fromRoom.TryGetConnection(
                    direction,
                    out CS_RoomMoveConnection connection))
            {
                continue;
            }

            if (connection == null)
            {
                continue;
            }

            if (connection.TargetCreatePoint == targetRoom)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// WarpWallの位置情報を使って、入口と出口のワープオブジェクトを生成します。
    /// </summary>
    private void CreateWarpPointPair(GameObject warpWallA, GameObject warpWallB)
    {
        CS_WarpWallSwitch warpWallSwitchA = warpWallA.GetComponent<CS_WarpWallSwitch>();
        CS_WarpWallSwitch warpWallSwitchB = warpWallB.GetComponent<CS_WarpWallSwitch>();

        if (warpWallSwitchA == null)
        {
            Debug.LogWarning(
                "[CS_WarpSpawn] CS_WarpWallSwitch が付いていません : "
                + warpWallA.name,
                warpWallA
            );

            return;
        }

        if (warpWallSwitchB == null)
        {
            Debug.LogWarning(
                "[CS_WarpSpawn] CS_WarpWallSwitch が付いていません : "
                + warpWallB.name,
                warpWallB
            );

            return;
        }

        Transform warpPointA = warpWallSwitchA.GetWarpPointTransform();
        Transform warpPointB = warpWallSwitchB.GetWarpPointTransform();

        Transform warpExitPositionA = FindChildObject(warpWallA.transform, "WarpExitPosition");
        Transform warpExitPositionB = FindChildObject(warpWallB.transform, "WarpExitPosition");

        if (warpExitPositionA == null)
        {
            Debug.LogWarning(
                "[CS_WarpSpawn] WarpExitPosition が見つかりません : "
                + warpWallA.name,
                warpWallA
            );

            warpExitPositionA = warpPointA;
        }

        if (warpExitPositionB == null)
        {
            Debug.LogWarning(
                "[CS_WarpSpawn] WarpExitPosition が見つかりません : "
                + warpWallB.name,
                warpWallB
            );

            warpExitPositionB = warpPointB;
        }

        GameObject entranceObj = Instantiate(
            warpEntrancePrefab,
            warpPointA.position,
            warpPointA.rotation
        );

        GameObject exitObj = Instantiate(
            warpExitPrefab,
            warpPointB.position,
            warpPointB.rotation
        );

        lgo_currentWarpObjects.Add(entranceObj);
        lgo_currentWarpObjects.Add(exitObj);

        currentEntrance = entranceObj;
        currentExit = exitObj;

        //WarpPoint のリンク設定（双方向）
        CS_WarpPoint entranceWP = entranceObj.GetComponent<CS_WarpPoint>();
        CS_WarpPoint exitWP = exitObj.GetComponent<CS_WarpPoint>();

        if (entranceWP == null)
        {
            Debug.LogWarning(
                "[CS_WarpSpawn] 入口Prefabに CS_WarpPoint が付いていません : "
                + entranceObj.name,
                entranceObj
            );

            return;
        }

        if (exitWP == null)
        {
            Debug.LogWarning(
                "[CS_WarpSpawn] 出口Prefabに CS_WarpPoint が付いていません : "
                + exitObj.name,
                exitObj
            );

            return;
        }

        entranceWP.targetPoint = exitWP;
        exitWP.targetPoint = entranceWP;

        entranceWP.warpExitPosition = warpExitPositionA;
        exitWP.warpExitPosition = warpExitPositionB;
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
