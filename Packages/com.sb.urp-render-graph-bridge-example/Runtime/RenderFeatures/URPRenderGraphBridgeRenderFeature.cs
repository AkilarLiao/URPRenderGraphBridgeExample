/// <summary>
/// Author: SmallBurger Inc
/// Date: 2025/05/26
/// Desc:
/// </summary>
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SB.URPRenderGraphBridgeExample
{
    internal struct DeprecationMessage
    {
        internal const string CompatibilityScriptingAPIObsolete = "This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.";
        internal const string CompatibilityScriptingAPIConsoleWarning = "The project currently uses the compatibility mode where the Render Graph API is disabled. Support for this mode will be removed in future Unity versions. Migrate existing ScriptableRenderPasses to the new RenderGraph API. After the migration, disable the compatibility mode in Edit > Projects Settings > Graphics > Render Graph.";
    }

    [DisallowMultipleRendererFeature("URPRenderGraphBridgeRenderFeature")]
    public class URPRenderGraphBridgeRenderFeature : ScriptableRendererFeature
    {
        public override void Create()
        {
            if (!isActive)
                return;
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, 
            ref RenderingData renderingData) { }
        protected override void Dispose(bool disposing)
        {
        }
    }
}