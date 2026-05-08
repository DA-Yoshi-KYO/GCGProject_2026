/*
+=====================================
 ファイル名 : CSED_CreateTools_FieldCanvas.cs
 概要     : CreateToolsの中央エリアに変数ブロックを表示するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/08 新規作成
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// CreateToolsの中央エリア描画処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// 黒枠から中央パネルまでの余白です。
    /// </summary>
    private const float c_FieldCanvasPanelMargin = 6.0f;

    /// <summary>
    /// 中央パネル内の余白です。
    /// </summary>
    private const float c_FieldCanvasContentPadding = 8.0f;

    /// <summary>
    /// 変数ブロック同士の余白です。
    /// </summary>
    private const float c_FieldCanvasBlockSpacing = 6.0f;

    /// <summary>
    /// 中央エリアに変数キャンバスを描画します。
    /// </summary>
    /// <param name="f_areaRect">中央エリア全体のRect</param>
    private void DrawFieldCanvas(Rect f_areaRect)
    {
        EnsureFieldDataList();

        Rect panelRect = GetFieldCanvasPanelRect(f_areaRect);

        DrawFieldCanvasPanel(panelRect);

        Rect contentRect = GetFieldCanvasContentRect(panelRect);

        GUILayout.BeginArea(contentRect);
        {
            m_FieldCanvasScrollPosition = EditorGUILayout.BeginScrollView(m_FieldCanvasScrollPosition);
            {
                DrawFieldCanvasTitle();

                GUILayout.Space(c_FieldCanvasBlockSpacing);

                if (m_FieldDataList.Count <= 0)
                {
                    DrawFieldCanvasEmptyMessage();
                }
                else
                {
                    DrawFieldCanvasBlocks();
                }
            }
            EditorGUILayout.EndScrollView();
        }
        GUILayout.EndArea();
    }

    /// <summary>
    /// 中央エリア用の内側パネルRectを取得します。
    /// </summary>
    /// <param name="f_areaRect">中央エリア全体のRect</param>
    /// <returns>内側パネルRect</returns>
    private Rect GetFieldCanvasPanelRect(Rect f_areaRect)
    {
        return new Rect(
            f_areaRect.x + c_FieldCanvasPanelMargin,
            f_areaRect.y + c_FieldCanvasPanelMargin,
            Mathf.Max(0.0f, f_areaRect.width - (c_FieldCanvasPanelMargin * 2.0f)),
            Mathf.Max(0.0f, f_areaRect.height - (c_FieldCanvasPanelMargin * 2.0f)));
    }

    /// <summary>
    /// 中央エリア用のコンテンツRectを取得します。
    /// </summary>
    /// <param name="f_panelRect">内側パネルRect</param>
    /// <returns>コンテンツRect</returns>
    private Rect GetFieldCanvasContentRect(Rect f_panelRect)
    {
        return new Rect(
            f_panelRect.x + c_FieldCanvasContentPadding,
            f_panelRect.y + c_FieldCanvasContentPadding,
            Mathf.Max(0.0f, f_panelRect.width - (c_FieldCanvasContentPadding * 2.0f)),
            Mathf.Max(0.0f, f_panelRect.height - (c_FieldCanvasContentPadding * 2.0f)));
    }

    /// <summary>
    /// 中央エリアの内側パネルを描画します。
    /// </summary>
    /// <param name="f_panelRect">内側パネルRect</param>
    private void DrawFieldCanvasPanel(Rect f_panelRect)
    {
        EditorGUI.DrawRect(f_panelRect, new Color(0.28f, 0.28f, 0.28f));

        Rect innerRect = new Rect(
            f_panelRect.x + 1.0f,
            f_panelRect.y + 1.0f,
            Mathf.Max(0.0f, f_panelRect.width - 2.0f),
            Mathf.Max(0.0f, f_panelRect.height - 2.0f));

        EditorGUI.DrawRect(innerRect, new Color(0.16f, 0.16f, 0.16f));
    }

    /// <summary>
    /// 中央エリアのタイトルを描画します。
    /// </summary>
    private void DrawFieldCanvasTitle()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.normal.textColor = Color.white;

        EditorGUILayout.LabelField("作成用エディター", titleStyle);
    }

    /// <summary>
    /// 中央エリアが空のときの案内を描画します。
    /// </summary>
    private void DrawFieldCanvasEmptyMessage()
    {
        EditorGUILayout.HelpBox(
            "左上の変数パレットから型を選ぶと、ここに変数ブロックが追加されます。",
            MessageType.Info);
    }

    /// <summary>
    /// 追加済みの変数ブロックを描画します。
    /// </summary>
    private void DrawFieldCanvasBlocks()
    {
        for (int i = 0 ; i < m_FieldDataList.Count ; i++)
        {
            DrawFieldCanvasBlock(m_FieldDataList[i], i);
            GUILayout.Space(c_FieldCanvasBlockSpacing);
        }
    }

    /// <summary>
    /// 変数ブロックを1つ描画します。
    /// </summary>
    /// <param name="f_fieldData">描画する変数データ</param>
    /// <param name="f_index">表示番号</param>
    private void DrawFieldCanvasBlock(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField(
                "Field " + (f_index + 1).ToString(),
                EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Type", GetFieldTypeDisplayName(f_fieldData.FieldType));
            EditorGUILayout.LabelField("Name", f_fieldData.FieldName);
            EditorGUILayout.LabelField("Layout", f_fieldData.FieldLayoutType.ToString());
        }
        EditorGUILayout.EndVertical();
    }
}
#endif
