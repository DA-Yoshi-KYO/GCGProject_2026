using UnityEngine;

/*
+=====================================
 ファイル名 : CS_PotVATVisualSwitcher.cs
 概要     : 壺の通常Modelと破壊VAT Modelを切り替えるクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/07/17 新規作成
             2026/07/17 VAT再生時のTransform上書きを防止
=====================================+
*/

/// <summary>
/// 壺の通常表示とVAT破壊表示を切り替えます。
/// </summary>
public class CS_PotVATVisualSwitcher : MonoBehaviour
{
    [Header("ひびなし通常壺Model")]
    [SerializeField]
    private GameObject go_NormalPotModel;

    [Header("ひびあり破壊VAT Model")]
    [SerializeField]
    private GameObject go_VATPotModel;

    [Header("壺破壊VAT")]
    [SerializeField]
    private CS_EffectVAT cs_PotVATEffect;

    /// <summary>
    /// VAT Effectの初期LocalScaleです。
    /// Effect再生時にScaleが上書きされるのを防ぐため保存します。
    /// </summary>
    private Vector3 v3_DefaultVATLocalScale = Vector3.one;

    /// <summary>
    /// VAT EffectのScaleを保存済みかどうかです。
    /// </summary>
    private bool b_IsVATScaleCached;

    /// <summary>
    /// 初期化処理です。
    /// VATの元サイズを保存し、通常壺を表示します。
    /// </summary>
    private void Awake()
    {
        CacheVATLocalScale();

        ShowNormalPot();
    }

    /// <summary>
    /// VAT Effectの初期LocalScaleを保存します。
    /// </summary>
    private void CacheVATLocalScale()
    {
        if (cs_PotVATEffect == null)
        {
            return;
        }

        v3_DefaultVATLocalScale =
            cs_PotVATEffect.transform.localScale;

        b_IsVATScaleCached = true;
    }

    /// <summary>
    /// ひびなしの通常壺を表示します。
    /// </summary>
    public void ShowNormalPot()
    {
        if (go_NormalPotModel != null)
        {
            go_NormalPotModel.SetActive(true);
        }

        if (go_VATPotModel != null)
        {
            go_VATPotModel.SetActive(false);
        }
    }

    /// <summary>
    /// 通常壺を隠し、ひびありVATを再生します。
    /// VAT再生前の位置・回転・サイズを維持します。
    /// </summary>
    public void PlayDestructionVAT()
    {
        if (cs_PotVATEffect == null)
        {
            Debug.LogWarning(
                "[CS_PotVATVisualSwitcher] " +
                "CS_EffectVATが設定されていません。",
                this);

            return;
        }

        if (b_IsVATScaleCached == false)
        {
            CacheVATLocalScale();
        }

        if (go_NormalPotModel != null)
        {
            go_NormalPotModel.SetActive(false);
        }

        if (go_VATPotModel != null)
        {
            go_VATPotModel.SetActive(true);
        }

        // Effect共通処理によってTransformが上書きされないよう、
        // 現在の位置・回転と、保存した元のLocalScaleを明示的に渡します。
        Vector3 v3_VATPosition =
            cs_PotVATEffect.transform.position;

        Quaternion q_VATRotation =
            cs_PotVATEffect.transform.rotation;

        CSST_EffectPlayData csst_EffectPlayData =
            new CSST_EffectPlayData();

        csst_EffectPlayData.CSST_EffectPlayData_Init();

        csst_EffectPlayData.SetPosition(
            v3_VATPosition);

        csst_EffectPlayData.SetRotation(
            q_VATRotation);

        csst_EffectPlayData.SetScale(
            v3_DefaultVATLocalScale);

        cs_PotVATEffect.PlayEffect(
            csst_EffectPlayData);
    }
}
