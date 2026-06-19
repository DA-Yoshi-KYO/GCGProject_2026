using System.Collections.Generic;
using UnityEngine;

public class EmptyChestGimmick : GimmickBase
{
    [Header("耐久値")]
    [Tooltip("この値が0になると壊れる"), Min(0)]
    public float durability = 50f;

    [Tooltip("対象にしている泥棒のリスト")]
    private List<CS_ThiefAI> targetThiefAIList = new List<CS_ThiefAI>();

    protected override void IdleUpdate()
    {
        SetHitChecker(transform.position);

        if(durability <= 0)
        {
            gimmickState = GimmickState.Broken;
        }
    }

    protected override void ActiveUpdate()
    {
        gimmickState = GimmickState.Idle;
    }

    protected override void SearchUpdate()
    {
            gimmickState = GimmickState.Idle;
    }

    protected override void BrokenUpdate()
    {
        foreach (var thiefAI in targetThiefAIList)
        {
            thiefAI.read_ThiefGimmickAction.EmptyChestEnd(this);
        }
        targetThiefAIList.Clear();

        base.BrokenUpdate();

        DeleteHitChecker();
    }

    /// <summary>
    /// 呼び出しごとに耐久値を減らす関数
    /// </summary>
    public void Durability_Value_Decreased()
    {
        durability -= Time.deltaTime;
    }

    /// <summary>
    /// 対象にしている泥棒のリストに泥棒を追加する関数
    /// </summary>
    /// <param name="thiefAI">追加する泥棒のAI</param>
    public void AddTargetThiefAI(CS_ThiefAI thiefAI)
    {
        if (!targetThiefAIList.Contains(thiefAI))
        {
            targetThiefAIList.Add(thiefAI);
        }
    }
}
