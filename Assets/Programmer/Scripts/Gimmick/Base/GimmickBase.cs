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

public class GimmickBase : MonoBehaviour
{
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
    private Material[] materials;

    [Header("MeshScaleAdaptation")]
    [SerializeField] private bool isMeshScaleAdaptation;

    [Header("Broken Fade")]
    [SerializeField] private float brokenFadeSpeed = 1.0f;
    private float brokenAlpha = 1.0f;
    private bool brokenFadeStart = false;

    //設置プレビュー用
    [Header("Preview")]
    [SerializeField] private float previewAlpha = 0.5f;

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

    protected CS_OutlineController outlineController;
    protected float outlineWidth = 0.1f;

    private void Start()
    {
        GameObject X = search.transform.Find("X").gameObject;
        GameObject Z = search.transform.Find("Z").gameObject;
        searchColliderX = X.GetComponent<BoxCollider>();
        searchColliderZ = Z.GetComponent<BoxCollider>();

        GameObject soundManager = GameObject.Find("AudioManager");
        if (soundManager != null)
        {
            gimmickSound = soundManager.GetComponentInChildren<CS_3DPlaySE>();
        }

        if (gimmickSound == null)
        {
            Debug.LogWarning("CS_3DPlaySEコンポーネントが見つかりません。サウンドが再生されません。");
        }

        targetPoint = transform.position;
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y - transform.localScale.y / 2.0f,
            transform.position.z
        );

        InitMaterials();
        roomIndex = CS_RoomCreatePointRaycast.GetRayRoomCreatePoint(this.gameObject).transform.GetSiblingIndex();

        outlineController = new CS_OutlineController(GetComponentInChildren<Renderer>());
        outlineController.SetOutline(Color.gray, outlineWidth);
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

