using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
+=====================================
 ファイル名 : CS_AnkhEffectPlayer.cs
 概要     : Ankh用の波紋連続再生クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/07/03 新規作成
=====================================+
*/

/// <summary>
/// Ankh用の波紋連続再生クラスです。
/// 子にある最初のRenderer付きObjectをテンプレートとして使い、
/// 指定間隔ごとに新しい波紋を追加で発生させます。
/// </summary>
public class CS_AnkhEffectPlayer : CSAD_EffectCommonProcessBase
{
    /// <summary>
    /// 波紋1個分の制御データです。
    /// </summary>
    private class CS_AnkhRippleUnit
    {
        public GameObject go_RippleObject;
        public Renderer[] rd_Renderers;
        public MaterialPropertyBlock mpb_PropertyBlock;
        public Coroutine co_PlayCoroutine;
        public bool b_IsPlaying;
    }

    private static readonly int int_ProgressPropertyId = Shader.PropertyToID("_Progress");
    private static readonly int int_BlackCutPropertyId = Shader.PropertyToID("_BlackCut");

    [Header("波紋の出現間隔")]
    [SerializeField]
    private float f_RippleInterval = 0.25f;

    [Header("波紋を何発出すか 0以下で無限")]
    [SerializeField]
    private int n_RippleShotCount = 0;

    [Header("Progress開始値")]
    [SerializeField]
    private float f_StartProgress = 0.0f;

    [Header("Progress到達値")]
    [SerializeField]
    private float f_TargetProgress = 0.4f;

    [Header("BlackCut開始値")]
    [SerializeField]
    private float f_StartBlackCut = 0.0f;

    [Header("BlackCut到達値")]
    [SerializeField]
    private float f_TargetBlackCut = 0.6f;

    /// <summary>
    /// 波紋Unit一覧です。
    /// </summary>
    private List<CS_AnkhRippleUnit> list_RippleUnits =
        new List<CS_AnkhRippleUnit>();

    /// <summary>
    /// 複製元にする波紋Objectです。
    /// </summary>
    private GameObject go_RippleTemplateObject;

    /// <summary>
    /// 波紋Objectの親です。
    /// </summary>
    private Transform tr_RippleParent;

    /// <summary>
    /// 波紋発生管理Coroutineです。
    /// </summary>
    private Coroutine co_MasterCoroutine;

    /// <summary>
    /// 終了待機Coroutineです。
    /// </summary>
    private Coroutine co_EndWaitCoroutine;

    /// <summary>
    /// 初期化処理です。
    /// </summary>
    public override void InitEffect()
    {
        InitializeTemplateObject();
        InitializeFirstRippleUnit();
    }

    /// <summary>
    /// Effect再生処理です。
    /// </summary>
    protected override void PlayEffectProcess()
    {
        StopAllInternalCoroutines();

        ResetAllRippleUnits();

        co_MasterCoroutine = StartCoroutine(MasterPlayCoroutine());
    }

    /// <summary>
    /// Effect終了処理です。
    /// 新しい波紋の発生を止め、今出ている波紋が終わったら終了します。
    /// </summary>
    protected override void EndEffectProcess()
    {
        if (co_MasterCoroutine != null)
        {
            StopCoroutine(co_MasterCoroutine);
            co_MasterCoroutine = null;
        }

        if (co_EndWaitCoroutine != null)
        {
            StopCoroutine(co_EndWaitCoroutine);
            co_EndWaitCoroutine = null;
        }

        co_EndWaitCoroutine = StartCoroutine(EndWaitCoroutine());
    }

    /// <summary>
    /// テンプレートObjectを初期化します。
    /// Ankh配下の最初のRenderer付きObjectを複製元にします。
    /// </summary>
    private void InitializeTemplateObject()
    {
        if (go_RippleTemplateObject != null)
        {
            return;
        }

        Renderer[] rd_Renderers = GetComponentsInChildren<Renderer>(true);

        if (rd_Renderers == null || rd_Renderers.Length <= 0)
        {
            Debug.LogWarning("[CS_AnkhEffectPlayer] 子にRendererがありません。");
            return;
        }

        Renderer rd_TemplateRenderer = null;

        for (int i = 0 ; i < rd_Renderers.Length ; i++)
        {
            if (rd_Renderers[i] == null)
            {
                continue;
            }

            if (rd_Renderers[i].transform == transform)
            {
                continue;
            }

            rd_TemplateRenderer = rd_Renderers[i];
            break;
        }

        if (rd_TemplateRenderer == null)
        {
            rd_TemplateRenderer = rd_Renderers[0];
        }

        go_RippleTemplateObject = rd_TemplateRenderer.gameObject;
        tr_RippleParent = go_RippleTemplateObject.transform.parent;
    }

