/*
+=====================================
 ファイル名 : CS_EffectPrefabKeyboardTestPlayer.cs
 概要     : キーボード入力でエフェクトPrefabの生成・停止・再利用をテストする
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
            2026/06/03 EffectId指定、再利用、生成スケール指定を追加
            2026/06/03 各変数と処理内容のコメントを追加
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
    private CSE_EffectId effectId = CSE_EffectId.None; // 再生対象のEffectIdです。

    [Header("生成位置")]
    [SerializeField]
    private Transform spawnPoint; // エフェクトを生成する基準位置です。未設定の場合はこのObjectの位置を使います。

    [Header("生成スケール")]
    [SerializeField]
    private Vector3 spawnScale = Vector3.one; // エフェクト生成時、または再利用時に設定するスケールです。

    [Header("再生キー")]
    [SerializeField]
    private KeyCode playKey = KeyCode.Return; // エフェクトを再生するキーです。

    [Header("停止キー")]
    [SerializeField]
    private KeyCode stopKey = KeyCode.Space; // エフェクトを停止するキーです。

    [Header("削除キー")]
    [SerializeField]
    private KeyCode destroyKey = KeyCode.Backspace; // エフェクトを即削除するキーです。

    [Header("デフォルト速度を使うか")]
    [SerializeField]
    private bool bool_IsUseDefaultPlaySpeed = true; // trueの場合、CS_EffectRoot側のDefaultPlaySpeedを使用します。

    [Header("上書き再生速度")]
    [SerializeField]
    private float overridePlaySpeed = 1.0f; // デフォルト速度を使わない場合に使用する再生速度です。

    [Header("停止後に削除するか")]
    [SerializeField]
    private bool bool_IsDestroyAfterStop = true; // trueの場合、停止演出後にObjectを削除します。

    [Header("サイズ拡大キー")]
    [SerializeField]
    private KeyCode scaleUpKey = KeyCode.UpArrow; // 現在表示中のエフェクトを大きくするキーです。

    [Header("サイズ縮小キー")]
    [SerializeField]
    private KeyCode scaleDownKey = KeyCode.DownArrow; // 現在表示中のエフェクトを小さくするキーです。

    [Header("サイズ変更量")]
    [SerializeField]
    private float scaleStep = 0.2f; // 1回の入力で変更するスケール量です。

    // 現在生成されているエフェクトの操作用ハンドルです。
    private CS_EffectHandle currentEffectHandle;

    /// <summary>
    /// 毎フレーム、再生・停止・削除キーの入力を確認します。
    /// </summary>
    private void Update()
    {
        // 再生キーが押されたら、エフェクトを生成または再利用して再生します。
        if (Input.GetKeyDown(playKey))
        {
            PlayEffectInstance();
        }

        // 停止キーが押されたら、現在のエフェクトを停止します。
        if (Input.GetKeyDown(stopKey))
        {
            StopCurrentEffect();
        }

        // 削除キーが押されたら、現在のエフェクトを即削除します。
        if (Input.GetKeyDown(destroyKey))
        {
            DestroyCurrentEffect();
        }

        // サイズ拡大キーが押されたら、現在表示中のエフェクトを大きくします。
        if (Input.GetKeyDown(scaleUpKey))
        {
            AddCurrentEffectScale(scaleStep);
        }

        // サイズ縮小キーが押されたら、現在表示中のエフェクトを小さくします。
        if (Input.GetKeyDown(scaleDownKey))
        {
            AddCurrentEffectScale(-scaleStep);
        }
    }

    /// <summary>
    /// 現在表示中のエフェクトのスケールを加算変更します。
    /// </summary>
    /// <param name="addScale">加算するスケール量。</param>
    private void AddCurrentEffectScale(float addScale)
    {
        // エフェクトが存在しない場合は、変更できないため処理を終了します。
        if (currentEffectHandle == null || !currentEffectHandle.IsValid)
        {
            Debug.LogWarning("[EffectPrefabKeyboardTestPlayer] サイズ変更できるエフェクトがありません。");
            return;
        }

        // spawnScaleも更新しておくことで、次回Replay時にも同じサイズを使えます。
        spawnScale += Vector3.one * addScale;

        // スケールが0以下になると見えなくなるため、最低値を設定します。
        spawnScale.x = Mathf.Max(0.01f, spawnScale.x);
        spawnScale.y = Mathf.Max(0.01f, spawnScale.y);
        spawnScale.z = Mathf.Max(0.01f, spawnScale.z);

        // 現在表示中のエフェクトに即反映します。
        currentEffectHandle.SetScale(spawnScale);

        Debug.Log("[EffectPrefabKeyboardTestPlayer] エフェクトサイズを変更しました : " + spawnScale);
    }

    /// <summary>
    /// エフェクトPrefabを生成、または生成済みエフェクトを再利用して再生します。
    /// </summary>
    private void PlayEffectInstance()
    {
        // spawnPointが未設定の場合は、このObjectのTransformを基準にします。
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;

        // spawnPointが設定されている場合は、その位置と回転を使用します。
        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position;
            spawnRotation = spawnPoint.rotation;
        }

        // すでにエフェクトが生成済みなら、新しくInstantiateせずに再利用します。
        if (currentEffectHandle != null && currentEffectHandle.IsValid)
        {
            // デフォルト速度を使わない場合は、再利用前に再生速度を上書きします。
            if (!bool_IsUseDefaultPlaySpeed)
            {
                currentEffectHandle.SetPlaySpeed(Mathf.Max(0.01f, overridePlaySpeed));
            }

            // 位置・回転・スケールを更新して再生します。
            currentEffectHandle.Replay(spawnPosition, spawnRotation, spawnScale);

            Debug.Log("[EffectPrefabKeyboardTestPlayer] 生成済みエフェクトを再利用して再生しました。");
            return;
        }

        // CS_EffectRoot側のDefaultPlaySpeedを使用して
        if (bool_IsUseDefaultPlaySpeed)
        {
            currentEffectHandle = CS_EffectPlayer.Play(
                effectId,
                spawnPosition,
                spawnRotation,
                null,
                spawnScale);
        }
        // Inspectorで指定したoverridePlaySpeedを使用して
        else
        {
            currentEffectHandle = CS_EffectPlayer.Play(
                effectId,
                spawnPosition,
                spawnRotation,
                null,
                spawnScale,
                overridePlaySpeed);
        }

        Debug.Log("[EffectPrefabKeyboardTestPlayer] エフェクトPrefabを生成して再生しました。");
    }

    /// <summary>
    /// 現在のエフェクトを停止します。
    /// bool_IsDestroyAfterStop が true の場合は、停止演出後に削除します。
    /// </summary>
    private void StopCurrentEffect()
    {
        // エフェクトが存在しない場合は停止できないため、ここで処理を終了します。
        if (currentEffectHandle == null || !currentEffectHandle.IsValid)
        {
            Debug.LogWarning("[EffectPrefabKeyboardTestPlayer] 停止できるエフェクトがありません。");
            return;
        }

        // 停止後に削除する設定の場合、停止演出で非アクティブになったあと削除します。
        if (bool_IsDestroyAfterStop)
        {
            currentEffectHandle.StopAndDestroyWhenInactive();

            // 削除待ち状態に入ったため、テスト用の参照は外します。
            currentEffectHandle = null;

            Debug.Log("[EffectPrefabKeyboardTestPlayer] エフェクトを停止後、非アクティブになったら削除します。");
            return;
        }

        // 削除しない場合は停止だけ行い、次回再生時に同じObjectを再利用します。
        currentEffectHandle.Stop();

        Debug.Log("[EffectPrefabKeyboardTestPlayer] エフェクトを停止しました。次回再生時は再利用します。");
    }

    /// <summary>
    /// 現在のエフェクトを即削除します。
    /// 停止演出を待たずにObjectを破棄します。
    /// </summary>
    private void DestroyCurrentEffect()
    {
        // エフェクトが存在しない場合は削除できないため、ここで処理を終了します。
        if (currentEffectHandle == null || !currentEffectHandle.IsValid)
        {
            Debug.LogWarning("[EffectPrefabKeyboardTestPlayer] 削除できるエフェクトがありません。");
            return;
        }

        // 現在のエフェクトObjectを即削除します。
        currentEffectHandle.Destroy();

        // 削除済みなので参照を外します。
        currentEffectHandle = null;

        Debug.Log("[EffectPrefabKeyboardTestPlayer] 現在のエフェクトを即削除しました。");
    }
}
