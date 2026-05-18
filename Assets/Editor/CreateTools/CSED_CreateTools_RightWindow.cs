/*
+=====================================
 ファイル名 : CSED_CreateTools_RightWindow.cs
 概要     : CreateToolsの右側ウィンドウ描画クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/08 新規作成
            2026/05/18 右側はPreview専用に戻し、Tool生成設定は三点メニュー内へ移動
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
        Rect rightRect = GetRightWindowRect();

        DrawBlackFrame(
            rightRect.x,
            rightRect.y,
            rightRect.width,
            rightRect.height);

        DrawPreview(rightRect);
    }
}
#endif
