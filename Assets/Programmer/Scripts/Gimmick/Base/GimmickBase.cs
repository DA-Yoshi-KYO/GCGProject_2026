// == GimmickBase.cs ==
// 作者 :秋野翔太
// 更新 :2026/04/22 作成開始
//      :2026/04/24 ギミックの大きさ取得関数の追加
//      :2026/04/24 ギミックの識別タグ取得関数の追加

// ギミック仕様
// Active状態のときに、命中範囲、効果範囲に当たり判定を設ける
// 当たり判定内に、敵がいた場合、攻撃力を与える
//

using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public enum Gimmick
{
    None,
    Pot,
    IronBall,
    FakeChest,
}

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
    public float gimmickSizeX;
    [Tooltip("Y方向の大きさ"), Min(0)]
    public float gimmickSizeY;

    // 命中範囲
    [Header("命中範囲")]
    [Tooltip("X方向の命中範囲"), Min(0)]
    public float hitRangeX;
    [Tooltip("Y方向の命中範囲"), Min(0)]
    public float hitRangeY;

    // 効果範囲
    [Header("効果範囲")]
    [Tooltip("X方向の効果範囲"), Min(0)]
    public float effectRangeX;
    [Tooltip("Y方向の効果範囲"), Min(0)]
    public float effectRangeY;

    // 必要なソウル数
    [Header("必要ソウル数")]
    [Tooltip("必要なソウル数"), Min(0)]
    public int requiredSoul;

    // 攻撃力と効果力
    [Header("攻撃力")]
    [Tooltip("命中時"), Min(0)]
    public int attackPower;
    [Tooltip("非命中時"), Min(0)]
    public int effectPower;

    [Header("RoomGrid")]
    [Tooltip("RoomGridのオブジェクト")]
    public RoomGrid roomGrid;

    [Header("HitChecker")]
    [Tooltip("HitCheckerのオブジェクト")]
    public GameObject hitCheckerPrefab;

    // ギミックの向き
    [Header("ギミックの向き")]
    public GimmickDirection gimmickDirection;

    // ギミックの種類
    [Header("ギミックのタイプ")]
    public GimmickType gimmickType = GimmickType.NotReusable;

    [Header("ギミックの種類")]
    public Gimmick gimmick;

    // ギミックの状態
    [Header("ギミックの状態")]
    public GimmickState gimmickState;

    [Header("調整用（プログラマー専用）")]
    public int Adjust;

    // ギミックのグリッド上の位置
    protected Vector2Int gimmickGridPos;

    private GameObject hitChecker;

    private void Start()
    {
        AdjustScaleToGrid();
        SetGimmickPos(new Vector2Int(1,0));
    }


    /// <summary>
    /// ギミックの大きさを、グリッドの大きさに合わせて調整する関数
    /// </summary>
    private void AdjustScaleToGrid()
    {
        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogWarning("MeshFilterが見つかりません: " + gameObject.name);
            return;
        }

        Vector3 meshSize = meshFilter.sharedMesh.bounds.size;

        float targetSizeX = gimmickSizeX * roomGrid.gridSize.x;
        float targetSizeZ = gimmickSizeY * roomGrid.gridSize.y;

        float scaleX = targetSizeX / meshSize.x;
        float scaleZ = targetSizeZ / meshSize.z;
        float scaleY = (scaleX + scaleZ) / 2f;
        scaleX = scaleX * Adjust;
        scaleY = scaleY * Adjust;
        scaleZ = scaleZ * Adjust;

        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }

    /// <summary>
    /// ギミックをアクティブにする関数
    /// </summary>
    public void ActivateGimmick()
    {
        if (gimmickState == GimmickState.Idle)
        {
            gimmickState = GimmickState.Active;
        }
    }


    /// <summary>
    /// グリッド座標からワールド座標に変換して、ギミックの位置を設定する
    /// </summary>
    /// <param name="gridPos">グリッド座標</param>
    public void SetGimmickPos(Vector2Int gridPos)
    {
        gimmickGridPos = gridPos;
        Vector3 newWorldPos = roomGrid.GetWorldPosFromGrid(gridPos);
        newWorldPos.x = newWorldPos.x * (float)Adjust;
        newWorldPos.y = newWorldPos.y * (float)Adjust;
        newWorldPos.z = newWorldPos.z * (float)Adjust;
        transform.position = newWorldPos;
    }

    /// <summary>
    /// ワールド座標からグリッド座標に変換して、ギミックの位置を設定する
    /// </summary>
    /// <param name="WorldPos">ワールド座標</param>
    public void SetGimmickPos(Vector3 WorldPos)
    {
        Vector2Int gridPos = roomGrid.GetGridFromPos(WorldPos);
        SetGimmickPos(gridPos);
    }

    /// <summary>
    /// ギミックの向きを設定する関数
    /// </summary>
    /// <param name="direction">ギミックの向き</param>
    public void SetGimmickDirection(GimmickDirection direction)
    {
        gimmickDirection = direction;
    }

    /// <summary>
    /// 泥棒に対する当たり判定を設定する関数
    /// </summary>
    /// <param name="GridX">グリッド座標</param>
    /// <param name="GridY">グリッド座標</param>
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

            Vector3 EffectSize = new Vector3(effectRangeX * roomGrid.gridSize.x,1, effectRangeY * roomGrid.gridSize.y);
            Vector3 HitSize = new Vector3(hitRangeX * roomGrid.gridSize.x, 1, hitRangeY * roomGrid.gridSize.y);
            
            EffectSize.x = EffectSize.x * (float)Adjust;
            EffectSize.y = EffectSize.y * (float)Adjust;
            EffectSize.z = EffectSize.z * (float)Adjust;

            HitSize.x = HitSize.x * (float)Adjust;
            HitSize.y = HitSize.y * (float)Adjust;
            HitSize.z = HitSize.z * (float)Adjust;

            Effect.transform.localScale = EffectSize;
            Hit.transform.localScale = HitSize;
            Cube.transform.localScale = EffectSize;

        }

        Vector3 HitCheckerPos;
        HitCheckerPos = transform.position;


        HitCheckerPos = roomGrid.GetWorldPosFromGrid(new Vector2Int(GridX, GridY));
        // 無限数チェック
        if (float.IsInfinity(HitCheckerPos.x) || float.IsInfinity(HitCheckerPos.y) || float.IsInfinity(HitCheckerPos.z) || GridX < 0 || GridY < 0)
        {
            Debug.LogWarning("SetHitChecker: Invalid grid position (" + GridX + ", " + GridY + ")");
            DeleteHitChecker();
            return;
        }

        HitCheckerPos.x = HitCheckerPos.x * (float)Adjust;
        HitCheckerPos.y = HitCheckerPos.y * (float)Adjust;
        HitCheckerPos.z = HitCheckerPos.z * (float)Adjust;

        hitChecker.transform.position = HitCheckerPos;
    }

    /// <summary>
    /// 泥棒に対する当たり判定を削除する関数
    /// </summary>
    protected void DeleteHitChecker()
    {
        if(hitChecker != null)
        {
            Destroy(hitChecker);
        }
    }

    /// <summary>
    /// トラップの向きをベクトルで返す関数
    /// </summary>
    /// <returns>ギミックの向きを表すベクトル</returns>
    public Vector2Int GetDirectionVec()
    {
        switch(gimmickDirection)
        {
            case GimmickDirection.Up:
                return new Vector2Int(0, -1);
            case GimmickDirection.Down:
                return new Vector2Int(0, 1);
            case GimmickDirection.Left:
                return new Vector2Int(-1, 0);
            case GimmickDirection.Right:
                return new Vector2Int(1, 0);
            default:
                return Vector2Int.zero;
        }
    }

    /// <summary>
    /// ギミックの大きさをグリッド単位で返す関数
    /// </summary>
    /// <returns>ギミックの大きさを表すベクトル</returns>
    public Vector2Int GetGimmickSize()
    {
        return new Vector2Int((int)gimmickSizeX, (int)gimmickSizeY);
    }

    /// <summary>
    /// ギミックの識別タグを返す関数
    /// </summary>
    /// <returns>ギミックの識別タグ</returns>
    public Gimmick GetGimmickTag()
    {
        return gimmick;
    }


    // ===============================================================================

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

    // ===============================================================================
}

