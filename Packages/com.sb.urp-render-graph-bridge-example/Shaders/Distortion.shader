/// <summary>
/// Author: SmallBurger Inc
/// Date: 2025/05/27
/// Desc:
/// </summary>
Shader "Hidden/SB/Distortion"
{   
    //Disabling asynchronous compilation for specific Shader objects
    //You can disable asynchronous shader compilation for specific Shader objects by forcing the Editor to always compile them synchronously.
    //This is a good option for data generating Shader objects that are always present at the start of your rendering,
    //and which are relatively quick to compile.You would most likely need this if you are performing advanced rendering.
    //To force synchronous compilation for a Shader object, add the #pragma editor_sync_compilation directive to your shader source code.
    //Note: You should not force synchronous compilation for complex Shader objects that encounter new shader variants during rendering;
    //this can stall rendering in the Editor.
    /*HLSLINCLUDE
        #pragma editor_sync_compilation
    ENDHLSL*/
    SubShader
    {
        Tags{ "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        Cull Off ZWrite Off ZTest Always

        // ------------------------------------------------------------------
        // Ambient Occlusion
        // ------------------------------------------------------------------
        Pass
        {   
            Name "DistortionPass"
            ZTest Always
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex VertexProgram            
            #pragma fragment FragmentProgram
            //#pragma multi_compile_local_fragment _ _ORTHOGRAPHIC
            //#pragma multi_compile_local_fragment _SAMPLE_LOW _SAMPLE_MEDIUM _SAMPLE_HIGH
            //#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "DistortionImpl.hlsl"
            ENDHLSL
        }
    }
}
