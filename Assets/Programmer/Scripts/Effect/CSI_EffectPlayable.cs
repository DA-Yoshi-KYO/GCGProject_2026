/*
+=====================================
 ファイル名 : CSI_EffectPlayable.cs
 概要     : Effect再生に必要な共通操作を定義するInterface
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
=====================================+
*/

/// <summary>
/// Effect再生に必要な共通操作を定義するInterfaceです。
/// </summary>
public interface CSI_EffectPlayable
{
    /// <summary>
    /// Effectを初期化します。
    /// </summary>
    void InitEffect();

    /// <summary>
    /// Effectを再生します。
    /// </summary>
    /// <param name="csst_effectData">Effect再生データ。</param>
    void PlayEffect(CSST_EffectPlayData csst_effectData);

    /// <summary>
    /// Effectを終了します。
    /// </summary>
    void EndEffect();
}
