/*
+=====================================
 ファイル名 : CSI_EffectPlayable.cs
 概要     : エフェクトの再生・停止を共通化するインターフェース
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
=====================================+
*/

/// <summary>
/// エフェクトの再生・停止を共通化するインターフェースです。
/// </summary>
public interface CSI_EffectPlayable
{
    /// <summary>
    /// エフェクトを再生します。
    /// </summary>
    void PlayEffect();

    /// <summary>
    /// エフェクトを停止します。
    /// </summary>
    void StopEffect();

    /// <summary>
    /// エフェクトの再生速度を設定します。
    /// </summary>
    /// <param name="playSpeed">再生速度。</param>
    void SetPlaySpeed(float playSpeed);
}
