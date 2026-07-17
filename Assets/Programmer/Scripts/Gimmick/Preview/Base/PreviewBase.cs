using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

enum PreviewState
{
    None,
    Start,
    Run,
    End,
}

public class PreviewBase : MonoBehaviour
{
    private static readonly Color PlaceableColor = Color.yellow;
    private static readonly Color UnplaceableColor = Color.red;
    private const float PlacementPlaneAlpha = 0.25f;
    [SerializeField]
    private float PlacementPlaneYOffset;
    [SerializeField]
    private Vector3 PlacementPlaneScale;

    [Header("PreviewGimmickTag")]
    [SerializeField]
    private Gimmick gimmick;

    [Header("PreviewGimmickSize")]
    [SerializeField]
    private Vector3Int prevSize;

    [Header("PreviewIsGimmick ?")]
    [SerializeField]
    private bool isGimmick;

    [Header("RendererComponent")]
    [SerializeField] private MeshRenderer[] mesh;
    private Material[] materials;
    private CS_OutlineTarget[] outlineTargets;
    private GimmickBase placementGimmick;
    private Material placementPlaneMaterial;
    private Renderer placementPlaneRenderer;
    private MaterialPropertyBlock placementPlaneProperties;

    //設置プレビュー用
    [Header("Preview")]
    [SerializeField] private float previewAlpha;

    PreviewState state;

    //:::::::::::::::::::::::::::::::
    // Start

    void Start()
    {
        InitMaterials();
    }
    private void InitMaterials()
    {
        mesh = GetComponentsInChildren<MeshRenderer>(true);
        List<Material> materialList = new List<Material>();

        foreach (MeshRenderer renderer in mesh)
        {
            materialList.AddRange(renderer.materials);
        }

        outlineTargets = GetComponentsInChildren<CS_OutlineTarget>(true);

        materials = materialList.ToArray();

        foreach (Material mat in materials)
        {
            if (mat != null && mat.HasProperty("_Alpha"))
            {
                mat.SetFloat("_Alpha", previewAlpha);
            }
        }
    }

    //:::::::::::::::::::::::::::::::
    // Update

    private void Update()
    {
        UpdatePlacementAppearance();

        switch (state)
        {
            case PreviewState.None:
                break;
            case PreviewState.Start:
                StartUpdate();
                break;
            case PreviewState.Run:
                RunUpdate();
                break;
            case PreviewState.End:
                EndUpdate();
                break;
        }
    }

    public void SetupPlacementPreview(GimmickBase sourceGimmick, RoomGrid grid)
    {
        placementGimmick = sourceGimmick;
        placementGimmick.roomGrid = grid;

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.GetComponent<CS_OutlineTarget>() == null)
                renderer.gameObject.AddComponent<CS_OutlineTarget>();
        }
        outlineTargets = GetComponentsInChildren<CS_OutlineTarget>(true);

        // プレビュー自身を設置判定のレイに含めない
        foreach (Collider previewCollider in GetComponentsInChildren<Collider>(true))
            previewCollider.enabled = false;

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "PlacementGridPlane";
        Destroy(plane.GetComponent<Collider>());
        plane.transform.SetParent(transform, false);
        plane.transform.localPosition = new Vector3(0.0f, PlacementPlaneYOffset, 0.0f);
        plane.transform.localScale = new Vector3(
            sourceGimmick.GetGimmickSize().x * grid.gridSize.x * PlacementPlaneScale.x,
            1.0f * PlacementPlaneScale.y,
            sourceGimmick.GetGimmickSize().y * grid.gridSize.y * PlacementPlaneScale.z);

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            Debug.LogWarning("PreviewBase: No shader was found for the placement plane.");
            Destroy(plane);
            return;
        }
        placementPlaneMaterial = new Material(shader);
        placementPlaneMaterial.name = "PlacementGridPreviewMaterial";
        placementPlaneMaterial.SetFloat("_Surface", 1.0f);
        placementPlaneMaterial.SetFloat("_ZWrite", 0.0f);
        placementPlaneMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        placementPlaneMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        placementPlaneMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        placementPlaneMaterial.renderQueue = 3000;
        placementPlaneRenderer = plane.GetComponent<Renderer>();
        placementPlaneRenderer.sharedMaterial = placementPlaneMaterial;
        placementPlaneProperties = new MaterialPropertyBlock();

        UpdatePlacementAppearance();
    }

    private void UpdatePlacementAppearance()
    {
        if (placementGimmick == null)
            return;

        bool canPlace =
            placementGimmick.GetIsSettingArea(transform.position) &&
            (placementGimmick.roomGrid == null ||
             !placementGimmick.roomGrid.IsTreasureAtPosition(transform.position));
        Color color = canPlace ? PlaceableColor : UnplaceableColor;

        if (outlineTargets != null)
        {
            foreach (CS_OutlineTarget target in outlineTargets)
            {
                if (target != null)
                    target.SetOutlineColor(color);
            }
        }

        if (placementPlaneMaterial != null && placementPlaneRenderer != null)
        {
            color.a = PlacementPlaneAlpha;
            placementPlaneMaterial.color = color;
            if (placementPlaneMaterial.HasProperty("_BaseColor"))
                placementPlaneMaterial.SetColor("_BaseColor", color);

            placementPlaneRenderer.GetPropertyBlock(placementPlaneProperties);
            placementPlaneProperties.SetColor("_Color", color);
            placementPlaneProperties.SetColor("_BaseColor", color);
            placementPlaneRenderer.SetPropertyBlock(placementPlaneProperties);
        }
    }

    private void OnDestroy()
    {
        if (placementPlaneMaterial != null)
            Destroy(placementPlaneMaterial);
    }

    protected void StartUpdate()
    { }
    protected void RunUpdate()
    { }
    protected void EndUpdate()
    { }

    private void SetState(PreviewState prevState)
    {
        state = prevState;
    }


    public void SetGimmickTag(Gimmick gimmickTag)
    {
        gimmick = gimmickTag;
    }
    public Gimmick GetGimmickTag()
    {
        return gimmick;
    }

    public Vector3Int GetPreviewSize()
    {
        return prevSize;
    }

    public bool GetIsGimmick()
    {
        return isGimmick;
    }
}
