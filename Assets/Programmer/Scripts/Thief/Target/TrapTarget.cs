/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    罠のターゲットクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-24 | 初回作成
 * 
 */
using UnityEngine;

// 罠のターゲットクラス
public class TrapTarget : ThiefTarget
{
    [Header("警戒度")]
    [Tooltip("泥棒のこの罠に対する警戒度"), Range(0, 100)]
    public int alertValue;

    [Header("このギミックのScript")]
    [Tooltip("この罠のギミックのScript")]
    public GimmickBase gimmickScript;

    private void Awake()
    {
        if (gimmickScript == null)
        {
            gimmickScript = GetComponent<GimmickBase>();
        }
    }

}