    /// <summary>
    /// 最初の波紋Unitを作成します。
    /// </summary>
    private void InitializeFirstRippleUnit()
    {
        if (go_RippleTemplateObject == null)
        {
            return;
        }

        if (list_RippleUnits == null)
        {
            list_RippleUnits = new List<CS_AnkhRippleUnit>();
        }

        if (list_RippleUnits.Count > 0)
        {
            return;
        }

        CS_AnkhRippleUnit cs_FirstRippleUnit =
            CreateRippleUnitFromObject(go_RippleTemplateObject);

        if (cs_FirstRippleUnit == null)
        {
            return;
        }

        list_RippleUnits.Add(cs_FirstRippleUnit);
    }

    /// <summary>
    /// 波紋の発生管理処理です。
    /// </summary>
    private IEnumerator MasterPlayCoroutine()
    {
        do
        {
            int n_CurrentShotCount = 0;

            while (b_IsEndRequested == false)
            {
                if (n_RippleShotCount > 0 &&
                    n_CurrentShotCount >= n_RippleShotCount)
                {
                    break;
                }

                PlayNewRipple();

                n_CurrentShotCount++;

                if (f_RippleInterval > 0.0f)
                {
                    yield return new WaitForSeconds(f_RippleInterval);
                }
                else
                {
                    yield return null;
                }
            }

            if (b_IsEndRequested)
            {
                break;
            }

            yield return StartCoroutine(WaitUntilAllRippleFinishedCoroutine());

            ResetAllRippleUnits();

            if (IsLoopEffect() == false)
            {
                break;
            }

            yield return null;

        } while (b_IsEndRequested == false);

        co_MasterCoroutine = null;

        if (b_IsEndRequested)
        {
            yield break;
        }

        FinishEndEffect();
    }

    /// <summary>
    /// 終了待機処理です。
    /// </summary>
    private IEnumerator EndWaitCoroutine()
    {
        yield return StartCoroutine(WaitUntilAllRippleFinishedCoroutine());

        ResetAllRippleUnits();

        co_EndWaitCoroutine = null;

        FinishEndEffect();
    }

    /// <summary>
    /// 新しい波紋を発生させます。
    /// 再生中の波紋は止めず、空きUnitか追加生成Unitを使います。
    /// </summary>
    private void PlayNewRipple()
    {
        CS_AnkhRippleUnit cs_RippleUnit = GetFreeRippleUnit();

        if (cs_RippleUnit == null)
        {
            cs_RippleUnit = CreateAdditionalRippleUnit();
        }

        if (cs_RippleUnit == null)
        {
            return;
        }

        if (cs_RippleUnit.co_PlayCoroutine != null)
        {
            StopCoroutine(cs_RippleUnit.co_PlayCoroutine);
            cs_RippleUnit.co_PlayCoroutine = null;
        }

        cs_RippleUnit.go_RippleObject.SetActive(true);

        ApplyShaderValue(
            cs_RippleUnit,
            f_StartProgress,
            f_StartBlackCut);

        cs_RippleUnit.co_PlayCoroutine =
            StartCoroutine(RipplePlayCoroutine(cs_RippleUnit));
    }

    /// <summary>
    /// 空いている波紋Unitを取得します。
    /// </summary>
    private CS_AnkhRippleUnit GetFreeRippleUnit()
    {
        if (list_RippleUnits == null)
        {
            return null;
        }

        for (int i = 0 ; i < list_RippleUnits.Count ; i++)
        {
            CS_AnkhRippleUnit cs_RippleUnit = list_RippleUnits[i];

            if (cs_RippleUnit == null)
            {
                continue;
            }

            if (cs_RippleUnit.b_IsPlaying == false)
            {
                return cs_RippleUnit;
            }
        }

        return null;
    }

    /// <summary>
    /// 追加の波紋Unitを作成します。
    /// </summary>
    private CS_AnkhRippleUnit CreateAdditionalRippleUnit()
    {
        if (go_RippleTemplateObject == null)
        {
            return null;
        }

        Transform tr_Parent = tr_RippleParent;

        if (tr_Parent == null)
        {
            tr_Parent = transform;
        }

        GameObject go_NewRippleObject = Instantiate(
            go_RippleTemplateObject,
            tr_Parent);

        go_NewRippleObject.name =
            go_RippleTemplateObject.name + "_Ripple_" + list_RippleUnits.Count.ToString("00");

        CS_AnkhRippleUnit cs_RippleUnit =
            CreateRippleUnitFromObject(go_NewRippleObject);

        if (cs_RippleUnit == null)
        {
            Destroy(go_NewRippleObject);
            return null;
        }

        list_RippleUnits.Add(cs_RippleUnit);

        return cs_RippleUnit;
    }

