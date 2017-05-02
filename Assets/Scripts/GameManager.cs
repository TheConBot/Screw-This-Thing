using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI tapText;
    public TextMeshProUGUI countdownText;
    [Header("UI Options")]
    public float fadeTransitionMultiplier;
    public int secondsBeforeRound = 1;
    public float countdownTimeScale = 1.5f;
    [Header("Shake Options")]
    public float shakeDuration = .15f;
    public float shakeMinMagnitude = .1f;
    public float shakeMaxMagnitude = 1;

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
        countdownText.enabled = false;
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
        StartCoroutine(TransitionGameView(titlePanel, gamePanel));
        while (isTransitioning)
        {
            yield return null;
        }
        isCountdown = true;
        Time.timeScale = countdownTimeScale;
        countdownText.enabled = true;
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
        countdownText.enabled = false;
        countdownText.fontSize = origonalFontSize;
        Time.timeScale = 1;
        isCountdown = false;
        Handheld.Vibrate();
        isPlaying = true;
        StartCoroutine(timerEnumerator);
    }

    private IEnumerator EndRound()
    {
        if(currentRound == maxRound)
        {
            EndGame();
        }
        StopCoroutine(timerEnumerator);
        ToggleGameItem(currentGameItem);
        currentIndex++;
        SpawnNewItem();
        StartCoroutine(TransitionGameView(gamePanel, titlePanel));
        while (isTransitioning)
        {
            yield return null;
        }
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
        timeText.text = roundTime.ToString("F2");
        UpdateTapText();
    }

    private IEnumerator Timer()
    {
        while (roundTime > 0)
        {
            roundTime = Mathf.Clamp(roundTime - Time.deltaTime, 0, Mathf.Infinity);
            timeText.text = roundTime.ToString("F2");
            yield return new WaitForEndOfFrame();
        }

        if (!isTransitioning && isPlaying)
        {
            EndGame();
        }
    }

    public void ScreenTapped()
    {
        if (isTransitioning || isCountdown)
        {
            return;
        }
        else if (isPlaying)
        {
            tapsThisRound++;
            UpdateTapText();
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

    private void UpdateTapText()
    {
        if(tapGoal != 42)
        {
            tapText.text = tapsThisRound + "/" + tapGoal;

        }
        else {
            tapText.text = tapsThisRound + "/" + 42.0f.ToString("F1");
        }
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
        if (tapsThisRound >= tapGoal)
        {
            return true;
        }
        return false;
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
