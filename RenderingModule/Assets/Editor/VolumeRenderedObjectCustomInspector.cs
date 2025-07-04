﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityVolumeRendering
{
    [CustomEditor(typeof(VolumeRenderedObject))]
    public class VolumeRenderedObjectCustomInspector : Editor, IProgressView
    {
        private bool tfSettings = true;
        private bool lightSettings = true;
        private bool otherSettings = true;
        private bool highlightSettings = true;
        private float currentProgress = 1.0f;
        private string currentProgressDescrition = "";
        private bool progressDirty = false;

        public void StartProgress(string title, string description)
        {
        }

        public void FinishProgress(ProgressStatus status = ProgressStatus.Succeeded)
        {
            currentProgress = 1.0f;
        }

        public void UpdateProgress(float totalProgress, float currentStageProgress, string description)
        {
            currentProgressDescrition = description;
            currentProgress = totalProgress;
            progressDirty = true;
        }
        public override bool RequiresConstantRepaint()
        {
            return progressDirty;
        }

        public override void OnInspectorGUI()
        {
            VolumeRenderedObject volRendObj = (VolumeRenderedObject)target;

            if (currentProgress < 1.0f)
            {
                Rect rect = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(rect, currentProgress, currentProgressDescrition);
                GUILayout.Space(18);
                EditorGUILayout.EndVertical();
            }
            progressDirty = false;

            // Render mode
            RenderMode oldRenderMode = volRendObj.GetRenderMode();
            RenderMode newRenderMode = (RenderMode)EditorGUILayout.EnumPopup("Render mode", oldRenderMode);
            if (newRenderMode != oldRenderMode)
            {
                Task task = volRendObj.SetRenderModeAsync(newRenderMode, new ProgressHandler(this));
            }

            // Visibility window
            Vector2 visibilityWindow = volRendObj.GetVisibilityWindow();
            EditorGUILayout.MinMaxSlider("Visible value range", ref visibilityWindow.x, ref visibilityWindow.y, 0.0f, 1.0f);
            volRendObj.SetVisibilityWindow(visibilityWindow);

            if (newRenderMode == RenderMode.IsosurfaceRendering)
            {
                float oldThreshold = volRendObj.GetGradientVisibilityThreshold();
                float oldThresholdSqrt = Mathf.Sqrt(oldThreshold); // Convert to square root scaling (=> more precision close to 0)
                float newThreshold = EditorGUILayout.Slider(
                    new GUIContent("Gradient visibility threshold", "Minimum gradient maginitude value that will be visible"),
                    oldThresholdSqrt, 0.0f, 1.0f
                );
                newThreshold = newThreshold * newThreshold; // Convert back to linear scaling
                if (newThreshold != oldThreshold)
                    volRendObj.SetGradientVisibilityThreshold(newThreshold);
            }

            // Transfer function settings
            EditorGUILayout.Space();
            tfSettings = EditorGUILayout.Foldout(tfSettings, "Transfer function");
            if (tfSettings)
            {
                // Transfer function type
                TFRenderMode tfMode = (TFRenderMode)EditorGUILayout.EnumPopup("Transfer function type", volRendObj.GetTransferFunctionMode());
                if (tfMode != volRendObj.GetTransferFunctionMode())
                {
                    Task task = volRendObj.SetTransferFunctionModeAsync(tfMode, new ProgressHandler(this));
                }

                // Show TF button
                if (GUILayout.Button("Edit transfer function"))
                {
                    if (tfMode == TFRenderMode.TF1D)
                        TransferFunctionEditorWindow.ShowWindow(volRendObj);
                    else
                        TransferFunction2DEditorWindow.ShowWindow();
                }
            }

            // Lighting settings
            GUILayout.Space(10);
            lightSettings = EditorGUILayout.Foldout(lightSettings, "Lighting");
            if (lightSettings)
            {
                if (volRendObj.GetRenderMode() == RenderMode.DirectVolumeRendering)
                {
                    Task task = volRendObj.SetLightingEnabledAsync(GUILayout.Toggle(volRendObj.GetLightingEnabled(), "Enable lighting"), new ProgressHandler(this));
                }
                else
                    volRendObj.SetLightingEnabled(false);

                if (volRendObj.GetLightingEnabled() || volRendObj.GetRenderMode() == RenderMode.IsosurfaceRendering)
                {
                    LightSource oldLightSource = volRendObj.GetLightSource();
                    LightSource newLightSource = (LightSource)EditorGUILayout.EnumPopup("Light source", oldLightSource);
                    if (newLightSource != oldLightSource)
                        volRendObj.SetLightSource(newLightSource);

                    // Gradient lighting threshold: Threshold for how low gradients can contribute to lighting.
                    Vector2 gradLightThreshold = volRendObj.GetGradientLightingThreshold();
                    // Convert to square root scaling (=> more precision close to 0)
                    gradLightThreshold = new Vector2(Mathf.Sqrt(gradLightThreshold.x), Mathf.Sqrt(gradLightThreshold.y));
                    EditorGUILayout.MinMaxSlider(
                        new GUIContent("Gradient lighting threshold",
                            "Minimum and maximum threshold for gradient contribution to lighting.\n"
                            + "Voxels with gradient less than min will be unlit, and with gradient >= max will fully shaded."),
                        ref gradLightThreshold.x, ref gradLightThreshold.y, 0.0f, 1.0f
                    );
                    // Convert back to linear scale, before setting updated value.
                    volRendObj.SetGradientLightingThreshold(new Vector2(gradLightThreshold.x * gradLightThreshold.x, gradLightThreshold.y * gradLightThreshold.y));
                }
            }

            // HighLight Settings
            GUILayout.Space(10);
            highlightSettings = EditorGUILayout.Foldout(highlightSettings, "High Light Settings");

            // Other settings
            GUILayout.Space(10);
            otherSettings = EditorGUILayout.Foldout(otherSettings, "Other Settings");
            if (otherSettings)
            {
                if (volRendObj.GetRenderMode() == RenderMode.DirectVolumeRendering)
                {
                    // Early ray termination
                    volRendObj.SetRayTerminationEnabled(GUILayout.Toggle(volRendObj.GetRayTerminationEnabled(), "Enable early ray termination"));
                }

                volRendObj.SetCubicInterpolationEnabled(GUILayout.Toggle(volRendObj.GetCubicInterpolationEnabled(), "Enable cubic interpolation (better quality)"));
            }
        }
    }
}
