using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/*==================================================
 *  ファイル名  : CS_RoomCreatePointRaycast.cs
 *  制作者      : 吉本竜
 *  内容        : 指定オブジェクトの下方向へRayを飛ばし、
 *                Planeタグの床から所属するRoomCreatePointを取得する処理
 *  履歴        : 2026/04/27 新規作成(ヨシモト)
 *==================================================*/

/// <summary>
/// RoomCreatePointをRaycastで取得するためのユーティリティクラスです。
/// </summary>
public static class CS_RoomCreatePointRaycast
{
    private const float F_RAY_DISTANCE = 255.0f;
    private const string STR_PLANE_TAG_NAME = "Plane";
    private const string STR_ROOM_CREATE_POINT_PREFIX = "RoomCreatePoint";

    /// <summary>
    /// 指定したオブジェクトの下にあるPlaneタグを探し、
    /// そのPlaneが所属しているRoomCreatePointを取得します。
    /// </summary>
    /// <param name="go_TargetObject">判定元のオブジェクトです。</param>
    /// <returns>
    /// 見つかったRoomCreatePointのGameObjectを返します。
    /// 見つからない場合はnullを返します。
    /// </returns>
    public static GameObject GetRayRoomCreatePoint(GameObject go_TargetObject)
    {
        if (go_TargetObject == null)
        {
            Debug.LogWarning("[CS_RoomCreatePointRaycast] 判定元オブジェクトがnullです。");
            return null;
        }

        Scene scene_Target = go_TargetObject.scene;

        if (!scene_Target.IsValid() || !scene_Target.isLoaded)
        {
            Debug.LogWarning($"[CS_RoomCreatePointRaycast] {go_TargetObject.name} はシーン上のオブジェクトではありません。");
            return null;
        }

        Physics.SyncTransforms();

        Vector3 v3_RayStartPosition = go_TargetObject.transform.position + Vector3.up * 0.05f;
        Ray ray_Down = new Ray(v3_RayStartPosition, Vector3.down);

        RaycastHit[] hits = Physics.RaycastAll(
            ray_Down,
            F_RAY_DISTANCE,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide
        );

        if (hits == null || hits.Length <= 0)
        {
            Debug.LogWarning($"[CS_RoomCreatePointRaycast] {go_TargetObject.name} の下方向にColliderが見つかりませんでした。");
            return null;
        }

        Array.Sort(hits, (hit_A, hit_B) => hit_A.distance.CompareTo(hit_B.distance));

        foreach (RaycastHit hit in hits)
        {
            GameObject go_HitObject = hit.collider.gameObject;

            if (go_HitObject.scene != scene_Target)
            {
                continue;
            }

            if (!go_HitObject.CompareTag(STR_PLANE_TAG_NAME))
            {
                continue;
            }

            Transform tf_RoomCreatePoint = FindRoomCreatePointParent(hit.collider.transform);

            if (tf_RoomCreatePoint == null)
            {
                Debug.LogWarning($"[CS_RoomCreatePointRaycast] Planeタグには当たりましたが、親階層にRoomCreatePointがありません。Hit : {go_HitObject.name}");
                return null;
            }

            return tf_RoomCreatePoint.gameObject;
        }

        Debug.LogWarning($"[CS_RoomCreatePointRaycast] {go_TargetObject.name} の下方向にPlaneタグのColliderが見つかりませんでした。");
        return null;
    }

    /// <summary>
    /// 指定Transformから親階層をたどり、RoomCreatePointを探します。
    /// </summary>
    /// <param name="tf_Start">探索開始Transformです。</param>
    /// <returns>RoomCreatePointが見つかった場合はTransform、見つからない場合はnullを返します。</returns>
    private static Transform FindRoomCreatePointParent(Transform tf_Start)
    {
        Transform tf_Current = tf_Start;

        while (tf_Current != null)
        {
            if (IsRoomCreatePointName(tf_Current.name))
            {
                return tf_Current;
            }

            tf_Current = tf_Current.parent;
        }

        return null;
    }

    /// <summary>
    /// オブジェクト名がRoomCreatePoint系の名前か確認します。
    /// RoomCreatePointsという親管理用オブジェクトを誤取得しないようにします。
    /// </summary>
    /// <param name="str_ObjectName">確認するオブジェクト名です。</param>
    /// <returns>RoomCreatePoint名の場合はtrueを返します。</returns>
    private static bool IsRoomCreatePointName(string str_ObjectName)
    {
        if (string.IsNullOrEmpty(str_ObjectName))
        {
            return false;
        }

        if (!str_ObjectName.StartsWith(STR_ROOM_CREATE_POINT_PREFIX, StringComparison.Ordinal))
        {
            return false;
        }

        if (str_ObjectName.Length == STR_ROOM_CREATE_POINT_PREFIX.Length)
        {
            return true;
        }

        char char_Next = str_ObjectName[STR_ROOM_CREATE_POINT_PREFIX.Length];
        return char.IsDigit(char_Next);
    }
}
