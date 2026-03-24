using UnityEditor;
using UnityEngine;
using ProceduralUI;

[CustomEditor(typeof(ProceduralImage))]
public class ProceduralImageEditor : Editor
{
    // Serialized properties — we use SerializedProperty so Undo/Redo and Prefab
    // overrides work correctly in Editor.
    SerializedProperty _type;

    // Fill
    SerializedProperty _fillType;
    SerializedProperty _fillAmount;
    SerializedProperty _borderFillAmount;
    SerializedProperty _radialOffset;
    SerializedProperty _radialClockwise;

    // Hollow
    SerializedProperty _hollow;

    // Shapes & Modifiers
    SerializedProperty _shape;
    SerializedProperty _modifier;
    SerializedProperty _side;
    SerializedProperty _uniformRadius;
    SerializedProperty _sidesRadius;
    SerializedProperty _topLeft;
    SerializedProperty _topRight;
    SerializedProperty _bottomLeft;
    SerializedProperty _bottomRight;

    // Border
    SerializedProperty _borderWidth;
    SerializedProperty _isBorderGradient;
    SerializedProperty _borderGradientType;
    SerializedProperty _borderGradientAngle;
    SerializedProperty _borderGradientSteps;
    SerializedProperty _borderColor;
    SerializedProperty _borderTopColor;
    SerializedProperty _borderBottomColor;

    SerializedProperty _falloff;

    // Color
    SerializedProperty _isGradient;
    SerializedProperty _gradientType;
    SerializedProperty _gradientAngle;
    SerializedProperty _gradientSteps;
    SerializedProperty _imageColor;
    SerializedProperty _topColor;
    SerializedProperty _bottomColor;

    // Blur
    SerializedProperty _useBlur;
    SerializedProperty _blurAmount;

    bool showBorder = false;
    bool showBlur = false;
    bool showColor = false;

