using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*==================================================
 *  ファイル名  : CS_RoomBlockPrefabGenerator.cs
 *  制作者      : 吉本竜
 *  内容        : 登録されたRoomCreatePointの子としてRoomPrefabを生成し、
 *                生成後にRoomMovePoint同士を動的接続する
 *                Element0の固定Room内にあるStartPlayerPointへPlayerPrefabを生成する
 *  履歴        : 2026/04/27 RoomCreatePointの子に生成する形へ修正(ヨシモト)
 *                2026/04/27 実行時の自動再生成処理を追加(ヨシモト)
 *                2026/04/28 登録リスト生成方式へ変更(ヨシモト)
 *                2026/04/28 生成方式をRoomCreatePointごとの設定へ変更(ヨシモト)
 *                2026/04/28 RoomPlayerPositionを同一GameObjectから取得する形へ変更(ヨシモト)
 *                2026/04/28 Fixedは事前生成、Randomはゲーム開始時生成へ変更(ヨシモト)
 *                2026/04/29 Element0のStartPlayerPointへPlayerPrefabを生成する処理を追加(ヨシモト)
 *==================================================*/

/// <summary>
/// RoomPrefabの生成方式です。
/// </summary>
public enum CSE_RoomBlockGenerateType
{
    Fixed,
    Random
}

/// <summary>
/// 登録されたRoomCreatePointの子としてRoomPrefabを生成するクラスです。
/// Fixedはゲーム開始前に生成し、Randomはゲーム開始時に自動再生成します。
/// </summary>
public class CS_RoomBlockPrefabGenerator : MonoBehaviour
{
    private const string ROOM_CREATE_POINT_TAG = "RoomCreatePoint";
    private const string START_PLAYER_POINT_NAME = "StartPlayerPoint";
    private const string GENERATED_NAME_PREFIX = "__GeneratedRoom_";
    private const string DELETING_NAME_PREFIX = "__DeletingRoom_";
    private const string OLD_GENERATED_ROOT_NAME = "__GeneratedRoomBlocks";

    [System.Serializable]
    private class CS_RoomCreatePointGenerateData
    {
        [Header("生成先RoomCreatePoint")]
        [SerializeField]
        private GameObject go_RoomCreatePointObject;

        [Header("このRoomCreatePointの生成方式")]
        [SerializeField]
        private CSE_RoomBlockGenerateType e_GenerateType = CSE_RoomBlockGenerateType.Random;

        [Header("固定生成で使うRoomPrefab")]
        [SerializeField]
        private GameObject go_FixedRoomPrefab;

        [Header("ランダム生成で使うRoomPrefab候補")]
        [SerializeField]
        private List<GameObject> list_RandomRoomBlockPrefabs = new List<GameObject>();

        /// <summary>
        /// RoomCreatePointのGameObjectを取得します。
        /// </summary>
        public GameObject RoomCreatePointObject => go_RoomCreatePointObject;

        /// <summary>
        /// このRoomCreatePointの生成方式を取得します。
        /// </summary>
        public CSE_RoomBlockGenerateType GenerateType => e_GenerateType;

        /// <summary>
        /// 固定生成で使うRoomPrefabを取得します。
        /// </summary>
        public GameObject FixedRoomPrefab => go_FixedRoomPrefab;

        /// <summary>
        /// ランダム生成で使うRoomPrefab候補を取得します。
        /// </summary>
        public List<GameObject> RandomRoomBlockPrefabs => list_RandomRoomBlockPrefabs;

        /// <summary>
        /// RoomCreatePointのTransformを取得します。
        /// </summary>
        public Transform RoomCreatePointTransform
        {
            get
            {
                if (go_RoomCreatePointObject == null)
                {
                    return null;
                }

                return go_RoomCreatePointObject.transform;
            }
        }

        /// <summary>
        /// CS_RoomCreatePointを取得します。
        /// </summary>
        public CS_RoomCreatePoint RoomCreatePoint
        {
            get
            {
                if (go_RoomCreatePointObject == null)
                {
                    return null;
                }

                return go_RoomCreatePointObject.GetComponent<CS_RoomCreatePoint>();
            }
        }
    }

    [Header("生成対象RoomCreatePoint一覧")]
    [SerializeField]
    private List<CS_RoomCreatePointGenerateData> list_RoomCreatePointGenerateData =
        new List<CS_RoomCreatePointGenerateData>();

