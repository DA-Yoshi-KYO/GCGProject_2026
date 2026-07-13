using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectShaderOnlyTestPlayer.cs
 概要     : ShaderOnlyEffectのテスト再生クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
            2026/07/13 Warpカラー変更テストを追加
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

    [Header("Warpカラー変更テスト")]
    [SerializeField]
    private bool b_SetWarpColorFromCaller = true;

    [SerializeField]
    [ColorUsage(true, true)]
    private Color c_TestWarpColor = Color.cyan;

    [Header("実行中のカラー変更を即時反映するか")]
    [SerializeField]
    private bool b_ApplyWarpColorInRealTime = true;

    /// <summary>
    /// 前回反映したWarpカラーです。
    /// </summary>
    private Color c_LastAppliedWarpColor;

    [Header("Loop中Transform変更テスト")]
    [SerializeField]
    private Vector3 v3_TestMoveOffset =
        new Vector3(0.0f, 1.0f, 0.0f);

    [SerializeField]
    private Vector3 v3_TestRotateEulerAngles =
        new Vector3(0.0f, 180.0f, 0.0f);

    [SerializeField]
    private Vector3 v3_TestScale =
        new Vector3(2.0f, 2.0f, 2.0f);

    [SerializeField]
    private float f_TestTransformChangeTime = 0.5f;

    /// <summary>
    /// 現在テスト再生しているEffectです。
    /// </summary>
    private CSAD_EffectCommonProcessBase csad_CurrentTestEffect;

    private void Start()
    {
        c_LastAppliedWarpColor = c_TestWarpColor;

        if (b_PlayOnStart)
        {
            PlayTestEffect();
        }
    }

    private void Update()
    {
        UpdateWarpColorInRealTime();

        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayTestEffect();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            EndTestEffect();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ApplyCurrentWarpColorTest();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            MoveCurrentEffectTest();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            RotateCurrentEffectTest();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            ScaleCurrentEffectTest();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            StopCurrentEffectTransformTest();
        }
    }

    /// <summary>
    /// Inspector上でWarpカラーが変更された場合、
    /// 現在再生中のWarpへ即時反映します。
    /// </summary>
    private void UpdateWarpColorInRealTime()
    {
        if (b_ApplyWarpColorInRealTime == false)
        {
            return;
        }

        if (b_SetWarpColorFromCaller == false)
        {
            return;
        }

        if (IsSameColor(
            c_LastAppliedWarpColor,
            c_TestWarpColor))
        {
            return;
        }

        c_LastAppliedWarpColor = c_TestWarpColor;

        ApplyCurrentWarpColorTest();
    }

    /// <summary>
    /// 2つのColorが同じか確認します。
    /// HDRカラーにも対応します。
    /// </summary>
    private bool IsSameColor(
        Color c_ColorA,
        Color c_ColorB)
    {
        return
            Mathf.Approximately(c_ColorA.r, c_ColorB.r) &&
            Mathf.Approximately(c_ColorA.g, c_ColorB.g) &&
            Mathf.Approximately(c_ColorA.b, c_ColorB.b) &&
            Mathf.Approximately(c_ColorA.a, c_ColorB.a);
    }

    /// <summary>
    /// Effectをテスト再生します。
    /// </summary>
    private void PlayTestEffect()
    {
        if (cs_EffectPlayer == null)
        {
            Debug.LogWarning(
                "[CS_EffectShaderOnlyTestPlayer] " +
                "CS_EffectPlayerが設定されていません。");

            return;
        }

        Vector3 v3_SpawnPosition = transform.position;
        Quaternion q_SpawnRotation = transform.rotation;

        if (tr_SpawnPoint != null)
        {
            v3_SpawnPosition = tr_SpawnPoint.position;
            q_SpawnRotation = tr_SpawnPoint.rotation;
        }

        CSST_EffectPlayData csst_EffectPlayData =
            new CSST_EffectPlayData();

        csst_EffectPlayData.CSST_EffectPlayData_Init();

        if (b_SetPositionFromCaller)
        {
            csst_EffectPlayData.SetPosition(
                v3_SpawnPosition);
        }

        if (b_SetRotationFromCaller)
        {
            csst_EffectPlayData.SetRotation(
                q_SpawnRotation);
        }

        if (b_SetPlayEndTimeFromCaller)
        {
            csst_EffectPlayData.SetPlayEndTime(
                f_PlayEndTime);
        }

        if (b_SetHideOnEndFromCaller)
        {
            csst_EffectPlayData.SetHideOnEnd(
                b_HideOnEnd);
        }

        if (b_SetMaxPoolCountFromCaller)
        {
            csad_CurrentTestEffect =
                cs_EffectPlayer.PlayEffect(
                    csst_EffectPlayData,
                    n_OverrideMaxPoolCount);
        }
        else
        {
            csad_CurrentTestEffect =
                cs_EffectPlayer.PlayEffect(
                    csst_EffectPlayData);
        }

        ApplyCurrentWarpColorTest();
    }

    /// <summary>
    /// 現在再生しているWarpへテストカラーを反映します。
    /// </summary>
    private void ApplyCurrentWarpColorTest()
    {
        if (b_SetWarpColorFromCaller == false)
        {
            return;
        }

        if (csad_CurrentTestEffect == null)
        {
            return;
        }

        CS_EffectWarpShaderOnly cs_WarpEffect =
            csad_CurrentTestEffect
                .GetComponentInChildren<CS_EffectWarpShaderOnly>(true);

        if (cs_WarpEffect == null)
        {
            Debug.LogWarning(
                "[CS_EffectShaderOnlyTestPlayer] " +
                "再生中のEffectにCS_EffectWarpShaderOnlyがありません : " +
                csad_CurrentTestEffect.name);

            return;
        }

        cs_WarpEffect.SetEffectColor(c_TestWarpColor);

        c_LastAppliedWarpColor = c_TestWarpColor;
    }

    /// <summary>
    /// 現在のEffectを終了します。
    /// </summary>
    private void EndTestEffect()
    {
        if (csad_CurrentTestEffect != null)
        {
            csad_CurrentTestEffect.EndEffect();
            return;
        }

        if (cs_EffectPlayer == null)
        {
            return;
        }

        cs_EffectPlayer.EndCurrentEffect();
    }

    /// <summary>
    /// 現在のEffectを移動させるテストです。
    /// </summary>
    private void MoveCurrentEffectTest()
    {
        if (TryGetCurrentEffectTransformController(
            out CS_EffectTransformController cs_EffectTransformController)
            == false)
        {
            return;
        }

        Vector3 v3_TargetPosition =
            csad_CurrentTestEffect.transform.position +
            v3_TestMoveOffset;

        cs_EffectTransformController.MoveEffect(
            v3_TargetPosition,
            f_TestTransformChangeTime);
    }

    /// <summary>
    /// 現在のEffectを回転させるテストです。
    /// </summary>
    private void RotateCurrentEffectTest()
    {
        if (TryGetCurrentEffectTransformController(
            out CS_EffectTransformController cs_EffectTransformController)
            == false)
        {
            return;
        }

        Quaternion q_TargetRotation =
            csad_CurrentTestEffect.transform.rotation *
            Quaternion.Euler(v3_TestRotateEulerAngles);

        cs_EffectTransformController.RotateEffect(
            q_TargetRotation,
            f_TestTransformChangeTime);
    }

    /// <summary>
    /// 現在のEffectのScaleを変更するテストです。
    /// </summary>
    private void ScaleCurrentEffectTest()
    {
        if (TryGetCurrentEffectTransformController(
            out CS_EffectTransformController cs_EffectTransformController)
            == false)
        {
            return;
        }

        cs_EffectTransformController.ScaleEffect(
            v3_TestScale,
            f_TestTransformChangeTime);
    }

    /// <summary>
    /// 現在のEffectのTransform変更処理を止めるテストです。
    /// </summary>
    private void StopCurrentEffectTransformTest()
    {
        if (TryGetCurrentEffectTransformController(
            out CS_EffectTransformController cs_EffectTransformController)
            == false)
        {
            return;
        }

        cs_EffectTransformController.StopTransformControl();
    }

    /// <summary>
    /// 現在のEffectからTransformControllerを取得します。
    /// </summary>
    private bool TryGetCurrentEffectTransformController(
        out CS_EffectTransformController cs_EffectTransformController)
    {
        cs_EffectTransformController = null;

        if (csad_CurrentTestEffect == null)
        {
            Debug.LogWarning(
                "[CS_EffectShaderOnlyTestPlayer] " +
                "現在再生中のEffectがありません。");

            return false;
        }

        cs_EffectTransformController =
            csad_CurrentTestEffect
                .GetComponent<CS_EffectTransformController>();

        if (cs_EffectTransformController == null)
        {
            Debug.LogWarning(
                "[CS_EffectShaderOnlyTestPlayer] " +
                "CS_EffectTransformControllerがEffectに付いていません : " +
                csad_CurrentTestEffect.name);

            return false;
        }

        return true;
    }
}
