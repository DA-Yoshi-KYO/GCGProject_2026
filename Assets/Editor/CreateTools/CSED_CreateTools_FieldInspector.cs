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
using System.Collections.Generic;
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
    /// FieldTypeに応じて使用可能なLayoutTypeだけを表示するPopupを描画します。
    /// </summary>
    /// <param name="f_label">表示ラベル</param>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>選択後のLayoutType</returns>
    private CSE_CreateTools_FieldLayoutType DrawSmallAllowedFieldLayoutTypePopup(
        string f_label,
        CSED_CreateTools_FieldData f_fieldData)
    {
        CSE_CreateTools_FieldLayoutType[] allowedLayoutTypes =
            GetAllowedFieldLayoutTypes(f_fieldData);

        string[] displayNames = new string[allowedLayoutTypes.Length];

        for (int i = 0 ; i < allowedLayoutTypes.Length ; i++)
        {
            displayNames[i] = GetFieldLayoutTypeDisplayName(allowedLayoutTypes[i]);
        }

        int selectedIndex = 0;

        for (int i = 0 ; i < allowedLayoutTypes.Length ; i++)
        {
            if (allowedLayoutTypes[i] == f_fieldData.FieldLayoutType)
            {
                selectedIndex = i;
                break;
            }
        }

        Rect rowRect = GetFieldInspectorRowRect();
        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

        EditorGUI.LabelField(labelRect, f_label);

        selectedIndex = EditorGUI.Popup(
            inputRect,
            selectedIndex,
            displayNames);

        return allowedLayoutTypes[selectedIndex];
    }

    /// <summary>
    /// FieldDataで使用可能なLayoutType一覧を取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>使用可能なLayoutType一覧</returns>
    private CSE_CreateTools_FieldLayoutType[] GetAllowedFieldLayoutTypes(CSED_CreateTools_FieldData f_fieldData)
    {
        CSE_CreateTools_FieldType targetFieldType = GetLayoutTargetFieldType(f_fieldData);

        switch (targetFieldType)
        {
            case CSE_CreateTools_FieldType.Int:
            case CSE_CreateTools_FieldType.Float:
                return new CSE_CreateTools_FieldLayoutType[]
                {
                CSE_CreateTools_FieldLayoutType.InputField,
                CSE_CreateTools_FieldLayoutType.Slider,
                CSE_CreateTools_FieldLayoutType.MinMaxField
                };

            case CSE_CreateTools_FieldType.String:
                return new CSE_CreateTools_FieldLayoutType[]
                {
                CSE_CreateTools_FieldLayoutType.InputField,
                CSE_CreateTools_FieldLayoutType.TextArea
                };

            case CSE_CreateTools_FieldType.Bool:
                return new CSE_CreateTools_FieldLayoutType[]
                {
                CSE_CreateTools_FieldLayoutType.Toggle
                };

            case CSE_CreateTools_FieldType.ScriptableObject:
            case CSE_CreateTools_FieldType.Script:
            case CSE_CreateTools_FieldType.GameObject:
                return new CSE_CreateTools_FieldLayoutType[]
                {
                CSE_CreateTools_FieldLayoutType.Select
                };

            case CSE_CreateTools_FieldType.Enum:
                return new CSE_CreateTools_FieldLayoutType[]
                {
                CSE_CreateTools_FieldLayoutType.Dropdown
                };

            default:
                return new CSE_CreateTools_FieldLayoutType[]
                {
                CSE_CreateTools_FieldLayoutType.InputField
                };
        }
    }

    /// <summary>
    /// LayoutTypeの表示名を取得します。
    /// </summary>
    /// <param name="f_layoutType">LayoutType</param>
    /// <returns>表示名</returns>
    private string GetFieldLayoutTypeDisplayName(CSE_CreateTools_FieldLayoutType f_layoutType)
    {
        switch (f_layoutType)
        {
            case CSE_CreateTools_FieldLayoutType.InputField:
                return "Input Field";

            case CSE_CreateTools_FieldLayoutType.Slider:
                return "Slider";

            case CSE_CreateTools_FieldLayoutType.MinMaxField:
                return "Min Max Field";

            case CSE_CreateTools_FieldLayoutType.Toggle:
                return "Toggle";

            case CSE_CreateTools_FieldLayoutType.TextArea:
                return "Text Area";

            case CSE_CreateTools_FieldLayoutType.Dropdown:
                return "Dropdown";

            case CSE_CreateTools_FieldLayoutType.Select:
                return "Select";

            default:
                return "Unknown";
        }
    }

    /// <summary>
    /// FieldDataに対して現在のLayoutが使えるか確認し、不正なら初期Layoutに戻します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    private void NormalizeFieldLayoutType(CSED_CreateTools_FieldData f_fieldData)
    {
        if (IsAllowedFieldLayoutType(f_fieldData, f_fieldData.FieldLayoutType))
        {
            return;
        }

        f_fieldData.FieldLayoutType = GetDefaultAllowedFieldLayoutType(f_fieldData);
    }

    /// <summary>
    /// FieldDataに対応した初期LayoutTypeを取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>初期LayoutType</returns>
    private CSE_CreateTools_FieldLayoutType GetDefaultAllowedFieldLayoutType(CSED_CreateTools_FieldData f_fieldData)
    {
        CSE_CreateTools_FieldType targetFieldType = GetLayoutTargetFieldType(f_fieldData);

        switch (targetFieldType)
        {
            case CSE_CreateTools_FieldType.Bool:
                return CSE_CreateTools_FieldLayoutType.Toggle;

            case CSE_CreateTools_FieldType.ScriptableObject:
            case CSE_CreateTools_FieldType.Script:
            case CSE_CreateTools_FieldType.GameObject:
                return CSE_CreateTools_FieldLayoutType.Select;

            case CSE_CreateTools_FieldType.Enum:
                return CSE_CreateTools_FieldLayoutType.Dropdown;

            default:
                return CSE_CreateTools_FieldLayoutType.InputField;
        }
    }

    /// <summary>
    /// Layout判定に使うFieldTypeを取得します。
    /// Listの場合はListの中身の型を使います。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>Layout判定用FieldType</returns>
    private CSE_CreateTools_FieldType GetLayoutTargetFieldType(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.List)
        {
            return f_fieldData.ListElementFieldType;
        }

        return f_fieldData.FieldType;
    }

    /// <summary>
    /// 指定したLayoutTypeがFieldDataで使用可能か判定します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <param name="f_layoutType">判定するLayoutType</param>
    /// <returns>使用可能ならtrue</returns>
    private bool IsAllowedFieldLayoutType(
        CSED_CreateTools_FieldData f_fieldData,
        CSE_CreateTools_FieldLayoutType f_layoutType)
    {
        CSE_CreateTools_FieldType targetFieldType = GetLayoutTargetFieldType(f_fieldData);

        switch (targetFieldType)
        {
            case CSE_CreateTools_FieldType.Int:
            case CSE_CreateTools_FieldType.Float:
                return
                    f_layoutType == CSE_CreateTools_FieldLayoutType.InputField ||
                    f_layoutType == CSE_CreateTools_FieldLayoutType.Slider ||
                    f_layoutType == CSE_CreateTools_FieldLayoutType.MinMaxField;

            case CSE_CreateTools_FieldType.String:
                return
                    f_layoutType == CSE_CreateTools_FieldLayoutType.InputField ||
                    f_layoutType == CSE_CreateTools_FieldLayoutType.TextArea;

            case CSE_CreateTools_FieldType.Bool:
                return f_layoutType == CSE_CreateTools_FieldLayoutType.Toggle;

            case CSE_CreateTools_FieldType.Enum:
                return f_layoutType == CSE_CreateTools_FieldLayoutType.Dropdown;

            case CSE_CreateTools_FieldType.ScriptableObject:
            case CSE_CreateTools_FieldType.Script:
            case CSE_CreateTools_FieldType.GameObject:
                return f_layoutType == CSE_CreateTools_FieldLayoutType.Select;

            default:
                return f_layoutType == CSE_CreateTools_FieldLayoutType.InputField;
        }
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
            "中央のFieldをクリックすると、ここに詳細設定が表示されます",
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
            "  変数",
            f_fieldData.FieldType);

        if (beforeFieldType != f_fieldData.FieldType)
        {
            f_fieldData.FieldLayoutType = GetDefaultAllowedFieldLayoutType(f_fieldData);
        }

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.List)
        {
            GUILayout.Space(c_FieldInspectorRowSpacing);

            CSE_CreateTools_FieldType beforeListElementFieldType = f_fieldData.ListElementFieldType;

            f_fieldData.ListElementFieldType = DrawSmallListElementFieldTypePopup(
                "  リスト変数",
                f_fieldData.ListElementFieldType);

            if (beforeListElementFieldType != f_fieldData.ListElementFieldType)
            {
                f_fieldData.FieldLayoutType = GetDefaultAllowedFieldLayoutType(f_fieldData);
            }
        }

        if (ShouldShowScriptableObjectTypeField(f_fieldData))
        {
            GUILayout.Space(c_FieldInspectorRowSpacing);

            f_fieldData.ScriptableObjectTypeScript = DrawSmallScriptableObjectTypeField(
                "  ScriptableObject Type",
                f_fieldData.ScriptableObjectTypeScript);
        }

        if (ShouldShowEnumTypeField(f_fieldData))
        {
            GUILayout.Space(c_FieldInspectorRowSpacing);

            f_fieldData.EnumTypeScript = DrawSmallEnumTypeField(
                "  Enum Type",
                f_fieldData.EnumTypeScript);
        }

        GUILayout.Space(c_FieldInspectorRowSpacing);

        f_fieldData.FieldName = DrawSmallTextField(
            "  変数名",
            f_fieldData.FieldName);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        NormalizeFieldLayoutType(f_fieldData);

        f_fieldData.FieldLayoutType = DrawSmallAllowedFieldLayoutTypePopup(
            "  表示方法",
            f_fieldData);

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
        else if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.Toggle)
        {
            GUILayout.Space(c_FieldInspectorSectionTopSpacing);
            DrawToggleLayoutSettings(f_fieldData);
        }
        else if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.TextArea)
        {
            GUILayout.Space(c_FieldInspectorSectionTopSpacing);
            DrawTextAreaLayoutSettings(f_fieldData);
        }
        else if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.Select)
        {
            GUILayout.Space(c_FieldInspectorSectionTopSpacing);
            DrawSelectLayoutSettings(f_fieldData);
        }
        else if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.Dropdown)
        {
            GUILayout.Space(c_FieldInspectorSectionTopSpacing);
            DrawDropdownLayoutSettings(f_fieldData);
        }

        GUILayout.Space(c_FieldInspectorSectionTopSpacing);

        if (EditorGUI.EndChangeCheck())
        {
            Repaint();
        }
    }

    /// <summary>
    /// Dropdown用の詳細設定を描画します。
    /// </summary>
    private void DrawDropdownLayoutSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        EditorGUILayout.LabelField("Dropdown設定", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        DrawTagNameField(f_fieldData);

        GUILayout.Space(c_FieldInspectorSectionTopSpacing);

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.List)
        {
            DrawListDefaultValueSettingsByLayout(
                f_fieldData,
                CSE_CreateTools_FieldLayoutType.Dropdown);

            return;
        }

        DrawDefaultEnumValueSettings(f_fieldData);
    }

    /// <summary>
    /// Enum用のDefault設定を描画します。
    /// </summary>
    private void DrawDefaultEnumValueSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        EditorGUILayout.LabelField("Default設定", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        f_fieldData.IsDefaultValueNull = DrawSmallToggle(
            "  Default Is Null",
            f_fieldData.IsDefaultValueNull);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        EditorGUI.BeginDisabledGroup(f_fieldData.IsDefaultValueNull);
        {
            f_fieldData.DefaultValueText = DrawSmallEnumValuePopup(
                "  Default Value",
                f_fieldData,
                f_fieldData.DefaultValueText);
        }
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// Enum値選択Popupを描画します。
    /// </summary>
    private string DrawSmallEnumValuePopup(
        string f_label,
        CSED_CreateTools_FieldData f_fieldData,
        string f_value)
    {
        Rect rowRect = GetFieldInspectorRowRect();
        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

        EditorGUI.LabelField(labelRect, f_label);

        System.Type enumType = GetEnumTypeFromMonoScript(f_fieldData.EnumTypeScript);

        if (enumType == null)
        {
            EditorGUI.LabelField(inputRect, "Enum Type未設定");
            return f_value;
        }

        string[] enumNames = System.Enum.GetNames(enumType);

        if (enumNames == null || enumNames.Length <= 0)
        {
            EditorGUI.LabelField(inputRect, "Enum値なし");
            return f_value;
        }

        int selectedIndex = 0;

        for (int i = 0 ; i < enumNames.Length ; i++)
        {
            if (enumNames[i] == f_value)
            {
                selectedIndex = i;
                break;
            }
        }

        selectedIndex = EditorGUI.Popup(
            inputRect,
            selectedIndex,
            enumNames);

        return enumNames[selectedIndex];
    }

    /// <summary>
    /// Enum Type項目を表示するか判定します。
    /// </summary>
    private bool ShouldShowEnumTypeField(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Enum)
        {
            return true;
        }

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.List &&
            f_fieldData.ListElementFieldType == CSE_CreateTools_FieldType.Enum)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Enum型スクリプトを選択するFieldを描画します。
    /// </summary>
    private MonoScript DrawSmallEnumTypeField(
        string f_label,
        MonoScript f_value)
    {
        Rect rowRect = GetFieldInspectorRowRect();
        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

        EditorGUI.LabelField(labelRect, f_label);

        MonoScript selectedScript = (MonoScript)EditorGUI.ObjectField(
            inputRect,
            f_value,
            typeof(MonoScript),
            false);

        if (selectedScript == null)
        {
            return null;
        }

        if (IsEnumMonoScript(selectedScript))
        {
            return selectedScript;
        }

        EditorUtility.DisplayDialog(
            "Enum Type Error",
            "enum定義のスクリプトだけ選択できます。",
            "OK");

        return f_value;
    }

    /// <summary>
    /// MonoScriptがenumか判定します。
    /// </summary>
    private bool IsEnumMonoScript(MonoScript f_script)
    {
        System.Type enumType = GetEnumTypeFromMonoScript(f_script);

        return enumType != null && enumType.IsEnum;
    }

    /// <summary>
    /// MonoScriptからenum型を取得します。
    /// </summary>
    private System.Type GetEnumTypeFromMonoScript(MonoScript f_script)
    {
        if (f_script == null)
        {
            return null;
        }

        System.Type scriptType = f_script.GetClass();

        if (scriptType != null && scriptType.IsEnum)
        {
            return scriptType;
        }

        string assetPath = AssetDatabase.GetAssetPath(f_script);
        string scriptName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

        System.Reflection.Assembly[] assemblies =
            System.AppDomain.CurrentDomain.GetAssemblies();

        for (int i = 0 ; i < assemblies.Length ; i++)
        {
            System.Type[] types = null;

            try
            {
                types = assemblies[i].GetTypes();
            }
            catch (System.Reflection.ReflectionTypeLoadException exception)
            {
                types = exception.Types;
            }

            if (types == null)
            {
                continue;
            }

            for (int j = 0 ; j < types.Length ; j++)
            {
                if (types[j] == null)
                {
                    continue;
                }

                if (types[j].IsEnum && types[j].Name == scriptName)
                {
                    return types[j];
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Select用の詳細設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawSelectLayoutSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        EditorGUILayout.LabelField("Select設定", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        f_fieldData.TagName = DrawSmallTextField(
            "  タグ名",
            f_fieldData.TagName);

        GUILayout.Space(c_FieldInspectorSectionTopSpacing);

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.List)
        {
            DrawListDefaultValueSettingsByLayout(
                f_fieldData,
                CSE_CreateTools_FieldLayoutType.Select);

            return;
        }

        DrawDefaultSelectValueSettings(f_fieldData);
    }

    /// <summary>
    /// Select用の初期値設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawDefaultSelectValueSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        EditorGUILayout.LabelField("Default設定", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        f_fieldData.IsDefaultValueNull = DrawSmallToggle(
            "  NULL",
            f_fieldData.IsDefaultValueNull);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        EditorGUI.BeginDisabledGroup(f_fieldData.IsDefaultValueNull);
        {
            if (f_fieldData.FieldType == CSE_CreateTools_FieldType.ScriptableObject)
            {
                f_fieldData.DefaultScriptableObjectValue = DrawSmallScriptableObjectDefaultField(
                    "  初期値",
                    f_fieldData);
            }
            else if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Script)
            {
                f_fieldData.DefaultScriptValue = DrawSmallScriptDefaultField(
                    "  初期値",
                    f_fieldData.DefaultScriptValue);
            }
            else if (f_fieldData.FieldType == CSE_CreateTools_FieldType.GameObject)
            {
                f_fieldData.DefaultGameObjectValue = DrawSmallGameObjectDefaultField(
                    "  初期値",
                    f_fieldData.DefaultGameObjectValue);
            }
        }
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// GameObject用の初期値ObjectFieldを描画します。
    /// </summary>
    /// <param name="f_label">表示ラベル</param>
    /// <param name="f_value">現在のGameObject</param>
    /// <returns>選択後のGameObject</returns>
    private GameObject DrawSmallGameObjectDefaultField(
        string f_label,
        GameObject f_value)
    {
        Rect rowRect = GetFieldInspectorRowRect();
        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

        EditorGUI.LabelField(labelRect, f_label);

        return (GameObject)EditorGUI.ObjectField(
            inputRect,
            f_value,
            typeof(GameObject),
            false);
    }

    /// <summary>
    /// MonoScriptがScriptableObject継承クラスか判定します。
    /// </summary>
    /// <param name="f_script">確認するMonoScript</param>
    /// <returns>ScriptableObject継承ならtrue</returns>
    private bool IsScriptableObjectMonoScript(MonoScript f_script)
    {
        if (f_script == null)
        {
            return false;
        }

        System.Type scriptType = f_script.GetClass();

        if (scriptType == null)
        {
            return false;
        }

        return typeof(ScriptableObject).IsAssignableFrom(scriptType);
    }

    /// <summary>
    /// ScriptableObject継承スクリプトを選択するFieldを描画します。
    /// </summary>
    /// <param name="f_label">表示ラベル</param>
    /// <param name="f_value">現在のMonoScript</param>
    /// <returns>選択後のMonoScript</returns>
    private MonoScript DrawSmallScriptableObjectTypeField(
        string f_label,
        MonoScript f_value)
    {
        Rect rowRect = GetFieldInspectorRowRect();
        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

        EditorGUI.LabelField(labelRect, f_label);

        MonoScript selectedScript = (MonoScript)EditorGUI.ObjectField(
            inputRect,
            f_value,
            typeof(MonoScript),
            false);

        if (selectedScript == null)
        {
            return null;
        }

        if (IsScriptableObjectMonoScript(selectedScript))
        {
            return selectedScript;
        }

        EditorUtility.DisplayDialog(
            "ScriptableObject Type Error",
            "ScriptableObjectを継承しているスクリプトだけ選択できます。",
            "OK");

        return f_value;
    }

    /// <summary>
    /// ScriptableObject Type項目を表示するか判定します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>表示する場合true</returns>
    private bool ShouldShowScriptableObjectTypeField(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.ScriptableObject)
        {
            return true;
        }

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.List &&
            f_fieldData.ListElementFieldType == CSE_CreateTools_FieldType.ScriptableObject)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Text Area用の詳細設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawTextAreaLayoutSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        DrawInputFieldCommonSettings(f_fieldData);

        GUILayout.Space(c_FieldInspectorSectionTopSpacing);

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.List)
        {
            DrawListDefaultValueSettingsByLayout(
                f_fieldData,
                CSE_CreateTools_FieldLayoutType.TextArea);
        }
        else
        {
            DrawDefaultTextAreaValueSettings(f_fieldData);
        }
    }

    /// <summary>
    /// Text Area用の初期値設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawDefaultTextAreaValueSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        EditorGUILayout.LabelField("Default設定", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        f_fieldData.IsDefaultValueNull = DrawSmallToggle(
            "  NULL",
            f_fieldData.IsDefaultValueNull);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        EditorGUI.BeginDisabledGroup(f_fieldData.IsDefaultValueNull);
        {
            if (f_fieldData.FieldType == CSE_CreateTools_FieldType.ScriptableObject)
            {
                f_fieldData.DefaultScriptableObjectValue = DrawSmallScriptableObjectDefaultField(
                    "  初期値",
                    f_fieldData);
            }
            else
            {
                f_fieldData.DefaultValueText = DrawSmallTextField(
                    "  初期値",
                    f_fieldData.DefaultValueText);
            }
        }
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// ScriptableObject用の初期値ObjectFieldを描画します。
    /// </summary>
    /// <param name="f_label">表示ラベル</param>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>選択後のScriptableObject</returns>
    private ScriptableObject DrawSmallScriptableObjectDefaultField(
        string f_label,
        CSED_CreateTools_FieldData f_fieldData)
    {
        Rect rowRect = GetFieldInspectorRowRect();
        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

        EditorGUI.LabelField(labelRect, f_label);

        System.Type targetType = GetSelectedScriptableObjectType(f_fieldData);

        UnityEngine.Object selectedObject = EditorGUI.ObjectField(
            inputRect,
            f_fieldData.DefaultScriptableObjectValue,
            targetType,
            false);

        return selectedObject as ScriptableObject;
    }

    /// <summary>
    /// Script用の初期値ObjectFieldを描画します。
    /// </summary>
    /// <param name="f_label">表示ラベル</param>
    /// <param name="f_value">現在のScript</param>
    /// <returns>選択後のScript</returns>
    private MonoScript DrawSmallScriptDefaultField(
        string f_label,
        MonoScript f_value)
    {
        Rect rowRect = GetFieldInspectorRowRect();
        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

        EditorGUI.LabelField(labelRect, f_label);

        return (MonoScript)EditorGUI.ObjectField(
            inputRect,
            f_value,
            typeof(MonoScript),
            false);
    }

    /// <summary>
    /// 選択中のScriptableObject型を取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>ScriptableObject型</returns>
    private System.Type GetSelectedScriptableObjectType(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.ScriptableObjectTypeScript == null)
        {
            return typeof(ScriptableObject);
        }

        System.Type scriptType = f_fieldData.ScriptableObjectTypeScript.GetClass();

        if (scriptType == null)
        {
            return typeof(ScriptableObject);
        }

        if (typeof(ScriptableObject).IsAssignableFrom(scriptType) == false)
        {
            return typeof(ScriptableObject);
        }

        return scriptType;
    }

    /// <summary>
    /// 小さめのTextAreaを描画します。
    /// </summary>
    /// <param name="f_label">表示ラベル</param>
    /// <param name="f_value">現在の文字列</param>
    /// <returns>入力後の文字列</returns>
    private string DrawSmallTextArea(string f_label, string f_value)
    {
        const float textAreaHeight = 64.0f;

        Rect rowRect = EditorGUILayout.GetControlRect(
            false,
            textAreaHeight);

        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

        EditorGUI.LabelField(labelRect, f_label);

        return EditorGUI.TextArea(inputRect, f_value);
    }

    /// <summary>
    /// Input Field用の詳細設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawInputFieldLayoutSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        DrawInputFieldCommonSettings(f_fieldData);

        GUILayout.Space(c_FieldInspectorSectionTopSpacing);

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.List)
        {
            DrawListDefaultValueSettingsByLayout(
                f_fieldData,
                CSE_CreateTools_FieldLayoutType.InputField);
        }
        else
        {
            DrawDefaultValueSettings(f_fieldData);
        }
    }

    /// <summary>
    /// List用の初期値設定をLayoutに応じて描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    /// <param name="f_layoutType">適用するLayout</param>
    private void DrawListDefaultValueSettingsByLayout(
        CSED_CreateTools_FieldData f_fieldData,
        CSE_CreateTools_FieldLayoutType f_layoutType)
    {
        EnsureListDefaultElementValueList(f_fieldData);

        EditorGUILayout.LabelField("Default設定", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        DrawListElementCountControl(f_fieldData);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        f_fieldData.IsListDefaultValueNull = DrawSmallToggle(
            "  NULL",
            f_fieldData.IsListDefaultValueNull);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        EditorGUI.BeginDisabledGroup(f_fieldData.IsListDefaultValueNull);
        {
            for (int i = 0 ; i < f_fieldData.ListDefaultElementValueTextList.Count ; i++)
            {
                DrawListDefaultElementByLayout(f_fieldData, f_layoutType, i);

                GUILayout.Space(c_FieldInspectorRowSpacing);
            }
        }
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// ListのDefault要素をLayoutに応じて描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    /// <param name="f_layoutType">Layout種別</param>
    /// <param name="f_index">要素番号</param>
    private void DrawListDefaultElementByLayout(
        CSED_CreateTools_FieldData f_fieldData,
        CSE_CreateTools_FieldLayoutType f_layoutType,
        int f_index)
    {
        if (f_index < 0 ||
            f_index >= f_fieldData.ListDefaultElementValueTextList.Count)
        {
            return;
        }

        if (IsVectorFieldType(f_fieldData.ListElementFieldType))
        {
            DrawListDefaultVectorElement(f_fieldData, f_index);
            return;
        }

        if (f_layoutType == CSE_CreateTools_FieldLayoutType.MinMaxField)
        {
            DrawListDefaultMinMaxElement(f_fieldData, f_index);
            return;
        }

        if (f_layoutType == CSE_CreateTools_FieldLayoutType.Toggle)
        {
            DrawListDefaultToggleElement(f_fieldData, f_index);
            return;
        }

        if (f_layoutType == CSE_CreateTools_FieldLayoutType.TextArea)
        {
            DrawListDefaultTextAreaElement(f_fieldData, f_index);
            return;
        }

        if (f_layoutType == CSE_CreateTools_FieldLayoutType.Select)
        {
            DrawListDefaultSelectElement(f_fieldData, f_index);
            return;
        }

        if (f_layoutType == CSE_CreateTools_FieldLayoutType.Dropdown)
        {
            DrawListDefaultEnumElement(f_fieldData, f_index);
            return;
        }

        DrawListDefaultSingleValueElement(f_fieldData, f_index);
    }

    /// <summary>
    /// ListのEnum要素を描画します。
    /// </summary>
    private void DrawListDefaultEnumElement(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        EnsureListDefaultElementValueList(f_fieldData);

        if (f_index < 0 ||
            f_index >= f_fieldData.ListDefaultElementValueTextList.Count)
        {
            return;
        }

        f_fieldData.ListDefaultElementValueTextList[f_index] =
            DrawSmallEnumValuePopup(
                "  Element " + f_index.ToString(),
                f_fieldData,
                f_fieldData.ListDefaultElementValueTextList[f_index]);
    }

    /// <summary>
    /// ListのVector要素をX/Y/Z入力として描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    /// <param name="f_index">List要素番号</param>
    private void DrawListDefaultVectorElement(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        int componentCount = GetVectorFieldComponentCount(f_fieldData.ListElementFieldType);

        EditorGUILayout.LabelField(
            "  Element " + f_index.ToString(),
            EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        for (int i = 0 ; i < componentCount ; i++)
        {
            string componentLabel = GetVectorComponentLabel(i);
            string componentValue = GetListVectorDefaultComponentText(
                f_fieldData,
                f_index,
                i);

            componentValue = DrawSmallTextField(
                "    " + componentLabel,
                componentValue);

            SetListVectorDefaultComponentText(
                f_fieldData,
                f_index,
                i,
                componentValue);

            GUILayout.Space(c_FieldInspectorRowSpacing);
        }
    }

    /// <summary>
    /// List内Vector初期値の指定要素を取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <param name="f_listIndex">List要素番号</param>
    /// <param name="f_componentIndex">Vector要素番号</param>
    /// <returns>要素文字列</returns>
    private string GetListVectorDefaultComponentText(
        CSED_CreateTools_FieldData f_fieldData,
        int f_listIndex,
        int f_componentIndex)
    {
        string[] values = GetListVectorDefaultValueParts(f_fieldData, f_listIndex);

        if (f_componentIndex < 0 || f_componentIndex >= values.Length)
        {
            return "0";
        }

        if (string.IsNullOrEmpty(values[f_componentIndex]))
        {
            return "0";
        }

        return values[f_componentIndex];
    }

    /// <summary>
    /// List内Vector初期値の指定要素を設定します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <param name="f_listIndex">List要素番号</param>
    /// <param name="f_componentIndex">Vector要素番号</param>
    /// <param name="f_value">設定値</param>
    private void SetListVectorDefaultComponentText(
        CSED_CreateTools_FieldData f_fieldData,
        int f_listIndex,
        int f_componentIndex,
        string f_value)
    {
        EnsureListDefaultElementValueList(f_fieldData);

        if (f_listIndex < 0 ||
            f_listIndex >= f_fieldData.ListDefaultElementValueTextList.Count)
        {
            return;
        }

        int componentCount = GetVectorFieldComponentCount(f_fieldData.ListElementFieldType);
        string[] values = GetListVectorDefaultValueParts(f_fieldData, f_listIndex);

        if (values.Length != componentCount)
        {
            System.Array.Resize(ref values, componentCount);
        }

        for (int i = 0 ; i < values.Length ; i++)
        {
            if (string.IsNullOrEmpty(values[i]))
            {
                values[i] = "0";
            }
        }

        if (f_componentIndex >= 0 && f_componentIndex < values.Length)
        {
            values[f_componentIndex] = f_value;
        }

        f_fieldData.ListDefaultElementValueTextList[f_listIndex] =
            string.Join(",", values);
    }

    /// <summary>
    /// List内Vector初期値文字列を分解します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <param name="f_listIndex">List要素番号</param>
    /// <returns>分解後の値配列</returns>
    private string[] GetListVectorDefaultValueParts(
        CSED_CreateTools_FieldData f_fieldData,
        int f_listIndex)
    {
        int componentCount = GetVectorFieldComponentCount(f_fieldData.ListElementFieldType);
        string[] values = new string[componentCount];

        if (f_fieldData.ListDefaultElementValueTextList == null ||
            f_listIndex < 0 ||
            f_listIndex >= f_fieldData.ListDefaultElementValueTextList.Count)
        {
            for (int i = 0 ; i < values.Length ; i++)
            {
                values[i] = "0";
            }

            return values;
        }

        string sourceText = f_fieldData.ListDefaultElementValueTextList[f_listIndex];

        if (string.IsNullOrEmpty(sourceText) == false)
        {
            string[] splitValues = sourceText.Split(',');

            for (int i = 0 ; i < splitValues.Length && i < values.Length ; i++)
            {
                values[i] = splitValues[i];
            }
        }

        for (int i = 0 ; i < values.Length ; i++)
        {
            if (string.IsNullOrEmpty(values[i]))
            {
                values[i] = "0";
            }
        }

        return values;
    }

    /// <summary>
    /// ListのSelect要素を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    /// <param name="f_index">要素番号</param>
    private void DrawListDefaultSelectElement(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        EnsureListDefaultElementValueList(f_fieldData);

        if (f_fieldData.ListDefaultObjectValueList == null)
        {
            return;
        }

        if (f_index < 0 ||
            f_index >= f_fieldData.ListDefaultObjectValueList.Count)
        {
            return;
        }

        Rect rowRect = GetFieldInspectorRowRect();
        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

        EditorGUI.LabelField(
            labelRect,
            "  Element " + f_index.ToString());

        if (f_fieldData.ListElementFieldType == CSE_CreateTools_FieldType.ScriptableObject)
        {
            System.Type targetType = GetSelectedScriptableObjectType(f_fieldData);

            f_fieldData.ListDefaultObjectValueList[f_index] = EditorGUI.ObjectField(
                inputRect,
                f_fieldData.ListDefaultObjectValueList[f_index],
                targetType,
                false);

            return;
        }

        if (f_fieldData.ListElementFieldType == CSE_CreateTools_FieldType.Script)
        {
            f_fieldData.ListDefaultObjectValueList[f_index] = EditorGUI.ObjectField(
                inputRect,
                f_fieldData.ListDefaultObjectValueList[f_index],
                typeof(MonoScript),
                false);

            return;
        }

        if (f_fieldData.ListElementFieldType == CSE_CreateTools_FieldType.GameObject)
        {
            f_fieldData.ListDefaultObjectValueList[f_index] = EditorGUI.ObjectField(
                inputRect,
                f_fieldData.ListDefaultObjectValueList[f_index],
                typeof(GameObject),
                false);

            return;
        }

        EditorGUI.LabelField(inputRect, "Select未対応Type");
    }

    /// <summary>
    /// Listの単一値要素を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    /// <param name="f_index">要素番号</param>
    private void DrawListDefaultSingleValueElement(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        f_fieldData.ListDefaultElementValueTextList[f_index] = DrawSmallTextField(
            "  Element " + f_index.ToString(),
            f_fieldData.ListDefaultElementValueTextList[f_index]);
    }

    /// <summary>
    /// ListのToggle要素を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    /// <param name="f_index">要素番号</param>
    private void DrawListDefaultToggleElement(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        bool value = false;

        bool.TryParse(
            f_fieldData.ListDefaultElementValueTextList[f_index],
            out value);

        value = DrawSmallToggle(
            "  Element " + f_index.ToString(),
            value);

        f_fieldData.ListDefaultElementValueTextList[f_index] = value.ToString();
    }

    /// <summary>
    /// ListのTextArea要素を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    /// <param name="f_index">要素番号</param>
    private void DrawListDefaultTextAreaElement(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        f_fieldData.ListDefaultElementValueTextList[f_index] = DrawSmallTextArea(
            "  Element " + f_index.ToString(),
            f_fieldData.ListDefaultElementValueTextList[f_index]);
    }

    /// <summary>
    /// ListのMinMax要素を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    /// <param name="f_index">要素番号</param>
    private void DrawListDefaultMinMaxElement(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        EnsureListDefaultElementValueList(f_fieldData);

        if (f_index < 0 ||
            f_index >= f_fieldData.ListDefaultMinValueTextList.Count ||
            f_index >= f_fieldData.ListDefaultMaxValueTextList.Count)
        {
            return;
        }

        Rect rowRect = GetFieldInspectorRowRect();
        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

        float smallLabelWidth = 26.0f;
        float spacing = 4.0f;

        float fieldWidth =
            (inputRect.width
            - smallLabelWidth
            - smallLabelWidth
            - (spacing * 3.0f)) * 0.5f;

        fieldWidth = Mathf.Max(24.0f, fieldWidth);

        Rect minLabelRect = new Rect(
            inputRect.x,
            inputRect.y,
            smallLabelWidth,
            inputRect.height);

        Rect minValueRect = new Rect(
            minLabelRect.xMax + spacing,
            inputRect.y,
            fieldWidth,
            inputRect.height);

        Rect maxLabelRect = new Rect(
            minValueRect.xMax + spacing,
            inputRect.y,
            smallLabelWidth,
            inputRect.height);

        Rect maxValueRect = new Rect(
            maxLabelRect.xMax + spacing,
            inputRect.y,
            fieldWidth,
            inputRect.height);

        EditorGUI.LabelField(
            labelRect,
            "  Element " + f_index.ToString());

        EditorGUI.LabelField(minLabelRect, "Min");

        f_fieldData.ListDefaultMinValueTextList[f_index] = EditorGUI.TextField(
            minValueRect,
            f_fieldData.ListDefaultMinValueTextList[f_index]);

        EditorGUI.LabelField(maxLabelRect, "Max");

        f_fieldData.ListDefaultMaxValueTextList[f_index] = EditorGUI.TextField(
            maxValueRect,
            f_fieldData.ListDefaultMaxValueTextList[f_index]);
    }



    /// <summary>
    /// Input Field共通設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawInputFieldCommonSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        EditorGUILayout.LabelField("Input Field設定", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        DrawTagNameField(f_fieldData);
    }

    /// <summary>
    /// Tag Name項目だけを描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawTagNameField(CSED_CreateTools_FieldData f_fieldData)
    {
        f_fieldData.TagName = DrawSmallTextField(
            "  タグ名",
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

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.List)
        {
            DrawListDefaultValueSettingsByLayout(
                f_fieldData,
                CSE_CreateTools_FieldLayoutType.Slider);
        }
        else
        {
            DrawDefaultValueSettings(f_fieldData);
        }

        GUILayout.Space(c_FieldInspectorSectionTopSpacing);

        DrawSliderRangeSettings(f_fieldData);
    }

    /// <summary>
    /// Toggle用の詳細設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawToggleLayoutSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        DrawInputFieldCommonSettings(f_fieldData);

        GUILayout.Space(c_FieldInspectorSectionTopSpacing);

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.List)
        {
            DrawListDefaultValueSettingsByLayout(
                f_fieldData,
                CSE_CreateTools_FieldLayoutType.Toggle);
        }
        else
        {
            DrawDefaultBoolValueSettings(f_fieldData);
        }
    }

    /// <summary>
    /// bool用の通常初期値設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawDefaultBoolValueSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        EditorGUILayout.LabelField("Default設定", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        f_fieldData.IsDefaultValueNull = DrawSmallToggle(
            "  NULL",
            f_fieldData.IsDefaultValueNull);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        EditorGUI.BeginDisabledGroup(f_fieldData.IsDefaultValueNull);
        {
            bool boolValue = GetFieldDataDefaultBoolValue(f_fieldData);

            boolValue = DrawSmallToggle(
                "  初期値",
                boolValue);

            f_fieldData.DefaultValueText = boolValue.ToString();
        }
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// FieldDataのDefaultValueTextをboolとして取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>bool値</returns>
    private bool GetFieldDataDefaultBoolValue(CSED_CreateTools_FieldData f_fieldData)
    {
        bool result = false;

        bool.TryParse(
            f_fieldData.DefaultValueText,
            out result);

        return result;
    }

    /// <summary>
    /// 通常初期値設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawDefaultValueSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        if (IsVectorFieldType(f_fieldData.FieldType))
        {
            DrawDefaultVectorValueSettings(f_fieldData);
            return;
        }

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
    /// Vector系の初期値設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawDefaultVectorValueSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        EditorGUILayout.LabelField("Default設定", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        f_fieldData.IsDefaultValueNull = DrawSmallToggle(
            "  Default Is Null",
            f_fieldData.IsDefaultValueNull);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        EditorGUI.BeginDisabledGroup(f_fieldData.IsDefaultValueNull);
        {
            int componentCount = GetVectorFieldComponentCount(f_fieldData.FieldType);

            for (int i = 0 ; i < componentCount ; i++)
            {
                string componentLabel = GetVectorComponentLabel(i);
                string componentValue = GetVectorDefaultComponentText(f_fieldData, i);

                componentValue = DrawSmallTextField(
                    "  " + componentLabel,
                    componentValue);

                SetVectorDefaultComponentText(f_fieldData, i, componentValue);

                GUILayout.Space(c_FieldInspectorRowSpacing);
            }
        }
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// Vector系のFieldTypeか判定します。
    /// </summary>
    /// <param name="f_fieldType">確認するFieldType</param>
    /// <returns>Vector系ならtrue</returns>
    private bool IsVectorFieldType(CSE_CreateTools_FieldType f_fieldType)
    {
        return
            f_fieldType == CSE_CreateTools_FieldType.Vector2Int ||
            f_fieldType == CSE_CreateTools_FieldType.Vector3Int ||
            f_fieldType == CSE_CreateTools_FieldType.Vector2 ||
            f_fieldType == CSE_CreateTools_FieldType.Vector3;
    }

    /// <summary>
    /// Vector系Fieldの要素数を取得します。
    /// </summary>
    /// <param name="f_fieldType">FieldType</param>
    /// <returns>要素数</returns>
    private int GetVectorFieldComponentCount(CSE_CreateTools_FieldType f_fieldType)
    {
        if (f_fieldType == CSE_CreateTools_FieldType.Vector3Int ||
            f_fieldType == CSE_CreateTools_FieldType.Vector3)
        {
            return 3;
        }

        return 2;
    }

    /// <summary>
    /// Vector要素ラベルを取得します。
    /// </summary>
    /// <param name="f_index">要素番号</param>
    /// <returns>要素ラベル</returns>
    private string GetVectorComponentLabel(int f_index)
    {
        switch (f_index)
        {
            case 0:
                return "X";

            case 1:
                return "Y";

            case 2:
                return "Z";

            default:
                return "Value";
        }
    }

    /// <summary>
    /// Vector初期値の指定要素を取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <param name="f_index">要素番号</param>
    /// <returns>要素文字列</returns>
    private string GetVectorDefaultComponentText(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        string[] values = GetVectorDefaultValueParts(f_fieldData);

        if (f_index < 0 || f_index >= values.Length)
        {
            return "0";
        }

        if (string.IsNullOrEmpty(values[f_index]))
        {
            return "0";
        }

        return values[f_index];
    }

    /// <summary>
    /// Vector初期値の指定要素を設定します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <param name="f_index">要素番号</param>
    /// <param name="f_value">設定値</param>
    private void SetVectorDefaultComponentText(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index,
        string f_value)
    {
        int componentCount = GetVectorFieldComponentCount(f_fieldData.FieldType);
        string[] values = GetVectorDefaultValueParts(f_fieldData);

        if (values.Length != componentCount)
        {
            System.Array.Resize(ref values, componentCount);
        }

        for (int i = 0 ; i < values.Length ; i++)
        {
            if (string.IsNullOrEmpty(values[i]))
            {
                values[i] = "0";
            }
        }

        if (f_index >= 0 && f_index < values.Length)
        {
            values[f_index] = f_value;
        }

        f_fieldData.DefaultValueText = string.Join(",", values);
    }

    /// <summary>
    /// Vector初期値文字列を分解します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>分解後の値配列</returns>
    private string[] GetVectorDefaultValueParts(CSED_CreateTools_FieldData f_fieldData)
    {
        int componentCount = GetVectorFieldComponentCount(f_fieldData.FieldType);

        string[] values = new string[componentCount];

        if (string.IsNullOrEmpty(f_fieldData.DefaultValueText) == false)
        {
            string[] splitValues = f_fieldData.DefaultValueText.Split(',');

            for (int i = 0 ; i < splitValues.Length && i < values.Length ; i++)
            {
                values[i] = splitValues[i];
            }
        }

        for (int i = 0 ; i < values.Length ; i++)
        {
            if (string.IsNullOrEmpty(values[i]))
            {
                values[i] = "0";
            }
        }

        return values;
    }

    /// <summary>
    /// Slider範囲設定を描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawSliderRangeSettings(CSED_CreateTools_FieldData f_fieldData)
    {
        EditorGUILayout.LabelField("Slider Range設定", EditorStyles.boldLabel);

        GUILayout.Space(c_FieldInspectorSectionTitleBottomSpacing);

        f_fieldData.IsSliderMinValueNull = DrawSmallToggle(
            "  Min Is Null",
            f_fieldData.IsSliderMinValueNull);

        GUILayout.Space(c_FieldInspectorRowSpacing);

        EditorGUI.BeginDisabledGroup(f_fieldData.IsSliderMinValueNull);
        {
            f_fieldData.SliderMinValueText = DrawSmallTextField(
                "  Slider Min Value",
                f_fieldData.SliderMinValueText);
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(c_FieldInspectorSectionTopSpacing);

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
        DrawInputFieldCommonSettings(f_fieldData);

        GUILayout.Space(c_FieldInspectorSectionTopSpacing);

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.List)
        {
            DrawListDefaultValueSettingsByLayout(
                f_fieldData,
                CSE_CreateTools_FieldLayoutType.MinMaxField);

            return;
        }

        EditorGUILayout.LabelField("Default Min", EditorStyles.boldLabel);

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

        EditorGUILayout.LabelField("Default Max", EditorStyles.boldLabel);

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


    /// <summary>
    /// List初期値リストを使用可能な状態にします。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    private void EnsureListDefaultElementValueList(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.ListDefaultElementValueTextList == null)
        {
            f_fieldData.ListDefaultElementValueTextList = new List<string>();
        }

        if (f_fieldData.ListDefaultMinValueTextList == null)
        {
            f_fieldData.ListDefaultMinValueTextList = new List<string>();
        }

        if (f_fieldData.ListDefaultMaxValueTextList == null)
        {
            f_fieldData.ListDefaultMaxValueTextList = new List<string>();
        }

        if (f_fieldData.ListDefaultObjectValueList == null)
        {
            f_fieldData.ListDefaultObjectValueList = new List<UnityEngine.Object>();
        }

        while (f_fieldData.ListDefaultObjectValueList.Count < f_fieldData.ListDefaultElementValueTextList.Count)
        {
            f_fieldData.ListDefaultObjectValueList.Add(null);
        }

        while (f_fieldData.ListDefaultObjectValueList.Count > f_fieldData.ListDefaultElementValueTextList.Count)
        {
            f_fieldData.ListDefaultObjectValueList.RemoveAt(f_fieldData.ListDefaultObjectValueList.Count - 1);
        }

        while (f_fieldData.ListDefaultMinValueTextList.Count < f_fieldData.ListDefaultElementValueTextList.Count)
        {
            f_fieldData.ListDefaultMinValueTextList.Add("0");
        }

        while (f_fieldData.ListDefaultMaxValueTextList.Count < f_fieldData.ListDefaultElementValueTextList.Count)
        {
            f_fieldData.ListDefaultMaxValueTextList.Add("1");
        }

        while (f_fieldData.ListDefaultMinValueTextList.Count > f_fieldData.ListDefaultElementValueTextList.Count)
        {
            f_fieldData.ListDefaultMinValueTextList.RemoveAt(f_fieldData.ListDefaultMinValueTextList.Count - 1);
        }

        while (f_fieldData.ListDefaultMaxValueTextList.Count > f_fieldData.ListDefaultElementValueTextList.Count)
        {
            f_fieldData.ListDefaultMaxValueTextList.RemoveAt(f_fieldData.ListDefaultMaxValueTextList.Count - 1);
        }
    }

    private void AddListDefaultElement(CSED_CreateTools_FieldData f_fieldData)
    {
        EnsureListDefaultElementValueList(f_fieldData);

        f_fieldData.ListDefaultElementValueTextList.Add(
            CreateDefaultListElementValue(f_fieldData.ListElementFieldType));

        f_fieldData.ListDefaultObjectValueList.Add(null);

        Repaint();
    }

    /// <summary>
    /// <summary>
    /// List初期値要素を末尾から削除します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    private void RemoveListDefaultElement(CSED_CreateTools_FieldData f_fieldData)
    {
        EnsureListDefaultElementValueList(f_fieldData);

        if (f_fieldData.ListDefaultElementValueTextList.Count <= 0)
        {
            return;
        }

        int removeIndex = f_fieldData.ListDefaultElementValueTextList.Count - 1;

        f_fieldData.ListDefaultElementValueTextList.RemoveAt(removeIndex);

        if (removeIndex < f_fieldData.ListDefaultObjectValueList.Count)
        {
            f_fieldData.ListDefaultObjectValueList.RemoveAt(removeIndex);
        }

        Repaint();
    }

    /// <summary>
    /// List要素型に応じた初期値文字列を作成します。
    /// </summary>
    /// <param name="f_fieldType">List要素型</param>
    /// <returns>初期値文字列</returns>
    private string CreateDefaultListElementValue(CSE_CreateTools_FieldType f_fieldType)
    {
        switch (f_fieldType)
        {
            case CSE_CreateTools_FieldType.Int:
                return "0";

            case CSE_CreateTools_FieldType.Float:
                return "0";

            case CSE_CreateTools_FieldType.Bool:
                return "False";

            case CSE_CreateTools_FieldType.String:
                return string.Empty;

            case CSE_CreateTools_FieldType.ScriptableObject:
                return string.Empty;

            case CSE_CreateTools_FieldType.Script:
                return string.Empty;

            case CSE_CreateTools_FieldType.GameObject:
                return string.Empty;

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// Listの要素数変更UIを描画します。
    /// </summary>
    /// <param name="f_fieldData">選択中Fieldデータ</param>
    private void DrawListElementCountControl(CSED_CreateTools_FieldData f_fieldData)
    {
        Rect rowRect = GetFieldInspectorRowRect();
        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

        float buttonWidth = 24.0f;
        float countWidth = 40.0f;
        float spacing = 4.0f;

        Rect minusRect = new Rect(
            inputRect.x,
            inputRect.y,
            buttonWidth,
            inputRect.height);

        Rect countRect = new Rect(
            minusRect.xMax + spacing,
            inputRect.y,
            countWidth,
            inputRect.height);

        Rect plusRect = new Rect(
            countRect.xMax + spacing,
            inputRect.y,
            buttonWidth,
            inputRect.height);

        EditorGUI.LabelField(labelRect, "  要素数");

        if (GUI.Button(minusRect, "-"))
        {
            RemoveListDefaultElement(f_fieldData);
        }

        EditorGUI.LabelField(
            countRect,
            f_fieldData.ListDefaultElementValueTextList.Count.ToString());

        if (GUI.Button(plusRect, "+"))
        {
            AddListDefaultElement(f_fieldData);
        }
    }

    /// <summary>
    /// <summary>
    /// Listの中身の型を選択するPopupを描画します。
    /// </summary>
    /// <param name="f_label">表示ラベル</param>
    /// <param name="f_value">現在のList中身の型</param>
    /// <returns>選択後のList中身の型</returns>
    private CSE_CreateTools_FieldType DrawSmallListElementFieldTypePopup(
        string f_label,
        CSE_CreateTools_FieldType f_value)
    {
        string[] displayNames =
        {
        "int",
        "Vector2Int",
        "Vector3Int",
        "float",
        "Vector2",
        "Vector3",
        "string",
        "bool",
        "enum",
        "ScriptableObject",
        "Script",
        "GameObject"
    };

        CSE_CreateTools_FieldType[] values =
        {
        CSE_CreateTools_FieldType.Int,
        CSE_CreateTools_FieldType.Vector2Int,
        CSE_CreateTools_FieldType.Vector3Int,
        CSE_CreateTools_FieldType.Float,
        CSE_CreateTools_FieldType.Vector2,
        CSE_CreateTools_FieldType.Vector3,
        CSE_CreateTools_FieldType.String,
        CSE_CreateTools_FieldType.Bool,
        CSE_CreateTools_FieldType.Enum,
        CSE_CreateTools_FieldType.ScriptableObject,
        CSE_CreateTools_FieldType.Script,
        CSE_CreateTools_FieldType.GameObject
    };

        int selectedIndex = 0;

        for (int i = 0 ; i < values.Length ; i++)
        {
            if (values[i] == f_value)
            {
                selectedIndex = i;
                break;
            }
        }

        Rect rowRect = GetFieldInspectorRowRect();
        Rect labelRect = GetFieldInspectorLabelRect(rowRect);
        Rect inputRect = GetFieldInspectorInputRect(rowRect);

        EditorGUI.LabelField(labelRect, f_label);

        selectedIndex = EditorGUI.Popup(
            inputRect,
            selectedIndex,
            displayNames);

        return values[selectedIndex];
    }
}
#endif
