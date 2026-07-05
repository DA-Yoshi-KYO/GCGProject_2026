using UnityEngine;

public class MagicAnkhGimmick : GimmickBase
{
    [Header("アンクの発動時間")]
    [SerializeField]
    private float f_ActiveDuration = 2.0f;

    [Header("アンク発動時Effect")]
    [SerializeField]
    private GameObject go_AnkhEffectPrefab;

    [Header("Effect位置Offset")]
    [SerializeField]
    private Vector3 v3_EffectOffset = Vector3.zero;

    private float f_CurrentActiveTime = 0.0f;

    private bool isActiveFirst = false;
    private bool isBrokenFirst = false;

    private CSAD_EffectCommonProcessBase csad_AnkhEffect;

    protected override void IdleUpdate()
    {
    }

    protected override void ActiveUpdate()
    {
        if (isActiveFirst == false)
        {
            isActiveFirst = true;

            // ここで必ず発動時間を入れ直す
            f_CurrentActiveTime = f_ActiveDuration;

            PlayAnkhEffect();
        }

        SetHitChecker(transform.position);

        f_CurrentActiveTime -= Time.deltaTime;

        if (f_CurrentActiveTime <= 0.0f)
        {
            gimmickState = GimmickState.Broken;
        }
    }

    protected override void BrokenUpdate()
    {
        base.BrokenUpdate();

        if (isBrokenFirst) return;

        isBrokenFirst = true;

        DeleteHitChecker();

        EndAnkhEffect();
    }

    private void PlayAnkhEffect()
    {
        if (go_AnkhEffectPrefab == null)
        {
            return;
        }

        Vector3 v3_EffectPosition =
            transform.position + v3_EffectOffset;

        Quaternion q_EffectRotation =
            go_AnkhEffectPrefab.transform.rotation;

        csad_AnkhEffect = CS_EffectFactory.CreateEffect(
            go_AnkhEffectPrefab,
            v3_EffectPosition,
            q_EffectRotation,
            null);

        if (csad_AnkhEffect == null)
        {
            return;
        }

        CSST_EffectPlayData csst_EffectPlayData = new CSST_EffectPlayData();
        csst_EffectPlayData.CSST_EffectPlayData_Init();

        csst_EffectPlayData.SetPosition(v3_EffectPosition);
        csst_EffectPlayData.SetRotation(q_EffectRotation);

        csad_AnkhEffect.PlayEffect(csst_EffectPlayData);
    }

    private void EndAnkhEffect()
    {
        if (csad_AnkhEffect == null)
        {
            return;
        }

        csad_AnkhEffect.EndEffect();
        csad_AnkhEffect = null;
    }
}
