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
        public void ProcessRenderGraphInitialize()
        {
            m_renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData, profilingSampler))
            {
                //Currently, ForwardLights' ForwardPlus implementation exposes only m_ZBinsBuffer and m_TileMasksBuffer,
                //but there is no way to explicitly declare usage of these specific global buffers in the RenderGraph.
                //As a result, we are forced to call builder.UseAllGlobalTextures(true), which binds *all* global resources,
                //even those that are completely unrelated to this pass. This is highly inefficient and not ideal.

                //var zbinHandle = graph.ImportBuffer(m_ZBinsBuffer); // Import AFTER SetData()
                //builder.UseBuffer(zbinHandle);
                builder.UseAllGlobalTextures(true);
                
                if (resourceData.activeColorTexture.IsValid())
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);

                if(resourceData.activeDepthTexture.IsValid())
                    builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

                TextureHandle mainShadowsTexture = resourceData.mainShadowsTexture;
                if (mainShadowsTexture.IsValid())
                    builder.UseTexture(mainShadowsTexture, AccessFlags.Read);

                TextureHandle additionalShadowsTexture = resourceData.additionalShadowsTexture;
                if (additionalShadowsTexture.IsValid())
                    builder.UseTexture(additionalShadowsTexture, AccessFlags.Read);

                var sortingCriteria = cameraData.defaultOpaqueSortFlags;
                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(m_shaderTagID, renderingData,
                    cameraData, lightData, sortingCriteria);
                CreateRendererListWithRenderStateBlock(renderGraph, ref renderingData.cullResults, drawingSettings,
                    m_filteringSettings, m_renderStateBlock, ref passData.m_rendererListHdl);

                builder.UseRendererList(passData.m_rendererListHdl);
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);
                
                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    rgContext.cmd.DrawRendererList(data.m_rendererListHdl);
                });
            }
        }
        
        private void CreateRendererListWithRenderStateBlock(RenderGraph renderGraph, ref CullingResults cullResults,
            DrawingSettings ds, FilteringSettings fs, RenderStateBlock rsb, ref RendererListHandle rl)
        {
            ms_shaderTagValues[0] = ShaderTagId.none;
            ms_senderStateBlocks[0] = rsb;
            NativeArray<ShaderTagId> tagValues = new NativeArray<ShaderTagId>(ms_shaderTagValues, Allocator.Temp);
            NativeArray<RenderStateBlock> stateBlocks = new NativeArray<RenderStateBlock>(ms_senderStateBlocks, Allocator.Temp);
            var param = new RendererListParams(cullResults, ds, fs)
            {
                tagValues = tagValues,
                stateBlocks = stateBlocks,
                isPassTagName = false
            };
            rl = renderGraph.CreateRendererList(param);
        }
        
        private RenderStateBlock m_renderStateBlock;
        private static ShaderTagId[] ms_shaderTagValues = new ShaderTagId[1];
        private static RenderStateBlock[] ms_senderStateBlocks = new RenderStateBlock[1];

        private class PassData
        {   
            public RendererListHandle m_rendererListHdl;
        }
    }
}
#endif //UNITY_2023_3_OR_NEWER