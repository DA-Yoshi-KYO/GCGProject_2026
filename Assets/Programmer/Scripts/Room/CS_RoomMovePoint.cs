using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*==================================================
 *  ファイル名  : CS_RoomMovePoint.cs
 *  制作者      : 吉本竜
 *  内容        : 生成後に自動接続されるRoom移動ポイント
 *  履歴        : 2026/04/27 接続先をEditor生成後も保持するよう修正(ヨシモト)
 *==================================================*/

/// <summary>
/// Room内の出入口ポイントです。
/// 実際の移動先はRoom生成後にGeneratorから自動設定されます。
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

    [SerializeField, HideInInspector]
    private CS_RoomMovePoint cs_TargetMovePoint;

    private static float s_LastMoveTime = -999.0f;

    private Collider[] colliders;

    private CS_RoomPlayerPosition roomPlayerPosition;


    /// <summary>
    /// このRoomMovePointの方向を取得します。
    /// </summary>
    public CSE_RoomDoorDirection MoveDirection => e_MoveDirection;

    /// <summary>
    /// 接続先があるか取得します。
    /// </summary>
    public bool HasTarget => cs_TargetMovePoint != null;

    /// <summary>
    /// 初期化します。
    /// </summary>
    private void Awake()
    {
        roomPlayerPosition = FindFirstObjectByType<CS_RoomPlayerPosition>();

        CacheColliders();
        ApplyUsableState(cs_TargetMovePoint != null);
    }

    /// <summary>
    /// 有効化時に状態を反映します。
    /// </summary>
    private void OnEnable()
    {
        CacheColliders();
        ApplyUsableState(cs_TargetMovePoint != null);
    }

    /// <summary>
    /// 移動先RoomMovePointを設定します。
    /// </summary>
    /// <param name="cs_Target">移動先RoomMovePoint。</param>
    public void SetTargetMovePoint(CS_RoomMovePoint cs_Target)
    {
        cs_TargetMovePoint = cs_Target;
        ApplyUsableState(cs_TargetMovePoint != null);
        MarkDirty();
    }

    /// <summary>
    /// 移動先を解除します。
    /// </summary>
    public void ClearTarget()
    {
        cs_TargetMovePoint = null;
        ApplyUsableState(false);
        MarkDirty();
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
        Debug.Log("[RoomMovePoint] Triggerに入りました : " + other.name);

        if (cs_TargetMovePoint == null)
        {
            Debug.LogWarning("[RoomMovePoint] 移動先が設定されていません : " + name);
            return;
        }

        if (!TryGetPlayerTransform(other, out Transform playerTransform))
        {
            Debug.LogWarning("[RoomMovePoint] Playerではありません : " + other.name);
            return;
        }

        if (Time.time - s_LastMoveTime < f_MoveCoolTime)
        {
            return;
        }

        s_LastMoveTime = Time.time;

        MovePlayer(playerTransform, cs_TargetMovePoint.GetSpawnTransform());
    }

    /// <summary>
    /// ColliderからプレイヤーTransformを取得します。
    /// </summary>
    /// <param name="other">Triggerに入ったCollider。</param>
    /// <param name="playerTransform">取得したプレイヤーTransform。</param>
    /// <returns>プレイヤーだった場合はtrue。</returns>
    private bool TryGetPlayerTransform(Collider other, out Transform playerTransform)
    {
        if (other.CompareTag(str_PlayerTag))
        {
            playerTransform = other.attachedRigidbody != null
                ? other.attachedRigidbody.transform
                : other.transform;

            return true;
        }

        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(str_PlayerTag))
        {
            playerTransform = other.attachedRigidbody.transform;
            return true;
        }

        Transform rootTransform = other.transform.root;

        if (rootTransform != null && rootTransform.CompareTag(str_PlayerTag))
        {
            playerTransform = rootTransform;
            return true;
        }

        playerTransform = null;
        return false;
    }

    /// <summary>
    /// プレイヤーを指定位置へ移動します。
    /// </summary>
    /// <param name="playerTransform">プレイヤーTransform。</param>
    /// <param name="targetTransform">移動先Transform。</param>
    private void MovePlayer(Transform playerTransform, Transform targetTransform)
    {
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

        if (roomPlayerPosition != null)
        {
            roomPlayerPosition.RefreshPlayerRoomData();
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

    /// <summary>
    /// Editor上で変更を保存対象にします。
    /// </summary>
    private void MarkDirty()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
