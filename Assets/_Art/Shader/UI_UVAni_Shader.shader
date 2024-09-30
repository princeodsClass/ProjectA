Shader "038/UI_UVAni_Shader" {
	Properties {
		_USpeed("U Speed", Range(-3.0, 3.0)) = 0.0
		_VSpeed("V Speed", Range(-3.0, 3.0)) = 0.0

		_Color("Color", Color) = (1, 1, 1, 1)
		[PerRendererData] _MainTex ("Main Texture", 2D) = "white" { }
	}
	SubShader {
		Tags {
			"Queue"="Transparent" 
			"RenderType"="Transparent"
			"RenderPipeline"="UniversalPipeline"
			"IgnoreProjector"="True" 
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		ZWrite Off
		Lighting Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			HLSLPROGRAM
			#pragma target 3.0
			#pragma vertex VSMain
			#pragma fragment PSMain

			#pragma multi_compile_ui
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			float _USpeed;
			float _VSpeed;

			float4 _Color;
			sampler2D _MainTex;

			/** 입력 */
			struct STInput {
				float3 m_stPos: POSITION;
				float4 m_stColor: COLOR;

				float2 m_stUV: TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			/** 출력 */
			struct STOutput {
				float4 m_stPos: SV_POSITION;
				float4 m_stColor: COLOR;

				float2 m_stUV: TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			/** 정점 쉐이더 */
			STOutput VSMain(STInput a_stInput) {
				STOutput stOutput = (STOutput)0;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				stOutput.m_stPos = TransformObjectToHClip(a_stInput.m_stPos);
				stOutput.m_stColor = a_stInput.m_stColor * _Color;
				stOutput.m_stUV = a_stInput.m_stUV + float2(_Time.y * _USpeed, _Time.y * _VSpeed);

				return stOutput;
			}

			/** 픽셀 쉐이더 */
			float4 PSMain(STOutput a_stOutput) : SV_Target {
				return tex2D(_MainTex, a_stOutput.m_stUV) * a_stOutput.m_stColor;
			}
			ENDHLSL
		}
	}
}
