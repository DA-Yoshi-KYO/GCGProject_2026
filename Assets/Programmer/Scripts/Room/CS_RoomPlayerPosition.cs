using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomPlayerPosition.cs
 *  制作者      : 吉本竜
 *  内容        : PlayerPrefabを生成し、Playerが現在いるRoomCreatePointを管理するクラス
 *  履歴        : 2026/04/27 新規作成(ヨシモト)
 *                2026/04/29 PlayerPrefabをStartPlayerPointへ生成する処理を追加(ヨシモト)
 *                2026/04/29 現在取得しているRoomCreatePointのDebug表示を追加(ヨシモト)
 *                2026/05/03 PlayerData.currentRoomData設定処理を追加(ヨシモト)
 *                2026/05/03 PlayerCameraが参照できるRoom階層を保証する処理を追加(ヨシモト)
 *==================================================*/

/// <summary>
/// PlayerPrefabを生成し、Playerが現在いるRoomCreatePointを管理するクラスです。
/// </summary>
public class CS_RoomPlayerPosition : MonoBehaviour
{
    private const string GENERATED_ROOM_PREFIX = "__GeneratedRoom_";
    private const string CENTER_NAME = "Center";

    [Header("プレイヤーPrefab")]
    [SerializeField]
    private GameObject player;

    [Header("デバッグ表示")]
    [SerializeField]
    private bool bool_IsDebugCurrentRoom = true;

    private GameObject playerInstance;
    private GameObject playerRoomData;
    private GameObject playerFloorData;

    /// <summary>
    /// PlayerPrefabを取得します。
    /// </summary>
    public GameObject PlayerPrefab => player;

    /// <summary>
    /// 生成済みPlayerを取得します。
    /// </summary>
    public GameObject PlayerInstance => playerInstance;

    /// <summary>
    /// Playerが現在いるRoomCreatePointを取得します。
    /// </summary>
    public GameObject PlayerRoomData => playerRoomData;

    /// <summary>
    /// Playerが現在いる床を取得します。
    /// </summary>
    public GameObject PlayerFloorData => playerRoomData;

    /// <summary>
    /// StartPlayerPointにPlayerPrefabを生成します。
    /// 互換用です。RoomCreatePointはRaycastで取得します。
    /// </summary>
    /// <param name="startPlayerPoint">Player生成位置。</param>
    public void CreatePlayerAtStartPoint(Transform startPlayerPoint)
    {
        CreatePlayerInstance(startPlayerPoint);
        RefreshPlayerRoomData();
    }

    /// <summary>
    /// StartPlayerPointにPlayerPrefabを生成し、現在RoomCreatePointを直接設定します。
    /// </summary>
    /// <param name="startPlayerPoint">Player生成位置。</param>
    /// <param name="roomCreatePointObject">現在いるRoomCreatePoint。</param>
    public void CreatePlayerAtStartPoint(
        Transform startPlayerPoint,
        GameObject roomCreatePointObject)
    {
        CreatePlayerInstance(startPlayerPoint);
        SetPlayerRoomData(roomCreatePointObject);
    }

    /// <summary>
    /// PlayerPrefabを生成します。
    /// </summary>
    /// <param name="startPlayerPoint">Player生成位置。</param>
    private void CreatePlayerInstance(Transform startPlayerPoint)
    {
        if (player == null)
        {
            Debug.LogWarning("[RoomPlayerPosition] PlayerPrefabが設定されていません。");
            return;
        }

        if (startPlayerPoint == null)
        {
            Debug.LogWarning("[RoomPlayerPosition] StartPlayerPointがnullです。");
            return;
        }

        DeletePlayerInstance();

        playerInstance = Instantiate(
            player,
            startPlayerPoint.position,
            startPlayerPoint.rotation
        );

        SetupPlayerDataReference();
    }

    /// <summary>
    /// 生成したPlayerのPlayerDataに、現在Room管理クラスを設定します。
    /// </summary>
    private void SetupPlayerDataReference()
    {
        if (playerInstance == null)
        {
            return;
        }

        CS_PlayerData playerData = playerInstance.GetComponent<CS_PlayerData>();

        if (playerData == null)
        {
            Debug.LogWarning("[RoomPlayerPosition] 生成したPlayerにPlayerDataが付いていません : " + playerInstance.name);
            return;
        }
    }

