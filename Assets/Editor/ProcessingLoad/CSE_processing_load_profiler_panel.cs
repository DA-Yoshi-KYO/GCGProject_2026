/*
+=====================================
ファイル名 : CSE_processing_load_profiler_panel.cs
概要       : Processing Load用パフォーマンスパネル表示
作者       : ヨシモト リョウ
履歴       : 2026/09/11 新規作成
             2026/09/11 スクロール・折りたたみ機能を追加
=====================================+
*/

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public partial class CSE_processing_load
{
    // パフォーマンスパネルが開いているかどうか
    private bool profilerPanelOpen =
        true;

    // パフォーマンスパネル内部のスクロール位置
    private Vector2 profilerScrollPosition;


    /// <summary>
    /// CPU / GPU / Memory / Rendering情報を表示する
    /// </summary>
    private void DrawPerformancePanel(
        Rect viewRect)
    {
        // パネル横幅
        // 元の300pxから約40%削減したサイズ
        const float panelWidth =
            180.0f;


        // パネルを開いている時の高さ
        const float panelHeight =
            370.0f;


        // 上部ヘッダーの高さ
        const float headerHeight =
            26.0f;


        // Game画面端からの余白
        const float margin =
            10.0f;


        // =====================================
        // パネル全体
        // =====================================

        Rect panelRect =
            new Rect(
                viewRect.x + margin,
                viewRect.y + margin,
                panelWidth,

                // 閉じている場合はヘッダー部分だけ表示
                profilerPanelOpen
                    ? panelHeight
                    : headerHeight
            );


        // パネル背景
        EditorGUI.DrawRect(
            panelRect,
            new Color(
                0.05f,
                0.05f,
                0.05f,
                0.85f
            )
        );


        // =====================================
        // ヘッダー
        // =====================================

        Rect headerRect =
            new Rect(
                panelRect.x,
                panelRect.y,
                panelRect.width,
                headerHeight
            );


        // =====================================
        // 開閉ボタン
        // =====================================

        Rect buttonRect =
            new Rect(
                headerRect.x + 5.0f,
                headerRect.y + 3.0f,
                22.0f,
                20.0f
            );


        // ▼ = 開いている
        // ▶ = 閉じている
        if (GUI.Button(
            buttonRect,
            profilerPanelOpen
                ? "▼"
                : "▶"
        ))
        {
            // 開閉状態を反転
            profilerPanelOpen =
                !profilerPanelOpen;
        }


        // =====================================
        // タイトル
        // =====================================

        Rect titleRect =
            new Rect(
                headerRect.x + 32.0f,
                headerRect.y,
                headerRect.width - 37.0f,
                headerRect.height
            );


        // タイトル用Style
        GUIStyle titleStyle =
            new GUIStyle(
                EditorStyles.boldLabel
            );


        // 左中央揃え
        titleStyle.alignment =
            TextAnchor.MiddleLeft;


        // タイトル表示
        GUI.Label(
            titleRect,
            "Performance",
            titleStyle
        );


        // パネルが閉じられている場合は
        // ヘッダーだけ表示して終了
        if (!profilerPanelOpen)
        {
            return;
        }


        // =====================================
        // スクロール表示領域
        // =====================================

        Rect scrollRect =
            new Rect(
                panelRect.x + 5.0f,
                panelRect.y + headerHeight,
                panelRect.width - 10.0f,
                panelRect.height - headerHeight - 5.0f
            );


        // =====================================
        // スクロール内部のコンテンツ領域
        // =====================================

        // 表示領域より縦長にしておくことで
        // マウスホイールによるスクロールを可能にする
        Rect contentRect =
            new Rect(
                0.0f,
                0.0f,
                scrollRect.width - 20.0f,
                520.0f
            );


        // スクロール開始
        profilerScrollPosition =
            GUI.BeginScrollView(
                scrollRect,
                profilerScrollPosition,
                contentRect
            );


        // スクロール内部をGUILayoutで描画
        GUILayout.BeginArea(
            contentRect
        );


        // =====================================
        // CPU
        // =====================================

        DrawCPUPerformance();


        // 項目間の余白
        GUILayout.Space(
            8.0f
        );


        // =====================================
        // GPU
        // =====================================

        DrawGPUPerformance();


        // 項目間の余白
        GUILayout.Space(
            8.0f
        );


        // =====================================
        // Memory
        // =====================================

        DrawMemoryPerformance();


        // 項目間の余白
        GUILayout.Space(
            8.0f
        );


        // =====================================
        // Rendering
        // =====================================

        DrawRenderingPerformance();


        // GUILayout描画終了
        GUILayout.EndArea();


        // スクロール終了
        GUI.EndScrollView();
    }
}

#endif
