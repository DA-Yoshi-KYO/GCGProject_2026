/*
+=====================================
 ファイル名 : CSED_CreateTools_FieldCanvas.cs
 概要     : CreateToolsの中央エリアに変数ブロックを表示するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/08 新規作成
            2026/05/08 中央エリアにInspector風パネルを追加
            2026/05/08 Fieldのドラッグ並び替えに対応
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
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
    /// ReorderableListの1要素の高さです。
    /// </summary>
    private const float c_FieldCanvasElementHeight = 96.0f;

    /// <summary>
    /// 中央エリアのField並び替え用リストです。
    /// </summary>
    private ReorderableList m_FieldDataReorderableList;

    /// <summary>
    /// 左上エリアに変数キャンバスを描画します。
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
    /// 中央エリアのタイトルと操作ボタンを描画します。
    /// </summary>
    private void DrawFieldCanvasTitle()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.normal.textColor = Color.white;

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("作業用エディター", titleStyle);

            GUILayout.FlexibleSpace();

            GUI.enabled = m_SelectedFieldDataIndex >= 0 && m_FieldDataList != null && m_SelectedFieldDataIndex < m_FieldDataList.Count;

            if (GUILayout.Button("選択削除", GUILayout.Width(72.0f)))
            {
                RemoveSelectedFieldData();
            }

            GUI.enabled = m_FieldDataList != null && m_FieldDataList.Count > 0;

            if (GUILayout.Button("全消し", GUILayout.Width(60.0f)))
            {
                ClearAllFieldData();
            }

            GUI.enabled = true;
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 中央エリアが空のときの案内を描画します。
    /// </summary>
    private void DrawFieldCanvasEmptyMessage()
    {
        EditorGUILayout.HelpBox(
           "左上のエディターパレットから要素を選ぶと、ここに追加されます。",
            MessageType.Info);
    }

    /// <summary>
    /// 追加済みの変数ブロックを描画します。
    /// </summary>
    private void DrawFieldCanvasBlocks()
    {
        EnsureFieldDataReorderableList();

        m_FieldDataReorderableList.DoLayoutList();
    }

    /// <summary>
    /// Field並び替え用のReorderableListを使用可能な状態にします。
    /// </summary>
    private void EnsureFieldDataReorderableList()
    {
        if (m_FieldDataReorderableList != null && m_FieldDataReorderableList.list == m_FieldDataList)
        {
            return;
        }

        m_FieldDataReorderableList = new ReorderableList(
            m_FieldDataList,
            typeof(CSED_CreateTools_FieldData),
            true,
            false,
            false,
            false);

        m_FieldDataReorderableList.elementHeight = c_FieldCanvasElementHeight;

        m_FieldDataReorderableList.drawElementCallback = DrawFieldCanvasElement;

        m_FieldDataReorderableList.onSelectCallback = OnFieldCanvasSelected;

        m_FieldDataReorderableList.onReorderCallback = OnFieldCanvasReordered;
    }

    /// <summary>
    /// ReorderableListのField要素を描画します。
    /// </summary>
    /// <param name="f_rect">描画範囲</param>
    /// <param name="f_index">要素番号</param>
    /// <param name="f_isActive">選択中かどうか</param>
    /// <param name="f_isFocused">フォーカス中かどうか</param>
    private void DrawFieldCanvasElement(
        Rect f_rect,
        int f_index,
        bool f_isActive,
        bool f_isFocused)
    {
        if (f_index < 0 || f_index >= m_FieldDataList.Count)
        {
            return;
        }

        CSED_CreateTools_FieldData fieldData = m_FieldDataList[f_index];

        Rect boxRect = new Rect(
            f_rect.x,
            f_rect.y + 2.0f,
            f_rect.width,
            f_rect.height - 6.0f);

        GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

        float labelX = boxRect.x + 10.0f;
        float valueX = boxRect.x + 140.0f;
        float currentY = boxRect.y + 6.0f;
        float lineHeight = 18.0f;

        Rect titleRect = new Rect(
            labelX,
            currentY,
            boxRect.width - 20.0f,
            lineHeight);

        EditorGUI.LabelField(
            titleRect,
            "Field " + (f_index + 1).ToString(),
            EditorStyles.boldLabel);

        currentY += lineHeight;

        DrawFieldCanvasElementLine(
            labelX,
            valueX,
            currentY,
            boxRect.width,
            "Variable Type",
            GetFieldTypeDisplayName(fieldData.FieldType));

        currentY += lineHeight;

        DrawFieldCanvasElementLine(
            labelX,
            valueX,
            currentY,
            boxRect.width,
            "Variable Name",
            fieldData.FieldName);

        currentY += lineHeight;

        DrawFieldCanvasElementLine(
            labelX,
            valueX,
            currentY,
            boxRect.width,
            "Layout",
            fieldData.FieldLayoutType.ToString());
    }

    /// <summary>
    /// Field要素の1行を描画します。
    /// </summary>
    /// <param name="f_labelX">ラベルX座標</param>
    /// <param name="f_valueX">値X座標</param>
    /// <param name="f_y">Y座標</param>
    /// <param name="f_width">全体横幅</param>
    /// <param name="f_label">ラベル</param>
    /// <param name="f_value">値</param>
    private void DrawFieldCanvasElementLine(
        float f_labelX,
        float f_valueX,
        float f_y,
        float f_width,
        string f_label,
        string f_value)
    {
        float lineHeight = 18.0f;

        Rect labelRect = new Rect(
            f_labelX,
            f_y,
            120.0f,
            lineHeight);

        Rect valueRect = new Rect(
            f_valueX,
            f_y,
            Mathf.Max(0.0f, f_width - 160.0f),
            lineHeight);

        EditorGUI.LabelField(labelRect, f_label);
        EditorGUI.LabelField(valueRect, f_value);
    }

    /// <summary>
    /// Fieldの並び順が変更されたときの処理を行います。
    /// </summary>
    /// <param name="f_list">並び替えされたReorderableList</param>
    private void OnFieldCanvasReordered(ReorderableList f_list)
    {
        Debug.Log("[CreateTools] Fieldの順番を変更しました。");

        Repaint();
    }

    /// <summary>
    /// Fieldが選択されたときの処理を行います。
    /// </summary>
    /// <param name="f_list">選択されたReorderableList</param>
    private void OnFieldCanvasSelected(ReorderableList f_list)
    {
        SetSelectedFieldDataIndex(f_list.index);
    }
}
#endif
