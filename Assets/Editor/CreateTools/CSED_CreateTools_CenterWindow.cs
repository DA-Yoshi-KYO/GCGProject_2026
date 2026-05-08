/*
+=====================================
 ファイル名 : CSED_CreateTools_CenterWindow.cs
 概要     : CreateToolsの中央側ウィンドウ描画クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/04/20 新規作成
            2026/04/22 可変幅対応
            2026/05/08 中央エリアに作成用エディター描画を追加
=====================================+
*/

#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// CreateToolsの中央側ウィンドウ描画処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// 中央側のウィンドウを描画します。
    /// </summary>
    private void DrawCenterWindowView()
    {
        Rect rect = GetCenterWindowRect();

        DrawBlackFrame(rect.x, rect.y, rect.width, rect.height);

        DrawFieldCanvas(rect);
    }
}
#endif
