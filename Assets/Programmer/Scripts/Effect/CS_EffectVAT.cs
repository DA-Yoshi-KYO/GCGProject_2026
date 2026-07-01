using UnityEngine;
using UnityEngine.Rendering;

/*
+=====================================
 ファイル名 : CS_EffectVAT.cs
 概要     : VAT形式Effectの再生制御クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/30 新規作成
=====================================+
*/

/// <summary>
/// VAT形式のEffectを再生するクラスです。
/// MaterialのDisplay Frameを、共通再生時間に合わせて0から最終Frameまで進めます。
/// </summary>
public class CS_EffectVAT : CSAD_EffectCommonProcessBase
{
    /// <summary>
    /// 子を含めたRenderer一覧です。
    /// </summary>
    private Renderer[] rd_ChildRenderers;

    /// <summary>
    /// MaterialPropertyBlockです。
    /// </summary>
    private MaterialPropertyBlock mpb_PropertyBlock;

    /// <summary>
    /// Display FrameのProperty IDです。
    /// </summary>
    private int n_DisplayFramePropertyId = -1;

    /// <summary>
    /// Frame CountのProperty IDです。
    /// </summary>
    private int n_FrameCountPropertyId = -1;

    /// <summary>
    /// Auto PlaybackのProperty IDです。
    /// </summary>
    private int n_AutoPlaybackPropertyId = -1;

    /// <summary>
    /// 再生中かどうかです。
    /// </summary>
    private bool b_IsPlaying;

    /// <summary>
    /// 終了済みかどうかです。
    /// </summary>
    private bool b_IsFinished;

    /// <summary>
    /// 現在の再生経過時間です。
    /// </summary>
    private float f_CurrentPlayTime;

    /// <summary>
    /// VATのFrame数です。
    /// </summary>
    private float f_FrameCount = 1.0f;

    /// <summary>
    /// 警告表示済みかどうかです。
    /// </summary>
    private bool b_IsLoggedMissingProperty;

    /// <summary>
    /// 初期化処理です。
    /// </summary>
    public override void InitEffect()
    {
        rd_ChildRenderers = GetComponentsInChildren<Renderer>(true);

        if (mpb_PropertyBlock == null)
        {
            mpb_PropertyBlock = new MaterialPropertyBlock();
        }

        CacheVATMaterialProperty();

        SetDisplayFrame(0.0f);
    }

    /// <summary>
    /// VAT再生開始処理です。
    /// </summary>
    protected override void PlayEffectProcess()
    {
        b_IsPlaying = true;
        b_IsFinished = false;
        f_CurrentPlayTime = 0.0f;

        SetRenderersVisible(true);

        // 再生開始時に必ず最初のFrameへ戻します。
        SetDisplayFrame(0.0f);
    }

    /// <summary>
    /// VAT終了処理です。
    /// </summary>
    protected override void EndEffectProcess()
    {
        FinishVATEffect();
    }

    private void Update()
    {
        if (b_IsPlaying == false)
        {
            return;
        }

        float f_PlayTime = GetPlayTime();

        if (f_PlayTime <= 0.0f)
        {
            SetDisplayFrame(GetLastFrame());
            FinishVATEffect();
            return;
        }

        f_CurrentPlayTime += Time.deltaTime;

        float f_NormalizedTime = f_CurrentPlayTime / f_PlayTime;

        if (IsLoop())
        {
            f_NormalizedTime = Mathf.Repeat(f_NormalizedTime, 1.0f);
        }
        else
        {
            f_NormalizedTime = Mathf.Clamp01(f_NormalizedTime);
        }

        float f_DisplayFrame = Mathf.Lerp(
            0.0f,
            GetLastFrame(),
            f_NormalizedTime);

        SetDisplayFrame(f_DisplayFrame);

        if (IsLoop() == false && f_CurrentPlayTime >= f_PlayTime)
        {
            SetDisplayFrame(GetLastFrame());
            FinishVATEffect();
        }
    }

