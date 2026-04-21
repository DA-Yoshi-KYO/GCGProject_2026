/*
+=====================================
 ファイル名 : CSE_CreateTools_VerticalSplit.cs
 概要     : SCreateToolsツールの上下分割制御クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/04/22 新規作成
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// ツール作成ツールの上下分割制御をまとめるクラス
/// </summary>
public partial class CSE_CreateTools
{
    /// <summary>
    /// 左内部上下分割の初期化を行う。
    /// </summary>
    private void InitializeVerticalSplit()
    {
        if (m_IsVerticalInitialized)
        {
            ClampVerticalSplit();
            return;
        }

        Rect leftRect = GetLeftWindowRect();
        float totalHeight = leftRect.height - c_VerticalSplitterHeight;

        m_LeftTopCurrentHeight = totalHeight * c_LeftTopInitRatio;

        ClampVerticalSplit();

        m_IsVerticalInitialized = true;
    }

    /// <summary>
    /// 上下分割のドラッグ処理を行う。
    /// </summary>
    /// <param name="f_event">現在イベント</param>
    private void HandleVerticalSplitDrag(Event f_event)
    {
        Rect splitterRect = GetLeftVerticalSplitterRect();

        if (f_event.type == EventType.MouseDown && f_event.button == 0)
        {
            if (splitterRect.Contains(f_event.mousePosition))
            {
                m_IsDraggingLeftVertical = true;
                m_VerticalDragOffset = f_event.mousePosition.y - splitterRect.yMin;
                f_event.Use();
            }
        }

        if (f_event.type == EventType.MouseDrag && f_event.button == 0)
        {
            if (m_IsDraggingLeftVertical)
            {
                Rect leftRect = GetLeftWindowRect();

                float splitterY = f_event.mousePosition.y - m_VerticalDragOffset;
                float nextTopHeight = splitterY - leftRect.y;

                float maxTopHeight = leftRect.height - c_VerticalSplitterHeight - c_LeftBottomMinHeight;

                m_LeftTopCurrentHeight = Mathf.Clamp(
                    nextTopHeight,
                    c_LeftTopMinHeight,
                    maxTopHeight);

                Repaint();
                f_event.Use();
            }
        }

        if (f_event.type == EventType.MouseUp)
        {
            if (m_IsDraggingLeftVertical)
            {
                m_IsDraggingLeftVertical = false;
                f_event.Use();
            }
        }
    }

    /// <summary>
    /// 上下分割バーを描画する。
    /// </summary>
    private void DrawVerticalSplitters()
    {
        Rect splitterRect = GetLeftVerticalSplitterRect();

        EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeVertical);
        EditorGUI.DrawRect(splitterRect, new Color(0.22f, 0.22f, 0.22f));
    }

    /// <summary>
    /// 上下分割の値を有効範囲に収める。
    /// </summary>
    private void ClampVerticalSplit()
    {
        Rect leftRect = GetLeftWindowRect();

        float maxTopHeight = leftRect.height - c_VerticalSplitterHeight - c_LeftBottomMinHeight;

        m_LeftTopCurrentHeight = Mathf.Clamp(
            m_LeftTopCurrentHeight,
            c_LeftTopMinHeight,
            maxTopHeight);
    }

    /// <summary>
    /// 左上Rectを取得する。
    /// </summary>
    /// <returns>左上Rect</returns>
    private Rect GetLeftTopWindowRect()
    {
        Rect leftRect = GetLeftWindowRect();

        return new Rect(
            leftRect.x,
            leftRect.y,
            leftRect.width,
            m_LeftTopCurrentHeight);
    }

    /// <summary>
    /// 左内部上下分割バーRectを取得する。
    /// </summary>
    /// <returns>上下分割バーRect</returns>
    private Rect GetLeftVerticalSplitterRect()
    {
        Rect topRect = GetLeftTopWindowRect();

        return new Rect(
            topRect.x,
            topRect.yMax,
            topRect.width,
            c_VerticalSplitterHeight);
    }

    /// <summary>
    /// 左下Rectを取得する。
    /// </summary>
    /// <returns>左下Rect</returns>
    private Rect GetLeftBottomWindowRect()
    {
        Rect leftRect = GetLeftWindowRect();
        Rect splitterRect = GetLeftVerticalSplitterRect();

        return new Rect(
            leftRect.x,
            splitterRect.yMax,
            leftRect.width,
            leftRect.yMax - splitterRect.yMax);
    }
}
#endif