/// <summary>
/// Author: SmallBurger Inc
/// Date: 2025/05/26
/// Desc:
/// </summary>
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SB.URPRenderGraphBridgeExample
{
    [System.Serializable]
    public class Setting
    {
        [Tooltip("The normal texture u speed")]
        [Range(0.01f, 10.0f)]
        public float m_normalTextureUScale = 1.0f;

        [Tooltip("The normal texture v speed")]
        [Range(0.01f, 10.0f)]
        public float m_normalTextureVScale = 1.0f;

        [Tooltip("The normal texture u speed")]
        [Range(-2.0f, 2.0f)]
        public float m_normalTextureUSpeed = 0.05f;

        [Tooltip("The normal texture v speed")]
        [Range(-2.0f, 2.0f)]
        public float m_normalTextureVSpeed = 0.0f;

        [Tooltip("The normal distortion range")]
        [Range(0.0f, 2.0f)]
        public float m_normalDistortion = 1.3f;

        //[Tooltip("The start depth weight")]
        //[Range(0.0f, 0.99f)]
        //public float m_startDepthWeight = 0.15f;

        //[Tooltip("The depth pow")]
        //[Range(0.01f, 10.0f)]
        //public float m_depthPow = 3.0f;

        [Tooltip("The linear start")]
        [Range(1.0f, 1000.0f)]
        public float m_linearStart = 300.0f;

        [Tooltip("The linear end")]
        [Range(1.0f, 2000.0f)]
        public float m_linearEnd = 800.0f;

        [Tooltip("The linear density")]
        [Range(0.0001f, 100.0f)]
        public float m_density = 1.0f;
    }

    internal struct DeprecationMessage
    {
        internal const string CompatibilityScriptingAPIObsolete = 
            "This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.";
        
        internal const string CompatibilityScriptingAPIConsoleWarning = 
            "The project currently uses the compatibility mode where the Render Graph API is disabled. " +
            "Support for this mode will be removed in future Unity versions. " +
            "Migrate existing ScriptableRenderPasses to the new RenderGraph API. After the migration, " +
            "disable the compatibility mode in Edit > Projects Settings > Graphics > Render Graph.";
    }

    [DisallowMultipleRendererFeature("URPRenderGraphBridgeRenderFeature")]
    public class URPRenderGraphBridgeRenderFeature : ScriptableRendererFeature
    {
        public override void Create()
        {
            if (!isActive)
                return;
            ResourceReloader.TryReloadAllNullIn(this, msr_packagePath);

            m_customRenderObjectPass.ReInitialize();
            m_distortionPass.ReInitialize(m_internalResource.m_distortionPS, m_internalResource.m_distortionNormalTexture,
                m_setting);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_customRenderObjectPass);
            renderer.EnqueuePass(m_distortionPass);
        }
        protected override void Dispose(bool disposing)
        {
            m_distortionPass.Release();
            m_customRenderObjectPass.Release();
        }

        private CustomRenderObjectPass m_customRenderObjectPass = new CustomRenderObjectPass();
        private DistortionPass m_distortionPass = new DistortionPass();

        //[SerializeField]
        //[HideInInspector]
        //[Reload("Shaders/Distortion.shader")]
        //private Shader m_distortionPS = null;

        [SerializeField]
        private Setting m_setting = new Setting();

        [System.Serializable, ReloadGroup]
        public sealed class InternalResource
        {
            [Reload("Shaders/Distortion.shader")]
            public Shader m_distortionPS = null;
            [Reload("Textures/DistortionNormal.png")]
            public Texture2D m_distortionNormalTexture = null;
        }
        //[HideInInspector]
        [SerializeField]
        private InternalResource m_internalResource = null;

#if UNITY_EDITOR
        private static readonly string msr_packagePath = "Packages/com.sb.urp-render-graph-bridge-example";
#endif //UNITY_EDITOR
    }
}