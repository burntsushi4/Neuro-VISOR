using UnityEngine;
using UnityEngine.XR;
namespace C2M2.Utils
{
    public class QuitGame : MonoBehaviour
    {
        public KeyCode quitKey = KeyCode.Escape;
        //public XRNode quitHand = XRNode.LeftHand; unused
        private bool XRRequested
        {
            get
            {
                return GetXRButton(XRNode.LeftHand, CommonUsages.menuButton) || GetXRButton(XRNode.RightHand, CommonUsages.menuButton);
            }
        }
        private bool QuitRequested
        {
            get
            {
                return GameManager.instance.vrDeviceManager.VRActive ?
                    (XRRequested || Input.GetKey(quitKey))
                    : Input.GetKey(quitKey);
            }
        }

        private bool GetXRButton(XRNode node, InputFeatureUsage<bool> usage)
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(node);

            if (device.isValid && device.TryGetFeatureValue(usage, out bool value))
                return value;

            return false;
        }

        [Tooltip("If true, game will quit after X frames.")]
        public bool QuitAfterX = false;
        [Tooltip("Number of frames to quit after.")]
        public int xFrames = 300;

        // Update is called once per frame
        void Update()
        {
            // Quit if the user requests or when the user requests
            if ((QuitAfterX && Time.frameCount >= xFrames) || QuitRequested)
            {
                Quit();
            }
        }

        private void Quit()
        {
#if UNITY_STANDALONE
            Application.Quit();
#endif
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}