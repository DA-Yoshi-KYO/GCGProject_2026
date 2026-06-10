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
    [field: SerializeField]
    public Vector3 v_Position { get; private set; }

    /// <summary>
    /// Effectを再生する回転です。
    /// </summary>
    [field: SerializeField]
    public Quaternion q_Rotation { get; private set; }

    /// <summary>
    /// Effectを再生する大きさです。
    /// </summary>
    [field: SerializeField]
    public Vector3 sc_Scale { get; private set; }

    /// <summary>
    /// Effectの再生に必要な時間です。
    /// </summary>
    [field: SerializeField]
    public float f_PlayTime { get; private set; }

    /// <summary>
    /// Effectの終了に必要な時間です。
    /// </summary>
    [field: SerializeField]
    public float f_EndTime { get; private set; }

    /// <summary>
    /// Effectをループ再生するかどうかです。
    /// </summary>
    [field: SerializeField]
    public bool b_LoopFlag { get; private set; }

    /// <summary>
    /// Effectの生成から消えるまでの時間です。
    /// </summary>
    [field: SerializeField]
    public float f_PlayEndTime { get; private set; }

    /// <summary>
    /// Effect再生データを作成します。
    /// </summary>
    public CSST_EffectPlayData()
        : this()
    {
        v_Position = null;
        q_Rotation = Quaternion.identity;
        sc_Scale = Vector3.one;
        f_PlayTime = 0.0f;
        f_EndTime = 1.0f;
        b_LoopFlag = false;
        f_PlayEndTime = 1.0f;
    }

    /// <summary>
    /// 再生位置を設定します。
    /// </summary>
    /// <param name="v_Position">再生位置。</param>
    public void SetPosition(Vector3 v_Position)
    {
        this.v_Position = v_Position;
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
        this.sc_Scale = sc_Scale;
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
    /// Effectの生成から消えるまでの時間を設定します。
    /// </summary>
    /// <param name="f_PlayEndTime">生成から消えるまでの時間。</param>
    public void SetPlayEndTime(float f_PlayEndTime)
    {
        this.f_PlayEndTime = f_PlayEndTime;
    }
}
