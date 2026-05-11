/*
+=====================================
 ファイル名 : CSED_CreateTools_LeftWindow.cs
 概要     : CreateToolsの左側ウィンドウ描画クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/04/20 新規作成
            2026/04/22 上下分割描画対応
            2026/05/08 左上エリアに変数パレット描画を追加
            2026/05/08 左下エリアに選択中Field設定描画を追加
=====================================+
*/

#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// CreateToolsの左側ウィンドウ描画処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// 左側のウィンドウを描画します。
    /// </summary>
    private void DrawLeftWindowView()
    {
        Rect topRect = GetLeftTopWindowRect();
        Rect bottomRect = GetLeftBottomWindowRect();

        DrawBlackFrame(topRect.x, topRect.y, topRect.width, topRect.height);
        DrawBlackFrame(bottomRect.x, bottomRect.y, bottomRect.width, bottomRect.height);

        DrawVariablePalette(topRect);
        DrawFieldInspector(bottomRect);
    }
}
#endif
