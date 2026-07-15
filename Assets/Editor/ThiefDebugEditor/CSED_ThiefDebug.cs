/*＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒デバッグツール本体
 *＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 *　2026-05-19 | 初回作成
 *　2026-05-22 | ファイル名を変更（ThiefDebug.cs → CSED_ThiefDebug.cs）
 *　           | クラス名を変更（ThiefDebug → CSED_ThiefDebug）
 *　2026-07-12 | 泥棒デバックエディタのエラーログ解消のための大改修
 *　           | 対象の泥棒リストを親エディタウィンドウで管理するように変更
 *　
 */
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 泥棒に関するデバッグ機能をタブでまとめたウィンドウ。
/// </summary>
public sealed class CSED_ThiefDebug : EditorWindow
{
    /// <summary>
    /// 表示するタブ種別
    /// </summary>
    private enum CSE_Tab
    {
        /// <summary>ダメージデバッグ</summary>
        Damage,
        /// <summary>部屋記憶（参照）</summary>
        RoomMemoryView,
        /// <summary>部屋記憶（追加）</summary>
        RoomMemoryAdd,
        /// <summary>機能フラグ</summary>
        Flags,
    }

    // 現在表示しているタブ
    private CSE_Tab currentTab = CSE_Tab.Damage;

    // タブ内容用のスクロール位置
    private Vector2 scroll;

    // 各タブの描画クラス
    private CSED_ThiefDebugDamageTab damageTab;
    private CSED_ThiefDebugRoomMemoryTab roomMemoryTab;
    private CSED_ThiefDebugRoomMemoryAddTab roomMemoryAddTab;
    private CSED_ThiefDebugFlagTab flagTab;

    // ターゲット管理
    [Tooltip("対象（泥棒AIコンポーネント）リスト")]
    private readonly List<MonoBehaviour> targets = new List<MonoBehaviour>();

    [Tooltip("targets内の選択中index（-1は未選択）")]
    private int selectedTargetIndex = -1;

    /// <summary>
    /// Unityメニューからデバッグウィンドウを開く
    /// </summary>
    [MenuItem("Tools/Thief Debug")]
    public static void Open()
    {
        // 既存があれば再利用、無ければ生成して表示
        GetWindow<CSED_ThiefDebug>("ThiefDebug");
    }

    /// <summary>
    /// Script Reload / Window復帰時の初期化
    /// </summary>
    private void OnEnable()
    {
        // Script Reload後も保持される必要があるものは、ここで生成
        if (damageTab == null) damageTab = new CSED_ThiefDebugDamageTab();
        if (roomMemoryTab == null) roomMemoryTab = new CSED_ThiefDebugRoomMemoryTab();
        if (roomMemoryAddTab == null) roomMemoryAddTab = new CSED_ThiefDebugRoomMemoryAddTab();
        if (flagTab == null) flagTab = new CSED_ThiefDebugFlagTab();
    }

    /// <summary>
    /// ウィンドウ描画（毎フレーム呼ばれる）
    /// </summary>
    private void OnGUI()
    {
        // アプリケーションが再生中でない場合は警告を表示
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("実行中のみ有効です。", MessageType.Warning);
            return;
        }

        // ターゲット管理UI
        DrawTargetManagementArea();
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(4);

        // タブ切り替えUI
        DrawToolbar();

