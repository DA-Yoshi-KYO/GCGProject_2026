using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomPlayerPosition.cs
 *  制作者      : 吉本竜
 *  内容        : PlayerPrefabを生成し、Playerが現在いるRoomCreatePointを管理するクラス
 *  履歴        : 2026/04/27 新規作成(ヨシモト)
 *                2026/04/29 PlayerPrefabをStartPlayerPointへ生成する処理を追加(ヨシモト)
 *                2026/04/29 現在取得しているRoomCreatePointのDebug表示を追加(ヨシモト)
 *==================================================*/

/// <summary>
/// PlayerPrefabを生成し、Playerが現在いるRoomCreatePointを管理するクラスです。
/// </summary>
public class CS_RoomPlayerPosition : MonoBehaviour
{
    [Header("プレイヤーPrefab")]
    [SerializeField]
    private GameObject player;

    [Header("デバッグ表示")]
    [SerializeField]
    private bool bool_IsDebugCurrentRoom = true;

    private GameObject playerInstance;

    private GameObject playerRoomData;

    /// <summary>
    /// StartPlayerPointにPlayerPrefabを生成します。
    /// すでに生成済みのPlayerがある場合は削除してから生成します。
    /// </summary>
    /// <param name="startPlayerPoint">Player生成位置。</param>
    public void CreatePlayerAtStartPoint(Transform startPlayerPoint)
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

        RefreshPlayerRoomData();
    }

    /// <summary>
    /// RaycastでPlayerが現在いるRoomCreatePointを更新します。
    /// </summary>
    public void RefreshPlayerRoomData()
    {
        if (playerInstance == null)
        {
            playerRoomData = null;

            if (bool_IsDebugCurrentRoom)
            {
                Debug.LogWarning("[RoomPlayerPosition] 生成済みPlayerがありません。");
            }

            return;
        }

        playerRoomData = CS_RoomCreatePointRaycast.GetRayRoomCreatePoint(playerInstance);

        DebugCurrentRoomData();
    }

    /// <summary>
    /// Playerが現在いるRoomCreatePointを取得します。
    /// </summary>
    /// <returns>Playerが現在いるRoomCreatePoint。</returns>
    public GameObject GetPlayerRoomData()
    {
        return playerRoomData;
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
                + playerInstance.name
                + " / Position : "
                + playerInstance.transform.position
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
