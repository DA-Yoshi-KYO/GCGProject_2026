/*
+=====================================
 ファイル名 : CSE_CreateTools_CenterWindow.cs
 概要     : SCreateToolsツールの中央側のウィンドウ描画クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/04/20 新規作成
            2026/04/22 可変幅対応
=====================================+
*/

#if UNITY_EDITOR
using UnityEngine;


/// <summary>
/// ツール作成ツールの中央側のウィンドウ描画処理をまとめるクラス
/// </summary>
public partial class CSE_CreateTools
{
    /// <summary>
    /// 中央側のウィンドウを描画する。
    /// </summary>
    private void DrawCenterWindowView()
    {
        Rect rect = GetCenterWindowRect();
        DrawBlackFrame(rect.x, rect.y, rect.width, rect.height);
    }
}
#endif