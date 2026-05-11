/*
+=====================================
 ファイル名 : CSED_CreateTools_FieldInspector.cs
 概要     : CreateToolsの左下に選択中Fieldの詳細設定を表示するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/08 新規作成
            2026/05/11 InputField用設定の表示間隔と入力欄幅を調整
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// CreateToolsの選択中Field詳細設定描画処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// 黒枠からField詳細設定パネルまでの余白です。
    /// </summary>
    private const float c_FieldInspectorPanelMargin = 6.0f;

    /// <summary>
    /// Field詳細設定パネル内の余白です。
    /// </summary>
    private const float c_FieldInspectorContentPadding = 12.0f;

    /// <summary>
    /// Field詳細設定項目同士の余白です。
    /// </summary>
    private const float c_FieldInspectorSpacing = 10.0f;

    /// <summary>
    /// Layout項目とLayout専用設定の間の余白です。
    /// </summary>
    private const float c_FieldInspectorLayoutBottomSpacing = 24.0f;

    /// <summary>
    /// Field詳細設定のラベル幅です。
    /// </summary>
    private const float c_FieldInspectorLabelWidth = 78.0f;

    /// <summary>
    /// ラベルと入力欄の間の余白です。
    /// </summary>
    private const float c_FieldInspectorLabelToInputSpacing = 14.0f;

    /// <summary>
    /// Field詳細設定の入力欄幅です。
    /// </summary>
    private const float c_FieldInspectorInputWidth = 120.0f;

    /// <summary>
    /// Field詳細設定の1行高さです。
    /// </summary>
    private const float c_FieldInspectorLineHeight = 18.0f;

    /// <summary>
    /// 左下エリアに選択中Fieldの詳細設定を描画します。
    /// </summary>
    /// <param name="f_areaRect">左下エリア全体のRect</param>
    private void DrawFieldInspector(Rect f_areaRect)
    {
        Rect panelRect = GetFieldInspectorPanelRect(f_areaRect);

        DrawFieldInspectorPanel(panelRect);

        Rect contentRect = GetFieldInspectorContentRect(panelRect);

        GUILayout.BeginArea(contentRect);
        {
            m_FieldInspectorScrollPosition = EditorGUILayout.BeginScrollView(m_FieldInspectorScrollPosition);
            {
                DrawFieldInspectorTitle();

                GUILayout.Space(c_FieldInspectorSpacing);

                if (TryGetSelectedFieldData(out CSED_CreateTools_FieldData fieldData) == false)
                {
                    DrawFieldInspectorEmptyMessage();
                }
                else
                {
                    DrawSelectedFieldInspector(fieldData);
                }
            }
            EditorGUILayout.EndScrollView();
        }
        GUILayout.EndArea();
    }

    /// <summary>
    /// Field詳細設定用の内側パネルRectを取得します。
    /// </summary>
    /// <param name="f_areaRect">左下エリア全体のRect</param>
    /// <returns>内側パネルRect</returns>
    private Rect GetFieldInspectorPanelRect(Rect f_areaRect)
    {
        return new Rect(
            f_areaRect.x + c_FieldInspectorPanelMargin,
            f_areaRect.y + c_FieldInspectorPanelMargin,
            Mathf.Max(0.0f, f_areaRect.width - (c_FieldInspectorPanelMargin * 2.0f)),
            Mathf.Max(0.0f, f_areaRect.height - (c_FieldInspectorPanelMargin * 2.0f)));
    }

    /// <summary>
    /// Field詳細設定用のコンテンツRectを取得します。
    /// </summary>
    /// <param name="f_panelRect">内側パネルRect</param>
    /// <returns>コンテンツRect</returns>
    private Rect GetFieldInspectorContentRect(Rect f_panelRect)
    {
        return new Rect(
            f_panelRect.x + c_FieldInspectorContentPadding,
            f_panelRect.y + c_FieldInspectorContentPadding,
            Mathf.Max(0.0f, f_panelRect.width - (c_FieldInspectorContentPadding * 2.0f)),
            Mathf.Max(0.0f, f_panelRect.height - (c_FieldInspectorContentPadding * 2.0f)));
    }

    /// <summary>
    /// Field詳細設定の内側パネルを描画します。
    /// </summary>
    /// <param name="f_panelRect">内側パネルRect</param>
    private void DrawFieldInspectorPanel(Rect f_panelRect)
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
    /// Field詳細設定のタイトルを描画します。
    /// </summary>
    private void DrawFieldInspectorTitle()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.normal.textColor = Color.white;

        EditorGUILayout.LabelField("選択中Field設定", titleStyle);
    }

    /// <summary>
    /// Fieldが未選択のときの案内を描画します。
    /// </summary>
    private void DrawFieldInspectorEmptyMessage()
    {
        EditorGUILayout.HelpBox(
            "中央のFieldをクリックすると、ここに詳細設定が表示されます。",
            MessageType.Info);
    }

    /// <summary>
    /// 選択中Fieldの詳細設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawSelectedFieldInspector(CSED_CreateTools_FieldData f_fieldData)
    {
        EditorGUILayout.LabelField(
            "Field " + (m_SelectedFieldDataIndex + 1).ToString(),
            EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSpacing + 4.0f);

        EditorGUI.BeginChangeCheck();

        CSE_CreateTools_FieldType beforeFieldType = f_fieldData.FieldType;

        f_fieldData.FieldType = DrawSmallFieldTypePopup(
            "Type",
            f_fieldData.FieldType);

        if (beforeFieldType != f_fieldData.FieldType)
        {
            f_fieldData.FieldLayoutType = CreateDefaultFieldLayoutType(f_fieldData.FieldType);
        }

        GUILayout.Space(c_FieldInspectorSpacing);

        f_fieldData.FieldName = DrawSmallTextField(
            "Name",
            f_fieldData.FieldName);

        GUILayout.Space(c_FieldInspectorSpacing);

        f_fieldData.FieldLayoutType = DrawSmallFieldLayoutTypePopup(
            "Layout",
            f_fieldData.FieldLayoutType);

        GUILayout.Space(c_FieldInspectorLayoutBottomSpacing);

        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.InputField)
        {
            DrawInputFieldLayoutSettings(f_fieldData);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Repaint();
        }
    }

    /// <summary>
    /// Input Field用の詳細設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawInputFieldLayoutSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        EditorGUILayout.LabelField("Input Field設定", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSpacing);

        f_fieldData.TagName = DrawSmallTextField(
            "Tag Name",
            f_fieldData.TagName);
    }

    /// <summary>
    /// 小さめのTextFieldを描画します。
    /// </summary>
    /// <param name="f_label">表示ラベル</param>
    /// <param name="f_value">現在の文字列</param>
    /// <returns>入力後の文字列</returns>
    private string DrawSmallTextField(string f_label, string f_value)
    {
        Rect baseRect = EditorGUILayout.GetControlRect(false, c_FieldInspectorLineHeight);

        Rect labelRect = new Rect(
            baseRect.x,
            baseRect.y,
            c_FieldInspectorLabelWidth,
            baseRect.height);

        Rect inputRect = new Rect(
            baseRect.x + c_FieldInspectorLabelWidth + c_FieldInspectorLabelToInputSpacing,
            baseRect.y,
            c_FieldInspectorInputWidth,
            baseRect.height);

        EditorGUI.LabelField(labelRect, f_label);

        return EditorGUI.TextField(inputRect, f_value);
    }

    /// <summary>
    /// 小さめのFieldType用Popupを描画します。
    /// </summary>
    /// <param name="f_label">表示ラベル</param>
    /// <param name="f_value">現在のFieldType</param>
    /// <returns>選択後のFieldType</returns>
    private CSE_CreateTools_FieldType DrawSmallFieldTypePopup(
        string f_label,
        CSE_CreateTools_FieldType f_value)
    {
        Rect baseRect = EditorGUILayout.GetControlRect(false, c_FieldInspectorLineHeight);

        Rect labelRect = new Rect(
            baseRect.x,
            baseRect.y,
            c_FieldInspectorLabelWidth,
            baseRect.height);

        Rect inputRect = new Rect(
            baseRect.x + c_FieldInspectorLabelWidth + c_FieldInspectorLabelToInputSpacing,
            baseRect.y,
            c_FieldInspectorInputWidth,
            baseRect.height);

        EditorGUI.LabelField(labelRect, f_label);

        return (CSE_CreateTools_FieldType)EditorGUI.EnumPopup(inputRect, f_value);
    }

    /// <summary>
    /// 小さめのFieldLayoutType用Popupを描画します。
    /// </summary>
    /// <param name="f_label">表示ラベル</param>
    /// <param name="f_value">現在のFieldLayoutType</param>
    /// <returns>選択後のFieldLayoutType</returns>
    private CSE_CreateTools_FieldLayoutType DrawSmallFieldLayoutTypePopup(
        string f_label,
        CSE_CreateTools_FieldLayoutType f_value)
    {
        Rect baseRect = EditorGUILayout.GetControlRect(false, c_FieldInspectorLineHeight);

        Rect labelRect = new Rect(
            baseRect.x,
            baseRect.y,
            c_FieldInspectorLabelWidth,
            baseRect.height);

        Rect inputRect = new Rect(
            baseRect.x + c_FieldInspectorLabelWidth + c_FieldInspectorLabelToInputSpacing,
            baseRect.y,
            c_FieldInspectorInputWidth,
            baseRect.height);

        EditorGUI.LabelField(labelRect, f_label);

        return (CSE_CreateTools_FieldLayoutType)EditorGUI.EnumPopup(inputRect, f_value);
    }
}
#endif
