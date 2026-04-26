using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*==================================================
 *  ファイル名  : CS_RoomBlockPrefabGenerator.cs
 *  制作者      : 吉本竜
 *  内容        : RoomCreatePointタグの位置にPrefabリストからランダムでルームブロックを生成する
 *  履歴        : 2026/04/26 新規作成(ヨシモト)
 *==================================================*/

/// <summary>
/// RoomCreatePointタグが付いたオブジェクトの中心位置に、
/// Prefabリストからランダムにルームブロックを生成するクラスです。
/// </summary>
public class CS_RoomBlockPrefabGenerator : MonoBehaviour
{
    private const string GENERATED_ROOT_NAME = "__GeneratedRoomBlocks";

    [Header("生成ポイントタグ")]
    [SerializeField]
    private string str_RoomCreatePointTag = "RoomCreatePoint";

    [Header("ランダム生成に使うルームブロックPrefab")]
    [SerializeField]
    private List<GameObject> list_RoomBlockPrefabs = new List<GameObject>();

    private void Awake()
    {
        CreateRooms();
    }

    /// <summary>
    /// ルームブロックを生成します。
    /// すでに生成済みの場合は重複生成を防ぐため、処理を中断します。
    /// </summary>
    [ContextMenu("ルームブロックを生成")]
    public void GenerateRoomBlocks()
    {
        GenerateRoomBlocksInternal(false);
    }

    /// <summary>
    /// 生成済みのルームブロックを削除してから、再度ランダム生成します。
    /// </summary>
    [ContextMenu("ルームブロックを再生成")]
    public void RegenerateRoomBlocks()
    {
        ClearGeneratedRoomBlocks();
        GenerateRoomBlocksInternal(true);
    }

    /// <summary>
    /// 生成済みのルームブロックを削除します。
    /// </summary>
    [ContextMenu("生成済みルームブロックを削除")]
    public void DeleteGeneratedRoomBlocks()
    {
        Transform tf_GeneratedRoot = FindGeneratedRoot();

        if (tf_GeneratedRoot == null)
        {
            Debug.Log("[RoomBlockGenerator] 削除対象の生成済みルームブロックはありません。");
            return;
        }

        DestroyObjectSafe(tf_GeneratedRoot.gameObject);

        Debug.Log("[RoomBlockGenerator] 生成済みルームブロックを削除しました。");
    }

    /// <summary>
    /// RoomCreatePointタグの位置にルームブロックを生成します。
    /// </summary>
    /// <param name="isForceGenerate">生成済みチェックを無視して生成するかどうか。</param>
    private void GenerateRoomBlocksInternal(bool isForceGenerate)
    {
        if (!isForceGenerate && HasGeneratedRoomBlocks())
        {
            Debug.LogWarning("[RoomBlockGenerator] すでに生成済みです。作り直す場合は「ルームブロックを再生成」を使ってください。");
            return;
        }

        List<GameObject> list_ValidPrefabs = GetValidRoomBlockPrefabs();

        if (list_ValidPrefabs.Count <= 0)
        {
            Debug.LogError("[RoomBlockGenerator] 生成に使えるPrefabがありません。Prefabリストを確認してください。");
            return;
        }

        GameObject[] roomCreatePoints = FindRoomCreatePoints();

        if (roomCreatePoints.Length <= 0)
        {
            Debug.LogWarning("[RoomBlockGenerator] RoomCreatePointタグが付いたオブジェクトが見つかりません。");
            return;
        }

        Transform tf_GeneratedRoot = GetOrCreateGeneratedRoot();

        for (int i = 0 ; i < roomCreatePoints.Length ; i++)
        {
            GameObject go_Point = roomCreatePoints[i];
            GameObject go_RandomPrefab = GetRandomRoomBlockPrefab(list_ValidPrefabs);

            if (go_RandomPrefab == null)
            {
                continue;
            }

            Vector3 v3_SpawnPosition = GetObjectCenter(go_Point);
            Quaternion q_SpawnRotation = go_Point.transform.rotation;

            GameObject go_GeneratedBlock = CreateRoomBlockInstance(
                go_RandomPrefab,
                v3_SpawnPosition,
                q_SpawnRotation,
                tf_GeneratedRoot
            );

            go_GeneratedBlock.name = go_RandomPrefab.name + "_Generated_" + i.ToString("00");
        }

        Debug.Log("[RoomBlockGenerator] ルームブロックを生成しました。生成数 : " + roomCreatePoints.Length);
    }

