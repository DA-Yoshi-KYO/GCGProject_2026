#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayFromBootstrap
{
    static PlayFromBootstrap()
    {
        // Bootstrapシーンを指定
        var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(
            "Assets/Programmer/Scenes/Bootstrap.unity");

        EditorSceneManager.playModeStartScene = scene;
    }
}
#endif
