/*
+=====================================
 ファイル名 : CS_EffectHandle.cs
 概要     : 生成済みエフェクトを外部から停止・削除するためのハンドル
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
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

    /// <summary>
    /// コンストラクタ。
    /// </summary>
    /// <param name="effectObject">生成されたエフェクトObject。</param>
    /// <param name="cs_EffectRoot">エフェクトRoot。</param>
    /// <param name="effectPlayable">再生制御インターフェース。</param>
    public CS_EffectHandle(GameObject effectObject, CS_EffectRoot cs_EffectRoot, CSI_EffectPlayable effectPlayable)
    {
        this.effectObject = effectObject;
        this.cs_EffectRoot = cs_EffectRoot;
        this.effectPlayable = effectPlayable;
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
    /// エフェクトを停止したあと、停止演出の秒数を待ってから削除します。
    /// </summary>
    public void StopAndDestroy()
    {
        Stop();

        if (!IsValid)
        {
            return;
        }

        Object.Destroy(effectObject, Mathf.Max(0.0f, StopDuration));
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
    /// 再生演出にかかる秒数を取得します。
    /// </summary>
    public float PlayDuration
    {
        get
        {
            if (effectPlayable == null)
            {
                return 0.0f;
            }

            return effectPlayable.PlayDuration;
        }
    }

    /// <summary>
    /// 停止演出にかかる秒数を取得します。
    /// </summary>
    public float StopDuration
    {
        get
        {
            if (effectPlayable == null)
            {
                return 0.0f;
            }

            return effectPlayable.StopDuration;
        }
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
    /// 非アクティブ化されているエフェクトを指定位置に移動して再生します。
    /// </summary>
    /// <param name="position">再生位置。</param>
    public void Replay(Vector3 position)
    {
        if (!IsValid)
        {
            return;
        }

        effectObject.transform.position = position;
        Replay();
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

}
