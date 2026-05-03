using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraLink : MonoBehaviour
{
    public Camera gameUICamera;
    public Camera sysUICamera;
    public Canvas gameWorldCanvas;

    public static CameraLink Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void LinkToMainCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Debug.Log("链接至 MainCamera");
            var cameraData = mainCam.GetUniversalAdditionalCameraData();
            if (cameraData == null) return;
            if (gameUICamera != null && !cameraData.cameraStack.Contains(gameUICamera))
            {
                cameraData.cameraStack.Add(gameUICamera);
            }
            if (sysUICamera != null && !cameraData.cameraStack.Contains(sysUICamera))
            {
                cameraData.cameraStack.Add(sysUICamera);
            }
            if (gameWorldCanvas != null)
                gameWorldCanvas.worldCamera = mainCam;
            PlayerController.mainCam = mainCam;
        }
    }
}
