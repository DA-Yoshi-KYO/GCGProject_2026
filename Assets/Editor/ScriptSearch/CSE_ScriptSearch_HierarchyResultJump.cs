/*
+=====================================
 ファイル名 : CSE_ScriptSearch_HierarchyResultJump.cs
 概要     : ScriptSearchツールの「Hierarchy検索結果表示＆ジャンプ」
 作者     : ヨシモト リョウ
 履歴     : 2026/02/15 新規作成
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class CSE_ScriptSearch
{
    private Vector2 _hierarchyScroll;
    private int _hierarchySelectedIndex = -1;

    /// <summary>
    /// 検索結果（Hierarchy）の表示領域を描画する（ダブルクリックで飛べる）
    /// </summary>
    private void DrawHierarchyResultsView()
    {
        using (new GUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("Hierarchy 検索結果", EditorStyles.boldLabel);
            GUILayout.Space(4.0f);

            EditorGUILayout.HelpBox(_hierarchyResultMessage, MessageType.Info);
            GUILayout.Space(6.0f);

            if (_hierarchyHitPaths.Count <= 0)
            {
                EditorGUILayout.LabelField("ヒットなし", EditorStyles.miniBoldLabel);
                return;
            }

            _hierarchyScroll = EditorGUILayout.BeginScrollView(_hierarchyScroll, GUILayout.Height(300.0f));

            for (int i = 0; i < _hierarchyHitPaths.Count; i++)
            {
                string path = _hierarchyHitPaths[i];

                // 行Rectを取り、そこでクリック/ダブルクリックを検出する
                Rect rowRect = GUILayoutUtility.GetRect(
                    new GUIContent(path),
                    (i == _hierarchySelectedIndex) ? EditorStyles.boldLabel : EditorStyles.label,
                    GUILayout.ExpandWidth(true)
                );

                // テキスト描画
                EditorGUI.LabelField(rowRect, path, (i == _hierarchySelectedIndex) ? EditorStyles.boldLabel : EditorStyles.label);

                // 入力処理
                Event e = Event.current;
                if (e.type == EventType.MouseDown && rowRect.Contains(e.mousePosition))
                {
                    _hierarchySelectedIndex = i;

                    if (e.clickCount == 2)
                    {
                        JumpToHierarchyObject(path);
                    }

                    e.Use();
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }

    /// <summary>
    /// ダブルクリックで、対象Sceneへ移動して対象Objectを選択する
    /// </summary>
    private void JumpToHierarchyObject(string hierarchyPath)
    {
        if (string.IsNullOrEmpty(_hierarchyScenePath))
        {
            EditorUtility.DisplayDialog("ScriptSearch", "Scene情報がないよ。先に検索してね。", "OK");
            return;
        }

        // Sceneが開いていなければAdditiveで開く（既存作業を壊しにくい）
        Scene targetScene = FindLoadedSceneByPath(_hierarchyScenePath);
        if (!targetScene.IsValid())
        {
            targetScene = EditorSceneManager.OpenScene(_hierarchyScenePath, OpenSceneMode.Additive);
        }

        if (!targetScene.IsValid())
        {
            EditorUtility.DisplayDialog("ScriptSearch", "Sceneを開けなかったよ。", "OK");
            return;
        }

        SceneManager.SetActiveScene(targetScene);

        // HierarchyパスからGameObjectを探す
        GameObject go = FindGameObjectByHierarchyPath(targetScene, hierarchyPath);
        if (go == null)
        {
            EditorUtility.DisplayDialog("ScriptSearch", $"対象Objectが見つからなかったよ:\n{hierarchyPath}", "OK");
            return;
        }

        // 選択＆Ping（Hierarchy上でハイライトされる）
        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);

        // SceneViewがあるならフレーム
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
        }
    }

    /// <summary>
    /// ロード済みSceneからパス一致で探す
    /// </summary>
    private static Scene FindLoadedSceneByPath(string scenePath)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (!string.IsNullOrEmpty(s.path) && s.path == scenePath)
            {
                return s;
            }
        }
        return default;
    }

    /// <summary>
    /// Scene内のRootからHierarchyパスでGameObjectを探索する
    /// </summary>
    private static GameObject FindGameObjectByHierarchyPath(Scene scene, string hierarchyPath)
    {
        if (!scene.IsValid() || string.IsNullOrEmpty(hierarchyPath)) return null;

        string[] parts = hierarchyPath.Split('/');
        if (parts.Length <= 0) return null;

        // Root検索
        GameObject[] roots = scene.GetRootGameObjects();
        GameObject current = null;

        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] != null && roots[i].name == parts[0])
            {
                current = roots[i];
                break;
            }
        }
        if (current == null) return null;

        // 子を辿る
        for (int p = 1; p < parts.Length; p++)
        {
            string name = parts[p];
            Transform found = null;

            Transform parent = current.transform;
            for (int c = 0; c < parent.childCount; c++)
            {
                Transform child = parent.GetChild(c);
                if (child != null && child.name == name)
                {
                    found = child;
                    break;
                }
            }

            if (found == null) return null;
            current = found.gameObject;
        }

        return current;
    }
}
#endif
