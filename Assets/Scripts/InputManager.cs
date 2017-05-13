using InControl;
using System.Collections;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    //Private Vars
    private ScreenBounds screenBounds;
    private bool onCooldown;
    private struct ScreenBounds
    {
        public float xMax;
        public float xMin;
        public float yMax;
        public float yMin;
    }
    [HideInInspector]
    public int taps { get; private set; }

    //Inspector Vars
    [Header("Tap Options")]
    [Range(0f, 0.9f), Tooltip("Percentage of screen that is ignored when screen is tapped.")]
    public float tapRangeX;
    [Range(0f, 0.9f), Tooltip("Percentage of screen that is ignored when screen is tapped.")]
    public float tapRangeY;
    [Range(1f, 60f), Tooltip("The maximum allowed taps per second.")]
    public int maxTapsPerSecond = 7;

    //MonoBehaviour Functions
    private void Awake()
    {
        CalculateScreenBounds();
    }

    private void Update()
    {
        if(ValidTap()) OnTap();
        if (InControl.InputManager.ActiveDevice.LeftStick.HasChanged) OnSwipe();
    }

    //Core Functions
    private bool ValidTap(bool isValid = false)
    {
        if (GameManager.instance.debugEnabled)
        {
            return !onCooldown;
        }
        if (TouchManager.TouchCount != 0)
        {
            InControl.Touch touch = TouchManager.GetTouch(0);
            isValid = (touch.phase == TouchPhase.Began && !onCooldown && InScreenBounds(touch.position));
        }
        return isValid;
    }

    private void OnSwipe()
    {
        GameManager.instance.ScreenSwiped(InControl.InputManager.ActiveDevice.LeftStick.Value);
    }

    private void OnTap()
    {
        StartCoroutine(TapCooldown());
        taps++;
        GameManager.instance.ScreenTapped();
    }

    //Core Coroutines
    private IEnumerator TapCooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(1 / maxTapsPerSecond);
        onCooldown = false;
    }

    //Utility Functions
    private bool InScreenBounds(Vector3 pos)
    {
        return (pos.x < screenBounds.xMax && pos.x > screenBounds.xMin) && (pos.y < screenBounds.yMax && pos.y > screenBounds.yMin);
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
}
