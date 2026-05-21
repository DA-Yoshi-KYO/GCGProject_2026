/*＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 * DangerZoneSpawner
 *＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *目的：
 * -罠発動時に DangerZone を生成する処理を共通化する。
 * - 「一定距離内にいた泥棒」を取得し、その泥棒へ回避対象 zoneID を登録する。
 *
 *備考：
 * - VisionSensor を使わない簡易方式（範囲内=見ていたの代替）。
 * -生成する DangerZone の zoneID/radius/duration は prefab 側の設定を利用する。
 */
using UnityEngine;

public static class DangerZoneSpawner
{
    /// <summary>
    /// DangerZone を生成し、範囲内の泥棒（ThiefAI）へ回避対象として zoneID を登録する。
    /// </summary>
    /// <param name="dangerZonePrefab">DangerZone コンポーネント付き prefab</param>
    /// <param name="spawnPosition">生成位置</param>
    /// <param name="registerRadius">泥棒登録に使う半径（視認の代替）</param>
    /// <param name="thiefLayer">泥棒のLayer（未指定なら全レイヤー）</param>
    public static DangerZone SpawnAndRegister(DangerZone dangerZonePrefab, Vector3 spawnPosition, float registerRadius, LayerMask thiefLayer)
    {
        if (dangerZonePrefab == null)
        {
            Debug.LogWarning("DangerZoneSpawner: dangerZonePrefab が nullです。");
            return null;
        }

        // DangerZone を生成
        DangerZone zone = Object.Instantiate(dangerZonePrefab, spawnPosition, Quaternion.identity);
        if (zone == null) return null;

        // 範囲内泥棒を登録
        RegisterThievesInRange(zone, spawnPosition, registerRadius, thiefLayer);

        return zone;
    }

    private static void RegisterThievesInRange(DangerZone zone, Vector3 center, float radius, LayerMask thiefLayer)
    {
        if (zone == null) return;
        if (radius <= 0f) return;

        int layerMask = thiefLayer.value != 0 ? thiefLayer.value : ~0;

        //物理探索（GCを抑えたい場合は OverlapSphereNonAlloc に差し替え可能）
        Collider[] hits = Physics.OverlapSphere(center, radius, layerMask);
        if (hits == null || hits.Length == 0) return;

        for (int i = 0 ; i < hits.Length ; i++)
        {
            var col = hits[i];
            if (col == null) continue;

            // 子に付いている可能性を考慮
            ThiefAI thief = col.GetComponentInParent<ThiefAI>();
            if (thief == null) continue;

            thief.AddAvoidZoneID(zone.ZoneID);
        }
    }

    /// <summary>
    /// GimmickBase の効果範囲を「一定距離」として扱い、DangerZone生成＋登録を行う。
    /// </summary>
    public static DangerZone SpawnAndRegisterFromGimmick(
        DangerZone dangerZonePrefab,
        Vector3 spawnPosition,
        GimmickBase gimmick,
        CSS_ThiefCommonStatusData thiefCommon,
        LayerMask thiefLayer)
    {
        if (gimmick == null)
        {
            Debug.LogWarning("DangerZoneSpawner: gimmick が nullです。");
            return null;
        }

        float registerRadius = CalculateRegisterRadiusFromEffectRange(gimmick);

        DangerZone zone = SpawnAndRegister(dangerZonePrefab, spawnPosition, registerRadius, thiefLayer);
        if (zone == null) return null;

        // 残存時間は共通データで上書き
        if (thiefCommon != null)
        {
            zone.Initialize(zone.Radius, zone.ZoneID, thiefCommon.dangerZoneDuration);
        }

        return zone;
    }

    /// <summary>
    /// 効果範囲(effectRangeX/Z)から登録半径を作る。
    /// 仕様：効果範囲ボックスを包含する円の半径（XZ平面）
    /// </summary>
    private static float CalculateRegisterRadiusFromEffectRange(GimmickBase gimmick)
    {
        if (gimmick == null) return 0f;

        // effectRange は「マス数」想定なので、gridSize を掛けてワールド長にする
        float sizeX = 0f;
        float sizeZ = 0f;
        if (gimmick.roomGrid != null)
        {
            sizeX = gimmick.effectRangeX * gimmick.roomGrid.gridSize.x;
            sizeZ = gimmick.effectRangeZ * gimmick.roomGrid.gridSize.y;
        }
        else
        {
            // roomGrid が無い場合は effectRange をそのまま距離扱い（フォールバック）
            sizeX = gimmick.effectRangeX;
            sizeZ = gimmick.effectRangeZ;
        }

        // Box(XZ) を包含する円：半径 = 対角線/2
        float diag = Mathf.Sqrt(sizeX * sizeX + sizeZ * sizeZ);
        return diag * 0.5f;
    }
}
