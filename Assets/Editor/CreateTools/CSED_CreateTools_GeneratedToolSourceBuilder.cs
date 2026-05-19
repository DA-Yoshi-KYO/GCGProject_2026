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
        builder.AppendLine("    /// メニューからウィンドウを開きます。");
        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    [MenuItem(\"" + EscapeString(m_GeneratedToolMenuPath) + "\")]");
        builder.AppendLine("    public static void ShowWindow()");
        builder.AppendLine("    {");
        builder.AppendLine("        " + f_className + " window = GetWindow<" + f_className + ">(\"" + EscapeString(m_GeneratedToolWindowTitle) + "\");");
        builder.AppendLine("        window.minSize = new Vector2(360.0f, 240.0f);");
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
        builder.AppendLine("    /// Create Asset設定を開きます。");
        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    private void OpenCreateAssetSettings()");
        builder.AppendLine("    {");
        builder.AppendLine("        m_IsCreateAssetSettingsOpen = true;");
        builder.AppendLine("        Repaint();");
        builder.AppendLine("    }");
        builder.AppendLine();

        builder.AppendLine("    /// <summary>");
        builder.AppendLine("    /// GUIを描画します。");
        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    private void OnGUI()");
        builder.AppendLine("    {");

        AppendGeneratedOnGui(builder);
        AppendGeneratedCreateScriptableObjectButton(builder);

        builder.AppendLine("    }");
        builder.AppendLine();

        AppendGeneratedCreateAssetSettingsPanel(builder);

        AppendGeneratedCreateScriptableObjectMembers(builder, f_dataClassName);

        builder.AppendLine("}");
        builder.AppendLine("#endif");

        return builder.ToString();
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

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Float)
        {
            f_builder.AppendLine("        " + f_variableName + " = EditorGUILayout.FloatField(\"" + EscapeString(f_labelName) + "\", " + f_variableName + ");");
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

            case CSE_CreateTools_FieldType.Float:
                return "float";

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

            case CSE_CreateTools_FieldType.Float:
                return "float";

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

            case CSE_CreateTools_FieldType.Float:
                return GetGeneratedFloatText(f_fieldData.ListDefaultElementValueTextList[f_index]);

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

            case CSE_CreateTools_FieldType.Float:
                return "0.0f";

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
        f_builder.AppendLine();

        f_builder.AppendLine("        DrawCreateAssetSettingsPanel();");
    }

    /// <summary>
    /// 生成EditorWindowにScriptableObject作成用メンバーを追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    /// <param name="f_dataClassName">生成するScriptableObjectクラス名</param>
    private void AppendGeneratedCreateScriptableObjectMembers(
        StringBuilder f_builder,
        string f_dataClassName)
    {
        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// Create Asset設定を開いているかどうかです。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    private bool m_IsCreateAssetSettingsOpen = false;");
        f_builder.AppendLine();

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
        f_builder.AppendLine("    }");
    }

    /// <summary>
    /// 生成EditorWindowにCreate Asset設定パネルを追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    private void AppendGeneratedCreateAssetSettingsPanel(StringBuilder f_builder)
    {
        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// Create Asset設定パネルを描画します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    private void DrawCreateAssetSettingsPanel()");
        f_builder.AppendLine("    {");
        f_builder.AppendLine("        if (m_IsCreateAssetSettingsOpen == false)");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            return;");
        f_builder.AppendLine("        }");
        f_builder.AppendLine();
        f_builder.AppendLine("        GUILayout.Space(8.0f);");
        f_builder.AppendLine();
        f_builder.AppendLine("        EditorGUILayout.BeginVertical(EditorStyles.helpBox);");
        f_builder.AppendLine("        {");
        f_builder.AppendLine("            EditorGUILayout.BeginHorizontal();");
        f_builder.AppendLine("            {");
        f_builder.AppendLine("                EditorGUILayout.LabelField(\"Create Asset Settings\", EditorStyles.boldLabel);");
        f_builder.AppendLine();
        f_builder.AppendLine("                GUILayout.FlexibleSpace();");
        f_builder.AppendLine();
        f_builder.AppendLine("                if (GUILayout.Button(\"×\", GUILayout.Width(24.0f)))");
        f_builder.AppendLine("                {");
        f_builder.AppendLine("                    m_IsCreateAssetSettingsOpen = false;");
        f_builder.AppendLine("                }");
        f_builder.AppendLine("            }");
        f_builder.AppendLine("            EditorGUILayout.EndHorizontal();");
        f_builder.AppendLine();
        f_builder.AppendLine("            GUILayout.Space(6.0f);");
        f_builder.AppendLine();
        f_builder.AppendLine("            m_AssetFileName = EditorGUILayout.TextField(\"Asset Name\", m_AssetFileName);");
        f_builder.AppendLine("            m_AssetOutputFolderPath = EditorGUILayout.TextField(\"Asset Folder\", m_AssetOutputFolderPath);");
        f_builder.AppendLine("        }");
        f_builder.AppendLine("        EditorGUILayout.EndVertical();");
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