    /// <summary>
    /// RoomCreatePointタグが付いたオブジェクトをシーンから取得します。
    /// </summary>
    /// <returns>RoomCreatePointタグが付いたオブジェクト配列。</returns>
    private GameObject[] FindRoomCreatePoints()
    {
        if (string.IsNullOrWhiteSpace(str_RoomCreatePointTag))
        {
            Debug.LogError("[RoomBlockGenerator] タグ名が空です。");
            return new GameObject[0];
        }

        try
        {
            return GameObject.FindGameObjectsWithTag(str_RoomCreatePointTag);
        }
        catch (UnityException)
        {
            Debug.LogError("[RoomBlockGenerator] Tag「" + str_RoomCreatePointTag + "」が存在しません。UnityのTagsに追加してください。");
            return new GameObject[0];
        }
    }

    /// <summary>
    /// PrefabリストからnullではないPrefabだけを取得します。
    /// </summary>
    /// <returns>有効なPrefabリスト。</returns>
    private List<GameObject> GetValidRoomBlockPrefabs()
    {
        List<GameObject> list_ValidPrefabs = new List<GameObject>();

        for (int i = 0 ; i < list_RoomBlockPrefabs.Count ; i++)
        {
            if (list_RoomBlockPrefabs[i] == null)
            {
                continue;
            }

            list_ValidPrefabs.Add(list_RoomBlockPrefabs[i]);
        }

        return list_ValidPrefabs;
    }

    /// <summary>
    /// 有効なPrefabリストからランダムで1つ取得します。
    /// </summary>
    /// <param name="list_ValidPrefabs">有効なPrefabリスト。</param>
    /// <returns>ランダムに選ばれたPrefab。</returns>
    private GameObject GetRandomRoomBlockPrefab(List<GameObject> list_ValidPrefabs)
    {
        if (list_ValidPrefabs == null || list_ValidPrefabs.Count <= 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, list_ValidPrefabs.Count);
        return list_ValidPrefabs[randomIndex];
    }

    /// <summary>
    /// オブジェクトの中心位置を取得します。
    /// Collider、Renderer、Transformの順で中心を取得します。
    /// </summary>
    /// <param name="go_Target">中心を取得したい対象オブジェクト。</param>
    /// <returns>対象オブジェクトの中心位置。</returns>
    private Vector3 GetObjectCenter(GameObject go_Target)
    {
        if (TryGetCenterFromColliders(go_Target, out Vector3 v3_ColliderCenter))
        {
            return v3_ColliderCenter;
        }

        if (TryGetCenterFromRenderers(go_Target, out Vector3 v3_RendererCenter))
        {
            return v3_RendererCenter;
        }

        return go_Target.transform.position;
    }

    /// <summary>
    /// Colliderから中心位置を取得します。
    /// </summary>
    /// <param name="go_Target">中心を取得したい対象オブジェクト。</param>
    /// <param name="v3_Center">取得した中心位置。</param>
    /// <returns>中心位置を取得できた場合はtrue。</returns>
    private bool TryGetCenterFromColliders(GameObject go_Target, out Vector3 v3_Center)
    {
        Collider[] colliders = go_Target.GetComponentsInChildren<Collider>();

        if (colliders.Length <= 0)
        {
            v3_Center = Vector3.zero;
            return false;
        }

        Bounds bounds = colliders[0].bounds;

        for (int i = 1 ; i < colliders.Length ; i++)
        {
            bounds.Encapsulate(colliders[i].bounds);
        }

        v3_Center = bounds.center;
        return true;
    }

