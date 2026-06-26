//|| NyakiGimmick.cs ||―――――――――――――――――
//|| 作者 : 大瀧蓮
//||
//|| 更新 : 2026/未確認 作成開始
//||        2026/0618   リファクタ(大瀧)
//||
//|| ――――――――――――――――――――――――――
//|| 
//|| 概要 : にゃきによって広範囲に渡り、
//||        泥棒にダメージを与えられる。
//||        
//||        設置→インタラクトではなく
//||        設置＝インタラクトなため、
//||        設置した瞬間アクティブになるものとする。
//||
//|| ――――――――――――――――――――――――――

using UnityEngine;
using UnityEngine.Rendering;
public class NyakiGimmick : GimmickBase
{
    private Volume volume;

    [Header("効果時間")]
    [SerializeField]
    private float time;

    CSV_CatEye catEye;
    private bool isFirstActive = false;

    protected override void IdleUpdate()
    {
        //設置ではなく発動方式なため
        //設置＝発動となる
        gimmickState = GimmickState.Active;
        volume = FindFirstObjectByType<Volume>();
        volume.profile.TryGet(out catEye);
    }
    protected override void ActiveUpdate()
    {
        if (!isFirstActive)
        {
            Debug.Log("にゃき発動");
            isFirstActive = true;
            //エフェクト発生
            if (catEye != null)
            {
                catEye.active = true;
                catEye.isEnabled.value = true;
            }
            //当たり判定追加
            SetHitChecker(transform.position);
        }
        //時間でエフェクト消去
        time -= Time.deltaTime;
        if(time <= 0)
        {
            gimmickState = GimmickState.Broken;
        }
    }

    protected override void BrokenUpdate()
    {
        //破壊関数
        if (catEye != null)
        {
            catEye.isEnabled.value = false;
        }
        DeleteHitChecker();
        Destroy(gameObject);
    }
}
