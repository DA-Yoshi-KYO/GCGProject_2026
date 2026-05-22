/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    危険地帯マネージャ
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-21 | 初回作成
 * 2026-05-22 | ファイル名を変更（DangerZoneManager.cs → CS_DangerZoneManager.cs）
 *            | クラス名を変更（DangerZoneManager → CS_DangerZoneManager）
 * 
 */
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シーン内の <see cref="CS_DangerZone"/> を管理するマネージャ。
/// </summary>
public sealed class CS_DangerZoneManager : MonoBehaviour
{
    private static CS_DangerZoneManager instance;
    public static CS_DangerZoneManager Instance => instance;

    [Tooltip("シーン内の DangerZone を管理するマネージャ。")]
    private readonly List<CS_DangerZone> zones = new List<CS_DangerZone>(64);

    /// <summary>現在有効な DangerZone のスナップショット参照。</summary>
    public IReadOnlyList<CS_DangerZone> Zones => zones;

    /// <summary>
    /// シングルトンインスタンスの初期化。シーンに複数置かれた場合は先勝ち。
    /// </summary>
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

    /// <summary>
    /// シングルトンインスタンスのクリーンアップ。自分がインスタンスであれば null にする。
    /// </summary>
    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    /// <summary>
    /// DangerZone を登録する。すでに登録されている場合は無視する。
    /// </summary>
    /// <param name="zone">登録する危険地帯</param>
    public void Register(CS_DangerZone zone)
    {
        if (zone == null) return;
        if (zones.Contains(zone)) return;
        zones.Add(zone);
    }

    /// <summary>
    /// DangerZone を登録解除する。登録されていない場合は無視する。
    /// </summary>
    /// <param name="zone">解除する危険地帯</param>
    public void Unregister(CS_DangerZone zone)
    {
        if (zone == null) return;
        zones.Remove(zone);
    }
}
