/// <summary>
/// Author: SmallBurger Inc
/// Date: 2024/04/26
/// Desc:
/// </summary>
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SB.URPRenderGraphBridgeExample
{
    public sealed partial class CustomRenderObjectPass : ScriptableRenderPass
    {
        public void ReInitialize()
        {
            profilingSampler = new ProfilingSampler(mc_profilerTag);
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
            m_filteringSettings = new FilteringSettings(RenderQueueRange.opaque, ~0);
            m_shaderTagID = new ShaderTagId(mc_shaderTageName);            
        }

        public void Release()
        {
        }

#if UNITY_2023_3_OR_NEWER
        [Obsolete(DeprecationMessage.CompatibilityScriptingAPIObsolete, false)]
#endif //UNITY_2023_3_OR_NEWER
        public override void Execute(ScriptableRenderContext context,
            ref RenderingData renderingData)
        {
            CommandBuffer commandBuffer = CommandBufferPool.Get();
            using (new ProfilingScope(commandBuffer, profilingSampler))
            {
                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Clear();

                Camera camera = renderingData.cameraData.camera;
                var drawSettings = CreateDrawingSettings(m_shaderTagID,
                    ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);

                var filterSettings = m_filteringSettings;
                context.DrawRenderers(renderingData.cullResults,
                    ref drawSettings, ref filterSettings);
            }
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }

        private FilteringSettings m_filteringSettings;        
        private ShaderTagId m_shaderTagID;

        private const string mc_profilerTag = "CustomRenderObjectPass";
        private const string mc_shaderTageName = "CustomRenderObject";
    }
}