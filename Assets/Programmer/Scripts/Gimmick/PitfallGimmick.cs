//|| PitfallGimmick.cs ||――――――――――
//|| 作者 : 大瀧蓮
//||
//|| 更新 : 2026/06/081 作成開始
//||―――――――――――――――――――――
//|| 概要 : 落とし穴ギミック
//||        一定時間敵拘束可能にする
//||        拘束自体はHitManager.csで行う
//||―――――――――――――――――――――

using UnityEngine;

public class PitfallGimmick : GimmickBase
{
    [SerializeField]
    [Header("ギミックが有効な時間")] private float activeTime = 5f;

    private bool isFirstActive = false;

    void Start()
    {
        //Idle状態で当たり判定を設定して
        //当たったらActive状態にする
        //※消えるまでの時間計算用(Active)
        SetHitChecker(transform.position);
    }
    protected override void ActiveUpdate()
    {
        if (!isFirstActive)
        {//Active状態になった瞬間に当たり判定を消す
            isFirstActive = true;
            DeleteHitChecker();
        }

        activeTime -= Time.deltaTime;
        if (activeTime <= 0)
        {//Active状態の時間が終わったら壊れる
            gimmickState = GimmickState.Broken;
        }
    }
    protected override void BrokenUpdate()
    {
        GetThiefGimmickAction().PitFallEnd();
        Destroy(gameObject);
    }
}
