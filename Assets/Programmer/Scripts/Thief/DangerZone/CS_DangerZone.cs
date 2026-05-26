/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    危険地帯クラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-21 | 初回作成
 * 2026-05-22 | ファイル名を変更（DangerZone.cs → CS_DangerZone.cs）
 *            | クラス名を変更（DangerZone → CS_DangerZone）
 * 
 */
using UnityEngine;

/// <summary>
/// 危険地帯クラス
/// </summary>
public sealed class CS_DangerZone : MonoBehaviour
{
    [SerializeField, Tooltip("危険エリア半径")]
    private float radius = 5f;

    [SerializeField, Tooltip("危険エリアID (例:0=TrapA,1=TrapB)")]
    private int zoneID;

    [SerializeField, Min(0f), Tooltip("生存時間(秒)。0以下で無期限")]
    private float duration = 5f;

    public float Radius => radius;
    public int ZoneID => zoneID;
    public float Duration => duration;

    public Vector3 Position => transform.position;

    [Tooltip("残り時間")]
    private float remaining;

    /// <summary>
    /// DangerZoneManager に登録。生存時間がある場合は残り時間を初期化。
    /// </summary>
    private void OnEnable()
    {
        if (CS_DangerZoneManager.Instance != null)
        {
            CS_DangerZoneManager.Instance.Register(this);
        }

        remaining = duration;
    }

    /// <summary>
    /// DangerZoneManager から登録解除。
    /// </summary>
    private void OnDisable()
    {
        if (CS_DangerZoneManager.Instance != null)
        {
            CS_DangerZoneManager.Instance.Unregister(this);
        }
    }

    /// <summary>
    /// 生存時間がある場合は残り時間を減算し、0以下になったら自動で破棄。
    /// </summary>
    private void Update()
    {
        if (duration <= 0f) return;

        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            CS_ThiefManager thiefManager = GameObject.FindObjectOfType<CS_ThiefManager>();
            if (thiefManager != null)
            {
                thiefManager.EraseTheAvoidZoneIDToAllThief(zoneID);
            }

            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 指定位置が危険範囲内か。
    /// </summary>
    public bool IsInside(Vector3 position)
    {
        // Vector3.Distance は sqrt を含むため、sqrMagnitudeで軽量化。
        float sqr = (Position - position).sqrMagnitude;
        return sqr <= radius * radius;
    }

    /// <summary>
    /// 生成直後にパラメータを上書きしたい場合に使用。
    /// </summary>
    public void Initialize(float radius, int zoneID, float duration)
    {
        if (radius > 0f) this.radius = radius;
        this.zoneID = zoneID;
        if (duration >= 0f) this.duration = duration;

        // 既に有期限運用なら残り時間も同期
        remaining = this.duration;
    }

    /// <summary>
    /// Gizmosで危険エリアを可視化。選択中でなくても表示。
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
