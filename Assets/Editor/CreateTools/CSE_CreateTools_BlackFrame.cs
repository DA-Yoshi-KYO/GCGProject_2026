/*
+=====================================
 ファイル名 : CSE_CreateTools_BlackFrame.cs
 概要     : SCreateToolsツールの黒いframe描画クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/04/20 新規作成
            2026/04/20 黒枠描画処理を追加
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// CreateToolsの黒い枠描画処理をまとめるクラス
/// </summary>
public partial class CSE_CreateTools
{
    /// <summary>
    /// 指定した位置とサイズで黒い枠を描画する。
    /// </summary>
    /// <param name="f_x">ポジションX</param>
    /// <param name="f_y">ポジションY</param>
    /// <param name="f_width">横幅</param>
    /// <param name="f_height">高さ</param>
    private void DrawBlackFrame(
        float f_x,
        float f_y,
        float f_width,
        float f_height)
    {
        Rect rect = new Rect(f_x, f_y, f_width, f_height);
        EditorGUI.DrawRect(rect, Color.black);
    }
}
#endif