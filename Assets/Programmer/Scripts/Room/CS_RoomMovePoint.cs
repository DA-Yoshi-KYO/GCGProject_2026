using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*==================================================
 *  ファイル名  : CS_RoomMovePoint.cs
 *  制作者      : 吉本竜
 *  内容        : 生成後に自動接続されるRoom移動ポイント
 *  履歴        : 2026/04/27 接続先をEditor生成後も保持するよう修正(ヨシモト)
 *                2026/05/03 Player移動後のRoom更新をRaycastではなく直接設定へ変更(ヨシモト)
 *                2026/05/03 PlayerData.currentRoomDataを保証してからPlayerCameraを更新する処理を追加(ヨシモト)
 *==================================================*/

/// <summary>
/// Room内の出入口ポイントです。
/// 実際の移動先はRoom生成後にGeneratorから自動設定されます。
/// </summary>
[DisallowMultipleComponent]
public class CS_RoomMovePoint : MonoBehaviour
{
    private const string ROOM_CREATE_POINT_TAG = "RoomCreatePoint";
    private const string PLAYER_TAG = "Player";
    private const string THIEF_TAG = "Thief";

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

    [Header("移動可能タグ")]
    [SerializeField]
    private List<string> list_MoveTargetTags = new List<string>
    {
        PLAYER_TAG,
        THIEF_TAG
    };

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

        if (!TryGetMoveTargetTransform(other, out Transform moveTargetTransform))
        {
            Debug.LogWarning("[RoomMovePoint] 移動対象ではありません : " + other.name);
            return;
        }

        if (Time.time - s_LastMoveTime < f_MoveCoolTime)
        {
            return;
        }

        s_LastMoveTime = Time.time;

        if (IsTagName(moveTargetTransform.gameObject, PLAYER_TAG))
        {
            MovePlayer(moveTargetTransform, cs_TargetMovePoint.GetSpawnTransform());
            return;
        }

        if (IsTagName(moveTargetTransform.gameObject, THIEF_TAG))
        {
            MoveThief(moveTargetTransform, cs_TargetMovePoint.GetSpawnTransform());
            return;
        }