        // タブ内容領域（内容が増えた場合に備えてスクロール対応）
        using (var sv = new EditorGUILayout.ScrollViewScope(scroll))
        {
            scroll = sv.scrollPosition;

            // 現在のタブに応じて描画処理を委譲
            switch (currentTab)
            {
                case CSE_Tab.Damage:
                    damageTab.Draw(targets, selectedTargetIndex);
                    break;
                case CSE_Tab.RoomMemoryView:
                    roomMemoryTab.Draw(targets, selectedTargetIndex);
                    break;
                case CSE_Tab.RoomMemoryAdd:
                    roomMemoryAddTab.Draw(targets, selectedTargetIndex);
                    break;
                case CSE_Tab.Flags:
                    flagTab.Draw();
                    break;
            }
        }
    }

    /// <summary>
    /// タブ選択用のツールバー（上部）
    /// </summary>
    private void DrawToolbar()
    {
        EditorGUILayout.Space(4);
        currentTab = (CSE_Tab)GUILayout.Toolbar((int)currentTab, new[] { "ダメージ", "記憶参照", "記憶追加", "フラグ" });
        EditorGUILayout.Space(8);
    }

    /// <summary>
    /// ターゲット管理エリアの描画
    /// </summary>
    private void DrawTargetManagementArea()
    {
        EditorGUILayout.LabelField("対象の管理", EditorStyles.boldLabel);

        // 一括操作（シーン収集/クリア）
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("シーンから泥棒AIを収集"))
            {
                var thiefAiType = FindTypeByName("CS_ThiefAI");
                if (thiefAiType != null)
                {
                    var found = UnityEngine.Object.FindObjectsOfType(thiefAiType);
                    foreach (var o in found)
                    {
                        var mb = o as MonoBehaviour;
                        if (mb == null) continue;
                        if (!targets.Contains(mb)) targets.Add(mb);
                    }

                    if (targets.Count > 0 && selectedTargetIndex < 0)
                    {
                        selectedTargetIndex = 0;
                    }
                }
            }

            using (new EditorGUI.DisabledScope(!HasValidSelectedTarget()))
            {
                if (GUILayout.Button("選択中を削除", GUILayout.Width(100)))
                {
                    targets.RemoveAt(selectedTargetIndex);
                    // インデックスをクランプして範囲内に収める
                    selectedTargetIndex = Mathf.Clamp(selectedTargetIndex, 0, targets.Count - 1);
                    // リストが空になった場合は未選択にする
                    if (targets.Count == 0)
                    {
                        selectedTargetIndex = -1;
                    }
                }
            }

            using (new EditorGUI.DisabledScope(targets.Count == 0))
            {
                if (GUILayout.Button("リストをクリア", GUILayout.Width(100)))
                {
                    targets.Clear();
                    selectedTargetIndex = -1;
                }
            }
        }

        // 参照切れを掃除
        CleanupNullTargets();

        if (targets.Count == 0)
        {
            EditorGUILayout.HelpBox("対象がありません。「シーンから泥棒AIを収集」で登録してください。", MessageType.Info);
            return;
        }

        // Popup表示用のラベル配列
        string[] options = new string[targets.Count];
        for (int i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            options[i] = t != null ? (i + ": " + t.name) : (i + ": (Missing)");
        }

        selectedTargetIndex = Mathf.Clamp(selectedTargetIndex, 0, targets.Count - 1);
        selectedTargetIndex = EditorGUILayout.Popup("選択中", selectedTargetIndex, options);

        // 選択中のオブジェクトをハイライト
        if (HasValidSelectedTarget())
        {
            var t = targets[selectedTargetIndex];
            if (t != null)
            {
                if (GUILayout.Button("選択対象をProject/Hierarchyで表示"))
                {
                    EditorGUIUtility.PingObject(t.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// targets 内の null（Destroy済みなど）を削除して、安全に操作できる状態に整える
    /// </summary>
    private void CleanupNullTargets()
    {
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (targets[i] == null) targets.RemoveAt(i);
        }

        if (targets.Count == 0) selectedTargetIndex = -1;
        else selectedTargetIndex = Mathf.Clamp(selectedTargetIndex, 0, targets.Count - 1);
    }

    /// <summary>
    /// 現在選択中の対象が有効か判定する
    /// </summary>
    private bool HasValidSelectedTarget()
    {
        return selectedTargetIndex >= 0
        && selectedTargetIndex < targets.Count
        && targets[selectedTargetIndex] != null;
    }

    /// <summary>
    /// 型名からTypeを検索する（UnityのAssembly構成差異に耐えるためのヘルパ）
    /// </summary>
    public static Type FindTypeByName(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var t = asm.GetType(typeName);
                if (t != null) return t;
            }
            catch
            {
            }
        }

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch
            {
                continue;
            }

            foreach (var t in types)
            {
                if (t == null) continue;
                if (t.Name == typeName) return t;
            }
        }

        return null;
    }
}
#endif
