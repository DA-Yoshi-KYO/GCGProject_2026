/*
+=====================================
 ファイル名 : CSED_CreateTools_FieldInspector.cs
 概要     : CreateToolsの左下に選択中Fieldの詳細設定を表示するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/08 新規作成
            2026/05/11 InputField用設定の表示間隔と入力欄幅を調整
            2026/05/13 選択中Field設定の行間ルールを統一
            2026/05/13 Slider設定とDefault設定を追加
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
    /// Field詳細設定のスクロールバー想定幅です。
    /// </summary>
    private const float c_FieldInspectorScrollBarWidth = 16.0f;

    /// <summary>
    /// Field詳細設定の入力欄右側の余白です。
    /// エディターパレットのボタン右端と揃えるための値です。
    /// </summary>
    private const float c_FieldInspectorInputRightGap = 10.0f;

    /// <summary>
    /// 黒枠からField詳細設定パネルまでの余白です。
    /// </summary>
    private const float c_FieldInspectorPanelMargin = 6.0f;

    /// <summary>
    /// Field詳細設定パネル内の余白です。
    /// </summary>
    private const float c_FieldInspectorContentPadding = 8.0f;

    /// <summary>
    /// 通常項目同士の縦余白です。
    /// </summary>
    private const float c_FieldInspectorRowSpacing = 5.0f;

    /// <summary>
    /// セクション前の大きい縦余白です。
    /// </summary>
    private const float c_FieldInspectorSectionTopSpacing = 32.0f;

    /// <summary>
    /// セクション見出しと最初の項目の縦余白です。
    /// </summary>
    private const float c_FieldInspectorSectionTitleBottomSpacing = 12.0f;

    /// <summary>
    /// Field詳細設定のラベル幅です。
    /// </summary>
    private const float c_FieldInspectorLabelWidth = 105.0f;

    /// <summary>
    /// ラベルと入力欄の間の余白です。
    /// </summary>
    private const float c_FieldInspectorLabelToInputSpacing = 8.0f;

    /// <summary>
    /// Field詳細設定の1行高さです。
    /// </summary>
    private const float c_FieldInspectorLineHeight = 18.0f;

    /// <summary>
    /// Toggleの横幅です。
    /// </summary>
    private const float c_FieldInspectorToggleWidth = 18.0f;

    /// <summary>
    /// Field詳細設定の現在のコンテンツ横幅です。
    /// </summary>
    private float m_FieldInspectorCurrentContentWidth;

    /// <summary>
    /// 左下エリアに選択中Fieldの詳細設定を描画します。
    /// </summary>
    /// <param name="f_areaRect">左下エリア全体のRect</param>
    private void DrawFieldInspector(Rect f_areaRect)
    {
        Rect panelRect = GetFieldInspectorPanelRect(f_areaRect);

        DrawFieldInspectorPanel(panelRect);

        Rect contentRect = GetFieldInspectorContentRect(panelRect);

        m_FieldInspectorCurrentContentWidth = contentRect.width;

        GUILayout.BeginArea(contentRect);
        {
            m_FieldInspectorScrollPosition = EditorGUILayout.BeginScrollView(m_FieldInspectorScrollPosition);
            {
                DrawFieldInspectorTitle();

                GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

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
            "Field" + (m_SelectedFieldDataIndex + 1).ToString(),
            EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        EditorGUI.BeginChangeCheck();

        CSE_CreateTools_FieldType beforeFieldType = f_fieldData.FieldType;

        f_fieldData.FieldType = DrawSmallFieldTypePopup(
            "  Variable Type",
            f_fieldData.FieldType);

        if (beforeFieldType != f_fieldData.FieldType)
        {
            f_fieldData.FieldLayoutType = CreateDefaultFieldLayoutType(f_fieldData.FieldType);
        }

        GUILayout.Space(c_FieldInspectorRowSpacing);

        f_fieldData.FieldName = DrawSmallTextField(
            "  Variable Name",
            f_fieldData.FieldName);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        f_fieldData.FieldLayoutType = DrawSmallFieldLayoutTypePopup(
            "  Layout",
            f_fieldData.FieldLayoutType);

        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.InputField)
        {
            GUILayout.Space(c_FieldInspectorSectionTopSpacing);
            DrawInputFieldLayoutSettings(f_fieldData);
        }
        else if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.Slider)
        {
            GUILayout.Space(c_FieldInspectorSectionTopSpacing);
            DrawSliderLayoutSettings(f_fieldData);
        }
        else if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.MinMaxField)
        {
            GUILayout.Space(c_FieldInspectorSectionTopSpacing);
            DrawMinMaxFieldLayoutSettings(f_fieldData);
        }

        GUILayout.Space(c_FieldInspectorSectionTopSpacing);

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
        DrawInputFieldCommonSettings(f_fieldData);

        GUILayout.Space(c_FieldInspectorSectionTopSpacing);

        DrawDefaultValueSettings(f_fieldData);
    }

    /// <summary>
    /// Input Field共通設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawInputFieldCommonSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        EditorGUILayout.LabelField("Input Field設定", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        f_fieldData.TagName = DrawSmallTextField(
            "  Tag Name",
            f_fieldData.TagName);
    }

    /// <summary>
    /// Slider用の詳細設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawSliderLayoutSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        DrawInputFieldCommonSettings(f_fieldData);

        GUILayout.Space(c_FieldInspectorSectionTopSpacing);

        DrawDefaultValueSettings(f_fieldData);

        GUILayout.Space(c_FieldInspectorSectionTopSpacing);

        DrawSliderRangeSettings(f_fieldData);
    }

    /// <summary>
    /// 通常初期値設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawDefaultValueSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        EditorGUILayout.LabelField("Default設定", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        f_fieldData.IsDefaultValueNull = DrawSmallToggle(
            "  Default Is Null",
            f_fieldData.IsDefaultValueNull);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        EditorGUI.BeginDisabledGroup(f_fieldData.IsDefaultValueNull);
        {
            f_fieldData.DefaultValueText = DrawSmallTextField(
                "  Default Value",
                f_fieldData.DefaultValueText);
        }
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// Slider範囲設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawSliderRangeSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        EditorGUILayout.LabelField("Slider Range設定", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        f_fieldData.IsSliderMaxValueNull = DrawSmallToggle(
            "  Max Is Null",
            f_fieldData.IsSliderMaxValueNull);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        EditorGUI.BeginDisabledGroup(f_fieldData.IsSliderMaxValueNull);
        {
            f_fieldData.SliderMaxValueText = DrawSmallTextField(
                "  Slider Max Value",
                f_fieldData.SliderMaxValueText);
        }
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// Min Max Field用の詳細設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawMinMaxFieldLayoutSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        EditorGUILayout.LabelField("  Min Max Field設定", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        EditorGUILayout.LabelField("  Default Min", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        f_fieldData.IsDefaultMinValueNull = DrawSmallToggle(
            "  Min Is Null",
            f_fieldData.IsDefaultMinValueNull);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        EditorGUI.BeginDisabledGroup(f_fieldData.IsDefaultMinValueNull);
        {
            f_fieldData.DefaultMinValueText = DrawSmallTextField(
                "  Min Value",
                f_fieldData.DefaultMinValueText);
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(c_FieldInspectorSectionTopSpacing);

        EditorGUILayout.LabelField("  Default Max", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        f_fieldData.IsDefaultMaxValueNull = DrawSmallToggle(
            "  Max Is Null",
            f_fieldData.IsDefaultMaxValueNull);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        EditorGUI.BeginDisabledGroup(f_fieldData.IsDefaultMaxValueNull);
        {
            f_fieldData.DefaultMaxValueText = DrawSmallTextField(
                "  Max Value",
                f_fieldData.DefaultMaxValueText);
        }
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// Field詳細設定の1行分のRectを取得します。
    /// </summary>
    /// <returns>1行分のRect</returns>
    private Rect GetFieldInspectorRowRect()
    {
        return EditorGUILayout.GetControlRect(
            false,
            c_FieldInspectorLineHeight);
    }

    /// <summary>
    /// Field詳細設定のラベルRectを取得します。
    /// </summary>
    /// <param name="f_rowRect">1行分のRect</param>
    /// <returns>ラベルRect</returns>
    private Rect GetFieldInspectorLabelRect(Rect f_rowRect)
    {
        return new Rect(
            f_rowRect.x,
            f_rowRect.y,
            c_FieldInspectorLabelWidth,
            f_rowRect.height);
    }

    /// <summary>
    /// Field詳細設定の入力項目Rectを取得します。
    /// </summary>
    /// <param name="f_rowRect">1行分のRect</param>
    /// <returns>入力項目Rect</returns>
    private Rect GetFieldInspectorInputRect(Rect f_rowRect)
    {
        float inputX =
            f_rowRect.x
            + c_FieldInspectorLabelWidth
            + c_FieldInspectorLabelToInputSpacing;

        float targetRightX =
            m_FieldInspectorCurrentContentWidth
            - c_FieldInspectorScrollBarWidth
            - c_FieldInspectorInputRightGap;

        float inputWidth = Mathf.Max(
            0.0f,
            targetRightX - inputX);

        return new Rect(
            inputX,
            f_rowRect.y,
            inputWidth,
            f_rowRect.height);
    }

    /// <summary>
    /// 小さめのTextFieldを描画します。
    /// </summary>
    /// <param name="f_label">表示ラベル</param>
    /// <param name="f_value">現在の文字列</param>
    /// <returns>入力後の文字列</returns>
    private string DrawSmallTextField(string f_label, string f_value)
    {
        Rect rowRect = GetFieldInspectorRowRect();
        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

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
        Rect rowRect = GetFieldInspectorRowRect();
        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

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
        Rect rowRect = GetFieldInspectorRowRect();
        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

        EditorGUI.LabelField(labelRect, f_label);

        return (CSE_CreateTools_FieldLayoutType)EditorGUI.EnumPopup(inputRect, f_value);
    }

    /// <summary>
    /// 小さいToggle入力欄を描画します。
    /// </summary>
    /// <param name="f_label">表示ラベル</param>
    /// <param name="f_value">現在の値</param>
    /// <returns>変更後の値</returns>
    private bool DrawSmallToggle(string f_label, bool f_value)
    {
        Rect rowRect = GetFieldInspectorRowRect();
        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

        Rect toggleRect = new Rect(
            inputRect.x,
            inputRect.y,
            c_FieldInspectorToggleWidth,
            inputRect.height);

        EditorGUI.LabelField(labelRect, f_label);

        return EditorGUI.Toggle(toggleRect, f_value);
    }
}
#endif
