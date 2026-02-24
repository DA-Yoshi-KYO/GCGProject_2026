/*
+=====================================
 ファイル名 : CSE_ScriptSearch_WikiLink.cs
 概要     : ScriptSearchツールの「左下WIKIリンク」
 作者     : ヨシモト リョウ
 履歴     : 2026/02/15 新規作成
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public partial class CSE_ScriptSearch
{
    private const string WikiUrl = "https://www.notion.so/3094e199f4d880098da5dddf225934c6?source=copy_link";

    /// <summary>
    /// 左下に固定でWIKIリンクを描画する
    /// </summary>
    private void DrawWikiLinkBottomLeft()
    {
        const float padX = 8.0f;
        const float padY = 6.0f;

        GUIContent content = new GUIContent("WIKI : ツール使い方");

        GUIStyle style = new GUIStyle(EditorStyles.linkLabel)
        {
            alignment = TextAnchor.MiddleLeft
        };

        // 表示に必要なサイズを計算
        Vector2 size = style.CalcSize(content);

        Rect rect = new Rect(
            padX,
            position.height - size.y - padY,
            size.x + 6.0f,
            size.y
        );

        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

        if (GUI.Button(rect, content, style))
        {
            Application.OpenURL(WikiUrl);
        }
    }

}
#endif