    /// <summary>
    /// RaycastでPlayerが現在いるRoomCreatePointを更新します。
    /// 基本的には保険用です。
    /// </summary>
    public void RefreshPlayerRoomData()
    {
        if (playerInstance == null)
        {
            playerRoomData = null;
            playerFloorData = null;

            if (bool_IsDebugCurrentRoom)
            {
                Debug.LogWarning("[RoomPlayerPosition] 生成済みPlayerがありません。");
            }

            return;
        }

        GameObject rayRoomObject =
            CS_RoomCreatePointRaycast.GetRayRoomCreatePoint(playerInstance);

        SetPlayerRoomData(rayRoomObject);
    }

    /// <summary>
    /// Playerが現在いるRoomCreatePointを直接設定します。
    /// PlayerCameraが参照できるようにRoom階層も整えます。
    /// </summary>
    /// <param name="roomCreatePointObject">現在いるRoomCreatePoint、またはその子階層。</param>
    public void SetPlayerRoomData(GameObject roomCreatePointObject)
    {
        GameObject normalizedRoomCreatePoint =
            FindRoomCreatePointObject(roomCreatePointObject);

        if (normalizedRoomCreatePoint == null)
        {
            playerRoomData = null;
            playerFloorData = null;
            DebugCurrentRoomData();
            return;
        }

        SetupRoomForPlayerCamera(normalizedRoomCreatePoint);

        playerRoomData = normalizedRoomCreatePoint;
        // TODO: 部屋の情報も取得できるように(もっといい方法ある...？)
        playerFloorData = playerRoomData.transform.GetComponentInChildren<RoomGrid>().gameObject;

        DebugCurrentRoomData();
    }

    /// <summary>
    /// Playerが現在いるRoomCreatePointを取得します。
    /// PlayerCameraなど既存コード互換用です。
    /// </summary>
    /// <returns>Playerが現在いるRoomCreatePoint。</returns>
    public GameObject GetPlayerRoomData()
    {
        return playerRoomData;
    }

    /// <summary>
    /// Playerが現在いるRoomCreatePointを取得します。
    /// PlayerActionなどギミックの設置位置決定用です。
    /// </summary>
    /// <returns>Playerが現在いるRoomCreatePoint。</returns>
    public GameObject GetPlayerFloorData()
    {
        return playerFloorData;
    }

    /// <summary>
    /// 生成済みPlayerを取得します。
    /// </summary>
    /// <returns>生成済みPlayer。</returns>
    public GameObject GetPlayerObject()
    {
        return playerInstance;
    }

    /// <summary>
    /// PlayerPrefabを取得します。
    /// </summary>
    /// <returns>PlayerPrefab。</returns>
    public GameObject GetPlayerPrefab()
    {
        return player;
    }

    /// <summary>
    /// 渡されたObjectまたは親階層からRoomCreatePointを取得します。
    /// </summary>
    /// <param name="targetObject">検索開始Object。</param>
    /// <returns>RoomCreatePointのGameObject。</returns>
    private GameObject FindRoomCreatePointObject(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return null;
        }

        Transform currentTransform = targetObject.transform;

        while (currentTransform != null)
        {
            CS_RoomCreatePoint roomCreatePoint =
                currentTransform.GetComponent<CS_RoomCreatePoint>();

            if (roomCreatePoint != null)
            {
                return currentTransform.gameObject;
            }

            currentTransform = currentTransform.parent;
        }

