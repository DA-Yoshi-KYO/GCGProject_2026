using System.Collections.Generic;
using UnityEngine;

public enum Gimmick
{
    None,
    Pot,
    IronBall,
    EmptyChest,
    Nyaki,
    Pitfall,
    MagicAnkh,
    CloneCat,
    Teleport,
}

public enum GimmickState
{
    Preview,
    Spawn,
    Idle,
    Search,
    Active,
    Cooldown,
    Broken,
};

public enum GimmickType
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

public enum GimmickMaterial
{
    Material,
    NoMaterial,
}
public enum GimmickOutline
{
    Default,
    NoOutline,
}

public static class GimmickPlacementSurfaceRules
{
    private const string FloorsGroupName = "Floors";
    private const string SecondFloorsGroupName = "SecondFloors";
    private const string PolesGroupName = "Poles";
    private const string PartitionGroupNamePart = "Partition";

    public static bool IsAllowed(
        Gimmick gimmick,
        Transform surfaceTransform)
    {
        switch (gimmick)
        {
            case Gimmick.Pot:
                return IsInGroup(
                    surfaceTransform,
                    PolesGroupName) ||
                       IsInGroupContaining(
                           surfaceTransform,
                           PartitionGroupNamePart);

            case Gimmick.IronBall:
            case Gimmick.EmptyChest:
            case Gimmick.Pitfall:
                return IsInGroup(
                    surfaceTransform,
                    FloorsGroupName,
                    SecondFloorsGroupName);

            default:
                return true;
        }
    }

