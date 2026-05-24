/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    罠のターゲットクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-24 | 初回作成
 * 2026-05-22 | ファイル名を変更（TrapTarget.cs → CS_TrapTarget.cs）
 *            | クラス名を変更（TrapTarget → CS_TrapTarget）
 * 
 */
using UnityEngine;

/// <summary>
/// 罠のターゲットクラス
/// </summary>
public class CS_TrapTarget : CS_ThiefTarget
{
    [Header("警戒度")]
    [Tooltip("泥棒のこの罠に対する警戒度"), Range(0, 100)]
    public int alertValue;

    [Header("このギミックのScript")]
    [Tooltip("この罠のギミックのScript")]
    public GimmickBase gimmickScript;

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Awake()
    {
        if (gimmickScript == null)
        {
            gimmickScript = GetComponent<GimmickBase>();
        }
    }

}
