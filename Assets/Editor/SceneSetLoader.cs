/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    実行時のロードを行うスクリプト
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    吉田 京志郎
 * ----------------------------------------------------------
 * 2026-04-20 | 初回作成
 * 
 */
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneSetLoader
{
    // インゲーム用のシーンセットを開くメニューアイテム
    [MenuItem("Scenes/Open InGame Set")]
    public static void OpenInGameSet()
    {
        // InGame用のフォルダからシーンを検索
        var guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Programmer/Scenes/InGame" });

        List<string> paths = new List<string>();    // シーンのパスを格納するリスト
        for (int i = 0; i < guids.Length; i++)
        {
            paths.Add(AssetDatabase.GUIDToAssetPath(guids[i])); // GUIDからシーンのパスを取得してリストに追加

            // メインシーンを先に開く
            if (paths[i].Contains("MainScene")) EditorSceneManager.OpenScene(paths[i]);
        }

        // メインシーン以外のシーンをアドティブモードで開く
        foreach (var path in paths)
        {
            if (path.Contains("MainScene")) continue; // メインシーンはすでに開いているのでスキップ
            else EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        }
    }

    // アウトゲーム用のシーンセットを開くメニューアイテム
    [MenuItem("Scenes/Open OutGame Set")]
    public static void OpenGameSet()
    {
        // OutGame用のフォルダからシーンを検索
        var guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Programmer/Scenes/OutGame" });
        if (guids.Length == 0)
        {
            Debug.LogError("No scenes found in the specified folder.");
            return;
        }

        List<string> paths = new List<string>();    // シーンのパスを格納するリスト
        for (int i = 0; i < guids.Length ; i++)
        {
            paths.Add(AssetDatabase.GUIDToAssetPath(guids[i])); // GUIDからシーンのパスを取得してリストに追加

            // メインシーンを先に開く
            if (paths[i].Contains("MainScene")) EditorSceneManager.OpenScene(paths[i]);
        }

        // メインシーン以外のシーンをアドティブモードで開く
        foreach (var path in paths)
        {
            if (path.Contains("MainScene")) continue; // メインシーンはすでに開いているのでスキップ
            else EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        }
    }
}
#endif
