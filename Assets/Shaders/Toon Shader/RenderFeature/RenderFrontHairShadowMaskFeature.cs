using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

public class RenderFrontHairShadowMaskFeature : ScriptableRendererFeature
{
    private RenderFrontHairShadowMaskPass renderFrontHairMaskPass;

    public override void Create()
    {
        renderFrontHairMaskPass = new RenderFrontHairShadowMaskPass();
        renderFrontHairMaskPass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderFrontHairMaskPass);
    }

    class RenderFrontHairShadowMaskPass : ScriptableRenderPass
    {
        private static readonly int maskId = Shader.PropertyToID("_HairShadowMask");
        private static readonly string keyword = "_HAIRSHADOWMASK";
        private ShaderTagId maskTag = new ShaderTagId("HairShadowMask");

        private RTHandle maskRTHandle;

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // Allocate RTHandle using camera size
            Vector2Int size = new Vector2Int(cameraTextureDescriptor.width, cameraTextureDescriptor.height);

            maskRTHandle = RTHandles.Alloc(
                size, 
                colorFormat: GraphicsFormat.R16_UNorm, // <- Use GraphicsFormat instead of RenderTextureFormat
                filterMode: FilterMode.Point,
                useDynamicScale: false,
                name: "_HairShadowMask"
            );

            ConfigureTarget(maskRTHandle);
            ConfigureClear(ClearFlag.Color, Color.black);

            cmd.EnableShaderKeyword(keyword);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var drawSettings = CreateDrawingSettings(maskTag, ref renderingData, SortingCriteria.CommonOpaque);
            var filterSettings = new FilteringSettings(RenderQueueRange.opaque);

            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (maskRTHandle != null)
            {
                maskRTHandle.Release();
                maskRTHandle = null;
            }

            if (cmd != null)
                cmd.DisableShaderKeyword(keyword);
        }
    }
}
