/*
+=====================================
 ファイル名 : CSED_CreateTools_VariablePalette.cs
 概要     : CreateToolsの左上に表示する変数パレット描画クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/08 新規作成
            2026/05/08 黒枠内にInspector風パネルを表示する形に変更
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// CreateToolsの変数パレット描画処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// 黒枠から内側パネルまでの余白です。
    /// </summary>
    private const float c_VariablePalettePanelMargin = 6.0f;

    /// <summary>
    /// 内側パネル内の余白です。
    /// </summary>
    private const float c_VariablePaletteContentPadding = 8.0f;

    /// <summary>
    /// 変数パレットボタン高さです。
    /// </summary>
    private const float c_VariablePaletteButtonHeight = 24.0f;

    /// <summary>
    /// 変数パレット項目間の余白です。
    /// </summary>
    private const float c_VariablePaletteSpacing = 4.0f;

    /// <summary>
    /// 変数パレットのスクロール位置です。
    /// </summary>
    private Vector2 m_VariablePaletteScrollPosition;

    /// <summary>
    /// 左上エリアに変数パレットを描画します。
    /// </summary>
    /// <param name="f_areaRect">変数パレットを描画する範囲</param>
    private void DrawVariablePalette(Rect f_areaRect)
    {
        Rect panelRect = GetVariablePalettePanelRect(f_areaRect);

        DrawVariablePalettePanel(panelRect);

        Rect contentRect = GetVariablePaletteContentRect(panelRect);

        GUILayout.BeginArea(contentRect);
        {
            m_VariablePaletteScrollPosition = EditorGUILayout.BeginScrollView(m_VariablePaletteScrollPosition);
            {
                DrawVariablePaletteTitle();

                GUILayout.Space(c_VariablePaletteSpacing);

                DrawVariablePaletteButton(CSE_CreateTools_FieldType.Int, "int");
                DrawVariablePaletteButton(CSE_CreateTools_FieldType.Float, "float");
                DrawVariablePaletteButton(CSE_CreateTools_FieldType.String, "string");
                DrawVariablePaletteButton(CSE_CreateTools_FieldType.Bool, "bool");
                DrawVariablePaletteButton(CSE_CreateTools_FieldType.List, "List<T>");
            }
            EditorGUILayout.EndScrollView();
        }
        GUILayout.EndArea();
    }

    /// <summary>
    /// 変数パレット用の内側パネルRectを取得します。
    /// </summary>
    /// <param name="f_areaRect">左上エリア全体のRect</param>
    /// <returns>内側パネルRect</returns>
    private Rect GetVariablePalettePanelRect(Rect f_areaRect)
    {
        return new Rect(
            f_areaRect.x + c_VariablePalettePanelMargin,
            f_areaRect.y + c_VariablePalettePanelMargin,
            Mathf.Max(0.0f, f_areaRect.width - (c_VariablePalettePanelMargin * 2.0f)),
            Mathf.Max(0.0f, f_areaRect.height - (c_VariablePalettePanelMargin * 2.0f)));
    }

    /// <summary>
    /// 変数パレット用の内側コンテンツRectを取得します。
    /// </summary>
    /// <param name="f_panelRect">内側パネルRect</param>
    /// <returns>コンテンツRect</returns>
    private Rect GetVariablePaletteContentRect(Rect f_panelRect)
    {
        return new Rect(
            f_panelRect.x + c_VariablePaletteContentPadding,
            f_panelRect.y + c_VariablePaletteContentPadding,
            Mathf.Max(0.0f, f_panelRect.width - (c_VariablePaletteContentPadding * 2.0f)),
            Mathf.Max(0.0f, f_panelRect.height - (c_VariablePaletteContentPadding * 2.0f)));
    }

    /// <summary>
    /// 変数パレットの内側パネルを描画します。
    /// </summary>
    /// <param name="f_panelRect">内側パネルRect</param>
    private void DrawVariablePalettePanel(Rect f_panelRect)
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
    /// 変数パレットのタイトルを描画します。
    /// </summary>
    private void DrawVariablePaletteTitle()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.normal.textColor = Color.white;

        EditorGUILayout.LabelField("変数パレット", titleStyle);
    }

    /// <summary>
    /// 変数型ボタンを描画します。
    /// </summary>
    /// <param name="f_fieldType">ボタンに対応する変数型</param>
    /// <param name="f_buttonText">ボタンに表示する文字</param>
    private void DrawVariablePaletteButton(
        CSE_CreateTools_FieldType f_fieldType,
        string f_buttonText)
    {
        if (GUILayout.Button(f_buttonText, GUILayout.Height(c_VariablePaletteButtonHeight)))
        {
            OnVariablePaletteButtonClicked(f_fieldType);
        }

        GUILayout.Space(c_VariablePaletteSpacing);
    }

    /// <summary>
    /// 変数型ボタンが押された時の処理を行います。
    /// </summary>
    /// <param name="f_fieldType">押された変数型</param>
    private void OnVariablePaletteButtonClicked(CSE_CreateTools_FieldType f_fieldType)
    {
        AddFieldData(f_fieldType);
    }

    /// <summary>
    /// 変数型の表示名を取得します。
    /// </summary>
    /// <param name="f_fieldType">表示名を取得する変数型</param>
    /// <returns>変数型の表示名</returns>
    private string GetFieldTypeDisplayName(CSE_CreateTools_FieldType f_fieldType)
    {
        switch (f_fieldType)
        {
            case CSE_CreateTools_FieldType.Int:
                return "int";

            case CSE_CreateTools_FieldType.Float:
                return "float";

            case CSE_CreateTools_FieldType.String:
                return "string";

            case CSE_CreateTools_FieldType.Bool:
                return "bool";

            case CSE_CreateTools_FieldType.List:
                return "List<T>";

            default:
                return "Unknown";
        }
    }
}
#endif
