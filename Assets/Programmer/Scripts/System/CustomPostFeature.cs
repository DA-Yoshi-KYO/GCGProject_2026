/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    カスタムなポストエフェクトを実装するためのRendererFeature
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    吉田 京志郎
 * ----------------------------------------------------------
 * 2026-04-21 | 初回作成
 * 
 */

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
            var cmd = CommandBufferPool.Get("CustomPost");
            if (!AttachVolume())
            {
                CommandBufferPool.Release(cmd);
                return;
            }

            // source → temp(加工)
            Blitter.BlitCameraTexture(cmd, source, tempRT, material, 0);

            // temp → source(戻す)
            Blitter.BlitCameraTexture(cmd, tempRT, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // RTHandleは自動管理されるので基本不要
        }

        private bool AttachVolume()
        {
            var stack = VolumeManager.instance.stack;   // Volumeスタックの取得
            if (stack == null)
            {
                Debug.LogWarning("VolumeStackが見つかりません");
                return false;
            }

            // パラメータの適応
            // グレースケール
            GrayScaleVolume volume = stack.GetComponent<GrayScaleVolume>();
            if (volume.active)
            {
                material.SetFloat("_Intensity", volume.intensity.value);
            }


            return true;
        }
    }

    public Material material;
    CustomPass pass;

    public override void Create()
    {
        pass = new CustomPass(material);
        pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}
