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
            Vector2Int directionVec = GetDirectionVec();
            Vector2Int hitCheckerGridPos = new Vector2Int(gimmickGridPos.x + directionVec.x, gimmickGridPos.y + directionVec.y);
            SetHitChecker(hitCheckerGridPos.x, hitCheckerGridPos.y);
        }
    }

    protected override void BrokenUpdate()
    {
        isFirstUpdate = true;
        DeleteHitChecker();
    }
}
