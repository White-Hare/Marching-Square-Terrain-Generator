Shader "Custom Shaders/My Shaders/Flat Shader"
{
	Properties{
		_Tint("Tint", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white"{}
		
		[NoScaleOffset] _NormalMap("Normals", 2D) = "bump"{}
		_BumpScale ("Bump Scale", Float) = 1
		[NoScaleOffset] _DetailNormalMap ("Detail Normals", 2D) = "bump" {}
		_DetailBumpScale("Detail Bump Scale", Float) = 1

		_DetailTex("Detail Texture", 2D) = "gray" {}
		
		[Gama] _Metallic("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
	}

	CGINCLUDE
		#define BINORMAL_PER_FRAMGENT 
	ENDCG


	SubShader{
		Tags {"RenderType" = "Opaque"}
	
		Pass{
			Tags{"LightMode" = "ForwardBase"}
			CGPROGRAM
				
				#pragma target 4.0
			
				#pragma vertex MyVertexProgram
				#pragma fragment MyFragmentProgram
				#pragma geometry MyGeometryProgram

				#define FORWARD_BASE_PASS

				#pragma multi_compile _ SHADOWS_SCREEN
				#pragma multi_compile _ VERTEXLIGHT_ON

				#include "FlatShader.cginc"

			ENDCG
		}

		Pass{
			Tags{"LightMode" = "ForwardAdd"}
			Blend One One
			ZWrite Off 

			CGPROGRAM
				#pragma target 4.0

				#pragma vertex MyVertexProgram
				#pragma fragment MyFragmentProgram
				#pragma geometry MyGeometryProgram


				#pragma multi_compile_fwdadd_fullshadows 

				#include "FlatShader.cginc"
				
			ENDCG
		}

		Pass{
			Tags{"LightMode" = "ShadowCaster"}

			CGPROGRAM

				#pragma target 4.0
			
				#pragma multi_compile_shadowcaster

				#pragma vertex MyVertexProgram
				#pragma fragment MyFragmentProgram

		
				#include "ShadowCaster.cginc"
		
			ENDCG
		}
	}
}