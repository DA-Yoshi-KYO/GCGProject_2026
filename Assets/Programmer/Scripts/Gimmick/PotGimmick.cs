using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotGimmick : GimmickBase
{
    private bool isFirstUpdate = true;
    protected override void IdleUpdate()
    {
    }

    protected override void ActiveUpdate()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            SetHitChecker(gimmickGridPos.x, gimmickGridPos.y);
        }
    }

    protected override void BrokenUpdate()
    {
        isFirstUpdate = true;
        DeleteHitChecker();
    }
}
