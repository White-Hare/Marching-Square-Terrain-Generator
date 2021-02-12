#if !defined(FLAT_INCLUDED)
#define FLAT_INCLUDED


#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"


sampler2D _MainTex, _DetailTex, _DetailNormalMap;
float4 _MainTex_ST, _DetailTex_ST;

sampler2D _NormalMap;
float _BumpScale, _DetailBumpScale;

float4 _Tint;
float _Metallic, _Smoothness;


struct VertexData{
	float4 vertex: POSITION;
	float2 uv: TEXCOORD0;
	float3 normal: NORMAL;
	float4 tangent: TANGENT;
};


struct Interpolators{
	float4 pos : SV_POSITION;
	float4 uv : TEXCOORD0;
	float3 normal : TEXCOORD1;
	float3 worldPos : TEXCOORD2;

	#if defined(BINORMAL_PER_FRAGMENT)
		float4 tangent : TEXCOORD3;
	#else
		float3 tangent : TEXCOORD3;
		float3 binormal : TEXCOORD4;
	#endif

	#if defined(VERTEXLIGHT_ON)
		float3 vertexLightColor : TEXCOORD5;
	#endif


	SHADOW_COORDS(5)
};


void ComputeVertexLightColor(inout Interpolators i){
	#if defined(VERTEXLIGHT_ON)
		i.vertexLightColor = Shader4PointLights(
			unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
			unity_LightColor[0].rgb, unity_LightColor[1].rgb,
			unity_LightColor[2].rgb, unity_LightColor[3].rgb,
			unity_4LightAtten0, i.worldPos, i.normal
		);

	#endif
}


UnityLight CreateLight(Interpolators i){
	UnityLight light;

	#if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
		light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
	#else
		light.dir = _WorldSpaceLightPos0.xyz;
	#endif

	UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);
	
	light.color = _LightColor0.rgb * attenuation;
	light.ndotl = DotClamped(i.normal, light.dir); 

	return light;
}


UnityIndirect CreateIndirectLight(Interpolators i){
	UnityIndirect indirect;
	
	indirect.diffuse = 0;
	indirect.specular = 0;

	#if defined(VERTEXLIGHT_ON)
		indirect.diffuse = i.vertexLightColor;
	#endif

	#if defined(FORWARD_BASE_PASS)
		indirect.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
	#endif

	return indirect;
}


void InitializeFragmentNormal(inout Interpolators i){
	
	//i.normal = normalize(cross(ddy(i.worldPos), ddx(i.worldPos)));//make it flat without Geometry Shader


	float3 mainNormal = UnpackScaleNormal(tex2D(_NormalMap, i.uv.xy), _BumpScale);
	float3 detailNormal = UnpackScaleNormal(tex2D(_DetailNormalMap, i.uv.zw), _DetailBumpScale);
	
	float3 tangentSpaceNormal = BlendNormals(mainNormal, detailNormal);

	#if defined(BINORMAL_PER_FRAGMENT)
		float3 binormal = cross(i.normal, i.tangent.xyz) * (i.tangent.w * unity_WorldTransformParams.w);
	#else
		float3 binormal = i.binormal;
	#endif

	i.normal = normalize(
		tangentSpaceNormal.x * i.tangent +
		tangentSpaceNormal.y * binormal + 
		tangentSpaceNormal.z * i.normal);


}


Interpolators MyVertexProgram(VertexData v){
	
	Interpolators i;

	i.pos = UnityObjectToClipPos(v.vertex);
	i.worldPos = mul(unity_ObjectToWorld, v.vertex);
	
	i.normal = UnityObjectToWorldNormal(v.normal);	
	

	#if defined(BINORMAL_PER_FRAGMENT)
		i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
	#else
		i.tangent = UnityObjectToWorldDir(v.tangent.xyz);
		i.binormal = cross(i.normal, i.tangent.xyz) * (v.tangent.w * unity_WorldTransformParams.w);
	#endif



	i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
	i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);
	 

	TRANSFER_SHADOW(i);

	ComputeVertexLightColor(i);

	return i;
}


[maxvertexcount(3)]
void MyGeometryProgram(triangle Interpolators i[3], inout TriangleStream<Interpolators> stream){
	float3 p0 = i[0].worldPos.xyz;
	float3 p1 = i[1].worldPos.xyz;
	float3 p2 = i[2].worldPos.xyz;
	
	float3 triangleNormal = normalize(cross(p1 - p0, p2 - p0));

	i[0].normal = triangleNormal;
	i[1].normal = triangleNormal;
	i[2].normal = triangleNormal;


	stream.Append(i[0]);
	stream.Append(i[1]);
	stream.Append(i[2]);
}


float4 MyFragmentProgram(Interpolators i) : SV_TARGET0
{
	InitializeFragmentNormal(i);

	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);


	float3 specularTint;
	float oneMinusReflectivity;

	float3 albedo = _Tint.rgb * tex2D(_MainTex, i.uv.xy).rgb;
	albedo *= tex2D(_DetailTex, i.uv.zw) * unity_ColorSpaceDouble;
	albedo = DiffuseAndSpecularFromMetallic(albedo, _Metallic, specularTint, oneMinusReflectivity);
	

	return UNITY_BRDF_PBS(albedo, specularTint,
						  oneMinusReflectivity, _Smoothness,
						  i.normal, viewDir, 
						  CreateLight(i), CreateIndirectLight(i));
}


#endif