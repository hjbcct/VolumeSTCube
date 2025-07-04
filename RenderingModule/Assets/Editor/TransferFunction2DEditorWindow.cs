﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace UnityVolumeRendering
{
    public class TransferFunction2DEditorWindow : EditorWindow
    {
        private Texture2D hist2DTex = null;

        private bool needsRegenTexture = true;

        private Material tfGUIMat = null;
        private int selectedBoxIndex = -1;
        private bool isMovingBox = false;

        private VolumeRenderedObject volRendObject = null;
        private VolumeControllerObject volControlObj = null;

        private List<ResizableArea> tfAreas = new List<ResizableArea>();

        public static void ShowWindow()
        {
            // Close all (if any) 1D TF editor windows
            TransferFunctionEditorWindow[] tf1dWnds = Resources.FindObjectsOfTypeAll<TransferFunctionEditorWindow>();
            foreach (TransferFunctionEditorWindow tf1dWnd in tf1dWnds)
                tf1dWnd.Close();

            TransferFunction2DEditorWindow tf2dWnd = (TransferFunction2DEditorWindow)EditorWindow.GetWindow(typeof(TransferFunction2DEditorWindow));
            tf2dWnd.Show();
            tf2dWnd.SetInitialPosition();
        }

        private void SetInitialPosition()
        {
            Rect rect = this.position;
            rect.width = 800.0f;
            rect.height = 500.0f;
            this.position = rect;
        }

        private void OnEnable()
        {
            tfGUIMat = Resources.Load<Material>("TransferFunction2DGUIMat");

            volRendObject = SelectionHelper.GetSelectedVolumeObject();
            if (volRendObject == null)
            {
                Debug.Log(123123);
                volControlObj = GameObject.FindObjectOfType<VolumeControllerObject>();
            }
            //volRendObject = GameObject.FindObjectOfType<VolumeRenderedObject>();
            else if (volControlObj == null)
            {
                volRendObject = GameObject.FindObjectOfType<VolumeRenderedObject>();
            }
            if (volRendObject != null)
                volRendObject.SetTransferFunctionMode(TFRenderMode.TF2D);
            if(volControlObj != null)
                volControlObj.SetTransferFunctionMode(TFRenderMode.TF2D);
        }

        private void OnGUI()
        {
            Vector2 mousePos = new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y);

            // Update selected object
            //if (volRendObject == null)
            //    volRendObject = SelectionHelper.GetSelectedVolumeObject();
            
            if (volRendObject == null && volControlObj == null)
                return;

            if (hist2DTex == null)
            {
                if (volRendObject != null)
                {
                    hist2DTex = HistogramTextureGenerator.Generate2DHistogramTexture(volRendObject.dataset);
                }
                else
                {
                    hist2DTex = HistogramTextureGenerator.Generate2DHistogramTexture(volControlObj.volumeContainerObjects[0].dataset);
                }
            }
            TransferFunction2D tf2d = null;
            if (volRendObject != null)
            {
                tf2d = volRendObject.transferFunction2D;
            }
            else if (volControlObj != null)
            {
                tf2d = volControlObj.transferFunction2D;
            }

            if (tf2d.boxes.Count != tfAreas.Count)
            {
                Debug.Log("old count: " + tfAreas.Count + ", new count: " + tf2d.boxes.Count);
                tfAreas.Clear();
                foreach (TransferFunction2D.TF2DBox tfBox in tf2d.boxes)
                {
                    ResizableArea area = new ResizableArea();
                    tfAreas.Add(area);
                }
            }

            // Calculate GUI width (minimum of window width and window height * 2)
            float bgWidth = Mathf.Min(this.position.width - 20.0f, (this.position.height - 250.0f) * 2.0f);
            // Draw the histogram
            Rect histRect = new Rect(0.0f, 0.0f, bgWidth, bgWidth * 0.5f);
            Graphics.DrawTexture(histRect, hist2DTex);
            // Draw the TF texture (showing the rectangles)
            tfGUIMat.SetTexture("_TFTex", tf2d.GetTexture());
            Graphics.DrawTexture(histRect, tf2d.GetTexture(), tfGUIMat);

            // Handle mouse drag
            for (int i = 0; i < tf2d.boxes.Count; i++)
            {
                int iBox = (i + selectedBoxIndex + 1) % tf2d.boxes.Count;
                TransferFunction2D.TF2DBox box = tf2d.boxes[iBox];
                ResizableArea tfArea = tfAreas[iBox];

                if (isMovingBox && selectedBoxIndex == iBox)
                {
                    if (Event.current.type == EventType.MouseUp)
                    {
                        tfArea.StopMoving();
                        isMovingBox = false;
                    }
                    else
                        tfArea.UpdateMoving(mousePos);

                    if (tfArea.rectChanged)
                    {
                        Rect rect = tfArea.GetRect();
                        box.rect.x = rect.x / histRect.width - histRect.x;
                        box.rect.y = 1.0f - (rect.y + rect.height) / histRect.height;
                        box.rect.width = rect.width / histRect.width;
                        box.rect.height = rect.height / histRect.height;
                        tf2d.boxes[iBox] = box;
                        needsRegenTexture = true;
                    }
                }
                else
                {
                    Rect boxRect = new Rect(histRect.x + box.rect.x * histRect.width, histRect.y + (1.0f - box.rect.height - box.rect.y) * histRect.height, box.rect.width * histRect.width, box.rect.height * histRect.height);
                    tfArea.SetRect(boxRect);
                }

                tfArea.Draw();
            }

            if (Event.current.type == EventType.MouseDown)
            {
                // First priority: Pick area where mouse intersects the border.
                int candidate = GetIntersectingAreas(selectedBoxIndex + 1, (ResizableArea area) => { return area.IntersectsBorder(mousePos); });
                // Second priority: Pick area where mouse intersects the rect.
                if (candidate == -1)
                    candidate = GetIntersectingAreas(selectedBoxIndex, (ResizableArea area) => { return area.Intersects(mousePos); });
                if (candidate != -1)
                {
                    selectedBoxIndex = candidate;
                    isMovingBox = true;
                    tfAreas[candidate].StartMoving(mousePos);
                }
            }

            float startX = histRect.x;
            float startY = histRect.y + histRect.height + 10;
            // Show GUI for editing selected rectangle
            if (selectedBoxIndex != -1)
            {
                EditorGUI.BeginChangeCheck();
                TransferFunction2D.TF2DBox box = tf2d.boxes[selectedBoxIndex];
                box.colour = EditorGUI.ColorField(new Rect(startX + 250.0f, startY + 10, 100.0f, 20.0f), box.colour);
                box.minAlpha = EditorGUI.Slider(new Rect(startX + 250.0f, startY + 30, 200.0f, 20.0f), "min alpha", box.minAlpha, 0.0f, 1.0f);
                box.alpha = EditorGUI.Slider(new Rect(startX + 250.0f, startY + 60, 200.0f, 20.0f), "max alpha", box.alpha, 0.0f, 1.0f);

                tf2d.boxes[selectedBoxIndex] = box;
                needsRegenTexture |= EditorGUI.EndChangeCheck();
            }
            else
            {
                EditorGUI.LabelField(new Rect(startX, startY, this.position.width - startX, 40.0f), "Select a rectangle in the above view, or add a new one.");
            }

            // Add new rectangle
            if (GUI.Button(new Rect(startX, startY + 40, 150.0f, 30.0f), "Add rectangle"))
            {
                tf2d.AddBox(0.1f, 0.1f, 0.8f, 0.8f, Color.white, 0.5f);
                needsRegenTexture = true;
            }
            // Remove selected shape
            if (selectedBoxIndex != -1)
            {
                if (GUI.Button(new Rect(startX, startY + 80, 150.0f, 30.0f), "Remove selected shape"))
                {
                    tf2d.boxes.RemoveAt(selectedBoxIndex);
                    selectedBoxIndex = -1;
                    needsRegenTexture = true;
                }
            }

            //if(GUI.Button(new Rect(startX, startY + 120, 150.0f, 30.0f), "Save"))
            //{
            //    string filepath = EditorUtility.SaveFilePanel("Save transfer function", "", "default.tf2d", "tf2d");
            //    if(filepath != "")
            //        TransferFunctionDatabase.SaveTransferFunction2D(tf2d, filepath);
            //}
            //if(GUI.Button(new Rect(startX, startY + 160, 150.0f, 30.0f), "Load"))
            //{
            //    string filepath = EditorUtility.OpenFilePanel("Save transfer function", "", "tf2d");
            //    if(filepath != "")
            //    {
            //        TransferFunction2D newTF = TransferFunctionDatabase.LoadTransferFunction2D(filepath);
            //        if(newTF != null)
            //        {
            //            volRendObject.transferFunction2D = tf2d = newTF;
            //            needsRegenTexture = true;
            //        }
            //    }
            //}
        }

        private void OnSelectionChange()
        {
            VolumeRenderedObject newVolRendObj = Selection.activeGameObject?.GetComponent<VolumeRenderedObject>();
            // If we selected another volume object than the one previously edited in this GUI
            if (volRendObject != null && newVolRendObj != null && newVolRendObj != volRendObject)
                this.Close();
        }

        public void OnInspectorUpdate()
        {
            Repaint();

            // TODO: regenerate on add/remove/modify (and do it async)
            if (needsRegenTexture)
            {
                TransferFunction2D tf2d = null;
                //TransferFunction2D tf2d = volRendObject.transferFunction2D;
                if (volControlObj != null)
                {
                    tf2d = volControlObj.transferFunction2D;
                }
                else
                {
                    tf2d = volRendObject.transferFunction2D;
                }
                if(tf2d == null)
                {
                    Debug.LogWarning("Transfer function 2D is null");
                    return;
                }
                tf2d.GenerateTexture();
                needsRegenTexture = false;
            }
        }

        private int GetIntersectingAreas(int startIndex, System.Func<ResizableArea, bool> comparator)
        {
            for (int i = 0; i < tfAreas.Count; i++)
            {
                int iBox = (i + selectedBoxIndex) % tfAreas.Count;
                if (comparator(tfAreas[iBox]))
                    return iBox;
            }
            return -1;
        }
    }
}
