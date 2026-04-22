// == GimmickBase.cs ==
// 作者 :秋野翔太
// 更新 :2026/04/22 作成開始

// ギミック仕様
// Active状態のときに、命中範囲、効果範囲に当たり判定を設ける
// 当たり判定内に、敵がいた場合、攻撃力を与える
//

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

public enum GimmickDirection
{
    Up,
    Down,
    Left,
    Right,
}

public class GimmickBase : MonoBehaviour
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

    [Header("RoomGrid")]
    [Tooltip("RoomGridのオブジェクト")]
    public RoomGrid roomGrid;

    [Header("HitChecker")]
    [Tooltip("HitCheckerのオブジェクト")]
    public Object hitCheckerPrefab;

    // ギミックの種類
    [Header("ギミックの種類")]
    public GimmickType gimmickType = GimmickType.NotReusable;

    // ギミックの状態
    protected GimmickState gimmickState;

    // ギミックのグリッド上の位置
    protected Vector2Int gimmickGridPos;

    // ギミックの向き
    protected GimmickDirection gimmickDirection;

    // 関数名：ActivateGimmick
    // 引　数：なし
    // 戻り値：なし
    // 概　要：ギミックをActive状態にする関数
    public void ActivateGimmick()
    {
        if (gimmickState == GimmickState.Idle)
        {
            gimmickState = GimmickState.Active;
        }
    }

    // 関数名：SetGimmickPos
    // 引　数：Vector2Int gridPos - ギミックのグリッド
    // 戻り値：なし
    // 概　要：ギミックのグリッド上の位置を設定
    public void SetGimmickPos(Vector2Int gridPos)
    {
        gimmickGridPos = gridPos;
    }

    // 関数名：SetGimmickDirection
    // 引　数：GimmickDirection direction - ギミックの向き
    // 戻り値：なし
    // 概　要：ギミックの向きを設定
    public void SetGimmickDirection(GimmickDirection direction)
    {
        gimmickDirection = direction;
    }
}
