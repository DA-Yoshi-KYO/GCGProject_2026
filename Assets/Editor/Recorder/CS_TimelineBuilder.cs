using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public static class CS_TimelineBuilder
{
    public static void Build(
        List<RecordedObject> records)
    {
        string folder =
            "Assets/RecordedTimeline";

        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder(
                "Assets",
                "RecordedTimeline");
        }

        TimelineAsset timeline =
            ScriptableObject.CreateInstance<TimelineAsset>();

        AssetDatabase.CreateAsset(
            timeline,
            folder + "/Recorded.playable");

        GameObject directorObj =
            new GameObject("RecordedDirector");

        PlayableDirector director =
            directorObj.AddComponent<PlayableDirector>();

        director.playableAsset = timeline;

        foreach (var record in records)
        {
            AnimationClip clip =
                CreateAnimation(record);

            AssetDatabase.CreateAsset(
                clip,
                $"{folder}/{record.target.name}.anim");

            AnimationTrack track =
                timeline.CreateTrack<AnimationTrack>(
                    null,
                    record.target.name);

            TimelineClip timelineClip =
                track.CreateDefaultClip();

            var asset =
                timelineClip.asset
                as AnimationPlayableAsset;

            asset.clip = clip;

            timelineClip.duration =
                clip.length;

            director.SetGenericBinding(
                track,
                record.target.gameObject);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static AnimationClip CreateAnimation(
    RecordedObject record)
    {
        AnimationClip clip =
            new AnimationClip();

        AnimationCurve posX = new();
        AnimationCurve posY = new();
        AnimationCurve posZ = new();

        AnimationCurve rotX = new();
        AnimationCurve rotY = new();
        AnimationCurve rotZ = new();
        AnimationCurve rotW = new();

        AnimationCurve scaleX = new();
        AnimationCurve scaleY = new();
        AnimationCurve scaleZ = new();

        foreach (var f in record.frames)
        {
            posX.AddKey(f.time, f.position.x);
            posY.AddKey(f.time, f.position.y);
            posZ.AddKey(f.time, f.position.z);

            rotX.AddKey(f.time, f.rotation.x);
            rotY.AddKey(f.time, f.rotation.y);
            rotZ.AddKey(f.time, f.rotation.z);
            rotW.AddKey(f.time, f.rotation.w);

            scaleX.AddKey(f.time, f.scale.x);
            scaleY.AddKey(f.time, f.scale.y);
            scaleZ.AddKey(f.time, f.scale.z);
        }

        clip.SetCurve(
            "",
            typeof(Transform),
            "m_LocalPosition.x",
            posX);

        clip.SetCurve(
            "",
            typeof(Transform),
            "m_LocalPosition.y",
            posY);

        clip.SetCurve(
            "",
            typeof(Transform),
            "m_LocalPosition.z",
            posZ);

        clip.SetCurve(
            "",
            typeof(Transform),
            "m_LocalRotation.x",
            rotX);

        clip.SetCurve(
            "",
            typeof(Transform),
            "m_LocalRotation.y",
            rotY);

        clip.SetCurve(
            "",
            typeof(Transform),
            "m_LocalRotation.z",
            rotZ);

        clip.SetCurve(
            "",
            typeof(Transform),
            "m_LocalRotation.w",
            rotW);

        clip.SetCurve(
            "",
            typeof(Transform),
            "m_LocalScale.x",
            scaleX);

        clip.SetCurve(
            "",
            typeof(Transform),
            "m_LocalScale.y",
            scaleY);

        clip.SetCurve(
            "",
            typeof(Transform),
            "m_LocalScale.z",
            scaleZ);

        return clip;
    }
}
