using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomCreatePointDoorPositionDebugger.cs
 *  制作者      : 吉本竜
 *  内容        : CS_RoomCreatePointのドア座標取得確認用デバッグ
 *  履歴        : 2026/06/23 新規作成
 *==================================================*/

/// <summary>
/// CS_RoomCreatePointのドア座標が正しく取得できているか確認するデバッグ用クラスです。
/// </summary>
public class CS_RoomCreatePointDoorPositionDebugger : MonoBehaviour
{
    [SerializeField, Header("確認対象のRoomCreatePoint")]
    private CS_RoomCreatePoint cs_TargetRoomCreatePoint;

    [SerializeField, Header("Start時に自動表示するか")]
    private bool bool_IsShowOnStart = true;

    [SerializeField, Header("Sceneビューに球を表示するか")]
    private bool bool_IsDrawGizmos = true;

    [SerializeField, Header("Gizmosの球サイズ")]
    private float float_GizmoSphereSize = 0.35f;

    private readonly CSE_RoomDoorDirection[] e_CheckDirections =
    {
        CSE_RoomDoorDirection.Right,
        CSE_RoomDoorDirection.Left,
        CSE_RoomDoorDirection.Front,
        CSE_RoomDoorDirection.Back,
    };

    private void Reset()
    {
        cs_TargetRoomCreatePoint = GetComponent<CS_RoomCreatePoint>();
    }

    private void Awake()
    {
        if (cs_TargetRoomCreatePoint == null)
        {
            cs_TargetRoomCreatePoint = GetComponent<CS_RoomCreatePoint>();
        }
    }

    private void Start()
    {
        if (bool_IsShowOnStart)
        {
            ShowDoorPositions();
        }
    }

    /// <summary>
    /// Inspectorの右クリックメニューからドア座標を確認します。
    /// </summary>
    [ContextMenu("ドア座標をデバッグ表示")]
    public void ShowDoorPositions()
    {
        if (cs_TargetRoomCreatePoint == null)
        {
            Debug.LogError("[DoorPositionDebug] CS_RoomCreatePoint が設定されていません。", this);
            return;
        }

        Debug.Log($"===== ドア座標確認 : {cs_TargetRoomCreatePoint.name} =====", cs_TargetRoomCreatePoint);

        for (int i = 0 ; i < e_CheckDirections.Length ; i++)
        {
            CSE_RoomDoorDirection e_Direction = e_CheckDirections[i];

            CSE_RoomDoorUsageType e_UsageType =
                cs_TargetRoomCreatePoint.GetDoorUsageType(e_Direction);

            Transform tr_DoorPoint =
                cs_TargetRoomCreatePoint.GetRoomDoorPosition(e_Direction);

            bool bool_HasConnection =
                cs_TargetRoomCreatePoint.TryGetConnection(e_Direction, out CS_RoomMoveConnection cs_Connection);

            int int_EnemyEntryCount =
                cs_TargetRoomCreatePoint.GetEnemyEntryCount(e_Direction);

            string str_DoorName = "未取得";
            string str_DoorPosition = "未取得";
            string str_PositionWarning = "";

            if (tr_DoorPoint != null)
            {
                str_DoorName = tr_DoorPoint.name;
                str_DoorPosition = tr_DoorPoint.position.ToString();

                if (e_UsageType != CSE_RoomDoorUsageType.None && tr_DoorPoint.position == Vector3.zero)
                {
                    str_PositionWarning = "  ※注意：ドア設定ありですが座標が Vector3.zero です";
                }
            }
            else
            {
                if (e_UsageType != CSE_RoomDoorUsageType.None)
                {
                    str_PositionWarning = "  ※注意：ドア設定ありなのにTransformが取得できていません";
                }
            }

            Debug.Log(
                $"[DoorPositionDebug] " +
                $"方向:{e_Direction} / " +
                $"用途:{e_UsageType} / " +
                $"接続あり:{bool_HasConnection} / " +
                $"敵数:{int_EnemyEntryCount} / " +
                $"ドア名:{str_DoorName} / " +
                $"座標:{str_DoorPosition}" +
                str_PositionWarning,
                cs_TargetRoomCreatePoint
            );
        }

        Debug.Log("===== ドア座標確認終了 =====", cs_TargetRoomCreatePoint);
    }

    private void OnDrawGizmos()
    {
        if (!bool_IsDrawGizmos)
        {
            return;
        }

        if (cs_TargetRoomCreatePoint == null)
        {
            cs_TargetRoomCreatePoint = GetComponent<CS_RoomCreatePoint>();
        }

        if (cs_TargetRoomCreatePoint == null)
        {
            return;
        }

        for (int i = 0 ; i < e_CheckDirections.Length ; i++)
        {
            CSE_RoomDoorDirection e_Direction = e_CheckDirections[i];

            CSE_RoomDoorUsageType e_UsageType =
                cs_TargetRoomCreatePoint.GetDoorUsageType(e_Direction);

            if (e_UsageType == CSE_RoomDoorUsageType.None)
            {
                continue;
            }

            Transform tr_DoorPoint =
                cs_TargetRoomCreatePoint.GetRoomDoorPosition(e_Direction);

            if (tr_DoorPoint == null)
            {
                continue;
            }

            Vector3 vec_DoorPosition = tr_DoorPoint.position;

            Gizmos.DrawSphere(vec_DoorPosition, float_GizmoSphereSize);
            Gizmos.DrawLine(cs_TargetRoomCreatePoint.transform.position, vec_DoorPosition);
        }
    }
}
