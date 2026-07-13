using UnityEngine;
using System.Collections.Generic;

/*
+=====================================
 ファイル名 : CS_JumpEffectShaderOnly.cs
 概要     : Jump用ShaderOnlyEffect制御クラス
            子Renderer全体へ_Progressを反映し、
            Rendererごとに_UVUpMoveをランダム化する
 作者     : ヨシモト リョウ
 履歴     : 2026/07/13 新規作成
            2026/07/13 複数子Renderer対応
            2026/07/13 RendererごとのUV Up Moveランダム化を追加
=====================================+
*/

/// <summary>
/// 親Objectに付けて、子Object内の_Progressを持つRendererすべてへ
/// 再生進行度を渡すJump用ShaderOnlyEffectです。
/// 各Rendererごとに_UVUpMoveをランダム化できます。
/// </summary>
public class CS_JumpEffectShaderOnly : CS_EffectShaderOnly
{
    private static readonly int int_ProgressPropertyId =
        Shader.PropertyToID("_Progress");

    private static readonly int int_UVUpMovePropertyId =
        Shader.PropertyToID("_UVUpMove");

    [Header("自動で子Rendererを集めるか")]
    [SerializeField]
    private bool b_AutoCollectChildRenderers = true;

    [Header("UV上昇・Fadeを適用するRenderer一覧")]
    [SerializeField]
    private Renderer[] rd_JumpShockwaveRenderers;

    [Header("UV Up MoveをRendererごとにランダム化するか")]
    [SerializeField]
    private bool b_RandomizeUVUpMove = true;

    [Header("UV Up Move 最小値")]
    [SerializeField]
    private float f_MinUVUpMove = 0.2f;

    private MaterialPropertyBlock mpb_PropertyBlock;

    private float[] f_RandomUVUpMoveArray;

    private float f_CurrentProgress;
    private float f_EndStartProgress;

    /// <summary>
    /// 初期化処理です。
    /// </summary>
    public override void InitEffect()
    {
        base.InitEffect();

        if (mpb_PropertyBlock == null)
        {
            mpb_PropertyBlock = new MaterialPropertyBlock();
        }

        if (b_AutoCollectChildRenderers)
        {
            CollectProgressRenderers();
        }

        if (rd_JumpShockwaveRenderers == null ||
            rd_JumpShockwaveRenderers.Length <= 0)
        {
            Debug.LogWarning(
                "[CS_JumpEffectShaderOnly] _Progressを持つ子Rendererが見つかりません : "
                + gameObject.name);
        }

        CreateRandomUVUpMoveArray();
    }

    /// <summary>
    /// 子Objectから_Progressを持つMaterialのRendererをすべて集めます。
    /// </summary>
    private void CollectProgressRenderers()
    {
        Renderer[] rd_AllChildRenderers =
            GetComponentsInChildren<Renderer>(true);

        List<Renderer> list_Renderers =
            new List<Renderer>();

        for (int i = 0 ; i < rd_AllChildRenderers.Length ; i++)
        {
            Renderer rd_Renderer =
                rd_AllChildRenderers[i];

            if (rd_Renderer == null)
            {
                continue;
            }

            if (HasProgressMaterial(rd_Renderer))
            {
                list_Renderers.Add(rd_Renderer);
            }
        }

        rd_JumpShockwaveRenderers =
            list_Renderers.ToArray();
    }

