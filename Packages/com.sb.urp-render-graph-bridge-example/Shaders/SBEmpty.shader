/// <summary>
/// Author: SmallBurger Inc
/// Date: 2024/12/19
/// Desc:
/// </summary>

Shader "Hidden/SB/SBEmpty"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags 
        {            
            //"Queue" = "Geometry-1"
            //"RenderPipeline" = "UniversalPipeline"
        }
        //LOD 100

        Pass
        {            
            //Blend SrcAlpha OneMinusSrcAlpha
            //ZWrite Off
            HLSLPROGRAM
            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram
            #include "SBEmptyImpl.hlsl"
            ENDHLSL
        }
    }
}
