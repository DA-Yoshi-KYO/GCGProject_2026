/*
+=====================================
ファイル名 : CSE_processing_load.cs
概要       : 処理負荷確認用ビュー
作者       : ヨシモト リョウ
履歴       : 2026/09/11 新規作成
             2026/09/11 Pause中の表示固定処理を追加
=====================================+
*/

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public partial class CSE_processing_load : EditorWindow
{
    /// <summary>
    /// Processing Loadウィンドウを開く
    /// </summary>
    [MenuItem("Tools/Processing Load")]
    public static void ShowWindow()
    {
        GetWindow<CSE_processing_load>(
            "Processing Load"
        );
    }


    /// <summary>
    /// EditorWindowが有効になったときに呼ばれる
    /// </summary>
    private void OnEnable()
    {
        // PlayModeの状態変化を監視
        EditorApplication.playModeStateChanged +=
            OnPlayModeStateChanged;

        // すでにPlay中にWindowを開いた場合
        if (EditorApplication.isPlaying)
        {
            // Processing Load専用Cameraを作成
            CreateProcessingCamera();

            // Memory計測開始
            StartMemoryProfiler();

            // Rendering計測開始
            StartRenderingProfiler();
        }
    }


    /// <summary>
    /// PlayModeの状態が変化したときに呼ばれる
    /// </summary>
    private void OnPlayModeStateChanged(
        PlayModeStateChange state)
    {
        // ゲーム実行開始
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // Processing Load専用Cameraを作成
            CreateProcessingCamera();

            // Memory計測開始
            StartMemoryProfiler();

            // Rendering計測開始
            StartRenderingProfiler();

            // Windowを再描画
            Repaint();

            return;
        }

        // ゲーム実行終了直前
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // Memory計測終了
            StopMemoryProfiler();

            // Rendering計測終了
            StopRenderingProfiler();

            // Camera / RenderTextureを削除
            DestroyProcessingResources();

            // Windowを再描画
            Repaint();

            return;
        }
    }


    /// <summary>
    /// Processing Loadビューを描画する
    /// </summary>
    private void OnGUI()
    {
        // ウィンドウ内で使用可能な描画領域を取得
        Rect viewRect =
            GUILayoutUtility.GetRect(
                0,
                0,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true)
            );


        // =====================================
        // Play前
        // =====================================

        // ゲームが実行されていない場合
        if (!EditorApplication.isPlaying)
        {
            DrawCenterMessage(
                viewRect,
                "ゲームを実行してください。"
            );

            return;
        }


        // =====================================
        // Pause中
        // =====================================

        // Unity EditorのPauseボタンが押されている場合
        if (EditorApplication.isPaused)
        {
            // 最後に描画されたRenderTextureが存在する場合
            if (renderTexture != null)
            {
                // Pause直前のゲーム画面をそのまま表示
                EditorGUI.DrawPreviewTexture(
                    viewRect,
                    renderTexture,
                    null,
                    ScaleMode.ScaleToFit
                );

                // CPU / GPU / Memory / Renderingも
                // Pause直前の値をそのまま表示
                DrawPerformancePanel(
                    viewRect
                );
            }

            // Pause中はCamera描画と計測値の更新を行わない
            return;
        }


        // =====================================
        // 通常Play中
        // =====================================

        // GameViewで現在設定されている解像度を取得
        Vector2 gameViewSize =
            GetGameViewSize();


        // RenderTextureで使用する幅
        int width =
            Mathf.Max(
                1,
                (int)gameViewSize.x
            );


        // RenderTextureで使用する高さ
        int height =
            Mathf.Max(
                1,
                (int)gameViewSize.y
            );


        // Processing Load専用Cameraで描画
        RenderTexture texture =
            RenderProcessingCamera(
                width,
                height
            );


        // MainCameraが存在しないなど、
        // Processing Load用の描画ができなかった場合
        if (texture == null)
        {
            DrawCenterMessage(
                viewRect,
                "Main Camera がありません。"
            );

            return;
        }


        // Processing Load専用Cameraの映像を表示
        EditorGUI.DrawPreviewTexture(
            viewRect,
            texture,
            null,
            ScaleMode.ScaleToFit
        );


        // CPU / GPU / Memory / Rendering情報を表示
        DrawPerformancePanel(
            viewRect
        );


        // Play中は継続してWindowを更新
        Repaint();
    }


    /// <summary>
    /// GameViewで現在設定されている解像度を取得する
    /// </summary>
    /// <returns>
    /// GameViewの幅と高さ
    /// </returns>
    private Vector2 GetGameViewSize()
    {
        // UnityEditor内部に存在するGameView型を取得
        System.Type gameViewType =
            typeof(Editor).Assembly.GetType(
                "UnityEditor.GameView"
            );


        // GameView型が取得できなかった場合
        if (gameViewType == null)
        {
            return new Vector2(
                1920,
                1080
            );
        }


        // 現在Unity Editor内に存在しているGameViewを取得
        Object[] gameViews =
            Resources.FindObjectsOfTypeAll(
                gameViewType
            );


        // GameViewが見つからなかった場合
        if (gameViews == null ||
            gameViews.Length == 0)
        {
            return new Vector2(
                1920,
                1080
            );
        }


        // 最初に見つかったGameViewを使用
        object gameView =
            gameViews[0];


        // GameView内部のtargetSizeプロパティを取得
        System.Reflection.PropertyInfo property =
            gameViewType.GetProperty(
                "targetSize",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public
            );


        // targetSizeが取得できなかった場合
        if (property == null)
        {
            return new Vector2(
                1920,
                1080
            );
        }


        // GameViewの現在の解像度を返す
        return (Vector2)property.GetValue(
            gameView
        );
    }


    /// <summary>
    /// ウィンドウ中央にメッセージを表示する
    /// </summary>
    private void DrawCenterMessage(
        Rect viewRect,
        string message)
    {
        // 中央表示用Styleを作成
        GUIStyle style =
            new GUIStyle(
                EditorStyles.boldLabel
            );


        // 文字を中央揃え
        style.alignment =
            TextAnchor.MiddleCenter;


        // 文字サイズ
        style.fontSize =
            16;


        // メッセージ描画
        GUI.Label(
            viewRect,
            message,
            style
        );
    }


    /// <summary>
    /// 60FPSの1フレーム時間を100%として負荷率を計算する
    /// </summary>
    /// <param name="milliseconds">
    /// CPUまたはGPUのフレーム時間
    /// </param>
    /// <returns>
    /// 60FPS基準の負荷率
    /// </returns>
    private double CalculateFrameLoad(
        double milliseconds)
    {
        // 60FPSの場合、
        // 1フレームで使用できる時間は約16.67ms
        const double frameBudget =
            1000.0 / 60.0;


        // 使用時間 ÷ 16.67ms × 100
        return milliseconds /
               frameBudget *
               100.0;
    }


    /// <summary>
    /// EditorWindowが無効になったときに呼ばれる
    /// </summary>
    private void OnDisable()
    {
        // PlayMode状態監視を解除
        EditorApplication.playModeStateChanged -=
            OnPlayModeStateChanged;


        // Memory計測終了
        StopMemoryProfiler();


        // Rendering計測終了
        StopRenderingProfiler();


        // Camera / RenderTextureを削除
        DestroyProcessingResources();
    }
}

#endif
