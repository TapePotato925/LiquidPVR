using LiquidPVR;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace LiquidPVR.Editor
{
    public static class LiquidPVRSceneBootstrapper
    {
        private const string ScenePath = "Assets/Scenes/LiquidPVRField.unity";
        private const int VirtualContentLayer = 8;

        [MenuItem("LiquidPVR/Create Initial AR/Cardboard Scene")]
        public static void CreateInitialScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            new GameObject("AR Session").AddComponent<ARSession>();

            var originObject = new GameObject("XR Origin (Mobile AR)");
            var origin = originObject.AddComponent<XROrigin>();
            originObject.AddComponent<ARPlaneManager>().requestedDetectionMode = PlaneDetectionMode.Horizontal;

            var cameraOffset = new GameObject("Camera Offset");
            cameraOffset.transform.SetParent(originObject.transform, false);
            var arCameraObject = new GameObject("AR Camera");
            arCameraObject.tag = "MainCamera";
            arCameraObject.transform.SetParent(cameraOffset.transform, false);
            var arCamera = arCameraObject.AddComponent<Camera>();
            arCamera.nearClipPlane = 0.05f;
            arCameraObject.AddComponent<ARCameraManager>();
            arCameraObject.AddComponent<ARCameraBackground>();
            origin.Camera = arCamera;
            origin.CameraFloorOffsetObject = cameraOffset;

            var stereo = originObject.AddComponent<CardboardStereoPresenter>();
            var serializedStereo = new SerializedObject(stereo);
            serializedStereo.FindProperty("trackingCamera").objectReferenceValue = arCamera;
            serializedStereo.FindProperty("virtualContentMask").intValue = 1 << VirtualContentLayer;
            serializedStereo.ApplyModifiedPropertiesWithoutUndo();

            var field = GameObject.CreatePrimitive(PrimitiveType.Plane);
            field.name = "Initial Field";
            field.layer = VirtualContentLayer;
            field.transform.position = new Vector3(0f, -1.6f, 2.5f);
            field.transform.localScale = Vector3.one * 0.5f;

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Selection.activeGameObject = originObject;
            Debug.Log("Created LiquidPVRField. Enable ARKit in XR Plug-in Management before building for iOS.");
        }
    }
}
