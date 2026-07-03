using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectOverlapLoopSpriteSheet.cs
 概要     : SpriteSheetを重ねながらループ再生するEffectクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/07/03 新規作成
=====================================+
*/

/// <summary>
/// SpriteSheetを重ねながらループ再生するEffectです。
/// 例：30枚構成で25枚目に到達したら、上から1枚目を重ねて再生します。
/// </summary>
public class CS_EffectOverlapLoopSpriteSheet : CS_EffectSpriteSheet
{
    [Header("重ねループを使用するか")]
    [SerializeField]
    private bool b_UseOverlapLoop = true;

    [Header("何枚目から次ループを重ねるか")]
    [SerializeField, Min(2)]
    private int n_OverlapStartFrameNumber = 25;

    [Header("1ループの最後のフレーム番号")]
    [SerializeField, Min(1)]
    private int n_LoopEndFrameNumber = 30;

    [Header("上乗せSpriteRendererのSortingOrder加算値")]
    [SerializeField]
    private int n_OverlaySortingOrderOffset = 1;

    /// <summary>
    /// 上から重ねる用のSpriteRendererです。
    /// </summary>
    private SpriteRenderer sr_OverlaySpriteRenderer;

    /// <summary>
    /// 前回表示したメインSprite番号です。
    /// </summary>
    private int n_LastMainSpriteIndex = -1;

    /// <summary>
    /// 前回表示した上乗せSprite番号です。
    /// </summary>
    private int n_LastOverlaySpriteIndex = -1;

    /// <summary>
    /// 更新処理です。
    /// 親のUpdateSpriteSheetは通常ループ用なので、ここで重ねループ用に差し替えます。
    /// </summary>
    protected override void Update()
    {
        if (b_IsBillboard)
        {
            UpdateBillboardForOverlapLoop();
        }

        if (b_IsPlaying == false)
        {
            return;
        }

        if (CanUseOverlapLoop())
        {
            UpdateOverlapLoopSpriteSheet();
        }
        else
        {
            UpdateNormalSpriteSheet();
        }
    }

    /// <summary>
    /// SpriteSheet再生開始時の処理です。
    /// </summary>
    protected override void OnSpriteSheetPlayStart()
    {
        EnsureOverlaySpriteRenderer();

        n_LastMainSpriteIndex = -1;
        n_LastOverlaySpriteIndex = -1;

        HideOverlaySprite();
    }

    /// <summary>
    /// SpriteSheet終了時の処理です。
    /// </summary>
    protected override void OnSpriteSheetEnd()
    {
        HideOverlaySprite();

        n_LastMainSpriteIndex = -1;
        n_LastOverlaySpriteIndex = -1;
    }

