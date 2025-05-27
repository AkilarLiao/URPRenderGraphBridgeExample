/// <summary>
/// Author: SmallBurger Inc
/// Date: 2024/04/26
/// Desc:
/// </summary>
#if UNITY_2023_3_OR_NEWER
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace SB.URPRenderGraphBridgeExample
{
    public interface ISSAOPass
    {
        void GetNormalTextureHandle(out TextureHandle textureHandle);
    };

    public sealed partial class DistortionPass : ScriptableRenderPass
    {
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            var cameraDepth = resourceData.cameraDepth;
            if (!cameraDepth.IsValid())
                return;

            var cameraColorTH = resourceData.cameraColor;
            if (!cameraColorTH.IsValid())
                return;
            
            TextureHandle activeColorTH = resourceData.activeColorTexture;
            if (!activeColorTH.IsValid())
                return;
            

            using (IUnsafeRenderGraphBuilder builder = renderGraph.AddUnsafePass<BasePassData>(passName, out var passData,
                    profilingSampler))
            {
                builder.UseTexture(cameraDepth, AccessFlags.Read);

                passData.m_cameraColorTH = cameraColorTH;
                builder.UseTexture(cameraColorTH, AccessFlags.Read);

                passData.m_activeColorTH = activeColorTH;
                builder.UseTexture(activeColorTH, AccessFlags.Write);


                passData.m_bltMaterial = m_distortionMaterial;

                builder.SetRenderFunc((BasePassData data, UnsafeGraphContext rgContext) =>
                {
                    CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(rgContext.cmd);
                    Blitter.BlitCameraTexture(cmd, data.m_cameraColorTH, data.m_activeColorTH, RenderBufferLoadAction.DontCare,
                        RenderBufferStoreAction.Store, data.m_bltMaterial, 0);
                });
            }
        }

        private class BasePassData
        {
            internal TextureHandle m_cameraColorTH;
            internal TextureHandle m_activeColorTH;
            internal Material m_bltMaterial;
        }
    }
}
#endif //UNITY_2023_3_OR_NEWER