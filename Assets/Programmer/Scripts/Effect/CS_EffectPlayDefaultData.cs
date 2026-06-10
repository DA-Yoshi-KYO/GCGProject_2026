/*
+=====================================
 ファイル名 : CS_EffectPlayDefaultData.cs
 概要     : Effect再生時のPrefab側Default値を保持するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
=====================================+
*/

using UnityEngine;

/// <summary>
/// Effect再生時のPrefab側Default値を保持するクラスです。
/// 呼び出し側のCSST_EffectPlayDataに値が入っていない場合、このDefault値を使用します。
/// </summary>
[System.Serializable]
public class CS_EffectPlayDefaultData
{
    [Header("ループ設定")]
    [SerializeField]
    private bool bool_DefaultLoopFlag = false;

    [Header("終了時の非表示設定")]
    [SerializeField]
    private bool bool_DefaultHideOnEnd = true;

    [Header("自動終了設定")]
    [SerializeField]
    private bool bool_IsAutoEnd = true;

    [SerializeField]
    private float f_AutoEndTime = 3.0f;

    /// <summary>
    /// 呼び出し側の再生データとPrefab側Default値を合成します。
    /// 呼び出し側に値がある場合は呼び出し側を優先します。
    /// </summary>
    /// <param name="csst_RequestData">呼び出し側のEffect再生データ。</param>
    /// <returns>Default値を反映したEffect再生データ。</returns>
    public CSST_EffectPlayData CreateMergedPlayData(CSST_EffectPlayData csst_RequestData)
    {
        CSST_EffectPlayData csst_ResultData = new CSST_EffectPlayData();
        csst_ResultData.CSST_EffectPlayData_Init();

        if (csst_RequestData.v3_Position.HasValue)
        {
            csst_ResultData.SetPosition(csst_RequestData.v3_Position.Value);
        }

        if (csst_RequestData.q_Rotation.HasValue)
        {
            csst_ResultData.SetRotation(csst_RequestData.q_Rotation.Value);
        }

        if (csst_RequestData.v3_Scale.HasValue)
        {
            csst_ResultData.SetScale(csst_RequestData.v3_Scale.Value);
        }

        if (csst_RequestData.f_PlayTime.HasValue)
        {
            csst_ResultData.SetPlayTime(csst_RequestData.f_PlayTime.Value);
        }

        if (csst_RequestData.f_EndTime.HasValue)
        {
            csst_ResultData.SetEndTime(csst_RequestData.f_EndTime.Value);
        }

        if (csst_RequestData.b_LoopFlag.HasValue)
        {
            csst_ResultData.SetLoopFlag(csst_RequestData.b_LoopFlag.Value);
        }
        else
        {
            csst_ResultData.SetLoopFlag(bool_DefaultLoopFlag);
        }

        if (csst_RequestData.b_HideOnEnd.HasValue)
        {
            csst_ResultData.SetHideOnEnd(csst_RequestData.b_HideOnEnd.Value);
        }
        else
        {
            csst_ResultData.SetHideOnEnd(bool_DefaultHideOnEnd);
        }

        if (csst_RequestData.f_PlayEndTime.HasValue)
        {
            csst_ResultData.SetPlayEndTime(csst_RequestData.f_PlayEndTime.Value);
        }
        else if (bool_IsAutoEnd)
        {
            csst_ResultData.SetPlayEndTime(f_AutoEndTime);
        }

        return csst_ResultData;
    }
}
