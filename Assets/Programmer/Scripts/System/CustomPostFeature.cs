/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    カスタムなポストエフェクトを実装するためのRendererFeature
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    吉田 京志郎
 * ----------------------------------------------------------
 * 2026-04-21 | 初回作成
 * 
 */

/*
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostFeature : ScriptableRendererFeature
{
    public class CustomPass : ScriptableRenderPass
    {
        public Material material;

        RTHandle source;
        RTHandle tempRT;

        public CustomPass(Material mat)
        {
            material = mat;
        }

        public void Setup(RTHandle src)
        {
            source = src;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            var desc = renderingData.cameraData.cameraTargetDescriptor;

            RenderingUtils.ReAllocateIfNeeded(
                ref tempRT,
                desc,
                name: "_TempRT"
            );
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            AttachVolume();

            //if (!renderingData.cameraData.postProcessEnabled) return;
            //if (renderingData.cameraData.isSceneViewCamera) return;

            var cmd = CommandBufferPool.Get("CustomPost");

            var renderer = renderingData.cameraData.renderer;
            var src = renderer.cameraColorTargetHandle;

            if (material == null)
            {
                Debug.LogError("Material null");
                CommandBufferPool.Release(cmd);
                return;
            }

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            RenderingUtils.ReAllocateIfNeeded(ref tempRT, desc, name: "_TempRT");

            if (tempRT == null)
            {
                Debug.LogError("tempRT null");
                CommandBufferPool.Release(cmd);
                return;
            }

            AttachVolume();

            Blitter.BlitCameraTexture(cmd, src, tempRT, material, 0);
            Blitter.BlitCameraTexture(cmd, tempRT, src);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // RTHandleは自動管理されるので基本不要
        }

        private void AttachVolume()
        {
            var stack = VolumeManager.instance.stack;   // Volumeスタックの取得
            if (stack == null)
            {
                Debug.LogWarning("VolumeStackが見つかりません");
            }

            // パラメータの適応
            // グレースケール
            GrayScaleVolume volume = stack.GetComponent<GrayScaleVolume>();
            if (volume.active)
            {
                material.SetInt("_UseGrayscale", volume.enable.value ? 1 : 0);
                material.SetFloat("_Intensity", volume.intensity.value);
            }
        }
    }

    public Material material;
    CustomPass pass;

    public override void Create()
    {
        pass = new CustomPass(null);
        pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        pass.material = material;
        renderer.EnqueuePass(pass);
    }
}

*/
