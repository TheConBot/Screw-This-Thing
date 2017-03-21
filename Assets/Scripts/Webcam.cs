using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Webcam : MonoBehaviour {

    private WebCamTexture cameraFeed;
    public RawImage image;

    private void Start()
    {
        cameraFeed = new WebCamTexture();
        image.texture = cameraFeed;
        cameraFeed.Play();
    }
}
