/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    気絶した泥棒をターゲットにするためのクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-06-04 | 初回作成
 * 
 */
using UnityEngine;

/// <summary>
/// 気絶した泥棒をターゲットにするためのクラス
/// </summary>
public class CS_StunThiefTarget : CS_ThiefTarget
{
    [Tooltip("生成した危険地帯")]
    private CS_DangerZone dangerZone;
    public CS_DangerZone read_DangerZone => dangerZone;

    [Tooltip("危険地帯のプレハブ"), SerializeField, Header("危険地帯のプレハブ")]
    private CS_DangerZone dangerZonePrefab;

    /// <summary>
    /// 泥棒が気絶(耐久値が0)したときに呼び出されるメソッド。危険地帯の生成や、レイヤーの変更
    /// </summary>
    public void Notify()
    {
        // ThiefCommonDBから残存時間を取得
        CO_ThiefCommonStatusData common = null;
        var thiefManager = GameObject.FindObjectOfType<CS_ThiefManager>();
        if (thiefManager != null) common = thiefManager.GetThiefCommonDB();

        // 危険地帯を生成
        dangerZone = CS_DangerZoneSpawner.Spawn(dangerZonePrefab, transform.position, common);

        // 親のオブジェクトのレイヤーを設定
        transform.gameObject.layer = LayerMask.NameToLayer("VisionTarget");

    }

    private void OnDestroy()
    {


        // 危険地帯を削除
        if (dangerZone != null)
        {
            Object.Destroy(dangerZone.gameObject);
            dangerZone = null;
        }
    }
}
