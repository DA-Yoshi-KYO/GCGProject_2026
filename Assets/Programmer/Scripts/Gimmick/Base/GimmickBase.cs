// == GimmickBase.cs ==
// 作者 :秋野翔太
// 更新 :2026/04/22 作成開始
//      :2026/04/24 ギミックの大きさ取得関数の追加
//      :2026/04/24 ギミックの識別タグ取得関数の追加

// ギミック仕様
// Active状態のときに、命中範囲、効果範囲に当たり判定を設ける
// 当たり判定内に、敵がいた場合、攻撃力を与える
//

using System.Collections.Generic;
using UnityEngine;

public enum Gimmick
{
    None,
    Pot,
    IronBall,
    EmptyChest,
}

public enum GimmickState
{
    Idle,
    Search,
    Active,
    Cooldown,
    Broken,
};

public enum  GimmickType
{
    NotReusable = 0,
    Reusable = 1,
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
    // ギミックのイメ－ジ画像
    public Sprite gimmickImage;

    // 大きさ
    [Header("大きさ")]
    [Tooltip("X方向の大きさ"), Min(0)]
    public float gimmickSizeX;
    [Tooltip("Y方向の大きさ"), Min(0)]
    public float gimmickSizeY;
    [Tooltip("Z方向の大きさ"), Min(0)]
    public float gimmickSizeZ;
    [Tooltip("拡縮率 / ％"), Min(0)]
    public float gimmickScale = 100;

    // 命中範囲
    [Header("命中範囲")]
    [Tooltip("X方向の命中範囲"), Min(0)]
    public float hitRangeX;
    [Tooltip("Y方向の命中範囲"), Min(0)]
    public float hitRangeY;
    [Tooltip("Z方向の命中範囲"), Min(0)]
    public float hitRangeZ;

    // 効果範囲
    [Header("効果範囲")]
    [Tooltip("X方向の効果範囲"), Min(0)]
    public float effectRangeX;
    [Tooltip("Y方向の効果範囲"), Min(0)]
    public float effectRangeY;
    [Tooltip("Z方向の効果範囲"), Min(0)]
    public float effectRangeZ;

    //クールタイム
    [Header("クールタイム")]
    [SerializeField]
    private int coolTime;

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
    public GimmickType gimmickType;

    [Header("ギミックの種類")]
    public Gimmick gimmick;

    // ギミックの状態
    [Header("ギミックの状態")]
    public GimmickState gimmickState;

    [Header("泥棒検知")]
    [SerializeField] private GameObject search;
    [Tooltip("泥棒を検知する範囲"), Min(0)]
    [SerializeField] protected int searchGridRange;
    [Header("敵のレイヤー")]
    public LayerMask enemyLayer;

    [Header("調整用（プログラマー専用）")]
    [Tooltip("ギミックの大きさや位置を調整するための値"), Min(1)]
    public int Adjust;

    // ギミックのグリッド上の位置
    protected Vector2Int gimmickGridPos;

    private GameObject hitChecker;
    private BoxCollider searchColliderX;
    private BoxCollider searchColliderZ;

    private void Start()
    {
        GameObject X = search.transform.Find("X").gameObject;
        GameObject Z = search.transform.Find("Z").gameObject;
        searchColliderX = X.GetComponent<BoxCollider>();
        searchColliderZ = Z.GetComponent<BoxCollider>();
    }

    /// <summary>
    /// ギミックの大きさを、グリッドの大きさに合わせて調整する関数
    /// </summary>
    public void AdjustScaleToGrid()
    {
        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogWarning("MeshFilterが見つかりません: " + gameObject.name);
            return;
        }

        Vector3 meshSize = meshFilter.sharedMesh.bounds.size;

        float targetSizeX = gimmickSizeX * roomGrid.gridSize.x;
        float targetSizeZ = gimmickSizeZ * roomGrid.gridSize.y;

