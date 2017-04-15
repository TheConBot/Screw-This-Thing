using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class CameraFeed : MonoBehaviour
{
    private WebCamDevice[] allDevices;
    private WebCamDevice frontFacingDevice;
    private WebCamTexture cameraFeed;
    private RawImage image;

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
