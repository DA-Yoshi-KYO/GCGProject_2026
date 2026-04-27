using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*==================================================
 *  ファイル名  : CS_RoomBlockGenerator.cs
 *  制作者      : 吉本竜
 *  内容        : RoomCreatePointにRoomをランダム生成し、RoomMovePoint同士を動的接続する
 *  履歴        : 2026/04/26 新規作成(ヨシモト)
 *                2026/04/26 再生成処理と削除処理を追加
 *                2026/04/26 複数のRoomBlockDataからランダム生成する処理を追加
 *                2026/04/27 動的RoomMovePoint接続処理を追加(ヨシモト)
 *==================================================*/

/// <summary>
/// RoomCreatePointにランダムなRoomPrefabを生成し、
/// 生成後にRoomMovePoint同士を動的に接続するクラスです。
/// </summary>
public class CS_RoomBlockGenerator : MonoBehaviour
{
    private const string GENERATED_ROOT_NAME = "__GeneratedRoomBlocks";

    [Header("ランダム生成に使うRoomPrefab")]
    [SerializeField]
    private List<GameObject> list_RoomPrefabs = new List<GameObject>();

    /// <summary>
    /// Roomを生成します。
    /// </summary>
    [ContextMenu("ルームブロックを生成")]
    public void GenerateRoomBlocks()
    {
        GenerateRoomBlocksInternal(false);
    }

    /// <summary>
    /// 生成済みRoomを削除して再生成します。
    /// </summary>
    [ContextMenu("ルームブロックを再生成")]
    public void RegenerateRoomBlocks()
    {
        ClearGeneratedRoomBlocks();
        GenerateRoomBlocksInternal(true);
    }

    /// <summary>
    /// 生成済みRoomを削除します。
    /// </summary>
    [ContextMenu("生成済みルームブロックを削除")]
    public void DeleteGeneratedRoomBlocks()
    {
        Transform tf_GeneratedRoot = FindGeneratedRoot();

        if (tf_GeneratedRoot == null)
        {
            return;
        }

        DestroyObjectSafe(tf_GeneratedRoot.gameObject);
    }

