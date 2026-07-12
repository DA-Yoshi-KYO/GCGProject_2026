/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の危険地帯を考慮して目的地を決める、自己改良型NavMeshAgent
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *     宇留野陸斗
 * ----------------------------------------------------------
 * 2026-05-21 | 初回作成
 * 2026-05-22 | ファイル名を変更（SmartNavAgent.cs → CS_SmartNavAgent.cs）
 *            | クラス名を変更（SmartNavAgent → CS_SmartNavAgent）
 *
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// DangerZone を考慮して目的地を決める NavMeshAgent ラッパ。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public sealed class CS_SmartNavAgent : MonoBehaviour
{
    [SerializeField, Tooltip("使用する NavMeshAgent。未設定の場合は同一GameObjectから取得")]
    private NavMeshAgent agent;

    [SerializeField, Tooltip("このキャラクターが回避対象とする DangerZone の zoneID 一覧")]
    private List<int> avoidZoneIDs = new List<int>();

    [Header("Safe target search")]
    [SerializeField, Min(0.1f), Tooltip("危険がある場合の代替地点探索半径")]
    private float safeSearchRadius = 5f;

    [SerializeField, Min(1), Tooltip("代替地点探索の最大試行回数")]
    private int safeSearchTryCount = 20;

    [SerializeField, Tooltip("NavMesh.SamplePosition の最大探索距離")]
    private float sampleMaxDistance = 2f;

    [Tooltip("経路計算用（GC抑制のため使い回す）")]
    private NavMeshPath reusablePath;

    [Tooltip("現在の標的地点")]
    private Vector3 currentTargetPoint;

    public NavMeshAgent Agent => agent;

    public IReadOnlyList<int> AvoidZoneIDs => avoidZoneIDs;

    /// <summary>
    /// 初期化。NavMeshPath と NavMeshAgent を用意する。
    /// </summary>
    private void Awake()
    {
        if (reusablePath == null)
        {
            reusablePath = new NavMeshPath();
        }

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (agent == null)
        {
            Debug.LogError("SmartNavAgent: NavMeshAgent が見つかりません。", this);
        }
    }

    /// <summary>
    /// キャラクターごとに回避対象を差し替える。
    /// </summary>
    public void SetAvoidZoneIDs(IEnumerable<int> zoneIDs)
    {
        avoidZoneIDs.Clear();
        if (zoneIDs == null) return;

        foreach (var id in zoneIDs)
        {
            if (!avoidZoneIDs.Contains(id)) avoidZoneIDs.Add(id);
        }
    }

    /// <summary>
    /// 現在の標的地点に対して経路を再計算する。
    /// </summary>
    public void RefreshPath()
    {
        MoveTo(currentTargetPoint);
    }

    /// <summary>
    ///目的地へ移動。必要なら危険を避けた代替地点を探す。
    /// </summary>
    public void MoveTo(Vector3 target)
    {
        if (agent == null || !agent.isOnNavMesh) return;
        if (reusablePath == null) reusablePath = new NavMeshPath();

        // 回避対象が無いなら通常移動
        if (avoidZoneIDs == null || avoidZoneIDs.Count == 0)
        {
            agent.SetDestination(target);
            return;
        }

        // DangerZoneManager が無い/ゾーンが無いなら通常移動
        var mgr = CS_DangerZoneManager.Instance;
        if (mgr == null || mgr.Zones == null || mgr.Zones.Count == 0)
        {
            agent.SetDestination(target);
            return;
        }

        // 通常経路を計算
        if (!agent.CalculatePath(target, reusablePath) || reusablePath.status != NavMeshPathStatus.PathComplete)
        {
            // CalculatePath失敗時は SetDestination にフォールバック
            agent.SetDestination(target);
            return;
        }

        // 経路上に回避対象がなければ通常移動
        if (!PathContainsDanger(reusablePath))
        {
            agent.SetDestination(target);
            return;
        }

        // 現在の標的地点を更新
        currentTargetPoint = target;

        // 危険があれば安全地点検索
        Vector3 safe = FindSafePosition(target);
        agent.SetDestination(safe);
    }

    /// <summary>
    /// 経路上に回避対象 DangerZone があるか。
    /// </summary>
    public bool PathContainsDanger(NavMeshPath path)
    {
        if (path == null) return false;
        if (avoidZoneIDs == null || avoidZoneIDs.Count == 0) return false;

        var mgr = CS_DangerZoneManager.Instance;
        if (mgr == null || mgr.Zones == null || mgr.Zones.Count == 0) return false;

        var corners = path.corners;
        if (corners == null || corners.Length == 0) return false;

        // corners の点のみ判定（要件通り）。必要なら将来的に線分交差判定へ拡張可能。
        for (int i = 0 ; i < corners.Length ; i++)
        {
            Vector3 c = corners[i];

            // zones は破棄タイミングで null が混ざる可能性がある
            var zones = mgr.Zones;
            for (int z = 0 ; z < zones.Count ; z++)
            {
                var zone = zones[z];
                if (zone == null) continue;
                if (!avoidZoneIDs.Contains(zone.ZoneID)) continue;

                if (zone.IsInside(c))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 危険を避けた代替地点を探索する。
    /// </summary>
    private Vector3 FindSafePosition(Vector3 target)
    {
        if (agent == null) return target;
        if (reusablePath == null) reusablePath = new NavMeshPath();

        //失敗時は元target
        Vector3 best = target;

        for (int i = 0 ; i < safeSearchTryCount ; i++)
        {
            // target付近をランダム探索
            Vector3 offset = Random.insideUnitSphere * safeSearchRadius;
            offset.y = 0f;

            Vector3 candidate = target + offset;

            // NavMesh 上へスナップ
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(candidate, out hit, sampleMaxDistance, NavMesh.AllAreas))
            {
                continue;
            }

            // 経路を計算して危険がないか確認
            if (!agent.CalculatePath(hit.position, reusablePath) || reusablePath.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            if (!PathContainsDanger(reusablePath))
            {
                best = hit.position;
                break;
            }
        }

        return best;
    }
}
