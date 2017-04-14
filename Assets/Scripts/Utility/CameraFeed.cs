using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class CameraFeed : MonoBehaviour {

    private WebCamTexture cameraFeed;
    private RawImage image;

    private void Start()
    {
        image = GetComponent<RawImage>();
        cameraFeed = new WebCamTexture();
        image.texture = cameraFeed;
        cameraFeed.Play();
    }
}
