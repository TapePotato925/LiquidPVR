using UnityEngine;

namespace LiquidPVR
{
    /// <summary>
    /// Renders virtual content in side-by-side stereo while ARKit supplies the
    /// centre-eye 6DoF pose. The AR camera continues to draw the mono camera feed.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CardboardStereoPresenter : MonoBehaviour
    {
        [SerializeField] private Camera trackingCamera;
        [SerializeField, Min(0.04f)] private float interPupillaryDistance = 0.064f;
        [SerializeField] private bool stereoEnabled = true;
        [SerializeField] private LayerMask virtualContentMask = ~0;

        private Camera leftEye;
        private Camera rightEye;

        private void Awake()
        {
            if (trackingCamera == null)
                trackingCamera = Camera.main;

            if (trackingCamera == null)
            {
                Debug.LogError("LiquidPVR requires an AR tracking camera.", this);
                enabled = false;
                return;
            }

            CreateEyes();
            SetStereo(stereoEnabled);
        }

        private void LateUpdate()
        {
            if (!stereoEnabled || leftEye == null || rightEye == null)
                return;

            var halfIpd = interPupillaryDistance * 0.5f;
            var pose = trackingCamera.transform;
            leftEye.transform.SetPositionAndRotation(pose.position - pose.right * halfIpd, pose.rotation);
            rightEye.transform.SetPositionAndRotation(pose.position + pose.right * halfIpd, pose.rotation);
        }

        public void SetStereo(bool enabled)
        {
            stereoEnabled = enabled;
            if (leftEye != null) leftEye.enabled = enabled;
            if (rightEye != null) rightEye.enabled = enabled;
        }

        private void CreateEyes()
        {
            leftEye = CreateEye("Cardboard Left Eye", new Rect(0f, 0f, 0.5f, 1f), 1);
            rightEye = CreateEye("Cardboard Right Eye", new Rect(0.5f, 0f, 0.5f, 1f), 2);

            // ARCameraBackground is owned by the centre camera. It provides the
            // camera image once; the eye cameras add only virtual content over it.
            trackingCamera.cullingMask = 0;
            trackingCamera.depth = 0;
        }

        private Camera CreateEye(string eyeName, Rect viewport, float depth)
        {
            var eye = new GameObject(eyeName).AddComponent<Camera>();
            eye.transform.SetParent(transform, false);
            eye.CopyFrom(trackingCamera);
            eye.rect = viewport;
            eye.depth = depth;
            eye.clearFlags = CameraClearFlags.Depth;
            eye.cullingMask = virtualContentMask;
            eye.stereoTargetEye = StereoTargetEyeMask.None;
            eye.tag = "Untagged";
            return eye;
        }
    }
}