        return null;
    }

    /// <summary>
    /// PlayerCameraが参照できるRoom階層を保証します。
    /// </summary>
    /// <param name="roomCreatePointObject">RoomCreatePoint。</param>
    private void SetupRoomForPlayerCamera(GameObject roomCreatePointObject)
    {
        if (roomCreatePointObject == null)
        {
            return;
        }

        Transform generatedRoomTransform =
            FindGeneratedRoomChild(roomCreatePointObject.transform);

        if (generatedRoomTransform == null)
        {
            Debug.LogWarning(
                "[RoomPlayerPosition] RoomCreatePointの子に生成Roomがありません : "
                + roomCreatePointObject.name
            );

            return;
        }

        generatedRoomTransform.SetSiblingIndex(0);
        EnsureCenterDirectChild(generatedRoomTransform);
    }

    /// <summary>
    /// RoomCreatePointの子から生成Roomを取得します。
    /// </summary>
    /// <param name="roomCreatePointTransform">RoomCreatePointのTransform。</param>
    /// <returns>生成RoomのTransform。</returns>
    private Transform FindGeneratedRoomChild(Transform roomCreatePointTransform)
    {
        if (roomCreatePointTransform == null)
        {
            return null;
        }

        for (int i = 0 ; i < roomCreatePointTransform.childCount ; i++)
        {
            Transform childTransform = roomCreatePointTransform.GetChild(i);

            if (childTransform.name.StartsWith(GENERATED_ROOM_PREFIX))
            {
                return childTransform;
            }
        }

        if (roomCreatePointTransform.childCount > 0)
        {
            return roomCreatePointTransform.GetChild(0);
        }

        return null;
    }

    /// <summary>
    /// 生成Room直下にCenterがある状態を保証します。
    /// </summary>
    /// <param name="generatedRoomTransform">生成RoomのTransform。</param>
    private void EnsureCenterDirectChild(Transform generatedRoomTransform)
    {
        if (generatedRoomTransform == null)
        {
            return;
        }

        Transform directCenterTransform = generatedRoomTransform.Find(CENTER_NAME);

        if (directCenterTransform != null)
        {
            return;
        }

        Transform existingCenterTransform =
            FindChildByNameRecursive(generatedRoomTransform, CENTER_NAME);

        GameObject centerObject = new GameObject(CENTER_NAME);
        centerObject.transform.SetParent(generatedRoomTransform);

        if (existingCenterTransform != null)
        {
            centerObject.transform.SetPositionAndRotation(
                existingCenterTransform.position,
                existingCenterTransform.rotation
            );

            return;
        }

        centerObject.transform.SetPositionAndRotation(
            generatedRoomTransform.position,
            generatedRoomTransform.rotation
        );

        Debug.LogWarning(
            "[RoomPlayerPosition] 生成Room直下にCenterがなかったため、自動作成しました : "
            + generatedRoomTransform.name
        );
    }

    /// <summary>
    /// 子階層から指定名のTransformを探します。
    /// </summary>
    /// <param name="rootTransform">検索開始Transform。</param>
    /// <param name="targetName">探す名前。</param>
    /// <returns>見つかったTransform。</returns>
    private Transform FindChildByNameRecursive(Transform rootTransform, string targetName)
    {
        if (rootTransform == null)
        {
            return null;
        }

        Transform[] childTransforms =
            rootTransform.GetComponentsInChildren<Transform>(true);

        for (int i = 0 ; i < childTransforms.Length ; i++)
        {
            if (childTransforms[i].name == targetName)
            {
                return childTransforms[i];
            }
        }

        return null;
    }

    /// <summary>
    /// 生成済みPlayerを削除します。
    /// </summary>
    private void DeletePlayerInstance()
    {
        if (playerInstance == null)
        {
            return;
        }

        Destroy(playerInstance);
        playerInstance = null;
    }

    /// <summary>
    /// 現在取得しているRoomCreatePoint情報をDebug表示します。
    /// </summary>
    private void DebugCurrentRoomData()
    {
        if (!bool_IsDebugCurrentRoom)
        {
            return;
        }

        if (playerRoomData == null)
        {
            Debug.LogWarning(
                "[RoomPlayerPosition] Playerが現在いるRoomCreatePointを取得できていません。Player : "
                + GetPlayerName()
                + " / Position : "
                + GetPlayerPositionText()
            );

            return;
        }

        Transform roomTransform = playerRoomData.transform;
        Transform parentTransform = roomTransform.parent;

        string parentName = parentTransform != null ? parentTransform.name : "親なし";
        string hierarchyPath = GetHierarchyPath(roomTransform);

        Debug.Log(
            "[RoomPlayerPosition] 現在のRoomCreatePoint : "
            + playerRoomData.name
            + " / 親 : "
            + parentName
            + " / 階層 : "
            + hierarchyPath
        );
    }

    /// <summary>
    /// 生成済みPlayerの名前を取得します。
    /// </summary>
    /// <returns>生成済みPlayer名。</returns>
    private string GetPlayerName()
    {
        if (playerInstance == null)
        {
            return "null";
        }

        return playerInstance.name;
    }

    /// <summary>
    /// 生成済みPlayerの座標文字列を取得します。
    /// </summary>
    /// <returns>生成済みPlayer座標。</returns>
    private string GetPlayerPositionText()
    {
        if (playerInstance == null)
        {
            return "null";
        }

        return playerInstance.transform.position.ToString();
    }

    /// <summary>
    /// TransformのHierarchy上のパスを取得します。
    /// </summary>
    /// <param name="targetTransform">対象Transform。</param>
    /// <returns>Hierarchyパス。</returns>
    private string GetHierarchyPath(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            return "null";
        }

        string path = targetTransform.name;
        Transform currentTransform = targetTransform.parent;

        while (currentTransform != null)
        {
            path = currentTransform.name + "/" + path;
            currentTransform = currentTransform.parent;
        }

        return path;
    }
}
