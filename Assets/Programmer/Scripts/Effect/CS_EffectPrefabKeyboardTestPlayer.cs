/*
+=====================================
 ファイル名 : CS_EffectPrefabKeyboardTestPlayer.cs
 概要     : キーボード入力でエフェクトPrefabの生成・停止・再利用をテストする
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
            2026/06/03 生成済みエフェクトの再利用テスト処理を追加
=====================================+
*/

using UnityEngine;

/// <summary>
/// キーボード入力でエフェクトPrefabの生成・停止・削除・再利用を確認するテスト用クラスです。
/// </summary>
public class CS_EffectPrefabKeyboardTestPlayer : MonoBehaviour
{
    [Header("再生するEffectId")]
    [SerializeField]
    private CSE_EffectId effectId = CSE_EffectId.None;

    [Header("生成位置")]
    [SerializeField]
    private Transform spawnPoint;

    [Header("再生キー")]
    [SerializeField]
    private KeyCode playKey = KeyCode.Return;

    [Header("停止キー")]
    [SerializeField]
    private KeyCode stopKey = KeyCode.Space;

    [Header("削除キー")]
    [SerializeField]
    private KeyCode destroyKey = KeyCode.Backspace;

    [Header("デフォルト速度を使うか")]
    [SerializeField]
    private bool bool_IsUseDefaultPlaySpeed = true;

    [Header("上書き再生速度")]
    [SerializeField]
    private float overridePlaySpeed = 1.0f;

    [Header("停止後に削除するか")]
    [SerializeField]
    private bool bool_IsDestroyAfterStop = true;

    // 現在生成しているエフェクトを外部から操作するためのハンドルです。
    // Destroyしない運用の場合は、このHandleを残してReplayします。
    private CS_EffectHandle currentEffectHandle;

    /// <summary>
    /// 毎フレーム、入力を確認します。
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(playKey))
        {
            PlayEffectInstance();
        }

        if (Input.GetKeyDown(stopKey))
        {
            StopCurrentEffect();
        }

        if (Input.GetKeyDown(destroyKey))
        {
            DestroyCurrentEffect();
        }
    }

    /// <summary>
    /// エフェクトPrefabを生成、または生成済みエフェクトを再利用して再生します。
    /// </summary>
    private void PlayEffectInstance()
    {
        if (effectId == CSE_EffectId.None)
        {
            Debug.LogWarning("[EffectPrefabKeyboardTestPlayer] 生成するEffectPrefabが設定されていません。");
            return;
        }

        // 生成位置は、SpawnPointが設定されていればSpawnPointを使います。
        // 未設定なら、このテスト用Object自身の位置を使います。
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;

        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position;
            spawnRotation = spawnPoint.rotation;
        }

        // すでに生成済みのEffectが存在する場合は、再生成せずに再利用します。
        // Stop後にDestroyしていない場合、EffectObjectは非アクティブ状態で残っているため、
        // Replayで位置と回転を更新して再生します。
        if (currentEffectHandle != null && currentEffectHandle.IsValid)
        {
            currentEffectHandle.Replay(spawnPosition, spawnRotation);

            Debug.Log("[EffectPrefabKeyboardTestPlayer] 生成済みエフェクトを再利用して再生しました。");
            return;
        }

        // Handleが無い、または既にDestroy済みの場合は、新しくPrefabを生成します。
        if (bool_IsUseDefaultPlaySpeed)
        {
            // Prefab側のDefaultPlaySpeedを使って再生します。
            currentEffectHandle = CS_EffectPlayer.Play(
                effectId,
                spawnPosition,
                spawnRotation,
                null);
        }
        else
        {
            // Inspectorで指定したoverridePlaySpeedを使って再生します。
            currentEffectHandle = CS_EffectPlayer.Play(
                effectId,
                spawnPosition,
                spawnRotation,
                null,
                overridePlaySpeed);
        }

        Debug.Log("[EffectPrefabKeyboardTestPlayer] エフェクトPrefabを生成して再生しました。");
    }

    /// <summary>
    /// 現在のエフェクトを停止します。
    /// </summary>
    private void StopCurrentEffect()
    {
        if (currentEffectHandle == null || !currentEffectHandle.IsValid)
        {
            Debug.LogWarning("[EffectPrefabKeyboardTestPlayer] 停止できるエフェクトがありません。");
            return;
        }

        if (bool_IsDestroyAfterStop)
        {
            // 消失演出を再生したあと、EffectObjectが非アクティブになったタイミングで削除します。
            // この場合、Handleは使い回さないのでnullにします。
            currentEffectHandle.StopAndDestroyWhenInactive();
            currentEffectHandle = null;

            Debug.Log("[EffectPrefabKeyboardTestPlayer] エフェクトを停止後、非アクティブになったら削除します。");
            return;
        }

        // 削除せずに停止します。
        // Hide処理の最後でSetActive(false)される前提です。
        // Handleは残すので、次のEnterでReplayして再利用できます。
        currentEffectHandle.Stop();

        Debug.Log("[EffectPrefabKeyboardTestPlayer] エフェクトを停止しました。次回再生時は再利用します。");
    }

    /// <summary>
    /// 現在のエフェクトを即削除します。
    /// </summary>
    private void DestroyCurrentEffect()
    {
        if (currentEffectHandle == null || !currentEffectHandle.IsValid)
        {
            Debug.LogWarning("[EffectPrefabKeyboardTestPlayer] 削除できるエフェクトがありません。");
            return;
        }

        // 消失演出を待たずに即Destroyします。
        // テスト中にHierarchyを掃除したい場合に使います。
        currentEffectHandle.Destroy();
        currentEffectHandle = null;

        Debug.Log("[EffectPrefabKeyboardTestPlayer] 現在のエフェクトを即削除しました。");
    }
}
