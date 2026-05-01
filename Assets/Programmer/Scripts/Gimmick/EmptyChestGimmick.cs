using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class EmptyChestGimmick : GimmickBase
{
    [Header("耐久値")]
    [Tooltip("この値が0になると壊れる"), Min(0)]
    public float durability = 50f;

    protected override void IdleUpdate()
    {
        SetHitChecker(gimmickGridPos.x, gimmickGridPos.y);

        if(durability <= 0)
        {
            gimmickState = GimmickState.Broken;
        }
    }

    protected override void BrokenUpdate()
    {
        DeleteHitChecker();
        Destroy(gameObject);
    }

    /// <summary>
    /// 呼び出しごとに耐久値を減らす関数
    /// </summary>
    public void Durability_Value_Decreased()
    {
        durability -= Time.deltaTime;
    }

}
