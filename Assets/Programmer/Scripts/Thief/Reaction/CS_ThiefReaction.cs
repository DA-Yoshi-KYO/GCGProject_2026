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

    private void Start()
    {
        // SpriteRendererを取得
        reactionSpriteRenderer = GetComponent<SpriteRenderer>();
        if (reactionSpriteRenderer == null)
        {
            Debug.LogError("SpriteRendererが見つかりませんでした。");
        }
    }

    private void Update()
    {
    }

    /// <summary>
    /// リアクションの種類に応じてスプライトを変更するメソッド
    /// </summary>
    /// <param name="reactionType">変更するリアクションの種類</param>
    public void ChangeReaction(ThiefReactionType reactionType)
    {
        if (reactionSpriteRenderer == null)
        {
            Debug.LogError("SpriteRendererが見つかりませんでした。");
            return;
        }
        // リアクションの種類に応じてスプライトを変更
        reactionSpriteRenderer.sprite = reactionSprites[(int)reactionType];
    }

    /// <summary>
    /// リアクションをクリアするメソッド
    /// </summary>
    public void ClearReaction()
    {
        if (reactionSpriteRenderer == null)
        {
            Debug.LogError("SpriteRendererが見つかりませんでした。");
            return;
        }
        // スプライトをクリア
        reactionSpriteRenderer.sprite = null;
    }
}