    void OnEnable()
    {
        _type = serializedObject.FindProperty("_type");

        // Fill
        _fillType = serializedObject.FindProperty("_fillType");
        _fillAmount = serializedObject.FindProperty("_fillAmount");
        _borderFillAmount = serializedObject.FindProperty("_borderFillAmount");
        _radialOffset = serializedObject.FindProperty("_radialOffset");
        _radialClockwise = serializedObject.FindProperty("_radialClockwise");

        // Hollow
        _hollow = serializedObject.FindProperty("_hollow");

        _shape = serializedObject.FindProperty("_shape");
        _modifier = serializedObject.FindProperty("_modifier");
        _side = serializedObject.FindProperty("_side");
        _uniformRadius = serializedObject.FindProperty("_uniformRadius");
        _sidesRadius = serializedObject.FindProperty("_sidesRadius");
        _topLeft = serializedObject.FindProperty("_topLeft");
        _topRight = serializedObject.FindProperty("_topRight");
        _bottomLeft = serializedObject.FindProperty("_bottomLeft");
        _bottomRight = serializedObject.FindProperty("_bottomRight");

        // Border
        _borderWidth = serializedObject.FindProperty("_borderWidth");
        _isBorderGradient = serializedObject.FindProperty("_isBorderGradient");
        _borderGradientType = serializedObject.FindProperty("_borderGradientType");
        _borderGradientAngle = serializedObject.FindProperty("_borderGradientAngle");
        _borderGradientSteps = serializedObject.FindProperty("_borderGradientSteps");
        _borderColor = serializedObject.FindProperty("_borderColor");
        _borderTopColor = serializedObject.FindProperty("_borderTopColor");
        _borderBottomColor = serializedObject.FindProperty("_borderBottomColor");

        _falloff = serializedObject.FindProperty("_falloff");

        // Color
        _isGradient = serializedObject.FindProperty("_isGradient");
        _gradientType = serializedObject.FindProperty("_gradientType");
        _gradientAngle = serializedObject.FindProperty("_gradientAngle");
        _gradientSteps = serializedObject.FindProperty("_gradientSteps");
        _imageColor = serializedObject.FindProperty("_imageColor");
        _topColor = serializedObject.FindProperty("_topColor");
        _bottomColor = serializedObject.FindProperty("_bottomColor");

        // Blur
        _useBlur = serializedObject.FindProperty("_useBlur");
        _blurAmount = serializedObject.FindProperty("_blurAmount");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── Type ─────────────────────────────────────────────────────────────
        SectionLabel("Type");
        EditorGUILayout.BeginHorizontal();
        ImageTypeButton(ImageType.Normal, "Normal");
        ImageTypeButton(ImageType.Fill, "Fill");
        EditorGUILayout.EndHorizontal();

        var currentImageType = (ImageType)_type.enumValueIndex;

        if (currentImageType == ImageType.Fill)
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(_fillType, new GUIContent("Fill"));
            EditorGUILayout.PropertyField(_fillAmount, new GUIContent("Fill Amount"));
            EditorGUILayout.PropertyField(_borderFillAmount, new GUIContent("Border Fill Amount"));

            var currentFillType = (FillType)_fillType.enumValueIndex;

            if (currentFillType == FillType.Radial)
            {
                EditorGUILayout.PropertyField(_radialOffset, new GUIContent("Offset"));
                EditorGUILayout.PropertyField(_radialClockwise, new GUIContent("Clockwise"));
            }
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.PropertyField(_hollow, new GUIContent("Hollow"));

        // ── Color / Gradient ─────────────────────────────────────────────────
        EditorGUILayout.Space(12);
        showColor = EditorGUILayout.BeginFoldoutHeaderGroup(showColor, "Color");
        if (showColor)
        {
            EditorGUILayout.PropertyField(_isGradient, new GUIContent("Use Gradient"));

            if (_isGradient.boolValue)
            {
                EditorGUILayout.PropertyField(_gradientType, new GUIContent("Type"));
                EditorGUILayout.PropertyField(_gradientAngle, new GUIContent("Angle"));
                EditorGUILayout.PropertyField(_gradientSteps, new GUIContent("Steps"));
                EditorGUILayout.PropertyField(_topColor, new GUIContent("First Color"));
                EditorGUILayout.PropertyField(_bottomColor, new GUIContent("Second Color"));
            }
            else
            {
                EditorGUILayout.PropertyField(_imageColor, new GUIContent("Color"));
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // ── Shape ─────────────────────────────────────────────────────────────
        EditorGUILayout.Space(12);
        SectionLabel("Shape");
        EditorGUILayout.BeginHorizontal();
        ShapeButton(ProceduralShape.Rectangle, "Rectangle");
        ShapeButton(ProceduralShape.Rounded, "Rounded");
        ShapeButton(ProceduralShape.Circle, "Circle");
        EditorGUILayout.EndHorizontal();

        var currentShape = (ProceduralShape)_shape.enumValueIndex;

        // ── Corner Modifier (only relevant for Rounded) ───────────────────────
        if (currentShape == ProceduralShape.Rounded)
        {
            EditorGUILayout.Space(6);
            SectionLabel("Corner Modifier");
            EditorGUILayout.PropertyField(_modifier, new GUIContent("Modifier"));

            EditorGUILayout.Space(2);
            var mod = (ProceduralModifier)_modifier.enumValueIndex;

            switch (mod)
            {
                case ProceduralModifier.Uniform:
                    EditorGUILayout.PropertyField(_uniformRadius, new GUIContent("Radius"));
                    break;

                case ProceduralModifier.Free:
                    EditorGUILayout.PropertyField(_topLeft, new GUIContent("Top Left"));
                    EditorGUILayout.PropertyField(_topRight, new GUIContent("Top Right"));
                    EditorGUILayout.PropertyField(_bottomLeft, new GUIContent("Bottom Left"));
                    EditorGUILayout.PropertyField(_bottomRight, new GUIContent("Bottom Right"));
                    break;

                case ProceduralModifier.Sides:
                    SectionLabel("Round Side");
                    // Toolbar for side selection
                    _side.enumValueIndex = GUILayout.Toolbar(
                        _side.enumValueIndex,
                        new[] { "Top", "Right", "Bottom", "Left" }
                    );
                    EditorGUILayout.Space(2);
                    EditorGUILayout.PropertyField(_sidesRadius, new GUIContent("Radius"));
                    break;
            }
        }

        // ── Border ────────────────────────────────────────────────────────────
        EditorGUILayout.Space(12);
        showBorder = EditorGUILayout.BeginFoldoutHeaderGroup(showBorder, "Border");
        if (showBorder)
        {
            EditorGUILayout.PropertyField(_borderWidth, new GUIContent("Border Width"));
            EditorGUILayout.PropertyField(_isBorderGradient, new GUIContent("Use Gradient"));

            if (_isBorderGradient.boolValue)
            {
                EditorGUILayout.PropertyField(_borderGradientType, new GUIContent("Gradient Type"));
                EditorGUILayout.PropertyField(_borderGradientAngle, new GUIContent("Gradient Angle"));
                EditorGUILayout.PropertyField(_borderGradientSteps, new GUIContent("Gradient Steps"));
                EditorGUILayout.PropertyField(_borderTopColor, new GUIContent("First Color"));
                EditorGUILayout.PropertyField(_borderBottomColor, new GUIContent("Second Color"));
            } else
            {
                EditorGUILayout.PropertyField(_borderColor, new GUIContent("Border Color"));
            }


            EditorGUILayout.Space(12);
            EditorGUILayout.PropertyField(_falloff, new GUIContent("Falloff"));
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // ── Transparency / Blur ───────────────────────────────────────────────
        EditorGUILayout.Space(12);
        showBlur = EditorGUILayout.BeginFoldoutHeaderGroup(showBlur, "Blur");
        if (showBlur)
        {
            EditorGUILayout.PropertyField(_useBlur, new GUIContent("Enable Blur"));

            if (_useBlur.boolValue)
            {
                EditorGUILayout.PropertyField(_blurAmount, new GUIContent("Blur Amount"));
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        serializedObject.ApplyModifiedProperties();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    void SectionLabel(string text)
    {
        EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
    }

    void ShapeButton(ProceduralShape shapeValue, string label)
    {
        bool active = _shape.enumValueIndex == (int)shapeValue;
        bool pressed = GUILayout.Toggle(active, label, "Button");
        if (pressed && !active)
            _shape.enumValueIndex = (int)shapeValue;
    }

    void ImageTypeButton(ImageType imageTypeValue, string label)
    {
        bool active = _type.enumValueIndex == (int)imageTypeValue;
        bool pressed = GUILayout.Toggle(active, label, "Button");
        if (pressed && !active)
            _type.enumValueIndex = (int)imageTypeValue;
    }
}
