/*＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の部屋に関する記憶を表示するタブ
 *＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-19 | 初回作成 
 * 2026-05-22 | ファイル名を変更（ThiefDebugRoomMemoryTab.cs → CSED_ThiefDebugRoomMemoryTab.cs）
 *            | クラス名を変更（ThiefDebugRoomMemoryTab → CSED_ThiefDebugRoomMemoryTab）
 *　
 */

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// 部屋に関する記憶デバック用
/// </summary>
internal sealed class CSED_ThiefDebugRoomMemoryTab
{
    [Tooltip("対象（泥棒AIコンポーネント）リスト")]
    private readonly List<MonoBehaviour> targets = new List<MonoBehaviour>();

    [Tooltip("targets内の選択中index（-1は未選択）")]
    private int selectedTargetIndex = -1;

    [Tooltip("対象追加用のObjectField")]
    private MonoBehaviour addTarget;

    [Tooltip("ルームごとの「詳細表示」を保持（RoomInstanceID -> bool）")]
    private readonly Dictionary<int, bool> detailToggles = new Dictionary<int, bool>();


    /// <summary>
    /// タブ全体描画
    /// </summary>
    public void Draw()
    {
        EditorGUILayout.LabelField("ルーム記憶", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
        "選択中の泥棒が保持している roomMemories(部屋に関する記憶) を簡易表示します。\n" +
        "探索度/危険度/認識オブジェクト/入ってきたドア/選ばなかったドアなどを確認できます。",
        MessageType.Info);

        // 対象追加
        DrawAddTargetArea();
        EditorGUILayout.Space(8);

        // 対象リスト表示/選択
        DrawTargetListArea();
        EditorGUILayout.Space(8);

        // 選択中対象のメモリ表示
        DrawRoomMemoriesArea();
    }

    /// <summary>
    /// 対象（泥棒AI）をリストに追加するUI
    /// </summary>
    private void DrawAddTargetArea()
    {
        EditorGUILayout.LabelField("対象の追加", EditorStyles.boldLabel);

        //1件追加（ObjectField ->追加ボタン）
        using (new EditorGUILayout.HorizontalScope())
        {
            addTarget = (MonoBehaviour)EditorGUILayout.ObjectField("追加する泥棒(AI)", addTarget, typeof(MonoBehaviour), true);

            using (new EditorGUI.DisabledScope(addTarget == null))
            {
                if (GUILayout.Button("リストに追加", GUILayout.Width(100)))
                {
                    // 重複登録を防ぐ
                    if (!targets.Contains(addTarget))
                    {
                        targets.Add(addTarget);
                        selectedTargetIndex = targets.Count -1;
                    }
                    else
                    {
                        //既にある場合はその要素を選択状態にする
                        selectedTargetIndex = targets.IndexOf(addTarget);
                    }

                    // 次の追加のために入力欄をクリア
                    addTarget = null;
                }
            }
        }

        // 一括操作（シーン収集/クリア）
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("シーンから泥棒AIを収集"))
            {
                // 型名から泥棒AI型を探して収集
                var thiefAiType = FindTypeByName("CS_ThiefAI");
                if (thiefAiType == null)
                {
                    EditorGUILayout.HelpBox("泥棒AI型が見つかりません。", MessageType.Warning);
                }
                else
                {
                    var found = UnityEngine.Object.FindObjectsOfType(thiefAiType);
                    foreach (var o in found)
                    {
                        var mb = o as MonoBehaviour;
                        if (mb == null) continue;
                        if (!targets.Contains(mb)) targets.Add(mb);
                    }

                    // 選択が無ければ先頭を選択する
                    if (targets.Count >0 && selectedTargetIndex <0)
                    {
                        selectedTargetIndex =0;
                    }
                }
            }