    /// <summary>
    /// 重ねループが使える状態か確認します。
    /// </summary>
    /// <returns>使える場合はtrue。</returns>
    private bool CanUseOverlapLoop()
    {
        if (b_UseOverlapLoop == false)
        {
            return false;
        }

        if (IsLoop() == false)
        {
            return false;
        }

        if (sp_EffectSprites == null || sp_EffectSprites.Length <= 0)
        {
            return false;
        }

        int n_TotalFrameCount = GetTotalFrameCount();

        if (n_TotalFrameCount <= 1)
        {
            return false;
        }

        if (n_OverlapStartFrameNumber <= 1)
        {
            return false;
        }

        if (n_OverlapStartFrameNumber > n_TotalFrameCount)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 重ねループ用のSpriteSheet更新処理です。
    /// </summary>
    private void UpdateOverlapLoopSpriteSheet()
    {
        int n_TotalFrameCount = GetTotalFrameCount();
        float f_PlayTime = GetPlayTime();

        if (f_PlayTime <= 0.0f)
        {
            SetMainSpriteFrame(n_TotalFrameCount - 1);
            HideOverlaySprite();
            return;
        }

        f_CurrentPlayTime += Time.deltaTime;

        float f_FrameDuration = f_PlayTime / n_TotalFrameCount;

        if (f_FrameDuration <= 0.0f)
        {
            return;
        }

        int n_GlobalFrame = Mathf.FloorToInt(f_CurrentPlayTime / f_FrameDuration);

        // 25枚目で次の1枚目を重ねる場合、
        // 次ループ開始間隔は 25 - 1 = 24 フレーム。
        int n_CycleStartInterval = Mathf.Max(1, n_OverlapStartFrameNumber - 1);

        int n_CurrentCycleStartFrame =
            (n_GlobalFrame / n_CycleStartInterval) * n_CycleStartInterval;

        int n_PreviousCycleStartFrame =
            n_CurrentCycleStartFrame - n_CycleStartInterval;

        bool b_IsMainSet = false;
        bool b_IsOverlaySet = false;

        ApplyCycleFrame(
            n_PreviousCycleStartFrame,
            n_GlobalFrame,
            n_TotalFrameCount,
            ref b_IsMainSet,
            ref b_IsOverlaySet);

        ApplyCycleFrame(
            n_CurrentCycleStartFrame,
            n_GlobalFrame,
            n_TotalFrameCount,
            ref b_IsMainSet,
            ref b_IsOverlaySet);

        if (b_IsOverlaySet == false)
        {
            HideOverlaySprite();
        }
    }

    /// <summary>
    /// 指定サイクルの現在フレームを、メインまたは上乗せへ反映します。
    /// </summary>
    private void ApplyCycleFrame(
        int n_CycleStartFrame,
        int n_GlobalFrame,
        int n_TotalFrameCount,
        ref bool b_IsMainSet,
        ref bool b_IsOverlaySet)
    {
        if (n_CycleStartFrame < 0)
        {
            return;
        }

        int n_CycleAgeFrame = n_GlobalFrame - n_CycleStartFrame;

        if (n_CycleAgeFrame < 0)
        {
            return;
        }

        if (n_CycleAgeFrame >= n_TotalFrameCount)
        {
            return;
        }

        if (b_IsMainSet == false)
        {
            SetMainSpriteFrame(n_CycleAgeFrame);
            b_IsMainSet = true;
            return;
        }

        if (b_IsOverlaySet == false)
        {
            SetOverlaySpriteFrame(n_CycleAgeFrame);
            b_IsOverlaySet = true;
        }
    }

    /// <summary>
    /// 通常のSpriteSheet更新処理です。
    /// 非ループ時や重ねループ無効時はこちらを使います。
    /// </summary>
    private void UpdateNormalSpriteSheet()
    {
        if (sp_EffectSprites == null || sp_EffectSprites.Length <= 0)
        {
            return;
        }

        HideOverlaySprite();

        float f_PlayTime = GetPlayTime();

        if (f_PlayTime <= 0.0f)
        {
            SetMainSpriteFrame(sp_EffectSprites.Length - 1);
            return;
        }

        f_CurrentPlayTime += Time.deltaTime;

        float f_NormalizedTime = f_CurrentPlayTime / f_PlayTime;

        if (IsLoop())
        {
            f_NormalizedTime = f_NormalizedTime % 1.0f;
        }
        else
        {
            f_NormalizedTime = Mathf.Clamp01(f_NormalizedTime);
        }

        int n_SpriteIndex =
            Mathf.FloorToInt(f_NormalizedTime * sp_EffectSprites.Length);

        n_SpriteIndex = Mathf.Clamp(
            n_SpriteIndex,
            0,
            sp_EffectSprites.Length - 1);

        SetMainSpriteFrame(n_SpriteIndex);

        if (IsLoop() == false && f_CurrentPlayTime >= f_PlayTime)
        {
            b_IsPlaying = false;
            SetMainSpriteFrame(sp_EffectSprites.Length - 1);
            OnSpriteSheetPlayComplete();
        }
    }

    /// <summary>
    /// メインSpriteRendererへSpriteを設定します。
    /// </summary>
    private void SetMainSpriteFrame(int n_SpriteIndex)
    {
        if (sr_EffectSpriteRenderer == null)
        {
            return;
        }

        if (sp_EffectSprites == null || sp_EffectSprites.Length <= 0)
        {
            return;
        }

        n_SpriteIndex = Mathf.Clamp(
            n_SpriteIndex,
            0,
            sp_EffectSprites.Length - 1);

        if (n_LastMainSpriteIndex == n_SpriteIndex)
        {
            return;
        }

        n_LastMainSpriteIndex = n_SpriteIndex;
        n_CurrentSpriteIndex = n_SpriteIndex;

        sr_EffectSpriteRenderer.sprite = sp_EffectSprites[n_SpriteIndex];

        OnSpriteFrameChanged(n_SpriteIndex);
    }

    /// <summary>
    /// 上乗せSpriteRendererへSpriteを設定します。
    /// </summary>
    private void SetOverlaySpriteFrame(int n_SpriteIndex)
    {
        EnsureOverlaySpriteRenderer();

        if (sr_OverlaySpriteRenderer == null)
        {
            return;
        }

        if (sp_EffectSprites == null || sp_EffectSprites.Length <= 0)
        {
            return;
        }

        n_SpriteIndex = Mathf.Clamp(
            n_SpriteIndex,
            0,
            sp_EffectSprites.Length - 1);

        sr_OverlaySpriteRenderer.enabled = true;

        if (n_LastOverlaySpriteIndex == n_SpriteIndex)
        {
            return;
        }

        n_LastOverlaySpriteIndex = n_SpriteIndex;
        sr_OverlaySpriteRenderer.sprite = sp_EffectSprites[n_SpriteIndex];
    }

    /// <summary>
    /// 上乗せSpriteを非表示にします。
    /// </summary>
    private void HideOverlaySprite()
    {
        if (sr_OverlaySpriteRenderer == null)
        {
            return;
        }

        sr_OverlaySpriteRenderer.sprite = null;
        sr_OverlaySpriteRenderer.enabled = false;

        n_LastOverlaySpriteIndex = -1;
    }

    /// <summary>
    /// 上乗せ用SpriteRendererを用意します。
    /// </summary>
    private void EnsureOverlaySpriteRenderer()
    {
        if (sr_OverlaySpriteRenderer != null)
        {
            CopySpriteRendererSetting(
                sr_EffectSpriteRenderer,
                sr_OverlaySpriteRenderer);

            return;
        }

        Transform tr_Overlay = transform.Find("OverlaySpriteRenderer");

        if (tr_Overlay == null)
        {
            GameObject go_Overlay = new GameObject("OverlaySpriteRenderer");
            tr_Overlay = go_Overlay.transform;
            tr_Overlay.SetParent(transform, false);
        }

        sr_OverlaySpriteRenderer =
            tr_Overlay.GetComponent<SpriteRenderer>();

        if (sr_OverlaySpriteRenderer == null)
        {
            sr_OverlaySpriteRenderer =
                tr_Overlay.gameObject.AddComponent<SpriteRenderer>();
        }

        CopySpriteRendererSetting(
            sr_EffectSpriteRenderer,
            sr_OverlaySpriteRenderer);

        sr_OverlaySpriteRenderer.enabled = false;
    }

    /// <summary>
    /// メインSpriteRendererの設定を上乗せSpriteRendererへコピーします。
    /// </summary>
    private void CopySpriteRendererSetting(
        SpriteRenderer sr_Source,
        SpriteRenderer sr_Target)
    {
        if (sr_Source == null || sr_Target == null)
        {
            return;
        }

        sr_Target.sharedMaterial = sr_Source.sharedMaterial;
        sr_Target.color = sr_Source.color;
        sr_Target.flipX = sr_Source.flipX;
        sr_Target.flipY = sr_Source.flipY;

        sr_Target.sortingLayerID = sr_Source.sortingLayerID;
        sr_Target.sortingOrder =
            sr_Source.sortingOrder + n_OverlaySortingOrderOffset;

        sr_Target.maskInteraction = sr_Source.maskInteraction;
        sr_Target.drawMode = sr_Source.drawMode;
        sr_Target.size = sr_Source.size;
        sr_Target.tileMode = sr_Source.tileMode;
        sr_Target.spriteSortPoint = sr_Source.spriteSortPoint;

        sr_Target.transform.localPosition = Vector3.zero;
        sr_Target.transform.localRotation = Quaternion.identity;
        sr_Target.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 1ループで使うフレーム数を取得します。
    /// </summary>
    private int GetTotalFrameCount()
    {
        if (sp_EffectSprites == null || sp_EffectSprites.Length <= 0)
        {
            return 0;
        }

        return Mathf.Clamp(
            n_LoopEndFrameNumber,
            1,
            sp_EffectSprites.Length);
    }

    /// <summary>
    /// 再生時間を取得します。
    /// </summary>
    private float GetPlayTime()
    {
        if (csst_EffectPlayData.f_PlayTime.HasValue)
        {
            return csst_EffectPlayData.f_PlayTime.Value;
        }

        return 1.0f;
    }

    /// <summary>
    /// ループするかどうかを取得します。
    /// </summary>
    private bool IsLoop()
    {
        if (csst_EffectPlayData.b_LoopFlag.HasValue)
        {
            return csst_EffectPlayData.b_LoopFlag.Value;
        }

        return false;
    }

    /// <summary>
    /// Billboard処理です。
    /// </summary>
    private void UpdateBillboardForOverlapLoop()
    {
        if (tr_BillboardTarget == null)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                return;
            }

            tr_BillboardTarget = mainCamera.transform;
        }

        Vector3 v3_DirectionToCamera =
            transform.position - tr_BillboardTarget.position;

        if (v3_DirectionToCamera.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        transform.rotation =
            Quaternion.LookRotation(v3_DirectionToCamera.normalized);
    }
}
