﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C2M2.Simulation;
using TMPro;
namespace C2M2.Visualization
{
    public class GradientDisplay : MonoBehaviour
    {
        public LineRenderer linerend = null;
        public Gradient gradient;
        public float displayLength = 75;
        public float displayHeight = 10;
        public int numTextMarkers = 5;
        public MeshSimulation sim = null;
        public GameObject textMarkerPrefab = null;
        public GameObject textMarkerHolder = null;
        public TextMeshProUGUI unitText = null;
        public GameObject titleText = null;
        public string unit = "mV";
        public string precision = "F4";

        private void Awake()
        {
            if(linerend == null)
            {
                linerend = GetComponent<LineRenderer>();
                if(linerend == null)
                {
                    Debug.LogError("No line renderer found on " + name);
                    Destroy(this);
                }
            }

            linerend.positionCount = 2;
            linerend.SetPosition(0, Vector3.zero);
            linerend.SetPosition(1, new Vector3(displayLength, 0f, 0f));



            StartCoroutine(UpdateDisplayRoutine());
        }

        private IEnumerator UpdateDisplayRoutine()
        {
            // Wait for first frame to render, then run every 0.5 seconds
            yield return new WaitForEndOfFrame();
            while (true)
            {
                try
                {
                    UpdateDisplay();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    // We set hasChanged to be true so that it tries to update the display again
                    sim.colorLUT.hasChanged = true;
                }
                yield return new WaitUntil(() => sim.colorLUT.hasChanged == true);
            }
        }

        private void UpdateDisplay()
        {
            // Fetch graddient from simulation's colorLUT
            if (sim.colorLUT.hasChanged)
            {
                Debug.Log("Updating GradientDisplay...");

                gradient = sim.colorLUT.Gradient;

                GradientColorKey[] colorKeys = gradient.colorKeys;

                linerend.positionCount = colorKeys.Length;
                Vector3[] positions = new Vector3[colorKeys.Length];
                for (int i = 0; i < colorKeys.Length; i++)
                {
                    positions[i] = new Vector3(colorKeys[i].time * displayLength, 0f, 0f);
                }

                linerend.SetPositions(positions);

                linerend.colorGradient = gradient;

                linerend.startWidth = displayHeight;
                linerend.endWidth = displayHeight;

                UpdateText();

                sim.colorLUT.hasChanged = false;

                if(titleText != null)
                {
                    titleText.transform.localPosition = new Vector3(displayLength / 2, displayHeight, 0f);
                }
                else
                {
                    Debug.LogWarning("No title text found!");
                }
            }
        }
        private void UpdateText()
        {
            if (textMarkerHolder == null) { Debug.LogError("No text marker holder object found."); return; }
            if (textMarkerPrefab == null) { Debug.LogError("No text marker prefab found."); return; }

            // Clear old text markers
            foreach (TextMeshProUGUI marker in textMarkerHolder.GetComponentsInChildren<TextMeshProUGUI>())
            {
                Destroy(marker.gameObject);
            }
            BuildNewMarkers();

            if(unitText != null)
            {
                unitText.text = unit;
            }
        }
        private void BuildNewMarkers()
        {
            float max = sim.colorLUT.GlobalMax;
            float min = sim.colorLUT.GlobalMin;
            float valueStep = (max - min) / (numTextMarkers - 1);
            float placementStep = displayLength / (numTextMarkers - 1);
            for (int i = 0; i < numTextMarkers; i++)
            {
                GameObject newMarker = Instantiate(textMarkerPrefab, textMarkerHolder.transform);
                newMarker.transform.localPosition = new Vector3(i * placementStep, -displayHeight, 0f);
                newMarker.GetComponent<TextMeshProUGUI>().text = (min + (i * valueStep)).ToString(precision);
                LineRenderer lineMarker = newMarker.GetComponentInChildren<LineRenderer>();
                if(lineMarker != null)
                {
                    lineMarker.transform.localPosition = new Vector3(0f, displayLength / 2, 0f);
                    lineMarker.positionCount = 2;
                    lineMarker.SetPosition(0, Vector3.zero);
                    lineMarker.SetPosition(1, new Vector3(displayLength * 1.5f, 0f, 0f));
                }
                else
                {
                    Debug.LogWarning("No line marker found on text marker prefab!");
                }
            }        
        }
    }
}