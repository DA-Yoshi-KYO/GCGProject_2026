using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CustomRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader _shader;
    [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    private CustomRenderPass _blurRenderPass;

    public override void Create()
    {
        if (_shader == null)
        {
            Debug.LogError("シェーダーが用意されていません");
            return;
        }
        _blurRenderPass = new CustomRenderPass(_shader)
        {
            renderPassEvent = _renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_blurRenderPass);
    }

    protected override void Dispose(bool disposing)
    {
        _blurRenderPass.Dispose();
    }
}
