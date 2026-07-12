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
    [Tooltip("与えるダメージ量")]
    private int damageAmount = 1;

    [Tooltip("ダメージを与えるギミック種類")]
    private string damageReactionTypeName = "Pot";

    /// <summary>
    /// タブ全体描画
    /// </summary>
    public void Draw(List<MonoBehaviour> targets, int selectedTargetIndex)
    {
        EditorGUILayout.LabelField("ダメージ", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
        "登録した対象(泥棒AI)に対して、指定したダメージを与えます。\n" +
        "実行中に対象を追加/選択できます。",
        MessageType.Info);

        if (targets.Count == 0)
        {
            EditorGUILayout.HelpBox("対象がありません。上の『追加』から登録してください。", MessageType.Warning);
            return;
        }

        // ダメージ設定/実行
        DrawDamageExecuteArea(targets, selectedTargetIndex);
    }

    /// <summary>
    /// ダメージ量/種別の設定、および実行ボタン
    /// </summary>
    private void DrawDamageExecuteArea(List<MonoBehaviour> targets, int selectedTargetIndex)
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
                using (new EditorGUI.DisabledScope(!HasValidSelectedTarget(targets, selectedTargetIndex)))
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
                        for (int i = 0; i < targets.Count; i++)
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
        var reactionEnumType = CSED_ThiefDebug.FindTypeByName("CS_ThiefReactionUI+ThiefReactionUIType");
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
        var gimmickEnumType = CSED_ThiefDebug.FindTypeByName("Gimmick");
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
    private bool HasValidSelectedTarget(List<MonoBehaviour> targets, int selectedTargetIndex)
    {
        return selectedTargetIndex >= 0
        && selectedTargetIndex < targets.Count
        && targets[selectedTargetIndex] != null;
    }
}
#endif
