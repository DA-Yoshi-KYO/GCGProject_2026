/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    危険地帯クラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-21 | 初回作成
 * 
 */
using UnityEngine;

public sealed class DangerZone : MonoBehaviour
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

    private float remaining;

    private void OnEnable()
    {
        if (DangerZoneManager.Instance != null)
        {
            DangerZoneManager.Instance.Register(this);
        }

        remaining = duration;
    }

    private void OnDisable()
    {
        if (DangerZoneManager.Instance != null)
        {
            DangerZoneManager.Instance.Unregister(this);
        }
    }

    private void Update()
    {
        if (duration <= 0f) return;

        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