        Debug.LogWarning(
            "[RoomMovePoint] 移動対象タグですが、処理が未対応です : "
            + moveTargetTransform.name
            + " / Tag : "
            + moveTargetTransform.tag
        );
    }

    /// <summary>
    /// Colliderから移動対象Transformを取得します。
    /// </summary>
    /// <param name="other">Triggerに入ったCollider。</param>
    /// <param name="moveTargetTransform">取得した移動対象Transform。</param>
    /// <returns>移動対象だった場合はtrue。</returns>
    private bool TryGetMoveTargetTransform(Collider other, out Transform moveTargetTransform)
    {
        if (other == null)
        {
            moveTargetTransform = null;
            return false;
        }

        if (IsMoveTargetTag(other.gameObject))
        {
            moveTargetTransform = other.attachedRigidbody != null
                ? other.attachedRigidbody.transform
                : other.transform;

            return true;
        }

        if (other.attachedRigidbody != null && IsMoveTargetTag(other.attachedRigidbody.gameObject))
        {
            moveTargetTransform = other.attachedRigidbody.transform;
            return true;
        }

        Transform rootTransform = other.transform.root;

        if (rootTransform != null && IsMoveTargetTag(rootTransform.gameObject))
        {
            moveTargetTransform = rootTransform;
            return true;
        }

        moveTargetTransform = null;
        return false;
    }

    /// <summary>
    /// 指定GameObjectが移動対象タグを持っているか確認します。
    /// </summary>
    /// <param name="targetObject">確認対象GameObject。</param>
    /// <returns>移動対象タグを持っている場合はtrue。</returns>
    private bool IsMoveTargetTag(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return false;
        }

        if (list_MoveTargetTags == null || list_MoveTargetTags.Count <= 0)
        {
            return false;
        }

        for (int i = 0 ; i < list_MoveTargetTags.Count ; i++)
        {
            if (string.IsNullOrWhiteSpace(list_MoveTargetTags[i]))
            {
                continue;
            }

            if (IsTagName(targetObject, list_MoveTargetTags[i]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// CompareTagを使わずにTag Nameを確認します。
    /// 未定義Tag Nameでも例外を出さないためです。
    /// </summary>
    /// <param name="targetObject">確認対象GameObject。</param>
    /// <param name="tagName">確認するTag Name。</param>
    /// <returns>Tag Nameが一致する場合はtrue。</returns>
    private bool IsTagName(GameObject targetObject, string tagName)
    {
        if (targetObject == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(tagName))
        {
            return false;
        }

        return targetObject.tag == tagName;
    }

    /// <summary>
    /// プレイヤーを指定位置へ移動します。
    /// </summary>
    /// <param name="playerTransform">プレイヤーTransform。</param>
    /// <param name="targetTransform">移動先Transform。</param>
    private void MovePlayer(Transform playerTransform, Transform targetTransform)
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("[RoomMovePoint] PlayerTransformがnullです。");
            return;
        }

        if (targetTransform == null)
        {
            Debug.LogWarning("[RoomMovePoint] 移動先Transformがnullです。");
            return;
        }

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

        UpdatePlayerRoomData(playerTransform);
        UpdatePlayerCamera(playerTransform);
    }

    /// <summary>
    /// Playerが現在いるRoomCreatePointを移動先から直接更新します。
    /// </summary>
    /// <param name="playerTransform">プレイヤーTransform。</param>
    private void UpdatePlayerRoomData(Transform playerTransform)
    {
        if (roomPlayerPosition == null)
        {
            roomPlayerPosition = FindFirstObjectByType<CS_RoomPlayerPosition>();
        }

        if (roomPlayerPosition == null)
        {
            Debug.LogWarning("[RoomMovePoint] CS_RoomPlayerPositionが見つかりません。");
            return;
        }

        if (cs_TargetMovePoint == null)
        {
            Debug.LogWarning("[RoomMovePoint] 移動先RoomMovePointがnullです。");
            return;
        }

        GameObject targetRoomCreatePoint =
            FindParentRoomCreatePoint(cs_TargetMovePoint.transform);

        if (targetRoomCreatePoint == null)
        {
            Debug.LogWarning(
                "[RoomMovePoint] 移動先RoomMovePointの親階層にRoomCreatePointがありません。"
                + " / TargetMovePoint : "
                + cs_TargetMovePoint.name
            );

            return;
        }

        roomPlayerPosition.SetPlayerRoomData(targetRoomCreatePoint);
        SetupPlayerDataCurrentRoom(playerTransform);
    }

    /// <summary>
    /// PlayerData.currentRoomDataにRoom管理クラスを設定します。
    /// </summary>
    /// <param name="playerTransform">プレイヤーTransform。</param>
    private void SetupPlayerDataCurrentRoom(Transform playerTransform)
    {
        if (playerTransform == null)
        {
            return;
        }

        if (roomPlayerPosition == null)
        {
            return;
        }

        PlayerData playerData = playerTransform.GetComponent<PlayerData>();

        if (playerData == null)
        {
            Debug.LogWarning("[RoomMovePoint] PlayerDataがPlayerに付いていません : " + playerTransform.name);
            return;
        }
    }

    /// <summary>
    /// PlayerCameraを移動後の部屋に合わせて更新します。
    /// </summary>
    /// <param name="playerTransform">プレイヤーTransform。</param>
    private void UpdatePlayerCamera(Transform playerTransform)
    {
        if (playerTransform == null)
        {
            return;
        }

        SetupPlayerDataCurrentRoom(playerTransform);

        CS_PlayerCamera playerCamera = playerTransform.GetComponent<CS_PlayerCamera>();

        if (playerCamera == null)
        {
            Debug.LogWarning("[RoomMovePoint] PlayerCameraがPlayerに付いていません : " + playerTransform.name);
            return;
        }

        playerCamera.OnRoomMove();
    }

    /// <summary>
    /// 指定Transformの親階層からRoomCreatePointを探します。
    /// </summary>
    /// <param name="targetTransform">検索開始Transform。</param>
    /// <returns>見つかったRoomCreatePoint。</returns>
    private GameObject FindParentRoomCreatePoint(Transform targetTransform)
    {
        Transform currentTransform = targetTransform;

        while (currentTransform != null)
        {
            CS_RoomCreatePoint roomCreatePoint =
                currentTransform.GetComponent<CS_RoomCreatePoint>();

            if (roomCreatePoint != null)
            {
                return currentTransform.gameObject;
            }

            if (IsTagName(currentTransform.gameObject, ROOM_CREATE_POINT_TAG))
            {
                return currentTransform.gameObject;
            }

            currentTransform = currentTransform.parent;
        }

        return null;
    }

    /// <summary>
    /// 泥棒を指定位置へ移動します。
    /// </summary>
    /// <param name="thiefTransform">泥棒のTransform。</param>
    /// <param name="targetTransform">移動先のTransform。</param>
    private void MoveThief(Transform thiefTransform, Transform targetTransform)
    {
        if (thiefTransform == null)
        {
            Debug.LogWarning("[RoomMovePoint] ThiefTransformがnullです。");
            return;
        }

        if (targetTransform == null)
        {
            Debug.LogWarning("[RoomMovePoint] 移動先Transformがnullです。");
            return;
        }

        ThiefAI thiefAI = thiefTransform.GetComponent<ThiefAI>();

        if (thiefAI != null)
        {
            thiefAI.WarpAction(targetTransform.position, targetTransform.parent.GetComponent<CS_RoomMovePoint>().e_MoveDirection);
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