    private static bool IsInGroup(
        Transform surfaceTransform,
        params string[] groupNames)
    {
        if (surfaceTransform == null)
            return false;

        Transform current = surfaceTransform;
        while (current != null)
        {
            foreach (string groupName in groupNames)
            {
                if (current.name == groupName)
                    return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static bool IsInGroupContaining(
        Transform surfaceTransform,
        string groupNamePart)
    {
        if (surfaceTransform == null)
            return false;

        Transform current = surfaceTransform;
        while (current != null)
        {
            if (current.name.Contains(groupNamePart))
                return true;

            current = current.parent;
        }

        return false;
    }
}

public class GimmickBase : MonoBehaviour
{
    // -----------------------------------------------------------------
    // 定数
    // -----------------------------------------------------------------
    private const string AlphaPropertyName = "_Alpha";
    private const string SearchAxisXName = "X";
    private const string SearchAxisZName = "Z";
    private const string HitCheckerEffectName = "Effect";
    private const string HitCheckerHitName = "Hit";
    private const string HitCheckerSearchName = "Search";
    private const string AudioManagerPath = "AudioManager/3DSE";
    private const string SummonSeName = "Gimmick_Summon";
    private const string SummonSeCategory = "Summon";

    // GimmickDirection -> 回転 / 方向ベクトル の対応表（switch の重複を排除）
    private static readonly Dictionary<GimmickDirection, Quaternion> DirectionRotations = new Dictionary<GimmickDirection, Quaternion>
    {
        { GimmickDirection.Up,    Quaternion.Euler(0, 0, 0) },
        { GimmickDirection.Down,  Quaternion.Euler(0, 180, 0) },
        { GimmickDirection.Left,  Quaternion.Euler(0, 90, 0) },
        { GimmickDirection.Right, Quaternion.Euler(0, -90, 0) },
    };

    private static readonly Dictionary<GimmickDirection, Vector2Int> DirectionVectors = new Dictionary<GimmickDirection, Vector2Int>
    {
        { GimmickDirection.Up,    new Vector2Int(0, 1) },
        { GimmickDirection.Down,  new Vector2Int(0, -1) },
        { GimmickDirection.Left,  new Vector2Int(1, 0) },
        { GimmickDirection.Right, new Vector2Int(-1, 0) },
    };

    public Sprite gimmickImage;
    public Sprite gimmickTextImage;

    [Header("大きさ")]
    [Tooltip("グリッド基準の大きさ"), Min(0)]
    [SerializeField] protected Vector2Int gimmickSize;
    [Tooltip("拡縮率 / ％"), Min(0)]
    [SerializeField] protected float gimmickScale = 100;

    [Header("HitChecker")]
    [SerializeField] protected GameObject hitCheckerPrefab;

    [Header("当たり判定")]
    [SerializeField] protected Vector3 hitRange;
    [SerializeField] protected Vector3 effectRange;
    [SerializeField] protected Vector3 searchRange;

    [Header("攻撃力")]
    [SerializeField] protected int attackPower;
    [SerializeField] protected int effectPower;

    public RoomGrid roomGrid;

    [Header("ギミックの向き")]
    [SerializeField] protected GimmickDirection gimmickDirection;
    [Header("ギミックのタイプ")]
    [SerializeField] protected GimmickType gimmickType;
    [Header("ギミックの種類")]
    [SerializeField] public Gimmick gimmick;
    [Header("ギミックの状態")]
    [SerializeField] public GimmickState gimmickState;
    [Header("マテリアルの種別")]
    [SerializeField] private GimmickMaterial gimmickMaterial;
    [Header("アウトラインの種別")]
    [SerializeField] private GimmickOutline gimmickOutline;

    [Header("泥棒検知")]
    [SerializeField] protected GameObject search;
    [SerializeField] protected int searchGridRange;

    [Header("敵のレイヤー")]
    [SerializeField] protected LayerMask enemyLayer;

    [Header("召喚速度")]
    [SerializeField] protected float spawnSpeed;
    [Header("召喚震度")]
    [SerializeField] protected float spawnVibration;
    [Header("振動感覚")]
    [SerializeField] protected int spawnVibrationSpeed;
    private int spawnVibrationCount = 0;
    private bool isOffsetSet = false;

    [Header("RendererComponent")]
    [SerializeField] private MeshRenderer[] mesh;
    // マテリアル未使用時にも null 参照が起きないよう、常に空配列で初期化しておく
    private Material[] materials = System.Array.Empty<Material>();

    [Header("MeshScaleAdaptation")]
    [SerializeField] private bool isMeshScaleAdaptation;

    [Header("Broken Fade")]
    [SerializeField] private float brokenFadeSpeed = 1.0f;
    private float brokenAlpha = 1.0f;
    private bool brokenFadeStart = false;

    //設置プレビュー用
    [Header("Preview")]
    [SerializeField] private float previewAlpha = 0.5f;

    [Header("Effect再生処理")]
    [SerializeField]
    private CS_EffectPlayer cs_EffectPlayer;
    [SerializeField] private Vector3 effectOffsetPosition = Vector3.zero;
    [SerializeField] private float effectOffsetDirection = 0.0f;
    [SerializeField] private Quaternion effectOffsetRotation = Quaternion.identity;
    [SerializeField] private bool effectRotationUseDirection = true;
    [SerializeField] private Vector3 effectOffsetScale = Vector3.one;

    protected Vector2Int gimmickGridPos;
    protected Vector3 targetPoint;
    protected Vector3 currentPoint;

    protected GameObject hitChecker;
    protected BoxCollider searchColliderX;
    protected BoxCollider searchColliderZ;
    protected CS_3DPlaySE gimmickSound;
    protected int roomIndex;

    private HitChecker hit;
    public HitChecker read_Hit => hit;

    protected CS_OutlineTarget outlineTarget;
    protected float outlineWidth = 6f;

    private readonly HashSet<Collider> searchHitBuffer = new HashSet<Collider>();

    protected virtual void Start()
    {
        InitSearchColliders();

        targetPoint = transform.position;
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y - transform.localScale.y / 2.0f,
            transform.position.z
        );

        InitSound();

        GameObject rayRoomCreatePoint = CS_RoomCreatePointRaycast.GetRayRoomCreatePoint(this.gameObject);
        if (rayRoomCreatePoint == null)
        {
            Debug.LogError("GimmickBase: 床下のRoomCreatePointが取得できません: " + gameObject.name);
        }
        else
        {
            roomIndex = rayRoomCreatePoint.transform.GetSiblingIndex();
        }

        if (gimmickMaterial == GimmickMaterial.Material)
            InitMaterials();

        if (gimmickOutline == GimmickOutline.Default)
        {
            outlineTarget = GetComponentInChildren<CS_OutlineTarget>();
            if (outlineTarget != null)
                outlineTarget.SetOutline(Color.gray, outlineWidth);
            else
                Debug.LogWarning("CS_OutlineTargetが見つかりません: " + gameObject.name);
        }
    }

    private void InitSearchColliders()
    {
        if (search == null)
        {
            Debug.LogWarning("searchが設定されていません: " + gameObject.name);
            return;
        }

        Transform xTransform = search.transform.Find(SearchAxisXName);
        Transform zTransform = search.transform.Find(SearchAxisZName);

        if (xTransform == null || zTransform == null)
        {
            Debug.LogWarning("searchの子オブジェクト(X/Z)が見つかりません: " + gameObject.name);
            return;
        }

        searchColliderX = xTransform.GetComponent<BoxCollider>();
        searchColliderZ = zTransform.GetComponent<BoxCollider>();
    }

    private void InitSound()
    {
        GameObject soundManager = GameObject.Find(AudioManagerPath);
        if (soundManager != null)
        {
            gimmickSound = soundManager.GetComponent<CS_3DPlaySE>();
        }

        if (gimmickSound == null)
        {
            Debug.LogWarning("CS_3DPlaySEコンポーネントが見つかりません。サウンドが再生されません。");
        }
        else
        {
            //召喚SE再生
            gimmickSound.PlayOneShotSE(SummonSeName, transform.position, SummonSeCategory);
        }
    }

    private void InitMaterials()
    {
        mesh = GetComponentsInChildren<MeshRenderer>(true);

        List<Material> materialList = new List<Material>();

        foreach (MeshRenderer renderer in mesh)
        {
            materialList.AddRange(renderer.materials);
        }

        materials = materialList.ToArray();
        SetMaterialsAlpha(1.0f);
    }

    private void SetMaterialsAlpha(float alpha)
    {
        if (materials == null)
            return;

        foreach (Material mat in materials)
        {
            if (mat != null && mat.HasProperty(AlphaPropertyName))
            {
                mat.SetFloat(AlphaPropertyName, alpha);
            }
        }
    }

    public void AdjustScaleToGrid()
    {
        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();

        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogWarning("MeshFilterが見つかりません: " + gameObject.name);
            return;
        }

        Vector3 meshSize = meshFilter.sharedMesh.bounds.size;

        float targetSizeX = gimmickSize.x * roomGrid.gridSize.x;
        float targetSizeZ = gimmickSize.y * roomGrid.gridSize.y;

        float scaleX = targetSizeX;
        float scaleZ = targetSizeZ;

        if (isMeshScaleAdaptation)
        {//メッシュスケール適応
            scaleX /= meshSize.x;
            scaleZ /= meshSize.z;
        }
        float scaleY = (scaleX + scaleZ) / 2f;

        scaleX = scaleX * gimmickScale / 100f;
        scaleY = scaleY * gimmickScale / 100f;
        scaleZ = scaleZ * gimmickScale / 100f;

        transform.localScale = new Vector3(scaleX, scaleY, scaleX);

        if (search == null)
            return;

        Vector3 set = search.transform.position;
        set.y = 0.0f;
        search.transform.position = set;

        Transform xTransform = search.transform.Find(SearchAxisXName);
        Transform zTransform = search.transform.Find(SearchAxisZName);

        if (xTransform == null || zTransform == null)
            return;

        Vector3 searchX = xTransform.localScale;
        Vector3 searchZ = zTransform.localScale;

        searchX.x = searchGridRange * roomGrid.gridSize.x;
        searchZ.z = searchGridRange * roomGrid.gridSize.y;

        xTransform.localScale = searchX;
        zTransform.localScale = searchZ;
    }

    public void ActivateGimmick()
    {
        if (gimmickState == GimmickState.Idle)
        {
            gimmickState = GimmickState.Search;
        }
    }

    public void SetGimmickPos(Vector2Int gridPos)
    {
        gimmickGridPos = gridPos;
    }

    public void SetGimmickPos(Vector3 worldPos)
    {
        Vector2Int gridPos = roomGrid.GetGridFromPos(worldPos);
        SetGimmickPos(gridPos);
    }

    public void SetGimmickDirection(GimmickDirection direction)
    {
        gimmickDirection = direction;
    }

    protected void SetHitChecker(int gridX, int gridY)
    {
        Vector3 hitCheckerPos = roomGrid.GetWorldPosFromGrid(new Vector2Int(gridX, gridY));

        if (float.IsInfinity(hitCheckerPos.x) ||
            float.IsInfinity(hitCheckerPos.y) ||
            float.IsInfinity(hitCheckerPos.z) ||
            gridX < 0 ||
            gridY < 0)
        {
            Debug.LogWarning("SetHitChecker: Invalid grid position (" + gridX + ", " + gridY + ")");
            DeleteHitChecker();
            return;
        }

        hitCheckerPos.y += (effectRange.y * roomGrid.gridSize.y) / 2.0f;

        EnsureHitCheckerCreated(useHitRangeForHit: false);
        hitChecker.transform.position = hitCheckerPos;
    }

    protected void SetHitChecker(Vector3 worldPos)
    {
        EnsureHitCheckerCreated(useHitRangeForHit: true);
        hitChecker.transform.position = worldPos;
    }

    private void EnsureHitCheckerCreated(bool useHitRangeForHit)
    {
        if (hitChecker != null)
            return;

        if (hitCheckerPrefab == null)
        {
            Debug.LogWarning("hitCheckerPrefabが設定されていません: " + gameObject.name);
            return;
        }

        hitChecker = Instantiate(hitCheckerPrefab);
        hit = hitChecker.GetComponent<HitChecker>();

        if (hit != null)
        {
            hit.SetHitDamage(attackPower);
            hit.SetEffectDamage(effectPower);
            hit.HitLoop(gimmickType == GimmickType.Reusable);
            hit.SetGimmick(gimmick);
            hit.SetParentGameObject(gameObject);
        }

        Transform effectT = hitChecker.transform.Find(HitCheckerEffectName);
        Transform hitT = hitChecker.transform.Find(HitCheckerHitName);
        Transform searchT = hitChecker.transform.Find(HitCheckerSearchName);

        if (effectT == null || hitT == null || searchT == null)
        {
            Debug.LogWarning("hitCheckerPrefabにEffect/Hit/Searchの子オブジェクトが見つかりません: " + hitCheckerPrefab.name);
            return;
        }

        Vector3 effectSize = new Vector3(
            effectRange.x * roomGrid.gridSize.x,
            effectRange.y * roomGrid.gridSize.y,
            effectRange.z * roomGrid.gridSize.y
        );

        Vector3 hitSize = useHitRangeForHit
            ? new Vector3(
                hitRange.x * roomGrid.gridSize.x,
                hitRange.y * roomGrid.gridSize.y,
                hitRange.z * roomGrid.gridSize.y)
            : new Vector3(
                effectRange.x * roomGrid.gridSize.x,
                effectRange.y * roomGrid.gridSize.y,
                hitRange.z * roomGrid.gridSize.y);

        Vector3 searchSize = useHitRangeForHit
            ? new Vector3(
                searchRange.x * roomGrid.gridSize.x,
                searchRange.y * roomGrid.gridSize.y,
                searchRange.z * roomGrid.gridSize.y)
            : new Vector3(
                effectRange.x * roomGrid.gridSize.x,
                effectRange.y * roomGrid.gridSize.y,
                searchRange.z * roomGrid.gridSize.y);

        effectT.localScale = effectSize;
        hitT.localScale = hitSize;
        searchT.localScale = searchSize;
    }

    //エフェクト
    protected void PlayEffectPlayer()
    {
        PlayEffectPlayer(transform.position);
    }

    protected void PlayEffectPlayer(Vector3 basePosition)
    {
        if (cs_EffectPlayer == null)
        {
            cs_EffectPlayer = GetComponent<CS_EffectPlayer>();
        }

        if (cs_EffectPlayer == null)
        {
            Debug.LogWarning(
                "[CS_EffectTest] CS_EffectPlayerがありません。"
            );
            return;
        }

        Quaternion directionRotation = GetEffectDirectionRotation();
        Quaternion baseRotation =
            effectRotationUseDirection ? directionRotation : transform.rotation;
        Quaternion effectRotation =
            baseRotation * GetSafeEffectOffsetRotation();

        Vector3 effectPosition =
            basePosition +
            directionRotation * effectOffsetPosition +
            directionRotation * Vector3.forward * effectOffsetDirection;

        CSST_EffectPlayData csst_EffectPlayData =
            new CSST_EffectPlayData();

        csst_EffectPlayData.CSST_EffectPlayData_Init();

        csst_EffectPlayData.SetPosition(
            effectPosition
        );

        csst_EffectPlayData.SetRotation(
            effectRotation
        );

        if (effectOffsetScale != Vector3.zero)
        {
            csst_EffectPlayData.SetScale(
                effectOffsetScale
            );
        }

        csst_EffectPlayData.SetLoopFlag(false);
        csst_EffectPlayData.SetHideOnEnd(true);

        CSAD_EffectCommonProcessBase smokeEffect =
            cs_EffectPlayer.PlayEffect(
                csst_EffectPlayData
            );

        if (smokeEffect != null)
        {
            smokeEffect.transform.SetParent(null, true);
        }
    }

    protected virtual bool GetGimmickSettingsArea()
    {
        return true;
    }

    protected bool TryGetPlacementSurface(out RaycastHit surfaceHit)
    {
        const float rayOffset = 0.1f;
        const float rayLength = 0.2f;
        Vector3 rayOrigin =
            placementCheckPosition + Vector3.up * rayOffset;

        return Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out surfaceHit,
            rayLength,
            ~0,
            QueryTriggerInteraction.Ignore);
    }

