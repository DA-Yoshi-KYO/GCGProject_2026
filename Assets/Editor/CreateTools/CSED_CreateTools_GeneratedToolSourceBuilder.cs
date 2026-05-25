/*
+=====================================
 ファイル名 : CSED_CreateTools_GeneratedToolSourceBuilder.cs
 概要     : CreateToolsで生成するEditorWindowソースコードを作成するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/18 新規作成
            2026/05/18 FieldDataを元に生成EditorWindowへ項目を出力
=====================================+
*/

#if UNITY_EDITOR
using System.Text;
using UnityEngine;

/// <summary>
/// CreateToolsのEditorWindowソース生成処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// 生成EditorWindowのソースコードを作成します。
    /// </summary>
    /// <param name="f_className">生成するEditorWindowクラス名</param>
    /// <param name="f_dataClassName">生成するScriptableObjectクラス名</param>
    /// <returns>生成ソースコード</returns>
    private string CreateGeneratedEditorToolSource(
        string f_className,
        string f_dataClassName)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("/*");
        builder.AppendLine("+=====================================");
        builder.AppendLine(" ファイル名 : " + f_className + ".cs");
        builder.AppendLine(" 概要     : " + GetGeneratedHeaderCommentText(
            m_GeneratedEditorHeaderContents,
            "CreateToolsから自動生成されたEditorWindow"));
        builder.AppendLine(" 作者     : " + GetGeneratedHeaderCommentText(
            m_GeneratedHeaderAuthorName,
            "ヨシモト リョウ"));
        builder.AppendLine(" 履歴     : " + GetGeneratedHeaderCommentText(
            m_GeneratedHeaderHistoryDate,
            System.DateTime.Now.ToString("yyyy/MM/dd")) + " CreateToolsから自動生成");
        builder.AppendLine("=====================================+");
        builder.AppendLine("*/");
        builder.AppendLine();
        builder.AppendLine("#if UNITY_EDITOR");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using UnityEditor;");
        builder.AppendLine("using UnityEngine;");
        builder.AppendLine();
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// CreateToolsから自動生成されたEditorWindowです。");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("public class " + f_className + " : EditorWindow, IHasCustomMenu");
        builder.AppendLine("{");

        AppendGeneratedFieldVariables(builder);

        builder.AppendLine("    /// <summary>");
        builder.AppendLine("    /// メイン画面のスクロール位置です。");
        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    private Vector2 m_MainScrollPosition;");
        builder.AppendLine();

        builder.AppendLine("    /// <summary>");
        builder.AppendLine("    /// メニューからウィンドウを開きます。");
        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    [MenuItem(\"" + EscapeString(m_GeneratedToolMenuPath) + "\")]");
        builder.AppendLine("    public static void ShowWindow()");
        builder.AppendLine("    {");
        builder.AppendLine("        " + f_className + " window = GetWindow<" + f_className + ">(\"" + EscapeString(m_GeneratedToolWindowTitle) + "\");");
        builder.AppendLine("        window.minSize = new Vector2(360.0f, 240.0f);");
        builder.AppendLine("        " + f_className + "_CreatedAssetsWindow.OpenWindow();");
        builder.AppendLine("        window.Focus();");
        builder.AppendLine("    }");
        builder.AppendLine();

        builder.AppendLine("    /// <summary>");
        builder.AppendLine("    /// EditorWindow右上メニューに項目を追加します。");
        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    /// <param name=\"f_menu\">追加先メニュー</param>");
        builder.AppendLine("    public void AddItemsToMenu(GenericMenu f_menu)");
        builder.AppendLine("    {");
        builder.AppendLine("        f_menu.AddItem(");
        builder.AppendLine("            new GUIContent(\"Create Asset Settings\"),");
        builder.AppendLine("            false,");
        builder.AppendLine("            OpenCreateAssetSettings);");
        builder.AppendLine("    }");
        builder.AppendLine();

        builder.AppendLine("    /// <summary>");
        builder.AppendLine("    /// 作成済みAsset一覧Windowを開きます。");
        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    private void OpenCreatedAssetsWindow()");
        builder.AppendLine("    {");
        builder.AppendLine("        " + f_className + "_CreatedAssetsWindow.OpenWindow();");
        builder.AppendLine("    }");
        builder.AppendLine();

        builder.AppendLine("    /// <summary>");
        builder.AppendLine("    /// Create Asset設定を開きます。");
        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    private void OpenCreateAssetSettings()");
        builder.AppendLine("    {");
        builder.AppendLine("        CreateAssetSettingsWindow.Open(this);");
        builder.AppendLine("    }");
        builder.AppendLine();

        builder.AppendLine("    /// <summary>");
        builder.AppendLine("    /// GUIを描画します。");
        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    private void OnGUI()");
        builder.AppendLine("    {");
        builder.AppendLine("        m_MainScrollPosition = EditorGUILayout.BeginScrollView(m_MainScrollPosition);");
        builder.AppendLine("        {");

        AppendGeneratedOnGui(builder);
        AppendGeneratedCreateScriptableObjectButton(builder);

        builder.AppendLine("        }");
        builder.AppendLine("        EditorGUILayout.EndScrollView();");
        builder.AppendLine("    }");
        builder.AppendLine();

        AppendGeneratedCreateScriptableObjectMembers(builder, f_dataClassName, f_className);

        AppendGeneratedCreateAssetSettingsWindow(builder, f_className);

        builder.AppendLine("}");

        AppendGeneratedCreatedAssetsWindow(builder, f_className, f_dataClassName);

        builder.AppendLine("#endif");

        return builder.ToString();
    }

    /// <summary>
    /// 生成対象Fieldが単体MinMaxFieldか判定します。
    /// </summary>
    /// <param name="f_fieldData">確認するFieldData</param>
    /// <returns>単体MinMaxFieldの場合はtrue</returns>
    private bool IsGeneratedSingleMinMaxField(CSED_CreateTools_FieldData f_fieldData)
    {
        return
            f_fieldData.FieldType != CSE_CreateTools_FieldType.List &&
            f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.MinMaxField;
    }

    /// <summary>
    /// Min用変数名を取得します。
    /// </summary>
    /// <param name="f_baseVariableName">元の変数名</param>
    /// <returns>Min用変数名</returns>
    private string CreateGeneratedMinVariableName(string f_baseVariableName)
    {
        return f_baseVariableName + "Min";
    }

    /// <summary>
    /// Max用変数名を取得します。
    /// </summary>
    /// <param name="f_baseVariableName">元の変数名</param>
    /// <returns>Max用変数名</returns>
    private string CreateGeneratedMaxVariableName(string f_baseVariableName)
    {
        return f_baseVariableName + "Max";
    }

    /// <summary>
    /// MinMaxField用のMin初期値文字列を取得します。
    /// </summary>
    /// <param name="f_fieldData">FieldData</param>
    /// <returns>Min初期値文字列</returns>
    private string GetGeneratedMinDefaultValueText(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Int)
        {
            if (f_fieldData.IsDefaultMinValueNull)
            {
                return "0";
            }

            return GetGeneratedIntText(f_fieldData.DefaultMinValueText);
        }

        if (f_fieldData.IsDefaultMinValueNull)
        {
            return "0.0f";
        }

        return GetGeneratedFloatText(f_fieldData.DefaultMinValueText);
    }

    /// <summary>
    /// MinMaxField用のMax初期値文字列を取得します。
    /// </summary>
    /// <param name="f_fieldData">FieldData</param>
    /// <returns>Max初期値文字列</returns>
    private string GetGeneratedMaxDefaultValueText(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Int)
        {
            if (f_fieldData.IsDefaultMaxValueNull)
            {
                return "0";
            }

            return GetGeneratedIntText(f_fieldData.DefaultMaxValueText);
        }

        if (f_fieldData.IsDefaultMaxValueNull)
        {
            return "0.0f";
        }

        return GetGeneratedFloatText(f_fieldData.DefaultMaxValueText);
    }

    /// <summary>
    /// 生成EditorWindowの変数定義を追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    private void AppendGeneratedFieldVariables(StringBuilder f_builder)
    {
        if (m_FieldDataList == null)
        {
            return;
        }

        for (int i = 0 ; i < m_FieldDataList.Count ; i++)
        {
            CSED_CreateTools_FieldData fieldData = m_FieldDataList[i];

            string variableName = CreateGeneratedVariableName(fieldData.FieldName, i);
            string typeName = GetGeneratedFieldTypeName(fieldData);
            string defaultValue = GetGeneratedDefaultValueText(fieldData);

            if (IsGeneratedSingleMinMaxField(fieldData))
            {
                string minVariableName = CreateGeneratedMinVariableName(variableName);
                string maxVariableName = CreateGeneratedMaxVariableName(variableName);

                f_builder.AppendLine("    /// <summary>");
                f_builder.AppendLine("    /// " + minVariableName + "です。");
                f_builder.AppendLine("    /// </summary>");
                f_builder.AppendLine("    private " + typeName + " " + minVariableName + " = " + GetGeneratedMinDefaultValueText(fieldData) + ";");
                f_builder.AppendLine();

                f_builder.AppendLine("    /// <summary>");
                f_builder.AppendLine("    /// " + maxVariableName + "です。");
                f_builder.AppendLine("    /// </summary>");
                f_builder.AppendLine("    private " + typeName + " " + maxVariableName + " = " + GetGeneratedMaxDefaultValueText(fieldData) + ";");
                f_builder.AppendLine();

                continue;
            }

            f_builder.AppendLine("    /// <summary>");
            f_builder.AppendLine("    /// " + variableName + "です。");
            f_builder.AppendLine("    /// </summary>");
            f_builder.AppendLine("    private " + typeName + " " + variableName + " = " + defaultValue + ";");
            f_builder.AppendLine();
        }
    }

    /// <summary>
    /// 生成EditorWindowのOnGUI描画処理を追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    private void AppendGeneratedOnGui(StringBuilder f_builder)
    {
        if (m_FieldDataList == null)
        {
            return;
        }

        for (int i = 0 ; i < m_FieldDataList.Count ; i++)
        {
            CSED_CreateTools_FieldData fieldData = m_FieldDataList[i];

            string variableName = CreateGeneratedVariableName(fieldData.FieldName, i);
            string labelName = GetGeneratedLabelName(fieldData);

            if (fieldData.FieldType == CSE_CreateTools_FieldType.List)
            {
                AppendGeneratedListFieldOnGui(f_builder, fieldData, variableName, labelName);
            }
            else
            {
                AppendGeneratedSingleFieldOnGui(f_builder, fieldData, variableName, labelName);
            }

            f_builder.AppendLine("        GUILayout.Space(6.0f);");
            f_builder.AppendLine();
        }
    }

    /// <summary>
    /// 単体FieldのOnGUI描画処理を追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    /// <param name="f_fieldData">FieldData</param>
    /// <param name="f_variableName">変数名</param>
    /// <param name="f_labelName">表示名</param>
    private void AppendGeneratedSingleFieldOnGui(
        StringBuilder f_builder,
        CSED_CreateTools_FieldData f_fieldData,
        string f_variableName,
        string f_labelName)
    {
        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.MinMaxField)
        {
            AppendGeneratedMinMaxFieldOnGui(f_builder, f_fieldData, f_variableName, f_labelName);
            return;
        }

        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.Slider)
        {
            AppendGeneratedSliderOnGui(f_builder, f_fieldData, f_variableName, f_labelName);
            return;
        }

        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.Toggle)
        {
            f_builder.AppendLine("        " + f_variableName + " = EditorGUILayout.Toggle(\"" + EscapeString(f_labelName) + "\", " + f_variableName + ");");
            return;
        }

        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.TextArea)
        {
            f_builder.AppendLine("        EditorGUILayout.LabelField(\"" + EscapeString(f_labelName) + "\");");
            f_builder.AppendLine("        " + f_variableName + " = EditorGUILayout.TextArea(" + f_variableName + ", GUILayout.Height(64.0f));");
            return;
        }

        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.Select)
        {
            f_builder.AppendLine("        " + f_variableName + " = (" + GetGeneratedFieldTypeName(f_fieldData) + ")EditorGUILayout.ObjectField(\"" + EscapeString(f_labelName) + "\", " + f_variableName + ", typeof(" + GetGeneratedFieldTypeName(f_fieldData) + "), false);");
            return;
        }

        AppendGeneratedInputFieldOnGui(f_builder, f_fieldData, f_variableName, f_labelName);
    }

    /// <summary>
    /// MinMaxFieldのOnGUI描画処理を追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    /// <param name="f_fieldData">FieldData</param>
    /// <param name="f_variableName">変数名</param>
    /// <param name="f_labelName">表示名</param>
    private void AppendGeneratedMinMaxFieldOnGui(
        StringBuilder f_builder,
        CSED_CreateTools_FieldData f_fieldData,
        string f_variableName,
        string f_labelName)
    {
        string minVariableName = CreateGeneratedMinVariableName(f_variableName);
        string maxVariableName = CreateGeneratedMaxVariableName(f_variableName);

        f_builder.AppendLine("        {");
        f_builder.AppendLine("            Rect minMaxRowRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);");
        f_builder.AppendLine();
        f_builder.AppendLine("            float minMaxMainLabelWidth = 120.0f;");
        f_builder.AppendLine("            float minMaxSmallLabelWidth = 28.0f;");
        f_builder.AppendLine("            float minMaxSpacing = 6.0f;");
        f_builder.AppendLine();
        f_builder.AppendLine("            float minMaxValueAreaX = minMaxRowRect.x + minMaxMainLabelWidth + minMaxSpacing;");
        f_builder.AppendLine("            float minMaxValueAreaWidth = minMaxRowRect.width - minMaxMainLabelWidth - minMaxSpacing;");
        f_builder.AppendLine("            float minMaxFieldWidth = (minMaxValueAreaWidth - minMaxSmallLabelWidth - minMaxSmallLabelWidth - (minMaxSpacing * 3.0f)) * 0.5f;");
        f_builder.AppendLine("            minMaxFieldWidth = Mathf.Max(35.0f, minMaxFieldWidth);");
        f_builder.AppendLine();
        f_builder.AppendLine("            Rect minMaxLabelRect = new Rect(minMaxRowRect.x, minMaxRowRect.y, minMaxMainLabelWidth, minMaxRowRect.height);");
        f_builder.AppendLine("            Rect minLabelRect = new Rect(minMaxValueAreaX, minMaxRowRect.y, minMaxSmallLabelWidth, minMaxRowRect.height);");
        f_builder.AppendLine("            Rect minValueRect = new Rect(minLabelRect.xMax + minMaxSpacing, minMaxRowRect.y, minMaxFieldWidth, minMaxRowRect.height);");
        f_builder.AppendLine("            Rect maxLabelRect = new Rect(minValueRect.xMax + minMaxSpacing, minMaxRowRect.y, minMaxSmallLabelWidth, minMaxRowRect.height);");
        f_builder.AppendLine("            Rect maxValueRect = new Rect(maxLabelRect.xMax + minMaxSpacing, minMaxRowRect.y, minMaxFieldWidth, minMaxRowRect.height);");
        f_builder.AppendLine();
        f_builder.AppendLine("            EditorGUI.LabelField(minMaxLabelRect, \"" + EscapeString(f_labelName) + "\");");
        f_builder.AppendLine("            EditorGUI.LabelField(minLabelRect, \"Min\");");

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Int)
        {
            f_builder.AppendLine("            " + minVariableName + " = EditorGUI.IntField(minValueRect, " + minVariableName + ");");
            f_builder.AppendLine("            EditorGUI.LabelField(maxLabelRect, \"Max\");");
            f_builder.AppendLine("            " + maxVariableName + " = EditorGUI.IntField(maxValueRect, " + maxVariableName + ");");
        }
        else
        {
            f_builder.AppendLine("            " + minVariableName + " = EditorGUI.FloatField(minValueRect, " + minVariableName + ");");
            f_builder.AppendLine("            EditorGUI.LabelField(maxLabelRect, \"Max\");");
            f_builder.AppendLine("            " + maxVariableName + " = EditorGUI.FloatField(maxValueRect, " + maxVariableName + ");");
        }

        f_builder.AppendLine("        }");
    }

    /// <summary>
    /// InputFieldのOnGUI描画処理を追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    /// <param name="f_fieldData">FieldData</param>
    /// <param name="f_variableName">変数名</param>
    /// <param name="f_labelName">表示名</param>
    private void AppendGeneratedInputFieldOnGui(
        StringBuilder f_builder,
        CSED_CreateTools_FieldData f_fieldData,
        string f_variableName,
        string f_labelName)
    {
        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Int)
        {
            f_builder.AppendLine("        " + f_variableName + " = EditorGUILayout.IntField(\"" + EscapeString(f_labelName) + "\", " + f_variableName + ");");
            return;
        }

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Vector2Int)
        {
            f_builder.AppendLine("        " + f_variableName + " = EditorGUILayout.Vector2IntField(\"" + EscapeString(f_labelName) + "\", " + f_variableName + ");");
            return;
        }

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Vector3Int)
        {
            f_builder.AppendLine("        " + f_variableName + " = EditorGUILayout.Vector3IntField(\"" + EscapeString(f_labelName) + "\", " + f_variableName + ");");
            return;
        }

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Float)
        {
            f_builder.AppendLine("        " + f_variableName + " = EditorGUILayout.FloatField(\"" + EscapeString(f_labelName) + "\", " + f_variableName + ");");
            return;
        }

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Vector2)
        {
            f_builder.AppendLine("        " + f_variableName + " = EditorGUILayout.Vector2Field(\"" + EscapeString(f_labelName) + "\", " + f_variableName + ");");
            return;
        }

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Vector3)
        {
            f_builder.AppendLine("        " + f_variableName + " = EditorGUILayout.Vector3Field(\"" + EscapeString(f_labelName) + "\", " + f_variableName + ");");
            return;
        }

        f_builder.AppendLine("        " + f_variableName + " = EditorGUILayout.TextField(\"" + EscapeString(f_labelName) + "\", " + f_variableName + ");");
    }

    /// <summary>
    /// SliderのOnGUI描画処理を追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    /// <param name="f_fieldData">FieldData</param>
    /// <param name="f_variableName">変数名</param>
    /// <param name="f_labelName">表示名</param>
    private void AppendGeneratedSliderOnGui(
        StringBuilder f_builder,
        CSED_CreateTools_FieldData f_fieldData,
        string f_variableName,
        string f_labelName)
    {
        float minValue = GetGeneratedFloatValue(f_fieldData.SliderMinValueText, 0.0f);
        float maxValue = GetGeneratedFloatValue(f_fieldData.SliderMaxValueText, 100.0f);

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Int)
        {
            f_builder.AppendLine("        " + f_variableName + " = EditorGUILayout.IntSlider(\"" + EscapeString(f_labelName) + "\", " + f_variableName + ", " + Mathf.RoundToInt(minValue).ToString() + ", " + Mathf.RoundToInt(maxValue).ToString() + ");");
            return;
        }

        f_builder.AppendLine("        " + f_variableName + " = EditorGUILayout.Slider(\"" + EscapeString(f_labelName) + "\", " + f_variableName + ", " + minValue.ToString("0.0") + "f, " + maxValue.ToString("0.0") + "f);");
    }

    /// <summary>
    /// ListのOnGUI描画処理を追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    /// <param name="f_fieldData">FieldData</param>
    /// <param name="f_variableName">変数名</param>
    /// <param name="f_labelName">表示名</param>
    private void AppendGeneratedListFieldOnGui(
        StringBuilder f_builder,
        CSED_CreateTools_FieldData f_fieldData,
        string f_variableName,
        string f_labelName)
    {
        f_builder.AppendLine("        EditorGUILayout.LabelField(\"" + EscapeString(f_labelName) + "\", EditorStyles.boldLabel);");
        f_builder.AppendLine();
        f_builder.AppendLine("        EditorGUILayout.BeginHorizontal();");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            if (GUILayout.Button(\"-\", GUILayout.Width(24.0f)) && " + f_variableName + ".Count > 0)");
        f_builder.AppendLine("            {");
        f_builder.AppendLine("                " + f_variableName + ".RemoveAt(" + f_variableName + ".Count - 1);");
        f_builder.AppendLine("            }");
        f_builder.AppendLine();
        f_builder.AppendLine("            EditorGUILayout.LabelField(" + f_variableName + ".Count.ToString(), GUILayout.Width(32.0f));");
        f_builder.AppendLine();
        f_builder.AppendLine("            if (GUILayout.Button(\"+\", GUILayout.Width(24.0f)))");
        f_builder.AppendLine("            {");
        f_builder.AppendLine("                " + f_variableName + ".Add(" + GetGeneratedListElementDefaultValueText(f_fieldData) + ");");
        f_builder.AppendLine("            }");
        f_builder.AppendLine("        }");
        f_builder.AppendLine("        EditorGUILayout.EndHorizontal();");
        f_builder.AppendLine();
        f_builder.AppendLine("        for (int i = 0; i < " + f_variableName + ".Count; i++)");
        f_builder.AppendLine("        {");

        AppendGeneratedListElementOnGui(f_builder, f_fieldData, f_variableName);

        f_builder.AppendLine("        }");
    }

    /// <summary>
    /// List要素のOnGUI描画処理を追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    /// <param name="f_fieldData">FieldData</param>
    /// <param name="f_variableName">変数名</param>
    private void AppendGeneratedListElementOnGui(
        StringBuilder f_builder,
        CSED_CreateTools_FieldData f_fieldData,
        string f_variableName)
    {
        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.Toggle)
        {
            f_builder.AppendLine("            " + f_variableName + "[i] = EditorGUILayout.Toggle(\"Element \" + i.ToString(), " + f_variableName + "[i]);");
            return;
        }

        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.TextArea)
        {
            f_builder.AppendLine("            EditorGUILayout.LabelField(\"Element \" + i.ToString());");
            f_builder.AppendLine("            " + f_variableName + "[i] = EditorGUILayout.TextArea(" + f_variableName + "[i], GUILayout.Height(48.0f));");
            return;
        }

        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.Select)
        {
            f_builder.AppendLine("            " + f_variableName + "[i] = (" + GetGeneratedListElementTypeName(f_fieldData) + ")EditorGUILayout.ObjectField(\"Element \" + i.ToString(), " + f_variableName + "[i], typeof(" + GetGeneratedListElementTypeName(f_fieldData) + "), false);");
            return;
        }

        if (f_fieldData.ListElementFieldType == CSE_CreateTools_FieldType.Vector2Int)
        {
            f_builder.AppendLine("            " + f_variableName + "[i] = EditorGUILayout.Vector2IntField(\"Element \" + i.ToString(), " + f_variableName + "[i]);");
            return;
        }

        if (f_fieldData.ListElementFieldType == CSE_CreateTools_FieldType.Vector3Int)
        {
            f_builder.AppendLine("            " + f_variableName + "[i] = EditorGUILayout.Vector3IntField(\"Element \" + i.ToString(), " + f_variableName + "[i]);");
            return;
        }

        if (f_fieldData.ListElementFieldType == CSE_CreateTools_FieldType.Vector2)
        {
            f_builder.AppendLine("            " + f_variableName + "[i] = EditorGUILayout.Vector2Field(\"Element \" + i.ToString(), " + f_variableName + "[i]);");
            return;
        }

        if (f_fieldData.ListElementFieldType == CSE_CreateTools_FieldType.Vector3)
        {
            f_builder.AppendLine("            " + f_variableName + "[i] = EditorGUILayout.Vector3Field(\"Element \" + i.ToString(), " + f_variableName + "[i]);");
            return;
        }

        if (f_fieldData.ListElementFieldType == CSE_CreateTools_FieldType.Int)
        {
            f_builder.AppendLine("            " + f_variableName + "[i] = EditorGUILayout.IntField(\"Element \" + i.ToString(), " + f_variableName + "[i]);");
            return;
        }

        if (f_fieldData.ListElementFieldType == CSE_CreateTools_FieldType.Float)
        {
            f_builder.AppendLine("            " + f_variableName + "[i] = EditorGUILayout.FloatField(\"Element \" + i.ToString(), " + f_variableName + "[i]);");
            return;
        }

        f_builder.AppendLine("            " + f_variableName + "[i] = EditorGUILayout.TextField(\"Element \" + i.ToString(), " + f_variableName + "[i]);");
    }

    /// <summary>
    /// 生成用のField型名を取得します。
    /// </summary>
    /// <param name="f_fieldData">FieldData</param>
    /// <returns>型名</returns>
    private string GetGeneratedFieldTypeName(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.List)
        {
            return "List<" + GetGeneratedListElementTypeName(f_fieldData) + ">";
        }

        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.TextArea)
        {
            return "string";
        }

        switch (f_fieldData.FieldType)
        {
            case CSE_CreateTools_FieldType.Int:
                return "int";

            case CSE_CreateTools_FieldType.Vector2Int:
                return "Vector2Int";

            case CSE_CreateTools_FieldType.Vector3Int:
                return "Vector3Int";

            case CSE_CreateTools_FieldType.Float:
                return "float";

            case CSE_CreateTools_FieldType.Vector2:
                return "Vector2";

            case CSE_CreateTools_FieldType.Vector3:
                return "Vector3";

            case CSE_CreateTools_FieldType.String:
                return "string";

            case CSE_CreateTools_FieldType.Bool:
                return "bool";

            case CSE_CreateTools_FieldType.ScriptableObject:
                return GetGeneratedScriptableObjectTypeName(f_fieldData);

            case CSE_CreateTools_FieldType.Script:
                return "MonoScript";

            case CSE_CreateTools_FieldType.GameObject:
                return "GameObject";

            default:
                return "string";
        }
    }

    /// <summary>
    /// 生成用のList要素型名を取得します。
    /// </summary>
    /// <param name="f_fieldData">FieldData</param>
    /// <returns>型名</returns>
    private string GetGeneratedListElementTypeName(CSED_CreateTools_FieldData f_fieldData)
    {
        switch (f_fieldData.ListElementFieldType)
        {
            case CSE_CreateTools_FieldType.Int:
                return "int";

            case CSE_CreateTools_FieldType.Vector2Int:
                return "Vector2Int";

            case CSE_CreateTools_FieldType.Vector3Int:
                return "Vector3Int";

            case CSE_CreateTools_FieldType.Float:
                return "float";

            case CSE_CreateTools_FieldType.Vector2:
                return "Vector2";

            case CSE_CreateTools_FieldType.Vector3:
                return "Vector3";

            case CSE_CreateTools_FieldType.String:
                return "string";

            case CSE_CreateTools_FieldType.Bool:
                return "bool";

            case CSE_CreateTools_FieldType.ScriptableObject:
                return GetGeneratedScriptableObjectTypeName(f_fieldData);

            case CSE_CreateTools_FieldType.Script:
                return "MonoScript";

            case CSE_CreateTools_FieldType.GameObject:
                return "GameObject";

            default:
                return "string";
        }
    }

    /// <summary>
    /// 生成用のScriptableObject型名を取得します。
    /// </summary>
    /// <param name="f_fieldData">FieldData</param>
    /// <returns>型名</returns>
    private string GetGeneratedScriptableObjectTypeName(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.ScriptableObjectTypeScript == null)
        {
            return "ScriptableObject";
        }

        System.Type scriptType = f_fieldData.ScriptableObjectTypeScript.GetClass();

        if (scriptType == null)
        {
            return "ScriptableObject";
        }

        return scriptType.Name;
    }

    /// <summary>
    /// 生成用の初期値文字列を取得します。
    /// </summary>
    /// <param name="f_fieldData">FieldData</param>
    /// <returns>初期値文字列</returns>
    private string GetGeneratedDefaultValueText(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.List)
        {
            return GetGeneratedListDefaultValueText(f_fieldData);
        }

        if (f_fieldData.IsDefaultValueNull)
        {
            if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Vector2Int)
            {
                return "Vector2Int.zero";
            }

            if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Vector3Int)
            {
                return "Vector3Int.zero";
            }

            if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Vector2)
            {
                return "Vector2.zero";
            }

            if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Vector3)
            {
                return "Vector3.zero";
            }

            if (f_fieldData.FieldType == CSE_CreateTools_FieldType.String)
            {
                return "string.Empty";
            }

            if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Int)
            {
                return "0";
            }

            if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Float)
            {
                return "0.0f";
            }

            if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Bool)
            {
                return "false";
            }

            return "null";
        }

        switch (f_fieldData.FieldType)
        {
            case CSE_CreateTools_FieldType.Int:
                return GetGeneratedIntText(f_fieldData.DefaultValueText);

            case CSE_CreateTools_FieldType.Float:
                return GetGeneratedFloatText(f_fieldData.DefaultValueText);

            case CSE_CreateTools_FieldType.String:
                return "\"" + EscapeString(f_fieldData.DefaultValueText) + "\"";

            case CSE_CreateTools_FieldType.Bool:
                return GetGeneratedBoolText(f_fieldData.DefaultValueText);

            default:
                return "null";
        }
    }

    /// <summary>
    /// Listの初期値文字列を取得します。
    /// </summary>
    /// <param name="f_fieldData">FieldData</param>
    /// <returns>初期値文字列</returns>
    private string GetGeneratedListDefaultValueText(CSED_CreateTools_FieldData f_fieldData)
    {
        string listTypeName = GetGeneratedFieldTypeName(f_fieldData);

        StringBuilder builder = new StringBuilder();

        builder.Append("new ");
        builder.Append(listTypeName);
        builder.Append("()");
        builder.AppendLine();
        builder.Append("    {");

        int elementCount = 0;

        if (f_fieldData.ListDefaultElementValueTextList != null)
        {
            elementCount = f_fieldData.ListDefaultElementValueTextList.Count;
        }

        for (int i = 0 ; i < elementCount ; i++)
        {
            builder.AppendLine();
            builder.Append("        ");
            builder.Append(GetGeneratedListElementValueText(f_fieldData, i));

            if (i < elementCount - 1)
            {
                builder.Append(",");
            }
        }

        if (elementCount > 0)
        {
            builder.AppendLine();
            builder.Append("    ");
        }

        builder.Append("}");

        return builder.ToString();
    }

    /// <summary>
    /// List要素の初期値文字列を取得します。
    /// </summary>
    /// <param name="f_fieldData">FieldData</param>
    /// <param name="f_index">要素番号</param>
    /// <returns>初期値文字列</returns>
    private string GetGeneratedListElementValueText(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        if (f_fieldData.IsListDefaultValueNull)
        {
            return GetGeneratedListElementDefaultValueText(f_fieldData);
        }

        switch (f_fieldData.ListElementFieldType)
        {
            case CSE_CreateTools_FieldType.Int:
                return GetGeneratedIntText(f_fieldData.ListDefaultElementValueTextList[f_index]);

            case CSE_CreateTools_FieldType.Vector2Int:
                return "new Vector2Int("
                    + GetGeneratedIntText(GetListVectorDefaultComponentText(f_fieldData, f_index, 0))
                    + ", "
                    + GetGeneratedIntText(GetListVectorDefaultComponentText(f_fieldData, f_index, 1))
                    + ")";

            case CSE_CreateTools_FieldType.Vector3Int:
                return "new Vector3Int("
                    + GetGeneratedIntText(GetListVectorDefaultComponentText(f_fieldData, f_index, 0))
                    + ", "
                    + GetGeneratedIntText(GetListVectorDefaultComponentText(f_fieldData, f_index, 1))
                    + ", "
                    + GetGeneratedIntText(GetListVectorDefaultComponentText(f_fieldData, f_index, 2))
                    + ")";

            case CSE_CreateTools_FieldType.Float:
                return GetGeneratedFloatText(f_fieldData.ListDefaultElementValueTextList[f_index]);

            case CSE_CreateTools_FieldType.Vector2:
                return "new Vector2("
                    + GetGeneratedFloatText(GetListVectorDefaultComponentText(f_fieldData, f_index, 0))
                    + ", "
                    + GetGeneratedFloatText(GetListVectorDefaultComponentText(f_fieldData, f_index, 1))
                    + ")";

            case CSE_CreateTools_FieldType.Vector3:
                return "new Vector3("
                    + GetGeneratedFloatText(GetListVectorDefaultComponentText(f_fieldData, f_index, 0))
                    + ", "
                    + GetGeneratedFloatText(GetListVectorDefaultComponentText(f_fieldData, f_index, 1))
                    + ", "
                    + GetGeneratedFloatText(GetListVectorDefaultComponentText(f_fieldData, f_index, 2))
                    + ")";

            case CSE_CreateTools_FieldType.String:
                return "\"" + EscapeString(f_fieldData.ListDefaultElementValueTextList[f_index]) + "\"";

            case CSE_CreateTools_FieldType.Bool:
                return GetGeneratedBoolText(f_fieldData.ListDefaultElementValueTextList[f_index]);

            default:
                return "null";
        }
    }

    /// <summary>
    /// List要素追加時の初期値文字列を取得します。
    /// </summary>
    /// <param name="f_fieldData">FieldData</param>
    /// <returns>初期値文字列</returns>
    private string GetGeneratedListElementDefaultValueText(CSED_CreateTools_FieldData f_fieldData)
    {
        switch (f_fieldData.ListElementFieldType)
        {
            case CSE_CreateTools_FieldType.Int:
                return "0";

            case CSE_CreateTools_FieldType.Vector2Int:
                return "Vector2Int.zero";

            case CSE_CreateTools_FieldType.Vector3Int:
                return "Vector3Int.zero";

            case CSE_CreateTools_FieldType.Float:
                return "0.0f";

            case CSE_CreateTools_FieldType.Vector2:
                return "Vector2.zero";

            case CSE_CreateTools_FieldType.Vector3:
                return "Vector3.zero";

            case CSE_CreateTools_FieldType.String:
                return "string.Empty";

            case CSE_CreateTools_FieldType.Bool:
                return "false";

            default:
                return "null";
        }
    }

    /// <summary>
    /// 表示ラベル名を取得します。
    /// </summary>
    /// <param name="f_fieldData">FieldData</param>
    /// <returns>表示ラベル名</returns>
    private string GetGeneratedLabelName(CSED_CreateTools_FieldData f_fieldData)
    {
        if (string.IsNullOrEmpty(f_fieldData.TagName) == false)
        {
            return f_fieldData.TagName;
        }

        return f_fieldData.FieldName;
    }

    /// <summary>
    /// 生成用の変数名を作成します。
    /// </summary>
    /// <param name="f_fieldName">Field名</param>
    /// <param name="f_index">Field番号</param>
    /// <returns>生成用変数名</returns>
    private string CreateGeneratedVariableName(string f_fieldName, int f_index)
    {
        if (string.IsNullOrEmpty(f_fieldName))
        {
            return "generatedField" + f_index.ToString();
        }

        StringBuilder builder = new StringBuilder();

        for (int i = 0 ; i < f_fieldName.Length ; i++)
        {
            char currentChar = f_fieldName[i];

            if (char.IsLetterOrDigit(currentChar) || currentChar == '_')
            {
                builder.Append(currentChar);
            }
        }

        if (builder.Length <= 0)
        {
            return "generatedField" + f_index.ToString();
        }

        if (char.IsDigit(builder[0]))
        {
            builder.Insert(0, "_");
        }

        return builder.ToString();
    }

    /// <summary>
    /// int文字列を生成コード用に変換します。
    /// </summary>
    /// <param name="f_text">入力文字列</param>
    /// <returns>int文字列</returns>
    private string GetGeneratedIntText(string f_text)
    {
        int value = 0;
        int.TryParse(f_text, out value);
        return value.ToString();
    }

    /// <summary>
    /// float文字列を生成コード用に変換します。
    /// </summary>
    /// <param name="f_text">入力文字列</param>
    /// <returns>float文字列</returns>
    private string GetGeneratedFloatText(string f_text)
    {
        float value = 0.0f;
        float.TryParse(f_text, out value);
        return value.ToString("0.0") + "f";
    }

    /// <summary>
    /// bool文字列を生成コード用に変換します。
    /// </summary>
    /// <param name="f_text">入力文字列</param>
    /// <returns>bool文字列</returns>
    private string GetGeneratedBoolText(string f_text)
    {
        bool value = false;
        bool.TryParse(f_text, out value);
        return value ? "true" : "false";
    }

    /// <summary>
    /// float値を取得します。
    /// </summary>
    /// <param name="f_text">入力文字列</param>
    /// <param name="f_defaultValue">初期値</param>
    /// <returns>float値</returns>
    private float GetGeneratedFloatValue(string f_text, float f_defaultValue)
    {
        float value = f_defaultValue;
        float.TryParse(f_text, out value);
        return value;
    }

    /// <summary>
    /// 生成EditorWindowにScriptableObject作成ボタンを追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    private void AppendGeneratedCreateScriptableObjectButton(StringBuilder f_builder)
    {
        f_builder.AppendLine("        GUILayout.Space(12.0f);");
        f_builder.AppendLine("        EditorGUILayout.LabelField(\"Create Asset\", EditorStyles.boldLabel);");
        f_builder.AppendLine();

        f_builder.AppendLine("        if (GUILayout.Button(\"Create ScriptableObject\", GUILayout.Height(28.0f)))");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            CreateScriptableObjectAsset();");
        f_builder.AppendLine("        }");
    }

    /// <summary>
    /// 生成EditorWindowにCreate Asset設定用の別EditorWindowを追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    /// <param name="f_className">生成するEditorWindowクラス名</param>
    private void AppendGeneratedCreateAssetSettingsWindow(
        StringBuilder f_builder,
        string f_className)
    {
        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// Create Asset設定専用のEditorWindowです。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    private class CreateAssetSettingsWindow : EditorWindow");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        /// <summary>");
        f_builder.AppendLine("        /// 設定対象のEditorWindowです。");
        f_builder.AppendLine("        /// </summary>");
        f_builder.AppendLine("        private " + f_className + " m_OwnerWindow;");
        f_builder.AppendLine();

        f_builder.AppendLine("        /// <summary>");
        f_builder.AppendLine("        /// Create Asset設定Windowを開きます。");
        f_builder.AppendLine("        /// </summary>");
        f_builder.AppendLine("        /// <param name=\"f_ownerWindow\">設定対象のEditorWindow</param>");
        f_builder.AppendLine("        public static void Open(" + f_className + " f_ownerWindow)");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            CreateAssetSettingsWindow window = CreateInstance<CreateAssetSettingsWindow>();");
        f_builder.AppendLine("            window.titleContent = new GUIContent(\"Create Asset Settings\");");
        f_builder.AppendLine("            window.m_OwnerWindow = f_ownerWindow;");
        f_builder.AppendLine("            window.minSize = new Vector2(360.0f, 120.0f);");
        f_builder.AppendLine("            window.position = new Rect(");
        f_builder.AppendLine("                f_ownerWindow.position.x + 40.0f,");
        f_builder.AppendLine("                f_ownerWindow.position.y + 40.0f,");
        f_builder.AppendLine("                360.0f,");
        f_builder.AppendLine("                120.0f);");
        f_builder.AppendLine("            window.ShowUtility();");
        f_builder.AppendLine("        }");
        f_builder.AppendLine();

        f_builder.AppendLine("        /// <summary>");
        f_builder.AppendLine("        /// GUIを描画します。");
        f_builder.AppendLine("        /// </summary>");
        f_builder.AppendLine("        private void OnGUI()");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            if (m_OwnerWindow == null)");
        f_builder.AppendLine("            {");
        f_builder.AppendLine("                EditorGUILayout.HelpBox(\"設定対象のEditorWindowが見つかりません。\", MessageType.Warning);");
        f_builder.AppendLine("                return;");
        f_builder.AppendLine("            }");
        f_builder.AppendLine();

        f_builder.AppendLine("            EditorGUILayout.LabelField(\"Create Asset Settings\", EditorStyles.boldLabel);");
        f_builder.AppendLine("            GUILayout.Space(6.0f);");
        f_builder.AppendLine();

        f_builder.AppendLine("            m_OwnerWindow.m_AssetFileName = EditorGUILayout.TextField(\"Asset Name\", m_OwnerWindow.m_AssetFileName);");
        f_builder.AppendLine("            m_OwnerWindow.m_AssetOutputFolderPath = EditorGUILayout.TextField(\"Asset Folder\", m_OwnerWindow.m_AssetOutputFolderPath);");
        f_builder.AppendLine();

        f_builder.AppendLine("            if (GUI.changed)");
        f_builder.AppendLine("            {");
        f_builder.AppendLine("                m_OwnerWindow.Repaint();");
        f_builder.AppendLine("            }");
        f_builder.AppendLine("        }");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();
    }

    /// <summary>
    /// 生成EditorWindowにScriptableObject作成用メンバーを追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    /// <param name="f_dataClassName">生成するScriptableObjectクラス名</param>
    /// <param name="f_className">生成するEditorWindowクラス名</param>
    private void AppendGeneratedCreateScriptableObjectMembers(
        StringBuilder f_builder,
        string f_dataClassName,
        string f_className)
    {
        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// 作成するScriptableObjectアセット名です。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    private string m_AssetFileName = \"" + EscapeString(m_GeneratedDefaultAssetName) + "\";");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// ScriptableObjectアセットの保存先です。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    private string m_AssetOutputFolderPath = \"" + EscapeString(m_GeneratedDefaultAssetFolder) + "\";");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// ScriptableObjectアセットを作成します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    private void CreateScriptableObjectAsset()");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        if (System.IO.Directory.Exists(m_AssetOutputFolderPath) == false)");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            System.IO.Directory.CreateDirectory(m_AssetOutputFolderPath);");
        f_builder.AppendLine("        }");
        f_builder.AppendLine();

        f_builder.AppendLine("        " + f_dataClassName + " asset = CreateInstance<" + f_dataClassName + ">();");

        AppendGeneratedAssignScriptableObjectValues(f_builder, "asset");

        f_builder.AppendLine();
        f_builder.AppendLine("        string assetPath = AssetDatabase.GenerateUniqueAssetPath(");
        f_builder.AppendLine("            System.IO.Path.Combine(m_AssetOutputFolderPath, m_AssetFileName + \".asset\"));");
        f_builder.AppendLine();
        f_builder.AppendLine("        AssetDatabase.CreateAsset(asset, assetPath);");
        f_builder.AppendLine("        AssetDatabase.SaveAssets();");
        f_builder.AppendLine("        AssetDatabase.Refresh();");
        f_builder.AppendLine("        Selection.activeObject = asset;");
        f_builder.AppendLine("        " + f_className + "_CreatedAssetsWindow.RepaintOpenedWindows();");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();
    }

    /// <summary>
    /// 生成EditorWindowに作成済みScriptableObject一覧Windowを追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    /// <param name="f_className">生成するEditorWindowクラス名</param>
    /// <param name="f_dataClassName">生成するScriptableObjectクラス名</param>
    private void AppendGeneratedCreatedAssetsWindow(
        StringBuilder f_builder,
        string f_className,
        string f_dataClassName)
    {
        string assetWindowClassName = f_className + "_CreatedAssetsWindow";

        f_builder.AppendLine();
        f_builder.AppendLine("/// <summary>");
        f_builder.AppendLine("/// " + f_dataClassName + "で作成されたScriptableObject一覧を表示するEditorWindowです。");
        f_builder.AppendLine("/// </summary>");
        f_builder.AppendLine("public class " + assetWindowClassName + " : EditorWindow");
        f_builder.AppendLine("{");

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// Asset一覧のスクロール位置です。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    private Vector2 m_AssetListScrollPosition;");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// 表示用にキャッシュしたAssetパス一覧です。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    private List<string> m_CachedAssetPathList = new List<string>();");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// Assetごとの設定表示状態です。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    private Dictionary<string, bool> m_AssetFoldoutStateDictionary = new Dictionary<string, bool>();");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// Created Assetsウィンドウを開きます。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    public static void OpenWindow()");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        " + assetWindowClassName + " window = GetWindow<" + assetWindowClassName + ">(");
        f_builder.AppendLine("            \"Created Assets\",");
        f_builder.AppendLine("            false,");
        f_builder.AppendLine("            typeof(" + f_className + "));");
        f_builder.AppendLine();
        f_builder.AppendLine("        window.minSize = new Vector2(420.0f, 300.0f);");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// 開いているCreated Assetsウィンドウを再描画します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    public static void RepaintOpenedWindows()");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        " + assetWindowClassName + "[] windows = Resources.FindObjectsOfTypeAll<" + assetWindowClassName + ">();");
        f_builder.AppendLine();
        f_builder.AppendLine("        for (int i = 0; i < windows.Length; i++)");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            windows[i].RefreshAssetPathList();");
        f_builder.AppendLine("            windows[i].Repaint();");
        f_builder.AppendLine("        }");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// Window有効化時にAsset一覧を更新します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    private void OnEnable()");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        RefreshAssetPathList();");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// Asset一覧を更新します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    private void RefreshAssetPathList()");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        m_CachedAssetPathList.Clear();");
        f_builder.AppendLine();
        f_builder.AppendLine("        string[] assetGuids = AssetDatabase.FindAssets(\"t:\" + nameof(" + f_dataClassName + "));");
        f_builder.AppendLine();
        f_builder.AppendLine("        for (int i = 0; i < assetGuids.Length; i++)");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[i]);");
        f_builder.AppendLine("            m_CachedAssetPathList.Add(assetPath);");
        f_builder.AppendLine("        }");
        f_builder.AppendLine();
        f_builder.AppendLine("        m_CachedAssetPathList.Sort(CompareAssetPathByNaturalName);");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// GUIを描画します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    private void OnGUI()");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        DrawCreatedAssetList();");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// 作成済みAsset一覧を描画します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    private void DrawCreatedAssetList()");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        EditorGUILayout.BeginHorizontal();");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            EditorGUILayout.LabelField(\"Created ScriptableObjects\", EditorStyles.boldLabel);");
        f_builder.AppendLine();
        f_builder.AppendLine("            GUILayout.FlexibleSpace();");
        f_builder.AppendLine();
        f_builder.AppendLine("            if (GUILayout.Button(\"Refresh\", GUILayout.Width(80.0f), GUILayout.Height(20.0f)))");
        f_builder.AppendLine("            {");
        f_builder.AppendLine("                RefreshAssetPathList();");
        f_builder.AppendLine("            }");
        f_builder.AppendLine("        }");
        f_builder.AppendLine("        EditorGUILayout.EndHorizontal();");
        f_builder.AppendLine();
        f_builder.AppendLine("        if (m_CachedAssetPathList.Count <= 0)");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            EditorGUILayout.HelpBox(\"まだ作成済みAssetがありません。\", MessageType.Info);");
        f_builder.AppendLine("            return;");
        f_builder.AppendLine("        }");
        f_builder.AppendLine();
        f_builder.AppendLine("        m_AssetListScrollPosition = EditorGUILayout.BeginScrollView(m_AssetListScrollPosition);");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            for (int i = 0; i < m_CachedAssetPathList.Count; i++)");
        f_builder.AppendLine("            {");
        f_builder.AppendLine("                string assetPath = m_CachedAssetPathList[i];");
        f_builder.AppendLine("                " + f_dataClassName + " asset = AssetDatabase.LoadAssetAtPath<" + f_dataClassName + ">(assetPath);");
        f_builder.AppendLine();
        f_builder.AppendLine("                if (asset == null)");
        f_builder.AppendLine("                {");
        f_builder.AppendLine("                    continue;");
        f_builder.AppendLine("                }");
        f_builder.AppendLine();
        f_builder.AppendLine("                DrawCreatedAssetListItem(asset, assetPath);");
        f_builder.AppendLine("                GUILayout.Space(10.0f);");
        f_builder.AppendLine("            }");
        f_builder.AppendLine("        }");
        f_builder.AppendLine("        EditorGUILayout.EndScrollView();");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// AssetパスをAsset名の自然順で比較します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    /// <param name=\"f_leftPath\">左側Assetパス</param>");
        f_builder.AppendLine("    /// <param name=\"f_rightPath\">右側Assetパス</param>");
        f_builder.AppendLine("    /// <returns>比較結果</returns>");
        f_builder.AppendLine("    private int CompareAssetPathByNaturalName(string f_leftPath, string f_rightPath)");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        string leftName = System.IO.Path.GetFileNameWithoutExtension(f_leftPath);");
        f_builder.AppendLine("        string rightName = System.IO.Path.GetFileNameWithoutExtension(f_rightPath);");
        f_builder.AppendLine();
        f_builder.AppendLine("        int nameCompare = CompareNaturalText(leftName, rightName);");
        f_builder.AppendLine();
        f_builder.AppendLine("        if (nameCompare != 0)");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            return nameCompare;");
        f_builder.AppendLine("        }");
        f_builder.AppendLine();
        f_builder.AppendLine("        return string.Compare(f_leftPath, f_rightPath, System.StringComparison.OrdinalIgnoreCase);");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// 文字列を自然順で比較します。");
        f_builder.AppendLine("    /// a1, a2, a10 のように数字部分を数値として比較します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    /// <param name=\"f_leftText\">左側文字列</param>");
        f_builder.AppendLine("    /// <param name=\"f_rightText\">右側文字列</param>");
        f_builder.AppendLine("    /// <returns>比較結果</returns>");
        f_builder.AppendLine("    private int CompareNaturalText(string f_leftText, string f_rightText)");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        int leftIndex = 0;");
        f_builder.AppendLine("        int rightIndex = 0;");
        f_builder.AppendLine();
        f_builder.AppendLine("        while (leftIndex < f_leftText.Length && rightIndex < f_rightText.Length)");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            char leftChar = f_leftText[leftIndex];");
        f_builder.AppendLine("            char rightChar = f_rightText[rightIndex];");
        f_builder.AppendLine();
        f_builder.AppendLine("            if (char.IsDigit(leftChar) && char.IsDigit(rightChar))");
        f_builder.AppendLine("            {");
        f_builder.AppendLine("                int numberCompare = CompareNaturalNumberPart(f_leftText, ref leftIndex, f_rightText, ref rightIndex);");
        f_builder.AppendLine();
        f_builder.AppendLine("                if (numberCompare != 0)");
        f_builder.AppendLine("                {");
        f_builder.AppendLine("                    return numberCompare;");
        f_builder.AppendLine("                }");
        f_builder.AppendLine();
        f_builder.AppendLine("                continue;");
        f_builder.AppendLine("            }");
        f_builder.AppendLine();
        f_builder.AppendLine("            int charCompare = string.Compare(");
        f_builder.AppendLine("                leftChar.ToString(),");
        f_builder.AppendLine("                rightChar.ToString(),");
        f_builder.AppendLine("                true,");
        f_builder.AppendLine("                System.Globalization.CultureInfo.GetCultureInfo(\"ja-JP\"));");
        f_builder.AppendLine();
        f_builder.AppendLine("            if (charCompare != 0)");
        f_builder.AppendLine("            {");
        f_builder.AppendLine("                return charCompare;");
        f_builder.AppendLine("            }");
        f_builder.AppendLine();
        f_builder.AppendLine("            leftIndex++;");
        f_builder.AppendLine("            rightIndex++;");
        f_builder.AppendLine("        }");
        f_builder.AppendLine();
        f_builder.AppendLine("        return f_leftText.Length.CompareTo(f_rightText.Length);");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// 文字列内の数字部分を数値として比較します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    /// <param name=\"f_leftText\">左側文字列</param>");
        f_builder.AppendLine("    /// <param name=\"f_leftIndex\">左側現在位置</param>");
        f_builder.AppendLine("    /// <param name=\"f_rightText\">右側文字列</param>");
        f_builder.AppendLine("    /// <param name=\"f_rightIndex\">右側現在位置</param>");
        f_builder.AppendLine("    /// <returns>比較結果</returns>");
        f_builder.AppendLine("    private int CompareNaturalNumberPart(");
        f_builder.AppendLine("        string f_leftText,");
        f_builder.AppendLine("        ref int f_leftIndex,");
        f_builder.AppendLine("        string f_rightText,");
        f_builder.AppendLine("        ref int f_rightIndex)");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        long leftNumber = ReadNaturalNumber(f_leftText, ref f_leftIndex);");
        f_builder.AppendLine("        long rightNumber = ReadNaturalNumber(f_rightText, ref f_rightIndex);");
        f_builder.AppendLine();
        f_builder.AppendLine("        return leftNumber.CompareTo(rightNumber);");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// 文字列内の数字部分を読み取ります。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    /// <param name=\"f_text\">対象文字列</param>");
        f_builder.AppendLine("    /// <param name=\"f_index\">現在位置</param>");
        f_builder.AppendLine("    /// <returns>読み取った数値</returns>");
        f_builder.AppendLine("    private long ReadNaturalNumber(string f_text, ref int f_index)");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        long number = 0;");
        f_builder.AppendLine();
        f_builder.AppendLine("        while (f_index < f_text.Length && char.IsDigit(f_text[f_index]))");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            number = number * 10 + (f_text[f_index] - '0');");
        f_builder.AppendLine("            f_index++;");
        f_builder.AppendLine("        }");
        f_builder.AppendLine();
        f_builder.AppendLine("        return number;");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// Asset一覧の1項目を描画します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    /// <param name=\"f_asset\">対象Asset</param>");
        f_builder.AppendLine("    /// <param name=\"f_assetPath\">対象Assetパス</param>");
        f_builder.AppendLine("    private void DrawCreatedAssetListItem(" + f_dataClassName + " f_asset, string f_assetPath)");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        EditorGUILayout.BeginVertical(EditorStyles.helpBox);");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            EditorGUILayout.LabelField(f_asset.name, EditorStyles.boldLabel);");
        f_builder.AppendLine();
        f_builder.AppendLine("            EditorGUILayout.BeginHorizontal();");
        f_builder.AppendLine("            {");
        f_builder.AppendLine("                EditorGUILayout.LabelField(\"Path\", GUILayout.Width(36.0f));");
        f_builder.AppendLine("                EditorGUILayout.TextField(f_assetPath);");
        f_builder.AppendLine();
        f_builder.AppendLine("                string foldoutButtonText = IsAssetFoldoutOpened(f_assetPath) ? \"▼\" : \"▶\";");
        f_builder.AppendLine();
        f_builder.AppendLine("                if (GUILayout.Button(foldoutButtonText, GUILayout.Width(24.0f), GUILayout.Height(20.0f)))");
        f_builder.AppendLine("                {");
        f_builder.AppendLine("                    ToggleAssetFoldout(f_assetPath);");
        f_builder.AppendLine("                }");
        f_builder.AppendLine();
        f_builder.AppendLine("                if (GUILayout.Button(\"Select\", GUILayout.Width(80.0f), GUILayout.Height(20.0f)))");
        f_builder.AppendLine("                {");
        f_builder.AppendLine("                    SelectAsset(f_asset);");
        f_builder.AppendLine("                }");
        f_builder.AppendLine("            }");
        f_builder.AppendLine("            EditorGUILayout.EndHorizontal();");
        f_builder.AppendLine();
        f_builder.AppendLine("            if (IsAssetFoldoutOpened(f_assetPath))");
        f_builder.AppendLine("            {");
        f_builder.AppendLine("                GUILayout.Space(4.0f);");
        f_builder.AppendLine("                DrawCreatedAssetSettings(f_asset, f_assetPath);");
        f_builder.AppendLine("            }");
        f_builder.AppendLine("        }");
        f_builder.AppendLine("        EditorGUILayout.EndVertical();");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// Asset設定表示状態を切り替えます。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    /// <param name=\"f_assetPath\">対象Assetパス</param>");
        f_builder.AppendLine("    private void ToggleAssetFoldout(string f_assetPath)");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        if (m_AssetFoldoutStateDictionary.ContainsKey(f_assetPath) == false)");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            m_AssetFoldoutStateDictionary.Add(f_assetPath, true);");
        f_builder.AppendLine("            return;");
        f_builder.AppendLine("        }");
        f_builder.AppendLine();
        f_builder.AppendLine("        m_AssetFoldoutStateDictionary[f_assetPath] = !m_AssetFoldoutStateDictionary[f_assetPath];");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// Asset設定表示状態を取得します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    /// <param name=\"f_assetPath\">対象Assetパス</param>");
        f_builder.AppendLine("    /// <returns>表示中ならtrue</returns>");
        f_builder.AppendLine("    private bool IsAssetFoldoutOpened(string f_assetPath)");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        if (m_AssetFoldoutStateDictionary.ContainsKey(f_assetPath) == false)");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            return false;");
        f_builder.AppendLine("        }");
        f_builder.AppendLine();
        f_builder.AppendLine("        return m_AssetFoldoutStateDictionary[f_assetPath];");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// 作成済みAssetの設定項目を描画します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    /// <param name=\"f_asset\">対象Asset</param>");
        f_builder.AppendLine("    /// <param name=\"f_assetPath\">対象Assetパス</param>");
        f_builder.AppendLine("    private void DrawCreatedAssetSettings(" + f_dataClassName + " f_asset, string f_assetPath)");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        GUILayout.Space(4.0f);");
        f_builder.AppendLine();
        f_builder.AppendLine("        EditorGUI.BeginChangeCheck();");
        f_builder.AppendLine("        string newAssetName = EditorGUILayout.DelayedTextField(\"Asset Name\", f_asset.name);");
        f_builder.AppendLine();
        f_builder.AppendLine("        if (EditorGUI.EndChangeCheck())");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            if (string.IsNullOrEmpty(newAssetName) == false && newAssetName != f_asset.name)");
        f_builder.AppendLine("            {");
        f_builder.AppendLine("                string renameError = AssetDatabase.RenameAsset(f_assetPath, newAssetName);");
        f_builder.AppendLine();
        f_builder.AppendLine("                if (string.IsNullOrEmpty(renameError))");
        f_builder.AppendLine("                {");
        f_builder.AppendLine("                    AssetDatabase.SaveAssets();");
        f_builder.AppendLine("                    AssetDatabase.Refresh();");
        f_builder.AppendLine("                    RefreshAssetPathList();");
        f_builder.AppendLine("                    Repaint();");
        f_builder.AppendLine("                    GUIUtility.ExitGUI();");
        f_builder.AppendLine("                }");
        f_builder.AppendLine("                else");
        f_builder.AppendLine("                {");
        f_builder.AppendLine("                    EditorUtility.DisplayDialog(\"Rename Asset Error\", renameError, \"OK\");");
        f_builder.AppendLine("                }");
        f_builder.AppendLine("            }");
        f_builder.AppendLine("        }");
        f_builder.AppendLine();
        f_builder.AppendLine("        SerializedObject serializedObject = new SerializedObject(f_asset);");
        f_builder.AppendLine("        serializedObject.Update();");
        f_builder.AppendLine();
        f_builder.AppendLine("        DrawCreatedAssetSerializedFields(serializedObject);");
        f_builder.AppendLine();
        f_builder.AppendLine("        if (serializedObject.ApplyModifiedProperties())");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            EditorUtility.SetDirty(f_asset);");
        f_builder.AppendLine("            AssetDatabase.SaveAssets();");
        f_builder.AppendLine("        }");
        f_builder.AppendLine();
        f_builder.AppendLine("        GUILayout.Space(10.0f);");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        AppendGeneratedCreatedAssetSerializedFieldMethods(f_builder);

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// AssetをProject上で選択します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    /// <param name=\"f_asset\">選択するAsset</param>");
        f_builder.AppendLine("    private void SelectAsset(" + f_dataClassName + " f_asset)");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        Selection.activeObject = f_asset;");
        f_builder.AppendLine("        EditorGUIUtility.PingObject(f_asset);");
        f_builder.AppendLine("    }");
        f_builder.AppendLine("}");
        f_builder.AppendLine();
    }

    /// <summary>
    /// Created Assets側で、生成時のラベル名を使ってScriptableObjectの項目を描画する処理を追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    private void AppendGeneratedCreatedAssetSerializedFieldMethods(StringBuilder f_builder)
    {
        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// 作成済みAssetの各項目を、生成時の表示ラベルで描画します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    /// <param name=\"f_serializedObject\">対象SerializedObject</param>");
        f_builder.AppendLine("    private void DrawCreatedAssetSerializedFields(SerializedObject f_serializedObject)");
        f_builder.AppendLine("    {");

        if (m_FieldDataList != null)
        {
            for (int i = 0 ; i < m_FieldDataList.Count ; i++)
            {
                CSED_CreateTools_FieldData fieldData = m_FieldDataList[i];

                string variableName = CreateGeneratedVariableName(fieldData.FieldName, i);
                string labelName = EscapeString(GetGeneratedLabelName(fieldData));

                if (IsGeneratedSingleMinMaxField(fieldData))
                {
                    string minVariableName = CreateGeneratedMinVariableName(variableName);
                    string maxVariableName = CreateGeneratedMaxVariableName(variableName);

                    f_builder.AppendLine("        SerializedProperty " + minVariableName + "Property = f_serializedObject.FindProperty(\"" + minVariableName + "\");");
                    f_builder.AppendLine("        SerializedProperty " + maxVariableName + "Property = f_serializedObject.FindProperty(\"" + maxVariableName + "\");");
                    f_builder.AppendLine("        DrawCreatedAssetMinMaxField(\"" + labelName + "\", " + minVariableName + "Property, " + maxVariableName + "Property);");
                    f_builder.AppendLine("        GUILayout.Space(4.0f);");
                    f_builder.AppendLine();
                    continue;
                }

                f_builder.AppendLine("        DrawCreatedAssetProperty(");
                f_builder.AppendLine("            f_serializedObject,");
                f_builder.AppendLine("            \"" + variableName + "\",");
                f_builder.AppendLine("            \"" + labelName + "\");");
                f_builder.AppendLine("        GUILayout.Space(4.0f);");
                f_builder.AppendLine();
            }
        }

        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// 指定したPropertyを表示ラベル付きで描画します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    /// <param name=\"f_serializedObject\">対象SerializedObject</param>");
        f_builder.AppendLine("    /// <param name=\"f_propertyName\">Property名</param>");
        f_builder.AppendLine("    /// <param name=\"f_labelName\">表示ラベル</param>");
        f_builder.AppendLine("    private void DrawCreatedAssetProperty(");
        f_builder.AppendLine("        SerializedObject f_serializedObject,");
        f_builder.AppendLine("        string f_propertyName,");
        f_builder.AppendLine("        string f_labelName)");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        SerializedProperty property = f_serializedObject.FindProperty(f_propertyName);");
        f_builder.AppendLine();
        f_builder.AppendLine("        if (property == null)");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            return;");
        f_builder.AppendLine("        }");
        f_builder.AppendLine();
        f_builder.AppendLine("        EditorGUILayout.PropertyField(");
        f_builder.AppendLine("            property,");
        f_builder.AppendLine("            new GUIContent(f_labelName),");
        f_builder.AppendLine("            true);");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// MinMaxFieldを表示ラベル付きで描画します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    /// <param name=\"f_labelName\">表示ラベル</param>");
        f_builder.AppendLine("    /// <param name=\"f_minProperty\">Min側Property</param>");
        f_builder.AppendLine("    /// <param name=\"f_maxProperty\">Max側Property</param>");
        f_builder.AppendLine("    private void DrawCreatedAssetMinMaxField(");
        f_builder.AppendLine("        string f_labelName,");
        f_builder.AppendLine("        SerializedProperty f_minProperty,");
        f_builder.AppendLine("        SerializedProperty f_maxProperty)");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        if (f_minProperty == null || f_maxProperty == null)");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            return;");
        f_builder.AppendLine("        }");
        f_builder.AppendLine();

        f_builder.AppendLine("        Rect rowRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);");
        f_builder.AppendLine();

        f_builder.AppendLine("        float mainLabelWidth = 120.0f;");
        f_builder.AppendLine("        float smallLabelWidth = 28.0f;");
        f_builder.AppendLine("        float spacing = 6.0f;");
        f_builder.AppendLine();

        f_builder.AppendLine("        float valueAreaX = rowRect.x + mainLabelWidth + spacing;");
        f_builder.AppendLine("        float valueAreaWidth = rowRect.width - mainLabelWidth - spacing;");
        f_builder.AppendLine("        float fieldWidth = (valueAreaWidth - smallLabelWidth - smallLabelWidth - (spacing * 3.0f)) * 0.5f;");
        f_builder.AppendLine("        fieldWidth = Mathf.Max(35.0f, fieldWidth);");
        f_builder.AppendLine();

        f_builder.AppendLine("        Rect labelRect = new Rect(rowRect.x, rowRect.y, mainLabelWidth, rowRect.height);");
        f_builder.AppendLine("        Rect minLabelRect = new Rect(valueAreaX, rowRect.y, smallLabelWidth, rowRect.height);");
        f_builder.AppendLine("        Rect minValueRect = new Rect(minLabelRect.xMax + spacing, rowRect.y, fieldWidth, rowRect.height);");
        f_builder.AppendLine("        Rect maxLabelRect = new Rect(minValueRect.xMax + spacing, rowRect.y, smallLabelWidth, rowRect.height);");
        f_builder.AppendLine("        Rect maxValueRect = new Rect(maxLabelRect.xMax + spacing, rowRect.y, fieldWidth, rowRect.height);");
        f_builder.AppendLine();

        f_builder.AppendLine("        EditorGUI.LabelField(labelRect, f_labelName);");
        f_builder.AppendLine("        EditorGUI.LabelField(minLabelRect, \"Min\");");
        f_builder.AppendLine("        EditorGUI.PropertyField(minValueRect, f_minProperty, GUIContent.none);");
        f_builder.AppendLine("        EditorGUI.LabelField(maxLabelRect, \"Max\");");
        f_builder.AppendLine("        EditorGUI.PropertyField(maxValueRect, f_maxProperty, GUIContent.none);");
        f_builder.AppendLine("    }");
        f_builder.AppendLine();
    }

    /// <summary>
    /// 生成EditorWindowの入力値をScriptableObjectへ代入する処理を追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    /// <param name="f_assetVariableName">代入先アセット変数名</param>
    private void AppendGeneratedAssignScriptableObjectValues(
        StringBuilder f_builder,
        string f_assetVariableName)
    {
        if (m_FieldDataList == null)
        {
            return;
        }

        for (int i = 0 ; i < m_FieldDataList.Count ; i++)
        {
            CSED_CreateTools_FieldData fieldData = m_FieldDataList[i];

            string variableName = CreateGeneratedVariableName(fieldData.FieldName, i);

            if (IsGeneratedSingleMinMaxField(fieldData))
            {
                string minVariableName = CreateGeneratedMinVariableName(variableName);
                string maxVariableName = CreateGeneratedMaxVariableName(variableName);

                f_builder.AppendLine("        " + f_assetVariableName + "." + minVariableName + " = " + minVariableName + ";");
                f_builder.AppendLine("        " + f_assetVariableName + "." + maxVariableName + " = " + maxVariableName + ";");

                continue;
            }

            if (fieldData.FieldType == CSE_CreateTools_FieldType.List)
            {
                f_builder.AppendLine("        " + f_assetVariableName + "." + variableName + " = new " + GetGeneratedFieldTypeName(fieldData) + "(" + variableName + ");");
            }
            else
            {
                f_builder.AppendLine("        " + f_assetVariableName + "." + variableName + " = " + variableName + ";");
            }
        }
    }
}
#endif
