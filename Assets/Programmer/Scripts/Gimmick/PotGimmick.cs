using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotGimmick : GimmickBase
{
    protected override void IdleUpdate()
    {
    }

    protected override void ActiveUpdate()
    {
        SetHitChecker(gimmickGridPos.x, gimmickGridPos.y);
    }

    protected override void BrokenUpdate()
    {
        DeleteHitChecker();
    }
}
