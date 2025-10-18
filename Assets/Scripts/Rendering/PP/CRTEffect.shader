Shader "Hidden/CRTEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.1
        _ScanlineSpeed ("Scanline Speed", Range(0, 5)) = 1.0
        _ScanlineDensity ("Scanline Density", Range(100, 2000)) = 800
        _Curvature ("Curvature", Range(0, 0.5)) = 0.1
        _CurvatureRadius ("Curvature Radius", Range(1, 5)) = 2.0
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.02)) = 0.005
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.3
        _VignetteSmoothness ("Vignette Smoothness", Range(0.1, 1)) = 0.3
        _Brightness ("Brightness", Range(0.5, 2)) = 1.0
        _Contrast ("Contrast", Range(0.5, 2)) = 1.0
        _NoiseIntensity ("Noise Intensity", Range(0, 0.1)) = 0.02
        _NoiseSpeed ("Noise Speed", Range(0, 5)) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 100
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "CRT Pass"
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            // Material properties
            CBUFFER_START(UnityPerMaterial)
                float _ScanlineIntensity;
                float _ScanlineSpeed;
                float _ScanlineDensity;
                float _Curvature;
                float _CurvatureRadius;
                float _ChromaticAberration;
                float _VignetteIntensity;
                float _VignetteSmoothness;
                float _Brightness;
                float _Contrast;
                float _NoiseIntensity;
                float _NoiseSpeed;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = input.uv;
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }

            // Simple pseudo-random noise function
            float noise(float2 coord)
            {
                return frac(sin(dot(coord.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Improved noise with interpolation
            float smoothNoise(float2 coord)
            {
                float2 i = floor(coord);
                float2 f = frac(coord);
                
                float a = noise(i);
                float b = noise(i + float2(1.0, 0.0));
                float c = noise(i + float2(0.0, 1.0));
                float d = noise(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            // Curvature distortion effect
            float2 distort(float2 uv)
            {
                // Convert to normalized device coordinates (-1 to 1)
                uv = uv * 2.0 - 1.0;
                
                // Calculate distortion based on distance from center
                float2 offset = abs(uv.yx) / _CurvatureRadius;
                uv = uv + uv * offset * offset * _Curvature;
                
                // Convert back to texture coordinates (0 to 1)
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            // Vignette effect
            float vignette(float2 uv)
            {
                // Convert to centered coordinates
                uv = uv * 2.0 - 1.0;
                // Calculate distance from center and apply vignette
                float vignette = 1.0 - dot(uv, uv) * _VignetteIntensity;
                // Smooth the transition
                return smoothstep(0.0, _VignetteSmoothness, vignette);
            }

            // Scanlines effect
            float scanlines(float2 uv, float time)
            {
                // Create moving scanlines
                float scanline = sin(uv.y * _ScanlineDensity + time * _ScanlineSpeed * 10.0);
                // Normalize and apply intensity
                scanline = (scanline * 0.5 + 0.5) * _ScanlineIntensity;
                return 1.0 - scanline;
            }

            // Color adjustment (brightness and contrast)
            float3 adjustColor(float3 color)
            {
                // Apply contrast
                color = (color - 0.5) * _Contrast + 0.5;
                // Apply brightness
                color *= _Brightness;
                // Clamp to valid range
                return saturate(color);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float2 uv = input.uv;
                
                // Apply curvature distortion
                if (_Curvature > 0)
                {
                    uv = distort(uv);
                    
                    // If UV is outside [0,1] range, return black to create border
                    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    {
                        return half4(0, 0, 0, 1);
                    }
                }
                
                // Apply chromatic aberration
                half4 color;
                if (_ChromaticAberration > 0)
                {
                    float2 offset = float2(_ChromaticAberration, _ChromaticAberration);
                    half r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset).r;
                    half g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).g;
                    half b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - offset).b;
                    color = half4(r, g, b, 1.0);
                }
                else
                {
                    color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                }
                
                // Apply scanlines
                if (_ScanlineIntensity > 0)
                {
                    float scanline = scanlines(uv, _Time.y);
                    color.rgb *= scanline;
                }
                
                // Apply noise/grain
                if (_NoiseIntensity > 0)
                {
                    float n = smoothNoise(uv * 100.0 + _Time.y * _NoiseSpeed);
                    color.rgb += (n - 0.5) * _NoiseIntensity;
                }
                
                // Apply vignette
                if (_VignetteIntensity > 0)
                {
                    color.rgb *= vignette(uv);
                }
                
                // Apply color adjustments
                color.rgb = adjustColor(color.rgb);
                
                // Apply fog
                color.rgb = MixFog(color.rgb, input.fogCoord);
                
                return color;
            }
            ENDHLSL
        }
    }
    
    Fallback "Universal Render Pipeline/Unlit"
    CustomEditor "UnityEditor.ShaderGraph.MaterialEditor"
}