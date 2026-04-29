using UnityEngine;
using UnityEngine.XR;

namespace C2M2.Interaction
{
    /// <summary>
    /// Simple OpenXR controller pose follower.
    /// Put this on Left Controller / Right Controller under OpenXR Rig > Camera Offset.
    /// </summary>
    public class OpenXRControllerPose : MonoBehaviour
    {
        public XRNode hand = XRNode.RightHand;

        private InputDevice device;

        private void Update()
        {
            if (!device.isValid)
                device = InputDevices.GetDeviceAtXRNode(hand);

            if (!device.isValid)
                return;

            if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
                transform.localPosition = position;

            if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
                transform.localRotation = rotation;
        }
    }
}