    [Header("実行時ランダム生成設定")]
    [SerializeField]
    private bool bool_IsAutoRegenerateRandomOnStart = true;

    private bool bool_IsRuntimeRegenerating = false;

    private CS_RoomPlayerPosition cs_RoomPlayerPosition;

    /// <summary>
    /// ゲーム実行時にRandom設定のRoomだけを自動再生成します。
    /// Fixed設定のRoomは事前生成されたものをそのまま使います。
    /// </summary>
    private void Awake()
    {
        CacheRoomPlayerPosition();

        if (!Application.isPlaying)
        {
            return;
        }

        if (!bool_IsAutoRegenerateRandomOnStart)
        {
            RebuildGeneratedRoomLinks();
            CreatePlayerAtFirstRoomStartPoint();
            RefreshRoomPlayerPosition();
            return;
        }

        StartCoroutine(RegenerateRandomRoomBlocksRuntimeCoroutine());
    }

    /// <summary>
    /// Fixed設定のRoomを生成します。
    /// ゲーム開始前に使用する想定です。
    /// </summary>
    [ContextMenu("固定ルームブロックを生成")]
    public void GenerateRoomBlocks()
    {
        GenerateRoomBlocksByType(CSE_RoomBlockGenerateType.Fixed, false);
        RefreshRoomPlayerPosition();
    }

    /// <summary>
    /// Fixed設定のRoomを削除してから再生成します。
    /// ゲーム開始前に使用する想定です。
    /// </summary>
    [ContextMenu("固定ルームブロックを再生成")]
    public void RegenerateRoomBlocks()
    {
        DeleteGeneratedRoomBlocksByType(CSE_RoomBlockGenerateType.Fixed);
        GenerateRoomBlocksByType(CSE_RoomBlockGenerateType.Fixed, true);
        RefreshRoomPlayerPosition();
    }

    /// <summary>
    /// Random設定のRoomを生成します。
    /// 基本的にはゲーム開始時に自動実行されます。
    /// </summary>
    [ContextMenu("ランダムルームブロックを生成")]
    public void GenerateRandomRoomBlocks()
    {
        GenerateRoomBlocksByType(CSE_RoomBlockGenerateType.Random, false);
        RefreshRoomPlayerPosition();
    }

    /// <summary>
    /// Random設定のRoomを削除してから再生成します。
    /// </summary>
    [ContextMenu("ランダムルームブロックを再生成")]
    public void RegenerateRandomRoomBlocks()
    {
        if (Application.isPlaying)
        {
            StartCoroutine(RegenerateRandomRoomBlocksRuntimeCoroutine());
            return;
        }

        DeleteGeneratedRoomBlocksByType(CSE_RoomBlockGenerateType.Random);
        GenerateRoomBlocksByType(CSE_RoomBlockGenerateType.Random, true);
        RefreshRoomPlayerPosition();
    }

    /// <summary>
    /// 生成済みRoomをすべて削除します。
    /// </summary>
    [ContextMenu("生成済みルームブロックをすべて削除")]
    public void DeleteGeneratedRoomBlocks()
    {
        DeleteGeneratedRoomBlocksByType(CSE_RoomBlockGenerateType.Fixed);
        DeleteGeneratedRoomBlocksByType(CSE_RoomBlockGenerateType.Random);
        DeleteOldGeneratedRoot();

        Debug.Log("[RoomBlockPrefabGenerator] 生成済みRoomをすべて削除しました。");
    }

