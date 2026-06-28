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
