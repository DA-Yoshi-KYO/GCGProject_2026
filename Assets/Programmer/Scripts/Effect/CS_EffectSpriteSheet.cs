using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectSpriteSheet.cs
 概要     : SpriteSheet形式のEffectを制御する共通クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/11 新規作成
=====================================+
*/

/// <summary>
/// SpriteSheet形式のEffectを制御する共通クラスです。
/// 同じGameObjectについているSpriteRendererのSpriteを差し替えてアニメーション再生します。
/// </summary>
public abstract class CS_EffectSpriteSheet : CSAD_EffectCommonProcessBase
{
    /// <summary>
    /// このGameObjectについているSpriteRendererです。
    /// Inspectorには出さず、自動取得します。
    /// </summary>
    protected SpriteRenderer sr_EffectSpriteRenderer;

    [Header("再生するSprite一覧")]
    [SerializeField]
    protected Sprite[] sp_EffectSprites;

    [Header("Billboard設定")]
    [SerializeField]
    protected bool b_IsBillboard = false;

    /// <summary>
    /// Billboardで向く対象Transformです。
    /// nullの場合はMainCameraを使用します。
    /// </summary>
    protected Transform tr_BillboardTarget;

    /// <summary>
    /// Billboard対象を外部から設定します。
    /// </summary>
    /// <param name="tr_Target">Billboard対象Transform。</param>
    public void SetBillboardTarget(Transform tr_Target)
    {
        tr_BillboardTarget = tr_Target;
    }

    /// <summary>
    /// 現在の再生時間です。
    /// </summary>
    protected float f_CurrentPlayTime;

    /// <summary>
    /// 再生中かどうかです。
    /// </summary>
    protected bool b_IsPlaying;

    /// <summary>
    /// 現在表示しているSprite番号です。
    /// </summary>
    protected int n_CurrentSpriteIndex;

    /// <summary>
    /// 初期化処理です。
    /// </summary>
    public override void InitEffect()
    {
        sr_EffectSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 更新処理です。
    /// </summary>
    protected virtual void Update()
    {
        if (b_IsBillboard)
        {
            UpdateBillboard();
        }

        if (b_IsPlaying == false)
        {
            return;
        }

        UpdateSpriteSheet();
    }

    /// <summary>
    /// Billboard処理です。
    /// </summary>
    protected void UpdateBillboard()
    {
        // 外部から明示的に指定されている場合は、そのTransformを優先します。
        Transform tr_TargetCamera = tr_BillboardTarget;

        // 外部指定がない場合は、
        // キャッシュされている現在有効なMainCameraを使用します。
        if (tr_TargetCamera == null)
        {
            tr_TargetCamera =
                CS_BillboardCameraCache.GetActiveMainCameraTransform();
        }

        if (tr_TargetCamera == null)
        {
            return;
        }

        Vector3 v3_DirectionToCamera =
            transform.position - tr_TargetCamera.position;

        if (v3_DirectionToCamera.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        transform.rotation =
            Quaternion.LookRotation(
                v3_DirectionToCamera.normalized);
    }

    /// <summary>
    /// SpriteSheetEffectの再生処理です。
    /// </summary>
    protected override void PlayEffectProcess()
    {
        if (sr_EffectSpriteRenderer == null)
        {
            Debug.LogWarning("[CS_EffectSpriteSheet] 同じGameObjectにSpriteRendererがありません : " + gameObject.name);
            return;
        }

        if (sp_EffectSprites == null || sp_EffectSprites.Length <= 0)
        {
            Debug.LogWarning("[CS_EffectSpriteSheet] Sprite一覧が設定されていません : " + gameObject.name);
            return;
        }

        b_IsPlaying = true;
        f_CurrentPlayTime = 0.0f;
        n_CurrentSpriteIndex = 0;

        sr_EffectSpriteRenderer.sprite = sp_EffectSprites[0];

        OnSpriteSheetPlayStart();
        OnSpriteFrameChanged(0);
    }

    /// <summary>
    /// SpriteSheetの更新処理です。
    /// </summary>
    private void UpdateSpriteSheet()
    {
        if (sp_EffectSprites == null || sp_EffectSprites.Length <= 0)
        {
            return;
        }

        float f_PlayTime = GetPlayTime();

        if (f_PlayTime <= 0.0f)
        {
            SetSpriteFrame(sp_EffectSprites.Length - 1);
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

        int int_SpriteIndex = Mathf.FloorToInt(f_NormalizedTime * sp_EffectSprites.Length);

        if (int_SpriteIndex >= sp_EffectSprites.Length)
        {
            int_SpriteIndex = sp_EffectSprites.Length - 1;
        }

        if (int_SpriteIndex < 0)
        {
            int_SpriteIndex = 0;
        }

        SetSpriteFrame(int_SpriteIndex);

        if (IsLoop() == false && f_CurrentPlayTime >= f_PlayTime)
        {
            b_IsPlaying = false;
            SetSpriteFrame(sp_EffectSprites.Length - 1);
            OnSpriteSheetPlayComplete();
        }
    }

    /// <summary>
    /// 指定番号のSpriteを表示します。
    /// </summary>
    /// <param name="int_SpriteIndex">表示するSprite番号。</param>
    private void SetSpriteFrame(int int_SpriteIndex)
    {
        if (n_CurrentSpriteIndex == int_SpriteIndex)
        {
            return;
        }

        n_CurrentSpriteIndex = int_SpriteIndex;
        sr_EffectSpriteRenderer.sprite = sp_EffectSprites[n_CurrentSpriteIndex];

        OnSpriteFrameChanged(n_CurrentSpriteIndex);
    }

    /// <summary>
    /// 再生時間を取得します。
    /// </summary>
    /// <returns>再生時間。</returns>
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
    /// <returns>ループする場合はtrue。</returns>
    private bool IsLoop()
    {
        if (csst_EffectPlayData.b_LoopFlag.HasValue)
        {
            return csst_EffectPlayData.b_LoopFlag.Value;
        }

        return false;
    }

    /// <summary>
    /// SpriteSheetEffectの終了処理です。
    /// </summary>
    protected override void EndEffectProcess()
    {
        b_IsPlaying = false;

        OnSpriteSheetEnd();

        FinishEndEffect();
    }

    /// <summary>
    /// SpriteSheet再生開始時の処理です。
    /// 必要な場合だけ継承先で上書きします。
    /// </summary>
    protected virtual void OnSpriteSheetPlayStart()
    {

    }

    /// <summary>
    /// Spriteのフレームが変わった時の処理です。
    /// 必要な場合だけ継承先で上書きします。
    /// </summary>
    /// <param name="int_SpriteIndex">現在のSprite番号。</param>
    protected virtual void OnSpriteFrameChanged(int int_SpriteIndex)
    {

    }

    /// <summary>
    /// SpriteSheetを最後まで再生したときの処理です。
    /// 自動終了時間が設定されていない場合は、その場で終了します。
    /// </summary>
    protected virtual void OnSpriteSheetPlayComplete()
    {
        // 自動終了が設定されている場合は、
        // CSAD_EffectCommonProcessBase側のTimerに任せます。
        if (csst_EffectPlayData.f_PlayEndTime.HasValue)
        {
            return;
        }

        EndEffect();
    }

    /// <summary>
    /// SpriteSheet終了時の処理です。
    /// 必要な場合だけ継承先で上書きします。
    /// </summary>
    protected virtual void OnSpriteSheetEnd()
    {

    }
}