        foreach (Material mat in materials)
        {
            if (mat != null && mat.HasProperty("_Alpha"))
            {
                mat.SetFloat("_Alpha", 1.0f);
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

        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

        Vector3 set = search.transform.position;
        set.y = 0.0f;
        search.transform.position = set;

        GameObject X = search.transform.Find("X").gameObject;
        GameObject Z = search.transform.Find("Z").gameObject;

        Vector3 searchX = X.transform.localScale;
        Vector3 searchZ = Z.transform.localScale;

        searchX.x = searchGridRange * roomGrid.gridSize.x;
        searchZ.z = searchGridRange * roomGrid.gridSize.y;

        X.transform.localScale = searchX;
        Z.transform.localScale = searchZ;
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
        if (hitChecker == null)
        {
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

            GameObject effect = hitChecker.transform.Find("Effect").gameObject;
            GameObject hitObj = hitChecker.transform.Find("Hit").gameObject;
            GameObject searchObj = hitChecker.transform.Find("Search").gameObject;

            Vector3 effectSize = new Vector3(
                effectRange.x * roomGrid.gridSize.x,
                effectRange.y * roomGrid.gridSize.y,
                effectRange.z * roomGrid.gridSize.y
            );

            Vector3 hitSize = new Vector3(
                effectRange.x * roomGrid.gridSize.x,
                effectRange.y * roomGrid.gridSize.y,
                hitRange.z * roomGrid.gridSize.y
            );

            Vector3 searchSize = new Vector3(
                effectRange.x * roomGrid.gridSize.x,
                effectRange.y * roomGrid.gridSize.y,
                searchRange.z * roomGrid.gridSize.y
            );

            effect.transform.localScale = effectSize;
            hitObj.transform.localScale = hitSize;
            searchObj.transform.localScale = searchSize;
        }

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
        hitChecker.transform.position = hitCheckerPos;
    }

    protected void SetHitChecker(Vector3 worldPos)
    {
        if (hitChecker == null)
        {
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

            GameObject effect = hitChecker.transform.Find("Effect").gameObject;
            GameObject hitObj = hitChecker.transform.Find("Hit").gameObject;
            GameObject searchObj = hitChecker.transform.Find("Search").gameObject;

            Vector3 effectSize = new Vector3(
                effectRange.x * roomGrid.gridSize.x,
                effectRange.y * roomGrid.gridSize.y,
                effectRange.z * roomGrid.gridSize.y
            );

            Vector3 hitSize = new Vector3(
                hitRange.x * roomGrid.gridSize.x,
                hitRange.y * roomGrid.gridSize.y,
                hitRange.z * roomGrid.gridSize.y
            );

            Vector3 searchSize = new Vector3(
                searchRange.x * roomGrid.gridSize.x,
                searchRange.y * roomGrid.gridSize.y,
                searchRange.z * roomGrid.gridSize.y
            );

            effect.transform.localScale = effectSize;
            hitObj.transform.localScale = hitSize;
            searchObj.transform.localScale = searchSize;
        }

        hitChecker.transform.position = worldPos;
    }

    public CS_ThiefGimmickAction GetThiefGimmickAction()
    {
        return hit.GetThiefGA();
    }

    protected void DeleteHitChecker()
    {
        if (hitChecker != null)
        {
            Destroy(hitChecker);
        }
    }

    public Vector2Int GetDirectionVec()
    {
        switch (gimmickDirection)
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

    public Vector2Int GetGimmickSize()
    {
        return new Vector2Int(gimmickSize.x, gimmickSize.y);
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

    private Collider[] OverlapBoxCollider(BoxCollider box)
    {
        if (box == null)
        {
            return new Collider[0];
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
        switch(gimmickDirection)
        {
            case GimmickDirection.Up:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case GimmickDirection.Down:
                transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
            case GimmickDirection.Left:
                transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
            case GimmickDirection.Right:
                transform.rotation = Quaternion.Euler(0, -90, 0);
                break;
        }

        foreach (Material mat in materials)
        {
            if (mat != null && mat.HasProperty("_Alpha"))
            {
                mat.SetFloat("_Alpha", 0.4f);
            }
        }
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
            switch (gimmickDirection)
            {
                case GimmickDirection.Up:
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case GimmickDirection.Down:
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case GimmickDirection.Left:
                    transform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case GimmickDirection.Right:
                    transform.rotation = Quaternion.Euler(0, -90, 0);
                    break;
            }
        }
        else
        {
            transform.position = targetPoint;
            if (materials == null)
                return;
            foreach (Material mat in materials)
            {
                if (mat != null && mat.HasProperty("_Alpha"))
                {
                    mat.SetFloat("_Alpha", 1f);
                }
            }
            gimmickState = GimmickState.Idle;
        }
    }

    protected virtual void IdleUpdate()
    {
    }

    protected virtual void SearchUpdate()
    {
        gimmickState = GimmickState.Active;

        Collider[] hitsX = OverlapBoxCollider(searchColliderX);
        Collider[] hitsZ = OverlapBoxCollider(searchColliderZ);

        List<Collider> allHits = new List<Collider>(hitsX);

        foreach (Collider col in hitsZ)
        {
            if (!allHits.Contains(col))
            {
                allHits.Add(col);
            }
        }

        Debug.Log("Detected " + allHits.Count + " enemies in search area.");

        if (allHits.Count == 0)
        {
            return;
        }

        float minDist = float.MaxValue;
        Transform nearestEnemy = null;

        foreach (Collider col in allHits)
        {
            float dist = Vector3.Distance(transform.position, col.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                nearestEnemy = col.transform;
            }
        }

        Debug.Log("Nearest enemy: " + (nearestEnemy != null ? nearestEnemy.name : "None") + ", Distance: " + minDist);

        if (nearestEnemy == null)
        {
            return;
        }

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

            if (materials != null)
            {
                foreach (Material mat in materials)
                {
                    if (mat != null && mat.HasProperty("_Alpha"))
                    {
                        mat.SetFloat("_Alpha", brokenAlpha);
                    }
                }
            }
        }

        brokenAlpha -= brokenFadeSpeed * Time.deltaTime;
        brokenAlpha = Mathf.Clamp01(brokenAlpha);

        if (materials != null)
        {
            foreach (Material mat in materials)
            {
                if (mat != null && mat.HasProperty("_Alpha"))
                {
                    mat.SetFloat("_Alpha", brokenAlpha);
                }
            }
        }
        if (outlineController != null)
            outlineController.SetOutlineAlpha(0.0f);

        if (brokenAlpha <= 0.0f)
        {
            Destroy(gameObject);
        }
    }

    public void SetOutLineColor(Color col)
    {
        outlineController.SetOutlineColor(col);
    }
}