    /// <summary>
    /// 生成済みRoomの接続だけを更新します。
    /// </summary>
    [ContextMenu("生成済みルーム接続を更新")]
    public void RebuildGeneratedRoomLinks()
    {
        Dictionary<CS_RoomCreatePoint, CS_RoomInstance> dic_RoomMap = BuildGeneratedRoomMapFromScene();

        if (dic_RoomMap.Count <= 0)
        {
            Debug.LogWarning("[RoomBlockGenerator] 接続更新できる生成済みRoomがありません。");
            return;
        }

        ConnectGeneratedRooms(dic_RoomMap);
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
    /// Roomを生成し、生成後に接続します。
    /// </summary>
    /// <param name="isForceGenerate">強制生成するかどうか。</param>
    private void GenerateRoomBlocksInternal(bool isForceGenerate)
    {
        if (!isForceGenerate && HasGeneratedRoomBlocks())
        {
            Debug.LogWarning("[RoomBlockGenerator] すでに生成済みです。再生成を使ってください。");
            return;
        }

        List<GameObject> list_ValidPrefabs = GetValidRoomPrefabs();

        if (list_ValidPrefabs.Count <= 0)
        {
            Debug.LogError("[RoomBlockGenerator] 生成に使えるRoomPrefabがありません。");
            return;
        }

        CS_RoomCreatePoint[] createPoints = FindRoomCreatePoints();

        if (createPoints.Length <= 0)
        {
            Debug.LogWarning("[RoomBlockGenerator] CS_RoomCreatePointが見つかりません。");
            return;
        }

        Transform tf_GeneratedRoot = GetOrCreateGeneratedRoot();

        Dictionary<CS_RoomCreatePoint, CS_RoomInstance> dic_RoomMap =
            new Dictionary<CS_RoomCreatePoint, CS_RoomInstance>();

        for (int i = 0 ; i < createPoints.Length ; i++)
        {
            CS_RoomCreatePoint createPoint = createPoints[i];
            GameObject randomPrefab = GetRandomRoomPrefab(list_ValidPrefabs);

            if (randomPrefab == null)
            {
                continue;
            }

            Vector3 spawnPosition = GetObjectCenter(createPoint.gameObject);
            Quaternion spawnRotation = createPoint.transform.rotation;

            GameObject generatedRoom = CreateRoomInstance(
                randomPrefab,
                spawnPosition,
                spawnRotation,
                tf_GeneratedRoot
            );

            generatedRoom.name = randomPrefab.name + "_Generated_" + i.ToString("00");

            CS_RoomInstance roomInstance = generatedRoom.GetComponent<CS_RoomInstance>();

            if (roomInstance == null)
            {
                roomInstance = generatedRoom.AddComponent<CS_RoomInstance>();
            }

            roomInstance.InitializeMovePoints();

            CS_RoomGeneratedInfo generatedInfo = generatedRoom.GetComponent<CS_RoomGeneratedInfo>();

            if (generatedInfo == null)
            {
                generatedInfo = generatedRoom.AddComponent<CS_RoomGeneratedInfo>();
            }

            generatedInfo.SetSourceCreatePoint(createPoint);

            dic_RoomMap.Add(createPoint, roomInstance);
        }

        ConnectGeneratedRooms(dic_RoomMap);

        Debug.Log("[RoomBlockGenerator] Room生成と接続が完了しました。生成数 : " + dic_RoomMap.Count);
    }

    /// <summary>
    /// 生成済みRoom同士を接続します。
    /// </summary>
    /// <param name="dic_RoomMap">RoomCreatePointと生成Roomの対応表。</param>
    private void ConnectGeneratedRooms(Dictionary<CS_RoomCreatePoint, CS_RoomInstance> dic_RoomMap)
    {
        foreach (CS_RoomInstance roomInstance in dic_RoomMap.Values)
        {
            roomInstance.ClearAllMovePointTargets();
        }

        foreach (KeyValuePair<CS_RoomCreatePoint, CS_RoomInstance> pair in dic_RoomMap)
        {
            CS_RoomCreatePoint currentCreatePoint = pair.Key;
            CS_RoomInstance currentRoom = pair.Value;

            ConnectOneDirection(currentCreatePoint, currentRoom, CSE_RoomDoorDirection.Right, dic_RoomMap);
            ConnectOneDirection(currentCreatePoint, currentRoom, CSE_RoomDoorDirection.Left, dic_RoomMap);
            ConnectOneDirection(currentCreatePoint, currentRoom, CSE_RoomDoorDirection.Front, dic_RoomMap);
            ConnectOneDirection(currentCreatePoint, currentRoom, CSE_RoomDoorDirection.Back, dic_RoomMap);
        }
    }

    /// <summary>
    /// 1方向分のRoomMovePoint接続を行います。
    /// </summary>
    /// <param name="currentCreatePoint">現在のRoomCreatePoint。</param>
    /// <param name="currentRoom">現在の生成Room。</param>
    /// <param name="e_CurrentDirection">現在Roomの出口方向。</param>
    /// <param name="dic_RoomMap">RoomCreatePointと生成Roomの対応表。</param>
    private void ConnectOneDirection(
        CS_RoomCreatePoint currentCreatePoint,
        CS_RoomInstance currentRoom,
        CSE_RoomDoorDirection e_CurrentDirection,
        Dictionary<CS_RoomCreatePoint, CS_RoomInstance> dic_RoomMap)
    {
        if (!currentRoom.TryGetMovePoint(e_CurrentDirection, out CS_RoomMovePoint currentMovePoint))
        {
            return;
        }

        if (!currentCreatePoint.TryGetConnection(e_CurrentDirection, out CS_RoomMoveConnection connection))
        {
            currentMovePoint.ClearTarget();
            return;
        }

        if (!dic_RoomMap.TryGetValue(connection.TargetCreatePoint, out CS_RoomInstance targetRoom))
        {
            currentMovePoint.ClearTarget();
            Debug.LogWarning("[RoomBlockGenerator] 移動先RoomCreatePointに生成Roomがありません : " + connection.TargetCreatePoint.name);
            return;
        }

        if (!targetRoom.TryGetMovePoint(connection.TargetOutDirection, out CS_RoomMovePoint targetMovePoint))
        {
            currentMovePoint.ClearTarget();
            Debug.LogWarning("[RoomBlockGenerator] 移動先Roomに指定方向のRoomMovePointがありません : " + connection.TargetOutDirection);
            return;
        }

        currentMovePoint.SetTargetMovePoint(targetMovePoint);
    }

    /// <summary>
    /// 生成済みRoomからRoomCreatePointとの対応表を再構築します。
    /// </summary>
    /// <returns>RoomCreatePointと生成Roomの対応表。</returns>
    private Dictionary<CS_RoomCreatePoint, CS_RoomInstance> BuildGeneratedRoomMapFromScene()
    {
        Dictionary<CS_RoomCreatePoint, CS_RoomInstance> dic_RoomMap =
            new Dictionary<CS_RoomCreatePoint, CS_RoomInstance>();

        Transform tf_GeneratedRoot = FindGeneratedRoot();

        if (tf_GeneratedRoot == null)
        {
            return dic_RoomMap;
        }

        CS_RoomGeneratedInfo[] generatedInfos =
            tf_GeneratedRoot.GetComponentsInChildren<CS_RoomGeneratedInfo>(true);

        for (int i = 0 ; i < generatedInfos.Length ; i++)
        {
            CS_RoomGeneratedInfo info = generatedInfos[i];

            if (info.SourceCreatePoint == null)
            {
                continue;
            }

            CS_RoomInstance roomInstance = info.GetComponent<CS_RoomInstance>();

            if (roomInstance == null)
            {
                continue;
            }

            roomInstance.InitializeMovePoints();

            if (!dic_RoomMap.ContainsKey(info.SourceCreatePoint))
            {
                dic_RoomMap.Add(info.SourceCreatePoint, roomInstance);
            }
        }

        return dic_RoomMap;
    }

    /// <summary>
    /// シーン上のRoomCreatePointを取得します。
    /// </summary>
    /// <returns>RoomCreatePoint配列。</returns>
    private CS_RoomCreatePoint[] FindRoomCreatePoints()
    {
        CS_RoomCreatePoint[] createPoints = FindObjectsOfType<CS_RoomCreatePoint>(true);

        System.Array.Sort(
            createPoints,
            (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal)
        );

        return createPoints;
    }

    /// <summary>
    /// 有効なRoomPrefabだけを取得します。
    /// </summary>
    /// <returns>有効なRoomPrefabリスト。</returns>
    private List<GameObject> GetValidRoomPrefabs()
    {
        List<GameObject> validPrefabs = new List<GameObject>();

        for (int i = 0 ; i < list_RoomPrefabs.Count ; i++)
        {
            if (list_RoomPrefabs[i] == null)
            {
                continue;
            }

            validPrefabs.Add(list_RoomPrefabs[i]);
        }

        return validPrefabs;
    }

    /// <summary>
    /// RoomPrefabをランダムに取得します。
    /// </summary>
    /// <param name="validPrefabs">有効なPrefabリスト。</param>
    /// <returns>ランダムに選ばれたPrefab。</returns>
    private GameObject GetRandomRoomPrefab(List<GameObject> validPrefabs)
    {
        if (validPrefabs == null || validPrefabs.Count <= 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, validPrefabs.Count);
        return validPrefabs[randomIndex];
    }

    /// <summary>
    /// 生成済みRootを探します。
    /// </summary>
    /// <returns>生成済みRoot。</returns>
    private Transform FindGeneratedRoot()
    {
        return transform.Find(GENERATED_ROOT_NAME);
    }

    /// <summary>
    /// 生成済みRootを取得または作成します。
    /// </summary>
    /// <returns>生成済みRoot。</returns>
    private Transform GetOrCreateGeneratedRoot()
    {
        Transform generatedRoot = FindGeneratedRoot();

        if (generatedRoot != null)
        {
            return generatedRoot;
        }

        GameObject rootObject = new GameObject(GENERATED_ROOT_NAME);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.RegisterCreatedObjectUndo(rootObject, "Create Generated Room Root");
        }
#endif

        rootObject.transform.SetParent(transform);
        rootObject.transform.localPosition = Vector3.zero;
        rootObject.transform.localRotation = Quaternion.identity;
        rootObject.transform.localScale = Vector3.one;

        return rootObject.transform;
    }

