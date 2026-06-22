using UnityEngine;

public class HyperVoiceGimmick : GimmickBase
{
    private void Start()
    {
        gimmickState = GimmickState.Active;
        //サウンド実装箇所
    }
    protected override void IdleUpdate()
    {
        Vector2Int offset = GetDirectionVec();

        transform.position += 
            new Vector3(offset.x, 0f, offset.y) * hitRange.z;

        SetHitChecker(transform.position);

        gimmickState = GimmickState.Broken;
    }
    protected override void BrokenUpdate()
    {
        DeleteHitChecker();
        Destroy(gameObject);
    }
}