    /// <summary>
    /// VATのMaterial Propertyを保存します。
    /// </summary>
    private void CacheVATMaterialProperty()
    {
        n_DisplayFramePropertyId = -1;
        n_FrameCountPropertyId = -1;
        n_AutoPlaybackPropertyId = -1;
        f_FrameCount = 1.0f;

        Material material = GetFirstMaterial();

        if (material == null || material.shader == null)
        {
            return;
        }

        Shader shader = material.shader;
        int propertyCount = shader.GetPropertyCount();

        for (int i = 0 ; i < propertyCount ; i++)
        {
            ShaderPropertyType propertyType = shader.GetPropertyType(i);

            if (propertyType != ShaderPropertyType.Float &&
                propertyType != ShaderPropertyType.Range)
            {
                continue;
            }

            string propertyName = shader.GetPropertyName(i);
            string displayName = shader.GetPropertyDescription(i);

            string lowerPropertyName = propertyName.ToLowerInvariant();
            string lowerDisplayName = displayName.ToLowerInvariant();

            if (n_DisplayFramePropertyId < 0 &&
                IsDisplayFrameProperty(lowerPropertyName, lowerDisplayName))
            {
                n_DisplayFramePropertyId = Shader.PropertyToID(propertyName);
                continue;
            }

            if (n_FrameCountPropertyId < 0 &&
                IsFrameCountProperty(lowerPropertyName, lowerDisplayName))
            {
                n_FrameCountPropertyId = Shader.PropertyToID(propertyName);
                continue;
            }

            if (n_AutoPlaybackPropertyId < 0 &&
                IsAutoPlaybackProperty(lowerPropertyName, lowerDisplayName))
            {
                n_AutoPlaybackPropertyId = Shader.PropertyToID(propertyName);
            }
        }

        if (n_FrameCountPropertyId >= 0)
        {
            f_FrameCount = Mathf.Max(1.0f, material.GetFloat(n_FrameCountPropertyId));
        }

        // 念のためAuto PlaybackをPropertyBlockでも0にします。
        // Material側のAuto PlaybackチェックもOFFにしてください。
        SetAutoPlayback(false);
    }