    /// <summary>
    /// 生成済みRoomがあるか確認します。
    /// </summary>
    /// <returns>生成済みRoomがある場合はtrue。</returns>
    private bool HasGeneratedRoomBlocks()
    {
        Transform generatedRoot = FindGeneratedRoot();

        if (generatedRoot == null)
        {
            return false;
        }

        return generatedRoot.childCount > 0;
    }

    /// <summary>
    /// 生成済みRoomを削除します。
    /// Root自体は残します。
    /// </summary>
    private void ClearGeneratedRoomBlocks()
    {
        Transform generatedRoot = FindGeneratedRoot();

        if (generatedRoot == null)
        {
            return;
        }

        for (int i = generatedRoot.childCount - 1 ; i >= 0 ; i--)
        {
            DestroyObjectSafe(generatedRoot.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// RoomPrefabを生成します。
    /// </summary>
    /// <param name="prefab">生成するPrefab。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="rotation">生成回転。</param>
    /// <param name="parent">親Transform。</param>
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
                Undo.RegisterCreatedObjectUndo(instance, "Generate Room");
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
    /// <param name="target">対象オブジェクト。</param>
    /// <returns>中心位置。</returns>
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
    /// Play中とEditor中の両方に対応して安全に削除します。
    /// </summary>
    /// <param name="target">削除対象。</param>
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
}
