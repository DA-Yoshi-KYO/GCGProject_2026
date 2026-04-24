/*
+=====================================
 ファイル名 : CSE_ScriptSearch_AssetsResultJump.cs
 概要     : ScriptSearchツールの「Assets検索結果表示＆ジャンプ」
 作者     : ヨシモト リョウ
 履歴     : 2026/02/15 新規作成
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public partial class CSE_ScriptSearch
{
    private Vector2 _assetsScroll;
    private int _assetsSelectedIndex = -1;

    /// <summary>
    /// Assets検索結果ビューを描画する（ダブルクリックで飛べる）
    /// </summary>
    private void DrawAssetsResultsView()
    {
        using (new GUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("Assets 検索結果（Prefab）", EditorStyles.boldLabel);
            GUILayout.Space(4.0f);

            EditorGUILayout.HelpBox(_assetsResultMessage, MessageType.Info);
            GUILayout.Space(6.0f);

            if (_assetsHitPrefabPaths.Count <= 0)
            {
                EditorGUILayout.LabelField("ヒットなし", EditorStyles.miniBoldLabel);
                return;
            }

            _assetsScroll = EditorGUILayout.BeginScrollView(_assetsScroll, GUILayout.Height(300.0f));

            for (int i = 0; i < _assetsHitPrefabPaths.Count; i++)
            {
                string path = _assetsHitPrefabPaths[i];

                Rect rowRect = GUILayoutUtility.GetRect(
                    new GUIContent(path),
                    (i == _assetsSelectedIndex) ? EditorStyles.boldLabel : EditorStyles.label,
                    GUILayout.ExpandWidth(true)
                );

                EditorGUI.LabelField(rowRect, path, (i == _assetsSelectedIndex) ? EditorStyles.boldLabel : EditorStyles.label);

                Event e = Event.current;
                if (e.type == EventType.MouseDown && rowRect.Contains(e.mousePosition))
                {
                    _assetsSelectedIndex = i;

                    if (e.clickCount == 2)
                    {
                        JumpToAsset(path);
                    }

                    e.Use();
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }

    /// <summary>
    /// Prefabアセットへジャンプ（Project選択＆Ping＆Open）
    /// </summary>
    private static void JumpToAsset(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return;

        Object obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
        if (obj == null) return;

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = obj;
        EditorGUIUtility.PingObject(obj);

        // PrefabならPrefabモードで開ける（環境により挙動はUnity任せ）
        AssetDatabase.OpenAsset(obj);
    }
}
#endif
