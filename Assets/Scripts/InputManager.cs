using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    private struct ScreenBounds
    {
        public float xMax;
        public float xMin;
        public float yMax;
        public float yMin;
    }

    [Range(0f, 0.9f), Tooltip("Percentage of screen that is ignored when screen is tapped.")]
    public float tapRangeX, tapRangeY;

    private ScreenBounds screenBounds;
    public int taps { get; private set; }

    private void Awake()
    {
        CalculateScreenBounds();
    }

    private void Update()
    {
        //Non-Touch Input for in Editor
        if (Application.isEditor)
        {
            if (Input.GetMouseButtonDown(0) && ValidTap(Input.mousePosition))
            {
                OnTap();
            }
        }
        else
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began && ValidTap(Input.GetTouch(0).position))
            {
                OnTap();
            }
        }

    }

    private void OnTap()
    {
        taps++;
        GameManager.instance.ScreenTapped();
    }

    private void CalculateScreenBounds()
    {
        if (tapRangeX != 0)
        {
            screenBounds.xMin = Camera.main.pixelWidth * tapRangeX;
            screenBounds.xMax = Camera.main.pixelWidth - screenBounds.xMin;
        }
        else
        {
            screenBounds.xMax = Camera.main.pixelWidth;
        }
        if (tapRangeY != 0)
        {
            screenBounds.yMin = Camera.main.pixelHeight * tapRangeY;
            screenBounds.yMax = Camera.main.pixelHeight - screenBounds.yMin;
        }
        else
        {
            screenBounds.yMax = Camera.main.pixelHeight;
        }
    }

    private bool ValidTap(Vector3 pos)
    {
        if ((pos.x < screenBounds.xMax && pos.x > screenBounds.xMin) && (pos.y < screenBounds.yMax && pos.y > screenBounds.yMin))
        {
            return true;
        }
        Debug.LogWarning("Tap was not in range");
        return false;
    }

}
