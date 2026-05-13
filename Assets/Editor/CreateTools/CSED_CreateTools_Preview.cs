/*
+=====================================
 ファイル名 : CSED_CreateTools_Preview.cs
 概要     : CreateToolsの右側に作成後エディター見本を表示するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/13 新規作成
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
    /// 黒枠からプレビューパネルまでの余白です。
    /// </summary>
    private const float c_PreviewPanelMargin = 6.0f;

    /// <summary>
    /// プレビューパネル内の余白です。
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

        Rect panelRect = GetPreviewPanelRect(f_areaRect);

        DrawPreviewPanel(panelRect);

        Rect contentRect = GetPreviewContentRect(panelRect);

        GUILayout.BeginArea(contentRect);
        {
            m_PreviewScrollPosition = EditorGUILayout.BeginScrollView(m_PreviewScrollPosition);
            {
                DrawPreviewTitle();

                GUILayout.Space(c_PreviewSpacing);

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
    }

    /// <summary>
    /// プレビュー用の内側パネルRectを取得します。
    /// </summary>
    /// <param name="f_areaRect">右側エリア全体のRect</param>
    /// <returns>内側パネルRect</returns>
    private Rect GetPreviewPanelRect(Rect f_areaRect)
    {
        return new Rect(
            f_areaRect.x + c_PreviewPanelMargin,
            f_areaRect.y + c_PreviewPanelMargin,
            Mathf.Max(0.0f, f_areaRect.width - (c_PreviewPanelMargin * 2.0f)),
            Mathf.Max(0.0f, f_areaRect.height - (c_PreviewPanelMargin * 2.0f)));
    }

    /// <summary>
    /// プレビュー用のコンテンツRectを取得します。
    /// </summary>
    /// <param name="f_panelRect">内側パネルRect</param>
    /// <returns>コンテンツRect</returns>
    private Rect GetPreviewContentRect(Rect f_panelRect)
    {
        return new Rect(
            f_panelRect.x + c_PreviewContentPadding,
            f_panelRect.y + c_PreviewContentPadding,
            Mathf.Max(0.0f, f_panelRect.width - (c_PreviewContentPadding * 2.0f)),
            Mathf.Max(0.0f, f_panelRect.height - (c_PreviewContentPadding * 2.0f)));
    }

    /// <summary>
    /// プレビューの内側パネルを描画します。
    /// </summary>
    /// <param name="f_panelRect">内側パネルRect</param>
    private void DrawPreviewPanel(Rect f_panelRect)
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
    /// プレビュータイトルを描画します。
    /// </summary>
    private void DrawPreviewTitle()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.normal.textColor = Color.white;

        EditorGUILayout.LabelField("作成後エディター見本", titleStyle);
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
                EditorGUILayout.IntField(label, 0);
                break;

            case CSE_CreateTools_FieldType.Float:
                EditorGUILayout.FloatField(label, 0.0f);
                break;

            case CSE_CreateTools_FieldType.Bool:
                EditorGUILayout.Toggle(label, false);
                break;

            case CSE_CreateTools_FieldType.String:
                EditorGUILayout.TextField(label, string.Empty);
                break;

            case CSE_CreateTools_FieldType.List:
                EditorGUILayout.TextField(label, "List<T>");
                break;

            default:
                EditorGUILayout.TextField(label, string.Empty);
                break;
        }
    }

    /// <summary>
    /// Sliderの見本を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象のFieldData</param>
    private void DrawPreviewSlider(CSED_CreateTools_FieldData f_fieldData)
    {
        string label = GetPreviewLabel(f_fieldData);

        switch (f_fieldData.FieldType)
        {
            case CSE_CreateTools_FieldType.Int:
                EditorGUILayout.IntSlider(label, 0, 0, 100);
                break;

            case CSE_CreateTools_FieldType.Float:
                EditorGUILayout.Slider(label, 0.0f, 0.0f, 1.0f);
                break;

            default:
                EditorGUILayout.Slider(label, 0.0f, 0.0f, 1.0f);
                break;
        }
    }

    /// <summary>
    /// MinMaxFieldの見本を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象のFieldData</param>
    private void DrawPreviewMinMaxField(CSED_CreateTools_FieldData f_fieldData)
    {
        string label = GetPreviewLabel(f_fieldData);

        EditorGUILayout.LabelField(label);

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.FloatField("Min", 0.0f);
            EditorGUILayout.FloatField("Max", 100.0f);
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Toggleの見本を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象のFieldData</param>
    private void DrawPreviewToggle(CSED_CreateTools_FieldData f_fieldData)
    {
        string label = GetPreviewLabel(f_fieldData);

        EditorGUILayout.Toggle(label, false);
    }

    /// <summary>
    /// TextAreaの見本を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象のFieldData</param>
    private void DrawPreviewTextArea(CSED_CreateTools_FieldData f_fieldData)
    {
        string label = GetPreviewLabel(f_fieldData);

        EditorGUILayout.LabelField(label);
        EditorGUILayout.TextArea(string.Empty, GUILayout.Height(c_PreviewTextAreaHeight));
    }

    /// <summary>
    /// ReorderableList風の見本を描画します。
    /// </summary>
    /// <param name="f_fieldData">描画対象のFieldData</param>
    private void DrawPreviewReorderableList(CSED_CreateTools_FieldData f_fieldData)
    {
        string label = GetPreviewLabel(f_fieldData);

        EditorGUILayout.LabelField(label);
        EditorGUILayout.HelpBox("Element 0\nElement 1", MessageType.None);
    }

    /// <summary>
    /// プレビューで表示するラベル名を取得します。
    /// </summary>
    /// <param name="f_fieldData">対象FieldData</param>
    /// <returns>プレビュー表示用ラベル</returns>
    private string GetPreviewLabel(CSED_CreateTools_FieldData f_fieldData)
    {
        if (f_fieldData.FieldLayoutType == CSE_CreateTools_FieldLayoutType.InputField &&
            string.IsNullOrEmpty(f_fieldData.TagName) == false)
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
