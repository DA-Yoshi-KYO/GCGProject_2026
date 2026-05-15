/*
+=====================================
 ファイル名 : CSED_CreateTools_Preview.cs
 概要     : CreateToolsの右側に作成後エディター見本を表示するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/13 新規作成
            2026/05/13 黒背景上にUnity風の仮想EditorWindowを描画
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// CreateToolsの作成後エディター見本描画処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// Min Max Fieldプレビューのラベル幅です。
    /// </summary>
    private const float c_PreviewMinMaxMainLabelWidth = 110.0f;

    /// <summary>
    /// Min / Max文字の幅です。
    /// </summary>
    private const float c_PreviewMinMaxSmallLabelWidth = 28.0f;

    /// <summary>
    /// Min Max Fieldプレビューの項目間余白です。
    /// </summary>
    private const float c_PreviewMinMaxSpacing = 6.0f;

    /// <summary>
    /// Min Max Fieldプレビューの最小入力欄幅です。
    /// </summary>
    private const float c_PreviewMinMaxMinFieldWidth = 35.0f;

    /// <summary>
    /// 仮想EditorWindowの外側余白です。
    /// </summary>
    private const float c_PreviewWindowMargin = 24.0f;

    /// <summary>
    /// 仮想EditorWindowのタイトルバー高さです。
    /// </summary>
    private const float c_PreviewTitleBarHeight = 22.0f;

    /// <summary>
    /// 仮想EditorWindowのタブ最小横幅です。
    /// </summary>
    private const float c_PreviewTabMinWidth = 72.0f;

    /// <summary>
    /// 仮想EditorWindowのタブ最大横幅です。
    /// </summary>
    private const float c_PreviewTabMaxWidth = 180.0f;

    /// <summary>
    /// 仮想EditorWindowのタブ内側横余白です。
    /// </summary>
    private const float c_PreviewTabHorizontalPadding = 18.0f;

    /// <summary>
    /// 仮想EditorWindowの中身余白です。
    /// </summary>
    private const float c_PreviewContentPadding = 10.0f;

    /// <summary>
    /// プレビュー項目同士の余白です。
    /// </summary>
    private const float c_PreviewSpacing = 8.0f;

    /// <summary>
    /// TextAreaの仮表示高さです。
    /// </summary>
    private const float c_PreviewTextAreaHeight = 48.0f;

    /// <summary>
    /// 右側エリアに作成後エディター見本を描画します。
    /// </summary>
    /// <param name="f_areaRect">右側エリア全体のRect</param>
    private void DrawPreview(Rect f_areaRect)
    {
        EnsureFieldDataList();

        Rect windowRect = GetPreviewEditorWindowRect(f_areaRect);

        DrawPreviewEditorWindowFrame(windowRect);

        Rect contentRect = GetPreviewEditorWindowContentRect(windowRect);

        GUILayout.BeginArea(contentRect);
        {
            m_PreviewScrollPosition = EditorGUILayout.BeginScrollView(m_PreviewScrollPosition);
            {
                if (m_FieldDataList.Count <= 0)
                {
                    DrawPreviewEmptyMessage();
                }
                else
                {
                    DrawPreviewFields();
                }
            }
            EditorGUILayout.EndScrollView();
        }
        GUILayout.EndArea();

        if (m_IsPreviewEditorSettingsOpen)
        {
            DrawPreviewEditorSettingsPanel(windowRect);
        }
    }

    /// <summary>
    /// 仮想EditorWindowのRectを取得します。
    /// </summary>
    /// <param name="f_areaRect">右側エリア全体のRect</param>
    /// <returns>仮想EditorWindowのRect</returns>
    private Rect GetPreviewEditorWindowRect(Rect f_areaRect)
    {
        return new Rect(
            f_areaRect.x + c_PreviewWindowMargin,
            f_areaRect.y + c_PreviewWindowMargin,
            Mathf.Max(160.0f, f_areaRect.width - (c_PreviewWindowMargin * 2.0f)),
            Mathf.Max(160.0f, f_areaRect.height - (c_PreviewWindowMargin * 2.0f)));
    }

    /// <summary>
    /// 仮想EditorWindowの中身Rectを取得します。
    /// </summary>
    /// <param name="f_windowRect">仮想EditorWindowのRect</param>
    /// <returns>仮想EditorWindowの中身Rect</returns>
    private Rect GetPreviewEditorWindowContentRect(Rect f_windowRect)
    {
        return new Rect(
            f_windowRect.x + c_PreviewContentPadding,
            f_windowRect.y + c_PreviewTitleBarHeight + c_PreviewContentPadding,
            Mathf.Max(0.0f, f_windowRect.width - (c_PreviewContentPadding * 2.0f)),
            Mathf.Max(0.0f, f_windowRect.height - c_PreviewTitleBarHeight - (c_PreviewContentPadding * 2.0f)));
    }

    /// <summary>
    /// 仮想EditorWindowの枠を描画します。
    /// </summary>
    /// <param name="f_windowRect">仮想EditorWindowのRect</param>
    private void DrawPreviewEditorWindowFrame(Rect f_windowRect)
    {
        DrawPreviewWindowBody(f_windowRect);
        DrawPreviewTitleBar(f_windowRect);
        DrawPreviewTab(f_windowRect);
        DrawPreviewWindowButtons(f_windowRect);
        DrawPreviewWindowBorder(f_windowRect);
    }

    /// <summary>
    /// 仮想EditorWindowの本体を描画します。
    /// </summary>
    /// <param name="f_windowRect">仮想EditorWindowのRect</param>
    private void DrawPreviewWindowBody(Rect f_windowRect)
    {
        EditorGUI.DrawRect(f_windowRect, new Color(0.18f, 0.18f, 0.18f));
    }

    /// <summary>
    /// 仮想EditorWindowのタイトルバーを描画します。
    /// </summary>
    /// <param name="f_windowRect">仮想EditorWindowのRect</param>
    private void DrawPreviewTitleBar(Rect f_windowRect)
    {
        Rect titleBarRect = new Rect(
            f_windowRect.x,
            f_windowRect.y,
            f_windowRect.width,
            c_PreviewTitleBarHeight);

        EditorGUI.DrawRect(titleBarRect, new Color(0.13f, 0.13f, 0.13f));
    }

    /// <summary>
    /// 仮想EditorWindowのタブを描画します。
    /// </summary>
    /// <param name="f_windowRect">仮想EditorWindowのRect</param>
    private void DrawPreviewTab(Rect f_windowRect)
    {
        string titleName = GetPreviewEditorTitleName();

        GUIStyle tabStyle = new GUIStyle(EditorStyles.label);
        tabStyle.normal.textColor = new Color(0.82f, 0.82f, 0.82f);

        float tabWidth = GetPreviewTabWidth(titleName, tabStyle, f_windowRect);

        Rect tabRect = new Rect(
            f_windowRect.x,
            f_windowRect.y,
            tabWidth,
            c_PreviewTitleBarHeight);

        EditorGUI.DrawRect(tabRect, new Color(0.22f, 0.22f, 0.22f));

        Rect tabLabelRect = new Rect(
            tabRect.x + 8.0f,
            tabRect.y + 2.0f,
            Mathf.Max(0.0f, tabRect.width - 16.0f),
            tabRect.height);

        EditorGUI.LabelField(tabLabelRect, titleName, tabStyle);
    }

    /// <summary>
    /// 仮想EditorWindowのタブ横幅を取得します。
    /// </summary>
    /// <param name="f_titleName">タイトル名</param>
    /// <param name="f_tabStyle">タブ文字スタイル</param>
    /// <param name="f_windowRect">仮想EditorWindowのRect</param>
    /// <returns>タブ横幅</returns>
    private float GetPreviewTabWidth(
        string f_titleName,
        GUIStyle f_tabStyle,
        Rect f_windowRect)
    {
        Vector2 titleSize = f_tabStyle.CalcSize(new GUIContent(f_titleName));

        float preferredWidth = titleSize.x + c_PreviewTabHorizontalPadding;

        float maxWidthByWindow = Mathf.Max(
            c_PreviewTabMinWidth,
            f_windowRect.width - 70.0f);

        float tabMaxWidth = Mathf.Min(c_PreviewTabMaxWidth, maxWidthByWindow);

        return Mathf.Clamp(
            preferredWidth,
            c_PreviewTabMinWidth,
            tabMaxWidth);
    }

    /// <summary>
    /// 右側プレビューの仮想EditorWindowタイトル名を取得します。
    /// </summary>
    /// <returns>仮想EditorWindowタイトル名</returns>
    private string GetPreviewEditorTitleName()
    {
        if (string.IsNullOrEmpty(m_PreviewEditorTitleName))
        {
            return "Untitled";
        }

        return m_PreviewEditorTitleName;
    }

    /// <summary>
    /// 仮想EditorWindowの右上ボタン風表示を描画します。
    /// </summary>
    /// <param name="f_windowRect">仮想EditorWindowのRect</param>
    private void DrawPreviewWindowButtons(Rect f_windowRect)
    {
        GUIStyle buttonStyle = new GUIStyle(EditorStyles.label);
        buttonStyle.alignment = TextAnchor.MiddleCenter;
        buttonStyle.normal.textColor = new Color(0.82f, 0.82f, 0.82f);

        Rect menuButtonRect = new Rect(
            f_windowRect.xMax - 62.0f,
            f_windowRect.y + 2.0f,
            18.0f,
            c_PreviewTitleBarHeight - 4.0f);

        Rect maximizeButtonRect = new Rect(
            f_windowRect.xMax - 40.0f,
            f_windowRect.y + 2.0f,
            18.0f,
            c_PreviewTitleBarHeight - 4.0f);

        Rect closeButtonRect = new Rect(
            f_windowRect.xMax - 20.0f,
            f_windowRect.y + 2.0f,
            18.0f,
            c_PreviewTitleBarHeight - 4.0f);

        if (GUI.Button(menuButtonRect, "⋮", buttonStyle))
        {
            ShowPreviewEditorMenu();
        }

        EditorGUI.LabelField(maximizeButtonRect, "□", buttonStyle);
        EditorGUI.LabelField(closeButtonRect, "×", buttonStyle);
    }

    /// <summary>
    /// 右側プレビューの仮想EditorWindowメニューを表示します。
    /// </summary>
    private void ShowPreviewEditorMenu()
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(
            new GUIContent("エディター設定"),
            false,
            OpenPreviewEditorSettings);

        menu.ShowAsContext();
    }

    /// <summary>
    /// 右側プレビューの仮想EditorWindow設定パネルを描画します。
    /// </summary>
    /// <param name="f_windowRect">仮想EditorWindowのRect</param>
    private void DrawPreviewEditorSettingsPanel(Rect f_windowRect)
    {
        Rect panelRect = new Rect(
            f_windowRect.x + 24.0f,
            f_windowRect.y + c_PreviewTitleBarHeight + 24.0f,
            Mathf.Min(280.0f, f_windowRect.width - 48.0f),
            92.0f);

        EditorGUI.DrawRect(panelRect, new Color(0.24f, 0.24f, 0.24f));

        GUI.Box(panelRect, GUIContent.none);

        GUILayout.BeginArea(new Rect(
            panelRect.x + 10.0f,
            panelRect.y + 8.0f,
            panelRect.width - 20.0f,
            panelRect.height - 16.0f));
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("エディター設定", EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("×", GUILayout.Width(24.0f)))
                {
                    m_IsPreviewEditorSettingsOpen = false;
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(8.0f);

            EditorGUI.BeginChangeCheck();

            m_PreviewEditorTitleName = EditorGUILayout.TextField(
                "Title Name",
                m_PreviewEditorTitleName);

            if (EditorGUI.EndChangeCheck())
            {
                Repaint();
            }
        }
        GUILayout.EndArea();
    }

    /// <summary>
    /// 右側プレビューのエディター設定を開きます。
    /// </summary>
    private void OpenPreviewEditorSettings()
    {
        m_IsPreviewEditorSettingsOpen = true;

        Repaint();
    }

    /// <summary>
    /// 仮想EditorWindowの枠線を描画します。
    /// </summary>
    /// <param name="f_windowRect">仮想EditorWindowのRect</param>
    private void DrawPreviewWindowBorder(Rect f_windowRect)
    {
        Handles.BeginGUI();

        Color oldColor = Handles.color;
        Handles.color = new Color(0.32f, 0.32f, 0.32f);

        Vector3 topLeft = new Vector3(f_windowRect.xMin, f_windowRect.yMin);
        Vector3 topRight = new Vector3(f_windowRect.xMax, f_windowRect.yMin);
        Vector3 bottomLeft = new Vector3(f_windowRect.xMin, f_windowRect.yMax);
        Vector3 bottomRight = new Vector3(f_windowRect.xMax, f_windowRect.yMax);

        Handles.DrawLine(topLeft, topRight);
        Handles.DrawLine(topRight, bottomRight);
        Handles.DrawLine(bottomRight, bottomLeft);
        Handles.DrawLine(bottomLeft, topLeft);

        Handles.color = oldColor;

        Handles.EndGUI();
    }

    /// <summary>
    /// プレビュー対象が空のときの案内を描画します。
    /// </summary>
    private void DrawPreviewEmptyMessage()
    {
        EditorGUILayout.HelpBox(
            "中央の作業用エディターにFieldを追加すると、ここに作成後の見本が表示されます。",
            MessageType.Info);
    }

    /// <summary>
    /// FieldData一覧をもとにプレビュー項目を描画します。
    /// </summary>
    private void DrawPreviewFields()
    {
        EditorGUI.BeginDisabledGroup(true);
        {
            for (int i = 0 ; i < m_FieldDataList.Count ; i++)
            {
                DrawPreviewField(m_FieldDataList[i]);
                GUILayout.Space(c_PreviewSpacing);
            }
        }
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// FieldDataのレイアウト種別に応じたプレビューを描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象のFieldData</param>
    private void DrawPreviewField(CSED_CreateTools_FieldData f_fieldData)
    {
        switch (f_fieldData.FieldLayoutType)
        {
            case CSE_CreateTools_FieldLayoutType.InputField:
                DrawPreviewInputField(f_fieldData);
                break;

            case CSE_CreateTools_FieldLayoutType.Slider:
                DrawPreviewSlider(f_fieldData);
                break;

            case CSE_CreateTools_FieldLayoutType.MinMaxField:
                DrawPreviewMinMaxField(f_fieldData);
                break;

            case CSE_CreateTools_FieldLayoutType.Toggle:
                DrawPreviewToggle(f_fieldData);
                break;

            case CSE_CreateTools_FieldLayoutType.TextArea:
                DrawPreviewTextArea(f_fieldData);
                break;

            case CSE_CreateTools_FieldLayoutType.ReorderableList:
                DrawPreviewReorderableList(f_fieldData);
                break;

            default:
                DrawPreviewInputField(f_fieldData);
                break;
        }
    }

    /// <summary>
    /// InputFieldの見本を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象のFieldData</param>
    private void DrawPreviewInputField(CSED_CreateTools_FieldData f_fieldData)
    {
        string label = GetPreviewLabel(f_fieldData);

        switch (f_fieldData.FieldType)
        {
            case CSE_CreateTools_FieldType.Int:
                EditorGUILayout.IntField(
                    label,
                    GetPreviewIntDefaultValue(f_fieldData));
                break;

            case CSE_CreateTools_FieldType.Float:
                EditorGUILayout.FloatField(
                    label,
                    GetPreviewFloatDefaultValue(f_fieldData));
                break;

            case CSE_CreateTools_FieldType.Bool:
                EditorGUILayout.Toggle(
                    label,
                    GetPreviewBoolDefaultValue(f_fieldData));
                break;

            case CSE_CreateTools_FieldType.String:
                EditorGUILayout.TextField(
                    label,
                    GetPreviewStringDefaultValue(f_fieldData));
                break;

            case CSE_CreateTools_FieldType.List:
                EditorGUILayout.TextField(label, "List<T>");
                break;

            default:
                EditorGUILayout.TextField(
                    label,
                    GetPreviewStringDefaultValue(f_fieldData));
                break;
        }
    }

    /// <summary>
    /// プレビュー用のint初期値を取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>int初期値</returns>
    private int GetPreviewIntDefaultValue(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.IsDefaultValueNull)
        {
            return 0;
        }

        int result = 0;
        int.TryParse(f_fieldData.DefaultValueText, out result);

        return result;
    }

    /// <summary>
    /// プレビュー用の通常初期値をfloatで取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>float初期値</returns>
    private float GetPreviewFloatDefaultValue(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.IsDefaultValueNull)
        {
            return 0.0f;
        }

        float result = 0.0f;

        float.TryParse(
            f_fieldData.DefaultValueText,
            out result);

        return result;
    }

    /// <summary>
    /// プレビュー用のSlider最大値を取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>Slider最大値</returns>
    private float GetPreviewSliderMaxValue(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.IsSliderMaxValueNull)
        {
            return 100.0f;
        }

        float result = 100.0f;

        float.TryParse(
            f_fieldData.SliderMaxValueText,
            out result);

        return result;
    }

    /// <summary>
    /// プレビュー用のbool初期値を取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>bool初期値</returns>
    private bool GetPreviewBoolDefaultValue(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.IsDefaultValueNull)
        {
            return false;
        }

        bool result = false;

        bool.TryParse(
            f_fieldData.DefaultValueText,
            out result);

        return result;
    }

    /// <summary>
    /// プレビュー用のstring初期値を取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>string初期値</returns>
    private string GetPreviewStringDefaultValue(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.IsDefaultValueNull)
        {
            return string.Empty;
        }

        return f_fieldData.DefaultValueText;
    }

    /// <summary>
    /// Sliderの見本を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象のFieldData</param>
    private void DrawPreviewSlider(CSED_CreateTools_FieldData f_fieldData)
    {
        string label = GetPreviewLabel(f_fieldData);

        float minValue = GetPreviewSliderMinValue(f_fieldData);
        float maxValue = GetPreviewSliderMaxValue(f_fieldData);
        float defaultValue = GetPreviewFloatDefaultValue(f_fieldData);

        if (maxValue < minValue)
        {
            float temp = minValue;
            minValue = maxValue;
            maxValue = temp;
        }

        defaultValue = Mathf.Clamp(defaultValue, minValue, maxValue);

        EditorGUILayout.Slider(
            label,
            defaultValue,
            minValue,
            maxValue);
    }

    /// <summary>
    /// プレビュー用のSlider最小値を取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>Slider最小値</returns>
    private float GetPreviewSliderMinValue(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.IsSliderMinValueNull)
        {
            return 0.0f;
        }

        float result = 0.0f;

        float.TryParse(
            f_fieldData.SliderMinValueText,
            out result);

        return result;
    }

    /// <summary>
    /// Min Max Fieldの見本を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象のFieldData</param>
    private void DrawPreviewMinMaxField(CSED_CreateTools_FieldData f_fieldData)
    {
        string label = GetPreviewLabel(f_fieldData);

        float minValue = GetPreviewMinDefaultValue(f_fieldData);
        float maxValue = GetPreviewMaxDefaultValue(f_fieldData);

        Rect rowRect = EditorGUILayout.GetControlRect(
            false,
            EditorGUIUtility.singleLineHeight);

        float labelWidth = Mathf.Min(
            c_PreviewMinMaxMainLabelWidth,
            rowRect.width * 0.35f);

        float valueAreaX =
            rowRect.x
            + labelWidth
            + c_PreviewMinMaxSpacing;

        float valueAreaWidth =
            rowRect.width
            - labelWidth
            - c_PreviewMinMaxSpacing;

        float valueFieldWidth =
            (
                valueAreaWidth
                - c_PreviewMinMaxSmallLabelWidth
                - c_PreviewMinMaxSmallLabelWidth
                - (c_PreviewMinMaxSpacing * 3.0f)
            ) * 0.5f;

        valueFieldWidth = Mathf.Max(
            c_PreviewMinMaxMinFieldWidth,
            valueFieldWidth);

        Rect labelRect = new Rect(
            rowRect.x,
            rowRect.y,
            labelWidth,
            rowRect.height);

        Rect minLabelRect = new Rect(
            valueAreaX,
            rowRect.y,
            c_PreviewMinMaxSmallLabelWidth,
            rowRect.height);

        Rect minValueRect = new Rect(
            minLabelRect.xMax + c_PreviewMinMaxSpacing,
            rowRect.y,
            valueFieldWidth,
            rowRect.height);

        Rect maxLabelRect = new Rect(
            minValueRect.xMax + c_PreviewMinMaxSpacing,
            rowRect.y,
            c_PreviewMinMaxSmallLabelWidth,
            rowRect.height);

        Rect maxValueRect = new Rect(
            maxLabelRect.xMax + c_PreviewMinMaxSpacing,
            rowRect.y,
            valueFieldWidth,
            rowRect.height);

        EditorGUI.LabelField(labelRect, label);
        EditorGUI.LabelField(minLabelRect, "Min");
        EditorGUI.FloatField(minValueRect, minValue);
        EditorGUI.LabelField(maxLabelRect, "Max");
        EditorGUI.FloatField(maxValueRect, maxValue);
    }

    /// <summary>
    /// プレビュー用のMin初期値を取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>Min初期値</returns>
    private float GetPreviewMinDefaultValue(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.IsDefaultMinValueNull)
        {
            return 0.0f;
        }

        float result = 0.0f;

        float.TryParse(
            f_fieldData.DefaultMinValueText,
            out result);

        return result;
    }

    /// <summary>
    /// プレビュー用のMax初期値を取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>Max初期値</returns>
    private float GetPreviewMaxDefaultValue(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.IsDefaultMaxValueNull)
        {
            return 1.0f;
        }

        float result = 1.0f;

        float.TryParse(
            f_fieldData.DefaultMaxValueText,
            out result);

        return result;
    }

    /// <summary>
    /// Toggleの見本を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象のFieldData</param>
    private void DrawPreviewToggle(CSED_CreateTools_FieldData f_fieldData)
    {
        string label = GetPreviewLabel(f_fieldData);

        EditorGUILayout.Toggle(
            label,
            GetPreviewBoolDefaultValue(f_fieldData));
    }

    /// <summary>
    /// TextAreaの見本を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象のFieldData</param>
    private void DrawPreviewTextArea(CSED_CreateTools_FieldData f_fieldData)
    {
        string label = GetPreviewLabel(f_fieldData);
        string defaultValue = GetPreviewStringDefaultValue(f_fieldData);

        EditorGUILayout.LabelField(label);

        EditorGUILayout.TextArea(
            defaultValue,
            GUILayout.Height(64.0f));
    }

    /// <summary>
    /// ReorderableListの見本を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象のFieldData</param>
    private void DrawPreviewReorderableList(CSED_CreateTools_FieldData f_fieldData)
    {
        string label = GetPreviewLabel(f_fieldData);
        int count = GetPreviewListDefaultCount(f_fieldData);

        EditorGUILayout.LabelField(label);

        for (int i = 0 ; i < count ; i++)
        {
            EditorGUILayout.LabelField("Element " + i.ToString());
        }
    }

    /// <summary>
    /// プレビュー用のList初期要素数を取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>List初期要素数</returns>
    private int GetPreviewListDefaultCount(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.IsListDefaultCountNull)
        {
            return 2;
        }

        int result = 2;

        int.TryParse(
            f_fieldData.ListDefaultCountText,
            out result);

        return Mathf.Max(0, result);
    }

    /// <summary>
    /// プレビュー表示用のラベル名を取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>プレビューに表示するラベル名</returns>
    private string GetPreviewLabel(CSED_CreateTools_FieldData f_fieldData)
    {
        if (string.IsNullOrEmpty(f_fieldData.TagName) == false)
        {
            return f_fieldData.TagName;
        }

        if (string.IsNullOrEmpty(f_fieldData.FieldName) == false)
        {
            return f_fieldData.FieldName;
        }

        return "New Field";
    }
}
#endif
