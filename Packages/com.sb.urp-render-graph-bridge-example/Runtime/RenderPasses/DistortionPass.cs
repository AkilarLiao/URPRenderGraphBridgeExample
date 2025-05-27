/// <summary>
/// Author: SmallBurger Inc
/// Date: 2025/05/27
/// Desc:
/// </summary>

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SB.URPRenderGraphBridgeExample
{
    public sealed partial class DistortionPass : ScriptableRenderPass
    {
        public void ReInitialize(Shader distortionPS, Texture2D distortionNormalTexture, Setting setting)
        {
            Release();
            if ((!distortionPS) || (!distortionNormalTexture) || (setting == null))
                return;

            m_targetDistortionNormalTexture = distortionNormalTexture;
            m_targetSetting = setting;            
            m_distortionMaterial = CoreUtils.CreateEngineMaterial(distortionPS);
            RefreshMaterial();

            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            profilingSampler = new ProfilingSampler(mc_profilerTag);

#if UNITY_EDITOR
            RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
            RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
#endif //UNITY_EDITOR
        }

        public void Release()
        {
#if UNITY_EDITOR
            RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
#endif //UNITY_EDITOR

            if (m_distortionMaterial)
            {
                CoreUtils.Destroy(m_distortionMaterial);
                m_distortionMaterial = null;
            }
        }

#if UNITY_2023_3_OR_NEWER
        [Obsolete(DeprecationMessage.CompatibilityScriptingAPIObsolete, false)]
#endif //UNITY_2023_3_OR_NEWER
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var renderer = renderingData.cameraData.renderer;
            var cameraColorTargetHandle = renderer.cameraColorTargetHandle;
            if (!cameraColorTargetHandle.rt)
                return;

            CommandBuffer commandBuffer = CommandBufferPool.Get();
            using (new ProfilingScope(commandBuffer, profilingSampler))
            {
                CoreUtils.SetRenderTarget(commandBuffer, cameraColorTargetHandle, RenderBufferLoadAction.Load,
                    RenderBufferStoreAction.Store, ClearFlag.None, Color.clear);
                float4 rtHandleScale = cameraColorTargetHandle.rtHandleProperties.rtHandleScale;
                Vector2 viewportScale = cameraColorTargetHandle.useScaling ? rtHandleScale.xy : Vector2.one;
                Blitter.BlitTexture(commandBuffer, viewportScale, m_distortionMaterial, 0);
            }
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
        

#if UNITY_EDITOR
        private void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            RefreshMaterial();
        }
#endif //UNITY_EDITOR

        private void RefreshMaterial()
        {
            m_distortionMaterial.SetTexture(msr_distortionNormalTextureID, m_targetDistortionNormalTexture);

            m_distortionMaterial.SetVector(msr_distortionNormalTextureScaleID,
                new Vector2(m_targetSetting.m_normalTextureUScale, m_targetSetting.m_normalTextureVScale));

            float destLinearStart = MathF.Min(m_targetSetting.m_linearStart, m_targetSetting.m_linearEnd - 0.1f);
            float destLinearEnd = MathF.Max(destLinearStart + 0.1f, m_targetSetting.m_linearEnd);
            m_distortionMaterial.SetVector(msr_distortionParamsID, new Vector3(
                destLinearStart, destLinearEnd, m_targetSetting.m_density));

            m_distortionMaterial.SetVector(msr_distortionAnimateParamsID,
                    new Vector3(m_targetSetting.m_normalTextureUSpeed,
                    m_targetSetting.m_normalTextureVSpeed,
                    m_targetSetting.m_normalDistortion));
        }

        private Material m_distortionMaterial = null;
        
        private Texture2D m_targetDistortionNormalTexture = null;
        private Setting m_targetSetting = null;

        private const string mc_profilerTag = "DistortionPass";

        private static readonly int msr_distortionNormalTextureID = Shader.PropertyToID("_DistortionNormalTexture");
        private static readonly int msr_distortionNormalTextureScaleID = Shader.PropertyToID("_DistortionNormalTextureScale");
        private static readonly int msr_distortionParamsID = Shader.PropertyToID("_DistortionParams");
        private static readonly int msr_distortionAnimateParamsID = Shader.PropertyToID("_DistortionAnimateParams");
    }
}