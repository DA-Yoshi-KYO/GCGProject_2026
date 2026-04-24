// == PotGimmick.cs ==
// 作者 :秋野翔太
// 更新 :2026/04/22 作成開始
//
// ポットギミック仕様
// ・Active状態のとき、命中範囲に当たり判定を
// ・当たり判定内に、敵がいた場合、攻撃力を与える
// ・Broken状態のとき、当たり判定を消す
// ・当たり判定は、ギミックの向きに応じて、
//   ギミックの前方に設置する
// ・当たり判定の大きさは、HitRangeX, HitRangeY


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
