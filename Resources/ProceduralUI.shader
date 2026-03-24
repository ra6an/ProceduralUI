Shader "ProceduralUI/ProceduralImage"
{
    Properties
    {
        _Size                ("Size",         Vector) = (100,100,0,0)

        _FillAmount          ("Fill Amount", Float) = 1
        _BorderFillAmount    ("Border Fill Amount", Float) = 1
        _FillType            ("Fill Type", Float) = 0
        _RadialOffset        ("Radial Offset", Float) = 0
        _RadialClockwise     ("Clockwise", Float) = 1

        _Hollow              ("Hollow", Float) = 0

        _CornerRadii         ("Corner Radii", Vector) = (0,0,0,0)

        _IsGradient          ("Is Gradient", Float) = 0
        _GradientAngle       ("Gradient Angle", Float) = 0
        _GradientSteps       ("Gradient Steps", Float) = 1
        _GradientType        ("Gradient Type", Float) = 0
        _TopColor            ("Top Color",    Color)  = (1,1,1,1)
        _BottomColor         ("Bottom Color", Color)  = (1,1,1,1)

        _BorderWidth         ("Border Width", Float)  = 0
        _BorderTopColor      ("Border Top Color", Color) = (1,1,1,1)
        _BorderBottomColor   ("Border Bottom Color", Color) = (1,1,1,1)
        _IsBorderGradient    ("Is Border Gradient", Float) = 0
        _BorderGradientAngle ("Border Gradient Angle", Float) = 0
        _BorderGradientSteps ("Border Gradient Steps", Float) = 1
        _BorderGradientType  ("Border Gradient Type", Float) = 0

        _Falloff             ("Falloff",      Float)  = 1
        _ShapeMode           ("Shape Mode",   Float)  = 0
        _UseBlur             ("Use Blur",     Float)  = 0
        _BlurAmount          ("Blur Amount",  Float)  = 4

        [HideInInspector] _StencilComp      ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil          ("Stencil ID",         Float) = 0
        [HideInInspector] _StencilOp        ("Stencil Operation",  Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        [HideInInspector] _ColorMask        ("Color Mask",         Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "RenderType"        = "Transparent"
            "IgnoreProjector"   = "True"
            "CanUseSpriteAtlas" = "True"
            "PreviewType"       = "Plane"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull      Off
        ZWrite    Off
        ZTest     [unity_GUIZTestMode]
        ColorMask [_ColorMask]
        Lighting  Off

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
                float4 worldPos : TEXCOORD1;
            };

            float4 _Size;

            float  _FillAmount;
            float  _BorderFillAmount;
            float  _FillType;
            float  _RadialOffset;
            float  _RadialClockwise;

            float _Hollow;

            float4 _CornerRadii;   // x=topLeft  y=topRight  z=bottomRight  w=bottomLeft

            float4 _TopColor;
            float4 _BottomColor;
            float  _IsGradient;
            float  _GradientAngle;
            float  _GradientSteps;
            float  _GradientType;

            float  _BorderWidth;
            float4 _BorderTopColor;
            float4 _BorderBottomColor;
            float  _IsBorderGradient;
            float  _BorderGradientAngle;
            float  _BorderGradientSteps;
            float  _BorderGradientType;

            float  _Falloff;
            float  _ShapeMode;
            float  _UseBlur;
            float  _BlurAmount;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex   = UnityObjectToClipPos(v.vertex);
                o.uv       = v.uv;
                o.color    = v.color;
                o.worldPos = v.vertex;
                return o;
            }

            // ── SDF: signed distance to axis-aligned box centered at origin ──
            // Returns negative inside, 0 on edge, positive outside.
            float boxSDF(float2 p, float2 halfSize)
            {
                float2 q = abs(p) - halfSize;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0);
            }

            // ── SDF: rounded box with per-corner radii ────────────────────────
            // radii: x=topLeft y=topRight z=bottomRight w=bottomLeft
            float roundedBoxSDF(float2 p, float2 halfSize, float4 radii)
            {
                // pick radius for this quadrant
                // p.y > 0 = top half,  p.x > 0 = right half
                float r = (p.x > 0.0)
                        ? ((p.y > 0.0) ? radii.y : radii.z)
                        : ((p.y > 0.0) ? radii.x : radii.w);

                float maxR = min(halfSize.x, halfSize.y);
                r = min(r, maxR);

                float2 q = abs(p) - halfSize + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            // ── SDF to alpha: negative dist = inside = opaque ─────────────────
            // falloff controls the anti-alias width in pixels
            float sdToAlpha(float dist, float falloff)
            {
                return 1.0 - smoothstep(-falloff, 0.0, dist);
            }

            float gradientValue(float2 uv, float angle, float type, float steps)
            {
                float g;

                if (type < 0.5)
                {
                    // Linear
                    float rad = radians(angle);
                    float2 dir = float2(cos(rad), sin(rad));
                    g = dot(uv - 0.5, dir) + 0.5;
                }
                else
                {
                    // Radial
                    g = length(uv - 0.5) * 2.0;
                }

                // Steps
                if (steps > 1.0)
                {
                    g = floor(g * steps) / steps;
                }

                return saturate(g);
            }

            float fillMask(float2 uv, float2 p, float dist, float fillAmount, float fillType)
            {
                float f = 1.0;

                // Left → Right
                if (fillType < 0.5)
                    f = step(uv.x, fillAmount);

                // Right → Left
                else if (fillType < 1.5)
                    f = step(1.0 - uv.x, fillAmount);

                // Bottom → Top
                else if (fillType < 2.5)
                    f = step(uv.y, fillAmount);

                // Top → Bottom
                else if (fillType < 3.5)
                    f = step(1.0 - uv.y, fillAmount);

                // Offset from center using SDF (shape-based fill)
                else if (fillType < 4.5)
                {
                    float maxDist = min(_Size.x, _Size.y) * 0.5;
                    float d = saturate((-dist) / maxDist);
                    f = step(1.0 - fillAmount, d);
                }

                // Radial (clock fill)
                else
                {
                    float angle = atan2(p.y, -p.x);
                    angle = (angle + 3.14159265) / (2 * 3.14159265);

                    // Offset rotation
                    angle = frac(angle + _RadialOffset);

                    // Direction
                    if (_RadialClockwise < 0.5) 
                    {
                        angle = 1.0 - angle;
                    }

                    f = step(angle, fillAmount);
                }

                return f;
            }

            // float fillMask(float2 uv, float fillAmount, float fillType)
            // {
            //     float f = 1.0;

            //     if (fillType < 0.5)          LeftToRight
            //         f = step(uv.x, fillAmount);
            //     else if (fillType < 1.5)     RightToLeft
            //         f = step(1.0 - uv.x, fillAmount);
            //     else if (fillType < 2.5)     Offset (vertical)
            //         f = step(uv.y, fillAmount);
            //     else                         Radial
            //         f = step(length(uv - 0.5) * 2.0, fillAmount);

            //     return f;
            // }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv       = i.uv;
                float2 halfSize = _Size.xy * 0.5;
                float2 p        = (uv - 0.5) * _Size.xy;

                float falloff = max(0.5, _Falloff);
                float dist    = 0.0;

                // ── Shape SDF ────────────────────────────────────────────────
                if (_ShapeMode < 0.5)
                {
                    dist = boxSDF(p, halfSize);
                }
                else if (_ShapeMode < 1.5)
                {
                    dist = roundedBoxSDF(p, halfSize, _CornerRadii);
                }
                else
                {
                    float r = min(halfSize.x, halfSize.y);
                    dist = length(p) - r;
                }

                // ── Alpha masks ───────────────────────────────────────────────
                float outerAlpha = sdToAlpha(dist, falloff);
                float innerAlpha = 0.0;
                float borderMask = 0.0;
                float imageMask  = 0.0;

                if (_BorderWidth > 0.0)
                {
                    innerAlpha = sdToAlpha(dist + _BorderWidth, falloff);
                    borderMask = outerAlpha - innerAlpha;
                    imageMask  = innerAlpha;
                }
                else
                {
                    imageMask = outerAlpha;
                }

                // ── Fill masks ────────────────────────────────────────────────
                float fillMaskImage  = fillMask(uv, p, dist, _FillAmount, _FillType);
                float fillMaskBorder = fillMask(uv, p, dist, _BorderFillAmount, _FillType);

                imageMask  *= fillMaskImage;
                borderMask *= fillMaskBorder;

                // ── Hollow center ─────────────────────────────────────────────
                if (_Hollow > 0.001)
                {
                    float inner = sdToAlpha(dist + (_Hollow * min(_Size.x, _Size.y) * 0.5), falloff);
                    imageMask *= (1.0 - inner);
                    borderMask *= (1.0 - inner);
                }

                // ── Image gradient ────────────────────────────────────────────
                float g = gradientValue(uv, _GradientAngle, _GradientType, _GradientSteps);
                float4 imageColor = lerp(_BottomColor, _TopColor, g);

                // ── Border gradient ───────────────────────────────────────────
                float gb = gradientValue(uv, _BorderGradientAngle, _BorderGradientType, _BorderGradientSteps);
                float4 borderColor = lerp(_BorderBottomColor, _BorderTopColor, gb);

                // ── Combine colors ────────────────────────────────────────────
                float4 fillCol;
                float4 borderCol;

                // Fill color
                if (_IsGradient > 0.5)
                    fillCol = imageColor;
                else
                    fillCol = _TopColor;

                // Border color
                if (_IsBorderGradient > 0.5)
                    borderCol = borderColor;
                else
                    borderCol = _BorderTopColor;

                // Apply masks separately
                fillCol.a *= imageMask;
                borderCol.a *= borderMask;

                // Combine
                // float4 col = fillCol + borderCol;
                float4 col = 0;
                col += fillCol * imageMask;
                col += borderCol * borderMask;

                // ── Blur ──────────────────────────────────────────────────────
                if (_UseBlur > 0.5)
                {
                    float blurAlpha = sdToAlpha(dist, max(1.0, _BlurAmount));
                    col.a = lerp(col.a, blurAlpha, 0.5);
                }

                // ── Unity UI clipping ─────────────────────────────────────────
                col.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);

                return col;
            }
            ENDCG
        }
    }

    FallBack "UI/Default"
}