    protected bool IsPlacementSurfaceAllowed(
        Transform surfaceTransform)
    {
        return GimmickPlacementSurfaceRules.IsAllowed(
            gimmick,
            surfaceTransform);
    }

    //設置可能位置であるかを判定
    public bool GetIsSettingArea()
    {
        placementCheckPosition = transform.position;
        return GetGimmickSettingsArea();
    }

    // プレビューなど、まだギミック本体が生成されていない位置の設置判定用
    protected Vector3 placementCheckPosition;

    public bool GetIsSettingArea(Vector3 worldPosition)
    {
        placementCheckPosition = worldPosition;
        return GetGimmickSettingsArea();
    }

    private Quaternion GetEffectDirectionRotation()
    {
        if (DirectionRotations.TryGetValue(gimmickDirection, out Quaternion directionRotation))
        {
            return directionRotation;
        }

        return transform.rotation;
    }

    private Quaternion GetSafeEffectOffsetRotation()
    {
        if (Mathf.Approximately(effectOffsetRotation.x, 0.0f) &&
            Mathf.Approximately(effectOffsetRotation.y, 0.0f) &&
            Mathf.Approximately(effectOffsetRotation.z, 0.0f) &&
            Mathf.Approximately(effectOffsetRotation.w, 0.0f))
        {
            return Quaternion.identity;
        }

        return effectOffsetRotation;
    }