            using (new EditorGUI.DisabledScope(targets.Count ==0))
            {
                if (GUILayout.Button("リストをクリア", GUILayout.Width(100)))
                {
                    targets.Clear();
                    selectedTargetIndex = -1;
                }
            }
        }
    }

    /// <summary>
    /// 対象リストの表示/選択/削除
    /// </summary>
    private void DrawTargetListArea()
    {
        EditorGUILayout.LabelField("対象リスト", EditorStyles.boldLabel);

        //参照切れ（Destroy済み）を掃除
        CleanupNullTargets();

        if (targets.Count ==0)
        {
            EditorGUILayout.HelpBox("対象がありません。上の『追加』から登録してください。", MessageType.Warning);
            return;
        }

        // Popup表示用のラベル配列
        string[] options = new string[targets.Count];
        for (int i =0; i < targets.Count; i++)
        {
            var t = targets[i];
            options[i] = t != null ? (i + ": " + t.name) : (i + ": (Missing)");
        }

        selectedTargetIndex = Mathf.Clamp(selectedTargetIndex,0, targets.Count -1);
        selectedTargetIndex = EditorGUILayout.Popup("選択中", selectedTargetIndex, options);

        // 一覧 + 個別操作
        for (int i =0; i < targets.Count; i++)
        {
            var t = targets[i];
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(i.ToString(), t, typeof(MonoBehaviour), true);
                }

                if (GUILayout.Button("選択", GUILayout.Width(60)))
                {
                    selectedTargetIndex = i;
                    if (t != null) Selection.activeObject = t.gameObject;
                }

                if (GUILayout.Button("削除", GUILayout.Width(60)))
                {
                    targets.RemoveAt(i);
                    if (selectedTargetIndex >= targets.Count) selectedTargetIndex = targets.Count -1;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 現在選択中の対象が有効か判定する
    /// </summary>
    private bool HasValidSelectedTarget()
    {
        return selectedTargetIndex >=0
        && selectedTargetIndex < targets.Count
        && targets[selectedTargetIndex] != null;
    }

    /// <summary>
    /// targets 内の null（Destroy済みなど）を削除して、安全に操作できる状態に整える
    /// </summary>
    private void CleanupNullTargets()
    {
        for (int i = targets.Count -1; i >=0; i--)
        {
            if (targets[i] == null) targets.RemoveAt(i);
        }

        if (targets.Count ==0) selectedTargetIndex = -1;
        else selectedTargetIndex = Mathf.Clamp(selectedTargetIndex,0, targets.Count -1);
    }

    /// <summary>
    /// 選択中の泥棒AIが保持している roomMemories を一覧表示する
    /// </summary>
    private void DrawRoomMemoriesArea()
    {
        if (!HasValidSelectedTarget())
        {
            EditorGUILayout.HelpBox("対象が選択されていません。", MessageType.Warning);
            return;
        }

        var thief = targets[selectedTargetIndex];
        if (thief == null)
        {
            EditorGUILayout.HelpBox("選択中の対象が参照切れです。", MessageType.Warning);
            return;
        }

        // 誤って別コンポーネントが入った場合の保険
        if (thief.GetType().Name != "CS_ThiefAI")
        {
            EditorGUILayout.HelpBox("選択したオブジェクトは泥棒AIではありません。", MessageType.Warning);
            return;
        }

        // ThiefAI.RoomMemories をReflectionで取得
        object memoriesObj = GetPropertyValue(thief, "RoomMemories");
        if (memoriesObj == null)
        {
            EditorGUILayout.HelpBox("RoomMemories を取得できません（泥棒AI側に RoomMemories プロパティが必要です）。", MessageType.Warning);
            return;
        }

        var dict = memoriesObj as IEnumerable;
        if (dict == null)
        {
            EditorGUILayout.HelpBox("RoomMemories が辞書として扱えません。", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("部屋一覧", EditorStyles.boldLabel);

        bool hasAny = false;
        foreach (var entry in dict)
        {
            // KeyValuePair<,> を想定して Key/Value を取得
            var key = GetPropertyValue(entry, "Key") as UnityEngine.Object; // RoomNode想定
            var value = GetPropertyValue(entry, "Value"); // RoomMemory想定
            if (key == null || value == null) continue;

            hasAny = true;

            int roomId = key.GetInstanceID();
            bool detail;
            if (!detailToggles.TryGetValue(roomId, out detail)) detail = false;

            // --- 値取得 ---
            // RoomMemoryは public field のケースがあるため Public/NonPublic 両方から取得する
            var explorationLevel = GetFieldOrPropertyValue(value, "explorationLevel", includePublic: true);
            var dangerLevel = GetFieldOrPropertyValue(value, "dangerLevel", includePublic: true);
            var entered = GetFieldOrPropertyValue(value, "enteredDoorDirection", includePublic: true);

            var unchosenDoorsObj = GetFieldOrPropertyValue(value, "unchosenDoors", includePublic: true) as IEnumerable;
            var unchosenList = new List<string>();
            if (unchosenDoorsObj != null)
            {
                foreach (var d in unchosenDoorsObj)
                {
                    if (d == null) continue;
                    unchosenList.Add(d.ToString());
                }
            }

            var recognizedObjectsObj = GetFieldOrPropertyValue(value, "recognizedObjects", includePublic: true) as IEnumerable;
            var recognized = new List<UnityEngine.Object>();
            if (recognizedObjectsObj != null)
            {
                foreach (var o in recognizedObjectsObj)
                {
                    var uo = o as UnityEngine.Object;
                    if (uo != null) recognized.Add(uo);
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                // ヘッダー行
                //ルーム名：○○○○ [選択] ┃ 探索度：○○％ ┃ 詳細表示：☑
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("ルーム名：" + key.name, EditorStyles.boldLabel);

                    if (GUILayout.Button("選択", GUILayout.Width(60)))
                    {
                        Selection.activeObject = key;
                        EditorGUIUtility.PingObject(key);
                    }

                    GUILayout.FlexibleSpace();

                    EditorGUILayout.LabelField("探索度：" + ToPercentText(explorationLevel), GUILayout.Width(120));

                    bool newDetail = EditorGUILayout.ToggleLeft("詳細表示", detail, GUILayout.Width(90));
                    if (newDetail != detail)
                    {
                        detail = newDetail;
                        detailToggles[roomId] = detail;
                    }
                }

                // 探索度以外の補助情報（常時表示）
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("危険度：" + ToPercentText(dangerLevel), GUILayout.Width(120));
                    EditorGUILayout.LabelField("入ってきたドア：" + (entered != null ? entered.ToString() : "-"));
                }

                if (detail)
                {
                    EditorGUILayout.Space(4);

                    // (詳細)
                    EditorGUILayout.LabelField("(詳細)", EditorStyles.miniBoldLabel);

                    // 選択しなかったドア | ○○個
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("選択しなかったドア", GUILayout.Width(140));
                        EditorGUILayout.LabelField("| " + unchosenList.Count + "個");
                    }

                    // ・Right
                    for (int i =0; i < unchosenList.Count; i++)
                    {
                        EditorGUILayout.LabelField("・" + unchosenList[i]);
                    }

                    EditorGUILayout.Space(6);

                    // 認識オブジェクト | ○○個
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("認識オブジェクト", GUILayout.Width(140));
                        EditorGUILayout.LabelField("| " + recognized.Count + "個");
                    }

                    // ・オブジェクト名 [選択]
                    for (int i =0; i < recognized.Count; i++)
                    {
                        var obj = recognized[i];
                        if (obj == null) continue;

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("・" + obj.name, GUILayout.MinWidth(200));

                            if (GUILayout.Button("選択", GUILayout.Width(60)))
                            {
                                Selection.activeObject = obj;
                                EditorGUIUtility.PingObject(obj);
                            }
                        }
                    }
                }
            }
        }

        if (!hasAny)
        {
            EditorGUILayout.HelpBox("roomMemories が空、または表示可能な要素がありません。", MessageType.Info);
        }
    }

    /// <summary>
    /// 0-100の値を想定して「xx%」文字列に整形する
    /// </summary>
    private static string ToPercentText(object value)
    {
        if (value == null) return "-";

        if (value is int i) return i + "%";
        if (value is float f) return Mathf.RoundToInt(f) + "%";
        if (value is double d) return Mathf.RoundToInt((float)d) + "%";

        int parsed;
        if (int.TryParse(value.ToString(), out parsed)) return parsed + "%";

        return value + "%";
    }

    /// <summary>
    /// 指定名のプロパティ値を取得する
    /// </summary>
    private static object GetPropertyValue(object obj, string name)
    {
        if (obj == null) return null;
        var t = obj.GetType();
        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return p != null ? p.GetValue(obj, null) : null;
    }

    /// <summary>
    /// 指定名のFieldまたはPropertyから値を取得する
    /// </summary>
    private static object GetFieldOrPropertyValue(object obj, string name, bool includePublic)
    {
        if (obj == null) return null;

        var t = obj.GetType();
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        if (includePublic) flags |= BindingFlags.Public;

        var f = t.GetField(name, flags);
        if (f != null) return f.GetValue(obj);

        var p = t.GetProperty(name, flags);
        if (p != null) return p.GetValue(obj, null);

        return null;
    }

    /// <summary>
    /// 型名からTypeを検索する（UnityのAssembly構成差異に耐えるためのヘルパ）
    /// </summary>
    private static Type FindTypeByName(string typeName)
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
