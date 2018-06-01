using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Runningtap
{
    [CustomEditor(typeof(Painter))]
    public class PainterEditor : Editor
    {
        private SerializedProperty
            Mode,
            Bounds,
            Resolution,
            UseRelative,
            World,
            RenderTextureShader,
            Brush,
            BrushSize,
            BrushStrength,
            FadeOverTime,
            FadeSpeed;

        GUIStyle EditorTextureDisplay;
        GUISkin style;
        Texture2D Logo;
        Painter Target;

        public void OnEnable()
        {
            style = (GUISkin)Resources.Load("RunningtapGuiSkin", typeof(GUISkin));
            Logo = Resources.Load("RunningtapRevealShaderLogo") as Texture2D;
            Mode = serializedObject.FindProperty("LookupMode");
            Bounds = serializedObject.FindProperty("ManualBounds");
            UseRelative = serializedObject.FindProperty("UseRelative");
            Resolution = serializedObject.FindProperty("SplatResolution");
            World = serializedObject.FindProperty("World");
            RenderTextureShader = serializedObject.FindProperty("RenderTextureShader");
            Brush = serializedObject.FindProperty("Brush");
            BrushSize = serializedObject.FindProperty("BrushSize");
            BrushStrength = serializedObject.FindProperty("BrushStrength");
            FadeOverTime = serializedObject.FindProperty("FadeOverTime");
            FadeSpeed = serializedObject.FindProperty("FadeSpeed");

            EditorTextureDisplay = new GUIStyle()
            {
                normal = style.customStyles[0].normal,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(1, 1, 1, 1),
            };
        }

        public override void OnInspectorGUI()
        {
            Target = (Painter)target;
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.Space();

            Rect logoArea = GUILayoutUtility.GetRect(128, 32, GUILayout.ExpandWidth(false));
            GUI.DrawTexture(logoArea, Logo, ScaleMode.ScaleToFit, true, 0);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(Mode);
            if (Target.LookupMode == Painter.Mode.manual)
            {
                if(Target.ManualBounds == null)
                    Target.ManualBounds = Target.GetComponent<BoxCollider>();

                //Target.ManualBounds.enabled = true;
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(Bounds);
                EditorGUILayout.HelpBox("Edit the BoxCollider to set bounds manually.", MessageType.Info);
                EditorGUI.indentLevel--;
            }
            else
                Target.ManualBounds.enabled = false;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(Resolution);
            EditorGUILayout.PropertyField(UseRelative);
            EditorGUILayout.PropertyField(FadeOverTime);
            EditorGUILayout.PropertyField(World);
            EditorGUILayout.PropertyField(RenderTextureShader);
            EditorGUILayout.PropertyField(Brush);
            EditorGUILayout.PropertyField(BrushSize);
            EditorGUILayout.PropertyField(BrushStrength);

            if (FadeOverTime.boolValue == true)
            {
                EditorGUILayout.PropertyField(FadeSpeed);
            }
            else
            {
                Target.FadeSpeed = 0;
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Splat Map Preview:");
            if(Application.isPlaying)
            {
                EditorGUILayout.LabelField(Target.DebugRenderTexture.width + " x " + Target.DebugRenderTexture.height);

                var width = Target.DebugRenderTexture.width * 200 / (int)Target.SplatResolution;
                var height = Target.DebugRenderTexture.height * 200 / (int)Target.SplatResolution;

                EditorTextureDisplay.fixedWidth = width;
                EditorTextureDisplay.fixedHeight = height;

                GUILayout.BeginHorizontal(EditorTextureDisplay);
                    Rect box = GUILayoutUtility.GetRect(width, height, EditorTextureDisplay, GUILayout.ExpandWidth(true));
                    GUI.DrawTexture(box, Target.DebugRenderTexture, ScaleMode.ScaleToFit, true, 0);
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            Repaint();
            serializedObject.ApplyModifiedProperties();
        }
    }
}