    /// <summary>
    /// 最初のMaterialを取得します。
    /// </summary>
    private Material GetFirstMaterial()
    {
        if (rd_ChildRenderers == null)
        {
            return null;
        }

        for (int i = 0 ; i < rd_ChildRenderers.Length ; i++)
        {
            Renderer renderer = rd_ChildRenderers[i];

            if (renderer == null)
            {
                continue;
            }

            Material[] materials = renderer.sharedMaterials;

            for (int j = 0 ; j < materials.Length ; j++)
            {
                if (materials[j] != null)
                {
                    return materials[j];
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Display Frame Propertyか判定します。
    /// </summary>
    private bool IsDisplayFrameProperty(string str_PropertyName, string str_DisplayName)
    {
        if (str_DisplayName.Contains("display frame"))
        {
            return true;
        }

        if (str_PropertyName.Contains("display") &&
            str_PropertyName.Contains("frame"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Frame Count Propertyか判定します。
    /// </summary>
    private bool IsFrameCountProperty(string str_PropertyName, string str_DisplayName)
    {
        if (str_DisplayName.Contains("frame count"))
        {
            return true;
        }

        if (str_PropertyName.Contains("frame") &&
            str_PropertyName.Contains("count"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Auto Playback Propertyか判定します。
    /// </summary>
    private bool IsAutoPlaybackProperty(string str_PropertyName, string str_DisplayName)
    {
        if (str_DisplayName.Contains("auto playback"))
        {
            return true;
        }

        if (str_PropertyName.Contains("auto") &&
            str_PropertyName.Contains("playback"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// VATの1回再生に使う時間を取得します。
    /// </summary>
    private float GetPlayTime()
    {
        if (csst_EffectPlayData.f_PlayTime.HasValue)
        {
            return Mathf.Max(0.0f, csst_EffectPlayData.f_PlayTime.Value);
        }

        if (csst_EffectPlayData.f_PlayEndTime.HasValue)
        {
            return Mathf.Max(0.0f, csst_EffectPlayData.f_PlayEndTime.Value);
        }

        return 1.0f;
    }

    /// <summary>
    /// 最終Frameを取得します。
    /// </summary>
    private float GetLastFrame()
    {
        return Mathf.Max(0.0f, f_FrameCount - 1.0f);
    }

    /// <summary>
    /// ループ再生するか取得します。
    /// </summary>
    private bool IsLoop()
    {
        if (csst_EffectPlayData.b_LoopFlag.HasValue == false)
        {
            return false;
        }

        return csst_EffectPlayData.b_LoopFlag.Value;
    }

    /// <summary>
    /// 終了時に非表示にするか取得します。
    /// </summary>
    private bool IsHideOnEnd()
    {
        if (csst_EffectPlayData.b_HideOnEnd.HasValue == false)
        {
            return true;
        }

        return csst_EffectPlayData.b_HideOnEnd.Value;
    }

    /// <summary>
    /// Display FrameをMaterialへ反映します。
    /// </summary>
    private void SetDisplayFrame(float f_DisplayFrame)
    {
        if (rd_ChildRenderers == null)
        {
            return;
        }

        if (n_DisplayFramePropertyId < 0)
        {
            if (b_IsLoggedMissingProperty == false)
            {
                b_IsLoggedMissingProperty = true;
                Debug.LogWarning("[CS_EffectVAT] Display Frame Propertyが見つかりません。MaterialのShader Property名を確認してください。");
            }

            return;
        }

        for (int i = 0 ; i < rd_ChildRenderers.Length ; i++)
        {
            Renderer renderer = rd_ChildRenderers[i];

            if (renderer == null)
            {
                continue;
            }

            Material[] materials = renderer.sharedMaterials;

            for (int j = 0 ; j < materials.Length ; j++)
            {
                Material material = materials[j];

                if (material == null)
                {
                    continue;
                }

                if (material.HasProperty(n_DisplayFramePropertyId) == false)
                {
                    continue;
                }

                renderer.GetPropertyBlock(mpb_PropertyBlock, j);
                mpb_PropertyBlock.SetFloat(n_DisplayFramePropertyId, f_DisplayFrame);
                renderer.SetPropertyBlock(mpb_PropertyBlock, j);
            }
        }
    }

    /// <summary>
    /// Auto Playbackを切り替えます。
    /// </summary>
    private void SetAutoPlayback(bool b_IsAutoPlayback)
    {
        if (rd_ChildRenderers == null)
        {
            return;
        }

        if (n_AutoPlaybackPropertyId < 0)
        {
            return;
        }

        float value = b_IsAutoPlayback ? 1.0f : 0.0f;

        for (int i = 0 ; i < rd_ChildRenderers.Length ; i++)
        {
            Renderer renderer = rd_ChildRenderers[i];

            if (renderer == null)
            {
                continue;
            }

            Material[] materials = renderer.sharedMaterials;

            for (int j = 0 ; j < materials.Length ; j++)
            {
                Material material = materials[j];

                if (material == null)
                {
                    continue;
                }

                if (material.HasProperty(n_AutoPlaybackPropertyId) == false)
                {
                    continue;
                }

                renderer.GetPropertyBlock(mpb_PropertyBlock, j);
                mpb_PropertyBlock.SetFloat(n_AutoPlaybackPropertyId, value);
                renderer.SetPropertyBlock(mpb_PropertyBlock, j);
            }
        }
    }

    /// <summary>
    /// Renderer表示状態を切り替えます。
    /// </summary>
    private void SetRenderersVisible(bool b_IsVisible)
    {
        if (rd_ChildRenderers == null)
        {
            return;
        }

        for (int i = 0 ; i < rd_ChildRenderers.Length ; i++)
        {
            Renderer renderer = rd_ChildRenderers[i];

            if (renderer == null)
            {
                continue;
            }

            renderer.enabled = b_IsVisible;
        }
    }

    /// <summary>
    /// VATを完全終了します。
    /// </summary>
    private void FinishVATEffect()
    {
        if (b_IsFinished)
        {
            return;
        }

        b_IsFinished = true;
        b_IsPlaying = false;

        SetDisplayFrame(GetLastFrame());

        if (IsHideOnEnd())
        {
            SetRenderersVisible(false);
        }

        FinishEndEffect();
    }
}
