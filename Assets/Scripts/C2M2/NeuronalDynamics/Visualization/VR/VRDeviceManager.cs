﻿using UnityEngine;
using UnityEngine.XR;
using C2M2.Utils;
namespace C2M2.Interaction.VR
{
    using Interaction.Signaling;
    /// <summary>
    /// Handles switching between VR and emulator modes
    /// </summary>
    [RequireComponent(typeof(MovingOVRHeadsetEmulator))]
    [RequireComponent(typeof(MouseEventSignaler))]
    [RequireComponent(typeof(OVRPlayerController))]
    [RequireComponent(typeof(MovementController))]
    [ExecuteInEditMode]
    public class VRDeviceManager : MonoBehaviour
    {
        public GameObject informationOverlay = null;
        public GameObject informationDisplayTV = null;
        private MovingOVRHeadsetEmulator emulator;
        private MouseEventSignaler mouseSignaler;
        private OVRPlayerController playerController;
        private MovementController emulatorMove;

        //private Transform leftEye;
        private Transform centerEye;
        //private Transform rightEye;
        private Transform leftHand;
        private Transform rightHand;

        private Vector3 initialPlayerPositon;
        private Quaternion initialPlayerRotation;
        //private Vector3 initialLeftEyePositon;
        //private Quaternion initialLeftEyeRotation;
        private Vector3 initialCenterEyePositon;
        private Quaternion initialCenterEyeRotation;
        //private Vector3 initialRightEyePositon;
        //private Quaternion initialRightEyeRotation;
        private Vector3 initialLeftHandPositon;
        private Quaternion initialLeftHandRotation;
        private Vector3 initialRightHandPositon;
        private Quaternion  initialRightHandRotation;

        private readonly KeyCode switchModeKey = KeyCode.Space;
        private readonly OVRInput.Button switchModeButton = OVRInput.Button.Any;

        public bool VRActive { get; set; } = false;
        public bool VRDevicePresent { get { return !VRDevice.Equals(string.Empty); } }
        public string VRDevice { get; private set; }

        private void Awake()
        {   
            Camera[] cameras = GetComponentsInChildren<Camera>();
            //leftEye = cameras[0].transform;
            centerEye = cameras[1].transform;
            //rightEye = cameras[2].transform;
            PublicOVRGrabber[] hands = GetComponentsInChildren<PublicOVRGrabber>();
            leftHand = hands[0].transform;
            rightHand = hands[1].transform;

            initialPlayerPositon = transform.position;
            initialPlayerRotation = transform.rotation;
            //initialLeftEyePositon = leftEye.position;
            //initialLeftEyeRotation = leftEye.rotation;
            initialCenterEyePositon = centerEye.position;
            initialCenterEyeRotation = centerEye.rotation;
            //initialRightEyePositon = rightEye.position;
            //initialRightEyeRotation = rightEye.rotation;
            initialLeftHandPositon = leftHand.position;
            initialLeftHandRotation = leftHand.rotation;
            initialRightHandPositon = rightHand.position;
            initialRightHandRotation = rightHand.rotation;


            emulator = GetComponent<MovingOVRHeadsetEmulator>();
            emulatorMove = GetComponent<MovementController>();
            mouseSignaler = GetComponent<MouseEventSignaler>();
            playerController = GetComponent<OVRPlayerController>();

            CheckForVRDevice();

            SwitchState(VRDevicePresent);
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.Tilde)) SwitchState(true); // temp for testing
            if (VRActive && Input.GetKey(switchModeKey)) SwitchState(false);
            else if (!VRActive && OVRInput.Get(switchModeButton))
            {
                if (!VRDevicePresent) CheckForVRDevice();
                if (VRDevicePresent) SwitchState(true);
                else Debug.LogError("No VR Device Present");
            }
        }

        private void CheckForVRDevice()
        {
            // Get VR device (or lack of one)
            // Note: in Unity 2019.4 XRDevice.model is obsolete but still works.
            InputDevice inputDevice = new InputDevice();
            Debug.Log("VR Device Name: " + inputDevice.name);
            VRDevice = XRDevice.model;
        }

        private void SwitchState(bool vrActive)
        {
            VRActive = vrActive;

            XRSettings.enabled = vrActive;

            playerController.enabled = vrActive;
            emulator.enabled = !vrActive;
            mouseSignaler.enabled = !vrActive;
            emulatorMove.enabled = !vrActive;

            ResetView();

            if (informationDisplayTV != null) informationDisplayTV.SetActive(vrActive);
            if (informationOverlay != null) informationOverlay.SetActive(!vrActive);

            // only enable oculus signalers if VR is enabled
            //OculusEventSignaler[] oculusSignalers = GetComponentsInChildren<OculusEventSignaler>();
            //foreach (OculusEventSignaler o in oculusSignalers)
            //{
                //o.enabled = vrActive;
            //}
        }

        private void ResetView()
        {
            transform.position = initialPlayerPositon;
            transform.rotation = initialPlayerRotation;
            //leftEye.position = initialLeftEyePositon;
            //leftEye.rotation = initialLeftEyeRotation;
            centerEye.position = initialCenterEyePositon;
            centerEye.rotation = initialCenterEyeRotation;
            //rightEye.position = initialRightEyePositon;
            //rightEye.rotation = initialRightEyeRotation;
            leftHand.position = initialLeftHandPositon;
            leftHand.rotation = initialLeftHandRotation;
            rightHand.position = initialRightHandPositon;
            rightHand.rotation = initialRightHandRotation;

            playerController.ResetOrientation(); //Not sure if this does anything or is helpful. Occulus documentation is pretty unclear
        }
    }
}