using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*==================================================
 *  ファイル名  : CS_RoomBlockPrefabGenerator.cs
 *  制作者      : 吉本竜
 *  内容        : RoomCreatePointタグが付いたオブジェクトの子としてRoomPrefabをランダム生成する
 *  履歴        : 2026/04/27 RoomCreatePointの子に生成する形へ修正(ヨシモト)
 *==================================================*/

/// <summary>
/// RoomCreatePointタグが付いたオブジェクトを探し、
/// その子オブジェクトとしてRoomPrefabをランダム生成するクラスです。
/// </summary>
public class CS_RoomBlockPrefabGenerator : MonoBehaviour
{
    private const string GENERATED_NAME_PREFIX = "__GeneratedRoom_";
    private const string OLD_GENERATED_ROOT_NAME = "__GeneratedRoomBlocks";

    [Header("生成ポイントタグ")]
    [SerializeField]
    private string str_RoomCreatePointTag = "RoomCreatePoint";

    [Header("ランダム生成に使うルームブロックPrefab")]
    [SerializeField]
    private List<GameObject> list_RoomBlockPrefabs = new List<GameObject>();

    /// <summary>
    /// RoomCreatePointの子にRoomを生成します。
    /// </summary>
    [ContextMenu("ルームブロックを生成")]
    public void GenerateRoomBlocks()
    {
        if (HasGeneratedRoomBlocks())
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] すでに生成済みです。作り直す場合は「ルームブロックを再生成」を使ってください。");
            return;
        }

        GenerateRoomBlocksInternal();
    }

    /// <summary>
    /// 生成済みRoomを削除してから再生成します。
    /// </summary>
    [ContextMenu("ルームブロックを再生成")]
    public void RegenerateRoomBlocks()
    {
        DeleteGeneratedRoomBlocks();
        GenerateRoomBlocksInternal();
    }

    /// <summary>
    /// 生成済みRoomを削除します。
    /// </summary>
    [ContextMenu("生成済みルームブロックを削除")]
    public void DeleteGeneratedRoomBlocks()
    {
        GameObject[] roomCreatePoints = FindRoomCreatePointObjects();

        for (int i = 0 ; i < roomCreatePoints.Length ; i++)
        {
            DeleteGeneratedChildren(roomCreatePoints[i].transform);
        }

        DeleteOldGeneratedRoot();

        Debug.Log("[RoomBlockPrefabGenerator] 生成済みRoomを削除しました。");
    }

    /// <summary>
    /// 外部関数からRoom生成を実行します。
    /// </summary>
    public void CreateRooms()
    {
        GenerateRoomBlocks();
    }

    /// <summary>
    /// 外部関数からRoom再生成を実行します。
    /// </summary>
    public void RecreateRooms()
    {
        RegenerateRoomBlocks();
    }

    /// <summary>
    /// 外部関数からRoom削除を実行します。
    /// </summary>
    public void DeleteRooms()
    {
        DeleteGeneratedRoomBlocks();
    }

    /// <summary>
    /// RoomCreatePointの子にRoomを生成します。
    /// </summary>
    private void GenerateRoomBlocksInternal()
    {
        List<GameObject> validPrefabs = GetValidRoomBlockPrefabs();

        if (validPrefabs.Count <= 0)
        {
            Debug.LogError("[RoomBlockPrefabGenerator] 生成に使えるRoomPrefabがありません。");
            return;
        }

        GameObject[] roomCreatePoints = FindRoomCreatePointObjects();

        if (roomCreatePoints.Length <= 0)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] RoomCreatePointタグが付いたオブジェクトが見つかりません。");
            return;
        }

        DeleteOldGeneratedRoot();

        for (int i = 0 ; i < roomCreatePoints.Length ; i++)
        {
            GameObject pointObject = roomCreatePoints[i];
            GameObject randomPrefab = GetRandomRoomBlockPrefab(validPrefabs);

            if (randomPrefab == null)
            {
                continue;
            }

            Vector3 spawnPosition = GetObjectCenter(pointObject);
            Quaternion spawnRotation = pointObject.transform.rotation;

            GameObject generatedRoom = CreateRoomInstance(
                randomPrefab,
                spawnPosition,
                spawnRotation,
                pointObject.transform
            );

            generatedRoom.name = GENERATED_NAME_PREFIX + randomPrefab.name + "_" + i.ToString("00");
        }

        Dictionary<CS_RoomCreatePoint, GameObject> dic_GeneratedRoomMap =
        BuildGeneratedRoomMapFromScene();

        ConnectGeneratedRooms(dic_GeneratedRoomMap);

        Debug.Log("[RoomBlockPrefabGenerator] RoomCreatePointの子にRoomを生成しました。生成数 : " + roomCreatePoints.Length);
    }

    /// <summary>
    /// RoomCreatePointタグが付いたオブジェクトを取得します。
    /// </summary>
    /// <returns>RoomCreatePointタグが付いたオブジェクト配列。</returns>
    private GameObject[] FindRoomCreatePointObjects()
    {
        if (string.IsNullOrWhiteSpace(str_RoomCreatePointTag))
        {
            Debug.LogError("[RoomBlockPrefabGenerator] 生成ポイントタグが空です。");
            return new GameObject[0];
        }

        try
        {
            GameObject[] roomCreatePoints = GameObject.FindGameObjectsWithTag(str_RoomCreatePointTag);

            System.Array.Sort(
                roomCreatePoints,
                (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal)
            );

            return roomCreatePoints;
        }
        catch (UnityException)
        {
            Debug.LogError("[RoomBlockPrefabGenerator] Tag「" + str_RoomCreatePointTag + "」が存在しません。UnityのTagsに追加してください。");
            return new GameObject[0];
        }
    }

    /// <summary>
    /// nullではないRoomPrefabだけを取得します。
    /// </summary>
    /// <returns>有効なRoomPrefabリスト。</returns>
    private List<GameObject> GetValidRoomBlockPrefabs()
    {
        List<GameObject> validPrefabs = new List<GameObject>();

        for (int i = 0 ; i < list_RoomBlockPrefabs.Count ; i++)
        {
            if (list_RoomBlockPrefabs[i] == null)
            {
                continue;
            }

            validPrefabs.Add(list_RoomBlockPrefabs[i]);
        }

        return validPrefabs;
    }

    /// <summary>
    /// 有効なRoomPrefabリストからランダムで1つ取得します。
    /// </summary>
    /// <param name="validPrefabs">有効なRoomPrefabリスト。</param>
    /// <returns>ランダムに選ばれたRoomPrefab。</returns>
    private GameObject GetRandomRoomBlockPrefab(List<GameObject> validPrefabs)
    {
        if (validPrefabs == null || validPrefabs.Count <= 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, validPrefabs.Count);
        return validPrefabs[randomIndex];
    }

    /// <summary>
    /// RoomPrefabを生成します。
    /// Editor上ではPrefab接続を維持して生成します。
    /// </summary>
    /// <param name="prefab">生成するPrefab。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="rotation">生成回転。</param>
    /// <param name="parent">生成先の親Transform。</param>
    /// <returns>生成されたRoom。</returns>
    private GameObject CreateRoomInstance(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent)
    {
        GameObject instance = null;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;

            if (instance != null)
            {
                Undo.RegisterCreatedObjectUndo(instance, "Generate Room Block");
            }
        }
