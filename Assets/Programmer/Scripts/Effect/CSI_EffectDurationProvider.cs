/*
+=====================================
 ファイル名 : CSI_EffectDurationProvider.cs
 概要     : エフェクト演出の再生時間・停止時間を提供するインターフェース
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
=====================================+
*/

/// <summary>
/// エフェクト演出の再生時間・停止時間を提供するインターフェースです。
/// </summary>
public interface CSI_EffectDurationProvider
{
    /// <summary>
    /// 再生時間を持っているか取得します。
    /// </summary>
    bool HasPlayDuration { get; }

    /// <summary>
    /// 再生演出の秒数を取得します。
    /// </summary>
    float PlayDuration { get; }

    /// <summary>
    /// 停止時間を持っているか取得します。
    /// </summary>
    bool HasStopDuration { get; }

    /// <summary>
    /// 停止演出の秒数を取得します。
    /// </summary>
    float StopDuration { get; }
}
