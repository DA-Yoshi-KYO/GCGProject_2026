/*
+=====================================
 ファイル名 : CSS_EffectRegistry.cs
 概要     : 使用可能なエフェクトPrefab一覧を保持するScriptableObject
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
=====================================+
*/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 使用可能なエフェクトPrefab一覧を保持するScriptableObjectです。
/// </summary>
[CreateAssetMenu(fileName = "CSS_EffectRegistry", menuName = "Effect/CSS_EffectRegistry")]
public class CSS_EffectRegistry : ScriptableObject
{
    [Header("登録済みエフェクト一覧")]
    [SerializeField]
    private List<CSS_EffectData> list_EffectData = new List<CSS_EffectData>();

    /// <summary>
    /// 登録済みエフェクト一覧を取得します。
    /// </summary>
    public IReadOnlyList<CSS_EffectData> EffectDataList => list_EffectData;

    /// <summary>
    /// 登録済みエフェクト一覧を全て削除します。
    /// </summary>
    public void Clear()
    {
        list_EffectData.Clear();
    }

    /// <summary>
    /// エフェクト情報を追加します。
    /// </summary>
    /// <param name="effectData">追加するエフェクト情報。</param>
    public void AddEffectData(CSS_EffectData effectData)
    {
        if (effectData == null)
        {
            return;
        }

        list_EffectData.Add(effectData);
    }

    /// <summary>
    /// エフェクト名からPrefabを取得します。
    /// </summary>
    /// <param name="effectName">検索するエフェクト名。</param>
    /// <returns>見つかったEffectPrefab。見つからない場合はnull。</returns>
    public CS_EffectRoot FindEffectPrefab(string effectName)
    {
        CSS_EffectData effectData = FindEffectData(effectName);

        if (effectData == null)
        {
            return null;
        }

        return effectData.EffectPrefab;
    }

    /// <summary>
    /// エフェクト名からエフェクト情報を取得します。
    /// </summary>
    /// <param name="effectName">検索するエフェクト名。</param>
    /// <returns>見つかったEffectData。見つからない場合はnull。</returns>
    public CSS_EffectData FindEffectData(string effectName)
    {
        for (int i = 0 ; i < list_EffectData.Count ; i++)
        {
            CSS_EffectData effectData = list_EffectData[i];

            if (effectData == null)
            {
                continue;
            }

            if (effectData.EffectName == effectName)
            {
                return effectData;
            }
        }

        return null;
    }
}
