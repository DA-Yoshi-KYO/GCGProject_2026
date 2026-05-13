/*
+=====================================
 ファイル名 : CSED_CreateTools_RightWindow.cs
 概要     : CreateToolsの右側ウィンドウ描画クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/04/20 新規作成
            2026/04/22 可変幅対応
            2026/05/13 作成後エディター見本の描画を追加
=====================================+
*/

#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// CreateToolsの右側ウィンドウ描画処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// 右側のウィンドウを描画します。
    /// </summary>
    private void DrawRightWindowView()
    {
        Rect rect = GetRightWindowRect();

        DrawBlackFrame(rect.x, rect.y, rect.width, rect.height);

        DrawPreview(rect);
    }
}
#endif
