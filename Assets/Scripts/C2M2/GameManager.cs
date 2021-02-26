﻿#pragma warning disable 0618 // Ignore obsolete script warning

using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace C2M2
{
    using Interaction;
    using Interaction.UI;
    using Interaction.VR;
    using NeuronalDynamics.Interaction;
    using Simulation;
    using Visualization;
    /// <summary>
    /// Stores many global variables, handles pregame initializations
    /// </summary>
    [RequireComponent(typeof(NeuronClampManager))]
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance = null;
        
        public int mainThreadId { get; private set; } = -1;

        public VRDeviceManager vrDeviceManager = null;
        public bool VrIsActive {
            get
            {
                if (vrDeviceManager == null) Debug.LogError("No VR Device Manager Found!");
                return vrDeviceManager.VrIsActive;
            }
        }

        public GameObject cellPreviewer = null;
        public Interactable activeSim = null;
        public List<SimulationTimerLabel> timerLabels = new List<SimulationTimerLabel>();

        public NeuronClampManager ndClampManager = null;
        //  public GameObject[] clampControllers = new GameObject[0];

        [Header("Environment")]
        public int roomSelected = 0;
        public Room[] roomOptions = null;
        public Color wallColor = Color.white;
        [Header("Materials")]
        public Material defaultMaterial = null;
        public Material vertexColorationMaterial = null;
        public Material lineRendMaterial = null;

        [Tooltip("Used as an anchor point for neuron diameter control panel")]
        public Transform whiteboard = null;
        public Vector3 objScaleDefault = new Vector3(2f, 2f, 2f);
        public Vector3 objScaleMax = new Vector3(4f, 4f, 4f);
        public Vector3 objScaleMin = new Vector3(0.3f, 0.3f, 0.3f);

        [Header("OVR Player Controller")]
        public GameObject ovrRightHandAnchor = null;
        public GameObject ovrLeftHandAnchor = null;
        public OVRPlayerController ovrPlayerController { get; set; } = null;
        public GameObject nonVRCamera { get; set; } = null;

        [Header("FPS Counter")]
        public Utils.DebugUtils.FPSCounter fpsCounter;
        private bool isRunning = false;

        [Header("Obsolete")]
        public RaycastForward rightRaycaster;
        public RaycastForward leftRaycaster;
        public GameObject menu = null;
        public GameObject raycastKeyboardPrefab;
        public RaycastKeyboard raycastKeyboard { get; set; }
        public Transform menuSnapPosition;

        private void Awake()
        {
            // Initialize the GameManager
            DontDestroyOnLoad(gameObject);
            if (instance == null) { instance = this; }
            else if (instance != this) { Destroy(this); }

            mainThreadId = Thread.CurrentThread.ManagedThreadId;

            ndClampManager = GetComponent<NeuronClampManager>();
            if (ndClampManager == null) Debug.LogWarning("No neuron clamp manager found!");

            if(roomOptions != null && roomOptions.Length > 0)
            {
                Mathf.Clamp(roomSelected, 0, (roomOptions.Length - 1));
                // Only enable selected room, disable all others
                for(int i = 0; i < roomOptions.Length; i++)
                {
                    bool selected = (i == roomSelected) ? true : false;
                    roomOptions[i].gameObject.SetActive(selected);
                }
                // Apply wall color to selected room's walls
                if (roomOptions[roomSelected].walls != null && roomOptions[roomSelected].walls.Length > 0)
                {
                    foreach (MeshRenderer wall in roomOptions[roomSelected].walls)
                    {
                        wall.material.color = wallColor;
                    }
                }
            }
        }

        private void Update()
        {
            if(logQ != null && logQ.Count > 0)
            { // print every queued statement
                foreach (string s in logQ) { Debug.Log(s); }
                logQ.Clear();
            }
            if (eLogQ != null && eLogQ.Count > 0)
            { // print every queued statement
                foreach (string s in eLogQ) { Debug.LogError(s); }
                eLogQ.Clear();
            }
        }
        public void RaycasterRightChangeColor(Color color) => rightRaycaster.ChangeStaticHandColor(color);
        public void RaycasterLeftChangeColor(Color color) => leftRaycaster.ChangeStaticHandColor(color);


        private List<string> logQ = new List<string>();
        private readonly int logQCap = 100;
        /// <summary>
        /// Allows other threads to submit messages to be printed at the start of the next frame
        /// </summary>
        /// <remarks>
        /// Making any Unity API call from another thread is not safe. This method is a quick hack
        /// to avoid mkaing a Unity API call from another thread.
        /// </remarks>
        public void DebugLogSafe(string s)
        {
            if (isRunning)
            {
                if (logQ.Count > logQCap)
                {
                    Debug.LogWarning("Cannot call DebugLogSafe more than [" + logQCap + "] times per frame. New statements will not be added to queue");
                    return;
                }
                logQ.Add(s);
            }
        }
        public void DebugLogThreadSafe<T>(T t) => DebugLogSafe(t.ToString());

        private List<string> eLogQ = new List<string>();
        private readonly int eLogQCap = 100;
        /// <summary>
        /// Allows other threads to submit messages to be printed at the start of the next frame
        /// </summary>
        /// <remarks>
        /// Making any Unity API call from another thread is not safe. This method is a quick hack
        /// to avoid mkaing a Unity API call from another thread.
        /// </remarks>
        public void DebugLogErrorSafe(string s)
        {
            if (isRunning)
            {
                if (eLogQ.Count > eLogQCap)
                {
                    Debug.LogWarning("Cannot call DebugLogSafe more than [" + logQCap + "] times per frame. New statements will not be added to queue");
                    return;
                }
                eLogQ.Add(s);
            }
        }
        public void DebugLogErrorThreadSafe<T>(T t) => DebugLogErrorSafe(t.ToString());

        private void OnApplicationQuit()
        {
            isRunning = false;
        }
        private void OnApplicationPause(bool pause)
        {
            isRunning = !pause;
        }
    }
}