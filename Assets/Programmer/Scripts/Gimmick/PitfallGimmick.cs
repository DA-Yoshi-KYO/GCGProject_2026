//|| PitfallGimmick.cs ||――――――――――
//|| 作者 : 大瀧蓮
//||
//|| 更新 : 2026/06/08 作成開始
//|| 
//||―――――――――――――――――――――
//|| 概要 : 落とし穴ギミック
//||        一定時間敵拘束可能にする
//||        拘束自体はHitManager.csで行う
//||―――――――――――――――――――――

using System.Collections.Generic;
using UnityEngine;
public class PitfallGimmick : GimmickBase
{
    [SerializeField]
    [Header("ギミックが有効な時間")] private float activeTime = 5f;

    private bool isFirstActive = false;
    private bool isFirstBroken = false;
    private CS_ThiefGimmickAction trappedThiefGimmickAction;
    public List<RaycastHit> hitHoles = new List<RaycastHit>();   // 落とし穴としてアルファクリッピングする為にレイキャストでヒットしたオブジェクトを格納するリスト

    protected override void Start()
    {
        base.Start();
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
            trappedThiefGimmickAction = GetThiefGimmickAction();
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
        base.BrokenUpdate();
        if (!isFirstBroken)
        {
            isFirstBroken = true;
            if (trappedThiefGimmickAction != null)
            {
                trappedThiefGimmickAction.PitFallEnd();
            }
            else
            {
                Debug.LogWarning("PitfallGimmick: ThiefGimmickAction is null.");
            }
            Debug.Log("落とし穴ギミックが壊れました");
        }
    }

    protected override bool GetGimmickSettingsArea()
    {
        if (!TryGetPlacementSurface(out RaycastHit surfaceHit))
            return false;

        return IsPlacementSurfaceAllowed(surfaceHit.transform);
    }

    void OnDestroy()
    {
        if (hitHoles == null)
        {
            return;
        }

        foreach (var hit in hitHoles)
        {
            if (hit.collider != null)
            {
                var renderer = hit.collider.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    // アルファクリッピングを元に戻す処理
                    renderer.material.SetFloat("_UseHole", 0f);
                }
            }
        }
    }
}
