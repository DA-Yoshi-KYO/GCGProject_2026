/*
+=====================================
 ファイル名 : CSED_CreateTools_EditorPalette.cs
 概要     : CreateToolsの左上に表示するエディターパレット描画クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/08 新規作成
            2026/05/13 変数パレットからエディターパレットへ変更
            2026/05/13 Layout要素カテゴリを追加
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// CreateToolsのエディターパレット描画処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// エディターパレットボタンとスクロールバーの間の余白です。
    /// </summary>
    private const float c_EditorPaletteButtonRightGap = 14.0f;

    /// <summary>
    /// エディターパレットのスクロールバー想定幅です。
    /// </summary>
    private const float c_EditorPaletteScrollBarWidth = 16.0f;

    /// <summary>
    /// エディターパレットボタンの最小横幅です。
    /// </summary>
    private const float c_EditorPaletteButtonMinWidth = 80.0f;

    /// <summary>
    /// エディターパレットの現在のコンテンツ横幅です。
    /// </summary>
    private float m_EditorPaletteCurrentContentWidth;

    /// <summary>
    /// 黒枠からエディターパレットパネルまでの余白です。
    /// </summary>
    private const float c_EditorPalettePanelMargin = 6.0f;

    /// <summary>
    /// エディターパレットパネル内の余白です。
    /// </summary>
    private const float c_EditorPaletteContentPadding = 8.0f;

    /// <summary>
    /// エディターパレットボタン高さです。
    /// </summary>
    private const float c_EditorPaletteButtonHeight = 24.0f;

    /// <summary>
    /// エディターパレット項目間の余白です。
    /// </summary>
    private const float c_EditorPaletteSpacing = 4.0f;

    /// <summary>
    /// エディターパレットカテゴリ間の余白です。
    /// </summary>
    private const float c_EditorPaletteCategorySpacing = 10.0f;

    /// <summary>
    /// エディターパレットのスクロール位置です。
    /// </summary>
    private Vector2 m_EditorPaletteScrollPosition;

    /// <summary>
    /// 左上エリアにエディターパレットを描画します。
    /// </summary>
    /// <param name="f_areaRect">エディターパレットを描画する範囲</param>
    private void DrawEditorPalette(Rect f_areaRect)
    {
        Rect panelRect = GetEditorPalettePanelRect(f_areaRect);

        DrawEditorPalettePanel(panelRect);

        Rect contentRect = GetEditorPaletteContentRect(panelRect);

        m_EditorPaletteCurrentContentWidth = contentRect.width;

        GUILayout.BeginArea(contentRect);
        {
            m_EditorPaletteScrollPosition = EditorGUILayout.BeginScrollView(m_EditorPaletteScrollPosition);
            {
                DrawEditorPaletteTitle();

                GUILayout.Space(c_EditorPaletteCategorySpacing);

                DrawEditorPaletteCategoryLabel("Variable");

                GUILayout.Space(c_EditorPaletteSpacing);

                DrawVariablePaletteButton(CSE_CreateTools_FieldType.Int, "int");
                DrawVariablePaletteButton(CSE_CreateTools_FieldType.Float, "float");
                DrawVariablePaletteButton(CSE_CreateTools_FieldType.String, "string");
                DrawVariablePaletteButton(CSE_CreateTools_FieldType.Bool, "bool");
                DrawVariablePaletteButton(CSE_CreateTools_FieldType.List, "List<T>");

                GUILayout.Space(c_EditorPaletteCategorySpacing);

                DrawEditorPaletteCategoryLabel("Layout");

                GUILayout.Space(c_EditorPaletteSpacing);

                DrawLayoutPaletteButton(CSE_CreateTools_LayoutElementType.Space, "Space");
                DrawLayoutPaletteButton(CSE_CreateTools_LayoutElementType.Line, "Line");
                DrawLayoutPaletteButton(CSE_CreateTools_LayoutElementType.Header, "Header");
                DrawLayoutPaletteButton(CSE_CreateTools_LayoutElementType.Label, "Label");
                DrawLayoutPaletteButton(CSE_CreateTools_LayoutElementType.Box, "Box");
                DrawLayoutPaletteButton(CSE_CreateTools_LayoutElementType.Tab, "Tab");
            }
            EditorGUILayout.EndScrollView();
        }
        GUILayout.EndArea();
    }

    /// <summary>
    /// エディターパレット用の内側パネルRectを取得します。
    /// </summary>
    /// <param name="f_areaRect">左上エリア全体のRect</param>
    /// <returns>内側パネルRect</returns>
    private Rect GetEditorPalettePanelRect(Rect f_areaRect)
    {
        return new Rect(
            f_areaRect.x + c_EditorPalettePanelMargin,
            f_areaRect.y + c_EditorPalettePanelMargin,
            Mathf.Max(0.0f, f_areaRect.width - (c_EditorPalettePanelMargin * 2.0f)),
            Mathf.Max(0.0f, f_areaRect.height - (c_EditorPalettePanelMargin * 2.0f)));
    }

    /// <summary>
    /// エディターパレット用の内側コンテンツRectを取得します。
    /// </summary>
    /// <param name="f_panelRect">内側パネルRect</param>
    /// <returns>コンテンツRect</returns>
    private Rect GetEditorPaletteContentRect(Rect f_panelRect)
    {
        return new Rect(
            f_panelRect.x + c_EditorPaletteContentPadding,
            f_panelRect.y + c_EditorPaletteContentPadding,
            Mathf.Max(0.0f, f_panelRect.width - (c_EditorPaletteContentPadding * 2.0f)),
            Mathf.Max(0.0f, f_panelRect.height - (c_EditorPaletteContentPadding * 2.0f)));
    }

    /// <summary>
    /// エディターパレットの内側パネルを描画します。
    /// </summary>
    /// <param name="f_panelRect">内側パネルRect</param>
    private void DrawEditorPalettePanel(Rect f_panelRect)
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
    /// エディターパレットのタイトルを描画します。
    /// </summary>
    private void DrawEditorPaletteTitle()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.normal.textColor = Color.white;

        EditorGUILayout.LabelField("エディターパレット", titleStyle);
    }

    /// <summary>
    /// エディターパレットのカテゴリラベルを描画します。
    /// </summary>
    /// <param name="f_label">カテゴリ名</param>
    private void DrawEditorPaletteCategoryLabel(string f_label)
    {
        GUIStyle categoryStyle = new GUIStyle(EditorStyles.boldLabel);
        categoryStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

        EditorGUILayout.LabelField(f_label, categoryStyle);
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
        if (GUILayout.Button(
            f_buttonText,
            GUILayout.Width(GetEditorPaletteButtonWidth()),
            GUILayout.Height(c_EditorPaletteButtonHeight)))
        {
            OnVariablePaletteButtonClicked(f_fieldType);
        }

        GUILayout.Space(c_EditorPaletteSpacing);
    }

    /// <summary>
    /// エディターパレットボタンの横幅を取得します。
    /// </summary>
    /// <returns>ボタン横幅</returns>
    private float GetEditorPaletteButtonWidth()
    {
        float buttonWidth =
            m_EditorPaletteCurrentContentWidth
            - c_EditorPaletteScrollBarWidth
            - c_EditorPaletteButtonRightGap;

        return Mathf.Max(c_EditorPaletteButtonMinWidth, buttonWidth);
    }

    /// <summary>
    /// レイアウト要素ボタンを描画します。
    /// </summary>
    /// <param name="f_layoutElementType">ボタンに対応するレイアウト要素種別</param>
    /// <param name="f_buttonText">ボタンに表示する文字</param>
    private void DrawLayoutPaletteButton(
        CSE_CreateTools_LayoutElementType f_layoutElementType,
        string f_buttonText)
    {
        if (GUILayout.Button(
            f_buttonText,
            GUILayout.Width(GetEditorPaletteButtonWidth()),
            GUILayout.Height(c_EditorPaletteButtonHeight)))
        {
            OnLayoutPaletteButtonClicked(f_layoutElementType);
        }

        GUILayout.Space(c_EditorPaletteSpacing);
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
    /// レイアウト要素ボタンが押された時の処理を行います。
    /// </summary>
    /// <param name="f_layoutElementType">押されたレイアウト要素種別</param>
    private void OnLayoutPaletteButtonClicked(CSE_CreateTools_LayoutElementType f_layoutElementType)
    {
        Debug.Log("[CreateTools] Layout要素ボタンが押されました : " + f_layoutElementType);
    }
}
#endif
