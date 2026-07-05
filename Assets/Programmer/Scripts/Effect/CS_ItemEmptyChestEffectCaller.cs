using UnityEngine;

/*
+=====================================
 ファイル名 : CS_ItemEmptyChestEffectCaller.cs
 概要     : ItemからEffectをStart時に再生するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/07/05 新規作成
=====================================+
*/

/// <summary>
/// ItemからEffectをStart時に再生するクラスです。
/// 同じGameObjectにあるCS_EffectPlayerを使います。
/// </summary>
public class CS_ItemEmptyChestEffectCaller : MonoBehaviour
{
    [Header("Effect再生Facade")]
    [SerializeField]
    private CS_EffectPlayer cs_EffectPlayer;

    private void Awake()
    {
        if (cs_EffectPlayer == null)
        {
            cs_EffectPlayer = GetComponent<CS_EffectPlayer>();
        }
    }

    private void Start()
    {
        PlayEffect();
    }

    /// <summary>
    /// Effectを再生します。
    /// </summary>
    public void PlayEffect()
    {
        if (cs_EffectPlayer == null)
        {
            Debug.LogWarning("[CS_ItemEmptyChestEffectCaller] CS_EffectPlayerがありません。");
            return;
        }

        CSST_EffectPlayData csst_EffectPlayData = new CSST_EffectPlayData();
        csst_EffectPlayData.CSST_EffectPlayData_Init();

        cs_EffectPlayer.PlayEffect(csst_EffectPlayData);
    }
}
