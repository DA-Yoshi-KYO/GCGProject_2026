/*
+=====================================
 ファイル名 : CSE_CreateTools_RightWindow.cs
 概要     : SCreateToolsツールの右側のウィンドウ描画クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/04/20 新規作成
            2026/04/22 可変幅対応
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// ツール作成ツールの右側のウィンドウ描画処理をまとめるクラス
/// </summary>
public partial class CSE_CreateTools
{
    /// <summary>
    /// 右側のウィンドウを描画する。
    /// </summary>
    private void DrawRightWindowView()
    {
        Rect rect = GetRightWindowRect();
        DrawBlackFrame(rect.x, rect.y, rect.width, rect.height);
    }
}
#endif