#endif

        if (instance == null)
        {
            instance = Instantiate(prefab, parent);
        }

        instance.transform.SetPositionAndRotation(position, rotation);

        return instance;
    }

    /// <summary>
    /// 対象オブジェクトの中心位置を取得します。
    /// Collider、Renderer、Transformの順で取得します。
    /// </summary>
    /// <param name="target">中心を取得したい対象オブジェクト。</param>
    /// <returns>対象オブジェクトの中心位置。</returns>
    private Vector3 GetObjectCenter(GameObject target)
    {
        Collider[] colliders = target.GetComponentsInChildren<Collider>();

        if (colliders.Length > 0)
        {
            Bounds bounds = colliders[0].bounds;

            for (int i = 1 ; i < colliders.Length ; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }

            return bounds.center;
        }

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;

            for (int i = 1 ; i < renderers.Length ; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds.center;
        }

        return target.transform.position;
    }

    /// <summary>
    /// 生成済みRoomが存在するか確認します。
    /// </summary>
    /// <returns>生成済みRoomがある場合はtrue。</returns>
    private bool HasGeneratedRoomBlocks()
    {
        GameObject[] roomCreatePoints = FindRoomCreatePointObjects();

        for (int i = 0 ; i < roomCreatePoints.Length ; i++)
        {
            Transform pointTransform = roomCreatePoints[i].transform;

            for (int childIndex = 0 ; childIndex < pointTransform.childCount ; childIndex++)
            {
                if (IsGeneratedRoomName(pointTransform.GetChild(childIndex).name))
                {
                    return true;
                }
            }
        }

        Transform oldRoot = transform.Find(OLD_GENERATED_ROOT_NAME);

        if (oldRoot != null && oldRoot.childCount > 0)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// RoomCreatePointの子にある生成済みRoomだけを削除します。
    /// </summary>
    /// <param name="parent">RoomCreatePointのTransform。</param>
    private void DeleteGeneratedChildren(Transform parent)
    {
        for (int i = parent.childCount - 1 ; i >= 0 ; i--)
        {
            Transform child = parent.GetChild(i);

            if (!IsGeneratedRoomName(child.name))
            {
                continue;
            }

            DestroyObjectSafe(child.gameObject);
        }
    }

    /// <summary>
    /// 以前の設計でRoomManager下に生成されたRootを削除します。
    /// </summary>
    private void DeleteOldGeneratedRoot()
    {
        Transform oldRoot = transform.Find(OLD_GENERATED_ROOT_NAME);

        if (oldRoot == null)
        {
            return;
        }

        DestroyObjectSafe(oldRoot.gameObject);
    }

    /// <summary>
    /// 生成されたRoom名かどうか確認します。
    /// </summary>
    /// <param name="objectName">確認するオブジェクト名。</param>
    /// <returns>生成Room名の場合はtrue。</returns>
    private bool IsGeneratedRoomName(string objectName)
    {
        return objectName.StartsWith(GENERATED_NAME_PREFIX) || objectName.Contains("_Generated_");
    }

    /// <summary>
    /// Play中とEditor中の両方に対応して安全にオブジェクトを削除します。
    /// </summary>
    /// <param name="target">削除対象のGameObject。</param>
    private void DestroyObjectSafe(GameObject target)
    {
        if (target == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.DestroyObjectImmediate(target);
            return;
        }
#endif

        Destroy(target);
    }


    /// <summary>
    /// 生成済みRoomから、RoomCreatePointと生成Roomの対応表を作成します。
    /// </summary>
    /// <returns>RoomCreatePointと生成Roomの対応表。</returns>
    private Dictionary<CS_RoomCreatePoint, GameObject> BuildGeneratedRoomMapFromScene()
    {
        Dictionary<CS_RoomCreatePoint, GameObject> dic_GeneratedRoomMap =
            new Dictionary<CS_RoomCreatePoint, GameObject>();

        GameObject[] roomCreatePointObjects = FindRoomCreatePointObjects();

        for (int i = 0 ; i < roomCreatePointObjects.Length ; i++)
        {
            CS_RoomCreatePoint createPoint =
                roomCreatePointObjects[i].GetComponent<CS_RoomCreatePoint>();

            if (createPoint == null)
            {
                Debug.LogWarning("[RoomBlockPrefabGenerator] CS_RoomCreatePointが付いていません : " + roomCreatePointObjects[i].name);
                continue;
            }

            GameObject generatedRoom = FindGeneratedRoomChild(roomCreatePointObjects[i].transform);

            if (generatedRoom == null)
            {
                continue;
            }

            if (!dic_GeneratedRoomMap.ContainsKey(createPoint))
            {
                dic_GeneratedRoomMap.Add(createPoint, generatedRoom);
            }
        }

        return dic_GeneratedRoomMap;
    }

    /// <summary>
    /// RoomCreatePointの子から生成済みRoomを取得します。
    /// </summary>
    /// <param name="parent">RoomCreatePointのTransform。</param>
    /// <returns>生成済みRoom。</returns>
    private GameObject FindGeneratedRoomChild(Transform parent)
    {
        for (int i = 0 ; i < parent.childCount ; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name.StartsWith("__GeneratedRoom_") || child.name.Contains("_Generated_"))
            {
                return child.gameObject;
            }
        }

        return null;
    }

    /// <summary>
    /// 生成されたRoom同士のRoomMovePointを接続します。
    /// </summary>
    /// <param name="dic_GeneratedRoomMap">RoomCreatePointと生成Roomの対応表。</param>
    private void ConnectGeneratedRooms(Dictionary<CS_RoomCreatePoint, GameObject> dic_GeneratedRoomMap)
    {
        foreach (GameObject generatedRoom in dic_GeneratedRoomMap.Values)
        {
            CS_RoomMovePoint[] movePoints =
                generatedRoom.GetComponentsInChildren<CS_RoomMovePoint>(true);

            for (int i = 0 ; i < movePoints.Length ; i++)
            {
                movePoints[i].ClearTarget();
            }
        }

        foreach (KeyValuePair<CS_RoomCreatePoint, GameObject> pair in dic_GeneratedRoomMap)
        {
            CS_RoomCreatePoint currentCreatePoint = pair.Key;
            GameObject currentRoom = pair.Value;

            ConnectOneDirection(currentCreatePoint, currentRoom, CSE_RoomDoorDirection.Right, dic_GeneratedRoomMap);
            ConnectOneDirection(currentCreatePoint, currentRoom, CSE_RoomDoorDirection.Left, dic_GeneratedRoomMap);
            ConnectOneDirection(currentCreatePoint, currentRoom, CSE_RoomDoorDirection.Front, dic_GeneratedRoomMap);
            ConnectOneDirection(currentCreatePoint, currentRoom, CSE_RoomDoorDirection.Back, dic_GeneratedRoomMap);
        }

        Debug.Log("[RoomBlockPrefabGenerator] RoomMovePoint同士の接続を更新しました。");
    }

    /// <summary>
    /// 1方向分のRoomMovePoint接続を行います。
    /// </summary>
    /// <param name="currentCreatePoint">現在のRoomCreatePoint。</param>
    /// <param name="currentRoom">現在の生成Room。</param>
    /// <param name="currentDirection">現在Roomの出口方向。</param>
    /// <param name="dic_GeneratedRoomMap">RoomCreatePointと生成Roomの対応表。</param>
    private void ConnectOneDirection(
        CS_RoomCreatePoint currentCreatePoint,
        GameObject currentRoom,
        CSE_RoomDoorDirection currentDirection,
        Dictionary<CS_RoomCreatePoint, GameObject> dic_GeneratedRoomMap)
    {
        CS_RoomMovePoint currentMovePoint =
            FindMovePoint(currentRoom, currentDirection);

        if (currentMovePoint == null)
        {
            return;
        }

        if (!currentCreatePoint.TryGetConnection(currentDirection, out CS_RoomMoveConnection connection))
        {
            currentMovePoint.ClearTarget();
            return;
        }

        if (connection.TargetCreatePoint == null)
        {
            currentMovePoint.ClearTarget();
            return;
        }

        if (!dic_GeneratedRoomMap.TryGetValue(connection.TargetCreatePoint, out GameObject targetRoom))
        {
            currentMovePoint.ClearTarget();
            Debug.LogWarning("[RoomBlockPrefabGenerator] 移動先RoomCreatePointに生成Roomがありません : " + connection.TargetCreatePoint.name);
            return;
        }

        CS_RoomMovePoint targetMovePoint =
            FindMovePoint(targetRoom, connection.TargetOutDirection);

        if (targetMovePoint == null)
        {
            currentMovePoint.ClearTarget();
            Debug.LogWarning("[RoomBlockPrefabGenerator] 移動先Roomに指定方向のRoomMovePointがありません : " + connection.TargetOutDirection);
            return;
        }

        currentMovePoint.SetTargetMovePoint(targetMovePoint);
    }

    /// <summary>
    /// 指定Room内から指定方向のRoomMovePointを取得します。
    /// </summary>
    /// <param name="roomObject">生成されたRoom。</param>
    /// <param name="direction">探したい方向。</param>
    /// <returns>指定方向のRoomMovePoint。</returns>
    private CS_RoomMovePoint FindMovePoint(GameObject roomObject, CSE_RoomDoorDirection direction)
    {
        CS_RoomMovePoint[] movePoints =
            roomObject.GetComponentsInChildren<CS_RoomMovePoint>(true);

        for (int i = 0 ; i < movePoints.Length ; i++)
        {
            if (movePoints[i].MoveDirection == direction)
            {
                return movePoints[i];
            }
        }

        return null;
    }
}
