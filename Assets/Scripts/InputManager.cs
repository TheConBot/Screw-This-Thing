using InControl;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class InputManager : MonoBehaviour
{
    //Private Vars
    private ScreenBounds screenBounds;
    private Vector2 validTapMin;
    private Vector2 validTapMax;
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
    public Image tapZone;

    //Inspector Vars
    [Header("Tap Options")]
    [Tooltip("Min percentage of screen that is ignored when screen is tapped.")]
    public Vector2 tapRangeMin;
    [Tooltip("Max percentage of screen that is ignored when screen is tapped.")]
    public Vector2 tapRangeMax;
    [Range(1f, 60f), Tooltip("The maximum allowed taps per second.")]
    public int maxTapsPerSecond = 7;
    [Header("Random Tap Range Options")]
    public bool randomTapRangeEnabled;
    public float difficultyAdjuster = 3.3f;
    public float xDif = 0.2f;
    public float yDif = 0.15f;

    //MonoBehaviour Functions
    private void Awake()
    {
        MaxOutTapRange();
    }

    private void Update()
    {
        if (!onCooldown)
        {
            if (ValidTap()) OnTap();
            if (InControl.InputManager.ActiveDevice.LeftStick.HasChanged) OnSwipe();
        }
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
            isValid = (touch.phase == TouchPhase.Began && InScreenBounds(touch.position));
        }
        return isValid;
    }

    private void OnSwipe()
    {
        StartCoroutine(InputCooldown());
        GameManager.instance.ScreenSwiped(InControl.InputManager.ActiveDevice.LeftStick.Value);
    }

    private void OnTap()
    {
        StartCoroutine(InputCooldown());
        taps++;
        GameManager.instance.ScreenTapped();
    }

    //Core Coroutines
    private IEnumerator InputCooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(1 / maxTapsPerSecond);
        onCooldown = false;
    }

    //Utility Functions
    private bool InScreenBounds(Vector2 pos)
    {
        return (pos.x < screenBounds.xMax && pos.x > screenBounds.xMin) && (pos.y < screenBounds.yMax && pos.y > screenBounds.yMin);
    }

    public void RandomTapRange()
    {
        validTapMin.x = Random.Range(tapRangeMin.x, tapRangeMax.x - xDif);
        validTapMin.y = Random.Range(tapRangeMin.y, tapRangeMax.y - yDif);
        validTapMax.x = validTapMin.x + xDif;
        validTapMax.y = validTapMin.y + yDif;
        tapZone.rectTransform.anchorMin = validTapMin;
        tapZone.rectTransform.anchorMax = validTapMax;
        tapZone.enabled = true;
        CalculateScreenBounds();
    }

    private void CalculateScreenBounds()
    {
        screenBounds.xMin = Camera.main.pixelWidth * validTapMin.x;
        screenBounds.xMax = Camera.main.pixelWidth * validTapMax.x;
        screenBounds.yMin = Camera.main.pixelHeight * validTapMin.y;
        screenBounds.yMax = Camera.main.pixelHeight * validTapMax.y;
    }

    public void MaxOutTapRange()
    {
        validTapMin = tapRangeMin;
        validTapMax = tapRangeMax;
        tapZone.enabled = false;
        CalculateScreenBounds();
    }
}