    /// <summary>
    /// 指定Objectから波紋Unitを作成します。
    /// </summary>
    private CS_AnkhRippleUnit CreateRippleUnitFromObject(GameObject f_RippleObject)
    {
        if (f_RippleObject == null)
        {
            return null;
        }

        CS_AnkhRippleUnit cs_RippleUnit = new CS_AnkhRippleUnit();

        cs_RippleUnit.go_RippleObject = f_RippleObject;
        cs_RippleUnit.rd_Renderers =
            f_RippleObject.GetComponentsInChildren<Renderer>(true);
        cs_RippleUnit.mpb_PropertyBlock = new MaterialPropertyBlock();
        cs_RippleUnit.co_PlayCoroutine = null;
        cs_RippleUnit.b_IsPlaying = false;

        ApplyShaderValue(
            cs_RippleUnit,
            f_StartProgress,
            f_StartBlackCut);

        f_RippleObject.SetActive(false);

        return cs_RippleUnit;
    }

    /// <summary>
    /// 波紋1個分の再生処理です。
    /// </summary>
    private IEnumerator RipplePlayCoroutine(CS_AnkhRippleUnit f_RippleUnit)
    {
        if (f_RippleUnit == null)
        {
            yield break;
        }

        f_RippleUnit.b_IsPlaying = true;

        yield return StartCoroutine(ProgressCoroutine(f_RippleUnit));

        yield return StartCoroutine(BlackCutCoroutine(f_RippleUnit));

        ApplyShaderValue(
            f_RippleUnit,
            f_StartProgress,
            f_StartBlackCut);

        f_RippleUnit.b_IsPlaying = false;
        f_RippleUnit.co_PlayCoroutine = null;

        if (f_RippleUnit.go_RippleObject != null)
        {
            f_RippleUnit.go_RippleObject.SetActive(false);
        }
    }

    /// <summary>
    /// Progressを進めます。
    /// </summary>
    private IEnumerator ProgressCoroutine(CS_AnkhRippleUnit f_RippleUnit)
    {
        float f_PlayTime = GetPlayTime();

        if (f_PlayTime <= 0.0f)
        {
            ApplyShaderValue(
                f_RippleUnit,
                f_TargetProgress,
                f_StartBlackCut);

            yield break;
        }

        float f_CurrentTime = 0.0f;

        while (f_CurrentTime < f_PlayTime)
        {
            f_CurrentTime += Time.deltaTime;

            float f_Rate = Mathf.Clamp01(f_CurrentTime / f_PlayTime);

            float f_Progress = Mathf.Lerp(
                f_StartProgress,
                f_TargetProgress,
                f_Rate);

            ApplyShaderValue(
                f_RippleUnit,
                f_Progress,
                f_StartBlackCut);

            yield return null;
        }

        ApplyShaderValue(
            f_RippleUnit,
            f_TargetProgress,
            f_StartBlackCut);
    }

    /// <summary>
    /// Progressを固定したままBlackCutを進めます。
    /// </summary>
    private IEnumerator BlackCutCoroutine(CS_AnkhRippleUnit f_RippleUnit)
    {
        float f_EndTime = GetEndTime();

        if (f_EndTime <= 0.0f)
        {
            ApplyShaderValue(
                f_RippleUnit,
                f_TargetProgress,
                f_TargetBlackCut);

            yield break;
        }

        float f_CurrentTime = 0.0f;

        while (f_CurrentTime < f_EndTime)
        {
            f_CurrentTime += Time.deltaTime;

            float f_Rate = Mathf.Clamp01(f_CurrentTime / f_EndTime);

            float f_BlackCut = Mathf.Lerp(
                f_StartBlackCut,
                f_TargetBlackCut,
                f_Rate);

            ApplyShaderValue(
                f_RippleUnit,
                f_TargetProgress,
                f_BlackCut);

            yield return null;
        }

        ApplyShaderValue(
            f_RippleUnit,
            f_TargetProgress,
            f_TargetBlackCut);
    }

    /// <summary>
    /// 全波紋が終わるまで待機します。
    /// </summary>
    private IEnumerator WaitUntilAllRippleFinishedCoroutine()
    {
        while (IsAnyRipplePlaying())
        {
            yield return null;
        }
    }

