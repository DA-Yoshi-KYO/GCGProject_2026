using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectPoolRoot.cs
 概要     : Effect用ObjectPoolRootを取得・作成するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/07/09 新規作成
=====================================+
*/

/// <summary>
/// Effect用ObjectPoolRootを管理するクラスです。
/// Poolに戻ったEffectは ObjectPoolList_Effect の子にまとめます。
/// </summary>
public static class CS_EffectPoolRoot
{
    private const string EFFECT_POOL_ROOT_NAME = "ObjectPoolList_Effect";

    /// <summary>
    /// EffectPool用Rootです。
    /// </summary>
    private static Transform tr_EffectPoolRoot;

    /// <summary>
    /// EffectPool用Rootを取得します。
    /// Scene上に無ければ作成します。
    /// </summary>
    /// <returns>EffectPool用Root。</returns>
    public static Transform GetPoolRoot()
    {
        if (tr_EffectPoolRoot != null)
        {
            return tr_EffectPoolRoot;
        }

        GameObject go_Root = GameObject.Find(EFFECT_POOL_ROOT_NAME);

        if (go_Root == null)
        {
            go_Root = new GameObject(EFFECT_POOL_ROOT_NAME);
        }

        tr_EffectPoolRoot = go_Root.transform;

        return tr_EffectPoolRoot;
    }
}
