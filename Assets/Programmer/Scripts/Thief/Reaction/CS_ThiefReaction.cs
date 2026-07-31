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
        CS_RoomNode currentRoom = memorySystem.read_CurrentRoom;

        if (currentRoom != prevRoom)
            roomCamera = currentRoom.GetComponentInChildren<Camera>();

        if (roomCamera != null && roomCamera.enabled)
        {
            transform.LookAt(roomCamera.transform.position);
            transform.Rotate(0, 180, 0);
        }
        prevRoom = currentRoom;

        // タイマーの更新
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

        // 以前のリアクションをすべて停止
        ClearReaction();

        CSST_EffectPlayData csst_EffectPlayData =
            new CSST_EffectPlayData();

        csst_EffectPlayData.CSST_EffectPlayData_Init();

        cs_TargetEffectPlayer.PlayEffect(
            csst_EffectPlayData);

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

        notChangeTimer = 0.0f;
    }

    /// <summary>
    /// 指定した種類のリアクションだけを停止します。
    /// </summary>
    public void ClearReactionByType(
        ThiefReactionType reactionType)
    {
        if (!TryGetEffectPlayer(
            reactionType,
            out CS_EffectPlayer cs_TargetEffectPlayer))
        {
            return;
        }

        cs_TargetEffectPlayer.EndCurrentEffect();
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
}
