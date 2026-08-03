/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のリアクションを管理するクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-26 | 初回作成
 * 
 */
using System.Collections.Generic;
using UnityEngine;

public class CS_ThiefReaction : MonoBehaviour
{
    CS_EffectPlayer effectPlayer ;

    [Tooltip("リアクションの種類")]
    public enum ThiefReactionType
    {
        [Tooltip("ネコを追跡中")]
        ChasingCat,
        [Tooltip("ギミックが間近で被弾")]
        NearHitTrap,
        [Tooltip("警戒")]
        Alert,
        [Tooltip("お宝を見つける・お宝を運ぶ")]
        FoundTreasure,
        [Tooltip("物を探索")]
        Searching,
    }

    [Tooltip("リアクションを表示するスプライトレンダラー")]
    private SpriteRenderer reactionSpriteRenderer;

    [Tooltip("泥棒の部屋を取得する為のCS")]
    CS_MemorySystem memorySystem;
    [Tooltip("部屋の変更を検知する為の保存用変数")]
    CS_RoomNode prevRoom;
    [Tooltip("部屋のカメラ")]
    Camera roomCamera;

    [Tooltip("リアクションが変化しない時間を計測するタイマー")]
    public float notChangeTimer = 0.0f;

    [Header("リアクションEffectPrefab")]
    [SerializeField]
    private List<GameObject> list_EffectPrefabs = new List<GameObject>();

    [Header("リアクションEffect位置Offset")]
    [SerializeField]
    private Vector3 v3_EffectPositionOffset =
    new Vector3(0.0f, 0.5f, 0.0f);

    /// <summary>
    /// 現在再生しているリアクションです。
    /// </summary>
    private ThiefReactionType? currentReactionType = null;

    /// <summary>
    /// 現在実際に再生しているEffectです。
    /// </summary>
    private CSAD_EffectCommonProcessBase csad_CurrentReactionEffect;

    /// <summary>
    /// EffectPrefabごとに実行時生成するEffectPlayerです。
    /// </summary>
    private List<CS_EffectPlayer> list_EffectPlayers = new List<CS_EffectPlayer>();

    private void Awake()
    {
        InitializeEffectPlayers();
    }

    private void Start()
    {
        reactionSpriteRenderer = GetComponent<SpriteRenderer>();

        if (reactionSpriteRenderer == null)
        {
            Debug.LogError(
                "SpriteRendererが見つかりませんでした。",
                this);
        }

        CS_ThiefAI cs_ThiefAI =
            transform.parent.GetComponent<CS_ThiefAI>();

        if (cs_ThiefAI != null)
        {
            memorySystem = cs_ThiefAI.read_MemorySystem;
        }

    }

    /// <summary>
    /// 登録されたEffectPrefabごとに、
    /// 専用のCS_EffectPlayerを生成します。
    /// </summary>
    private void InitializeEffectPlayers()
    {
        list_EffectPlayers.Clear();

        if (list_EffectPrefabs == null)
        {
            return;
        }

        for (int i = 0 ; i < list_EffectPrefabs.Count ; i++)
        {
            GameObject go_EffectPrefab =
                list_EffectPrefabs[i];

            if (go_EffectPrefab == null)
            {
                list_EffectPlayers.Add(null);
                continue;
            }

            GameObject go_EffectPlayerObject =
                new GameObject(
                    "EffectPlayer_" +
                    i.ToString("00") +
                    "_" +
                    go_EffectPrefab.name);

            go_EffectPlayerObject.transform.SetParent(
                transform,
                false);

            CS_EffectPlayer cs_EffectPlayer =
                go_EffectPlayerObject.AddComponent<CS_EffectPlayer>();

            cs_EffectPlayer.SetEffectPrefab(
                go_EffectPrefab);

            list_EffectPlayers.Add(
                cs_EffectPlayer);
        }
    }


    private void Update()
    {
        // Effectが自然終了した場合、再生中情報を解除します。
        if (currentReactionType.HasValue &&
            !IsCurrentReactionEffectAlive())
        {
            ClearCurrentReactionReferences();
        }

        if (notChangeTimer > 0.0f)
        {
            notChangeTimer -= Time.deltaTime;

            if (notChangeTimer < 0.0f)
            {
                notChangeTimer = 0.0f;
            }
        }
    }

    /// <summary>
    /// 指定したリアクションEffectを再生します。
    /// </summary>
    public void ChangeReaction(
        ThiefReactionType reactionType,
        float setNotChangeTimer = 0.0f)
    {
        if (list_EffectPlayers == null ||
            list_EffectPlayers.Count != list_EffectPrefabs.Count)
        {
            InitializeEffectPlayers();
        }

        // 同じEffectが本当に再生中の場合だけ無視します。
        if (currentReactionType.HasValue &&
            currentReactionType.Value == reactionType &&
            IsCurrentReactionEffectAlive())
        {
            return;
        }

        // 自然終了・Pool返却済みなら保存情報を解除します。
        if (!IsCurrentReactionEffectAlive())
        {
            ClearCurrentReactionReferences();
        }

        if (notChangeTimer > 0.0f)
        {
            return;
        }

        if (!TryGetEffectPlayer(
            reactionType,
            out CS_EffectPlayer cs_TargetEffectPlayer))
        {
            return;
        }

        ClearReaction();

        CSST_EffectPlayData csst_EffectPlayData =
            new CSST_EffectPlayData();

        csst_EffectPlayData.CSST_EffectPlayData_Init();

        csst_EffectPlayData.SetPosition(
            transform.position +
            v3_EffectPositionOffset);

        csad_CurrentReactionEffect =
            cs_TargetEffectPlayer.PlayEffect(
                csst_EffectPlayData);

        if (csad_CurrentReactionEffect == null)
        {
            Debug.LogWarning(
                "[CS_ThiefReaction] Effectを再生できませんでした。" +
                " Reaction : " + reactionType,
                this);

            ClearCurrentReactionReferences();
            return;
        }

        currentReactionType = reactionType;

        ApplyBillboardTarget(
            csad_CurrentReactionEffect);

        notChangeTimer =
            Mathf.Max(0.0f, setNotChangeTimer);
    }