        float scaleX = targetSizeX / meshSize.x;
        float scaleZ = targetSizeZ / meshSize.z;
        float scaleY = (scaleX + scaleZ) / 2f;
        scaleX = scaleX * gimmickScale / 100f;
        scaleY = scaleY * gimmickScale / 100f;
        scaleZ = scaleZ * gimmickScale / 100f;

        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

        Vector3 Set = search.transform.position;
        Set.y = 0.0f;
        search.transform.position = Set;

        GameObject X = search.transform.Find("X").gameObject;
        GameObject Z = search.transform.Find("Z").gameObject;
        Vector3 searchX = X.transform.localScale;
        Vector3 searchZ = Z.transform.localScale;
        // X軸に対する当たり判定の大きさを設定
        searchX.x = searchGridRange * roomGrid.gridSize.x;
        // Z軸に対する当たり判定の大きさを設定
        searchZ.z = searchGridRange * roomGrid.gridSize.y;
        X.transform.localScale = searchX;
        Z.transform.localScale = searchZ;
    }

    /// <summary>
    /// ギミックをアクティブにする関数
    /// </summary>
    public void ActivateGimmick()
    {
        if (gimmickState == GimmickState.Idle)
        {
            gimmickState = GimmickState.Search;
        }
    }

    /// <summary>
    /// グリッド座標からワールド座標に変換して、ギミックの位置を設定する
    /// </summary>
    /// <param name="gridPos">グリッド座標</param>
    public void SetGimmickPos(Vector2Int gridPos)
    {
        gimmickGridPos = gridPos;
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

            HitChecker hit = hitChecker.GetComponent<HitChecker>();
            if (hit != null)
            {
                hit.SetHitDamage(attackPower);
                hit.SetEffectDamage(effectPower);
                hit.HitLoop(gimmickType == GimmickType.Reusable);
                hit.SetGimmick(gimmick);
                hit.SetParentGameObject(gameObject);
            }

            // 当たり判定の大きさを設定
            GameObject Effect = hitChecker.transform.Find("Effect").gameObject;
            GameObject Hit = hitChecker.transform.Find("Hit").gameObject;

            Vector3 EffectSize = new Vector3(effectRangeX * roomGrid.gridSize.x,effectRangeY * roomGrid.gridSize.y, effectRangeZ * roomGrid.gridSize.y);
            Vector3 HitSize = new Vector3(hitRangeX * roomGrid.gridSize.x, hitRangeY * roomGrid.gridSize.y, hitRangeZ * roomGrid.gridSize.y);
            
            EffectSize.x = EffectSize.x * (float)Adjust;
            EffectSize.y = EffectSize.y * (float)Adjust;
            EffectSize.z = EffectSize.z * (float)Adjust;

            HitSize.x = HitSize.x * (float)Adjust;
            HitSize.y = HitSize.y * (float)Adjust;
            HitSize.z = HitSize.z * (float)Adjust;

            Effect.transform.localScale = EffectSize;
            Hit.transform.localScale = HitSize;
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
        HitCheckerPos.y = (HitCheckerPos.y * (float)Adjust) + ((effectRangeY * roomGrid.gridSize.y) / 2.0f);
        HitCheckerPos.z = HitCheckerPos.z * (float)Adjust;

        hitChecker.transform.position = HitCheckerPos;
    }

    /// <summary>
    /// 泥棒に対する当たり判定を設定する関数（ワールド座標版）
    /// </summary>
    /// <param name="WorldPos">ワールド座標</param>
    protected void SetHitChecker(Vector3 WorldPos)
    {
        if (hitChecker == null)
        {
            hitChecker = Instantiate(hitCheckerPrefab);

            HitChecker hit = hitChecker.GetComponent<HitChecker>();
            if (hit != null)
            {
                hit.SetHitDamage(attackPower);
                hit.SetEffectDamage(effectPower);
                hit.HitLoop(gimmickType == GimmickType.Reusable);
                hit.SetGimmick(gimmick);
                hit.SetParentGameObject(gameObject);
            }

            // 当たり判定の大きさを設定
            GameObject Effect = hitChecker.transform.Find("Effect").gameObject;
            GameObject Hit = hitChecker.transform.Find("Hit").gameObject;

            Vector3 EffectSize = new Vector3(effectRangeX * roomGrid.gridSize.x, effectRangeY * roomGrid.gridSize.y, effectRangeZ * roomGrid.gridSize.y);
            Vector3 HitSize = new Vector3(hitRangeX * roomGrid.gridSize.x, hitRangeY * roomGrid.gridSize.y, hitRangeZ * roomGrid.gridSize.y);

            EffectSize.x = EffectSize.x * (float)Adjust;
            EffectSize.y = EffectSize.y * (float)Adjust;
            EffectSize.z = EffectSize.z * (float)Adjust;

            HitSize.x = HitSize.x * (float)Adjust;
            HitSize.y = HitSize.y * (float)Adjust;
            HitSize.z = HitSize.z * (float)Adjust;

            Effect.transform.localScale = EffectSize;
            Hit.transform.localScale = HitSize;
        }
        hitChecker.transform.position = WorldPos;
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
                return new Vector2Int(0, 1);
            case GimmickDirection.Down:
                return new Vector2Int(0, -1);
            case GimmickDirection.Left:
                return new Vector2Int(1, 0);
            case GimmickDirection.Right:
                return new Vector2Int(-1, 0);
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

    /// <summary>
    /// BoxColliderを使用して、命中範囲内の敵を検出する関数
    /// </summary>
    /// <param name="box">検出範囲のBoxCollider</param>
    /// <returns>検出された敵のコライダー配列</returns>
    private Collider[] OverlapBoxCollider(BoxCollider box)
    {
        if (box == null) return new Collider[0];

        // コライダーのワールド座標でのCenter・Size・回転を取得
        Vector3 worldCenter = box.transform.TransformPoint(box.center);
        Vector3 worldHalfExtents = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);
        Quaternion worldRotation = box.transform.rotation;

        return Physics.OverlapBox(worldCenter, worldHalfExtents, worldRotation, enemyLayer);
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
            case GimmickState.Search:
            // Search状態の処理
                SearchUpdate();
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
    protected virtual void SearchUpdate()
    {
        gimmickState = GimmickState.Active;

        // Collider内の泥棒を検知する処理
        Collider[] hitsX = OverlapBoxCollider(searchColliderX);
        Collider[] hitsZ = OverlapBoxCollider(searchColliderZ);

        // X軸・Z軸の検知結果を結合
        List<Collider> allHits = new List<Collider>(hitsX);
        foreach (var col in hitsZ)
        {
            if (!allHits.Contains(col))
                allHits.Add(col);
        }

        Debug.Log("Detected " + allHits.Count + " enemies in search area.");
        if (allHits.Count == 0) return;

        // 最も近い敵を探す
        float minDist = float.MaxValue;
        Transform nearestEnemy = null;

        foreach (var col in allHits)
        {
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestEnemy = col.transform;
            }
        }

        Debug.Log("Nearest enemy: " + (nearestEnemy != null ? nearestEnemy.name : "None") + ", Distance: " + minDist);
        if (nearestEnemy == null) return;

        // 自身から敵への方向ベクトル（XZ平面）
        Vector3 diff = nearestEnemy.position - transform.position;

        if (Mathf.Abs(diff.x) >= Mathf.Abs(diff.z))
        {
            gimmickDirection = diff.x >= 0f ? GimmickDirection.Left : GimmickDirection.Right;
        }
        else
        {
            gimmickDirection = diff.z >= 0f ? GimmickDirection.Up : GimmickDirection.Down;
        }
        Debug.Log("Detected enemy: " + nearestEnemy.name + ", Direction: " + gimmickDirection);

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

