// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#if !defined(SHADOWCASTER_INCLUDED)
#define SHADOWCASTER_INCLUDED


#include "UnityCG.cginc"


struct VertexData{
	float4 position : POSITION;
	float3 normal : NORMAL;
};


#if defined(SHADOW_CUBE)

	struct Interpolators{
		float4 position : SV_POSITION;
		float3 lightVec : TEXCOORD0;
	};


	Interpolators MyVertexProgram(VertexData vd) : SV_POSITION{
		Interpolators i;
		i.position = UnityObjectToClipPos(v.position);
		i.lightVec = mul(unity_ObjectToWorld, v.position).xyz - _LightPositionRange.xyz

		return i ;
	}

	float4 MyFragmentProgram(Interpolators i) : SV_TARGET0{
		float depth = length(i.lightVec) + unity_LightShadowBias.x;
		depth * = _LightPositionRange.w

		return UnityEncodeCubeShadowDepth(depth);
	}


#else
	float4 MyVertexProgram(VertexData vd): SV_POSITION{
		float4 position = UnityClipSpaceShadowCasterPos(vd.position.xyz, vd.normal);
		return UnityApplyLinearShadowBias(position);
	}


	float4 MyFragmentProgram() : SV_TARGET0
	{ 
		return 0;
	}
#endif


#endif