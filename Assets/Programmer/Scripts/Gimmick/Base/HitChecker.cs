// == HitChecker.cs ==
// 作者 :秋野翔太
// 更新 :2026/04/24 作成開始
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitChecker : MonoBehaviour
{
    [Header("命中範囲")]
    public BoxCollider Hit;
    [Header("効果範囲")]
    public BoxCollider Effect;

    private bool isHit = false;
    private bool isEffect = false;
    private bool isLoop = false;

    /// <summary>
    /// 当たり判定の処理をループさせるかどうか
    /// </summary>
    /// <param name="IsLoop">ループさせるかどうか</param>
    public void HitLoop(bool IsLoop)
    {
        isLoop = IsLoop;
    }


}
