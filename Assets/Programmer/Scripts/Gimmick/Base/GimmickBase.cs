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
using UnityEngine.Rendering;

public enum GimmickState
{
    Idle,
    Active,
    Cooldown,
    Broken,
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
    public GameObject hitCheckerPrefab;

    private GameObject hitChecker;

    // ギミックの種類
    [Header("ギミックの種類")]
    public GimmickType gimmickType = GimmickType.NotReusable;

    // ギミックの状態
    [Header("ギミックの状態")]
    public GimmickState gimmickState;

    // ギミックのグリッド上の位置
    protected Vector2Int gimmickGridPos;

    // ギミックの向き
    protected GimmickDirection gimmickDirection;

    private void Start()
    {
        AdjustScaleToGrid();
    }

    private void AdjustScaleToGrid()
    {
        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogWarning("MeshFilterが見つかりません: " + gameObject.name);
            return;
        }

        Vector3 meshSize = meshFilter.sharedMesh.bounds.size;

        // 各値をログで確認
        Debug.Log($"meshSize: {meshSize}");
        Debug.Log($"GimmickSizeX: {GimmickSizeX}, GimmickSizeY: {GimmickSizeY}");
        Debug.Log($"gridSize: {roomGrid.gridSize}");

        float targetSizeX = GimmickSizeX * roomGrid.gridSize.x;
        float targetSizeZ = GimmickSizeY * roomGrid.gridSize.y;

        Debug.Log($"targetSizeX: {targetSizeX}, targetSizeZ: {targetSizeZ}");

        float scaleX = targetSizeX / meshSize.x;
        float scaleZ = targetSizeZ / meshSize.z;
        float scaleY = (scaleX + scaleZ) / 2f;

        Debug.Log($"scale: ({scaleX}, {scaleY}, {scaleZ})");

        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }

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

    // 関数名：SetHitChecker
    // 引　数：int GridX - ギミックのグリッド上の
    //           int GridY - ギミックのグリッド上の
    // 戻り値：なし
    // 概　要：ギミックの当たり判定を設定
    protected void SetHitChecker(int GridX,int GridY)
    {
        if(hitChecker == null)
        {
            hitChecker = Instantiate(hitCheckerPrefab);
            
            // 当たり判定の大きさを設定
            GameObject Effect = hitChecker.transform.Find("Effect").gameObject;
            GameObject Hit = hitChecker.transform.Find("Hit").gameObject;

            // 確認用キューブ　（後で削除）
            GameObject Cube = hitChecker.transform.Find("Cube").gameObject;

            Vector3 EffectSize = new Vector3(EffectRangeX * roomGrid.gridSize.x,1, EffectRangeY * roomGrid.gridSize.y);
            Vector3 HitSize = new Vector3(HitRangeX * roomGrid.gridSize.x, 1, HitRangeY * roomGrid.gridSize.y);
            Vector3 CubeSize = new Vector3(EffectRangeX * roomGrid.gridSize.x, 1, EffectRangeY * roomGrid.gridSize.y);

            Effect.transform.localScale = EffectSize;
            Hit.transform.localScale = HitSize;
            Cube.transform.localScale = CubeSize;

        }

        Vector3 HitCheckerPos;
        HitCheckerPos = transform.position; // ギミックのワールド座標を取得（仮）

        // ToDo グリッドからワールド座標へ変換する関数ができたら実装

        hitChecker.transform.position = HitCheckerPos;
    }

    // 関数名：DeleteHitChecker
    // 引　数：なし
    // 戻り値：なし
    // 概　要：ギミックの当たり判定を削除
    protected void DeleteHitChecker()
    {
        if(hitChecker != null)
        {
            Destroy(hitChecker);
        }
    }


    private void FixedUpdate()
    {
        switch (gimmickState)
        {
            case GimmickState.Idle:
                // Idle状態の処理
                IdleUpdate();
                break;
            case GimmickState.Active:
                // Active状態の処理
                ActiveUpdate();
                break;
            case GimmickState.Cooldown:
                // Cooldown状態の処理
                CooldownUpdate();
                break;
            case GimmickState.Broken:
                // Broken状態の処理
                BrokenUpdate();
                break;
        }
    }

    protected virtual void IdleUpdate()
    {
        // Idle状態の処理
    }

    protected virtual void ActiveUpdate()
    {
        // Active状態の処理
    }

    protected virtual void CooldownUpdate()
    {
        // Cooldown状態の処理
    }

    protected virtual void BrokenUpdate()
    {
        // Broken状態の処理
    }

}

