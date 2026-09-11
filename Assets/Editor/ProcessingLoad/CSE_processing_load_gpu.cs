/*
+=====================================
ファイル名 : CSE_processing_load_gpu.cs
概要       : Processing Load用GPU情報取得・表示
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
    // GPUの1フレーム処理時間
    private double gpuFrameTime;

    // FrameTiming取得時に毎回配列を生成しないためのバッファ
    private readonly FrameTiming[] gpuFrameTimings =
        new FrameTiming[1];


    /// <summary>
    /// GPU情報を更新する
    /// </summary>
    private void UpdateGPUPerformance()
    {
        // Pause中は値を更新せず、
        // Pause直前のGPU時間を維持する
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
                gpuFrameTimings
            );


        // Timing情報を取得できなかった場合
        if (timingCount == 0)
        {
            gpuFrameTime =
                0.0;

            return;
        }


        // 最新フレームのTiming情報
        FrameTiming timing =
            gpuFrameTimings[0];


        // GPUフレーム時間を取得
        gpuFrameTime =
            timing.gpuFrameTime;
    }


    /// <summary>
    /// GPU情報をProcessing Load上へ表示する
    /// </summary>
    private void DrawGPUPerformance()
    {
        // GPU値を更新
        // Pause中の場合はUpdateGPUPerformance()側で更新されない
        UpdateGPUPerformance();


        // 項目タイトル
        EditorGUILayout.LabelField(
            "GPU",
            EditorStyles.boldLabel
        );


        // GPUフレーム時間
        EditorGUILayout.LabelField(
            $"GPU Frame : {gpuFrameTime:F2} ms"
        );


        // 60FPS基準のフレーム時間使用率
        EditorGUILayout.LabelField(
            $"Load (60 FPS) : {CalculateFrameLoad(gpuFrameTime):F1} %"
        );
    }
}

#endif
