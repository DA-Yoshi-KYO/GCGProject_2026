/*
+=====================================
 ファイル名 : CSS_EffectData.cs
 概要     : EffectRegistryに登録する1つ分のエフェクト情報を保持する
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
=====================================+
*/

using System;
using UnityEngine;

/// <summary>
/// EffectRegistryに登録する1つ分のエフェクト情報です。
/// </summary>
[Serializable]
public class CSS_EffectData
{
    [Header("エフェクト名")]
    [SerializeField]
    private string effectName;

    [Header("エフェクト種別")]
    [SerializeField]
    private CSE_EffectType effectType;

    [Header("エフェクトPrefab")]
    [SerializeField]
    private CS_EffectRoot cs_EffectPrefab;

    /// <summary>
    /// エフェクト名を取得します。
    /// </summary>
    public string EffectName => effectName;

    /// <summary>
    /// エフェクト種別を取得します。
    /// </summary>
    public CSE_EffectType EffectType => effectType;

    /// <summary>
    /// エフェクトPrefabを取得します。
    /// </summary>
    public CS_EffectRoot EffectPrefab => cs_EffectPrefab;

    /// <summary>
    /// エフェクト情報を設定します。
    /// </summary>
    /// <param name="effectName">エフェクト名。</param>
    /// <param name="effectType">エフェクト種別。</param>
    /// <param name="cs_EffectPrefab">エフェクトPrefab。</param>
    public void SetData(string effectName, CSE_EffectType effectType, CS_EffectRoot cs_EffectPrefab)
    {
        this.effectName = effectName;
        this.effectType = effectType;
        this.cs_EffectPrefab = cs_EffectPrefab;
    }
}
