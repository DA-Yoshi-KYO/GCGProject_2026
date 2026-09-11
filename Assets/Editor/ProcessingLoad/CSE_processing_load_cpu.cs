/*
+=====================================
ファイル名 : CSE_processing_load_cpu.cs
概要       : Processing Load用CPU情報取得・表示
作者       : ヨシモト リョウ
履歴       : 2026/09/11 新規作成
             2026/09/11 Pause中の値固定処理を追加
=====================================+
*/

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public partial class CSE_processing_load
{
    // CPU全体の1フレーム処理時間
    private double cpuTotalFrameTime;

    // Main Threadの1フレーム処理時間
    private double cpuMainThreadTime;

    // Render Threadの1フレーム処理時間
    private double cpuRenderThreadTime;

    // FrameTiming取得時に毎回配列を生成しないためのバッファ
    private readonly FrameTiming[] cpuFrameTimings =
        new FrameTiming[1];


    /// <summary>
    /// CPU情報を更新する
    /// </summary>
    private void UpdateCPUPerformance()
    {
        // Pause中は値を更新せず、
        // Pause直前に取得した値をそのまま維持する
        if (EditorApplication.isPaused)
        {
            return;
        }


        // 現在フレームのFrameTiming取得を要求
        FrameTimingManager.CaptureFrameTimings();


        // 最新1フレーム分のTiming情報を取得
        uint timingCount =
            FrameTimingManager.GetLatestTimings(
                1,
                cpuFrameTimings
            );


        // Timing情報を取得できなかった場合
        if (timingCount == 0)
        {
            cpuTotalFrameTime =
                0.0;

            cpuMainThreadTime =
                0.0;

            cpuRenderThreadTime =
                0.0;

            return;
        }


        // 最新フレームのTiming情報
        FrameTiming timing =
            cpuFrameTimings[0];


        // CPU全体のフレーム時間
        cpuTotalFrameTime =
            timing.cpuFrameTime;


        // Main Threadのフレーム時間
        cpuMainThreadTime =
            timing.cpuMainThreadFrameTime;


        // Render Threadのフレーム時間
        cpuRenderThreadTime =
            timing.cpuRenderThreadFrameTime;
    }


    /// <summary>
    /// CPU情報をProcessing Load上へ表示する
    /// </summary>
    private void DrawCPUPerformance()
    {
        // CPU値を更新
        // Pause中の場合はUpdateCPUPerformance()側で更新されない
        UpdateCPUPerformance();


        // 項目タイトル
        EditorGUILayout.LabelField(
            "CPU",
            EditorStyles.boldLabel
        );


        // CPU全体のフレーム時間
        EditorGUILayout.LabelField(
            $"Total Frame : {cpuTotalFrameTime:F2} ms"
        );


        // Main Threadのフレーム時間
        EditorGUILayout.LabelField(
            $"Main Thread : {cpuMainThreadTime:F2} ms"
        );


        // Render Threadのフレーム時間
        EditorGUILayout.LabelField(
            $"Render Thread : {cpuRenderThreadTime:F2} ms"
        );


        // 60FPS基準のフレーム時間使用率
        EditorGUILayout.LabelField(
            $"Load (60 FPS) : {CalculateFrameLoad(cpuTotalFrameTime):F1} %"
        );
    }
}

#endif
