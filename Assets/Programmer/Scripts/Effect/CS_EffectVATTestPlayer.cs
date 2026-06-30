using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectVATTestPlayer.cs
 概要     : VATEffectのテスト再生クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/30 新規作成
=====================================+
*/

/// <summary>
/// VATEffectのテスト再生クラスです。
/// Pキーで再生、Oキーで終了、Rキーで再再生します。
/// </summary>
public class CS_EffectVATTestPlayer : MonoBehaviour
{
    [Header("Effect再生Facade")]
    [SerializeField]
    private CS_EffectPlayer cs_EffectPlayer;

    [Header("生成位置")]
    [SerializeField]
    private Transform tr_SpawnPoint;

    [Header("Start時に再生するか")]
    [SerializeField]
    private bool b_PlayOnStart = false;

    [Header("呼び出し側からPositionを指定するか")]
    [SerializeField]
    private bool b_SetPositionFromCaller = true;

    [Header("呼び出し側からRotationを指定するか")]
    [SerializeField]
    private bool b_SetRotationFromCaller = true;

    [Header("呼び出し側からScaleを指定するか")]
    [SerializeField]
    private bool b_SetScaleFromCaller = false;

    [SerializeField]
    private Vector3 v3_PlayScale = Vector3.one;

    [Header("呼び出し側から再生時間を指定するか")]
    [SerializeField]
    private bool b_SetPlayTimeFromCaller = true;

    [SerializeField]
    private float f_PlayTime = 1.0f;

    [Header("Loop指定")]
    [SerializeField]
    private bool b_SetLoopFromCaller = true;

    [SerializeField]
    private bool b_Loop = false;

    [Header("終了時に非表示にするか")]
    [SerializeField]
    private bool b_SetHideOnEndFromCaller = true;

    [SerializeField]
    private bool b_HideOnEnd = true;

    /// <summary>
    /// 現在再生中のEffectです。
    /// </summary>
    private CSAD_EffectCommonProcessBase csad_CurrentEffect;

    private void Start()
    {
        if (b_PlayOnStart)
        {
            PlayVATEffect();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayVATEffect();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            EndVATEffect();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ReplayVATEffect();
        }
    }

    /// <summary>
    /// VATEffectを再生します。
    /// </summary>
    private void PlayVATEffect()
    {
        if (cs_EffectPlayer == null)
        {
            Debug.LogWarning("[CS_EffectVATTestPlayer] CS_EffectPlayerが設定されていません。");
            return;
        }

        Vector3 v3_SpawnPosition = transform.position;
        Quaternion q_SpawnRotation = transform.rotation;

        if (tr_SpawnPoint != null)
        {
            v3_SpawnPosition = tr_SpawnPoint.position;
            q_SpawnRotation = tr_SpawnPoint.rotation;
        }

        CSST_EffectPlayData csst_EffectPlayData = new CSST_EffectPlayData();
        csst_EffectPlayData.CSST_EffectPlayData_Init();

        if (b_SetPositionFromCaller)
        {
            csst_EffectPlayData.SetPosition(v3_SpawnPosition);
        }

        if (b_SetRotationFromCaller)
        {
            csst_EffectPlayData.SetRotation(q_SpawnRotation);
        }

        if (b_SetScaleFromCaller)
        {
            csst_EffectPlayData.SetScale(v3_PlayScale);
        }

        if (b_SetPlayTimeFromCaller)
        {
            csst_EffectPlayData.SetPlayTime(f_PlayTime);
        }

        if (b_SetLoopFromCaller)
        {
            csst_EffectPlayData.SetLoopFlag(b_Loop);
        }

        if (b_SetHideOnEndFromCaller)
        {
            csst_EffectPlayData.SetHideOnEnd(b_HideOnEnd);
        }

        csad_CurrentEffect = cs_EffectPlayer.PlayEffect(csst_EffectPlayData);
    }

    /// <summary>
    /// VATEffectを終了します。
    /// </summary>
    private void EndVATEffect()
    {
        if (csad_CurrentEffect != null)
        {
            csad_CurrentEffect.EndEffect();
            return;
        }

        if (cs_EffectPlayer != null)
        {
            cs_EffectPlayer.EndCurrentEffect();
        }
    }

    /// <summary>
    /// VATEffectを最初から再再生します。
    /// </summary>
    private void ReplayVATEffect()
    {
        EndVATEffect();
        PlayVATEffect();
    }
}
