/// <summary>
/// Author: SmallBurger Inc
/// Date: 2025/05/27
/// Desc:
/// </summary>
#if UNITY_2023_3_OR_NEWER
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace SB.URPRenderGraphBridgeExample
{
    public sealed partial class CustomRenderObjectPass
    {
        // Initialize any necessary states before RenderGraph execution
        public void ProcessRenderGraphInitialize()
        {
            ms_shaderTagValues[0] = m_shaderTagID;
            ms_senderStateBlocks[0] = new RenderStateBlock(RenderStateMask.Nothing);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData, profilingSampler))
            {
                // Currently, ForwardLights' ForwardPlus implementation exposes only m_ZBinsBuffer
                // and m_TileMasksBuffer, but there is no way to explicitly declare usage of these
                // specific global buffers in the RenderGraph. As a result, we are forced to call
                // builder.UseAllGlobalTextures(true), which binds *all* global resources,
                // even those that are completely unrelated to this pass. This is highly inefficient
                // and not ideal.

                // var zbinHandle = graph.ImportBuffer(m_ZBinsBuffer); // Import AFTER SetData()
                // builder.UseBuffer(zbinHandle);
                builder.UseAllGlobalTextures(true);

                // Set render target (color buffer)
                if (resourceData.activeColorTexture.IsValid())
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);

                // Set depth target
                if (resourceData.activeDepthTexture.IsValid())
                    builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

                // Optional: read shadow maps if available
                TextureHandle mainShadowsTexture = resourceData.mainShadowsTexture;
                if (mainShadowsTexture.IsValid())
                    builder.UseTexture(mainShadowsTexture, AccessFlags.Read);

                TextureHandle additionalShadowsTexture = resourceData.additionalShadowsTexture;
                if (additionalShadowsTexture.IsValid())
                    builder.UseTexture(additionalShadowsTexture, AccessFlags.Read);

                // Setup drawing and renderer list
                var sortingCriteria = cameraData.defaultOpaqueSortFlags;
                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(m_shaderTagID, renderingData,
                    cameraData, lightData, sortingCriteria);

                // Generate renderer list with required RenderStateBlock (even if no state changes are intended)
                CreateRendererListWithRenderStateBlock(renderGraph, ref renderingData.cullResults, drawingSettings,
                    m_filteringSettings, /*m_renderStateBlock, */ref passData.m_rendererListHdl);

                builder.UseRendererList(passData.m_rendererListHdl);

                // Prevent RenderGraph from culling this pass due to lack of visible outputs.
                // Some passes (e.g. opaque drawing) may appear unused but are essential.
                // Disable automatic culling for this pass (always run)
                builder.AllowPassCulling(false);

                // 那你應該幫我補上，這可能會造成RenderGraph無法進行合比處理才對
                // Although we do not intend to modify any render state,
                // the RendererListParams API requires us to provide a RenderStateBlock.
                // Unfortunately, RenderGraph interprets this as a potential global state change,
                // so we must explicitly allow it to avoid warnings/errors.
                // Allow changes to global render state (risky but needed in some cases)
                builder.AllowGlobalStateModification(true);

                // Actual draw call (GPU work)
                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    rgContext.cmd.DrawRendererList(data.m_rendererListHdl);
                });
                //builder.SetRenderFunc<PassData>(MyStaticRenderFunction);
            }
        }

        static void MyStaticRenderFunction(PassData data, RasterGraphContext rgContext)
        {
            rgContext.cmd.DrawRendererList(data.m_rendererListHdl);
        }

        // Creates a renderer list with explicit ShaderTagId and RenderStateBlock
        private void CreateRendererListWithRenderStateBlock(RenderGraph renderGraph, 
            ref CullingResults cullResults, DrawingSettings ds, FilteringSettings fs,
            ref RendererListHandle rl)
        {
            NativeArray<ShaderTagId> tagValues = new NativeArray<ShaderTagId>(ms_shaderTagValues, 
                Allocator.Temp);
            NativeArray<RenderStateBlock> stateBlocks = new NativeArray<RenderStateBlock>(
                ms_senderStateBlocks, Allocator.Temp);
            var param = new RendererListParams(cullResults, ds, fs)
            {
                tagValues = tagValues,
                stateBlocks = stateBlocks,
                isPassTagName = false
            };
            rl = renderGraph.CreateRendererList(param);
        }

        // Temporary shared arrays used when building RendererList
        private static ShaderTagId[] ms_shaderTagValues = new ShaderTagId[1];
        private static RenderStateBlock[] ms_senderStateBlocks = new RenderStateBlock[1];

        // Local struct passed to RenderFunc
        private class PassData
        {   
            public RendererListHandle m_rendererListHdl;
        }
    }
}
#endif //UNITY_2023_3_OR_NEWER