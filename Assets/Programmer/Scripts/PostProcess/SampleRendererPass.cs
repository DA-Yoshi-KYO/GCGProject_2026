using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRenderPass : ScriptableRenderPass
{
    private const string RENDER_PASS_NAME = nameof(CustomRenderPass);
    private readonly Material _material;
    private RenderTextureDescriptor _renderTextureDescriptor;
    private RTHandle _blurTextureHandle;

    public CustomRenderPass(Shader shader)
    {
        _material = CoreUtils.CreateEngineMaterial(shader);
        _renderTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        // Set the blur texture size to be the same as the camera target size.
        _renderTextureDescriptor.width = cameraTextureDescriptor.width;
        _renderTextureDescriptor.height = cameraTextureDescriptor.height;

        // Check if the descriptor has changed, and reallocate the RTHandle if necessary
        RenderingUtils.ReAllocateIfNeeded(ref _blurTextureHandle, _renderTextureDescriptor);
    }

    public bool UpdateBlurSettings()
    {
        return true;
        //if (_material == null) return false;

        //// Use the Volume settings or the default settings if no Volume is set.
        //var blurVolume = VolumeManager.instance.stack.GetComponent<BlurVolume>();
        //if (blurVolume.IsActive == false) return false;

        //float horizontalBlur = blurVolume.HorizontalBlur.overrideState
        //    ? blurVolume.HorizontalBlur.value
        //    : _defaultSettings.horizontalBlur;
        //float verticalBlur = blurVolume.VerticalBlur.overrideState
        //    ? blurVolume.VerticalBlur.value
        //    : _defaultSettings.verticalBlur;
        //_material.SetFloat(HorizontalBlurId, horizontalBlur);
        //_material.SetFloat(VerticalBlurId, verticalBlur);

        //return true;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        //Get a CommandBuffer from pool.
        CommandBuffer cmd = CommandBufferPool.Get(RENDER_PASS_NAME);

        RTHandle cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

        // Blit from the camera target to the temporary render texture,
        // using the first shader pass.
        Blit(cmd, cameraTargetHandle, _blurTextureHandle, _material, 0);
        // Blit from the temporary render texture to the camera target,
        // using the second shader pass.
        Blit(cmd, _blurTextureHandle, cameraTargetHandle, _material, 0);

        //Execute the command buffer and release it back to the pool.
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Dispose()
    {
        CoreUtils.Destroy(_material);
        _blurTextureHandle?.Release();
    }
}
