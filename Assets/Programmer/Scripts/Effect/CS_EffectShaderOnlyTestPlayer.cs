using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectShaderOnlyTestPlayer.cs
 概要     : ShaderOnlyEffectのテスト再生クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
=====================================+
*/

/// <summary>
/// ShaderOnlyEffectのテスト再生クラスです。
/// </summary>
public class CS_EffectShaderOnlyTestPlayer : MonoBehaviour
{
    [Header("Effect再生Facade")]
    [SerializeField]
    private CS_EffectPlayer cs_EffectPlayer;

    [Header("生成位置")]
    [SerializeField]
    private Transform tr_SpawnPoint;

    [Header("Start時に再生するか")]
    [SerializeField]
    private bool b_PlayOnStart = true;

    [Header("呼び出し側からPositionを指定するか")]
    [SerializeField]
    private bool b_SetPositionFromCaller = true;

    [Header("呼び出し側からRotationを指定するか")]
    [SerializeField]
    private bool b_SetRotationFromCaller = true;

    [Header("呼び出し側からPlayEndTimeを指定するか")]
    [SerializeField]
    private bool b_SetPlayEndTimeFromCaller = false;

    [SerializeField]
    private float f_PlayEndTime = 3.0f;

    [Header("呼び出し側からHideOnEndを指定するか")]
    [SerializeField]
    private bool b_SetHideOnEndFromCaller = false;
    [SerializeField]
    private bool b_HideOnEnd = true;

    [Header("呼び出し側からPool最大数を指定するか")]
    [SerializeField]
    private bool b_SetMaxPoolCountFromCaller = false;
    [SerializeField]
    private int n_OverrideMaxPoolCount = 3;

    private void Start()
    {
        if (b_PlayOnStart)
        {
            PlayTestEffect();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayTestEffect();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            EndTestEffect();
        }
    }

    private void PlayTestEffect()
    {
        if (cs_EffectPlayer == null)
        {
            Debug.LogWarning("[CS_EffectShaderOnlyTestPlayer] CS_EffectPlayerが設定されていません。");
            return;
        }

        Vector3 v3_SpawnPosition = transform.position;
        Quaternion q_SpawnRotation = transform.rotation;

        if (tr_SpawnPoint != null)
        {
            v3_SpawnPosition = tr_SpawnPoint.position;
            q_SpawnRotation = tr_SpawnPoint.rotation;
        }

        CSST_EffectPlayData csst_EffectPlayData = new CSST_EffectPlayData();
        csst_EffectPlayData.CSST_EffectPlayData_Init();

        if (b_SetPositionFromCaller)
        {
            csst_EffectPlayData.SetPosition(v3_SpawnPosition);
        }

        if (b_SetRotationFromCaller)
        {
            csst_EffectPlayData.SetRotation(q_SpawnRotation);
        }

        if (b_SetPlayEndTimeFromCaller)
        {
            csst_EffectPlayData.SetPlayEndTime(f_PlayEndTime);
        }

        if (b_SetHideOnEndFromCaller)
        {
            csst_EffectPlayData.SetHideOnEnd(b_HideOnEnd);
        }

        if (b_SetMaxPoolCountFromCaller)
        {
            cs_EffectPlayer.PlayEffect(
                csst_EffectPlayData,
                n_OverrideMaxPoolCount);

            return;
        }

        cs_EffectPlayer.PlayEffect(csst_EffectPlayData);
    }

    private void EndTestEffect()
    {
        if (cs_EffectPlayer == null)
        {
            return;
        }

        cs_EffectPlayer.EndCurrentEffect();
    }
}