    public CS_ThiefGimmickAction GetThiefGimmickAction()
    {
        return hit != null ? hit.GetThiefGA() : null;
    }

    protected void DeleteHitChecker()
    {
        if (hitChecker != null)
        {
            Destroy(hitChecker);
            hitChecker = null;
            hit = null;
        }
    }

    public Vector2Int GetDirectionVec()
    {
        return DirectionVectors.TryGetValue(gimmickDirection, out Vector2Int vec) ? vec : Vector2Int.zero;
    }

    public Vector2Int GetGimmickSize()
    {
        return gimmickSize;
    }

    public Gimmick GetGimmickTag()
    {
        return gimmick;
    }

    public Vector3 GetHitRange()
    {
        return hitRange;
    }

    public Vector3 GetEffectRange()
    {
        return effectRange;
    }

    public int GetRoomIndex()
    {
        return roomIndex;
    }
    public void SetRoomIndex(int index)
    {
        roomIndex = index;
    }

    private Collider[] OverlapBoxCollider(BoxCollider box)
    {
        if (box == null)
        {
            return System.Array.Empty<Collider>();
        }

        Vector3 worldCenter = box.transform.TransformPoint(box.center);
        Vector3 worldHalfExtents = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);
        Quaternion worldRotation = box.transform.rotation;

