/*
+=====================================
ファイル名 : CSE_processing_load_memory.cs
概要       : Processing Load用Memory情報取得・表示
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
    // OSから見たUnityの使用メモリ
    private ProfilerRecorder systemUsedMemoryRecorder;

    // Unityが現在使用しているメモリ
    private ProfilerRecorder totalUsedMemoryRecorder;

    // Unityが予約しているメモリ
    private ProfilerRecorder totalReservedMemoryRecorder;

    // Managed Heapが使用しているメモリ
    private ProfilerRecorder gcUsedMemoryRecorder;

    // Managed Heapが予約しているメモリ
    private ProfilerRecorder gcReservedMemoryRecorder;


    // =====================================
    // Pause時に固定するためのキャッシュ値
    // =====================================

    // System使用メモリ
    private long cachedSystemUsedMemory;

    // Total使用メモリ
    private long cachedTotalUsedMemory;

    // Total予約メモリ
    private long cachedTotalReservedMemory;

    // GC使用メモリ
    private long cachedGCUsedMemory;

    // GC予約メモリ
    private long cachedGCReservedMemory;


    /// <summary>
    /// Memory情報取得を開始する
    /// </summary>
    private void StartMemoryProfiler()
    {
        // 二重起動を防ぐため、
        // すでに存在するRecorderを先に終了
        StopMemoryProfiler();


        // OSから見たUnity使用メモリ
        systemUsedMemoryRecorder =
            ProfilerRecorder.StartNew(
                ProfilerCategory.Memory,
                "System Used Memory"
            );


        // Unityが使用中のメモリ
        totalUsedMemoryRecorder =
            ProfilerRecorder.StartNew(
                ProfilerCategory.Memory,
                "Total Used Memory"
            );


        // Unityが予約しているメモリ
        totalReservedMemoryRecorder =
            ProfilerRecorder.StartNew(
                ProfilerCategory.Memory,
                "Total Reserved Memory"
            );


        // GCが使用しているメモリ
        gcUsedMemoryRecorder =
            ProfilerRecorder.StartNew(
                ProfilerCategory.Memory,
                "GC Used Memory"
            );


        // GCが予約しているメモリ
        gcReservedMemoryRecorder =
            ProfilerRecorder.StartNew(
                ProfilerCategory.Memory,
                "GC Reserved Memory"
            );
    }


    /// <summary>
    /// Memory情報取得を終了する
    /// </summary>
    private void StopMemoryProfiler()
    {
        DisposeMemoryRecorder(
            ref systemUsedMemoryRecorder
        );


        DisposeMemoryRecorder(
            ref totalUsedMemoryRecorder
        );


        DisposeMemoryRecorder(
            ref totalReservedMemoryRecorder
        );


        DisposeMemoryRecorder(
            ref gcUsedMemoryRecorder
        );


        DisposeMemoryRecorder(
            ref gcReservedMemoryRecorder
        );
    }


    /// <summary>
    /// Memory用ProfilerRecorderを安全に破棄する
    /// </summary>
    private void DisposeMemoryRecorder(
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
    /// Memoryの最新値をキャッシュする
    /// </summary>
    private void UpdateMemoryPerformance()
    {
        // Pause中は更新しない
        // これによりPause直前の値を維持できる
        if (EditorApplication.isPaused)
        {
            return;
        }


        // 各Recorderが有効な場合のみ最新値を保存
        if (systemUsedMemoryRecorder.Valid)
        {
            cachedSystemUsedMemory =
                systemUsedMemoryRecorder.LastValue;
        }


        if (totalUsedMemoryRecorder.Valid)
        {
            cachedTotalUsedMemory =
                totalUsedMemoryRecorder.LastValue;
        }


        if (totalReservedMemoryRecorder.Valid)
        {
            cachedTotalReservedMemory =
                totalReservedMemoryRecorder.LastValue;
        }


        if (gcUsedMemoryRecorder.Valid)
        {
            cachedGCUsedMemory =
                gcUsedMemoryRecorder.LastValue;
        }


        if (gcReservedMemoryRecorder.Valid)
        {
            cachedGCReservedMemory =
                gcReservedMemoryRecorder.LastValue;
        }
    }


    /// <summary>
    /// Memory情報をProcessing Load上へ表示する
    /// </summary>
    private void DrawMemoryPerformance()
    {
        // Memory値を更新
        UpdateMemoryPerformance();


        // 項目タイトル
        EditorGUILayout.LabelField(
            "Memory",
            EditorStyles.boldLabel
        );


        // System使用メモリ
        DrawMemoryValue(
            "System Used",
            systemUsedMemoryRecorder,
            cachedSystemUsedMemory
        );


        // Unity使用メモリ
        DrawMemoryValue(
            "Total Used",
            totalUsedMemoryRecorder,
            cachedTotalUsedMemory
        );


        // Unity予約メモリ
        DrawMemoryValue(
            "Total Reserved",
            totalReservedMemoryRecorder,
            cachedTotalReservedMemory
        );


        // GC使用メモリ
        DrawMemoryValue(
            "GC Used",
            gcUsedMemoryRecorder,
            cachedGCUsedMemory
        );


        // GC予約メモリ
        DrawMemoryValue(
            "GC Reserved",
            gcReservedMemoryRecorder,
            cachedGCReservedMemory
        );
    }


    /// <summary>
    /// Memory値をMB単位で表示する
    /// </summary>
    private void DrawMemoryValue(
        string label,
        ProfilerRecorder recorder,
        long cachedValue)
    {
        // Recorder自体が使用できない場合
        if (!recorder.Valid)
        {
            EditorGUILayout.LabelField(
                $"{label} : N/A"
            );

            return;
        }


        // ByteからMBへ変換
        double megaBytes =
            cachedValue /
            (1024.0 * 1024.0);


        // Memory値を表示
        EditorGUILayout.LabelField(
            $"{label} : {megaBytes:F2} MB"
        );
    }
}

#endif