    /// <summary>
    /// 再生中の波紋があるか確認します。
    /// </summary>
    private bool IsAnyRipplePlaying()
    {
        if (list_RippleUnits == null)
        {
            return false;
        }

        for (int i = 0 ; i < list_RippleUnits.Count ; i++)
        {
            CS_AnkhRippleUnit cs_RippleUnit = list_RippleUnits[i];

            if (cs_RippleUnit == null)
            {
                continue;
            }

            if (cs_RippleUnit.b_IsPlaying)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 全波紋を初期状態に戻します。
    /// </summary>
    private void ResetAllRippleUnits()
    {
        if (list_RippleUnits == null)
        {
            return;
        }

        for (int i = 0 ; i < list_RippleUnits.Count ; i++)
        {
            CS_AnkhRippleUnit cs_RippleUnit = list_RippleUnits[i];

            if (cs_RippleUnit == null)
            {
                continue;
            }

            if (cs_RippleUnit.co_PlayCoroutine != null)
            {
                StopCoroutine(cs_RippleUnit.co_PlayCoroutine);
                cs_RippleUnit.co_PlayCoroutine = null;
            }

            cs_RippleUnit.b_IsPlaying = false;

            ApplyShaderValue(
                cs_RippleUnit,
                f_StartProgress,
                f_StartBlackCut);

            if (cs_RippleUnit.go_RippleObject != null)
            {
                cs_RippleUnit.go_RippleObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 内部Coroutineをすべて停止します。
    /// </summary>
    private void StopAllInternalCoroutines()
    {
        if (co_MasterCoroutine != null)
        {
            StopCoroutine(co_MasterCoroutine);
            co_MasterCoroutine = null;
        }

        if (co_EndWaitCoroutine != null)
        {
            StopCoroutine(co_EndWaitCoroutine);
            co_EndWaitCoroutine = null;
        }

        if (list_RippleUnits == null)
        {
            return;
        }

        for (int i = 0 ; i < list_RippleUnits.Count ; i++)
        {
            CS_AnkhRippleUnit cs_RippleUnit = list_RippleUnits[i];

            if (cs_RippleUnit == null)
            {
                continue;
            }

            if (cs_RippleUnit.co_PlayCoroutine != null)
            {
                StopCoroutine(cs_RippleUnit.co_PlayCoroutine);
                cs_RippleUnit.co_PlayCoroutine = null;
            }

            cs_RippleUnit.b_IsPlaying = false;
        }
    }

    /// <summary>
    /// Shader値を波紋Unitに反映します。
    /// </summary>
    private void ApplyShaderValue(
        CS_AnkhRippleUnit f_RippleUnit,
        float f_Progress,
        float f_BlackCut)
    {
        if (f_RippleUnit == null)
        {
            return;
        }

        if (f_RippleUnit.rd_Renderers == null)
        {
            return;
        }

        if (f_RippleUnit.mpb_PropertyBlock == null)
        {
            f_RippleUnit.mpb_PropertyBlock = new MaterialPropertyBlock();
        }

        for (int i = 0 ; i < f_RippleUnit.rd_Renderers.Length ; i++)
        {
            Renderer rd_Renderer = f_RippleUnit.rd_Renderers[i];

            if (rd_Renderer == null)
            {
                continue;
            }

            Material[] ma_SharedMaterials = rd_Renderer.sharedMaterials;

            for (int j = 0 ; j < ma_SharedMaterials.Length ; j++)
            {
                Material ma_SharedMaterial = ma_SharedMaterials[j];

                if (ma_SharedMaterial == null)
                {
                    continue;
                }

                bool b_HasProgress =
                    ma_SharedMaterial.HasProperty(int_ProgressPropertyId);

                bool b_HasBlackCut =
                    ma_SharedMaterial.HasProperty(int_BlackCutPropertyId);

                if (b_HasProgress == false && b_HasBlackCut == false)
                {
                    continue;
                }

                f_RippleUnit.mpb_PropertyBlock.Clear();

                rd_Renderer.GetPropertyBlock(
                    f_RippleUnit.mpb_PropertyBlock,
                    j);

                if (b_HasProgress)
                {
                    f_RippleUnit.mpb_PropertyBlock.SetFloat(
                        int_ProgressPropertyId,
                        f_Progress);
                }

                if (b_HasBlackCut)
                {
                    f_RippleUnit.mpb_PropertyBlock.SetFloat(
                        int_BlackCutPropertyId,
                        f_BlackCut);
                }

                rd_Renderer.SetPropertyBlock(
                    f_RippleUnit.mpb_PropertyBlock,
                    j);
            }
        }
    }

    /// <summary>
    /// 再生時間を取得します。
    /// </summary>
    private float GetPlayTime()
    {
        if (csst_EffectPlayData.f_PlayTime.HasValue == false)
        {
            return 0.0f;
        }

        return Mathf.Max(0.0f, csst_EffectPlayData.f_PlayTime.Value);
    }

    /// <summary>
    /// 終了時間を取得します。
    /// </summary>
    private float GetEndTime()
    {
        if (csst_EffectPlayData.f_EndTime.HasValue == false)
        {
            return 0.0f;
        }

        return Mathf.Max(0.0f, csst_EffectPlayData.f_EndTime.Value);
    }

    /// <summary>
    /// ループ設定を取得します。
    /// </summary>
    private bool IsLoopEffect()
    {
        if (csst_EffectPlayData.b_LoopFlag.HasValue == false)
        {
            return false;
        }

        return csst_EffectPlayData.b_LoopFlag.Value;
    }
}
