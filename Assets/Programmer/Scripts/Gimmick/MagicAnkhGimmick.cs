using UnityEngine;

public class MagicAnkhGimmick : GimmickBase
{
    [SerializeField]
    private float activeTime = 0.5f;

    bool isBrokenFirst = false;

    protected override void IdleUpdate()
    {
    }
    protected override void ActiveUpdate()
    {
        //プレイヤーアクションで
        //アンク指定時に鳴き声発生でアクティブ
        SetHitChecker(transform.position);
        
        if(activeTime <= 0)
        {
            gimmickState = GimmickState.Broken;
        }

        activeTime -= Time.deltaTime;
    }
    protected override void BrokenUpdate()
    {
        base.BrokenUpdate();

        if (isBrokenFirst) return;
            isBrokenFirst = true;
            DeleteHitChecker();
    }
}
