using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
namespace C2M2.Utils.DebugUtils.Actions
{
    /// <summary>
    /// Allows user to press a button to pause editor play mode from within the application
    /// </summary>
    public class EditorButtonPause : MonoBehaviour
    {
        public bool allowXRPause = true;
        public XRNode xrPauseHand = XRNode.LeftHand;
        public bool allowKeyboardPause = true;
        public KeyCode keyboardPauseButton = KeyCode.Space;
        private bool previousXRButtonState = false;
        // Update is called once per frame
        void Update()
        {
            if (allowXRPause)
            {
                if (GetXRButtonDown(xrPauseHand, CommonUsages.primary2DAxisClick))
                {
                    Debug.Break();
                    Debug.Log("Editor Paused");
                }
            }
            if (allowKeyboardPause)
            {
                if (Input.GetKeyDown(keyboardPauseButton))
                {
                    Debug.Break();
                    Debug.Log("Editor Paused");
                }
            }
        }
        private bool GetXRButtonDown(XRNode node, InputFeatureUsage<bool> usage)
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(node);

            bool currentState = false;

            if (device.isValid)
                device.TryGetFeatureValue(usage, out currentState);

            bool pressedThisFrame = currentState && !previousXRButtonState;
            previousXRButtonState = currentState;

            return pressedThisFrame;
        }
    }
}