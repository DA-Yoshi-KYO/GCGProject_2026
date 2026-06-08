/*
+=====================================
 ファイル名 : CS_EffectHandle.cs
 概要     : 生成済みエフェクトを外部から停止・削除・再利用するためのハンドル
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
            2026/06/03 停止後削除、非アクティブ化、再利用、スケール変更を追加
=====================================+
*/

using UnityEngine;

/// <summary>
/// 生成済みエフェクトを外部から操作するためのハンドルです。
/// </summary>
public class CS_EffectHandle
{
    private readonly GameObject effectObject;
    private readonly CS_EffectRoot cs_EffectRoot;
    private readonly CSI_EffectPlayable effectPlayable;

    /// <summary>
    /// エフェクトObjectを取得します。
    /// </summary>
    public GameObject EffectObject => effectObject;

    /// <summary>
    /// エフェクトが有効な状態か取得します。
    /// </summary>
    public bool IsValid => effectObject != null;

    // Prefab生成直後の初期Scaleです。
    private readonly Vector3 defaultLocalScale;

    /// <summary>
    /// コンストラクタ。
    /// </summary>
    /// <param name="effectObject">生成されたエフェクトObject。</param>
    /// <param name="cs_EffectRoot">エフェクトRoot。</param>
    /// <param name="effectPlayable">再生制御インターフェース。</param>
    /// <param name="defaultLocalScale">Prefab初期Scale。</param>
    public CS_EffectHandle(
        GameObject effectObject,
        CS_EffectRoot cs_EffectRoot,
        CSI_EffectPlayable effectPlayable,
        Vector3 defaultLocalScale)
    {
        this.effectObject = effectObject;
        this.cs_EffectRoot = cs_EffectRoot;
        this.effectPlayable = effectPlayable;
        this.defaultLocalScale = defaultLocalScale;
    }

    /// <summary>
    /// エフェクトを停止します。
    /// </summary>
    public void Stop()
    {
        if (!IsValid)
        {
            return;
        }

        if (effectPlayable == null)
        {
            return;
        }

        if (!effectObject.activeInHierarchy)
        {
            return;
        }

        effectPlayable.StopEffect();
    }

    /// <summary>
    /// エフェクトを停止したあと、対象Objectが非アクティブになったら削除します。
    /// 消失演出側で SetActive(false) されることが前提です。
    /// </summary>
    public void StopAndDestroyWhenInactive()
    {
        Stop();

        if (!IsValid)
        {
            return;
        }

        CS_EffectRuntimeRunner.RunWhenInactive(
            effectObject,
            () =>
            {
                if (IsValid)
                {
                    Object.Destroy(effectObject);
                }
            });
    }

    /// <summary>
    /// エフェクトObjectを削除します。
    /// </summary>
    public void Destroy()
    {
        if (!IsValid)
        {
            return;
        }

        Object.Destroy(effectObject);
    }

    /// <summary>
    /// 指定秒数後にエフェクトObjectを削除します。
    /// </summary>
    /// <param name="delaySeconds">削除までの秒数。</param>
    public void Destroy(float delaySeconds)
    {
        if (!IsValid)
        {
            return;
        }

        Object.Destroy(effectObject, delaySeconds);
    }

    /// <summary>
    /// エフェクトObjectを非アクティブにします。
    /// 削除せずに残したい場合に使用します。
    /// </summary>
    public void Deactivate()
    {
        if (!IsValid)
        {
            return;
        }

        effectObject.SetActive(false);
    }

    /// <summary>
    /// 非アクティブ化されているエフェクトを再度有効化して再生します。
    /// 現在位置のまま再生します。
    /// </summary>
    public void Replay()
    {
        if (!IsValid)
        {
            return;
        }

        effectObject.SetActive(true);

        if (!effectObject.activeInHierarchy)
        {
            Debug.LogWarning("[EffectHandle] 親Objectが非アクティブのため再生できません : " + effectObject.name);
            return;
        }

        if (effectPlayable == null)
        {
            return;
        }

        effectPlayable.PlayEffect();
    }

    /// <summary>
    /// 非アクティブ化されているエフェクトを指定位置・指定回転に移動して再生します。
    /// </summary>
    /// <param name="position">再生位置。</param>
    /// <param name="rotation">再生回転。</param>
    public void Replay(Vector3 position, Quaternion rotation)
    {
        if (!IsValid)
        {
            return;
        }

        effectObject.transform.SetPositionAndRotation(position, rotation);
        Replay();
    }

    /// <summary>
    /// 非アクティブ化されているエフェクトを指定位置・指定回転・指定スケールに変更して再生します。
    /// </summary>
    /// <param name="position">再生位置。</param>
    /// <param name="rotation">再生回転。</param>
    /// <param name="scale">外部指定Scale。1,1,1の場合はPrefab初期Scaleを維持します。</param>
    public void Replay(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (!IsValid)
        {
            return;
        }

        effectObject.transform.SetPositionAndRotation(position, rotation);

        // ここが重要。
        // localScaleへ直接入れず、Prefab初期Scale基準のSetScaleを必ず通します。
        SetScale(scale);

        Replay();
    }

    /// <summary>
    /// Prefab初期Scaleを基準に、エフェクトObjectのスケールを設定します。
    /// </summary>
    /// <param name="scale">外部指定Scale。1.0fの軸はPrefab初期値を維持します。</param>
    public void SetScale(Vector3 scale)
    {
        if (!IsValid)
        {
            return;
        }

        effectObject.transform.localScale =
            CS_EffectScaleUtility.CalculateScale(defaultLocalScale, scale);
    }

    /// <summary>
    /// エフェクトObjectのスケールを全軸共通で設定します。
    /// </summary>
    /// <param name="uniformScale">全軸共通スケール。</param>
    public void SetScale(float uniformScale)
    {
        SetScale(Vector3.one * uniformScale);
    }

    /// <summary>
    /// エフェクトの再生速度を設定します。
    /// </summary>
    /// <param name="playSpeed">再生速度。</param>
    public void SetPlaySpeed(float playSpeed)
    {
        if (effectPlayable == null)
        {
            return;
        }

        effectPlayable.SetPlaySpeed(playSpeed);
    }
}
