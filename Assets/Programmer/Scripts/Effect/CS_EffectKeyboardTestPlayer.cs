/*
+=====================================
 ファイル名 : CS_EffectKeyboardTestPlayer.cs
 概要     : キーボード入力でEffectRootの再生・停止をテストする
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
=====================================+
*/

using UnityEngine;

/// <summary>
/// キーボード入力でエフェクトの再生・停止を確認するテスト用クラスです。
/// </summary>
public class CS_EffectKeyboardTestPlayer : MonoBehaviour
{
    [Header("テスト対象エフェクト")]
    [SerializeField]
    private CS_EffectRoot cs_EffectRoot;

    [Header("再生キー")]
    [SerializeField]
    private KeyCode playKey = KeyCode.Return;

    [Header("停止キー")]
    [SerializeField]
    private KeyCode stopKey = KeyCode.Space;

    /// <summary>
    /// 毎フレーム、入力を確認します。
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(playKey))
        {
            PlayEffect();
        }

        if (Input.GetKeyDown(stopKey))
        {
            StopEffect();
        }
    }

    /// <summary>
    /// テスト対象のエフェクトを再生します。
    /// </summary>
    private void PlayEffect()
    {
        if (cs_EffectRoot == null)
        {
            Debug.LogWarning("[EffectKeyboardTestPlayer] CS_EffectRoot が設定されていません。");
            return;
        }

        if (!cs_EffectRoot.gameObject.activeSelf)
        {
            cs_EffectRoot.gameObject.SetActive(true);
        }

        CSI_EffectPlayable effectPlayable = cs_EffectRoot.GetEffectPlayable();

        if (effectPlayable == null)
        {
            Debug.LogWarning("[EffectKeyboardTestPlayer] CSI_EffectPlayable が見つかりません : " + cs_EffectRoot.name);
            return;
        }

        effectPlayable.SetPlaySpeed(cs_EffectRoot.DefaultPlaySpeed);
        effectPlayable.PlayEffect();

        Debug.Log("[EffectKeyboardTestPlayer] エフェクトを再生しました : " + cs_EffectRoot.name);
    }

    /// <summary>
    /// テスト対象のエフェクトを停止します。
    /// </summary>
    private void StopEffect()
    {
        if (cs_EffectRoot == null)
        {
            Debug.LogWarning("[EffectKeyboardTestPlayer] CS_EffectRoot が設定されていません。");
            return;
        }

        if (!cs_EffectRoot.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[EffectKeyboardTestPlayer] 対象エフェクトが非アクティブなので停止処理を呼べません : " + cs_EffectRoot.name);
            return;
        }

        CSI_EffectPlayable effectPlayable = cs_EffectRoot.GetEffectPlayable();

        if (effectPlayable == null)
        {
            Debug.LogWarning("[EffectKeyboardTestPlayer] CSI_EffectPlayable が見つかりません : " + cs_EffectRoot.name);
            return;
        }

        effectPlayable.StopEffect();

        Debug.Log("[EffectKeyboardTestPlayer] エフェクトを停止しました : " + cs_EffectRoot.name);
    }
}
