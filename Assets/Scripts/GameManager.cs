using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(InputManager))]
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    //Private Vars
    private GameObject currentGameItem;
    private IEnumerator timerEnumerator;
    private ItemData currentItem;
    private List<GameObject> gameItems = new List<GameObject>();
    private List<ItemData> items = new List<ItemData>();
    private bool isPlaying;
    private bool isTransitioning;
    private bool isCountdown;
    private bool isEnding;
    private const int minRound = 1;
    private float roundTime;
    private int currentIndex;
    private int currentRound;
    private int maxRound;
    private int tapsThisRound;
    private int tapGoal;
    //Inspector Vars
    [SerializeField]
    private GameData gameData;
    public bool debugEnabled;
    [Header("UI References")]
    public CanvasGroup gamePanel;
    public CanvasGroup titlePanel;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI titleText;
    public Image timeRadial;
    public Slider tapSlider;
    public TextMeshProUGUI countdownText;
    [Header("UI Options")]
    public float fadeTransitionMultiplier;
    public int secondsBeforeRound = 1;
    public float countdownTimeScale = 1.5f;
    [Header("Effect Options")]
    public float shakeDuration = .15f;
    public float shakeMinMagnitude = .1f;
    public float shakeMaxMagnitude = 1;
    public float explosionForce = 500;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        Time.timeScale = 1;
        InstantiateItems();
        titlePanel.alpha = 1;
        gamePanel.alpha = 0;
        countdownText.text = "";
        timerEnumerator = Timer();
        SpawnNewItem();
        ToggleGameItem(currentGameItem);
    }

    private void EndGame()
    {
        Debug.Log("You fucked up!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator StartRound()
    {
        ResetGameUI();
        StartCoroutine(TransitionGameView(titlePanel, gamePanel));
        while (isTransitioning)
        {
            yield return null;
        }
        isCountdown = true;
        Time.timeScale = countdownTimeScale;
        float workingSeconds = secondsBeforeRound;
        int fontCounting = secondsBeforeRound;
        float origonalFontSize = countdownText.fontSize;
        while(workingSeconds > 0)
        {
            if(workingSeconds < fontCounting - 1)
            {
                countdownText.fontSize = origonalFontSize;
                fontCounting--;
            }
            workingSeconds -= Time.deltaTime;
            countdownText.text = ((int)workingSeconds + 1).ToString();
            countdownText.fontSize--;
            yield return new WaitForEndOfFrame();
        }
        countdownText.text = "";
        countdownText.fontSize = origonalFontSize;
        Time.timeScale = 1;
        isCountdown = false;
        Handheld.Vibrate();
        isPlaying = true;
        StartCoroutine(timerEnumerator);
    }

    private IEnumerator EndRound()
    {
        isEnding = true;
        if(currentRound == maxRound)
        {
            EndGame();
        }
        StopCoroutine(timerEnumerator);
        Rigidbody body = currentGameItem.GetComponent<Rigidbody>();
        body.useGravity = true;
        body.AddForce(Vector3.up * explosionForce);
        body.AddTorque(Vector3.up * explosionForce);
        float time = 4;
        while(time > 0)
        { 
            time -= Time.deltaTime;
            currentGameItem.transform.localScale *= 0.995f;
            yield return new WaitForEndOfFrame();
        }
        ToggleGameItem(currentGameItem);
        currentIndex++;
        SpawnNewItem();
        StartCoroutine(TransitionGameView(gamePanel, titlePanel));
        while (isTransitioning)
        {
            yield return null;
        }
        isEnding = false;
        isPlaying = false;
        ToggleGameItem(currentGameItem);
    }

    private void InstantiateItems()
    {
        items = gameData.itemList;
        items = SortListByRound(items);
        maxRound = items.Count;
        currentIndex = 0;
        foreach (var item in items)
        {
            GameObject gameItem;
            if (item.itemPrefab.GetComponent<CameraFeed>() != null)
            {
                gameItem = Instantiate(item.itemPrefab, FindObjectOfType<Canvas>().transform);
            }
            else
            {
                gameItem = Instantiate(item.itemPrefab);
            }
            gameItems.Add(gameItem);
            gameItem.SetActive(false);
        }
    }
    private void SpawnNewItem()
    {
        currentItem = items[currentIndex];
        currentGameItem = gameItems[currentIndex];
        currentRound = currentItem.roundNumber;
        roundTime = currentItem.roundTime;
        tapGoal = currentItem.tapGoal;
        tapsThisRound = 0;
        titleText.text = ("Screw This " + currentItem.displayName + "!").ToUpper();
        roundText.text = "Round " + currentRound;
    }

    private void ResetGameUI()
    {
        tapSlider.value = 0;
        tapSlider.maxValue = tapGoal;
        timeRadial.fillAmount = 1;
    }

    private IEnumerator Timer()
    {
        while (timeRadial.fillAmount > 0)
        {
            timeRadial.fillAmount = Mathf.Clamp(timeRadial.fillAmount - (Time.deltaTime * 1/roundTime), 0, 1);
            yield return new WaitForEndOfFrame();
        }

        if (!isTransitioning && isPlaying)
        {
            EndGame();
        }
    }

    public void ScreenTapped()
    {
        if (isTransitioning || isCountdown || isEnding)
        {
            return;
        }
        else if (isPlaying)
        {
            tapsThisRound++;
            UpdateTapSlider();
            StartCoroutine(ShakeGameObject(Camera.main.gameObject, shakeDuration, ScaledShakeMagnitude()));
            if (TapGoalReached())
            {
                StartCoroutine(EndRound());
            }
        }
        else
        {
            StartCoroutine(StartRound());
        }
    }

    private void UpdateTapSlider()
    {
        tapSlider.value++;
    }

    private float ScaledShakeMagnitude()
    {
        float scaledMagnitude = (float)tapsThisRound / tapGoal;
        scaledMagnitude = Mathf.Clamp(scaledMagnitude, shakeMinMagnitude, shakeMaxMagnitude);
        return scaledMagnitude;
    }

    private IEnumerator ShakeGameObject(GameObject gameObject, float duration, float magnitude)
    {
        float elapsed = 0.0f;
        Vector3 originalObjectPos = gameObject.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float percentComplete = elapsed / duration;
            float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);
            float x = Random.value * 2.0f - 1.0f;
            float y = Random.value * 2.0f - 1.0f;
            x *= magnitude * damper;
            y *= magnitude * damper;

            gameObject.transform.position = new Vector3(x, y, originalObjectPos.z);
            yield return null;
        }

        gameObject.transform.position = new Vector3(0, 0, originalObjectPos.z);
        yield return null;
    }

    private bool TapGoalReached()
    {
        return (tapsThisRound >= tapGoal);
    }

    private void ToggleGameItem(GameObject gameItem)
    {
        gameItem.SetActive(!gameItem.activeSelf);
    }

    private IEnumerator TransitionGameView(CanvasGroup fadeOut, CanvasGroup fadeIn)
    {
        isTransitioning = true;
        while (fadeIn.alpha != 1 && fadeOut.alpha != 0)
        {
            // Transition Multiplier makes the text not show the old item's descriptor, punch cut text
            fadeIn.alpha += Time.deltaTime * fadeTransitionMultiplier;
            fadeOut.alpha -= Time.deltaTime * fadeTransitionMultiplier;
            yield return new WaitForEndOfFrame();
        }
        isTransitioning = false;
    }
    
    private List<ItemData> SortListByRound(List<ItemData> list)
    {
        list.Sort(delegate (ItemData a, ItemData b) { return a.roundNumber.CompareTo(b.roundNumber); });
        return list;
    }
}