    /// <summary>
    /// Rendererが_Progressを持つMaterialを使用しているか確認します。
    /// </summary>
    private bool HasProgressMaterial(Renderer rd_Renderer)
    {
        Material[] ma_Materials =
            rd_Renderer.sharedMaterials;

        for (int i = 0 ; i < ma_Materials.Length ; i++)
        {
            Material ma_Material =
                ma_Materials[i];

            if (ma_Material == null)
            {
                continue;
            }

            if (ma_Material.HasProperty(int_ProgressPropertyId))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// RendererごとのUV Up Moveランダム値を作成します。
    /// 最大値はMaterialに設定されている_UVUpMoveを使用します。
    /// </summary>
    private void CreateRandomUVUpMoveArray()
    {
        if (rd_JumpShockwaveRenderers == null)
        {
            f_RandomUVUpMoveArray = null;
            return;
        }

        f_RandomUVUpMoveArray =
            new float[rd_JumpShockwaveRenderers.Length];

        for (int i = 0 ; i < rd_JumpShockwaveRenderers.Length ; i++)
        {
            Renderer rd_Renderer =
                rd_JumpShockwaveRenderers[i];

            if (rd_Renderer == null)
            {
                f_RandomUVUpMoveArray[i] = f_MinUVUpMove;
                continue;
            }

            float f_MaxUVUpMove =
                GetMaterialUVUpMoveMaxValue(rd_Renderer);

            float f_Min =
                Mathf.Min(f_MinUVUpMove, f_MaxUVUpMove);

            float f_Max =
                Mathf.Max(f_MinUVUpMove, f_MaxUVUpMove);

            if (b_RandomizeUVUpMove)
            {
                f_RandomUVUpMoveArray[i] =
                    Random.Range(f_Min, f_Max);
            }
            else
            {
                f_RandomUVUpMoveArray[i] =
                    f_MaxUVUpMove;
            }
        }
    }

    /// <summary>
    /// RendererのMaterialから_UVUpMoveの値を取得します。
    /// これをランダム最大値として使います。
    /// </summary>
    private float GetMaterialUVUpMoveMaxValue(Renderer rd_Renderer)
    {
        if (rd_Renderer == null)
        {
            return f_MinUVUpMove;
        }

        Material[] ma_Materials =
            rd_Renderer.sharedMaterials;

        for (int i = 0 ; i < ma_Materials.Length ; i++)
        {
            Material ma_Material =
                ma_Materials[i];

            if (ma_Material == null)
            {
                continue;
            }

            if (ma_Material.HasProperty(int_UVUpMovePropertyId))
            {
                return ma_Material.GetFloat(int_UVUpMovePropertyId);
            }
        }

        return f_MinUVUpMove;
    }

    protected override void OnPlayPhaseStart()
    {
        f_CurrentProgress = 0.0f;
        f_EndStartProgress = 0.0f;

        CreateRandomUVUpMoveArray();

        ApplyProgressToAllRenderers(0.0f);
    }

    protected override void UpdatePlayPhase(float f_NormalizedPlayTime)
    {
        f_CurrentProgress =
            Mathf.Clamp01(f_NormalizedPlayTime);

        ApplyProgressToAllRenderers(f_CurrentProgress);
    }

    protected override void OnPlayPhaseComplete()
    {
        f_CurrentProgress = 1.0f;

        ApplyProgressToAllRenderers(1.0f);
    }

    protected override void OnEndPhaseStart()
    {
        f_EndStartProgress = f_CurrentProgress;
    }

    protected override void UpdateEndPhase(float f_NormalizedEndTime)
    {
        f_CurrentProgress =
            Mathf.Lerp(
                f_EndStartProgress,
                1.0f,
                Mathf.Clamp01(f_NormalizedEndTime));

        ApplyProgressToAllRenderers(f_CurrentProgress);
    }

    protected override void OnEndPhaseComplete()
    {
        f_CurrentProgress = 1.0f;

        ApplyProgressToAllRenderers(1.0f);
    }

    /// <summary>
    /// すべての対象RendererへProgressとUVUpMoveを反映します。
    /// </summary>
    private void ApplyProgressToAllRenderers(float f_Progress)
    {
        if (rd_JumpShockwaveRenderers == null)
        {
            return;
        }

        for (int i = 0 ; i < rd_JumpShockwaveRenderers.Length ; i++)
        {
            float f_UVUpMove =
                GetRandomUVUpMoveByIndex(i);

            ApplyPropertiesToRenderer(
                rd_JumpShockwaveRenderers[i],
                f_Progress,
                f_UVUpMove);
        }
    }

    /// <summary>
    /// 指定IndexのランダムUVUpMove値を取得します。
    /// </summary>
    private float GetRandomUVUpMoveByIndex(int f_Index)
    {
        if (f_RandomUVUpMoveArray == null ||
            f_Index < 0 ||
            f_Index >= f_RandomUVUpMoveArray.Length)
        {
            return f_MinUVUpMove;
        }

        return f_RandomUVUpMoveArray[f_Index];
    }

    /// <summary>
    /// 指定Rendererの_Progressと_UVUpMoveを更新します。
    /// </summary>
    private void ApplyPropertiesToRenderer(
        Renderer rd_Renderer,
        float f_Progress,
        float f_UVUpMove)
    {
        if (rd_Renderer == null)
        {
            return;
        }

        if (mpb_PropertyBlock == null)
        {
            mpb_PropertyBlock = new MaterialPropertyBlock();
        }

        Material[] ma_Materials =
            rd_Renderer.sharedMaterials;

        for (int i = 0 ; i < ma_Materials.Length ; i++)
        {
            Material ma_Material =
                ma_Materials[i];

            if (ma_Material == null)
            {
                continue;
            }

            if (ma_Material.HasProperty(int_ProgressPropertyId) == false)
            {
                continue;
            }

            mpb_PropertyBlock.Clear();

            rd_Renderer.GetPropertyBlock(
                mpb_PropertyBlock,
                i);

            mpb_PropertyBlock.SetFloat(
                int_ProgressPropertyId,
                Mathf.Clamp01(f_Progress));

            if (ma_Material.HasProperty(int_UVUpMovePropertyId))
            {
                mpb_PropertyBlock.SetFloat(
                    int_UVUpMovePropertyId,
                    f_UVUpMove);
            }

            rd_Renderer.SetPropertyBlock(
                mpb_PropertyBlock,
                i);
        }
    }
}
