using UnityEngine;
using UnityEngine.UI;

namespace ProceduralUI 
{
    public enum ImageType
    {
        Normal,
        Fill
    }

    public enum FillType
    {
        LeftToRight,
        RightToLeft,
        BottomToTop,
        TopToBottom,
        Offset,
        Radial
    }

    public enum GradientType
    {
        Linear,
        Radial
    }

    [ExecuteAlways]
    [AddComponentMenu("UI/Procedural Image")]
    [RequireComponent(typeof(CanvasRenderer))]
    public class ProceduralImage : Graphic
    {
        // ── Image Type ────────────────────────────────────────────────────────────
        [SerializeField] ImageType _type;
        public ImageType type
        {
            get => _type;
            set { _type = value; SetMaterialDirty(); }
        }

        // ── Fill ──────────────────────────────────────────────────────────────────
        [SerializeField] FillType _fillType;
        [SerializeField, Range(0, 1)] float _fillAmount = 1f;
        [SerializeField, Range(0, 1)] float _borderFillAmount = 1f;
        [SerializeField, Range(0, 1)] float _radialOffset = 0f;
        [SerializeField] bool _radialClockwise = true;

        [SerializeField, Range(0, 1)] float _hollow = 0f;

        public FillType fillType
        {
            get => _fillType;
            set { _fillType = value; SetMaterialDirty(); }
        }
        public float fillAmount
        {
            get => _fillAmount;
            set { _fillAmount = value; SetMaterialDirty(); }
        }
        public float borderFillAmount
        {
            get => _borderFillAmount;
            set { _borderFillAmount = value; SetMaterialDirty(); }
        }
        public float radialOffset
        {
            get => _radialOffset;
            set { _radialOffset = value; SetMaterialDirty(); }
        }
        public bool radialClockwise
        {
            get => _radialClockwise;
            set { _radialClockwise = value; SetMaterialDirty(); }
        }

        public float hollow
        {
            get => _hollow;
            set { _hollow = value; SetMaterialDirty(); }
        }

        // ── Shape ─────────────────────────────────────────────────────────────────
        [SerializeField] ProceduralShape _shape = ProceduralShape.Rectangle;
        public ProceduralShape shape
        {
            get => _shape;
            set { _shape = value; SetMaterialDirty(); }
        }

        // ── Corner Modifier ───────────────────────────────────────────────────────
        [SerializeField] ProceduralModifier _modifier = ProceduralModifier.Uniform;
        public ProceduralModifier modifier
        {
            get => _modifier;
            set { _modifier = value; SetMaterialDirty(); }
        }

        [SerializeField] ProceduralSide _side = ProceduralSide.Top;
        public ProceduralSide side
        {
            get => _side;
            set { _side = value; SetMaterialDirty(); }
        }

        // ── Corner Radii ──────────────────────────────────────────────────────────
        [SerializeField] float _uniformRadius = 20f;
        public float uniformRadius
        {
            get => _uniformRadius;
            set { _uniformRadius = value; SetMaterialDirty(); }
        }

        [SerializeField] float _sidesRadius = 20f;
        public float sidesRadius
        {
            get => _sidesRadius;
            set { _sidesRadius = value; SetMaterialDirty(); }
        }

        [SerializeField] float _topLeft;
        [SerializeField] float _topRight;
        [SerializeField] float _bottomLeft;
        [SerializeField] float _bottomRight;

        public float topLeft { get => _topLeft; set { _topLeft = value; SetMaterialDirty(); } }
        public float topRight { get => _topRight; set { _topRight = value; SetMaterialDirty(); } }
        public float bottomLeft { get => _bottomLeft; set { _bottomLeft = value; SetMaterialDirty(); } }
        public float bottomRight { get => _bottomRight; set { _bottomRight = value; SetMaterialDirty(); } }

        // ── BORDER ────────────────────────────────────────────────────────────────
        [SerializeField] bool _isBorderGradient = false;
        public bool isBorderGradient
        {
            get => _isBorderGradient;
            set { _isBorderGradient = value; SetMaterialDirty(); }
        }

        [SerializeField] float _borderWidth = 0f;
        [SerializeField] float _falloff = 1f;
        [SerializeField] Color _borderColor = Color.gray;
        [SerializeField] Color _borderTopColor = Color.gray;
        [SerializeField] Color _borderBottomColor = Color.gray;
        [SerializeField] GradientType _borderGradientType = GradientType.Linear;
        [SerializeField] float _borderGradientAngle = 0f;
        [SerializeField] int _borderGradientSteps = 1;

