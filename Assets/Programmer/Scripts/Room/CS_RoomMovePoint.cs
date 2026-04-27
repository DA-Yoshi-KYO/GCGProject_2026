using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomMovePoint.cs
 *  制作者      : 吉本竜
 *  内容        : 生成後に自動接続されるRoom移動ポイント
 *  履歴        : 2026/04/27 新規作成(ヨシモト)
 *==================================================*/

/// <summary>
/// Room内の出入口ポイントです。
/// 実際の移動先はRoom生成後にRoomManagerから自動設定されます。
/// </summary>
[DisallowMultipleComponent]
public class CS_RoomMovePoint : MonoBehaviour
{
    [Header("このRoomMovePointの方向")]
    [SerializeField]
    private CSE_RoomDoorDirection e_MoveDirection = CSE_RoomDoorDirection.Right;

    [Header("プレイヤー出現位置")]
    [SerializeField]
    private Transform tf_PlayerSpawnPoint;

    [Header("接続ありの時だけ表示するオブジェクト")]
    [SerializeField]
    private GameObject go_OpenDoorObject;

    [Header("接続なしの時だけ表示するオブジェクト")]
    [SerializeField]
    private GameObject go_ClosedWallObject;

    [Header("プレイヤータグ")]
    [SerializeField]
    private string str_PlayerTag = "Player";

    [Header("連続ワープ防止時間")]
    [SerializeField]
    private float f_MoveCoolTime = 0.25f;

    private static float s_LastMoveTime = -999.0f;

    private CS_RoomMovePoint cs_TargetMovePoint;
    private Collider[] colliders;

    /// <summary>
    /// このRoomMovePointの方向を取得します。
    /// </summary>
    public CSE_RoomDoorDirection MoveDirection => e_MoveDirection;

    /// <summary>
    /// 初期化します。
    /// </summary>
    private void Awake()
    {
        CacheColliders();

        if (cs_TargetMovePoint == null)
        {
            ApplyUsableState(false);
        }
    }

    /// <summary>
    /// 移動先RoomMovePointを設定します。
    /// </summary>
    /// <param name="cs_Target">移動先RoomMovePoint。</param>
    public void SetTargetMovePoint(CS_RoomMovePoint cs_Target)
    {
        cs_TargetMovePoint = cs_Target;
        ApplyUsableState(cs_TargetMovePoint != null);
    }

    /// <summary>
    /// 移動先を解除します。
    /// </summary>
    public void ClearTarget()
    {
        cs_TargetMovePoint = null;
        ApplyUsableState(false);
    }

    /// <summary>
    /// プレイヤー出現用Transformを取得します。
    /// </summary>
    /// <returns>出現位置用Transform。</returns>
    public Transform GetSpawnTransform()
    {
        return tf_PlayerSpawnPoint != null ? tf_PlayerSpawnPoint : transform;
    }

    /// <summary>
    /// Triggerに入ったオブジェクトを確認します。
    /// </summary>
    /// <param name="other">Triggerに入ったCollider。</param>
    private void OnTriggerEnter(Collider other)
    {
        if (cs_TargetMovePoint == null)
        {
            return;
        }

        if (!other.CompareTag(str_PlayerTag))
        {
            return;
        }

        if (Time.time - s_LastMoveTime < f_MoveCoolTime)
        {
            return;
        }

        s_LastMoveTime = Time.time;

        MovePlayer(other, cs_TargetMovePoint.GetSpawnTransform());
    }

    /// <summary>
    /// プレイヤーを指定位置へ移動します。
    /// </summary>
    /// <param name="playerCollider">プレイヤーのCollider。</param>
    /// <param name="targetTransform">移動先Transform。</param>
    private void MovePlayer(Collider playerCollider, Transform targetTransform)
    {
        Transform playerTransform = playerCollider.attachedRigidbody != null
            ? playerCollider.attachedRigidbody.transform
            : playerCollider.transform;

        CharacterController characterController = playerTransform.GetComponent<CharacterController>();

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        playerTransform.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);

        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }

    /// <summary>
    /// Colliderをキャッシュします。
    /// </summary>
    private void CacheColliders()
    {
        if (colliders != null && colliders.Length > 0)
        {
            return;
        }

        colliders = GetComponentsInChildren<Collider>(true);
    }

    /// <summary>
    /// 使用可能状態を反映します。
    /// </summary>
    /// <param name="isUsable">使用可能かどうか。</param>
    private void ApplyUsableState(bool isUsable)
    {
        SetColliderEnabled(isUsable);

        if (go_OpenDoorObject != null)
        {
            go_OpenDoorObject.SetActive(isUsable);
        }

        if (go_ClosedWallObject != null)
        {
            go_ClosedWallObject.SetActive(!isUsable);
        }
    }

    /// <summary>
    /// Colliderの有効状態を切り替えます。
    /// </summary>
    /// <param name="isEnabled">有効にするかどうか。</param>
    private void SetColliderEnabled(bool isEnabled)
    {
        CacheColliders();

        for (int i = 0 ; i < colliders.Length ; i++)
        {
            if (colliders[i] == null)
            {
                continue;
            }

            colliders[i].isTrigger = true;
            colliders[i].enabled = isEnabled;
        }
    }
}
