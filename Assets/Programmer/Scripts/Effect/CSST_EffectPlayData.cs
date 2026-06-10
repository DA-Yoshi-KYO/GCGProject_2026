/*
+=====================================
 ファイル名 : CSST_EffectPlayData.cs
 概要     : Effectの再生に必要なデータクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
=====================================+
*/

using UnityEngine;

/// <summary>
/// Effectの再生に必要なデータです。
/// 呼び出し側で必要な情報だけ設定して、Effect再生処理へ渡します。
/// </summary>
[System.Serializable]
public struct CSST_EffectPlayData
{
    /// <summary>
    /// Effectを再生する位置です。
    /// </summary>
    public Vector3? v3_Position { get; private set; }

    /// <summary>
    /// Effectを再生する回転です。
    /// </summary>
    public Quaternion? q_Rotation { get; private set; }

    /// <summary>
    /// Effectを再生する大きさです。
    /// </summary>
    public Vector3? v3_Scale { get; private set; }

    /// <summary>
    /// Effectの再生に必要な時間です。
    /// </summary>
    public float? f_PlayTime { get; private set; }

    /// <summary>
    /// Effectの終了に必要な時間です。
    /// </summary>
    public float? f_EndTime { get; private set; }

    /// <summary>
    /// Effectをループ再生するかどうかです。
    /// </summary>
    public bool? b_LoopFlag { get; private set; }

    /// <summary>
    /// Effect終了時にEffectを消すかどうかです。
    /// </summary>
    public bool? b_HideOnEnd { get; private set; }

    /// <summary>
    /// Effectの生成から消えるまでの時間です。
    /// </summary>
    public float? f_PlayEndTime { get; private set; }

    public void CSST_EffectPlayData_Init()
    {
        v3_Position = null;
        q_Rotation = null;
        v3_Scale = null;
        f_PlayTime = null;
        f_EndTime = null;
        b_LoopFlag = null;
        b_HideOnEnd = null;
        f_PlayEndTime = null;
    }

    /// <summary>
    /// 再生位置を設定します。
    /// </summary>
    /// <param name="v_Position">再生位置。</param>
    public void SetPosition(Vector3 v_Position)
    {
        this.v3_Position = v_Position;
    }

    /// <summary>
    /// 再生回転を設定します。
    /// </summary>
    /// <param name="q_Rotation">再生回転。</param>
    public void SetRotation(Quaternion q_Rotation)
    {
        this.q_Rotation = q_Rotation;
    }

    /// <summary>
    /// 再生スケールを設定します。
    /// </summary>
    /// <param name="sc_Scale">再生スケール。</param>
    public void SetScale(Vector3 sc_Scale)
    {
        this.v3_Scale = sc_Scale;
    }

    /// <summary>
    /// 再生に必要な時間を設定します。
    /// </summary>
    /// <param name="f_PlayTime">再生に必要な時間。</param>
    public void SetPlayTime(float f_PlayTime)
    {
        this.f_PlayTime = f_PlayTime;
    }

    /// <summary>
    /// 終了に必要な時間を設定します。
    /// </summary>
    /// <param name="f_EndTime">終了に必要な時間。</param>
    public void SetEndTime(float f_EndTime)
    {
        this.f_EndTime = f_EndTime;
    }

    /// <summary>
    /// ループ再生するかどうかを設定します。
    /// </summary>
    /// <param name="b_LoopFlag">ループ再生する場合はtrue。</param>
    public void SetLoopFlag(bool b_LoopFlag)
    {
        this.b_LoopFlag = b_LoopFlag;
    }

    /// <summary>
    /// 終了時にEffectを隠す(非アクティブ)かどうかを設定します。
    /// </summary>
    /// <param name="b_HideOnEnd">隠す(非アクティブ)場合はture</param>
    public void SetEndActive(bool b_HideOnEnd)
    {
        this.b_HideOnEnd = b_HideOnEnd;
    }

    /// <summary>
    /// Effectの生成から消えるまでの時間を設定します。
    /// </summary>
    /// <param name="f_PlayEndTime">生成から消えるまでの時間。</param>
    public void SetPlayEndTime(float f_PlayEndTime)
    {
        this.f_PlayEndTime = f_PlayEndTime;
    }
}