        public float borderWidth
        {
            get => _borderWidth;
            set { _borderWidth = value; SetMaterialDirty(); }
        }
        public float falloff
        {
            get => _falloff;
            set { _falloff = value; SetMaterialDirty(); }
        }
        public Color borderColor
        {
            get => _borderColor;
            set { _borderColor = value; SetMaterialDirty(); }
        }
        public Color borderTopColor
        {
            get => _borderTopColor;
            set { _borderTopColor = value; SetMaterialDirty(); }
        }
        public Color borderBottomColor
        {
            get => _borderBottomColor;
            set { _borderBottomColor = value; SetMaterialDirty(); }
        }
        public GradientType borderGradientType
        {
            get => _borderGradientType;
            set { _borderGradientType = value; SetMaterialDirty(); }
        }
        public float borderGradientAngle
        {
            get => _borderGradientAngle;
            set { _borderGradientAngle = value; SetMaterialDirty(); }
        }
        public int borderGradientSteps
        {
            get => _borderGradientSteps;
            set { _borderGradientSteps = value; SetMaterialDirty(); }
        }

        // ── IMAGE Color / Gradient ─────────────────────────────────────────────────
        [SerializeField] bool _isGradient = false;
        public bool isGradient
        {
            get => _isGradient;
            set { _isGradient = value; SetMaterialDirty(); }
        }

        // Solid color reuses Graphic.color (shown in Inspector as "Color")
        // For gradient we have two separate colors:
        [SerializeField] Color _imageColor = Color.white;
        [SerializeField] Color _topColor = Color.white;
        [SerializeField] Color _bottomColor = Color.white;
        [SerializeField] GradientType _gradientType = GradientType.Linear;
        [SerializeField] float _gradientAngle = 0f;
        [SerializeField] int _gradientSteps = 1;

        public Color imageColor
        {
            get => _imageColor;
            set { _imageColor = value; SetMaterialDirty(); }
        }
        public Color topColor
        {
            get => _topColor;
            set { _topColor = value; SetMaterialDirty(); }
        }
        public Color bottomColor
        {
            get => _bottomColor;
            set { _bottomColor = value; SetMaterialDirty(); }
        }
        public GradientType gradientType
        {
            get => _gradientType;
            set { _gradientType = value; SetMaterialDirty(); }
        }
        public float gradientAngle
        {
            get => _gradientAngle;
            set { _gradientAngle = value; SetMaterialDirty(); }
        }
        public int gradientSteps
        {
            get => _gradientSteps;
            set { _gradientSteps = value; SetMaterialDirty(); }
        }

        // ── Transparency / Blur ───────────────────────────────────────────────────
        [SerializeField] bool _useBlur = false;
        public bool useBlur
        {
            get => _useBlur;
            set { _useBlur = value; SetMaterialDirty(); }
        }

        [SerializeField, Range(0f, 100f)] float _blurAmount = 4f;
        public float blurAmount
        {
            get => _blurAmount;
            set { _blurAmount = value; SetMaterialDirty(); }
        }

        // ── Runtime Material ─────────────────────────────────────────────────────
        Material _runtimeMaterial;

        public override Material materialForRendering
        {
            get
            {
                if (_runtimeMaterial == null)
                {
                    // Resources.Load works reliably in both Editor and builds
                    // as long as the shader is inside any "Resources" folder.
                    var shader = Resources.Load<Shader>("ProceduralUI");
                    if (shader == null)
                    {
                        // Fallback: try Shader.Find (works if shader is in Always Included Shaders)
                        shader = Shader.Find("ProceduralUI/ProceduralImage");
                    }
                    if (shader == null)
                    {
                        Debug.LogError("[ProceduralImage] Shader not found! " +
                            "Make sure 'ProceduralUI.shader' is inside a folder named 'Resources'.");
                        return base.materialForRendering;
                    }
                    _runtimeMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                }
                ApplyShaderProperties(_runtimeMaterial);
                return _runtimeMaterial;
            }
        }

        // ── Unity Callbacks ───────────────────────────────────────────────────────

    #if UNITY_EDITOR
        // Push shader properties every editor frame — the Canvas caches the material
        // reference and doesn't re-call materialForRendering every frame, so we push
        // directly here to see live changes while tweaking values in the Inspector.
        protected virtual void Update()
        {
            if (!Application.isPlaying && _runtimeMaterial != null)
                ApplyShaderProperties(_runtimeMaterial);
        }
    #endif

        protected override void OnEnable()
        {
            base.OnEnable();
            SetMaterialDirty();
            SetVerticesDirty();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_runtimeMaterial != null)
            {
                DestroyImmediate(_runtimeMaterial);
                _runtimeMaterial = null;
            }
        }

