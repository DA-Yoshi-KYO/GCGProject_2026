/*
+=====================================
 ファイル名 : CSE_ScriptSearch_HierarchySearch.cs
 概要     : ScriptSearchツールのHierarchy検索（検索ロジック担当）
 作者     : ヨシモト リョウ
 履歴     : 2026/02/15 新規作成
=====================================+
*/

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class CSE_ScriptSearch
{
    // ----------------------------
    // Hierarchy検索結果データ
    // ----------------------------
    private readonly List<string> _hierarchyHitPaths = new List<string>(256);
    private string _hierarchyResultMessage = "まだ検索していません。";

    // 直近検索したSceneのパス（ダブルクリックジャンプで使う）
    private string _hierarchyScenePath = string.Empty;

    /// <summary>
    /// Hierarchy結果メッセージをセット
    /// </summary>
    private void SetHierarchyResultMessage(string message)
    {
        _hierarchyResultMessage = string.IsNullOrEmpty(message) ? "" : message;
        Repaint();
    }

    /// <summary>
    /// Hierarchy検索を実行する（Scene内の全GameObjectから、指定Scriptが付いたものを抽出）
    /// </summary>
    private void ExecuteHierarchySearch()
    {
        _hierarchyHitPaths.Clear();
        _hierarchyScenePath = string.Empty;

        // 入力チェック
        if (_targetSceneAsset == null)
        {
            SetHierarchyResultMessage("検索対象Sceneが未設定だよ。");
            return;
        }

        if (_targetScript == null)
        {
            SetHierarchyResultMessage("検索対象Scriptが未設定だよ。");
            return;
        }

        Type scriptType = _targetScript.GetClass();
        if (scriptType == null)
        {
            SetHierarchyResultMessage("Scriptからクラス型が取れなかったよ（コンパイルエラーや、クラスじゃない可能性）。");
            return;
        }

        if (!typeof(Component).IsAssignableFrom(scriptType))
        {
            SetHierarchyResultMessage("このScriptはComponentじゃないみたい（MonoBehaviour系のみ検索対象だよ）。");
            return;
        }

        string scenePath = AssetDatabase.GetAssetPath(_targetSceneAsset);
        if (string.IsNullOrEmpty(scenePath))
        {
            SetHierarchyResultMessage("Sceneのパス取得に失敗したよ。");
            return;
        }

        _hierarchyScenePath = scenePath;

        // Sceneがすでに開いてるならそれを使う。開いてなければAdditiveで開いて検索→閉じる。
        Scene targetScene = default;
        bool openedAdditive = false;

        Scene activeBefore = SceneManager.GetActiveScene();

        // 既にロード済みか確認
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (!string.IsNullOrEmpty(s.path) && s.path == scenePath)
            {
                targetScene = s;
                openedAdditive = false;
                break;
            }
        }

        try
        {
            if (!targetScene.IsValid())
            {
                targetScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                openedAdditive = true;
            }

            // 探索
            GameObject[] roots = targetScene.GetRootGameObjects();
            HashSet<string> unique = new HashSet<string>(StringComparer.Ordinal);

            for (int r = 0; r < roots.Length; r++)
            {
                Component[] comps = roots[r].GetComponentsInChildren(scriptType, true);
                for (int c = 0; c < comps.Length; c++)
                {
                    if (comps[c] == null) continue;

                    string path = BuildHierarchyPath(comps[c].gameObject);
                    if (unique.Add(path))
                    {
                        _hierarchyHitPaths.Add(path);
                    }
                }
            }

            _hierarchyHitPaths.Sort(StringComparer.OrdinalIgnoreCase);

            SetHierarchyResultMessage(
                $"Scene: {System.IO.Path.GetFileNameWithoutExtension(scenePath)} / Script: {scriptType.Name}\n" +
                $"Hit: {_hierarchyHitPaths.Count}\n" +
                $"（ダブルクリックでジャンプ）"
            );
        }
        catch (Exception e)
        {
            SetHierarchyResultMessage($"検索中に例外が出たよ:\n{e.Message}");
        }
        finally
        {
            // Additiveで開いた場合は閉じる（作業中のシーンは汚さない）
            if (openedAdditive && targetScene.IsValid())
            {
                EditorSceneManager.CloseScene(targetScene, true);
                if (activeBefore.IsValid()) EditorSceneManager.SetActiveScene(activeBefore);
            }
        }

        Repaint();
    }

    /// <summary>
    /// GameObjectのHierarchyパスを作る（例：Root/Child/Obj）
    /// </summary>
    private static string BuildHierarchyPath(GameObject go)
    {
        if (go == null) return "";

        string path = go.name;
        Transform t = go.transform.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }
}
#endif
