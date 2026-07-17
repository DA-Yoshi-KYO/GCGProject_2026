using UnityEngine;

/*
+=====================================
 ファイル名 : CS_PotVATVisualSwitcher.cs
 概要     : 壺の通常Modelと破壊VAT Modelを切り替えるクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/07/17 新規作成
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
    /// 初期状態をひびなし通常壺にします。
    /// </summary>
    private void Awake()
    {
        ShowNormalPot();
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
    /// </summary>
    public void PlayDestructionVAT()
    {
        if (go_NormalPotModel != null)
        {
            go_NormalPotModel.SetActive(false);
        }

        if (go_VATPotModel != null)
        {
            go_VATPotModel.SetActive(true);
        }

        if (cs_PotVATEffect == null)
        {
            Debug.LogWarning(
                "[CS_PotVATVisualSwitcher] " +
                "CS_EffectVATが設定されていません。",
                this);

            return;
        }

        CSST_EffectPlayData csst_EffectPlayData =
            new CSST_EffectPlayData();

        csst_EffectPlayData.CSST_EffectPlayData_Init();

        cs_PotVATEffect.PlayEffect(csst_EffectPlayData);
    }
}
