
/*
+=====================================
 ファイル名 : CSAD_EffectCommonProcessBase.cs
 概要     : Effectの共通処理の基底クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
=====================================+
*/

using UnityEngine;
using UnityEngine.UIElements;

public abstract class CSAD_EffectCommonProcessBase
{
    [Header("生成タイプ"), SerializeField]
    private CSE_EffectType cs_EffectType = CSE_EffectType.None;

    [Header("再生完了に必要な時間"), SerializeField]
    private float EffectPlayTime = 1.0f;

    [Header("削除に必要な時間"), SerializeField]
    private float EffectEndTime = 1.0f;

    [Header("Effectのループフラグ"), SerializeField]
    private bool EffectLoopFlag = false;

    [Header("Effectの生成から消えるまでの時間"), SerializeField]
    private float EffectPlayEndTime = 1.0f;

    /// <summary>
    /// 受け取ったEffectの再生に必要なデータを格納するクラス
    /// </summary>
    private CSST_EffectPlayData csst_EffectPlayData;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public virtual void InitEffect()
    {

    }

    /// <summary>
    /// 再生処理(引数なしdefault再生)
    /// </summary>
    public void PlayEffect(CSST_EffectPlayData csst_effectData)
    {
        csst_EffectPlayData = csst_effectData;
    }

    /// <summary>
    /// 終了処理(引数なしdefault再生)
    /// </summary>
    public void EndEffect()
    {
    }
}