    /// <summary>
    /// Rendererから中心位置を取得します。
    /// </summary>
    /// <param name="go_Target">中心を取得したい対象オブジェクト。</param>
    /// <param name="v3_Center">取得した中心位置。</param>
    /// <returns>中心位置を取得できた場合はtrue。</returns>
    private bool TryGetCenterFromRenderers(GameObject go_Target, out Vector3 v3_Center)
    {
        Renderer[] renderers = go_Target.GetComponentsInChildren<Renderer>();

        if (renderers.Length <= 0)
        {
            v3_Center = Vector3.zero;
            return false;
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1 ; i < renderers.Length ; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        v3_Center = bounds.center;
        return true;
    }

    /// <summary>
    /// 生成済みルームブロックをまとめる親オブジェクトを取得します。
    /// </summary>
    /// <returns>生成済みルームブロックの親Transform。</returns>
    private Transform FindGeneratedRoot()
    {
        return transform.Find(GENERATED_ROOT_NAME);
    }

    /// <summary>
    /// 生成済みルームブロックをまとめる親オブジェクトを取得または作成します。
    /// </summary>
    /// <returns>生成済みルームブロックの親Transform。</returns>
    private Transform GetOrCreateGeneratedRoot()
    {
        Transform tf_GeneratedRoot = FindGeneratedRoot();

        if (tf_GeneratedRoot != null)
        {
            return tf_GeneratedRoot;
        }

        GameObject go_GeneratedRoot = new GameObject(GENERATED_ROOT_NAME);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.RegisterCreatedObjectUndo(go_GeneratedRoot, "Create Generated Room Blocks Root");
        }
#endif

        go_GeneratedRoot.transform.SetParent(transform);
        go_GeneratedRoot.transform.localPosition = Vector3.zero;
        go_GeneratedRoot.transform.localRotation = Quaternion.identity;
        go_GeneratedRoot.transform.localScale = Vector3.one;

        return go_GeneratedRoot.transform;
    }

    /// <summary>
    /// 生成済みルームブロックだけを削除します。
    /// 親オブジェクト自体は残します。
    /// </summary>
    private void ClearGeneratedRoomBlocks()
    {
        Transform tf_GeneratedRoot = FindGeneratedRoot();

        if (tf_GeneratedRoot == null)
        {
            return;
        }

        for (int i = tf_GeneratedRoot.childCount - 1 ; i >= 0 ; i--)
        {
            DestroyObjectSafe(tf_GeneratedRoot.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// ルームブロックがすでに生成済みか確認します。
    /// </summary>
    /// <returns>生成済みルームブロックがある場合はtrue。</returns>
    private bool HasGeneratedRoomBlocks()
    {
        Transform tf_GeneratedRoot = FindGeneratedRoot();

        if (tf_GeneratedRoot == null)
        {
            return false;
        }

        return tf_GeneratedRoot.childCount > 0;
    }

    /// <summary>
    /// Prefabのインスタンスを作成します。
    /// Editor上ではPrefab接続をなるべく維持して生成します。
    /// </summary>
    /// <param name="go_Prefab">生成するPrefab。</param>
    /// <param name="v3_Position">生成位置。</param>
    /// <param name="q_Rotation">生成回転。</param>
    /// <param name="tf_Parent">生成先の親Transform。</param>
    /// <returns>生成されたGameObject。</returns>
    private GameObject CreateRoomBlockInstance(GameObject go_Prefab, Vector3 v3_Position, Quaternion q_Rotation, Transform tf_Parent)
    {
        GameObject go_Instance = null;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            try
            {
                go_Instance = PrefabUtility.InstantiatePrefab(go_Prefab, tf_Parent) as GameObject;
            }
            catch
            {
                go_Instance = null;
            }

            if (go_Instance != null)
            {
                Undo.RegisterCreatedObjectUndo(go_Instance, "Generate Room Block");
            }
        }
#endif

        if (go_Instance == null)
        {
            go_Instance = Instantiate(go_Prefab, tf_Parent);
        }

        go_Instance.transform.SetPositionAndRotation(v3_Position, q_Rotation);

        return go_Instance;
    }

    /// <summary>
    /// Play中とEditor中の両方に対応して、安全にオブジェクトを削除します。
    /// </summary>
    /// <param name="go_Target">削除対象のGameObject。</param>
    private void DestroyObjectSafe(GameObject go_Target)
    {
        if (go_Target == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.DestroyObjectImmediate(go_Target);
            return;
        }
#endif

        Destroy(go_Target);
    }

    /// <summary>
    /// 外部スクリプトやUIボタンからルームブロック生成を実行します。
    /// </summary>
    public void CreateRooms()
    {
        GenerateRoomBlocks();
    }

    /// <summary>
    /// 外部スクリプトやUIボタンからルームブロック再生成を実行します。
    /// </summary>
    public void RecreateRooms()
    {
        RegenerateRoomBlocks();
    }

    /// <summary>
    /// 外部スクリプトやUIボタンから生成済みルームブロック削除を実行します。
    /// </summary>
    public void DeleteRooms()
    {
        DeleteGeneratedRoomBlocks();
    }
}