    /// <summary>
    /// リアクションの種類を問わず、すべて停止します。
    /// </summary>
    public void ClearReaction()
    {
        if (list_EffectPlayers == null)
        {
            return;
        }

        for (int i = 0 ; i < list_EffectPlayers.Count ; i++)
        {
            CS_EffectPlayer cs_EffectPlayer =
                list_EffectPlayers[i];

            if (cs_EffectPlayer == null)
            {
                continue;
            }

            cs_EffectPlayer.EndCurrentEffect();
        }

        ClearCurrentReactionReferences();
        notChangeTimer = 0.0f;
    }

    /// <summary>
    /// 指定した種類のリアクションだけを停止します。
    /// </summary>
    public void ClearReactionByType(
        ThiefReactionType reactionType)
    {
        if (!currentReactionType.HasValue ||
            currentReactionType.Value != reactionType)
        {
            return;
        }

        if (csad_CurrentReactionEffect != null)
        {
            csad_CurrentReactionEffect.EndEffect();
        }

        if (TryGetEffectPlayer(
            reactionType,
            out CS_EffectPlayer cs_TargetEffectPlayer))
        {
            cs_TargetEffectPlayer.EndCurrentEffect();
        }

        ClearCurrentReactionReferences();
        notChangeTimer = 0.0f;
    }

    /// <summary>
    /// リアクションの種類に対応するEffectPlayerを取得します。
    /// </summary>
    private bool TryGetEffectPlayer(
        ThiefReactionType reactionType,
        out CS_EffectPlayer cs_TargetEffectPlayer)
    {
        cs_TargetEffectPlayer = null;

        int n_EffectIndex =
            (int)reactionType;

        if (list_EffectPlayers == null ||
            n_EffectIndex < 0 ||
            n_EffectIndex >= list_EffectPlayers.Count)
        {
            Debug.LogWarning(
                "[CS_ThiefReaction] 対応するEffectPrefabがありません。" +
                " Reaction : " + reactionType +
                " / Index : " + n_EffectIndex,
                this);

            return false;
        }

        cs_TargetEffectPlayer =
            list_EffectPlayers[n_EffectIndex];

        if (cs_TargetEffectPlayer == null)
        {
            Debug.LogWarning(
                "[CS_ThiefReaction] EffectPrefabが設定されていません。" +
                " Reaction : " + reactionType,
                this);

            return false;
        }

        return true;
    }

    private void LateUpdate()
    {
        UpdateReactionBillboard();
    }

    /// <summary>
    /// 現在のEffectが実際に再生中か確認します。
    /// </summary>
    private bool IsCurrentReactionEffectAlive()
    {
        if (csad_CurrentReactionEffect == null)
        {
            return false;
        }

        if (!csad_CurrentReactionEffect.gameObject.activeInHierarchy)
        {
            return false;
        }

        return !csad_CurrentReactionEffect.IsEndFinished();
    }

    /// <summary>
    /// 現在のカメラに向けてBillboardを更新します。
    /// </summary>
    private void UpdateReactionBillboard()
    {
        Camera cam_ActiveCamera =
            CS_BillboardCameraCache.GetActiveMainCamera();

        if (cam_ActiveCamera != null)
        {
            roomCamera = cam_ActiveCamera;
        }
        else if (memorySystem != null)
        {
            CS_RoomNode currentRoom =
                memorySystem.read_CurrentRoom;

            if (currentRoom != null &&
                (currentRoom != prevRoom || roomCamera == null))
            {
                roomCamera =
                    currentRoom.GetComponentInChildren<Camera>(true);

                prevRoom = currentRoom;
            }
        }

        if (roomCamera == null ||
            !roomCamera.isActiveAndEnabled)
        {
            return;
        }

        // SpriteSheet本体にも現在のCameraを渡します。
        ApplyBillboardTarget(
            csad_CurrentReactionEffect);

        Vector3 v3_Direction =
            transform.position -
            roomCamera.transform.position;

        if (v3_Direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        transform.rotation =
            Quaternion.LookRotation(
                v3_Direction.normalized);
    }

    /// <summary>
    /// SpriteSheetへBillboard対象Cameraを設定します。
    /// </summary>
    private void ApplyBillboardTarget(
        CSAD_EffectCommonProcessBase csad_Effect)
    {
        if (csad_Effect == null)
        {
            return;
        }

        CS_EffectSpriteSheet cs_SpriteSheetEffect =
            csad_Effect as CS_EffectSpriteSheet;

        if (cs_SpriteSheetEffect == null)
        {
            return;
        }

        Transform tr_CameraTransform = null;

        if (roomCamera != null &&
            roomCamera.isActiveAndEnabled)
        {
            tr_CameraTransform =
                roomCamera.transform;
        }

        cs_SpriteSheetEffect.SetBillboardTarget(
            tr_CameraTransform);
    }

    /// <summary>
    /// 保存している再生中情報を解除します。
    /// </summary>
    private void ClearCurrentReactionReferences()
    {
        currentReactionType = null;
        csad_CurrentReactionEffect = null;
    }
}
