/*
+=====================================
 ファイル名 : CSE_CreateTools_SystemState.cs
 概要     : SCreateToolsツールのシステム状態管理クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/04/22 新規作成
=====================================+
*/

#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// ツール作成ツールのシステム状態をまとめるクラス
/// </summary>
public partial class CSE_CreateTools
{
    /// <summary>
    /// 左ウィンドウ現在横幅
    /// </summary>
    private float m_LeftCurrentWidth;

    /// <summary>
    /// 右ウィンドウ現在横幅
    /// </summary>
    private float m_RightCurrentWidth;

    /// <summary>
    /// 左上エリア現在高さ
    /// </summary>
    private float m_LeftTopCurrentHeight;

    /// <summary>
    /// 左右分割初期化済みフラグ
    /// </summary>
    private bool m_IsHorizontalInitialized;

    /// <summary>
    /// 左内部上下分割初期化済みフラグ
    /// </summary>
    private bool m_IsVerticalInitialized;

    /// <summary>
    /// 左右分割の左バーをドラッグ中か
    /// </summary>
    private bool m_IsDraggingLeftHorizontal;

    /// <summary>
    /// 左右分割の右バーをドラッグ中か
    /// </summary>
    private bool m_IsDraggingRightHorizontal;

    /// <summary>
    /// 左内部上下バーをドラッグ中か
    /// </summary>
    private bool m_IsDraggingLeftVertical;

    /// <summary>
    /// 横方向ドラッグ補正値
    /// </summary>
    private float m_HorizontalDragOffset;

    /// <summary>
    /// 縦方向ドラッグ補正値
    /// </summary>
    private float m_VerticalDragOffset;
}
#endif