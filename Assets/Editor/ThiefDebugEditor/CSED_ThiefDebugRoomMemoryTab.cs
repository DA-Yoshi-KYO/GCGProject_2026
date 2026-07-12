/*＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の部屋に関する記憶を表示するタブ
 *＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-19 | 初回作成 
 * 2026-05-22 | ファイル名を変更（ThiefDebugRoomMemoryTab.cs → CSED_ThiefDebugRoomMemoryTab.cs）
 *            | クラス名を変更（ThiefDebugRoomMemoryTab → CSED_ThiefDebugRoomMemoryTab）
 * 2026-05-28 | CS_ThiefAIのroomMemoriesがCS_MemorySystemに移動したことへの対応
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
    [Tooltip("ルームごとの「詳細表示」を保持（RoomInstanceID -> bool）")]
    private readonly Dictionary<int, bool> detailToggles = new Dictionary<int, bool>();


    /// <summary>
    /// タブ全体描画
    /// </summary>
    public void Draw(List<MonoBehaviour> targets, int selectedTargetIndex)
    {
        EditorGUILayout.LabelField("ルーム記憶", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
        "選択中の泥棒が保持している roomMemories(部屋に関する記憶) を簡易表示します。\n" +
        "探索度/危険度/認識オブジェクト/入ってきたドア/選ばなかったドアなどを確認できます。",
        MessageType.Info);

        if (targets.Count == 0)
        {
            EditorGUILayout.HelpBox("対象がありません。上の『追加』から登録してください。", MessageType.Warning);
            return;
        }

        // 選択中対象のメモリ表示
        DrawRoomMemoriesArea(targets, selectedTargetIndex);
    }

    /// <summary>
    /// 現在選択中の対象が有効か判定する
    /// </summary>
    private bool HasValidSelectedTarget(List<MonoBehaviour> targets, int selectedTargetIndex)
    {
        return selectedTargetIndex >= 0
        && selectedTargetIndex < targets.Count
        && targets[selectedTargetIndex] != null;
    }

    /// <summary>
    /// 選択中の泥棒AIが保持している roomMemories を一覧表示する
    /// </summary>
    private void DrawRoomMemoriesArea(List<MonoBehaviour> targets, int selectedTargetIndex)
    {
        if (!HasValidSelectedTarget(targets, selectedTargetIndex))
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

        // 現行CS_ThiefAIでは roomMemories は CS_MemorySystem が保持している
        object memorySystemObj = GetPropertyValue(thief, "read_MemorySystem");
        if (memorySystemObj == null)
        {
            // フィールドの可能性にも対応
            memorySystemObj = GetFieldOrPropertyValue(thief, "memorySystem", includePublic: true);
        }

        if (memorySystemObj == null)
        {
            EditorGUILayout.HelpBox("MemorySystem を取得できません（CS_ThiefAI側に read_MemorySystem が必要です）。", MessageType.Warning);
            return;
        }

        object memoriesObj = GetPropertyValue(memorySystemObj, "read_RoomMemorys");
        if (memoriesObj == null)
        {
            // 将来の命名揺れ保険
            memoriesObj = GetPropertyValue(memorySystemObj, "RoomMemories");
        }

        if (memoriesObj == null)
        {
            EditorGUILayout.HelpBox("RoomMemory辞書を取得できません（CS_MemorySystem側に read_RoomMemorys が必要です）。", MessageType.Warning);
            return;
        }

        var dict = memoriesObj as IEnumerable;
        if (dict == null)
        {
            EditorGUILayout.HelpBox("RoomMemory辞書が列挙できません。", MessageType.Warning);
            return;
        }

        //追加：現在の探索対象・視認ターゲットのメモリ辞書を取得
        object currentTargetObj = GetPropertyValue(memorySystemObj, "read_CurrentTarget");
        if (currentTargetObj == null)
        {
            currentTargetObj = GetFieldOrPropertyValue(memorySystemObj, "currentTarget", includePublic: true);
        }

        IDictionary visionTargetMemoriesDict = GetFieldOrPropertyValue(memorySystemObj, "visionTargetMemories", includePublic: true) as IDictionary;

        EditorGUILayout.LabelField("部屋一覧", EditorStyles.boldLabel);

        bool hasAny = false;
        foreach (var entry in dict)
        {
            // KeyValuePair<,> を想定して Key/Value を取得
            var key = GetPropertyValue(entry, "Key") as UnityEngine.Object; // CS_RoomNode想定
            var value = GetPropertyValue(entry, "Value"); // CS_RoomMemory想定
            if (key == null || value == null) continue;

            hasAny = true;

            int roomId = key.GetInstanceID();
            bool detail;
            if (!detailToggles.TryGetValue(roomId, out detail)) detail = false;

            // --- 値取得 ---
            // CS_RoomMemoryは public field 想定（念のため Public/NonPublic 両対応）
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

                    //追加：現在の探索対象
                    DrawCurrentTargetArea(currentTargetObj, visionTargetMemoriesDict);

                    // 選択しなかったドア | ○○個
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("選択しなかったドア", GUILayout.Width(140));
                        EditorGUILayout.LabelField("| " + unchosenList.Count + "個");
                    }

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

                    // ・オブジェクト名 (探索済み: true/false) [選択]
                    for (int i =0; i < recognized.Count; i++)
                    {
                        var obj = recognized[i];
                        if (obj == null) continue;

                        string exploredText = "-";
                        try
                        {
                            if (visionTargetMemoriesDict != null && visionTargetMemoriesDict.Contains(obj))
                            {
                                var mem = visionTargetMemoriesDict[obj];
                                var isExplored = GetFieldOrPropertyValue(mem, "isExplored", includePublic: true);
                                exploredText = isExplored != null ? isExplored.ToString() : "-";
                            }
                        }
                        catch
                        {
                            exploredText = "-";
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("・" + obj.name + " (探索済み: " + exploredText + ")", GUILayout.MinWidth(200));

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
    /// 現在の探索対象エリアを描画
    /// </summary>
    private static void DrawCurrentTargetArea(object currentTargetObj, IDictionary visionTargetMemoriesDict)
    {
        // 線で分割
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

        EditorGUILayout.LabelField("現在の探索対象", EditorStyles.boldLabel);

        var uo = currentTargetObj as UnityEngine.Object;
        if (uo == null)
        {
            EditorGUILayout.LabelField("- (なし)");
            return;
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("オブジェクト名：" + uo.name, GUILayout.MinWidth(200));
            if (GUILayout.Button("選択", GUILayout.Width(60)))
            {
                Selection.activeObject = uo;
                EditorGUIUtility.PingObject(uo);
            }
        }

        // visionTargetMemories（CS_VisionTarget -> CS_VisionTargetMemory）の内容を表示
        if (visionTargetMemoriesDict == null)
        {
            EditorGUILayout.HelpBox("visionTargetMemories を取得できません。", MessageType.Info);
            return;
        }

        object memory = null;
        try
        {
            if (visionTargetMemoriesDict.Contains(uo))
            {
                memory = visionTargetMemoriesDict[uo];
            }
        }
        catch
        {
            // IDictionary実装差異などで例外が出ても落とさない
            memory = null;
        }

        if (memory == null)
        {
            EditorGUILayout.LabelField("visionTargetMemories：-(対象外 または 未登録)");
            return;
        }

        var explorationProgress = GetFieldOrPropertyValue(memory, "explorationProgress", includePublic: true);
        var isExplored = GetFieldOrPropertyValue(memory, "isExplored", includePublic: true);
        var searchThief = GetFieldOrPropertyValue(memory, "searchThief", includePublic: true) as UnityEngine.Object;

        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField("visionTargetMemories", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("探索進行度：" + ToPercentText(explorationProgress));
            EditorGUILayout.LabelField("探索済み：" + (isExplored != null ? isExplored.ToString() : "-"));

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("探索している人：" + (searchThief != null ? searchThief.name : "-"), GUILayout.MinWidth(200));
                using (new EditorGUI.DisabledScope(searchThief == null))
                {
                    if (GUILayout.Button("選択", GUILayout.Width(60)))
                    {
                        Selection.activeObject = searchThief;
                        EditorGUIUtility.PingObject(searchThief);
                    }
                }
            }
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
}

#endif
