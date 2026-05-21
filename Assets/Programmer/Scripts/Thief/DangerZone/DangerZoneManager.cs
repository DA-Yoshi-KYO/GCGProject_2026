/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    危険地帯マネージャ
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-21 | 初回作成
 * 
 */
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シーン内の <see cref="DangerZone"/> を管理するマネージャ。
/// </summary>
public sealed class DangerZoneManager : MonoBehaviour
{
    private static DangerZoneManager instance;
    public static DangerZoneManager Instance => instance;

    // 参照の生存を優先し、毎フレームGCが出ないよう List を使う。
    private readonly List<DangerZone> zones = new List<DangerZone>(64);

    /// <summary>現在有効な DangerZone のスナップショット参照。</summary>
    public IReadOnlyList<DangerZone> Zones => zones;

    private void Awake()
    {
        // シーンに複数置かれた場合は先勝ち。
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public void Register(DangerZone zone)
    {
        if (zone == null) return;
        if (zones.Contains(zone)) return;
        zones.Add(zone);
    }

    public void Unregister(DangerZone zone)
    {
        if (zone == null) return;
        zones.Remove(zone);
    }
}
