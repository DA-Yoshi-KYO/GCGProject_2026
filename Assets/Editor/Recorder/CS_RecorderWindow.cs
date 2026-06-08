using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class FrameData
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

[System.Serializable]
public class RecordedObject
{
    public Transform target;
    public List<FrameData> frames = new();
}

public class TransformRecorderWindow : EditorWindow
{
    const float SAMPLE_INTERVAL = 0.05f;

    List<RecordedObject> records = new();

    bool recording;

    double startTime;
    float nextSample;

    [MenuItem("Tools/Timeline Recorder")]
    static void Open()
    {
        GetWindow<TransformRecorderWindow>();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Add Selected"))
        {
            foreach (var go in Selection.transforms)
            {
                records.Add(new RecordedObject
                {
                    target = go
                });
            }
        }

        if (!recording)
        {
            if (GUILayout.Button("Start Record"))
            {
                StartRecord();
            }
        }
        else
        {
            if (GUILayout.Button("Stop Record"))
            {
                StopRecord();
            }
        }

        GUILayout.Space(10);

        foreach (var r in records)
        {
            EditorGUILayout.ObjectField(
                r.target,
                typeof(Transform),
                true);
        }
    }

    void Update()
    {
        if (!recording)
            return;

        float t =
            (float)(EditorApplication.timeSinceStartup - startTime);

        if (t < nextSample)
            return;


        nextSample += SAMPLE_INTERVAL;

        foreach (var r in records)
        {
            if (r.target == null)
                continue;
            Debug.Log(r.target.eulerAngles);
            Debug.Log(r.target.localEulerAngles);

            r.frames.Add(new FrameData
            {
                time = t,
                position = r.target.localPosition,
                rotation = r.target.localRotation,
                scale = r.target.localScale
            });
        }
    }

    void StartRecord()
    {
        foreach (var r in records)
            r.frames.Clear();

        startTime = EditorApplication.timeSinceStartup;
        nextSample = 0;

        recording = true;
    }

    void StopRecord()
    {
        recording = false;

        CS_TimelineBuilder.Build(records);
    }
}