    /// <summary>
    /// 生成済みRoomのRoomMovePoint接続だけを再構築します。
    /// </summary>
    [ContextMenu("生成済みルーム接続を更新")]
    public void RebuildGeneratedRoomLinks()
    {
        Dictionary<CS_RoomCreatePoint, GameObject> dic_GeneratedRoomMap =
            BuildGeneratedRoomMapFromRegisteredList();

        if (dic_GeneratedRoomMap.Count <= 0)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] 接続更新できる生成済みRoomがありません。");
            return;
        }

        ConnectGeneratedRooms(dic_GeneratedRoomMap);
    }

    /// <summary>
    /// 外部関数からFixedのRoom生成を実行します。
    /// </summary>
    public void CreateRooms()
    {
        GenerateRoomBlocks();
    }

    /// <summary>
    /// 外部関数からFixedのRoom再生成を実行します。
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
    /// Play中用のRandom Room再生成処理です。
    /// Destroyはフレーム終わりに実行されるため、1フレーム待ってから再生成します。
    /// </summary>
    private IEnumerator RegenerateRandomRoomBlocksRuntimeCoroutine()
    {
        if (bool_IsRuntimeRegenerating)
        {
            yield break;
        }

        bool_IsRuntimeRegenerating = true;

        DeleteGeneratedRoomBlocksByType(CSE_RoomBlockGenerateType.Random);

        yield return null;

        GenerateRoomBlocksByType(CSE_RoomBlockGenerateType.Random, true);

        yield return null;

        CreatePlayerAtFirstRoomStartPoint();
        RefreshRoomPlayerPosition();

        bool_IsRuntimeRegenerating = false;
    }

    /// <summary>
    /// 指定した生成方式のRoomを生成します。
    /// </summary>
    /// <param name="targetGenerateType">生成対象の方式。</param>
    /// <param name="bool_IsReplaceExisting">既存生成Roomを置き換える場合はtrue。</param>
    private void GenerateRoomBlocksByType(
        CSE_RoomBlockGenerateType targetGenerateType,
        bool bool_IsReplaceExisting)
    {
        if (list_RoomCreatePointGenerateData == null || list_RoomCreatePointGenerateData.Count <= 0)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] 生成対象RoomCreatePointが登録されていません。");
            return;
        }

        DeleteOldGeneratedRoot();

        int generatedCount = 0;

        for (int i = 0 ; i < list_RoomCreatePointGenerateData.Count ; i++)
        {
            CS_RoomCreatePointGenerateData generateData = list_RoomCreatePointGenerateData[i];

            if (generateData == null)
            {
                continue;
            }

            if (generateData.GenerateType != targetGenerateType)
            {
                continue;
            }

            if (!IsValidGenerateData(generateData, i))
            {
                continue;
            }

            Transform pointTransform = generateData.RoomCreatePointTransform;

            if (pointTransform == null)
            {
                continue;
            }

            if (bool_IsReplaceExisting)
            {
                DeleteGeneratedChildren(pointTransform);
            }
            else
            {
                if (FindGeneratedRoomChild(pointTransform) != null)
                {
                    Debug.LogWarning("[RoomBlockPrefabGenerator] すでに生成済みのRoomがあります。再生成したい場合は再生成メニューを使ってください : " + generateData.RoomCreatePointObject.name);
                    continue;
                }
            }

            GameObject roomPrefab = GetRoomBlockPrefab(generateData, i);

            if (roomPrefab == null)
            {
                continue;
            }

            Vector3 spawnPosition = pointTransform.position;
            Quaternion spawnRotation = pointTransform.rotation;

            GameObject generatedRoom = CreateRoomInstance(
                roomPrefab,
                spawnPosition,
                spawnRotation,
                pointTransform
            );

            generatedRoom.name = GENERATED_NAME_PREFIX + roomPrefab.name + "_" + i.ToString("00");

            generatedCount++;
        }

        Dictionary<CS_RoomCreatePoint, GameObject> dic_GeneratedRoomMap =
            BuildGeneratedRoomMapFromRegisteredList();

        if (dic_GeneratedRoomMap.Count > 0)
        {
            ConnectGeneratedRooms(dic_GeneratedRoomMap);
        }

        Debug.Log("[RoomBlockPrefabGenerator] " + targetGenerateType + " のRoomを生成しました。生成数 : " + generatedCount);
    }

    /// <summary>
    /// 指定した生成方式の生成済みRoomを削除します。
    /// </summary>
    /// <param name="targetGenerateType">削除対象の方式。</param>
    private void DeleteGeneratedRoomBlocksByType(CSE_RoomBlockGenerateType targetGenerateType)
    {
        if (list_RoomCreatePointGenerateData == null)
        {
            return;
        }

        for (int i = 0 ; i < list_RoomCreatePointGenerateData.Count ; i++)
        {
            CS_RoomCreatePointGenerateData generateData = list_RoomCreatePointGenerateData[i];

            if (generateData == null)
            {
                continue;
            }

            if (generateData.GenerateType != targetGenerateType)
            {
                continue;
            }

            if (generateData.RoomCreatePointTransform == null)
            {
                continue;
            }

            DeleteGeneratedChildren(generateData.RoomCreatePointTransform);
        }

        Debug.Log("[RoomBlockPrefabGenerator] " + targetGenerateType + " の生成済みRoomを削除しました。");
    }

    /// <summary>
    /// 生成データが有効か確認します。
    /// </summary>
    /// <param name="generateData">生成データ。</param>
    /// <param name="index">リスト番号。</param>
    /// <returns>有効な場合はtrue。</returns>
    private bool IsValidGenerateData(CS_RoomCreatePointGenerateData generateData, int index)
    {
        if (generateData == null)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] 生成データがnullです。Index : " + index);
            return false;
        }

        if (generateData.RoomCreatePointObject == null)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] RoomCreatePointObjectが登録されていません。Index : " + index);
            return false;
        }

        if (generateData.RoomCreatePoint == null)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] CS_RoomCreatePointが付いていません : " + generateData.RoomCreatePointObject.name);
            return false;
        }

        if (!IsRoomCreatePointTagValid(generateData.RoomCreatePointObject))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// RoomCreatePointタグが正しく設定されているか確認します。
    /// </summary>
    /// <param name="target">確認対象。</param>
    /// <returns>正しい場合はtrue。</returns>
    private bool IsRoomCreatePointTagValid(GameObject target)
    {
        if (target == null)
        {
            return false;
        }

        try
        {
            if (!target.CompareTag(ROOM_CREATE_POINT_TAG))
            {
                Debug.LogWarning("[RoomBlockPrefabGenerator] RoomCreatePointタグが付いていません : " + target.name);
                return false;
            }
        }
        catch (UnityException)
        {
            Debug.LogError("[RoomBlockPrefabGenerator] Tag「" + ROOM_CREATE_POINT_TAG + "」が存在しません。UnityのTagsに追加してください。");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 生成方式に応じたRoomPrefabを取得します。
    /// </summary>
    /// <param name="generateData">生成データ。</param>
    /// <param name="index">リスト番号。</param>
    /// <returns>生成に使うRoomPrefab。</returns>
    private GameObject GetRoomBlockPrefab(CS_RoomCreatePointGenerateData generateData, int index)
    {
        if (generateData.GenerateType == CSE_RoomBlockGenerateType.Fixed)
        {
            return GetFixedRoomBlockPrefab(generateData, index);
        }

        return GetRandomRoomBlockPrefab(generateData, index);
    }

    /// <summary>
    /// 固定生成用のRoomPrefabを取得します。
    /// </summary>
    /// <param name="generateData">生成データ。</param>
    /// <param name="index">リスト番号。</param>
    /// <returns>固定生成用RoomPrefab。</returns>
    private GameObject GetFixedRoomBlockPrefab(CS_RoomCreatePointGenerateData generateData, int index)
    {
        if (generateData.FixedRoomPrefab == null)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] 固定生成用RoomPrefabが設定されていません。Index : " + index);
            return null;
        }

        return generateData.FixedRoomPrefab;
    }

    /// <summary>
    /// ランダム生成用のRoomPrefabを取得します。
    /// </summary>
    /// <param name="generateData">生成データ。</param>
    /// <param name="index">リスト番号。</param>
    /// <returns>ランダムに選ばれたRoomPrefab。</returns>
    private GameObject GetRandomRoomBlockPrefab(CS_RoomCreatePointGenerateData generateData, int index)
    {
        List<GameObject> validPrefabs = GetValidRoomBlockPrefabs(generateData.RandomRoomBlockPrefabs);

        if (validPrefabs.Count <= 0)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] ランダム生成候補RoomPrefabが設定されていません。Index : " + index);
            return null;
        }

        int randomIndex = Random.Range(0, validPrefabs.Count);
        return validPrefabs[randomIndex];
    }

    /// <summary>
    /// nullではないRoomPrefabだけを取得します。
    /// </summary>
    /// <param name="roomPrefabs">確認対象Prefabリスト。</param>
    /// <returns>有効なRoomPrefabリスト。</returns>
    private List<GameObject> GetValidRoomBlockPrefabs(List<GameObject> roomPrefabs)
    {
        List<GameObject> validPrefabs = new List<GameObject>();

        if (roomPrefabs == null)
        {
            return validPrefabs;
        }

        for (int i = 0 ; i < roomPrefabs.Count ; i++)
        {
            if (roomPrefabs[i] == null)
            {
                continue;
            }

            validPrefabs.Add(roomPrefabs[i]);
        }

        return validPrefabs;
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
        if (string.IsNullOrEmpty(objectName))
        {
            return false;
        }

        if (objectName.StartsWith(DELETING_NAME_PREFIX))
        {
            return false;
        }

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

        target.name = DELETING_NAME_PREFIX + target.name;
        target.SetActive(false);
        Destroy(target);
    }

    /// <summary>
    /// 登録リストから、RoomCreatePointと生成Roomの対応表を作成します。
    /// </summary>
    /// <returns>RoomCreatePointと生成Roomの対応表。</returns>
    private Dictionary<CS_RoomCreatePoint, GameObject> BuildGeneratedRoomMapFromRegisteredList()
    {
        Dictionary<CS_RoomCreatePoint, GameObject> dic_GeneratedRoomMap =
            new Dictionary<CS_RoomCreatePoint, GameObject>();

        if (list_RoomCreatePointGenerateData == null)
        {
            return dic_GeneratedRoomMap;
        }

        for (int i = 0 ; i < list_RoomCreatePointGenerateData.Count ; i++)
        {
            CS_RoomCreatePointGenerateData generateData = list_RoomCreatePointGenerateData[i];

            if (generateData == null)
            {
                continue;
            }

            CS_RoomCreatePoint createPoint = generateData.RoomCreatePoint;

            if (createPoint == null)
            {
                continue;
            }

            Transform pointTransform = generateData.RoomCreatePointTransform;

            if (pointTransform == null)
            {
                continue;
            }

            GameObject generatedRoom = FindGeneratedRoomChild(pointTransform);

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
        if (parent == null)
        {
            return null;
        }

        for (int i = 0 ; i < parent.childCount ; i++)
        {
            Transform child = parent.GetChild(i);

            if (!IsGeneratedRoomName(child.name))
            {
                continue;
            }

            if (!child.gameObject.activeInHierarchy)
            {
                continue;
            }

            return child.gameObject;
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

    /// <summary>
    /// Element0の生成Room内にあるStartPlayerPointへPlayerPrefabを生成します。
    /// </summary>
    private void CreatePlayerAtFirstRoomStartPoint()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        CacheRoomPlayerPosition();

        if (cs_RoomPlayerPosition == null)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] 同じGameObjectにCS_RoomPlayerPositionが付いていません。");
            return;
        }

        if (list_RoomCreatePointGenerateData == null || list_RoomCreatePointGenerateData.Count <= 0)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] Element0のRoomCreatePointがありません。");
            return;
        }

        CS_RoomCreatePointGenerateData firstGenerateData = list_RoomCreatePointGenerateData[0];

        if (firstGenerateData == null || firstGenerateData.RoomCreatePointTransform == null)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] Element0のRoomCreatePointが設定されていません。");
            return;
        }

        GameObject firstGeneratedRoom = FindGeneratedRoomChild(firstGenerateData.RoomCreatePointTransform);

        if (firstGeneratedRoom == null)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] Element0のRoomCreatePoint内に生成済みRoomがありません。");
            return;
        }

        Transform startPlayerPoint = FindStartPlayerPoint(firstGeneratedRoom.transform);

        if (startPlayerPoint == null)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] Element0の生成Room内にStartPlayerPointがありません。");
            return;
        }

        cs_RoomPlayerPosition.CreatePlayerAtStartPoint(startPlayerPoint);
    }

    /// <summary>
    /// Room内からStartPlayerPointを探します。
    /// </summary>
    /// <param name="roomRoot">探す対象のRoomルート。</param>
    /// <returns>StartPlayerPointのTransform。</returns>
    private Transform FindStartPlayerPoint(Transform roomRoot)
    {
        if (roomRoot == null)
        {
            return null;
        }

        Transform[] childTransforms = roomRoot.GetComponentsInChildren<Transform>(true);

        for (int i = 0 ; i < childTransforms.Length ; i++)
        {
            if (childTransforms[i].name == START_PLAYER_POINT_NAME)
            {
                return childTransforms[i];
            }
        }

        return null;
    }

    /// <summary>
    /// 同じGameObjectに付いているCS_RoomPlayerPositionを取得します。
    /// </summary>
    private void CacheRoomPlayerPosition()
    {
        if (cs_RoomPlayerPosition != null)
        {
            return;
        }

        cs_RoomPlayerPosition = GetComponent<CS_RoomPlayerPosition>();
    }

    /// <summary>
    /// PlayerPosition側の現在Room情報を更新します。
    /// </summary>
    private void RefreshRoomPlayerPosition()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        CacheRoomPlayerPosition();

        if (cs_RoomPlayerPosition == null)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] 同じGameObjectにCS_RoomPlayerPositionが付いていません。");
            return;
        }

        cs_RoomPlayerPosition.RefreshPlayerRoomData();
    }
}
