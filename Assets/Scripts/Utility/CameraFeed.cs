using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class CameraFeed : MonoBehaviour
{
    //Private Vars
    private RawImage image;
    private WebCamDevice[] allDevices;
    private WebCamDevice frontFacingDevice;
    private WebCamTexture cameraFeed;

    //MonoBehaviour Functions
    private void Start()
    {
        allDevices = WebCamTexture.devices;
        image = GetComponent<RawImage>();
        if(cameraFeed = FrontCameraFeed())
        {
            image.texture = cameraFeed;
            cameraFeed.Play();
        }
    }

    public void Stop()
    {
        cameraFeed.Stop();
    }

    //Utility Functions
    private WebCamTexture FrontCameraFeed()
    {
        foreach (var device in allDevices)
        {
            if (device.isFrontFacing)
            {
                return new WebCamTexture(device.name);
            }
        }
        return null;
    }
}
