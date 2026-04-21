/*
+=====================================
 ファイル名 : CSE_CreateTools_LeftWindow.cs
 概要     : SCreateToolsツールの左側のウィンドウ描画クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/04/20 新規作成
            2026/04/22 上下分割描画対応
=====================================+
*/

#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// ツール作成ツールの左側のウィンドウ描画処理をまとめるクラス
/// </summary>
public partial class CSE_CreateTools
{
    /// <summary>
    /// 左側のウィンドウを描画する。
    /// </summary>
    private void DrawLeftWindowView()
    {
        Rect topRect = GetLeftTopWindowRect();
        Rect bottomRect = GetLeftBottomWindowRect();

        DrawBlackFrame(topRect.x, topRect.y, topRect.width, topRect.height);
        DrawBlackFrame(bottomRect.x, bottomRect.y, bottomRect.width, bottomRect.height);
    }
}
#endif