using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectVolumeTexture.cs
 概要       : 連番Volume Texture Effect再生クラス
 作者       : ヨシモト リョウ
 履歴       : 2026/07/14 新規作成
              2026/07/14 連番自動取得追加
              2026/07/14 フレーム補間追加
=====================================+
*/

/// <summary>
/// Houdiniから書き出した連番Volume Textureを再生するEffectです。
/// 現在フレームと次フレームをShader側で補間して再生します。
/// </summary>
public class CS_EffectVolumeTexture :
    CSAD_EffectCommonProcessBase
{
    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
    // Shader Property
    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝

    /// <summary>
    /// 現在フレームのVolume Atlasです。
    /// </summary>
    private static readonly int int_VolumeAtlasCurrentPropertyId =
        Shader.PropertyToID("_VolumeAtlasCurrent");

    /// <summary>
    /// 次フレームのVolume Atlasです。
    /// </summary>
    private static readonly int int_VolumeAtlasNextPropertyId =
        Shader.PropertyToID("_VolumeAtlasNext");

    /// <summary>
    /// 現在フレームと次フレームの補間率です。
    /// </summary>
    private static readonly int int_FrameBlendPropertyId =
        Shader.PropertyToID("_FrameBlend");

    /// <summary>
    /// Effect全体の透明度です。
    /// </summary>
    private static readonly int int_GlobalAlphaPropertyId =
        Shader.PropertyToID("_GlobalAlpha");


    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
    // Inspector設定
    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝

    [Header("Volumeを描画するRenderer")]
    [SerializeField]
    private Renderer rd_VolumeRenderer;

    [Header("Volume連番自動取得設定")]

    [Tooltip("このフォルダ直下にある連番Textureを自動取得します。")]
    [SerializeField]
    private string str_VolumeFrameFolderPath =
        "Assets/Effect/Effects/Smoke/Volume";

    [Tooltip("この文字列から始まるTextureだけを取得します。")]
    [SerializeField]
    private string str_VolumeFrameNamePrefix =
        "RunSmoke.";

    [Tooltip("Inspector更新時に連番Textureを自動取得します。")]
    [SerializeField]
    private bool b_AutoCollectVolumeFrames = true;

    [Header("自動取得されたVolume連番")]
    [SerializeField]
    private Texture2D[] tex_VolumeFrames;

    [Header("再生時間未指定時のFPS")]
    [SerializeField, Min(1.0f)]
    private float f_DefaultFramesPerSecond = 24.0f;

    [Header("制御するMaterial番号")]
    [SerializeField, Min(0)]
    private int n_MaterialIndex = 0;

    [Header("再生開始時にTexture名で並び替えるか")]
    [SerializeField]
    private bool b_SortFramesByName = true;

    [Header("Volume移動設定")]

    [Tooltip("実際に表示されている子のVolumeMeshです。")]
    [SerializeField]
    private Transform tr_VolumeMesh;

    [Tooltip("1秒間に移動するローカル座標の速度です。")]
    [SerializeField]
    private Vector3 v3_LocalMoveSpeed =
        new Vector3(
            0.0f,
            -0.03f,
            -0.05f
        );

    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
    // 内部データ
    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝

    /// <summary>
    /// Rendererへ値を渡すPropertyBlockです。
    /// </summary>
    private MaterialPropertyBlock mpb_PropertyBlock;

    /// <summary>
    /// 通常再生中かどうかです。
    /// </summary>
    private bool b_IsPlaying;

    /// <summary>
    /// 終了フェード中かどうかです。
    /// </summary>
    private bool b_IsEnding;

    /// <summary>
    /// 終了完了済みかどうかです。
    /// </summary>
    private bool b_IsFinished;

    /// <summary>
    /// 設定不備で次のUpdate時に終了するかどうかです。
    /// </summary>
    private bool b_RequestEndOnNextUpdate;

    /// <summary>
    /// 設定不備による終了かどうかです。
    /// </summary>
    private bool b_IsSettingInvalid;

    /// <summary>
    /// 現在の再生経過時間です。
    /// </summary>
    private float f_CurrentPlayTime;

    /// <summary>
    /// 現在の終了経過時間です。
    /// </summary>
    private float f_CurrentEndTime;

    /// <summary>
    /// 終了開始時点のAlphaです。
    /// </summary>
    private float f_EndStartAlpha;

    /// <summary>
    /// 現在のAlphaです。
    /// </summary>
    private float f_CurrentAlpha;

    /// <summary>
    /// Shaderへ設定済みの現在フレーム番号です。
    /// </summary>
    private int n_CurrentFrameIndex = -1;

    /// <summary>
    /// Shaderへ設定済みの次フレーム番号です。
    /// </summary>
    private int n_NextFrameIndex = -1;

    /// <summary>
    /// VolumeMeshのPrefab時のLocalPositionです。
    /// </summary>
    private Vector3 v3_DefaultVolumeMeshLocalPosition;

    /// <summary>
    /// 初期LocalPositionを保存済みかどうかです。
    /// </summary>
    private bool b_IsDefaultVolumeMeshPositionCached;

    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
    // Unity / Effect共通処理
    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝

    /// <summary>
    /// Effect初期化処理です。
    /// </summary>
    public override void InitEffect()
    {
        if (rd_VolumeRenderer == null)
        {
            rd_VolumeRenderer =
                GetComponentInChildren<Renderer>(true);
        }

        if (mpb_PropertyBlock == null)
        {
            mpb_PropertyBlock =
                new MaterialPropertyBlock();
        }

        if (b_SortFramesByName)
        {
            SortVolumeFrames();
        }

        if (tr_VolumeMesh == null &&
            rd_VolumeRenderer != null)
        {
            tr_VolumeMesh =
                rd_VolumeRenderer.transform;
        }

        if (tr_VolumeMesh != null &&
            b_IsDefaultVolumeMeshPositionCached == false)
        {
            v3_DefaultVolumeMeshLocalPosition =
                tr_VolumeMesh.localPosition;

            b_IsDefaultVolumeMeshPositionCached = true;
        }

        ResetVolumeMeshPosition();

        n_CurrentFrameIndex = -1;
        n_NextFrameIndex = -1;

        ApplyGlobalAlpha(0.0f);

        if (HasVolumeFrames())
        {
            ApplyFrameBlend(
                0,
                Mathf.Min(
                    1,
                    tex_VolumeFrames.Length - 1
                ),
                0.0f
            );
        }
    }

    /// <summary>
    /// Effect再生開始処理です。
    /// </summary>
    protected override void PlayEffectProcess()
    {
        ResetVolumeMeshPosition();

        b_IsPlaying = false;
        b_IsEnding = false;
        b_IsFinished = false;

        b_RequestEndOnNextUpdate = false;
        b_IsSettingInvalid = false;

        f_CurrentPlayTime = 0.0f;
        f_CurrentEndTime = 0.0f;

        f_EndStartAlpha = 1.0f;
        f_CurrentAlpha = 0.0f;

        n_CurrentFrameIndex = -1;
        n_NextFrameIndex = -1;

        if (ValidateSetting() == false)
        {
            b_IsSettingInvalid = true;
            b_RequestEndOnNextUpdate = true;

            ApplyGlobalAlpha(0.0f);
            SetRendererVisible(false);

            return;
        }

        SetRendererVisible(true);

        ApplyGlobalAlpha(1.0f);

        ApplyFrameBlend(
            0,
            Mathf.Min(
                1,
                tex_VolumeFrames.Length - 1
            ),
            0.0f
        );

        b_IsPlaying = true;
    }

    /// <summary>
    /// Effect終了開始処理です。
    /// </summary>
    protected override void EndEffectProcess()
    {
        b_IsPlaying = false;

        if (b_IsSettingInvalid)
        {
            FinishVolumeTextureEffect();
            return;
        }

        float f_EndTime =
            GetEndTime();

        if (f_EndTime <= 0.0f)
        {
            ApplyGlobalAlpha(0.0f);
            FinishVolumeTextureEffect();
            return;
        }

        b_IsEnding = true;

        f_CurrentEndTime = 0.0f;
        f_EndStartAlpha = f_CurrentAlpha;
    }

    /// <summary>
    /// 更新処理です。
    /// </summary>
    private void Update()
    {
        if (b_RequestEndOnNextUpdate)
        {
            b_RequestEndOnNextUpdate = false;
            EndEffect();
            return;
        }

        // 再生中または終了フェード中は
        // VolumeMeshを少しずつ移動させます。
        if (b_IsPlaying || b_IsEnding)
        {
            UpdateVolumeMeshMove();
        }

        if (b_IsEnding)
        {
            UpdateEndEffect();
            return;
        }

        if (b_IsPlaying == false)
        {
            return;
        }

        UpdateVolumeTexture();
    }

    /// <summary>
    /// VolumeMeshを設定されたローカル速度で移動します。
    /// </summary>
    private void UpdateVolumeMeshMove()
    {
        if (tr_VolumeMesh == null)
        {
            return;
        }

        tr_VolumeMesh.localPosition +=
            v3_LocalMoveSpeed *
            Time.deltaTime;
    }

    /// <summary>
    /// VolumeMeshをPrefab時の位置へ戻します。
    /// </summary>
    private void ResetVolumeMeshPosition()
    {
        if (tr_VolumeMesh == null)
        {
            return;
        }

        if (b_IsDefaultVolumeMeshPositionCached == false)
        {
            v3_DefaultVolumeMeshLocalPosition =
                tr_VolumeMesh.localPosition;

            b_IsDefaultVolumeMeshPositionCached = true;
        }

        tr_VolumeMesh.localPosition =
            v3_DefaultVolumeMeshLocalPosition;
    }

    /// <summary>
    /// Objectが非表示になった時に内部再生状態を停止します。
    /// </summary>
    private void OnDisable()
    {
        b_IsPlaying = false;
        b_IsEnding = false;

        b_RequestEndOnNextUpdate = false;
        b_IsSettingInvalid = false;
    }


    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
    // Volume連番再生
    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝

    /// <summary>
    /// Volume Textureの連番を更新します。
    /// 現在フレームと次フレームをShader側で補間します。
    /// </summary>
    private void UpdateVolumeTexture()
    {
        if (HasVolumeFrames() == false)
        {
            EndEffect();
            return;
        }

        float f_PlayTime =
            GetPlayTime();

        if (f_PlayTime <= 0.0f)
        {
            int n_LastFrameIndex =
                tex_VolumeFrames.Length - 1;

            ApplyFrameBlend(
                n_LastFrameIndex,
                n_LastFrameIndex,
                0.0f
            );

            EndEffect();
            return;
        }

        f_CurrentPlayTime +=
            Time.deltaTime;

        bool b_IsLoop =
            IsLoop();

        float f_NormalizedTime =
            f_CurrentPlayTime /
            f_PlayTime;

        int n_CurrentIndex;
        int n_NextIndex;
        float f_FrameBlend;

        if (b_IsLoop)
        {
            f_NormalizedTime =
                Mathf.Repeat(
                    f_NormalizedTime,
                    1.0f
                );

            // Loop時は最後のフレームから
            // 最初のフレームにも補間します。
            float f_FramePosition =
                f_NormalizedTime *
                tex_VolumeFrames.Length;

            n_CurrentIndex =
                Mathf.FloorToInt(
                    f_FramePosition
                );

            n_CurrentIndex %=
                tex_VolumeFrames.Length;

            n_NextIndex =
                (
                    n_CurrentIndex +
                    1
                ) %
                tex_VolumeFrames.Length;

            f_FrameBlend =
                Mathf.Repeat(
                    f_FramePosition,
                    1.0f
                );
        }
        else
        {
            f_NormalizedTime =
                Mathf.Clamp01(
                    f_NormalizedTime
                );

            float f_FramePosition =
                f_NormalizedTime *
                Mathf.Max(
                    tex_VolumeFrames.Length - 1,
                    0
                );

            n_CurrentIndex =
                Mathf.FloorToInt(
                    f_FramePosition
                );

            n_CurrentIndex =
                Mathf.Clamp(
                    n_CurrentIndex,
                    0,
                    tex_VolumeFrames.Length - 1
                );

            n_NextIndex =
                Mathf.Min(
                    n_CurrentIndex + 1,
                    tex_VolumeFrames.Length - 1
                );

            f_FrameBlend =
                f_FramePosition -
                n_CurrentIndex;
        }

        ApplyFrameBlend(
            n_CurrentIndex,
            n_NextIndex,
            f_FrameBlend
        );

        if (b_IsLoop == false &&
            f_CurrentPlayTime >= f_PlayTime)
        {
            int n_LastFrameIndex =
                tex_VolumeFrames.Length - 1;

            ApplyFrameBlend(
                n_LastFrameIndex,
                n_LastFrameIndex,
                0.0f
            );

            EndEffect();
        }
    }

    /// <summary>
    /// 終了フェードを更新します。
    /// </summary>
    private void UpdateEndEffect()
    {
        float f_EndTime =
            GetEndTime();

        if (f_EndTime <= 0.0f)
        {
            ApplyGlobalAlpha(0.0f);
            FinishVolumeTextureEffect();
            return;
        }

        f_CurrentEndTime +=
            Time.deltaTime;

        float f_EndRate =
            Mathf.Clamp01(
                f_CurrentEndTime /
                f_EndTime
            );

        float f_Alpha =
            Mathf.Lerp(
                f_EndStartAlpha,
                0.0f,
                f_EndRate
            );

        ApplyGlobalAlpha(f_Alpha);

        if (f_CurrentEndTime >= f_EndTime)
        {
            ApplyGlobalAlpha(0.0f);
            FinishVolumeTextureEffect();
        }
    }

    /// <summary>
    /// 現在フレーム・次フレーム・補間率をShaderへ設定します。
    /// </summary>
    /// <param name="n_CurrentIndex">現在フレーム番号。</param>
    /// <param name="n_NextIndex">次フレーム番号。</param>
    /// <param name="f_BlendRate">補間率。</param>
    private void ApplyFrameBlend(
        int n_CurrentIndex,
        int n_NextIndex,
        float f_BlendRate)
    {
        if (rd_VolumeRenderer == null)
        {
            return;
        }

        if (HasVolumeFrames() == false)
        {
            return;
        }

        if (mpb_PropertyBlock == null)
        {
            mpb_PropertyBlock =
                new MaterialPropertyBlock();
        }

        n_CurrentIndex =
            Mathf.Clamp(
                n_CurrentIndex,
                0,
                tex_VolumeFrames.Length - 1
            );

        n_NextIndex =
            Mathf.Clamp(
                n_NextIndex,
                0,
                tex_VolumeFrames.Length - 1
            );

        Texture2D tex_CurrentFrame =
            tex_VolumeFrames[n_CurrentIndex];

        Texture2D tex_NextFrame =
            tex_VolumeFrames[n_NextIndex];

        if (tex_CurrentFrame == null ||
            tex_NextFrame == null)
        {
            return;
        }

        rd_VolumeRenderer.GetPropertyBlock(
            mpb_PropertyBlock,
            n_MaterialIndex
        );

        if (n_CurrentFrameIndex !=
            n_CurrentIndex)
        {
            mpb_PropertyBlock.SetTexture(
                int_VolumeAtlasCurrentPropertyId,
                tex_CurrentFrame
            );

            n_CurrentFrameIndex =
                n_CurrentIndex;
        }

        if (n_NextFrameIndex !=
            n_NextIndex)
        {
            mpb_PropertyBlock.SetTexture(
                int_VolumeAtlasNextPropertyId,
                tex_NextFrame
            );

            n_NextFrameIndex =
                n_NextIndex;
        }

        mpb_PropertyBlock.SetFloat(
            int_FrameBlendPropertyId,
            Mathf.Clamp01(f_BlendRate)
        );

        rd_VolumeRenderer.SetPropertyBlock(
            mpb_PropertyBlock,
            n_MaterialIndex
        );
    }

    /// <summary>
    /// Shaderへ全体透明度を設定します。
    /// </summary>
    /// <param name="f_Alpha">設定する透明度。</param>
    private void ApplyGlobalAlpha(
        float f_Alpha)
    {
        f_CurrentAlpha =
            Mathf.Clamp01(f_Alpha);

        if (rd_VolumeRenderer == null)
        {
            return;
        }

        if (mpb_PropertyBlock == null)
        {
            mpb_PropertyBlock =
                new MaterialPropertyBlock();
        }

        rd_VolumeRenderer.GetPropertyBlock(
            mpb_PropertyBlock,
            n_MaterialIndex
        );

        mpb_PropertyBlock.SetFloat(
            int_GlobalAlphaPropertyId,
            f_CurrentAlpha
        );

        rd_VolumeRenderer.SetPropertyBlock(
            mpb_PropertyBlock,
            n_MaterialIndex
        );
    }

    /// <summary>
    /// Rendererの表示状態を設定します。
    /// </summary>
    private void SetRendererVisible(
        bool b_IsVisible)
    {
        if (rd_VolumeRenderer == null)
        {
            return;
        }

        rd_VolumeRenderer.enabled =
            b_IsVisible;
    }

    /// <summary>
    /// Effect終了を完了します。
    /// </summary>
    private void FinishVolumeTextureEffect()
    {
        if (b_IsFinished)
        {
            return;
        }

        b_IsFinished = true;
        b_IsPlaying = false;
        b_IsEnding = false;

        ApplyGlobalAlpha(0.0f);

        if (IsHideOnEnd())
        {
            SetRendererVisible(false);
        }

        FinishEndEffect();
    }


    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
    // 設定確認
    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝

    /// <summary>
    /// 再生に必要な設定を確認します。
    /// </summary>
    private bool ValidateSetting()
    {
        if (rd_VolumeRenderer == null)
        {
            Debug.LogWarning(
                "[CS_EffectVolumeTexture] " +
                "Rendererが設定されていません : " +
                gameObject.name,
                this
            );

            return false;
        }

        Material[] ma_SharedMaterials =
            rd_VolumeRenderer.sharedMaterials;

        if (ma_SharedMaterials == null ||
            n_MaterialIndex < 0 ||
            n_MaterialIndex >=
            ma_SharedMaterials.Length)
        {
            Debug.LogWarning(
                "[CS_EffectVolumeTexture] " +
                "Material番号が範囲外です : " +
                gameObject.name,
                this
            );

            return false;
        }

        Material ma_VolumeMaterial =
            ma_SharedMaterials[n_MaterialIndex];

        if (ma_VolumeMaterial == null)
        {
            Debug.LogWarning(
                "[CS_EffectVolumeTexture] " +
                "Materialが設定されていません : " +
                gameObject.name,
                this
            );

            return false;
        }

        if (
            ma_VolumeMaterial.HasProperty(
                int_VolumeAtlasCurrentPropertyId
            ) == false ||
            ma_VolumeMaterial.HasProperty(
                int_VolumeAtlasNextPropertyId
            ) == false ||
            ma_VolumeMaterial.HasProperty(
                int_FrameBlendPropertyId
            ) == false ||
            ma_VolumeMaterial.HasProperty(
                int_GlobalAlphaPropertyId
            ) == false
        )
        {
            Debug.LogWarning(
                "[CS_EffectVolumeTexture] " +
                "Volume Texture用Shader Propertyがありません : " +
                gameObject.name,
                this
            );

            return false;
        }

        if (HasVolumeFrames() == false)
        {
            Debug.LogWarning(
                "[CS_EffectVolumeTexture] " +
                "Volume Frameが設定されていません : " +
                gameObject.name,
                this
            );

            return false;
        }

        for (int i = 0 ;
             i < tex_VolumeFrames.Length ;
             i++)
        {
            if (tex_VolumeFrames[i] != null)
            {
                continue;
            }

            Debug.LogWarning(
                "[CS_EffectVolumeTexture] " +
                "Volume Frameにnullがあります。Index : " +
                i,
                this
            );

            return false;
        }

        return true;
    }

    /// <summary>
    /// Volume Frameが1枚以上あるか確認します。
    /// </summary>
    private bool HasVolumeFrames()
    {
        return tex_VolumeFrames != null &&
               tex_VolumeFrames.Length > 0;
    }


    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
    // Effect再生データ取得
    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝

    /// <summary>
    /// 1ループ分の再生時間を取得します。
    /// </summary>
    private float GetPlayTime()
    {
        if (csst_EffectPlayData.f_PlayTime.HasValue)
        {
            return Mathf.Max(
                0.0f,
                csst_EffectPlayData.f_PlayTime.Value
            );
        }

        if (HasVolumeFrames() == false)
        {
            return 0.0f;
        }

        float f_FramesPerSecond =
            Mathf.Max(
                1.0f,
                f_DefaultFramesPerSecond
            );

        if (tex_VolumeFrames.Length <= 1)
        {
            return 1.0f /
                   f_FramesPerSecond;
        }

        int n_FrameIntervalCount =
            IsLoop()
            ? tex_VolumeFrames.Length
            : tex_VolumeFrames.Length - 1;

        return n_FrameIntervalCount /
               f_FramesPerSecond;
    }

    /// <summary>
    /// 終了フェード時間を取得します。
    /// </summary>
    private float GetEndTime()
    {
        if (csst_EffectPlayData.f_EndTime.HasValue ==
            false)
        {
            return 0.0f;
        }

        return Mathf.Max(
            0.0f,
            csst_EffectPlayData.f_EndTime.Value
        );
    }

    /// <summary>
    /// ループ再生するか確認します。
    /// </summary>
    private bool IsLoop()
    {
        if (csst_EffectPlayData.b_LoopFlag.HasValue ==
            false)
        {
            return false;
        }

        return csst_EffectPlayData.b_LoopFlag.Value;
    }

    /// <summary>
    /// 終了時に非表示にするか確認します。
    /// </summary>
    private bool IsHideOnEnd()
    {
        if (csst_EffectPlayData.b_HideOnEnd.HasValue ==
            false)
        {
            return true;
        }

        return csst_EffectPlayData.b_HideOnEnd.Value;
    }


    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
    // 連番ソート
    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝

    /// <summary>
    /// Volume Frameを末尾番号順に並び替えます。
    /// </summary>
    private void SortVolumeFrames()
    {
        if (HasVolumeFrames() == false)
        {
            return;
        }

        Array.Sort(
            tex_VolumeFrames,
            CompareVolumeFrameTexture
        );
    }

    /// <summary>
    /// Texture名末尾のFrame番号で比較します。
    /// </summary>
    private static int CompareVolumeFrameTexture(
        Texture2D tex_Left,
        Texture2D tex_Right)
    {
        if (tex_Left == null &&
            tex_Right == null)
        {
            return 0;
        }

        if (tex_Left == null)
        {
            return 1;
        }

        if (tex_Right == null)
        {
            return -1;
        }

        int n_LeftFrameNumber =
            GetTrailingFrameNumber(
                tex_Left.name
            );

        int n_RightFrameNumber =
            GetTrailingFrameNumber(
                tex_Right.name
            );

        int n_FrameCompare =
            n_LeftFrameNumber.CompareTo(
                n_RightFrameNumber
            );

        if (n_FrameCompare != 0)
        {
            return n_FrameCompare;
        }

        return string.CompareOrdinal(
            tex_Left.name,
            tex_Right.name
        );
    }

    /// <summary>
    /// 名前末尾にある連続した数字を取得します。
    /// </summary>
    private static int GetTrailingFrameNumber(
        string str_TextureName)
    {
        if (string.IsNullOrEmpty(
                str_TextureName))
        {
            return int.MaxValue;
        }

        int n_NumberStartIndex =
            str_TextureName.Length;

        while (n_NumberStartIndex > 0)
        {
            char c_CurrentCharacter =
                str_TextureName[
                    n_NumberStartIndex - 1
                ];

            if (char.IsDigit(
                    c_CurrentCharacter) == false)
            {
                break;
            }

            n_NumberStartIndex--;
        }

        if (n_NumberStartIndex >=
            str_TextureName.Length)
        {
            return int.MaxValue;
        }

        string str_FrameNumber =
            str_TextureName.Substring(
                n_NumberStartIndex
            );

        if (int.TryParse(
                str_FrameNumber,
                out int n_FrameNumber))
        {
            return n_FrameNumber;
        }

        return int.MaxValue;
    }


#if UNITY_EDITOR

    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
    // Editor用Volume連番自動取得
    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝

    /// <summary>
    /// Inspector変更時にVolume連番を自動取得します。
    /// </summary>
    private void OnValidate()
    {
        if (b_AutoCollectVolumeFrames == false)
        {
            return;
        }

        if (Application.isPlaying)
        {
            return;
        }

        CollectVolumeFramesFromFolder();
    }

    /// <summary>
    /// 指定フォルダからVolume連番を再取得します。
    /// </summary>
    [ContextMenu("Volume連番をフォルダから再取得")]
    private void CollectVolumeFramesFromFolder()
    {
        string str_NormalizedFolderPath =
            NormalizeAssetFolderPath(
                str_VolumeFrameFolderPath
            );

        if (UnityEditor.AssetDatabase.IsValidFolder(
                str_NormalizedFolderPath) == false)
        {
            Debug.LogWarning(
                "[CS_EffectVolumeTexture] " +
                "Volume連番フォルダが見つかりません : " +
                str_NormalizedFolderPath,
                this
            );

            return;
        }

        string[] str_TextureGuids =
            UnityEditor.AssetDatabase.FindAssets(
                "t:Texture2D",
                new string[]
                {
                    str_NormalizedFolderPath
                }
            );

        List<Texture2D> list_VolumeFrames =
            new List<Texture2D>();

        for (int i = 0 ;
             i < str_TextureGuids.Length ;
             i++)
        {
            string str_AssetPath =
                UnityEditor.AssetDatabase
                    .GUIDToAssetPath(
                        str_TextureGuids[i]
                    );

            if (IsTextureInTargetFolder(
                    str_AssetPath,
                    str_NormalizedFolderPath) ==
                false)
            {
                continue;
            }

            Texture2D tex_VolumeFrame =
                UnityEditor.AssetDatabase
                    .LoadAssetAtPath<Texture2D>(
                        str_AssetPath
                    );

            if (tex_VolumeFrame == null)
            {
                continue;
            }

            if (
                string.IsNullOrEmpty(
                    str_VolumeFrameNamePrefix
                ) == false &&
                tex_VolumeFrame.name.StartsWith(
                    str_VolumeFrameNamePrefix,
                    StringComparison.OrdinalIgnoreCase
                ) == false
            )
            {
                continue;
            }

            list_VolumeFrames.Add(
                tex_VolumeFrame
            );
        }

        list_VolumeFrames.Sort(
            CompareVolumeFrameTexture
        );

        if (IsSameVolumeFrameArray(
                list_VolumeFrames))
        {
            return;
        }

        tex_VolumeFrames =
            list_VolumeFrames.ToArray();

        UnityEditor.EditorUtility.SetDirty(
            this
        );

        if (UnityEditor.PrefabUtility
            .IsPartOfPrefabInstance(this))
        {
            UnityEditor.PrefabUtility
                .RecordPrefabInstancePropertyModifications(
                    this
                );
        }

        Debug.Log(
            "[CS_EffectVolumeTexture] " +
            "Volume連番を自動設定しました。件数 : " +
            tex_VolumeFrames.Length +
            " / Folder : " +
            str_NormalizedFolderPath,
            this
        );
    }

    /// <summary>
    /// Unity用Assetパスへ正規化します。
    /// </summary>
    private static string NormalizeAssetFolderPath(
        string str_FolderPath)
    {
        if (string.IsNullOrWhiteSpace(
                str_FolderPath))
        {
            return string.Empty;
        }

        return str_FolderPath
            .Replace("\\", "/")
            .TrimEnd('/');
    }

    /// <summary>
    /// Textureが対象フォルダ直下にあるか確認します。
    /// </summary>
    private static bool IsTextureInTargetFolder(
        string str_AssetPath,
        string str_TargetFolderPath)
    {
        string str_AssetDirectory =
            Path.GetDirectoryName(
                str_AssetPath
            );

        if (string.IsNullOrEmpty(
                str_AssetDirectory))
        {
            return false;
        }

        str_AssetDirectory =
            str_AssetDirectory.Replace(
                "\\",
                "/"
            );

        return string.Equals(
            str_AssetDirectory,
            str_TargetFolderPath,
            StringComparison.OrdinalIgnoreCase
        );
    }

    /// <summary>
    /// 現在の配列と新しい取得結果が同一か確認します。
    /// </summary>
    private bool IsSameVolumeFrameArray(
        List<Texture2D> list_NewVolumeFrames)
    {
        if (list_NewVolumeFrames == null)
        {
            return tex_VolumeFrames == null ||
                   tex_VolumeFrames.Length == 0;
        }

        if (tex_VolumeFrames == null)
        {
            return list_NewVolumeFrames.Count == 0;
        }

        if (tex_VolumeFrames.Length !=
            list_NewVolumeFrames.Count)
        {
            return false;
        }

        for (int i = 0 ;
             i < tex_VolumeFrames.Length ;
             i++)
        {
            if (tex_VolumeFrames[i] !=
                list_NewVolumeFrames[i])
            {
                return false;
            }
        }

        return true;
    }

#endif
}
