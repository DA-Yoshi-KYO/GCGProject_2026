using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomPlayerPosition.cs
 *  制作者      : 吉本竜
 *  内容        : Playerが現在いるRoomCreatePointを管理するクラス
 *  履歴        : 2026/04/27 新規作成(ヨシモト)
 *==================================================*/

/// <summary>
/// Playerが現在いるRoomCreatePointを管理するクラスです。
/// </summary>
public class CS_RoomPlayerPosition : MonoBehaviour
{
    [Header("プレイヤー")]
    [SerializeField]
    private GameObject player;

    private GameObject playerRoomData;

    /// <summary>
    /// RaycastでPlayerが現在いるRoomCreatePointを更新します。
    /// </summary>
    public void RefreshPlayerRoomData()
    {
        playerRoomData = CS_RoomCreatePointRaycast.GetRayRoomCreatePoint(player);
    }

    /// <summary>
    /// Playerが現在いるRoomCreatePointを取得します。
    /// </summary>
    public GameObject GetPlayerRoomData()
    {
        return playerRoomData;
    }

    /// <summary>
    /// Playerオブジェクトを取得します。
    /// </summary>
    public GameObject GetPlayerObject()
    {
        return player;
    }
}
