using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomCreatePointRaycast.cs
 *  制作者      : 吉本竜
 *  内容        : Playerの下方向へRaycastを行い、現在いるRoomCreatePointを取得するクラス
 *  履歴        : 2026/04/27 新規作成(ヨシモト)
 *                2026/04/29 RaycastAllでPlaneタグを探す形へ修正(ヨシモト)
 *==================================================*/

/// <summary>
/// Playerの下方向へRaycastを行い、現在いるRoomCreatePointを取得するクラスです。
/// </summary>
public static class CS_RoomCreatePointRaycast
{
    private const string PLANE_TAG = "Plane";
    private const string ROOM_CREATE_POINT_TAG = "RoomCreatePoint";

    private const float RAY_START_UP_OFFSET = 1.0f;
    private const float RAY_DISTANCE = 100.0f;

    /// <summary>
    /// Playerの下方向にRaycastを行い、現在いるRoomCreatePointを取得します。
    /// </summary>
    /// <param name="player">確認対象のPlayer。</param>
    /// <returns>Playerが現在いるRoomCreatePoint。</returns>
    public static GameObject GetRayRoomCreatePoint(GameObject player)
    {
        if (player == null)
        {
            Debug.LogWarning("[CS_RoomCreatePointRaycast] Playerがnullです。");
            return null;
        }

        Vector3 rayStartPosition = player.transform.position + Vector3.up * RAY_START_UP_OFFSET;

        RaycastHit[] hits = Physics.RaycastAll(
            rayStartPosition,
            Vector3.down,
            RAY_DISTANCE,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        if (hits == null || hits.Length <= 0)
        {
            Debug.LogWarning("[CS_RoomCreatePointRaycast] " + player.name + " の下方向にColliderが見つかりませんでした。");
            Debug.DrawRay(rayStartPosition, Vector3.down * RAY_DISTANCE, Color.red, 3.0f);
            return null;
        }

        System.Array.Sort(hits, CompareRaycastHitDistance);

        for (int i = 0 ; i < hits.Length ; i++)
        {
            Collider hitCollider = hits[i].collider;

            if (hitCollider == null)
            {
                continue;
            }

            if (hitCollider.gameObject == player)
            {
                continue;
            }

            if (hitCollider.transform.IsChildOf(player.transform))
            {
                continue;
            }

            if (!hitCollider.CompareTag(PLANE_TAG))
            {
                continue;
            }

            GameObject roomCreatePoint = FindParentRoomCreatePoint(hitCollider.transform);

            if (roomCreatePoint == null)
            {
                Debug.LogWarning("[CS_RoomCreatePointRaycast] PlaneタグのColliderは見つかりましたが、親階層にRoomCreatePointがありません : " + hitCollider.name);
                Debug.DrawRay(rayStartPosition, Vector3.down * hits[i].distance, Color.yellow, 3.0f);
                return null;
            }

            Debug.DrawRay(rayStartPosition, Vector3.down * hits[i].distance, Color.green, 3.0f);
            return roomCreatePoint;
        }

        Debug.LogWarning("[CS_RoomCreatePointRaycast] " + player.name + " の下方向にPlaneタグのColliderが見つかりませんでした。");
        Debug.DrawRay(rayStartPosition, Vector3.down * RAY_DISTANCE, Color.red, 3.0f);

        return null;
    }

    /// <summary>
    /// RaycastHitを距離順に並べるための比較処理です。
    /// </summary>
    /// <param name="a">比較対象A。</param>
    /// <param name="b">比較対象B。</param>
    /// <returns>距離の比較結果。</returns>
    private static int CompareRaycastHitDistance(RaycastHit a, RaycastHit b)
    {
        return a.distance.CompareTo(b.distance);
    }

    /// <summary>
    /// 指定Transformの親階層からRoomCreatePointを探します。
    /// </summary>
    /// <param name="targetTransform">検索開始Transform。</param>
    /// <returns>見つかったRoomCreatePoint。</returns>
    private static GameObject FindParentRoomCreatePoint(Transform targetTransform)
    {
        Transform currentTransform = targetTransform;

        while (currentTransform != null)
        {
            if (currentTransform.CompareTag(ROOM_CREATE_POINT_TAG))
            {
                return currentTransform.gameObject;
            }

            currentTransform = currentTransform.parent;
        }

        return null;
    }
}
