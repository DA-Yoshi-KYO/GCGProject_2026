/*
+=====================================
 ファイル名 : CSE_CreateTools_HorizontalSplit.cs
 概要     : SCreateToolsツールの左右分割制御クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/04/22 新規作成
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// ツール作成ツールの左右分割制御をまとめるクラス
/// </summary>
public partial class CSE_CreateTools
{
    /// <summary>
    /// 左右分割の初期化を行う。
    /// </summary>
    private void InitializeHorizontalSplit()
    {
        if (m_IsHorizontalInitialized)
        {
            ClampHorizontalSplit();
            return;
        }

        float totalWidth = GetHorizontalContentWidth();

        m_LeftCurrentWidth = totalWidth * c_LeftInitRatio;
        m_RightCurrentWidth = totalWidth * c_RightInitRatio;

        ClampHorizontalSplit();

        m_IsHorizontalInitialized = true;
    }

    /// <summary>
    /// 左右分割のドラッグ処理を行う。
    /// </summary>
    /// <param name="f_event">現在イベント</param>
    private void HandleHorizontalSplitDrag(Event f_event)
    {
        Rect leftSplitterRect = GetLeftHorizontalSplitterRect();
        Rect rightSplitterRect = GetRightHorizontalSplitterRect();

        if (f_event.type == EventType.MouseDown && f_event.button == 0)
        {
            if (leftSplitterRect.Contains(f_event.mousePosition))
            {
                m_IsDraggingLeftHorizontal = true;
                m_HorizontalDragOffset = f_event.mousePosition.x - leftSplitterRect.xMin;
                f_event.Use();
            }
            else if (rightSplitterRect.Contains(f_event.mousePosition))
            {
                m_IsDraggingRightHorizontal = true;
                m_HorizontalDragOffset = f_event.mousePosition.x - rightSplitterRect.xMin;
                f_event.Use();
            }
        }

        if (f_event.type == EventType.MouseDrag && f_event.button == 0)
        {
            float totalWidth = GetHorizontalContentWidth();

            if (m_IsDraggingLeftHorizontal)
            {
                float splitterX = f_event.mousePosition.x - m_HorizontalDragOffset;
                float nextLeftWidth = splitterX - c_Margin;
                float maxLeftWidth = totalWidth - m_RightCurrentWidth - c_CenterMinWidth;

                m_LeftCurrentWidth = Mathf.Clamp(nextLeftWidth, c_LeftMinWidth, maxLeftWidth);

                Repaint();
                f_event.Use();
            }
            else if (m_IsDraggingRightHorizontal)
            {
                float splitterX = f_event.mousePosition.x - m_HorizontalDragOffset;
                float nextRightWidth = position.width - c_Margin - (splitterX + c_HorizontalSplitterWidth);
                float maxRightWidth = totalWidth - m_LeftCurrentWidth - c_CenterMinWidth;

                m_RightCurrentWidth = Mathf.Clamp(nextRightWidth, c_RightMinWidth, maxRightWidth);

                Repaint();
                f_event.Use();
            }
        }

        if (f_event.type == EventType.MouseUp)
        {
            if (m_IsDraggingLeftHorizontal || m_IsDraggingRightHorizontal)
            {
                m_IsDraggingLeftHorizontal = false;
                m_IsDraggingRightHorizontal = false;
                f_event.Use();
            }
        }
    }

    /// <summary>
    /// 左右分割バーを描画する。
    /// </summary>
    private void DrawHorizontalSplitters()
    {
        Rect leftSplitterRect = GetLeftHorizontalSplitterRect();
        Rect rightSplitterRect = GetRightHorizontalSplitterRect();

        EditorGUIUtility.AddCursorRect(leftSplitterRect, MouseCursor.ResizeHorizontal);
        EditorGUIUtility.AddCursorRect(rightSplitterRect, MouseCursor.ResizeHorizontal);

        EditorGUI.DrawRect(leftSplitterRect, new Color(0.22f, 0.22f, 0.22f));
        EditorGUI.DrawRect(rightSplitterRect, new Color(0.22f, 0.22f, 0.22f));
    }

    /// <summary>
    /// 左右分割の有効横幅を取得する。
    /// </summary>
    /// <returns>有効横幅</returns>
    private float GetHorizontalContentWidth()
    {
        return position.width - (c_Margin * 2.0f) - (c_HorizontalSplitterWidth * 2.0f);
    }

    /// <summary>
    /// 左右分割の値を有効範囲に収める。
    /// </summary>
    private void ClampHorizontalSplit()
    {
        float totalWidth = GetHorizontalContentWidth();

        float maxLeftWidth = totalWidth - m_RightCurrentWidth - c_CenterMinWidth;
        m_LeftCurrentWidth = Mathf.Clamp(m_LeftCurrentWidth, c_LeftMinWidth, maxLeftWidth);

        float maxRightWidth = totalWidth - m_LeftCurrentWidth - c_CenterMinWidth;
        m_RightCurrentWidth = Mathf.Clamp(m_RightCurrentWidth, c_RightMinWidth, maxRightWidth);
    }

    /// <summary>
    /// 左ウィンドウRectを取得する。
    /// </summary>
    /// <returns>左Rect</returns>
    private Rect GetLeftWindowRect()
    {
        return new Rect(
            c_Margin,
            c_Margin,
            m_LeftCurrentWidth,
            position.height - (c_Margin * 2.0f));
    }

    /// <summary>
    /// 左側左右分割バーRectを取得する。
    /// </summary>
    /// <returns>左側分割バーRect</returns>
    private Rect GetLeftHorizontalSplitterRect()
    {
        Rect leftRect = GetLeftWindowRect();

        return new Rect(
            leftRect.xMax,
            c_Margin,
            c_HorizontalSplitterWidth,
            position.height - (c_Margin * 2.0f));
    }

    /// <summary>
    /// 中央ウィンドウRectを取得する。
    /// </summary>
    /// <returns>中央Rect</returns>
    private Rect GetCenterWindowRect()
    {
        Rect leftSplitterRect = GetLeftHorizontalSplitterRect();

        return new Rect(
            leftSplitterRect.xMax,
            c_Margin,
            GetCenterWindowWidth(),
            position.height - (c_Margin * 2.0f));
    }

    /// <summary>
    /// 右側左右分割バーRectを取得する。
    /// </summary>
    /// <returns>右側分割バーRect</returns>
    private Rect GetRightHorizontalSplitterRect()
    {
        Rect centerRect = GetCenterWindowRect();

        return new Rect(
            centerRect.xMax,
            c_Margin,
            c_HorizontalSplitterWidth,
            position.height - (c_Margin * 2.0f));
    }

    /// <summary>
    /// 右ウィンドウRectを取得する。
    /// </summary>
    /// <returns>右Rect</returns>
    private Rect GetRightWindowRect()
    {
        Rect rightSplitterRect = GetRightHorizontalSplitterRect();

        return new Rect(
            rightSplitterRect.xMax,
            c_Margin,
            m_RightCurrentWidth,
            position.height - (c_Margin * 2.0f));
    }

    /// <summary>
    /// 中央ウィンドウの横幅を取得する。
    /// </summary>
    /// <returns>中央横幅</returns>
    private float GetCenterWindowWidth()
    {
        return GetHorizontalContentWidth() - m_LeftCurrentWidth - m_RightCurrentWidth;
    }
}
#endif