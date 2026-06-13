using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectPoolSetting.cs
 概要     : EffectPrefab側のPool設定を保持するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/11 新規作成
=====================================+
*/

/// <summary>
/// EffectPrefab側のPool設定を保持するクラスです。
/// Effectの再生処理とは分離して、Pool管理用の設定だけを持ちます。
/// </summary>
public class CS_EffectPoolSetting : MonoBehaviour
{
    [Header("Poolを使うか")]
    [SerializeField]
    private bool b_UsePool = true;

    [Header("Poolに保持する最大数")]
    [SerializeField]
    private int n_MaxPoolCount = 3;

    /// <summary>
    /// Poolを使うかどうかを取得します。
    /// </summary>
    /// <returns>Poolを使う場合はtrue。</returns>
    public bool IsUsePool()
    {
        return b_UsePool;
    }

    /// <summary>
    /// Poolに保持する最大数を取得します。
    /// </summary>
    /// <returns>Pool最大数。</returns>
    public int GetMaxPoolCount()
    {
        return n_MaxPoolCount;
    }
}
