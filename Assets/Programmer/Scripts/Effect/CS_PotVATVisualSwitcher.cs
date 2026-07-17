using UnityEngine;

/*
+=====================================
 ファイル名 : CS_PotVATVisualSwitcher.cs
 概要     : 壺の通常Modelと破壊VAT Modelを切り替えるクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/07/17 新規作成
             2026/07/17 RootScale固定処理を追加
             2026/07/17 Broken状態検知によるVAT自動再生を追加
=====================================+
*/

/// <summary>
/// 壺の通常表示とVAT破壊表示を切り替えます。
/// PotGimmickがBroken状態になった際、自動でVATを再生します。
/// </summary>
[DefaultExecutionOrder(-1000)]
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

    [Header("Effect再生Facade")]
    [SerializeField]
    private CS_EffectPlayer cs_EffectPlayer;

    [Header("Root Scaleを固定するか")]
    [SerializeField]
    private bool b_LockRootLocalScale = true;

    /// <summary>
    /// この壺のGimmick処理です。
    /// </summary>
    private PotGimmick cs_PotGimmick;

    /// <summary>
    /// Prefabに設定されていたRoot LocalScaleです。
    /// </summary>
    private Vector3 v3_DefaultRootLocalScale;

    /// <summary>
    /// VAT Object本来のLocalScaleです。
    /// </summary>
    private Vector3 v3_DefaultVATLocalScale;

    /// <summary>
    /// VATを再生済みかどうかです。
    /// </summary>
    private bool b_IsDestructionVATPlayed;

    /// <summary>
    /// 初期化処理です。
    /// Instantiate直後のPrefab Scaleを保存します。
    /// </summary>
    private void Awake()
    {
        v3_DefaultRootLocalScale =
            transform.localScale;

        cs_PotGimmick =
            GetComponent<PotGimmick>();

        if (cs_EffectPlayer == null)
        {
            cs_EffectPlayer =
                GetComponent<CS_EffectPlayer>();
        }

        if (cs_PotVATEffect != null)
        {
            v3_DefaultVATLocalScale =
                cs_PotVATEffect.transform.localScale;
        }
        else
        {
            v3_DefaultVATLocalScale =
                Vector3.one;
        }

        ShowNormalPot();
    }

    /// <summary>
    /// GimmickBase.Startより先にRoot Scaleを戻します。
    /// </summary>
    private void Start()
    {
        ApplyDefaultRootLocalScale();
    }

    /// <summary>
    /// PotGimmickの状態を確認します。
    /// Brokenになった瞬間にVATを再生します。
    /// </summary>
    private void Update()
    {
        if (b_IsDestructionVATPlayed)
        {
            return;
        }

        if (cs_PotGimmick == null)
        {
            return;
        }

        if (cs_PotGimmick.gimmickState !=
            GimmickState.Broken)
        {
            return;
        }

        PlayDestructionVAT();
    }

    /// <summary>
    /// 他処理によってRoot Scaleが変更された場合に戻します。
    /// </summary>
    private void LateUpdate()
    {
        ApplyDefaultRootLocalScale();
    }

    /// <summary>
    /// Physics処理時にもRoot Scaleを維持します。
    /// </summary>
    private void FixedUpdate()
    {
        ApplyDefaultRootLocalScale();
    }

    /// <summary>
    /// ひびなしの通常壺を表示します。
    /// </summary>
    public void ShowNormalPot()
    {
        b_IsDestructionVATPlayed = false;

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
    /// 通常壺を非表示にして破壊VATを再生します。
    /// </summary>
    public void PlayDestructionVAT()
    {
        if (b_IsDestructionVATPlayed)
        {
            return;
        }

        if (go_VATPotModel == null)
        {
            Debug.LogWarning(
                "[CS_PotVATVisualSwitcher] " +
                "ひびあり破壊VAT Modelが設定されていません。",
                this);

            return;
        }

        if (cs_PotVATEffect == null)
        {
            Debug.LogWarning(
                "[CS_PotVATVisualSwitcher] " +
                "CS_EffectVATが設定されていません。",
                this);

            return;
        }

        b_IsDestructionVATPlayed = true;

        // 通常モデルを非表示にします。
        if (go_NormalPotModel != null)
        {
            go_NormalPotModel.SetActive(false);
        }

        // VATモデルを表示します。
        go_VATPotModel.SetActive(true);

        // 非アクティブ解除後に本来のScaleを戻します。
        cs_PotVATEffect.transform.localScale =
            v3_DefaultVATLocalScale;

        CSST_EffectPlayData csst_EffectPlayData =
            new CSST_EffectPlayData();

        csst_EffectPlayData.CSST_EffectPlayData_Init();

        csst_EffectPlayData.SetPosition(
            cs_PotVATEffect.transform.position);

        csst_EffectPlayData.SetRotation(
            cs_PotVATEffect.transform.rotation);

        csst_EffectPlayData.SetScale(
            v3_DefaultVATLocalScale);

        csst_EffectPlayData.SetLoopFlag(false);

        // 壺本体がBroken処理中なので、
        // Effect側だけ勝手に非表示にしないようにします。
        csst_EffectPlayData.SetHideOnEnd(false);

        if (cs_EffectPlayer != null)
        {
            // Hierarchy上にある既存のVATをそのまま再生します。
            cs_EffectPlayer.PlayExistingEffect(
                cs_PotVATEffect,
                csst_EffectPlayData);
        }
        else
        {
            // Facadeが設定されていない場合の予備処理です。
            cs_PotVATEffect.gameObject.SetActive(true);

            cs_PotVATEffect.PlayEffect(
                csst_EffectPlayData);
        }

        ApplyDefaultRootLocalScale();

        Debug.Log(
            "[CS_PotVATVisualSwitcher] " +
            "壺破壊VATを再生しました。",
            this);
    }

    /// <summary>
    /// PotVatのRoot LocalScaleをPrefab時の値へ戻します。
    /// </summary>
    private void ApplyDefaultRootLocalScale()
    {
        if (b_LockRootLocalScale == false)
        {
            return;
        }

        transform.localScale =
            v3_DefaultRootLocalScale;
    }
}
