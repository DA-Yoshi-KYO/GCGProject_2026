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
    private bool bool_PlayOnStart = true;

    [Header("生成から終了までの時間")]
    [SerializeField]
    private float f_PlayEndTime = 3.0f;

    [Header("終了時に非表示にするか")]
    [SerializeField]
    private bool bool_EndActive = true;

    /// <summary>
    /// 開始処理です。
    /// </summary>
    private void Start()
    {
        if (bool_PlayOnStart)
        {
            PlayTestEffect();
        }
    }

    /// <summary>
    /// 更新処理です。
    /// </summary>
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

    /// <summary>
    /// テストEffectを再生します。
    /// </summary>
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

        csst_EffectPlayData.SetPosition(v3_SpawnPosition);
        csst_EffectPlayData.SetRotation(q_SpawnRotation);
        csst_EffectPlayData.SetPlayEndTime(f_PlayEndTime);
        csst_EffectPlayData.SetEndActive(bool_EndActive);

        cs_EffectPlayer.PlayEffect(csst_EffectPlayData);
    }

    /// <summary>
    /// テストEffectを終了します。
    /// </summary>
    private void EndTestEffect()
    {
        if (cs_EffectPlayer == null)
        {
            return;
        }

        cs_EffectPlayer.EndCurrentEffect();
    }
}