        return Physics.OverlapBox(worldCenter, worldHalfExtents, worldRotation, enemyLayer);
    }

    private void FixedUpdate()
    {
        switch (gimmickState)
        {
            case GimmickState.Preview:
                PreviewUpdate();
                break;

            case GimmickState.Spawn:
                SpawnUpdate();
                break;

            case GimmickState.Idle:
                IdleUpdate();
                break;

            case GimmickState.Search:
                SearchUpdate();
                break;

            case GimmickState.Active:
                ActiveUpdate();
                break;

            case GimmickState.Cooldown:
                CooldownUpdate();
                break;

            case GimmickState.Broken:
                BrokenUpdate();
                break;
        }
    }

    protected virtual void PreviewUpdate()
    {
        ApplyDirectionRotation();
        SetMaterialsAlpha(previewAlpha);
    }

    protected virtual void SpawnUpdate()
    {
        if (transform.position.y < targetPoint.y + transform.localScale.y / 2.0f)
        {
            currentPoint = transform.position;
            currentPoint.y += spawnSpeed * Time.deltaTime;
            if (spawnVibrationCount > spawnVibrationSpeed)
            {
                if (!isOffsetSet)
                {
                    currentPoint.x += Random.Range(-spawnVibration, spawnVibration);
                    currentPoint.z += Random.Range(-spawnVibration, spawnVibration);
                    isOffsetSet = true;
                }
                else
                {
                    currentPoint = transform.position;
                    isOffsetSet = false;
                }
                spawnVibrationCount = 0;
            }
            spawnVibrationCount++;
            transform.position = currentPoint;
            ApplyDirectionRotation();
        }
        else
        {
            transform.position = targetPoint;
            SetMaterialsAlpha(1f);
            gimmickState = GimmickState.Idle;
        }
    }

    private void ApplyDirectionRotation()
    {
        if (DirectionRotations.TryGetValue(gimmickDirection, out Quaternion rot))
        {
            transform.rotation = rot;
        }
    }

    protected virtual void IdleUpdate()
    {
    }

    protected virtual void SearchUpdate()
    {
        gimmickState = GimmickState.Active;

        bool hasSearchTarget =
            TryGetSearchDirection(
                out GimmickDirection searchDirection,
                out Transform nearestEnemy,
                out int detectedEnemyCount,
                out float nearestDistance);

        Debug.Log(
            "Detected " +
            detectedEnemyCount +
            " enemies in search area.");

        if (!hasSearchTarget)
            return;

        gimmickDirection = searchDirection;

        Debug.Log(
            "Nearest enemy: " +
            nearestEnemy.name +
            ", Distance: " +
            nearestDistance);
        Debug.Log(
            "Detected enemy: " +
            nearestEnemy.name +
            ", Direction: " +
            gimmickDirection);
    }

    public bool TryGetSearchDirection(
        out GimmickDirection direction)
    {
        return TryGetSearchDirection(
            out direction,
            out _,
            out _,
            out _);
    }

    private bool TryGetSearchDirection(
        out GimmickDirection direction,
        out Transform nearestEnemy,
        out int detectedEnemyCount,
        out float nearestDistance)
    {
        direction = gimmickDirection;
        nearestEnemy = null;
        nearestDistance = float.MaxValue;

        Collider[] hitsX = OverlapBoxCollider(searchColliderX);
        Collider[] hitsZ = OverlapBoxCollider(searchColliderZ);

        searchHitBuffer.Clear();
        foreach (Collider col in hitsX) searchHitBuffer.Add(col);
        foreach (Collider col in hitsZ) searchHitBuffer.Add(col);

        detectedEnemyCount = searchHitBuffer.Count;

        if (searchHitBuffer.Count == 0)
        {
            return false;
        }

        foreach (Collider col in searchHitBuffer)
        {
            if (col == null)
                continue;

            if (!CanPrioritizeSearchTarget(col))
                continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);

            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearestEnemy = col.transform;
            }
        }

        if (nearestEnemy == null)
        {
            return false;
        }

        Vector3 diff = nearestEnemy.position - transform.position;

        if (Mathf.Abs(diff.x) >= Mathf.Abs(diff.z))
        {
            direction = diff.x >= 0f
                ? GimmickDirection.Left
                : GimmickDirection.Right;
        }
        else
        {
            direction = diff.z >= 0f
                ? GimmickDirection.Down
                : GimmickDirection.Up;
        }

        return true;
    }

    private bool CanPrioritizeSearchTarget(
        Collider targetCollider)
    {
        if (targetCollider == null)
            return false;

        // ギミックごとのサーチ対象優先条件はここへ追加する
        switch (gimmick)
        {
            case Gimmick.IronBall:
                int rockSearchObstacleLayerMask =
                    LayerMask.GetMask("VisionObstacle");

                return HasClearHorizontalSearchPath(
                    targetCollider,
                    rockSearchObstacleLayerMask);

            default:
                return true;
        }
    }

    private bool HasClearHorizontalSearchPath(
        Collider targetCollider,
        int obstacleLayerMask)
    {
        Collider gimmickCollider = GetComponent<Collider>();
        Vector3 origin =
            gimmickCollider != null
                ? gimmickCollider.bounds.center
                : transform.position;
        Vector3 target = targetCollider.bounds.center;
        target.y = origin.y;
        Vector3 toTarget = target - origin;
        float distance = toTarget.magnitude;

        if (distance <= Mathf.Epsilon)
            return true;

        return !Physics.Raycast(
            origin,
            toTarget / distance,
            distance,
            obstacleLayerMask,
            QueryTriggerInteraction.Ignore);
    }

    protected virtual void ActiveUpdate()
    {
    }

    protected virtual void CooldownUpdate()
    {
    }

    protected virtual void BrokenUpdate()
    {
        if (!brokenFadeStart)
        {
            brokenAlpha = 1.0f;
            brokenFadeStart = true;
            SetMaterialsAlpha(brokenAlpha);
        }

        brokenAlpha -= brokenFadeSpeed * Time.deltaTime;
        brokenAlpha = Mathf.Clamp01(brokenAlpha);

        SetMaterialsAlpha(brokenAlpha);

        if (outlineTarget != null)
            outlineTarget.SetOutlineAlpha(0.0f);

        if (brokenAlpha <= 0.0f)
        {
            Destroy(gameObject);
        }
    }

    public void SetOutLineColor(Color col)
    {
        if (outlineTarget != null)
            outlineTarget.SetOutlineColor(col);
    }

    public CS_3DPlaySE GetGimmickSound()
    {
        return gimmickSound;
    }
}
