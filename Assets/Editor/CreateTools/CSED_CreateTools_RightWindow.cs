/*
+=====================================
 ファイル名 : CSED_CreateTools_RightWindow.cs
 概要     : CreateToolsの右側ウィンドウ描画クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/08 新規作成
            2026/05/18 Tool生成設定とCreateボタン表示を追加
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
    /// 右側の生成設定エリア高さです。
    /// </summary>
    private const float c_GeneratedToolInspectorHeight = 150.0f;

    /// <summary>
    /// 右側ウィンドウ内側余白です。
    /// </summary>
    private const float c_RightWindowContentPadding = 8.0f;

    /// <summary>
    /// 生成設定とプレビューの間隔です。
    /// </summary>
    private const float c_RightWindowContentSpacing = 8.0f;

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

        Rect contentRect = new Rect(
            rightRect.x + c_RightWindowContentPadding,
            rightRect.y + c_RightWindowContentPadding,
            Mathf.Max(0.0f, rightRect.width - (c_RightWindowContentPadding * 2.0f)),
            Mathf.Max(0.0f, rightRect.height - (c_RightWindowContentPadding * 2.0f)));

        Rect generatedToolInspectorRect = new Rect(
            contentRect.x,
            contentRect.y,
            contentRect.width,
            c_GeneratedToolInspectorHeight);

        Rect previewRect = new Rect(
            contentRect.x,
            generatedToolInspectorRect.yMax + c_RightWindowContentSpacing,
            contentRect.width,
            Mathf.Max(
                0.0f,
                contentRect.height - c_GeneratedToolInspectorHeight - c_RightWindowContentSpacing));

        DrawGeneratedToolInspector(generatedToolInspectorRect);

        DrawPreview(previewRect);
    }
}
#endif
