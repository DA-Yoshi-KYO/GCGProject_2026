/*
+=====================================
ファイル名 : CSE_processing_load_rendering.cs
概要       : Processing Load用Rendering情報取得・表示
作者       : ヨシモト リョウ
履歴       : 2026/09/11 新規作成
             2026/09/11 Pause中の値固定処理を追加
=====================================+
*/

#if UNITY_EDITOR

using Unity.Profiling;
using UnityEditor;

public partial class CSE_processing_load
{
    // 1フレーム内のDraw Call数
    private ProfilerRecorder drawCallsRecorder;

    // 1フレーム内のSetPass Call数
    private ProfilerRecorder setPassCallsRecorder;

    // 1フレーム内のBatch数
    private ProfilerRecorder batchesRecorder;

    // 1フレーム内で描画されたTriangle数
    private ProfilerRecorder trianglesRecorder;

    // 1フレーム内で描画されたVertex数
    private ProfilerRecorder verticesRecorder;


    // =====================================
    // Pause時に固定するためのキャッシュ値
    // =====================================

    // Draw Call数
    private long cachedDrawCalls;

    // SetPass Call数
    private long cachedSetPassCalls;

    // Batch数
    private long cachedBatches;

    // Triangle数
    private long cachedTriangles;

    // Vertex数
    private long cachedVertices;


    /// <summary>
    /// Rendering情報の取得を開始する
    /// </summary>
    private void StartRenderingProfiler()
    {
        // 二重起動を防ぐため、
        // すでに存在するRecorderを先に終了
        StopRenderingProfiler();


        // Draw Call数
        drawCallsRecorder =
            ProfilerRecorder.StartNew(
                ProfilerCategory.Render,
                "Draw Calls Count"
            );


        // SetPass Call数
        setPassCallsRecorder =
            ProfilerRecorder.StartNew(
                ProfilerCategory.Render,
                "SetPass Calls Count"
            );


        // Batch数
        batchesRecorder =
            ProfilerRecorder.StartNew(
                ProfilerCategory.Render,
                "Batches Count"
            );


        // Triangle数
        trianglesRecorder =
            ProfilerRecorder.StartNew(
                ProfilerCategory.Render,
                "Triangles Count"
            );


        // Vertex数
        verticesRecorder =
            ProfilerRecorder.StartNew(
                ProfilerCategory.Render,
                "Vertices Count"
            );
    }


    /// <summary>
    /// Rendering情報の取得を終了する
    /// </summary>
    private void StopRenderingProfiler()
    {
        DisposeRenderingRecorder(
            ref drawCallsRecorder
        );


        DisposeRenderingRecorder(
            ref setPassCallsRecorder
        );


        DisposeRenderingRecorder(
            ref batchesRecorder
        );


        DisposeRenderingRecorder(
            ref trianglesRecorder
        );


        DisposeRenderingRecorder(
            ref verticesRecorder
        );
    }


    /// <summary>
    /// Rendering用ProfilerRecorderを安全に破棄する
    /// </summary>
    private void DisposeRenderingRecorder(
        ref ProfilerRecorder recorder)
    {
        // 有効なRecorderの場合のみ破棄
        if (recorder.Valid)
        {
            recorder.Dispose();
        }


        // 参照を初期状態へ戻す
        recorder =
            default;
    }


    /// <summary>
    /// Renderingの最新値をキャッシュする
    /// </summary>
    private void UpdateRenderingPerformance()
    {
        // Pause中は更新しない
        // Pause直前のRendering値をそのまま維持する
        if (EditorApplication.isPaused)
        {
            return;
        }


        // 各Recorderが有効な場合のみ最新値を保存
        if (drawCallsRecorder.Valid)
        {
            cachedDrawCalls =
                drawCallsRecorder.LastValue;
        }


        if (setPassCallsRecorder.Valid)
        {
            cachedSetPassCalls =
                setPassCallsRecorder.LastValue;
        }


        if (batchesRecorder.Valid)
        {
            cachedBatches =
                batchesRecorder.LastValue;
        }


        if (trianglesRecorder.Valid)
        {
            cachedTriangles =
                trianglesRecorder.LastValue;
        }


        if (verticesRecorder.Valid)
        {
            cachedVertices =
                verticesRecorder.LastValue;
        }
    }


    /// <summary>
    /// Rendering情報をProcessing Load上へ表示する
    /// </summary>
    private void DrawRenderingPerformance()
    {
        // Rendering値を更新
        UpdateRenderingPerformance();


        // 項目タイトル
        EditorGUILayout.LabelField(
            "Rendering",
            EditorStyles.boldLabel
        );


        // Draw Call数
        DrawRenderingValue(
            "Draw Calls",
            drawCallsRecorder,
            cachedDrawCalls
        );


        // SetPass Call数
        DrawRenderingValue(
            "SetPass Calls",
            setPassCallsRecorder,
            cachedSetPassCalls
        );


        // Batch数
        DrawRenderingValue(
            "Batches",
            batchesRecorder,
            cachedBatches
        );


        // Triangle数
        DrawRenderingValue(
            "Triangles",
            trianglesRecorder,
            cachedTriangles
        );


        // Vertex数
        DrawRenderingValue(
            "Vertices",
            verticesRecorder,
            cachedVertices
        );
    }


    /// <summary>
    /// Renderingカウンターの値を表示する
    /// </summary>
    private void DrawRenderingValue(
        string label,
        ProfilerRecorder recorder,
        long cachedValue)
    {
        // Recorderが使用できない場合
        if (!recorder.Valid)
        {
            EditorGUILayout.LabelField(
                $"{label} : N/A"
            );

            return;
        }


        // キャッシュしている値を表示
        EditorGUILayout.LabelField(
            $"{label} : {cachedValue:N0}"
        );
    }
}

#endif
