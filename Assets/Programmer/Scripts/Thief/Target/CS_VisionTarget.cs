/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    視界検出ターゲットクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-19 | 初回作成
 * 2026-05-07 | 探索度を記憶システムに移動
 * 2026-05-22 | ファイル名を変更（VisionTarget.cs → CS_VisionTarget.cs）
 *            | クラス名を変更（VisionTarget → CS_VisionTarget）
 * 
 */
using UnityEngine;

/// <summary>
/// 視界検出ターゲットクラス
/// </summary>
public class CS_VisionTarget : CS_ThiefTarget
{
    // ターゲットの種類
    public enum TargetType
    {
        [Tooltip("宝物")]
        Treasure,
        [Tooltip("棚")]
        Shelf,
    }

    [Tooltip("ターゲットの種類")]
    public TargetType targetType;

    [Header("探索したときに得る探索度")]
    [Tooltip("探索したときに得る探索度")]
    public int explorationValue;

    [Header("探索済みとする距離")]
    [Tooltip("探索済みとする距離")]
    public float exploredDistanceThreshold;

    [SerializeField, Header("ギズモを表示")]
    private bool showGizmos = false;
    [SerializeField, Header("ギズモの色")]
    private Color gizmoColor = Color.yellow;

    [SerializeField, Header("このオブジェクトを探索している敵")]
    public GameObject searchThief;

    [Tooltip("盗まれて移動中かどうか")]
    private bool isStolenMoveing = false;
    public bool read_IsStolenMoveing => isStolenMoveing;

    [Tooltip("出口までの距離")]
    private int exitDistance = -1;
    public int read_ExitDistance => exitDistance;

    [Tooltip("盗んで移動中の敵のAI")]
    private CS_ThiefAI thiefAI;


    private void Start()
    {
        // ターゲットの種類が宝物の場合
        if (targetType == TargetType.Treasure)
        {
            // EndManagerを取得
            CS_EndManager endManager = GameObject.FindObjectOfType<CS_EndManager>();
            // EndManagerが存在しない場合は新たに作成
            if (endManager == null)
            {
                GameObject endManagerObj = new GameObject("EndManager");
                endManager = endManagerObj.AddComponent<CS_EndManager>();

                endManager = GameObject.Instantiate(endManager);
            }

            // EndManagerに宝物を追加
            endManager.AddTreasure(this);
        }
    }

    private void Update()
    {
        // 盗まれて移動中の場合、出口までの距離を計算
        if (isStolenMoveing)
        {
            exitDistance = thiefAI.read_AStarSystem.GetRouteDistance();
        }
    }

    /// <summary>
    /// 盗まれて移動中の敵のAIを設定
    /// </summary>
    /// <param name="thiefAI">盗まれて移動中の敵のAI</param>
    public void PlayStolen(CS_ThiefAI thiefAI)
    {
        this.thiefAI = thiefAI;
        isStolenMoveing = true;
    }

    public void StopStolen()
    {
        isStolenMoveing = false;
        thiefAI = null;
    }

    private void OnDestroy()
    {
        // ターゲットの種類が宝物の場合、EndManagerから宝物を減らす
        if (targetType == TargetType.Treasure)
        {
            // EndManagerを取得
            CS_EndManager endManager = GameObject.FindObjectOfType<CS_EndManager>();
            if (endManager == null) return;
            // EndManagerから宝物を減らす
            endManager.StolenTreasure(this);
        }
    }


    /// <summary>
    /// ギズモの表示
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // 探索済みとする距離をギズモで表示
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, exploredDistanceThreshold);
    }
}
