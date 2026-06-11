#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// 泥棒に部屋の記憶を追加するデバッグタブ
/// </summary>
public sealed class CSED_ThiefDebugRoomMemoryAddTab
{
    private readonly List<MonoBehaviour> targets = new List<MonoBehaviour>();
    private int selectedTargetIndex = -1;
    private MonoBehaviour addTarget;

    // 部屋ノードリスト
    private List<Component> roomNodeList = new List<Component>();
    private int selectedRoomNodeIndex = -1;

    private int explorationLevel = 0;

    public void Draw()
    {
        EditorGUILayout.LabelField("部屋記憶の追加", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("指定した泥棒に対して、特定の部屋の記憶(CS_RoomMemory)を追加します。部屋はScene内のRoomCreatePointの子にあるRoomNodeから選択してください。", MessageType.Info);

        DrawAddTargetArea();
        EditorGUILayout.Space(8);
        DrawTargetListArea();
        EditorGUILayout.Space(8);
        DrawAddMemoryArea();
    }

    private void DrawAddTargetArea()
    {
        EditorGUILayout.LabelField("対象の追加", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            addTarget = (MonoBehaviour)EditorGUILayout.ObjectField("追加する泥棒(AI)", addTarget, typeof(MonoBehaviour), true);
            using (new EditorGUI.DisabledScope(addTarget == null))
            {
                if (GUILayout.Button("リストに追加", GUILayout.Width(100)))
                {
                    if (!targets.Contains(addTarget))
                    {
                        targets.Add(addTarget);
                        selectedTargetIndex = targets.Count - 1;
                    }
                    else
                    {
                        selectedTargetIndex = targets.IndexOf(addTarget);
                    }
                    addTarget = null;
                }
            }
        }

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
                    if (targets.Count > 0 && selectedTargetIndex < 0) selectedTargetIndex = 0;
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
    }

    private void DrawTargetListArea()
    {
        EditorGUILayout.LabelField("対象リスト", EditorStyles.boldLabel);
        CleanupNullTargets();
        if (targets.Count == 0)
        {
            EditorGUILayout.HelpBox("対象がありません。上の『追加』から登録してください。", MessageType.Warning);
            return;
        }

        string[] options = new string[targets.Count];
        for (int i = 0 ; i < targets.Count ; i++)
        {
            var t = targets[i];
            options[i] = t != null ? (i + ": " + t.name) : (i + ": (Missing)");
        }

        selectedTargetIndex = Mathf.Clamp(selectedTargetIndex, 0, targets.Count - 1);
        selectedTargetIndex = EditorGUILayout.Popup("選択中", selectedTargetIndex, options);

        for (int i = 0 ; i < targets.Count ; i++)
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
                    if (selectedTargetIndex >= targets.Count) selectedTargetIndex = targets.Count - 1;
                    break;
                }
            }
        }
    }

    private bool HasValidSelectedTarget()
    {
        return selectedTargetIndex >= 0
        && selectedTargetIndex < targets.Count
        && targets[selectedTargetIndex] != null;
    }

    private void CleanupNullTargets()
    {
        for (int i = targets.Count - 1 ; i >= 0 ; i--)
        {
            if (targets[i] == null) targets.RemoveAt(i);
        }

        if (targets.Count == 0) selectedTargetIndex = -1;
        else selectedTargetIndex = Mathf.Clamp(selectedTargetIndex, 0, targets.Count - 1);
    }

    private void DrawAddMemoryArea()
    {
        EditorGUILayout.LabelField("記憶追加", EditorStyles.boldLabel);

        if (!HasValidSelectedTarget())
        {
            EditorGUILayout.HelpBox("対象が選択されていません。", MessageType.Warning);
            return;
        }

        var thief = targets[selectedTargetIndex];
        if (thief == null || thief.GetType().Name != "CS_ThiefAI")
        {
            EditorGUILayout.HelpBox("選択したオブジェクトは泥棒AIではありません。", MessageType.Warning);
            return;
        }

        // RoomNode一覧を表示
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("RoomNode一覧を更新"))
            {
                RefreshRoomNodeList();
            }
            GUILayout.Label(string.Empty);
        }

        if (roomNodeList.Count == 0)
        {
            EditorGUILayout.HelpBox("RoomNodeが見つかりません。まず" + "RoomNode一覧を更新" + "を押してください。", MessageType.Info);
        }
        else
        {
            string[] roomOptions = new string[roomNodeList.Count];
            for (int i = 0 ; i < roomNodeList.Count ; i++)
            {
                var comp = roomNodeList[i];
                string label = comp != null ? comp.gameObject.name + " (" + comp.transform.parent?.gameObject.name + ")" : i.ToString();
                roomOptions[i] = label;
            }

            selectedRoomNodeIndex = Mathf.Clamp(selectedRoomNodeIndex, 0, roomNodeList.Count - 1);
            selectedRoomNodeIndex = EditorGUILayout.Popup("選択RoomNode", selectedRoomNodeIndex, roomOptions);
        }

        explorationLevel = EditorGUILayout.IntField("探索度(0-100)", explorationLevel);

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(roomNodeList.Count == 0))
            {
                if (GUILayout.Button("記憶を追加"))
                {
                    var nodeComp = roomNodeList[selectedRoomNodeIndex];
                    TryAddRoomMemoryToThiefByNode(thief, nodeComp, explorationLevel);
                }
            }

            if (GUILayout.Button("現在の部屋を追加"))
            {
                object memorySystemObj = GetPropertyValue(thief, "read_MemorySystem");
                if (memorySystemObj == null)
                {
                    memorySystemObj = GetFieldOrPropertyValue(thief, "memorySystem", includePublic: true);
                }

                var currentRoomObj = GetPropertyValue(memorySystemObj, "read_CurrentRoom") as UnityEngine.Object;
                if (currentRoomObj == null)
                {
                    EditorUtility.DisplayDialog("エラー", "現在の部屋が取得できません。", "OK");
                }
                else
                {
                    var comp = currentRoomObj as Component;
                    if (comp != null)
                    {
                        TryAddRoomMemoryToThiefByNode(thief, comp, explorationLevel);
                    }
                    else
                    {
                        var go = currentRoomObj as GameObject;
                        if (go != null)
                        {
                            // try to find RoomNode component on the GameObject
                            Type roomNodeType = FindTypeByName("CS_RoomNode");
                            Component nodeComp = null;
                            if (roomNodeType != null) nodeComp = go.GetComponentInChildren(roomNodeType) as Component;
                            if (nodeComp != null) TryAddRoomMemoryToThiefByNode(thief, nodeComp, explorationLevel);
                            else EditorUtility.DisplayDialog("エラー", "現在の部屋から RoomNode を取得できませんでした。", "OK");
                        }
                        else EditorUtility.DisplayDialog("エラー", "現在の部屋情報が不正です。", "OK");
                    }
                }
            }
        }
    }

    private void RefreshRoomNodeList()
    {
        roomNodeList.Clear();
        selectedRoomNodeIndex = -1;

        var createPointType = FindTypeByName("CS_RoomCreatePoint");
        var roomNodeType = FindTypeByName("CS_RoomNode");
        if (createPointType == null || roomNodeType == null) return;

        var createPoints = UnityEngine.Object.FindObjectsOfType(createPointType);
        foreach (var cp in createPoints)
        {
            var cpComp = cp as Component;
            if (cpComp == null) continue;
            var node = cpComp.GetComponentInChildren(roomNodeType) as Component;
            if (node != null) roomNodeList.Add(node);
        }

        if (roomNodeList.Count > 0) selectedRoomNodeIndex = 0;
    }

    private void TryAddRoomMemoryToThiefByNode(MonoBehaviour thief, Component nodeComp, int exploration)
    {
        if (nodeComp == null)
        {
            EditorUtility.DisplayDialog("エラー", "RoomNode を指定してください。", "OK");
            return;
        }

        // MemorySystem を取得
        object memorySystemObj = GetPropertyValue(thief, "read_MemorySystem");
        if (memorySystemObj == null)
        {
            memorySystemObj = GetFieldOrPropertyValue(thief, "memorySystem", includePublic: true);
        }

        if (memorySystemObj == null)
        {
            EditorUtility.DisplayDialog("エラー", "MemorySystem を取得できませんでした。", "OK");
            return;
        }

        // roomMemories フィールドを取得して追加
        var roomMemoriesField = memorySystemObj.GetType().GetField("roomMemories", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (roomMemoriesField == null)
        {
            EditorUtility.DisplayDialog("エラー", "roomMemories フィールドが見つかりません。", "OK");
            return;
        }

        var roomMemoriesObj = roomMemoriesField.GetValue(memorySystemObj) as IDictionary;
        if (roomMemoriesObj == null)
        {
            EditorUtility.DisplayDialog("エラー", "roomMemories を取得できませんでした。", "OK");
            return;
        }

        //既存の key を確認
        if (roomMemoriesObj.Contains(nodeComp))
        {
            if (!EditorUtility.DisplayDialog("確認", "既にこの部屋の記憶が存在します。上書きしますか？", "上書き", "キャンセル"))
            {
                return;
            }
            roomMemoriesObj.Remove(nodeComp);
        }

        // CS_RoomMemory のインスタンスを作成
        var roomMemoryType = FindTypeByName("CS_RoomMemory");
        if (roomMemoryType == null)
        {
            EditorUtility.DisplayDialog("エラー", "CS_RoomMemory 型が見つかりません。", "OK");
            return;
        }

        var newMemory = Activator.CreateInstance(roomMemoryType);
        // explorationLevel を設定
        var explorationField = roomMemoryType.GetField("explorationLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (explorationField != null)
        {
            explorationField.SetValue(newMemory, Mathf.Clamp(exploration, 0, 100));
        }

        // recognizedObjects と unchosenDoors を初期化
        var recognizedField = roomMemoryType.GetField("recognizedObjects", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (recognizedField != null)
        {
            var thiefTargetType = FindTypeByName("CS_ThiefTarget");
            Type listType;
            if (thiefTargetType != null) listType = typeof(List<>).MakeGenericType(thiefTargetType);
            else listType = typeof(List<UnityEngine.Object>);
            var listInstance = Activator.CreateInstance(listType);
            recognizedField.SetValue(newMemory, listInstance);
        }
        var unchosenField = roomMemoryType.GetField("unchosenDoors", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (unchosenField != null)
        {
            var doorDirType = FindTypeByName("CSE_RoomDoorDirection");
            Type listType;
            if (doorDirType != null) listType = typeof(List<>).MakeGenericType(doorDirType);
            else listType = typeof(List<UnityEngine.Object>);
            var listInstance = Activator.CreateInstance(listType);
            unchosenField.SetValue(newMemory, listInstance);
        }

        // 辞書に追加
        roomMemoriesObj.Add(nodeComp, newMemory);

        EditorUtility.DisplayDialog("完了", "部屋の記憶を追加しました。", "OK");
    }

    // Helpers: reflection helpers replicated from viewer tab
    private static object GetPropertyValue(object obj, string name)
    {
        if (obj == null) return null;
        var t = obj.GetType();
        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return p != null ? p.GetValue(obj, null) : null;
    }

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

    private static Type FindTypeByName(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var t = asm.GetType(typeName);
                if (t != null) return t;
            }
            catch { }
        }

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); } catch { continue; }
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
