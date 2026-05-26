/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    危険地帯（DangerZone）生成＆泥棒登録ユーティリティ
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-21 | 初回作成
 * 2026-05-22 | ファイル名を変更（DangerZoneSpawner.cs → CS_DangerZoneSpawner.cs）
 *            | クラス名を変更（DangerZoneSpawner → CS_DangerZoneSpawner）
 * 
 */
using UnityEngine;

/// <summary>
/// 危険地帯（DangerZone）生成＆泥棒登録ユーティリティクラス。
/// </summary>
public static class CS_DangerZoneSpawner
{
    /// <summary>
    /// DangerZone を生成し、範囲内の泥棒（ThiefAI）へ回避対象として zoneID を登録する。
    /// </summary>
    /// <param name="dangerZonePrefab">DangerZone コンポーネント付き prefab</param>
    /// <param name="spawnPosition">生成位置</param>
    /// <param name="registerRadius">泥棒登録に使う半径（視認の代替）</param>
    /// <param name="thiefLayer">泥棒のLayer（未指定なら全レイヤー）</param>
    public static CS_DangerZone SpawnAndRegister(CS_DangerZone dangerZone, Vector3 spawnPosition, float registerRadius, LayerMask thiefLayer)
    {
        if (dangerZone == null)
        {
            Debug.LogWarning("DangerZoneSpawner: dangerZoneP が nullです。");
            return null;
        }

        // DangerZone を生成
        CS_DangerZone zone = Object.Instantiate(dangerZone, spawnPosition, Quaternion.identity);
        if (zone == null) return null;

        // 範囲内泥棒を登録
        RegisterThievesInRange(zone, spawnPosition, registerRadius, thiefLayer);

        return zone;
    }

    /// <summary>
    /// 指定範囲内の泥棒（ThiefAI）を探索し、DangerZone の zoneID を回避対象として登録する。
    /// </summary>  
    /// <param name="zone">登録する DangerZone</param>
    /// <param name="center">探索の中心位置</param>
    /// <param name="radius">探索半径</param>
    /// <param name="thiefLayer">泥棒のLayer （未指定なら全レイヤー）</param>
    private static void RegisterThievesInRange(CS_DangerZone zone, Vector3 center, float radius, LayerMask thiefLayer)
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
            CS_ThiefAI thief = col.GetComponentInParent<CS_ThiefAI>();
            if (thief == null) continue;

            thief.AddAvoidZoneID(zone.ZoneID);
        }
    }

    /// <summary>
    /// GimmickBase の効果範囲を「一定距離」として扱い、DangerZone生成＋登録を行う。
    /// </summary>
    public static CS_DangerZone SpawnAndRegisterFromGimmick(
        CS_DangerZone dangerZone,
        Vector3 spawnPosition,
        GimmickBase gimmick,
        CO_ThiefCommonStatusData thiefCommon,
        LayerMask thiefLayer)
    {
        if (gimmick == null)
        {
            Debug.LogWarning("DangerZoneSpawner: gimmick が nullです。");
            return null;
        }

        float registerRadius = CalculateRegisterRadiusFromEffectRange(gimmick);

        CS_DangerZone zone = SpawnAndRegister(dangerZone, spawnPosition, registerRadius, thiefLayer);
        if (zone == null) return null;

        // 残存時間は共通データで上書き
        if (thiefCommon != null)
        {
            zone.Initialize(gimmick.gimmickSizeX, zone.ZoneID, thiefCommon.dangerZoneDuration);
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
