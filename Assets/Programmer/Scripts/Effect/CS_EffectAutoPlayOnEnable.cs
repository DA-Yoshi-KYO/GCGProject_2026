/*
+=====================================
 ファイル名 : CS_EffectAutoPlayOnEnable.cs
 概要     : Hierarchy上に配置済みのエフェクトを有効化時に自動再生する
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
=====================================+
*/

using UnityEngine;

/// <summary>
/// Hierarchy上に配置済みのエフェクトを、OnEnable時に自動再生するコンポーネントです。
/// </summary>
[DisallowMultipleComponent]
public class CS_EffectAutoPlayOnEnable : MonoBehaviour
{
    /// <summary>
    /// 有効化時にエフェクトを再生します。
    /// </summary>
    private void OnEnable()
    {
        CS_EffectRoot effectRoot = GetComponent<CS_EffectRoot>();

        if (effectRoot == null)
        {
            Debug.LogWarning("[EffectAutoPlayOnEnable] CS_EffectRoot が見つかりません : " + gameObject.name);
            return;
        }

        CSI_EffectPlayable effectPlayable = effectRoot.GetEffectPlayable();

        if (effectPlayable == null)
        {
            Debug.LogWarning("[EffectAutoPlayOnEnable] CSI_EffectPlayable が見つかりません : " + gameObject.name);
            return;
        }

        effectPlayable.SetPlaySpeed(effectRoot.DefaultPlaySpeed);
        effectPlayable.PlayEffect();
    }
}
