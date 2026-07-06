using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
+=====================================
 ファイル名 : CS_StoneTabletParticleSystemEffect.cs
 概要     : StoneTablet用ParticleSystemEffect制御クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/07/05 新規作成
=====================================+
*/

/// <summary>
/// StoneTablet用のParticleSystemEffectです。
/// 通常Particleを再生し、StoneTablet_Rotate01 / StoneTablet_Rotate02を
/// テンプレートとしてClone生成してループ再生します。
/// Rotateの開始角度はZ軸だけランダムにします。
/// </summary>
public class CS_StoneTabletParticleSystemEffect : CS_EffectParticleSystem
{
    [Header("回転Particle 01")]
    [SerializeField]
    private ParticleSystem ps_StoneTabletRotate01;

    [Header("回転Particle 02")]
    [SerializeField]
    private ParticleSystem ps_StoneTabletRotate02;

    [Header("Rotateの出現間隔")]
    [SerializeField]
    private float f_RotatePlayInterval = 0.35f;

    private Coroutine co_RotateLoopCoroutine;

    private readonly List<GameObject> list_RotateInstanceObjects =
        new List<GameObject>();

    public override void InitEffect()
    {
        base.InitEffect();

        FindRotateParticleSystemsIfNeeded();

        SetRotateTemplateLoop(false);
        SetRotateTemplateActive(false);
    }

    protected override void PlayEffectProcess()
    {
        StopRotateLoopCoroutine();
        DestroyRotateInstances();

        if (ps_TargetParticleSystems == null || ps_TargetParticleSystems.Length <= 0)
        {
            CacheParticleSystems();
        }

        FindRotateParticleSystemsIfNeeded();

        HashSet<ParticleSystem> set_ExcludeParticleSystems =
            new HashSet<ParticleSystem>();

        if (ps_StoneTabletRotate01 != null)
        {
            set_ExcludeParticleSystems.Add(ps_StoneTabletRotate01);
        }

        if (ps_StoneTabletRotate02 != null)
        {
            set_ExcludeParticleSystems.Add(ps_StoneTabletRotate02);
        }

        // Rotateテンプレート以外を通常再生します。
        PlayParticleSystemsExcept(set_ExcludeParticleSystems);

        SetRotateTemplateActive(false);

        co_RotateLoopCoroutine = StartCoroutine(RotateLoopCoroutine());
    }

    /// <summary>
    /// Rotate01 / Rotate02を両方同時に生成してループ再生します。
    /// </summary>
    private IEnumerator RotateLoopCoroutine()
    {
        float f_Interval = Mathf.Max(0.01f, f_RotatePlayInterval);

        while (true)
        {
            CreateAndPlayRotateInstance(ps_StoneTabletRotate01);
            CreateAndPlayRotateInstance(ps_StoneTabletRotate02);

            yield return new WaitForSeconds(f_Interval);
        }
    }

    /// <summary>
    /// Rotateテンプレートから再生用Instanceを作成して再生します。
    /// </summary>
    private void CreateAndPlayRotateInstance(ParticleSystem ps_Template)
    {
        if (ps_Template == null)
        {
            return;
        }

        Transform tr_Template = ps_Template.transform;
        Transform tr_Parent = tr_Template.parent;

        GameObject go_Instance = Instantiate(
            ps_Template.gameObject,
            tr_Parent);

        go_Instance.name = ps_Template.gameObject.name + "_Play";

        Transform tr_Instance = go_Instance.transform;
        tr_Instance.localPosition = tr_Template.localPosition;
        tr_Instance.localScale = tr_Template.localScale;
        tr_Instance.localRotation = tr_Template.localRotation;

        go_Instance.SetActive(true);

        ParticleSystem ps_Instance =
            go_Instance.GetComponent<ParticleSystem>();

        if (ps_Instance == null)
        {
            Destroy(go_Instance);
            return;
        }

        ParticleSystem.MainModule ps_MainModule = ps_Instance.main;
        ps_MainModule.loop = false;

        ps_Instance.Clear(true);
        ps_Instance.Play(true);

        list_RotateInstanceObjects.Add(go_Instance);

        float f_DestroyTime =
            ps_MainModule.duration +
            ps_MainModule.startLifetime.constantMax +
            0.5f;

        StartCoroutine(DestroyRotateInstanceCoroutine(go_Instance, f_DestroyTime));
    }

    private IEnumerator DestroyRotateInstanceCoroutine(
        GameObject go_Instance,
        float f_DestroyTime)
    {
        yield return new WaitForSeconds(Mathf.Max(0.1f, f_DestroyTime));

        if (go_Instance != null)
        {
            list_RotateInstanceObjects.Remove(go_Instance);
            Destroy(go_Instance);
        }
    }

    private void FindRotateParticleSystemsIfNeeded()
    {
        if (ps_StoneTabletRotate01 == null)
        {
            ps_StoneTabletRotate01 =
                FindChildParticleSystemByName("StoneTablet_Rotate01");
        }

        if (ps_StoneTabletRotate02 == null)
        {
            ps_StoneTabletRotate02 =
                FindChildParticleSystemByName("StoneTablet_Rotate02");
        }
    }

    private ParticleSystem FindChildParticleSystemByName(string str_TargetName)
    {
        ParticleSystem[] ps_ChildParticleSystems =
            GetComponentsInChildren<ParticleSystem>(true);

        for (int i = 0 ; i < ps_ChildParticleSystems.Length ; i++)
        {
            ParticleSystem ps_Target = ps_ChildParticleSystems[i];

            if (ps_Target == null)
            {
                continue;
            }

            if (ps_Target.gameObject.name == str_TargetName)
            {
                return ps_Target;
            }
        }

        return null;
    }

    private void SetRotateTemplateLoop(bool b_IsLoop)
    {
        SetParticleLoop(ps_StoneTabletRotate01, b_IsLoop);
        SetParticleLoop(ps_StoneTabletRotate02, b_IsLoop);
    }

    private void SetParticleLoop(
        ParticleSystem ps_Target,
        bool b_IsLoop)
    {
        if (ps_Target == null)
        {
            return;
        }

        ParticleSystem.MainModule ps_MainModule = ps_Target.main;
        ps_MainModule.loop = b_IsLoop;
    }

    private void SetRotateTemplateActive(bool b_IsActive)
    {
        if (ps_StoneTabletRotate01 != null)
        {
            ps_StoneTabletRotate01.gameObject.SetActive(b_IsActive);
        }

        if (ps_StoneTabletRotate02 != null)
        {
            ps_StoneTabletRotate02.gameObject.SetActive(b_IsActive);
        }
    }

    protected override void EndEffectProcess()
    {
        StopRotateLoopCoroutine();
        DestroyRotateInstances();

        base.EndEffectProcess();
    }

    private void StopRotateLoopCoroutine()
    {
        if (co_RotateLoopCoroutine == null)
        {
            return;
        }

        StopCoroutine(co_RotateLoopCoroutine);
        co_RotateLoopCoroutine = null;
    }

    private void DestroyRotateInstances()
    {
        for (int i = list_RotateInstanceObjects.Count - 1 ; i >= 0 ; i--)
        {
            GameObject go_Instance = list_RotateInstanceObjects[i];

            if (go_Instance != null)
            {
                Destroy(go_Instance);
            }
        }

        list_RotateInstanceObjects.Clear();
    }

    protected override void OnDisable()
    {
        StopRotateLoopCoroutine();
        DestroyRotateInstances();

        base.OnDisable();
    }
}
