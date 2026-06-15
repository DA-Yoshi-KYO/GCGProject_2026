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
    [SerializeField, Header("リアクションに使用するスプライトリスト"), Tooltip("リアクションに使用するスプライトリスト")]
    private List<Sprite> reactionSprites = new List<Sprite>();

    [Tooltip("リアクションの種類")]
    public enum ThiefReactionType
    {
        [Tooltip("ネコを追跡中")]
        ChasingCat,
        [Tooltip("ギミックに直接被弾")]
        HitTrap,
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


    private void Start()
    {
        // SpriteRendererを取得
        reactionSpriteRenderer = GetComponent<SpriteRenderer>();
        if (reactionSpriteRenderer == null)
        {
            Debug.LogError("SpriteRendererが見つかりませんでした。");
        }

        memorySystem = transform.parent.GetComponent<CS_ThiefAI>().read_MemorySystem;
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
    /// リアクションの種類に応じてスプライトを変更するメソッド
    /// </summary>
    /// <param name="reactionType">変更するリアクションの種類</param>
    public void ChangeReaction(ThiefReactionType reactionType, float setNotChangeTimer = 0.0f)
    {
        if (notChangeTimer > 0.0f) return;

        if (reactionSpriteRenderer == null)
        {
            Debug.LogError("SpriteRendererが見つかりませんでした。");
            return;
        }
        // リアクションの種類に応じてスプライトを変更
        reactionSpriteRenderer.sprite = reactionSprites[(int)reactionType];

        notChangeTimer = setNotChangeTimer;
    }

    /// <summary>
    /// リアクションをクリアするメソッド
    /// </summary>
    public void ClearReaction()
    {
        if (notChangeTimer > 0.0f) return;

        if (reactionSpriteRenderer == null)
        {
            Debug.LogError("SpriteRendererが見つかりませんでした。");
            return;
        }
        // スプライトをクリア
        reactionSpriteRenderer.sprite = null;
    }

    /// <summary>
    /// 指定したリアクションのスプライトが表示されている場合にのみクリアするメソッド
    /// </summary>
    /// <param name="reactionType">クリアするリアクションの種類</param>
    public void ClearReactionByType(ThiefReactionType reactionType)
    {
        if (reactionSpriteRenderer == null)
        {
            Debug.LogError("SpriteRendererが見つかりませんでした。");
            return;
        }
        // 現在のスプライトが指定されたリアクションのスプライトと一致する場合にクリア
        if (reactionSpriteRenderer.sprite == reactionSprites[(int)reactionType])
        {
            reactionSpriteRenderer.sprite = null;
        }
    }
}
