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
    private ScreenBounds screenBounds;
    private bool onCooldown;
    [Header("Tap Settings"), Space()]
    [Range(0f, 0.9f), Tooltip("Percentage of screen that is ignored when screen is tapped.")]
    public float tapRangeX;
    [Range(0f, 0.9f), Tooltip("Percentage of screen that is ignored when screen is tapped.")]
    public float tapRangeY;
    [Range(1f, 60f), Tooltip("The maximum allowed taps per second.")]
    public int targetTapsPerSecond = 7;
    [HideInInspector]
    public int taps { get; private set; }

    private void Awake()
    {
        CalculateScreenBounds();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if(ValidEditorTap()) OnTap();
#else
        if(ValidTap()) OnTap();
#endif
    }

    private void OnTap()
    {
        StartCoroutine(TapCooldown());
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

    private bool ValidTap(bool isValid = false)
    {
        if (GameManager.instance.debugEnabled)
        {
            return !onCooldown;
        }
        isValid = Input.GetTouch(0).phase == TouchPhase.Began && !onCooldown && InScreenBounds(Input.GetTouch(0).position);
        return isValid;
    }

    private bool ValidEditorTap(bool isValid = false)
    {
        if (GameManager.instance.debugEnabled)
        {
            return !onCooldown;
        }
        isValid = Input.GetMouseButtonDown(0) && !onCooldown && InScreenBounds(Input.mousePosition);
        return isValid;
    }

    private bool InScreenBounds(Vector3 pos)
    {
        return (pos.x < screenBounds.xMax && pos.x > screenBounds.xMin) && (pos.y < screenBounds.yMax && pos.y > screenBounds.yMin);
    }

    private IEnumerator TapCooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(1 / targetTapsPerSecond);
        onCooldown = false;
    }
}
