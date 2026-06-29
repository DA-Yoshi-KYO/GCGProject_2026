using UnityEngine;

public class MagicAnkhGimmick : GimmickBase
{
    bool isBrokenFirst = false;

    protected override void IdleUpdate()
    {
    }
    protected override void ActiveUpdate()
    {
        //プレイヤーアクションで
        //アンク指定時に鳴き声発生でアクティブ
        SetHitChecker(transform.position);
        gimmickState = GimmickState.Broken;
    }
    protected override void BrokenUpdate()
    {
        base.BrokenUpdate();

        if (!isBrokenFirst)
        {
            isBrokenFirst = true;
            DeleteHitChecker();
            Destroy(gameObject);
        }
    }
}
