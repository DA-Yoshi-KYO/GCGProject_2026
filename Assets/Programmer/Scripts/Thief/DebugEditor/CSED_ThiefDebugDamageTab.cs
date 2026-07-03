/*＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のダメージデバッグタブ
 *＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-19 | 初回作成 
 * 2026-05-22 | ファイル名を変更（ThiefDebugDamageTab.cs → CSED_ThiefDebugDamageTab.cs）
 *            | クラス名を変更（ThiefDebugDamageTab → CSED_ThiefDebugDamageTab）
 * 2026-05-28 | ThiefReactionUIType enum からダメージ種別を選択する機能を追加
 *　
 */
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// ダメージデバックタブ用
/// </summary>
internal sealed class CSED_ThiefDebugDamageTab
{
    [Tooltip("対象（泥棒AIコンポーネント）リスト")]
    private readonly List<MonoBehaviour> targets = new List<MonoBehaviour>();

    [Tooltip("対象リスト内の選択中index（-1は未選択）")]
    private int selectedTargetIndex = -1;

    [Tooltip("追加用ObjectField")]
    private MonoBehaviour addTarget;

    [Tooltip("与えるダメージ量")]
    private int damageAmount = 1;

    [Tooltip("ダメージを与えるギミック種類")]
    private string damageReactionTypeName = "Pot";

