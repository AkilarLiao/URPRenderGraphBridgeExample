/// <summary>
/// Author: SmallBurger Inc
/// Date: 2024/04/26
/// Desc:
/// </summary>

using System;
using System.Collections.Generic;
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
            CommandBuffer commandBuffer = CommandBufferPool.Get();
            using (new ProfilingScope(commandBuffer, profilingSampler))
            {
                var cameraColorTargetHandle = renderer.cameraColorTargetHandle;
            }

            /*var renderer = renderingData.cameraData.renderer;
            if (!renderer.cameraDepthTargetHandle.rt)
                return;

            CommandBuffer commandBuffer = CommandBufferPool.Get();
            using (new ProfilingScope(commandBuffer, m_profilingSampler))
            {
#if ENABLE_VR && ENABLE_XR_MODULE
                if (renderingData.cameraData.xr.supportsFoveatedRendering)
                {
                    var qualitySetting = m_targetSetting.m_qualitySetting;
                    // If we are downsampling we can't use the VRS texture
                    // If it's a non uniform raster foveated rendering has to be turned off because it will keep applying non uniform for the other passes.
                    // When calculating normals from depth, this causes artifacts that are amplified from VRS when going to say 4x4.
                    // Thus we disable foveated because of that
                    if (qualitySetting.m_isDownsample || SystemInfo.foveatedRenderingCaps == FoveatedRenderingCaps.NonUniformRaster)
                    {
                        commandBuffer.SetFoveatedRenderingMode(FoveatedRenderingMode.Disabled);
                    }
                    // If we aren't downsampling and it's a VRS texture we can apply foveation in this case
                    else if (SystemInfo.foveatedRenderingCaps == FoveatedRenderingCaps.FoveationImage)
                    {
                        commandBuffer.SetFoveatedRenderingMode(FoveatedRenderingMode.Enabled);
                    }
                }
#endif
                var cameraColorTargetHandle = renderer.cameraColorTargetHandle;

                if(m_targetSetting.m_viewMode == VIEW_MODE.VIEW_MODE_NORMAL)

                {
                    Blitter.BlitCameraTexture(commandBuffer, m_tempRT1, renderer.cameraColorTargetHandle,
                        RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, m_targetMaterial, (int)ShaderPasseType.ViewNormal);
                }
                else
                {
                    Blitter.BlitCameraTexture(commandBuffer, cameraColorTargetHandle, m_tempRT1, m_targetMaterial,
                        (int)ShaderPasseType.AmbientOcclusion);
                    switch (m_targetBlurType)
                    {
                        case BlurType.Bilateral:
                            ProcessBilateralBlur(commandBuffer, cameraColorTargetHandle);
                            break;
                        case BlurType.Gaussian:
                            ProcessGaussianBlur(commandBuffer, cameraColorTargetHandle);
                            break;
                        default:
                            //case BlurType.Kawase:
                            ProcessKawaseBlur(commandBuffer, cameraColorTargetHandle);
                            break;
                    }
                }
            }
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
            */
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

        //Texture2D distortionNormalTexture, Setting setting
        private Texture2D m_targetDistortionNormalTexture = null;
        private Setting m_targetSetting = null;

        private const string mc_profilerTag = "DistortionPass";

        private static readonly int msr_distortionNormalTextureID = Shader.PropertyToID("_DistortionNormalTexture");
        private static readonly int msr_distortionNormalTextureScaleID = Shader.PropertyToID("_DistortionNormalTextureScale");
        private static readonly int msr_distortionParamsID = Shader.PropertyToID("_DistortionParams");
        private static readonly int msr_distortionAnimateParamsID = Shader.PropertyToID("_DistortionAnimateParams");
    }
}