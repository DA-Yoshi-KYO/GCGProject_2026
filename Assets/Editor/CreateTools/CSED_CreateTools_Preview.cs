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
    /// 生成後エディターで最初に表示するAsset名です。
    /// </summary>
    private string m_GeneratedDefaultAssetName = "NewData";

    /// <summary>
    /// 生成後エディターで最初に表示するAsset保存先フォルダです。
    /// </summary>
    private string m_GeneratedDefaultAssetFolder = "Assets/Programmer/ScriptableObject";

    /// <summary>
    /// 生成ファイルヘッダーに出力する作者名です。
    /// </summary>
    private string m_GeneratedHeaderAuthorName = "ヨシモト リョウ";

    /// <summary>
    /// 生成ファイルヘッダーに出力する履歴日付です。
    /// </summary>
    private string m_GeneratedHeaderHistoryDate = System.DateTime.Now.ToString("yyyy/MM/dd");

    /// <summary>
    /// 生成EditorWindowファイルの概要です。
    /// </summary>
    private string m_GeneratedEditorHeaderContents = "CreateToolsから自動生成されたEditorWindow";

    /// <summary>
    /// 生成ScriptableObjectファイルの概要です。
    /// </summary>
    private string m_GeneratedDataHeaderContents = "CreateToolsから自動生成されたScriptableObjectデータ";

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
    /// 生成ファイルヘッダー設定を描画します。
    /// </summary>
    private void DrawGeneratedHeaderSettingFields()
    {
        EditorGUILayout.LabelField("Header設定", EditorStyles.boldLabel);

        m_GeneratedHeaderAuthorName = EditorGUILayout.TextField(
            "  Author Name",
            m_GeneratedHeaderAuthorName);

        m_GeneratedHeaderHistoryDate = EditorGUILayout.TextField(
            "  History Date",
            m_GeneratedHeaderHistoryDate);

        m_GeneratedEditorHeaderContents = EditorGUILayout.TextField(
            "  Editor Contents",
            m_GeneratedEditorHeaderContents);

        m_GeneratedDataHeaderContents = EditorGUILayout.TextField(
            "  Data Contents",
            m_GeneratedDataHeaderContents);
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
            Mathf.Min(360.0f, f_windowRect.width - 48.0f),
            430.0f);

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

            m_GeneratedToolWindowTitle = m_PreviewEditorTitleName;

            GUILayout.Space(12.0f);

            DrawGeneratedHeaderSettingFields();

            GUILayout.Space(12.0f);

            DrawGeneratedToolSettingFields();

            GUILayout.Space(12.0f);

            DrawGeneratedDefaultAssetSettingFields();

            GUILayout.Space(12.0f);

            DrawGeneratedToolCreateButton();

            if (EditorGUI.EndChangeCheck())
            {
                Repaint();
            }
        }
        GUILayout.EndArea();
    }

    /// <summary>
    /// 生成後エディターのCreate Asset初期設定を描画します。
    /// </summary>
    private void DrawGeneratedDefaultAssetSettingFields()
    {
        EditorGUILayout.LabelField("Default設定", EditorStyles.boldLabel);

        m_GeneratedDefaultAssetName = EditorGUILayout.TextField(
            "  Default Asset Name",
            m_GeneratedDefaultAssetName);

        m_GeneratedDefaultAssetFolder = EditorGUILayout.TextField(
            "  Default Asset Folder",
            m_GeneratedDefaultAssetFolder);
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
            "中央の作業用エディターにFieldを追加すると、ここに作成後の見本が表示されます",
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
    /// Fieldのプレビューを描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象のFieldData</param>
    private void DrawPreviewField(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.List)
        {
            DrawPreviewListByLayout(f_fieldData);
            return;
        }

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

            case CSE_CreateTools_FieldLayoutType.Select:
                DrawPreviewSelect(f_fieldData);
                break;

            default:
                DrawPreviewInputField(f_fieldData);
                break;
        }
    }

    /// <summary>
    /// Selectの見本を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象FieldData</param>
    private void DrawPreviewSelect(CSED_CreateTools_FieldData f_fieldData)
    {
        string label = GetPreviewLabel(f_fieldData);

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.ScriptableObject)
        {
            DrawPreviewScriptableObjectSelect(label, f_fieldData);
            return;
        }

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.Script)
        {
            DrawPreviewScriptSelect(label, f_fieldData);
            return;
        }

        if (f_fieldData.FieldType == CSE_CreateTools_FieldType.GameObject)
        {
            DrawPreviewGameObjectSelect(label, f_fieldData);
            return;
        }

        EditorGUILayout.LabelField(label, "Select未対応Type");
    }

    /// <summary>
    /// GameObject選択の見本を描画します。
    /// </summary>
    /// <param name="f_label">表示ラベル</param>
    /// <param name="f_fieldData">描画対象FieldData</param>
    private void DrawPreviewGameObjectSelect(
        string f_label,
        CSED_CreateTools_FieldData f_fieldData)
    {
        f_fieldData.DefaultGameObjectValue = (GameObject)EditorGUILayout.ObjectField(
            f_label,
            f_fieldData.DefaultGameObjectValue,
            typeof(GameObject),
            false);
    }

    /// <summary>
    /// ScriptableObject選択の見本を描画します。
    /// </summary>
    /// <param name="f_label">表示ラベル</param>
    /// <param name="f_fieldData">描画対象FieldData</param>
    private void DrawPreviewScriptableObjectSelect(
        string f_label,
        CSED_CreateTools_FieldData f_fieldData)
    {
        System.Type targetType = GetSelectedScriptableObjectType(f_fieldData);

        UnityEngine.Object selectedObject = EditorGUILayout.ObjectField(
            f_label,
            f_fieldData.DefaultScriptableObjectValue,
            targetType,
            false);

        f_fieldData.DefaultScriptableObjectValue = selectedObject as ScriptableObject;
    }


    /// <summary>
    /// Script選択の見本を描画します。
    /// </summary>
    /// <param name="f_label">表示ラベル</param>
    /// <param name="f_fieldData">描画対象FieldData</param>
    private void DrawPreviewScriptSelect(
        string f_label,
        CSED_CreateTools_FieldData f_fieldData)
    {
        f_fieldData.DefaultScriptValue = (MonoScript)EditorGUILayout.ObjectField(
            f_label,
            f_fieldData.DefaultScriptValue,
            typeof(MonoScript),
            false);
    }
    /// <summary>
    /// List型のプレビューをLayoutに応じて描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象FieldData</param>
    private void DrawPreviewListByLayout(CSED_CreateTools_FieldData f_fieldData)
    {
        EnsureListDefaultElementValueList(f_fieldData);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(GetPreviewLabel(f_fieldData));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("-", GUILayout.Width(24.0f)))
                {
                    RemoveListDefaultElement(f_fieldData);
                }

                if (GUILayout.Button("+", GUILayout.Width(24.0f)))
                {
                    AddListDefaultElement(f_fieldData);
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4.0f);

            for (int i = 0 ; i < f_fieldData.ListDefaultElementValueTextList.Count ; i++)
            {
                DrawPreviewListElementByLayout(f_fieldData, i);
            }
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Listの各要素をLayoutに応じて描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象FieldData</param>
    /// <param name="f_index">要素番号</param>
    private void DrawPreviewListElementByLayout(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        if (f_index < 0 || f_index >= f_fieldData.ListDefaultElementValueTextList.Count)
        {
            return;
        }

        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.Slider)
        {
            DrawPreviewListSliderElement(f_fieldData, f_index);
            return;
        }

        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.MinMaxField)
        {
            DrawPreviewListMinMaxElement(f_fieldData, f_index);
            return;
        }

        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.Toggle)
        {
            DrawPreviewListToggleElement(f_fieldData, f_index);
            return;
        }

        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.TextArea)
        {
            DrawPreviewListTextAreaElement(f_fieldData, f_index);
            return;
        }

        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.Select)
        {
            DrawPreviewListSelectElement(f_fieldData, f_index);
            return;
        }

        DrawPreviewListInputFieldElement(f_fieldData, f_index);
    }

    /// <summary>
    /// ListのSelect要素を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象FieldData</param>
    /// <param name="f_index">要素番号</param>
    private void DrawPreviewListSelectElement(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        EnsureListDefaultElementValueList(f_fieldData);

        if (f_index < 0 || f_index >= f_fieldData.ListDefaultObjectValueList.Count)
        {
            return;
        }

        string label = "Element " + f_index.ToString();

        if (f_fieldData.ListElementFieldType == CSE_CreateTools_FieldType.ScriptableObject)
        {
            System.Type targetType = GetSelectedScriptableObjectType(f_fieldData);

            f_fieldData.ListDefaultObjectValueList[f_index] = EditorGUILayout.ObjectField(
                label,
                f_fieldData.ListDefaultObjectValueList[f_index],
                targetType,
                false);

            return;
        }

        if (f_fieldData.ListElementFieldType == CSE_CreateTools_FieldType.Script)
        {
            f_fieldData.ListDefaultObjectValueList[f_index] = EditorGUILayout.ObjectField(
                label,
                f_fieldData.ListDefaultObjectValueList[f_index],
                typeof(MonoScript),
                false);

            return;
        }

        if (f_fieldData.ListElementFieldType == CSE_CreateTools_FieldType.GameObject)
        {
            f_fieldData.ListDefaultObjectValueList[f_index] = EditorGUILayout.ObjectField(
                label,
                f_fieldData.ListDefaultObjectValueList[f_index],
                typeof(GameObject),
                false);

            return;
        }

        EditorGUILayout.LabelField(label, "Select未対応Type");
    }

    /// <summary>
    /// ListのMinMaxField要素を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象FieldData</param>
    /// <param name="f_index">要素番号</param>
    private void DrawPreviewListMinMaxElement(
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

        Rect rowRect = EditorGUILayout.GetControlRect(
            false,
            EditorGUIUtility.singleLineHeight);

        float mainLabelWidth = 110.0f;
        float smallLabelWidth = 28.0f;
        float spacing = 6.0f;

        float valueAreaX =
            rowRect.x
            + mainLabelWidth
            + spacing;

        float valueAreaWidth =
            rowRect.width
            - mainLabelWidth
            - spacing;

        float fieldWidth =
            (valueAreaWidth
            - smallLabelWidth
            - smallLabelWidth
            - (spacing * 3.0f)) * 0.5f;

        fieldWidth = Mathf.Max(30.0f, fieldWidth);

        Rect elementLabelRect = new Rect(
            rowRect.x,
            rowRect.y,
            mainLabelWidth,
            rowRect.height);

        Rect minLabelRect = new Rect(
            valueAreaX,
            rowRect.y,
            smallLabelWidth,
            rowRect.height);

        Rect minValueRect = new Rect(
            minLabelRect.xMax + spacing,
            rowRect.y,
            fieldWidth,
            rowRect.height);

        Rect maxLabelRect = new Rect(
            minValueRect.xMax + spacing,
            rowRect.y,
            smallLabelWidth,
            rowRect.height);

        Rect maxValueRect = new Rect(
            maxLabelRect.xMax + spacing,
            rowRect.y,
            fieldWidth,
            rowRect.height);

        EditorGUI.LabelField(
            elementLabelRect,
            "Element " + f_index.ToString());

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
    /// ListのInputField要素を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象FieldData</param>
    /// <param name="f_index">要素番号</param>
    private void DrawPreviewListInputFieldElement(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        string label = "Element " + f_index.ToString();

        switch (f_fieldData.ListElementFieldType)
        {
            case CSE_CreateTools_FieldType.Int:
                {
                    int value = 0;

                    int.TryParse(
                        f_fieldData.ListDefaultElementValueTextList[f_index],
                        out value);

                    value = EditorGUILayout.IntField(label, value);

                    f_fieldData.ListDefaultElementValueTextList[f_index] = value.ToString();
                    break;
                }

            case CSE_CreateTools_FieldType.Float:
                {
                    float value = 0.0f;

                    float.TryParse(
                        f_fieldData.ListDefaultElementValueTextList[f_index],
                        out value);

                    value = EditorGUILayout.FloatField(label, value);

                    f_fieldData.ListDefaultElementValueTextList[f_index] = value.ToString();
                    break;
                }

            case CSE_CreateTools_FieldType.Bool:
                {
                    bool value = false;

                    bool.TryParse(
                        f_fieldData.ListDefaultElementValueTextList[f_index],
                        out value);

                    value = EditorGUILayout.Toggle(label, value);

                    f_fieldData.ListDefaultElementValueTextList[f_index] = value.ToString();
                    break;
                }

            default:
                {
                    f_fieldData.ListDefaultElementValueTextList[f_index] =
                        EditorGUILayout.TextField(
                            label,
                            f_fieldData.ListDefaultElementValueTextList[f_index]);

                    break;
                }
        }
    }

    /// <summary>
    /// ListのToggle要素を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象FieldData</param>
    /// <param name="f_index">要素番号</param>
    private void DrawPreviewListToggleElement(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        string label = "Element " + f_index.ToString();

        bool value = false;

        bool.TryParse(
            f_fieldData.ListDefaultElementValueTextList[f_index],
            out value);

        value = EditorGUILayout.Toggle(label, value);

        f_fieldData.ListDefaultElementValueTextList[f_index] = value.ToString();
    }

    /// <summary>
    /// ListのTextArea要素を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象FieldData</param>
    /// <param name="f_index">要素番号</param>
    private void DrawPreviewListTextAreaElement(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        EditorGUILayout.LabelField("Element " + f_index.ToString());

        f_fieldData.ListDefaultElementValueTextList[f_index] =
            EditorGUILayout.TextArea(
                f_fieldData.ListDefaultElementValueTextList[f_index],
                GUILayout.Height(48.0f));
    }

    /// <summary>
    /// ListのSlider要素を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象FieldData</param>
    /// <param name="f_index">要素番号</param>
    private void DrawPreviewListSliderElement(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        string label = "Element " + f_index.ToString();

        float minValue = GetPreviewSliderMinValue(f_fieldData);
        float maxValue = GetPreviewSliderMaxValue(f_fieldData);

        if (maxValue < minValue)
        {
            float temp = minValue;
            minValue = maxValue;
            maxValue = temp;
        }

        float value = 0.0f;

        float.TryParse(
            f_fieldData.ListDefaultElementValueTextList[f_index],
            out value);

        value = Mathf.Clamp(value, minValue, maxValue);

        value = EditorGUILayout.Slider(
            label,
            value,
            minValue,
            maxValue);

        f_fieldData.ListDefaultElementValueTextList[f_index] = value.ToString();
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
    /// ReorderableListプレビューの各要素を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象FieldData</param>
    /// <param name="f_index">要素番号</param>
    private void DrawPreviewListElementField(
        CSED_CreateTools_FieldData f_fieldData,
        int f_index)
    {
        if (f_index < 0 || f_index >= f_fieldData.ListDefaultElementValueTextList.Count)
        {
            return;
        }

        string label = "Element " + f_index.ToString();

        switch (f_fieldData.ListElementFieldType)
        {
            case CSE_CreateTools_FieldType.Int:
                {
                    int value = 0;

                    int.TryParse(
                        f_fieldData.ListDefaultElementValueTextList[f_index],
                        out value);

                    value = EditorGUILayout.IntField(label, value);

                    f_fieldData.ListDefaultElementValueTextList[f_index] = value.ToString();
                    break;
                }

            case CSE_CreateTools_FieldType.Float:
                {
                    float value = 0.0f;

                    float.TryParse(
                        f_fieldData.ListDefaultElementValueTextList[f_index],
                        out value);

                    value = EditorGUILayout.FloatField(label, value);

                    f_fieldData.ListDefaultElementValueTextList[f_index] = value.ToString();
                    break;
                }

            case CSE_CreateTools_FieldType.Bool:
                {
                    bool value = false;

                    bool.TryParse(
                        f_fieldData.ListDefaultElementValueTextList[f_index],
                        out value);

                    value = EditorGUILayout.Toggle(label, value);

                    f_fieldData.ListDefaultElementValueTextList[f_index] = value.ToString();
                    break;
                }

            default:
                {
                    f_fieldData.ListDefaultElementValueTextList[f_index] =
                        EditorGUILayout.TextField(
                            label,
                            f_fieldData.ListDefaultElementValueTextList[f_index]);
                    break;
                }
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
