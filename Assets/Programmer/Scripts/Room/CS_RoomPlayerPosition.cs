using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomPlayerPosition.cs
 *  制作者      : 吉本竜
 *  内容        : Playerが現在いるRoomCreatePointを管理するクラス
 *  履歴        : 2026/04/27 新規作成(ヨシモト)
 *                2026/04/28 CreatePoint更新時にplaneObjectも探索するように更新(ヨシダ)
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
    [HideInInspector] public GameObject planeObject { private set; get; }

    /// <summary>
    /// RaycastでPlayerが現在いるRoomCreatePointを更新します。
    /// </summary>
    public void RefreshPlayerRoomData()
    {
        playerRoomData = CS_RoomCreatePointRaycast.GetRayRoomCreatePoint(player);

        if (playerRoomData != null)
        {
            Transform[] children = playerRoomData.GetComponentsInChildren<Transform>();
            foreach (Transform t in children)
            {
                if (t.name == "Plane")
                {
                    planeObject = t.gameObject;
                    break;
                }
            }
        }
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