    /// <summary>
    /// タブ全体描画
    /// </summary>
    public void Draw()
    {
        EditorGUILayout.LabelField("ダメージ", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
        "登録した対象(泥棒AI)に対して、指定したダメージを与えます。\n" +
        "実行中に対象を追加/選択できます。",
        MessageType.Info);

        // 対象追加
        DrawAddTargetArea();
        EditorGUILayout.Space(8);

        // 対象リスト表示/選択
        DrawTargetListArea();
        EditorGUILayout.Space(8);

        // ダメージ設定/実行
        DrawDamageExecuteArea();
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
                        selectedTargetIndex = targets.Count - 1;
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
                    if (targets.Count > 0 && selectedTargetIndex < 0)
                    {
                        selectedTargetIndex = 0;
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
    }

    /// <summary>
    /// 対象リストの表示/選択/削除
    /// </summary>
    private void DrawTargetListArea()
    {
        EditorGUILayout.LabelField("対象リスト", EditorStyles.boldLabel);

        //参照切れ（Destroy済み）を掃除
        CleanupNullTargets();

        if (targets.Count == 0)
        {
            EditorGUILayout.HelpBox("対象がありません。上の『追加』から登録してください。", MessageType.Warning);
            return;
        }

        // Popup表示用のラベル配列を作る（name表示）
        string[] options = new string[targets.Count];
        for (int i = 0 ; i < targets.Count ; i++)
        {
            var t = targets[i];
            options[i] = t != null ? (i + ": " + t.name) : (i + ": (Missing)");
        }

        // 範囲外参照が起きないように補正
        selectedTargetIndex = Mathf.Clamp(selectedTargetIndex, 0, targets.Count - 1);

        // 選択中の対象をPopupで切り替え
        selectedTargetIndex = EditorGUILayout.Popup("選択中", selectedTargetIndex, options);

        // 一覧 + 個別操作
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

    /// <summary>
    /// ダメージ量/種別の設定、および実行ボタン
    /// </summary>
    private void DrawDamageExecuteArea()
    {
        EditorGUILayout.LabelField("ダメージ設定", EditorStyles.boldLabel);

        // ダメージ量入力
        damageAmount = EditorGUILayout.IntField("ダメージ量", damageAmount);

        // マイナスダメージなどの入力を防ぐ
        if (damageAmount < 0) damageAmount = 0;

        // ダメージ種別選択（enum取得できない場合は文字入力欄にフォールバック）
        DrawDamageTypePopup();

        EditorGUILayout.Space(4);

        // TakeDamageはゲームロジックなので、再生中のみ実行できるよう制限
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("再生中のみ実行できます。", MessageType.Info);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                // 選択中の対象へダメージ
                using (new EditorGUI.DisabledScope(!HasValidSelectedTarget()))
                {
                    if (GUILayout.Button("選択中にダメージ"))
                    {
                        var t = targets[selectedTargetIndex];
                        if (t != null) TryInvokeTakeDamage(t);
                    }
                }

                // リスト全員へダメージ
                using (new EditorGUI.DisabledScope(targets.Count == 0))
                {
                    if (GUILayout.Button("リスト全員にダメージ"))
                    {
                        for (int i = 0 ; i < targets.Count ; i++)
                        {
                            var t = targets[i];
                            if (t == null) continue;
                            TryInvokeTakeDamage(t);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// ダメージ種別（リアクション種別）のPopup表示
    /// </summary>
    private void DrawDamageTypePopup()
    {
        // ThiefReactionUIType enum を取得
        var reactionEnumType = FindTypeByName("CS_ThiefReactionUI+ThiefReactionUIType");
        if (reactionEnumType != null) {
            // enumが見つかった場合は、選択中の種別名 -> enum値へ変換して保存
            damageReactionTypeName = ToGimmickValue(reactionEnumType, damageReactionTypeName)?.ToString() ?? damageReactionTypeName;
        }
        else {
        }

        // enumが取れない場合は入力欄にフォールバック
        if (reactionEnumType == null || !reactionEnumType.IsEnum)
        {
            EditorGUILayout.HelpBox("ダメージ種別(enum)を取得できません。", MessageType.Warning);
            damageReactionTypeName = EditorGUILayout.TextField("ギミック(名前)", damageReactionTypeName);
            return;
        }

        // enum名一覧からPopup
        var names = Enum.GetNames(reactionEnumType);
        int index = Array.IndexOf(names, damageReactionTypeName);
        if (index < 0) index = 0;
        index = EditorGUILayout.Popup("ギミック", index, names);
        damageReactionTypeName = names[index];
    }

    /// <summary>
    /// TakeDamage(int, Gimmick) をReflectionで呼び出す
    /// </summary>
    private void TryInvokeTakeDamage(MonoBehaviour thief)
    {
        if (thief == null) return;
        if (thief.GetType().Name != "CS_ThiefAI") return;

        // Gimmick enum を取得
        var gimmickEnumType = FindTypeByName("Gimmick");
        if (gimmickEnumType == null || !gimmickEnumType.IsEnum)
        {
            Debug.LogWarning("Gimmick enum が見つからないため TakeDamage を実行できません。");
            return;
        }

        // 選択中の種別名 -> Gimmick enum値へ変換
        object gimmickValue = ToGimmickValue(gimmickEnumType, damageReactionTypeName);
        if (gimmickValue == null)
        {
            Debug.LogWarning("ギミック変換に失敗しました: " + damageReactionTypeName);
            return;
        }

        // メソッド取得）
        var method = thief.GetType().GetMethod("TakeDamage");
        if (method == null)
        {
            Debug.LogWarning("TakeDamage(int, Gimmick) が見つかりません。");
            return;
        }

        Vector3 gimmickPoint = new Vector3(0, 0, 0); // ギミックポイントは仮に(0,0,0)を渡す。

        method.Invoke(thief, new[] { (object)damageAmount, gimmickValue, gimmickPoint, true });
    }

    /// <summary>
    /// 表示用の種別名を、Gimmick enum値へ変換する
    /// </summary>
    private static object ToGimmickValue(Type gimmickEnumType, string reactionTypeName)
    {
        // 種別名 -> ギミック名（命名が同一という前提。例：Pot/IronBall）
        var gimmickName = reactionTypeName;

        if (Enum.IsDefined(gimmickEnumType, gimmickName))
        {
            return Enum.Parse(gimmickEnumType, gimmickName);
        }

        // 無ければ None を試し、それも無ければ0
        if (Enum.IsDefined(gimmickEnumType, "None"))
        {
            return Enum.Parse(gimmickEnumType, "None");
        }

        try
        {
            return Enum.ToObject(gimmickEnumType, 0);
        }
        catch
        {
            return null;
        }
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
    /// targets 内の null（Destroy済みなど）を削除して、安全に操作できる状態に整える
    /// </summary>
    private void CleanupNullTargets()
    {
        for (int i = targets.Count - 1 ; i >= 0 ; i--)
        {
            if (targets[i] == null) targets.RemoveAt(i);
        }

        if (targets.Count == 0) selectedTargetIndex = -1;
        else selectedTargetIndex = Mathf.Clamp(selectedTargetIndex, 0, targets.Count - 1);
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
