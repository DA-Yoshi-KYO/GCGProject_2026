using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectParticleSystem.cs
 概要     : ParticleSystem形式のEffectを再生する共通クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/07/05 新規作成
=====================================+
*/

/// <summary>
/// ParticleSystem形式のEffectを再生する共通クラスです。
/// 親に付けて、子階層のParticleSystemをまとめて再生します。
/// Inspector用の余計な設定は持ちません。
/// </summary>
public class CS_EffectParticleSystem : CSAD_EffectCommonProcessBase
{
    /// <summary>
    /// 管理対象ParticleSystem一覧です。
    /// </summary>
    protected ParticleSystem[] ps_TargetParticleSystems;

    /// <summary>
    /// 終了時間待機Coroutineです。
    /// </summary>
    private Coroutine co_EndCoroutine;

    /// <summary>
    /// 初期化処理です。
    /// </summary>
    public override void InitEffect()
    {
        CacheParticleSystems();
    }

    /// <summary>
    /// 子階層を含めたParticleSystemを取得します。
    /// </summary>
    protected void CacheParticleSystems()
    {
        ps_TargetParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    /// <summary>
    /// ParticleSystem再生処理です。
    /// </summary>
    protected override void PlayEffectProcess()
    {
        StopEndCoroutine();

        if (ps_TargetParticleSystems == null || ps_TargetParticleSystems.Length <= 0)
        {
            CacheParticleSystems();
        }

        PlayAllParticleSystems();
    }

    /// <summary>
    /// 全ParticleSystemを再生します。
    /// </summary>
    protected void PlayAllParticleSystems()
    {
        if (ps_TargetParticleSystems == null)
        {
            return;
        }

        for (int i = 0 ; i < ps_TargetParticleSystems.Length ; i++)
        {
            PlayParticleSystem(ps_TargetParticleSystems[i]);
        }
    }

    /// <summary>
    /// 指定ParticleSystem以外を再生します。
    /// </summary>
    /// <param name="set_ExcludeParticleSystems">除外するParticleSystem一覧。</param>
    protected void PlayParticleSystemsExcept(HashSet<ParticleSystem> set_ExcludeParticleSystems)
    {
        if (ps_TargetParticleSystems == null)
        {
            return;
        }

        for (int i = 0 ; i < ps_TargetParticleSystems.Length ; i++)
        {
            ParticleSystem ps_Target = ps_TargetParticleSystems[i];

            if (ps_Target == null)
            {
                continue;
            }

            if (set_ExcludeParticleSystems != null &&
                set_ExcludeParticleSystems.Contains(ps_Target))
            {
                continue;
            }

            PlayParticleSystem(ps_Target);
        }
    }

    /// <summary>
    /// 指定ParticleSystemを再生します。
    /// </summary>
    /// <param name="ps_Target">再生対象。</param>
    protected void PlayParticleSystem(ParticleSystem ps_Target)
    {
        if (ps_Target == null)
        {
            return;
        }

        ps_Target.gameObject.SetActive(true);
        ps_Target.Clear(true);
        ps_Target.Play(true);
    }

    /// <summary>
    /// ParticleSystem終了処理です。
    /// </summary>
    protected override void EndEffectProcess()
    {
        StopEndCoroutine();

        StopAllParticleSystems(ParticleSystemStopBehavior.StopEmitting);

        float f_EndTime = GetEndTime();

        if (f_EndTime > 0.0f)
        {
            co_EndCoroutine = StartCoroutine(EndCoroutine(f_EndTime));
            return;
        }

        FinishEndEffect();
    }

    /// <summary>
    /// 全ParticleSystemを停止します。
    /// </summary>
    /// <param name="e_StopBehavior">停止方法。</param>
    protected void StopAllParticleSystems(ParticleSystemStopBehavior e_StopBehavior)
    {
        if (ps_TargetParticleSystems == null)
        {
            return;
        }

        for (int i = 0 ; i < ps_TargetParticleSystems.Length ; i++)
        {
            StopParticleSystem(ps_TargetParticleSystems[i], e_StopBehavior);
        }
    }

    /// <summary>
    /// 指定ParticleSystemを停止します。
    /// </summary>
    /// <param name="ps_Target">停止対象。</param>
    /// <param name="e_StopBehavior">停止方法。</param>
    protected void StopParticleSystem(
        ParticleSystem ps_Target,
        ParticleSystemStopBehavior e_StopBehavior)
    {
        if (ps_Target == null)
        {
            return;
        }

        ps_Target.Stop(true, e_StopBehavior);
    }

    /// <summary>
    /// 再生データから終了時間を取得します。
    /// </summary>
    /// <returns>終了時間。</returns>
    private float GetEndTime()
    {
        if (csst_EffectPlayData.f_EndTime.HasValue == false)
        {
            return 0.0f;
        }

        return Mathf.Max(0.0f, csst_EffectPlayData.f_EndTime.Value);
    }

    /// <summary>
    /// 終了時間分待ってから終了完了します。
    /// </summary>
    /// <param name="f_EndTime">終了待機時間。</param>
    private IEnumerator EndCoroutine(float f_EndTime)
    {
        yield return new WaitForSeconds(f_EndTime);

        co_EndCoroutine = null;

        FinishEndEffect();
    }

    /// <summary>
    /// 終了Coroutineを止めます。
    /// </summary>
    private void StopEndCoroutine()
    {
        if (co_EndCoroutine == null)
        {
            return;
        }

        StopCoroutine(co_EndCoroutine);
        co_EndCoroutine = null;
    }

    /// <summary>
    /// 非アクティブ時にCoroutineを止めます。
    /// </summary>
    protected virtual void OnDisable()
    {
        StopEndCoroutine();
    }
}
