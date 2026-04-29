using UnityEngine;

using C2M2.Interaction.VR;
namespace C2M2.Interaction
{
    public class MouseEventSignaler : RaycastEventSignaler
    {
        Transform grabTransform;
        PublicOVRGrabber grabber;
        SphereCollider grabVolume;

        protected override void OnAwake()
        {
            grabTransform = new GameObject().transform;
            // Name grabber object and initialize it 
            grabTransform.name = "EmulatorGrab";
            grabTransform.position = Vector3.zero;
            grabTransform.rotation = Quaternion.identity;
            grabTransform.localScale = Vector3.one;

            // Create grab collider
            grabVolume = grabTransform.gameObject.AddComponent<SphereCollider>();
            grabVolume.radius = 0.1f;
            grabVolume.isTrigger = true;

            // Create Rigidbody first
            Rigidbody rb = grabTransform.gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            // Create grabber
            grabber = grabTransform.gameObject.AddComponent<PublicOVRGrabber>();
            grabber.M_GrabVolumes = new Collider[] { grabVolume };
            grabTransform.name = "EmulatorGrab";
        }

        protected override void OnStart() { }
        
        // Mouse constantly raycasts
        protected override bool RaycastRequested() => true;
        /// <summary>
        /// This builds a ray from the mouse's position, and attempts a raycast using that ray
        /// </summary>
        protected override bool RaycastingMethod(out RaycastHit hit, float maxDistance, LayerMask layerMask)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool raycastHit = Physics.Raycast(ray, out hit, maxDistance, layerMask);

            return raycastHit;
        }
        // Left mouse button presses
        protected override bool PressCondition() => Input.GetMouseButton(0);
    }
}