    #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetMaterialDirty();
            SetVerticesDirty();
        }
    #endif

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            SetMaterialDirty();
            SetVerticesDirty();
        }

        // ── Mesh ──────────────────────────────────────────────────────────────────
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();

            UIVertex vert = UIVertex.simpleVert;
            vert.color = Color.white; // uses Graphic.color so Canvas color tinting still works

            vert.position = new Vector2(rect.xMin, rect.yMin); vert.uv0 = new Vector2(0, 0); vh.AddVert(vert);
            vert.position = new Vector2(rect.xMin, rect.yMax); vert.uv0 = new Vector2(0, 1); vh.AddVert(vert);
            vert.position = new Vector2(rect.xMax, rect.yMax); vert.uv0 = new Vector2(1, 1); vh.AddVert(vert);
            vert.position = new Vector2(rect.xMax, rect.yMin); vert.uv0 = new Vector2(1, 0); vh.AddVert(vert);

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        // ── Shader Property Upload ────────────────────────────────────────────────
        void ApplyShaderProperties(Material mat)
        {
            var rect = rectTransform.rect;
            mat.SetVector("_Size", new Vector4(rect.width, rect.height, 0, 0));

            if (_type == ImageType.Fill)
            {
                mat.SetFloat("_FillType", (float)fillType);
                mat.SetFloat("_FillAmount", _fillAmount);
                mat.SetFloat("_BorderFillAmount", _borderFillAmount);
                mat.SetFloat("_RadialOffset", _radialOffset);
                mat.SetFloat("_RadialClockwise", _radialClockwise ? 1f : 0f);
            }

            mat.SetFloat("_Hollow", _hollow);

            mat.SetVector("_CornerRadii", GetCornerRadii());

            mat.SetFloat("_BorderWidth", _borderWidth);
            mat.SetFloat("_IsBorderGradient", _isBorderGradient ? 1f : 0f);
            mat.SetFloat("_BorderGradientAngle", _borderGradientAngle);
            mat.SetFloat("_BorderGradientSteps", _borderGradientSteps);
            mat.SetFloat("_BorderGradientType", (float)_borderGradientType);

            if (_isBorderGradient)
            {
                mat.SetColor("_BorderTopColor", _borderTopColor);
                mat.SetColor("_BorderBottomColor", _borderBottomColor);
            } else
            {
                mat.SetColor("_BorderTopColor", _borderColor);
                mat.SetColor("_BorderBottomColor", _borderColor);
            }

                mat.SetFloat("_Falloff", Mathf.Max(0.001f, _falloff));

            // Shape mode: 0 = Rectangle, 1 = Rounded, 2 = Circle
            mat.SetFloat("_ShapeMode", (float)_shape);

            // Gradient
            mat.SetFloat("_IsGradient", _isGradient ? 1f : 0f);
            mat.SetFloat("_GradientAngle", _gradientAngle);
            mat.SetFloat("_GradientSteps", _gradientSteps);
            mat.SetFloat("_GradientType", (float)_gradientType);

            if (_isGradient)
            {
                mat.SetColor("_TopColor", _topColor);
                mat.SetColor("_BottomColor", _bottomColor);
            }
            else
            {
                mat.SetColor("_TopColor", _imageColor);
                mat.SetColor("_BottomColor", _imageColor);
            }

            // Blur
            mat.SetFloat("_UseBlur", _useBlur ? 1f : 0f);
            mat.SetFloat("_BlurAmount", _blurAmount);
        }

        // ── Corner Radii Helper ───────────────────────────────────────────────────
        public Vector4 GetCornerRadii()
        {
            // Circle: radius = half the shortest side
            if (_shape == ProceduralShape.Circle)
            {
                float r = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.5f;
                return new Vector4(r, r, r, r);
            }

            // Rectangle: no rounding
            if (_shape == ProceduralShape.Rectangle)
                return Vector4.zero;

            // Rounded: use modifier
            switch (_modifier)
            {
                case ProceduralModifier.Uniform:
                    return new Vector4(_uniformRadius, _uniformRadius, _uniformRadius, _uniformRadius);

                case ProceduralModifier.Free:
                    return new Vector4(_topLeft, _topRight, _bottomRight, _bottomLeft);

                case ProceduralModifier.Sides:
                    switch (_side)
                    {
                        case ProceduralSide.Top: return new Vector4(_sidesRadius, _sidesRadius, 0, 0);
                        case ProceduralSide.Bottom: return new Vector4(0, 0, _sidesRadius, _sidesRadius);
                        case ProceduralSide.Left: return new Vector4(_sidesRadius, 0, 0, _sidesRadius);
                        case ProceduralSide.Right: return new Vector4(0, _sidesRadius, _sidesRadius, 0);
                    }
                    break;
            }

            return Vector4.zero;
        }
    }
}