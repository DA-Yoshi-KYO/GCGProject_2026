/*
+=====================================
 ファイル名 : CS_EffectRoot.cs
 概要     : エフェクトPrefab共通のRoot情報を保持する
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
            2026/06/03 デフォルト生存時間を無限にできる設定を追加
            2026/06/03 自動再生設定を別コンポーネントへ分離
=====================================+
*/

using UnityEngine;

/// <summary>
/// エフェクトPrefabのRootに付ける共通コンポーネントです。
/// </summary>
[DisallowMultipleComponent]
public class CS_EffectRoot : MonoBehaviour
{
    [Header("エフェクト種別")]
    [SerializeField]
    private CSE_EffectType effectType = CSE_EffectType.CustomCS;

    [Header("デフォルト再生速度")]
    [SerializeField]
    private float defaultPlaySpeed = 1.0f;

    [Header("デフォルト生存時間を無限にするか")]
    [SerializeField]
    private bool isDefaultLifeTimeInfinite = true;

    [Header("デフォルト生存時間")]
    [SerializeField]
    [Min(0.0f)]
    private float defaultLifeTime = 3.0f;

    /// <summary>
    /// エフェクト種別を取得します。
    /// </summary>
    public CSE_EffectType EffectType => effectType;

    /// <summary>
    /// デフォルト再生速度を取得します。
    /// </summary>
    public float DefaultPlaySpeed => defaultPlaySpeed;

    /// <summary>
    /// デフォルト生存時間を無限にするか取得します。
    /// </summary>
    public bool IsDefaultLifeTimeInfinite => isDefaultLifeTimeInfinite;

    /// <summary>
    /// デフォルト生存時間を取得します。
    /// </summary>
    public float DefaultLifeTime => defaultLifeTime;

    /// <summary>
    /// デフォルト設定で自動削除する必要があるかを取得します。
    /// </summary>
    public bool ShouldAutoDestroyByDefault
    {
        get
        {
            if (isDefaultLifeTimeInfinite)
            {
                return false;
            }

            return defaultLifeTime > 0.0f;
        }
    }

    /// <summary>
    /// このエフェクトRootから再生制御コンポーネントを取得します。
    /// </summary>
    /// <returns>再生制御コンポーネント。見つからない場合はnull。</returns>
    public CSI_EffectPlayable GetEffectPlayable()
    {
        return GetComponentInChildren<CSI_EffectPlayable>(true);
    }
}
