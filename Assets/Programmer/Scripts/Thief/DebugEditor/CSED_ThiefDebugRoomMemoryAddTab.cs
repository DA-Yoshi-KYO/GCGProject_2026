/*＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の部屋記憶追加タブ
 *＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-06-11 | 初回作成 
 * 2026-06-11 | 日本語コメント追加
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
/// エディタ上で泥棒の部屋記憶（CS_RoomMemory）を追加・編集するためのUIクラス
/// - Scene内の `CS_RoomCreatePoint` の子にある `CS_RoomNode` を列挙して選択可能
/// - 対象泥棒はリスト化して個別/全員に対して操作可能
/// - Reflectionを使ってプロジェクトの private フィールドにもアクセスしている
/// </summary>
public sealed class CSED_ThiefDebugRoomMemoryAddTab
{
    // デバッグ対象の泥棒(MonoBehaviour)リスト
    private readonly List<MonoBehaviour> targets = new List<MonoBehaviour>();

    // targets 内で選択中のインデックス
    private int selectedTargetIndex = -1;

    // 対象追加用の ObjectFieldで一時的に保持する参照
    private MonoBehaviour addTarget;

    // Sceneから収集した RoomNode コンポーネントのリスト
    private List<Component> roomNodeList = new List<Component>();

    // roomNodeList 内で選択中のインデックス
    private int selectedRoomNodeIndex = -1;

    //追加する/設定する探索度（0-100)
    private int explorationLevel =0;

    /// <summary>
    /// タブ全体の描画エントリ
    /// </summary>
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

    /// <summary>
    /// 対象泥棒を追加するエリアの描画
    /// </summary>
    private void DrawAddTargetArea()
    {
        EditorGUILayout.LabelField("対象の追加", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            // ObjectFieldで泥棒(GameObjectに付いたコンポーネント)を指定
            addTarget = (MonoBehaviour)EditorGUILayout.ObjectField("追加する泥棒(AI)", addTarget, typeof(MonoBehaviour), true);

            //追加ボタン（選択されていない場合は無効化）
            using (new EditorGUI.DisabledScope(addTarget == null))
            {
                if (GUILayout.Button("リストに追加", GUILayout.Width(100)))
                {
                    // 重複防止してリストへ追加、選択インデックスを更新
                    if (!targets.Contains(addTarget))
                    {
                        targets.Add(addTarget);
                        selectedTargetIndex = targets.Count -1;
                    }
                    else
                    {
                        selectedTargetIndex = targets.IndexOf(addTarget);
                    }
                    addTarget = null; // 入力欄をクリア
                }
            }
        }

        // シーンから自動収集 / リストクリア
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
                    if (targets.Count >0 && selectedTargetIndex <0) selectedTargetIndex =0;
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
    /// 対象リスト表示エリアの描画
    /// </summary>
    private void DrawTargetListArea()
    {
        EditorGUILayout.LabelField("対象リスト", EditorStyles.boldLabel);
        CleanupNullTargets();
        if (targets.Count ==0)
        {
            EditorGUILayout.HelpBox("対象がありません。上の『追加』から登録してください。", MessageType.Warning);
            return;
        }

        // Popup用のラベル配列
        string[] options = new string[targets.Count];
        for (int i =0; i < targets.Count; i++)
        {
            var t = targets[i];
            options[i] = t != null ? (i + ": " + t.name) : (i + ": (Missing)");
        }

        selectedTargetIndex = Mathf.Clamp(selectedTargetIndex,0, targets.Count -1);
        selectedTargetIndex = EditorGUILayout.Popup("選択中", selectedTargetIndex, options);

        // 個別の操作ボタン（選択/削除）
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
    /// 選択中対象が有効か判定
    /// </summary>
    private bool HasValidSelectedTarget()
    {
        return selectedTargetIndex >=0
        && selectedTargetIndex < targets.Count
        && targets[selectedTargetIndex] != null;
    }

    /// <summary>
    /// null（破棄済み）のターゲットを除去してインデックスを調整
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
    /// 記憶追加エリアの描画
    /// </summary>
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

        if (roomNodeList.Count ==0)
        {
            EditorGUILayout.HelpBox("RoomNodeが見つかりません。まず" + "RoomNode一覧を更新" + "を押してください。", MessageType.Info);
        }
        else
        {
            // RoomNodeリストをPopupで表示（親のCreatePoint名も表示）
            string[] roomOptions = new string[roomNodeList.Count];
            for (int i =0; i < roomNodeList.Count; i++)
            {
                var comp = roomNodeList[i];
                string label = comp != null ? comp.gameObject.name + " (" + comp.transform.parent?.gameObject.name + ")" : i.ToString();
                roomOptions[i] = label;
            }

            selectedRoomNodeIndex = Mathf.Clamp(selectedRoomNodeIndex,0, roomNodeList.Count -1);
            selectedRoomNodeIndex = EditorGUILayout.Popup("選択RoomNode", selectedRoomNodeIndex, roomOptions);
        }

        explorationLevel = EditorGUILayout.IntSlider("探索度", explorationLevel,0,100);

        // 個別追加ボタン（選択部屋/現在の部屋）
        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(roomNodeList.Count ==0))
            {
                if (GUILayout.Button("選択部屋を追加(個別)"))
                {
                    var nodeComp = roomNodeList[selectedRoomNodeIndex];
                    var ok = TryAddRoomMemoryToThiefByNode(thief, nodeComp, explorationLevel, false);
                    if (ok) EditorUtility.DisplayDialog("完了", "部屋の記憶を追加しました。", "OK");
                }
            }

            if (GUILayout.Button("現在の部屋を追加(個別)"))
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
                        var ok = TryAddRoomMemoryToThiefByNode(thief, comp, explorationLevel, false);
                        if (ok) EditorUtility.DisplayDialog("完了", "部屋の記憶を追加しました。", "OK");
                    }
                    else
                    {
                        var go = currentRoomObj as GameObject;
                        if (go != null)
                        {
                            Type roomNodeType = FindTypeByName("CS_RoomNode");
                            Component nodeComp = null;
                            if (roomNodeType != null) nodeComp = go.GetComponentInChildren(roomNodeType) as Component;
                            if (nodeComp != null)
                            {
                                var ok = TryAddRoomMemoryToThiefByNode(thief, nodeComp, explorationLevel, false);
                                if (ok) EditorUtility.DisplayDialog("完了", "部屋の記憶を追加しました。", "OK");
                            }
                            else EditorUtility.DisplayDialog("エラー", "現在の部屋から RoomNode を取得できませんでした。", "OK");
                        }
                        else EditorUtility.DisplayDialog("エラー", "現在の部屋情報が不正です。", "OK");
                    }
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(roomNodeList.Count ==0 || targets.Count ==0))
            {
                if (GUILayout.Button("見つけた泥棒すべてに選択部屋を登録"))
                {
                    var nodeComp = roomNodeList.Count >0 && selectedRoomNodeIndex >=0 ? roomNodeList[selectedRoomNodeIndex] : null;
                    if (nodeComp == null)
                    {
                        EditorUtility.DisplayDialog("エラー", "追加するRoomNodeが選択されていません。", "OK");
                    }
                    else
                    {
                        int success =0;
                        foreach (var t in targets)
                        {
                            if (t == null) continue;
                            try
                            {
                                if (TryAddRoomMemoryToThiefByNode(t, nodeComp, explorationLevel, true)) success++;
                            }
                            catch { }
                        }
                        EditorUtility.DisplayDialog("完了", $"{success} 件の泥棒に記憶を登録しました。", "OK");
                    }
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(targets.Count ==0))
            {
                if (GUILayout.Button("見つけた泥棒すべての現在の部屋探索度を更新"))
                {
                    int updated =0;
                    foreach (var t in targets)
                    {
                        if (t == null) continue;
                        try
                        {
                            if (TryUpdateCurrentRoomExploration(t, explorationLevel, true)) updated++;
                        }
                        catch { }
                    }
                    EditorUtility.DisplayDialog("完了", $"{updated} 件の泥棒の現在部屋探索度を更新しました。", "OK");
                }
            }
        }
    }

    /// <summary>
    /// シーンから RoomCreatePoint の子にある RoomNode を列挙して roomNodeList に登録する
    /// </summary>
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

        if (roomNodeList.Count >0) selectedRoomNodeIndex =0;
    }

    /// <summary>
    /// 指定した RoomNode をその泥棒の記憶に追加する（Reflectionで roomMemories に直接追加）
    /// </summary>
    /// <param name="thief">対象泥棒コンポーネント</param>
    /// <param name="nodeComp">RoomNode コンポーネント</param>
    /// <param name="exploration">設定する探索度</param>
    /// <param name="silent">true のときダイアログを抑制する（バルク処理用）</param>
    /// <returns>追加に成功したら true</returns>
    private bool TryAddRoomMemoryToThiefByNode(MonoBehaviour thief, Component nodeComp, int exploration, bool silent = false)
    {
        if (nodeComp == null)
        {
            if (!silent) EditorUtility.DisplayDialog("エラー", "RoomNode を指定してください。", "OK");
            return false;
        }

        // MemorySystem を取得
        object memorySystemObj = GetPropertyValue(thief, "read_MemorySystem");
        if (memorySystemObj == null)
        {
            memorySystemObj = GetFieldOrPropertyValue(thief, "memorySystem", includePublic: true);
        }

        if (memorySystemObj == null)
        {
            if (!silent) EditorUtility.DisplayDialog("エラー", "MemorySystem を取得できませんでした。", "OK");
            return false;
        }

        // roomMemories フィールドを取得
        var roomMemoriesField = memorySystemObj.GetType().GetField("roomMemories", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (roomMemoriesField == null)
        {
            if (!silent) EditorUtility.DisplayDialog("エラー", "roomMemories フィールドが見つかりません。", "OK");
            return false;
        }

        var roomMemoriesObj = roomMemoriesField.GetValue(memorySystemObj) as IDictionary;
        if (roomMemoriesObj == null)
        {
            if (!silent) EditorUtility.DisplayDialog("エラー", "roomMemories を取得できませんでした。", "OK");
            return false;
        }

        //既存のキーがある場合は上書き確認（silent時は自動上書き）
        if (roomMemoriesObj.Contains(nodeComp))
        {
            if (!silent)
            {
                if (!EditorUtility.DisplayDialog("確認", "既にこの部屋の記憶が存在します。上書きしますか？", "上書き", "キャンセル"))
                {
                    return false;
                }
            }
            try { roomMemoriesObj.Remove(nodeComp); } catch { }
        }

        // CS_RoomMemory 型を生成して初期化
        var roomMemoryType = FindTypeByName("CS_RoomMemory");
        if (roomMemoryType == null)
        {
            if (!silent) EditorUtility.DisplayDialog("エラー", "CS_RoomMemory 型が見つかりません。", "OK");
            return false;
        }

        var newMemory = Activator.CreateInstance(roomMemoryType);
        // explorationLevel を設定
        var explorationField = roomMemoryType.GetField("explorationLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (explorationField != null)
        {
            explorationField.SetValue(newMemory, Mathf.Clamp(exploration,0,100));
        }

        // recognizedObjects / unchosenDoors を初期化
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

        // 辞書に追加（例外が出れば失敗）
        try
        {
            roomMemoriesObj.Add(nodeComp, newMemory);
        }
        catch
        {
            if (!silent) EditorUtility.DisplayDialog("エラー", "roomMemoriesへの追加に失敗しました。", "OK");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 指定泥棒の現在の部屋の探索度を更新する
    /// - 現在の部屋が roomMemories に存在する場合は探索度を上書き
    /// - 存在しない場合は新規に CS_RoomMemory を作成して登録
    /// </summary>
    private bool TryUpdateCurrentRoomExploration(MonoBehaviour thief, int exploration, bool silent)
    {
        if (thief == null) return false;

        object memorySystemObj = GetPropertyValue(thief, "read_MemorySystem");
        if (memorySystemObj == null)
        {
            memorySystemObj = GetFieldOrPropertyValue(thief, "memorySystem", includePublic: true);
        }

        if (memorySystemObj == null)
        {
            if (!silent) EditorUtility.DisplayDialog("エラー", "MemorySystem を取得できませんでした。", "OK");
            return false;
        }

        var currentRoomObj = GetPropertyValue(memorySystemObj, "read_CurrentRoom");
        if (currentRoomObj == null)
        {
            if (!silent) EditorUtility.DisplayDialog("エラー", "現在の部屋が取得できません。", "OK");
            return false;
        }

        // currentRoomObj が Componentか GameObjectかを判定して RoomNode コンポーネントを取得
        Component nodeComp = currentRoomObj as Component;
        if (nodeComp == null)
        {
            var go = currentRoomObj as GameObject;
            if (go != null)
            {
                Type roomNodeType = FindTypeByName("CS_RoomNode");
                if (roomNodeType != null) nodeComp = go.GetComponentInChildren(roomNodeType) as Component;
            }
        }

        if (nodeComp == null)
        {
            if (!silent) EditorUtility.DisplayDialog("エラー", "現在の部屋から RoomNode を取得できませんでした。", "OK");
            return false;
        }

        // roomMemories を取得して探索度を更新（存在しなければ新規作成）
        var roomMemoriesField = memorySystemObj.GetType().GetField("roomMemories", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (roomMemoriesField == null)
        {
            if (!silent) EditorUtility.DisplayDialog("エラー", "roomMemories フィールドが見つかりません。", "OK");
            return false;
        }

        var roomMemoriesObj = roomMemoriesField.GetValue(memorySystemObj) as IDictionary;
        if (roomMemoriesObj == null)
        {
            if (!silent) EditorUtility.DisplayDialog("エラー", "roomMemories を取得できませんでした。", "OK");
            return false;
        }

        //既存のメモリがある場合は探索度を設定
        if (roomMemoriesObj.Contains(nodeComp))
        {
            try
            {
                var mem = roomMemoriesObj[nodeComp];
                if (mem != null)
                {
                    var explorationField = mem.GetType().GetField("explorationLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (explorationField != null)
                    {
                        explorationField.SetValue(mem, Mathf.Clamp(exploration,0,100));
                        return true;
                    }
                }
            }
            catch { }
        }
        else
        {
            // 存在しない場合は新規に作成して追加
            var roomMemoryType = FindTypeByName("CS_RoomMemory");
            if (roomMemoryType == null)
            {
                if (!silent) EditorUtility.DisplayDialog("エラー", "CS_RoomMemory 型が見つかりません。", "OK");
                return false;
            }

            var newMemory = Activator.CreateInstance(roomMemoryType);
            var explorationField = roomMemoryType.GetField("explorationLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (explorationField != null) explorationField.SetValue(newMemory, Mathf.Clamp(exploration,0,100));

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

            try
            {
                roomMemoriesObj.Add(nodeComp, newMemory);
                return true;
            }
            catch
            {
                if (!silent) EditorUtility.DisplayDialog("エラー", "roomMemoriesへの追加に失敗しました。", "OK");
                return false;
            }
        }

        return false;
    }

    // Helpers: reflection helpers
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
