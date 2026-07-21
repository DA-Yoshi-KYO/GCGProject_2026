using UnityEngine;

/*
+=====================================
 ファイル名 : CS_PlayerInteractRangeEffectPlayer.cs
 概要     : Playerのインタラクト範囲Effect再生クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/12 新規作成
=====================================+
*/

/// <summary>
/// Playerのインタラクト範囲Effectを再生するクラスです。
/// CS_PlayerActionからEffect処理を分離します。
/// </summary>
public class CS_PlayerInteractRangeEffectPlayer : MonoBehaviour
{
    private const string INTERACT_RANGE_EFFECT_ROOT_NAME = "InteractRangeEffectRoot";

    [Header("インタラクト範囲EffectPrefab")]
    [SerializeField]
    private GameObject go_InteractRangeEffectPrefab;

    [Header("Effect位置補正")]
    [SerializeField]
    private Vector3 v3_EffectPositionOffset = Vector3.zero;

    [Header("EffectScale倍率")]
    [SerializeField]
    private Vector3 v3_EffectScaleMultiplier = Vector3.one;

    /// <summary>
    /// 現在再生中の範囲Effectです。
    /// </summary>
    private CSAD_EffectCommonProcessBase csad_CurrentRangeEffect;

    /// <summary>
    /// 範囲Effect用Rootです。
    /// </summary>
    private Transform tr_InteractRangeEffectRoot;

    /// <summary>
    /// インタラクト範囲Effectを再生します。
    /// </summary>
    /// <param name="tr_RangeField">範囲表示用Transform。</param>
    public void PlayInteractRangeEffect(Transform tr_RangeField)
    {
        if (go_InteractRangeEffectPrefab == null)
        {
            Debug.LogWarning("[CS_PlayerInteractRangeEffectPlayer] InteractRangeEffectPrefabが設定されていません。");
            return;
        }

        if (tr_RangeField == null)
        {
            return;
        }

        if (csad_CurrentRangeEffect != null &&
            csad_CurrentRangeEffect.gameObject.activeInHierarchy)
        {
            UpdateInteractRangeEffect(tr_RangeField);
            return;
        }

        Vector3 v3_EffectPosition =
            tr_RangeField.position + v3_EffectPositionOffset;

        Quaternion q_EffectRotation =
            go_InteractRangeEffectPrefab.transform.rotation;

        Vector3 v3_EffectScale =
            GetEffectScale(tr_RangeField.lossyScale);

        csad_CurrentRangeEffect =
            CS_EffectFactory.CreateEffect(
                go_InteractRangeEffectPrefab,
                v3_EffectPosition,
                q_EffectRotation,
                GetInteractRangeEffectRoot());

        if (csad_CurrentRangeEffect == null)
        {
            return;
        }

        csad_CurrentRangeEffect.SetOnEffectEndAction(DestroyRangeEffect);

        CSST_EffectPlayData csst_EffectPlayData = new CSST_EffectPlayData();
        csst_EffectPlayData.CSST_EffectPlayData_Init();

        csst_EffectPlayData.SetPosition(v3_EffectPosition);
        csst_EffectPlayData.SetRotation(q_EffectRotation);
        csst_EffectPlayData.SetScale(v3_EffectScale);

        csad_CurrentRangeEffect.PlayEffect(csst_EffectPlayData);
    }

    /// <summary>
    /// インタラクト範囲Effectの位置とサイズを更新します。
    /// </summary>
    /// <param name="tr_RangeField">範囲表示用Transform。</param>
    public void UpdateInteractRangeEffect(Transform tr_RangeField)
    {
        if (csad_CurrentRangeEffect == null)
        {
            return;
        }

        if (csad_CurrentRangeEffect.gameObject.activeInHierarchy == false)
        {
            csad_CurrentRangeEffect = null;
            return;
        }

        if (tr_RangeField == null)
        {
            return;
        }

        csad_CurrentRangeEffect.transform.position =
            tr_RangeField.position + v3_EffectPositionOffset;

        csad_CurrentRangeEffect.transform.localScale =
            GetEffectScale(tr_RangeField.lossyScale);
    }

    /// <summary>
    /// インタラクト範囲Effectの終了処理を走らせます。
    /// </summary>
    public void EndInteractRangeEffect()
    {
        if (csad_CurrentRangeEffect == null)
        {
            return;
        }

        csad_CurrentRangeEffect.EndEffect();
        csad_CurrentRangeEffect = null;
    }

    /// <summary>
    /// interactFieldのScaleからEffect用Scaleを作ります。
    /// EffectはScale1のRoot配下に生成されるため、
    /// localScaleではなくワールドスケール(lossyScale)を基準にします。
    /// </summary>
    /// <param name="v3_RangeScale">範囲表示用Scale。</param>
    /// <returns>Effect用Scale。</returns>
    private Vector3 GetEffectScale(Vector3 v3_RangeScale)
    {
        return new Vector3(
            v3_RangeScale.x * v3_EffectScaleMultiplier.x,
            v3_RangeScale.y * v3_EffectScaleMultiplier.y,
            v3_RangeScale.z * v3_EffectScaleMultiplier.z);
    }

    /// <summary>
    /// 範囲Effect用Rootを取得します。
    /// </summary>
    /// <returns>範囲Effect用Root。</returns>
    private Transform GetInteractRangeEffectRoot()
    {
        if (tr_InteractRangeEffectRoot != null)
        {
            return tr_InteractRangeEffectRoot;
        }

        GameObject go_Root = GameObject.Find(INTERACT_RANGE_EFFECT_ROOT_NAME);

        if (go_Root == null)
        {
            go_Root = new GameObject(INTERACT_RANGE_EFFECT_ROOT_NAME);
        }

        tr_InteractRangeEffectRoot = go_Root.transform;

        return tr_InteractRangeEffectRoot;
    }

    /// <summary>
    /// Effect終了時に破棄します。
    /// </summary>
    /// <param name="csad_Effect">終了したEffect。</param>
    private void DestroyRangeEffect(CSAD_EffectCommonProcessBase csad_Effect)
    {
        if (csad_Effect == null)
        {
            return;
        }

        Destroy(csad_Effect.gameObject);
    }
}
