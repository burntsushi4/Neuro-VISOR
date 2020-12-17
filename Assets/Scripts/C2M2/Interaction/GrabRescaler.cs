﻿using UnityEngine;
using C2M2.Interaction.VR;

namespace C2M2.Interaction
{
    /// <summary>
    /// Adds some percentage of a transform's original scale at runtime
    /// </summary>
    [RequireComponent(typeof(OVRGrabbable))]
    public class GrabRescaler : MonoBehaviour
    {
        private OVRGrabbable grabbable = null;
        private Vector3 origScale;
        public float scaler = 0.1f;
        public float minPercentage = 0.1f;
        public float maxPercentage = 10f;
        public bool xScale = true;
        public bool yScale = true;
        public bool zScale = true;
        private PublicOVRGrabber grabber;

        // Returns a value between -1 and 1, where -1 implies the thumbstick is all the way down and 1 implies it is all the way up
        private float ThumbstickScaler
        {
            get
            {
                // Uses joystick y axis value
                grabber = (PublicOVRGrabber)grabbable.grabbedBy;
                return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, grabber.Controller).y;
            }
        }

        private bool ThumbstickPressed
        {
            get
            {
                return OVRInput.Get(OVRInput.Button.PrimaryThumbstick);
            }
        }

        private void Start()
        {
            if (!GameManager.instance.vrIsActive) Destroy(this);

            grabbable = GetComponent<OVRGrabbable>();

            // Use this to determine how to scale at runtime
            origScale = transform.localScale;
        }

        void Update()
        {
            if (grabbable.isGrabbed)
            {
                // if joystick is pressed in, it resets the scale to the original scale
                if (ThumbstickPressed)
                {
                    transform.localScale = origScale;
                }
                else
                { // Otherwise resolve our new scale
                    Vector3 scaleValue = scaler * ThumbstickScaler * origScale;
                    Vector3 newLocalScale = transform.localScale + scaleValue;

                    // Is the new scale too big or too small?
                    bool newScaleAcceptable = newLocalScale.magnitude > (minPercentage * origScale).magnitude
                        && newLocalScale.magnitude < (maxPercentage * origScale).magnitude;
                    if (newScaleAcceptable)
                    {
                        if (!xScale) newLocalScale.x = transform.localScale.x;
                        if (!yScale) newLocalScale.y = transform.localScale.y;
                        if (!zScale) newLocalScale.z = transform.localScale.z;
                        transform.localScale = newLocalScale;
                    }
                }
            }
        }
    }
}