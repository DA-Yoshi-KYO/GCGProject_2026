/*
+=====================================
 ファイル名 : CSE_CreateTools_Config.cs
 概要     : SCreateToolsツールの設定値管理クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/04/22 新規作成
=====================================+
*/

#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// ツール作成ツールの設定値をまとめるクラス
/// </summary>
public partial class CSE_CreateTools
{
    /// <summary>
    /// 初期ウィンドウ横幅
    /// </summary>
    private const float c_WindowInitWidth = 1280.0f;

    /// <summary>
    /// 初期ウィンドウ高さ
    /// </summary>
    private const float c_WindowInitHeight = 720.0f;

    /// <summary>
    /// ウィンドウ最小横幅
    /// </summary>
    private const float c_WindowMinWidth = 960.0f;

    /// <summary>
    /// ウィンドウ最小高さ
    /// </summary>
    private const float c_WindowMinHeight = 540.0f;

    /// <summary>
    /// 外側余白
    /// </summary>
    private const float c_Margin = 5.0f;

    /// <summary>
    /// 左右分割バーの幅
    /// </summary>
    private const float c_HorizontalSplitterWidth = 5.0f;

    /// <summary>
    /// 上下分割バーの高さ
    /// </summary>
    private const float c_VerticalSplitterHeight = 5.0f;

    /// <summary>
    /// 左ウィンドウ初期比率
    /// </summary>
    private const float c_LeftInitRatio = 0.20f;

    /// <summary>
    /// 右ウィンドウ初期比率
    /// </summary>
    private const float c_RightInitRatio = 0.20f;

    /// <summary>
    /// 左内部上エリア初期比率
    /// </summary>
    private const float c_LeftTopInitRatio = 0.50f;

    /// <summary>
    /// 左ウィンドウ最小横幅
    /// </summary>
    private const float c_LeftMinWidth = 120.0f;

    /// <summary>
    /// 中央ウィンドウ最小横幅
    /// </summary>
    private const float c_CenterMinWidth = 250.0f;

    /// <summary>
    /// 右ウィンドウ最小横幅
    /// </summary>
    private const float c_RightMinWidth = 120.0f;

    /// <summary>
    /// 左上エリア最小高さ
    /// </summary>
    private const float c_LeftTopMinHeight = 80.0f;

    /// <summary>
    /// 左下エリア最小高さ
    /// </summary>
    private const float c_LeftBottomMinHeight = 80.0f;
}
#endif