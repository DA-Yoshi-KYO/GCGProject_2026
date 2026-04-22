// == GimmickBase.cs ==
// 作者 :秋野翔太
// 更新 :2026/04/22 作成開始

using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public enum GimmickState
{
    Idle,
    Active,
    Cooldown,
};

public enum  GimmickType
{
    NotReusable,
    Reusable,
}

public class NewBehaviourScript : MonoBehaviour
{
    // 大きさ
    [Header("大きさ")]
    [Tooltip("X方向の大きさ"), Min(0)]
    public float GimmickSizeX;
    [Tooltip("Y方向の大きさ"), Min(0)]
    public float GimmickSizeY;

    // 命中範囲
    [Header("命中範囲")]
    [Tooltip("X方向の命中範囲"), Min(0)]
    public float HitRangeX;
    [Tooltip("Y方向の命中範囲"), Min(0)]
    public float HitRangeY;

    // 効果範囲
    [Header("効果範囲")]
    [Tooltip("X方向の効果範囲"), Min(0)]
    public float EffectRangeX;
    [Tooltip("Y方向の効果範囲"), Min(0)]
    public float EffectRangeY;

    // 必要なソウル数
    [Header("必要ソウル数")]
    [Tooltip("必要なソウル数"), Min(0)]
    public int RequiredSoul;

    // 攻撃力と効果力
    [Header("攻撃力")]
    [Tooltip("命中時"), Min(0)]
    public int AttackPower;
    [Tooltip("非命中時"), Min(0)]
    public int EffectPower;

    // ギミックの種類
    [Header("ギミックの種類")]
    public GimmickType gimmickType = GimmickType.NotReusable;

    // ギミックの状態
    protected GimmickState gimmickState;

    protected void SetHitRange()
    {

    }

}
