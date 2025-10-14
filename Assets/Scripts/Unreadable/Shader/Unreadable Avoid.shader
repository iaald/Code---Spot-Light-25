Shader "Custom/Avoiding" {

    Properties {
        _MainTex ("Font Atlas", 2D) = "white" {}
        _FaceTex ("Font Texture", 2D) = "white" {}
        _FaceColor ("Text Color", Color) = (1, 1, 1, 1)

        _VertexOffsetX ("Vertex OffsetX", float) = 0
        _VertexOffsetY ("Vertex OffsetY", float) = 0
        _MaskSoftnessX ("Mask SoftnessX", float) = 0
        _MaskSoftnessY ("Mask SoftnessY", float) = 0

        _ClipRect ("Clip Rect", vector) = (-32767, -32767, 32767, 32767)

        // 鼠标交互相关
        _MousePosition ("Mouse Position", Vector) = (0, 0, 0, 0)
        _AvoidRadius ("Avoid Radius", Float) = 100
        _AvoidStrength ("Avoid Strength", Float) = 50
        _HoleRadius ("Hole Radius", Float) = 80
        _HoleSoftness ("Hole Softness", Float) = 20

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _CullMode ("Cull Mode", Float) = 0
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader{

        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }

        Lighting Off
        Cull [_CullMode]
        ZTest [unity_GUIZTestMode]
        ZWrite Off
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask[_ColorMask]

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float4 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float4 mask : TEXCOORD2;
                float2 localPos : TEXCOORD3; // 使用局部坐标
            };

            uniform sampler2D _MainTex;
            uniform sampler2D _FaceTex;
            uniform float4 _FaceTex_ST;
            uniform fixed4 _FaceColor;

            uniform float _VertexOffsetX;
            uniform float _VertexOffsetY;
            uniform float4 _ClipRect;
            uniform float _MaskSoftnessX;
            uniform float _MaskSoftnessY;
            uniform float _UIMaskSoftnessX;
            uniform float _UIMaskSoftnessY;
            uniform int _UIVertexColorAlwaysGammaSpace;

            // 鼠标交互变量
            uniform float2 _MousePosition;
            uniform float _AvoidRadius;
            uniform float _AvoidStrength;
            uniform float _HoleRadius;
            uniform float _HoleSoftness;

            v2f vert (appdata_t v)
            {
                float4 vert = v.vertex;

                // 保存原始局部坐标
                float2 originalLocalPos = vert.xy;

                // 计算与鼠标位置的距离（在局部空间）
                float2 toMouse = originalLocalPos - _MousePosition;
                float dist = length(toMouse);

                // 避让效果：在一定半径内推开顶点
                if (dist < _AvoidRadius && dist > 0.001)
                {
                    float pushFactor = 1.0 - (dist / _AvoidRadius);
                    pushFactor = pow(pushFactor, 2.0); // 平方使过渡更平滑
                    float2 pushDir = normalize(toMouse);
                    vert.xy += pushDir * pushFactor * _AvoidStrength;
                }

                vert.x += _VertexOffsetX;
                vert.y += _VertexOffsetY;

                vert.xy += (vert.w * 0.5) / _ScreenParams.xy;

                float4 vPosition = UnityPixelSnap(UnityObjectToClipPos(vert));

                if (_UIVertexColorAlwaysGammaSpace && ! IsGammaSpace())
                {
                    v.color.rgb = UIGammaToLinear(v.color.rgb);
                }
                fixed4 faceColor = v.color;
                faceColor *= _FaceColor;

                v2f OUT;
                OUT.vertex = vPosition;
                OUT.color = faceColor;
                OUT.texcoord0 = v.texcoord0;
                OUT.texcoord1 = TRANSFORM_TEX(v.texcoord1, _FaceTex);
                OUT.localPos = originalLocalPos; // 传递原始局部坐标

                float2 pixelSize = vPosition.w;
                pixelSize /= abs(float2(_ScreenParams.x * UNITY_MATRIX_P[0][0], _ScreenParams.y * UNITY_MATRIX_P[1][1]));

                const float4 clampedRect = clamp(_ClipRect, - 2e10, 2e10);
                const half2 maskSoftness = half2(max(_UIMaskSoftnessX, _MaskSoftnessX), max(_UIMaskSoftnessY, _MaskSoftnessY));
                OUT.mask = float4(vert.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * maskSoftness + pixelSize.xy));

                return OUT;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, IN.texcoord0);
                color = fixed4 (tex2D(_FaceTex, IN.texcoord1).rgb * IN.color.rgb, IN.color.a * color.a);

                // 计算"空洞"效果（使用局部坐标）
                float2 toMouse = IN.localPos - _MousePosition;
                float distToMouse = length(toMouse);

                // 在鼠标位置创建透明空洞
                float holeFactor = 1.0;
                if (distToMouse < _HoleRadius + _HoleSoftness)
                {
                    if (distToMouse < _HoleRadius)
                    {
                        holeFactor = 0.0;
                    }
                    else
                    {
                        holeFactor = (distToMouse - _HoleRadius) / _HoleSoftness;
                        holeFactor = smoothstep(0.0, 1.0, holeFactor);
                    }
                }

                color.a *= holeFactor;

                #if UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color *= m.x * m.y;
                #endif

                #if UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
