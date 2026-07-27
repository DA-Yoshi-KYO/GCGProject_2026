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

using System;
using System.Collections;
using Unity.VisualScripting;
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

    float postprocessProgress = 0.0f;
    [SerializeField] float activatePostprocessSpeed = 1.0f;

    protected override void Start()
    {
        base.Start();
        //設置ではなく発動方式なため
        //設置＝発動となる
        gimmickState = GimmickState.Active;
        volume = FindFirstObjectByType<Volume>();
        if (volume == null)
        {
            Debug.LogError("NyakiGimmick: シーン内にVolumeが見つかりません。", this);
            return;
        }
        volume.profile.TryGet(out catEye);
    }
    protected override void ActiveUpdate()
    {
        if (!isFirstActive)
        {
            isFirstActive = true;
            //エフェクト発生
            if (catEye != null)
            {
                catEye.active = true;
                StartCoroutine(ProgressPostprocess(false));
            }
            //当たり判定追加
            SetHitChecker(transform.position);

            gimmickSound.PlayOneShotSE("Gimmick_Nyaki", gameObject.transform.position, "NyakiSound");
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
            StartCoroutine(ProgressPostprocess(true, BrokenProgress));
            
        }

    }

    private void BrokenProgress()
    {
        catEye.isEnabled.value = false;
        DeleteHitChecker();
        Destroy(gameObject);
    }

    IEnumerator ProgressPostprocess(bool isInverse, Action onComplete = null)
    {
        if (isInverse)
        {
            while (postprocessProgress > 0.0f)
            {
                postprocessProgress -= Time.deltaTime * activatePostprocessSpeed;
                postprocessProgress = Mathf.Clamp01(postprocessProgress);
                catEye.progressFloat.value = postprocessProgress;

                yield return null;
            }
        }
        else
        {
            while (postprocessProgress < 1.0f)
            {
                catEye.isEnabled.value = true;
                postprocessProgress += Time.deltaTime * activatePostprocessSpeed;
                postprocessProgress = Mathf.Clamp01(postprocessProgress);
                catEye.progressFloat.value = postprocessProgress;
                yield return null;
            }
        }

        onComplete?.Invoke();
    }
}
