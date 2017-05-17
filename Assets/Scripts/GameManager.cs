using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(InputManager))]
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    //Private Vars
    private AudioClip[] currentEndSounds;
    private AudioClip[] currentIntroSounds;
    private AudioClip[] currentTapSounds;
    private AudioClip[] countdownSounds;
    private AudioClip transitionEndSound;
    private AudioClip transitionPlaySound;
    private GameObject currentGameItem;
    private GameState gameState;
    private IEnumerator timerEnumerator;
    private InputManager inputManager;
    private ItemData currentItem;
    private List<GameObject> gameItems = new List<GameObject>();
    private List<ItemData> items = new List<ItemData>();
    private Vector2 swipeDirection;
    private bool isTransitioning;
    private const int minRound = 1;
    private enum GameState
    {
        Idle,
        Starting,
        Playing,
        Ending
    }
    private float roundTime;
    private int currentIndex;
    private int currentRound;
    private int currentTapGoal;
    private int currentTaps;
    private int maxRound;

    //Inspector Vars
    [Header("Game References")]
    public GameData gameData;
    [Header("Debug Options")]
    public bool debugEnabled;
    public int startingIndex;
    [Header("UI References")]
    public CanvasGroup gamePanel;
    public CanvasGroup titlePanel;
    public GameObject swipePanel;
    public Image timeRadial;
    public Slider tapSlider;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI titleText;
    [Header("UI Options")]
    public float countdownTimeScale = 1.5f;
    public float fadeTransitionMultiplier;
    public int secondsBeforeRound = 1;
    [Header("Sound References")]
    public AudioSource endSource;
    public AudioSource introSource;
    public AudioSource tapSource;
    [Header("Effect Options")]
    public float explosionForce = 500;
    public float explosionDuration = 3;
    public float shakeDuration = .15f;
    public float shakeMaxMagnitude = 1;
    public float shakeMinMagnitude = .1f;
    public float swipeWaitDuration = 3;

    //MonoBehaviour Functions
    private void Start()
    {
        InitializeGame();
    }

    //Core Functions
    private void InitializeGame()
    {
        Time.timeScale = 1;
        InitializeItems();
        InitializeUI();
        inputManager = GetComponent<InputManager>();
        countdownSounds = gameData.countdownSounds;
        transitionPlaySound = gameData.transitionPlaySound;
        transitionEndSound = gameData.transitionEndSound;
        timerEnumerator = Timer();
        SpawnItem(currentIndex);
        ToggleGameObject(currentGameItem);
        PlaySoundEffect(introSource, currentIntroSounds);
    }

    private void InitializeItems()
    {
        items = gameData.itemList;
        items = SortListByRound(items);
        maxRound = items.Count;
        currentIndex = debugEnabled ? startingIndex : 0;
        foreach (var item in items)
        {
            GameObject gameItem;
            gameItem = Instantiate(item.itemPrefab);
            gameItem.name = item.displayName;
            gameItems.Add(gameItem);
            ToggleGameObject(gameItem);
        }
    }

    private void SpawnItem(int index)
    {
        currentItem = items[index];
        currentGameItem = gameItems[index];
        currentRound = currentItem.roundNumber;
        roundTime = currentItem.roundTime;
        if (inputManager.randomTapRangeEnabled)
        {
            currentTapGoal = Mathf.RoundToInt(Mathf.Clamp(currentItem.tapGoal / inputManager.difficultyAdjuster, 1.0f, Mathf.Infinity));
            Debug.Log(currentTapGoal);
        }
        else
        {
            currentTapGoal = currentItem.tapGoal;
        }
        currentIntroSounds = currentItem.introVOSounds;
        currentTapSounds = currentItem.tapSounds;
        currentEndSounds = currentItem.endSounds;
        currentTaps = 0;
        titleText.text = ("Screw This " + currentItem.displayName + "!").ToUpper();
        roundText.text = "Round " + currentRound;
    }

    private void TerminateGame()
    {
        Debug.Log("You fucked up!");
        if(currentGameItem.GetComponentInChildren<CameraFeed>() != null)
        {
            currentGameItem.GetComponentInChildren<CameraFeed>().Stop();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ScreenSwiped(Vector2 direction)
    {
        if(gameState != GameState.Ending)
        {
            return;
        }
        if (swipeDirection == Vector2.zero)
        {
            swipeDirection = direction;
        }
    }

    public void ScreenTapped()
    {
        if (gameState == GameState.Playing)
        {
            currentTaps++;
            PlaySoundEffect(tapSource, currentTapSounds);
            UpdateTapSlider();
            if((float)currentTaps / currentTapGoal > 0.1f) StartCoroutine(ShakeGameObject(Camera.main.gameObject, shakeDuration, ScaledShakeMagnitude()));
            if (TapGoalReached())
            {
                StartCoroutine(TerminateRound());
            }
            else if(inputManager.randomTapRangeEnabled)
            {
                inputManager.RandomTapRange();
            }
        }
        else if(gameState == GameState.Idle)
        {
            StartCoroutine(StartRound());
        }
    }

    //Core Coroutines
    private IEnumerator StartRound()
    {
        gameState = GameState.Starting;
        ResetGameUI();
        PlaySoundEffect(tapSource, transitionPlaySound);
        StartCoroutine(TransitionGameView(titlePanel, gamePanel));
        while (isTransitioning)
        {
            yield return null;
        }
        Time.timeScale = countdownTimeScale;
        float workingSeconds = secondsBeforeRound;
        int fontCounting = secondsBeforeRound;
        float origonalFontSize = countdownText.fontSize;
        PlaySoundEffect(tapSource, countdownSounds);
        while (workingSeconds > 0)
        {
            if (workingSeconds < fontCounting - 1)
            {
                countdownText.fontSize = origonalFontSize;
                fontCounting--;
                PlaySoundEffect(tapSource, countdownSounds);
            }
            workingSeconds -= Time.deltaTime;
            countdownText.text = ((int)workingSeconds + 1).ToString();
            countdownText.fontSize--;
            yield return new WaitForEndOfFrame();
        }
        countdownText.text = "";
        countdownText.fontSize = origonalFontSize;
        Time.timeScale = 1;
        Handheld.Vibrate();
        if(inputManager.randomTapRangeEnabled) inputManager.RandomTapRange();
        StartCoroutine(timerEnumerator);
        gameState = GameState.Playing;
    }

    private IEnumerator TerminateRound()
    {
        gameState = GameState.Ending;
        if (currentRound == maxRound)
        {
            TerminateGame();
            yield return null;
        }
        inputManager.MaxOutTapRange();
        StopCoroutine(timerEnumerator);
        ToggleGameObject(swipePanel);
        yield return new WaitForSeconds(0.25f);
        swipeDirection = Vector2.zero;
        float swipeTime = swipeWaitDuration;
        while((swipeDirection == Vector2.zero && swipeTime > 0) && !debugEnabled)
        {
            swipeTime -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        if(swipeDirection == Vector2.zero)
        {
            int rand = Random.Range(1, 5);
            switch (rand)
            {
                case 1:
                    swipeDirection = Vector2.up;
                    break;
                case 2:
                    swipeDirection = Vector2.down;
                    break;
                case 3:
                    swipeDirection = Vector2.right;
                    break;
                case 4:
                    swipeDirection = Vector2.left;
                    break;
            }
        }
        BlowUpRigidbody(currentGameItem.GetComponentInChildren<Rigidbody>(), swipeDirection);
        PlaySoundEffect(endSource, currentEndSounds);
        ToggleGameObject(swipePanel);
        float time = explosionDuration;
        while (time > 0 || endSource.isPlaying)
        {
            time -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        ToggleGameObject(currentGameItem);
        currentIndex++;
        SpawnItem(currentIndex);
        PlaySoundEffect(tapSource, transitionEndSound);
        StartCoroutine(TransitionGameView(gamePanel, titlePanel));
        while (isTransitioning)
        {
            yield return null;
        }
        ToggleGameObject(currentGameItem);
        PlaySoundEffect(introSource, currentIntroSounds);
        gameState = GameState.Idle;
    }

    private IEnumerator Timer()
    {
        while (timeRadial.fillAmount > 0)
        {
            timeRadial.fillAmount = Mathf.Clamp(timeRadial.fillAmount - (Time.deltaTime * 1 / roundTime), 0, 1);
            yield return new WaitForEndOfFrame();
        }

        if (gameState == GameState.Playing)
        {
            TerminateGame();
        }
    }

    //UI Functions
    private void InitializeUI()
    {
        titlePanel.alpha = 1;
        gamePanel.alpha = 0;
        countdownText.text = "";
        swipePanel.SetActive(false);
    }
    
    private void ResetGameUI()
    {
        tapSlider.value = 0;
        tapSlider.maxValue = currentTapGoal;
        timeRadial.fillAmount = 1;
    }

    private void UpdateTapSlider()
    {
        tapSlider.value++;
    }

    //UI Coroutines
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

    //Utility Functions
    private List<ItemData> SortListByRound(List<ItemData> list)
    {
        list.Sort(delegate (ItemData a, ItemData b) { return a.roundNumber.CompareTo(b.roundNumber); });
        return list;
    }

    private void BlowUpRigidbody(Rigidbody body, Vector2 direction)
    {
        //body.useGravity = true;
        body.AddForce(direction * explosionForce);
        body.AddTorque(new Vector3(Random.Range(-1, 2), Random.Range(-1, 2), Random.Range(-1, 2)) * explosionForce);
        swipeDirection = Vector2.zero;
    }

    private void PlaySoundEffect(AudioSource source, AudioClip[] sounds)
    {
        if(sounds.Length < 1)
        {
            return;
        }
        source.PlayOneShot(sounds[Random.Range(0, sounds.Length)]);
    }

    private void PlaySoundEffect(AudioSource source, AudioClip sound)
    {
        if (sound == null)
        {
            return;
        }
        source.PlayOneShot(sound);
    }

    private float ScaledShakeMagnitude()
    {
        float scaledMagnitude = (float)currentTaps/currentTapGoal;
        scaledMagnitude = Mathf.Clamp(scaledMagnitude, shakeMinMagnitude, shakeMaxMagnitude);
        return scaledMagnitude;
    }

    private bool TapGoalReached()
    {
        return (currentTaps >= currentTapGoal);
    }

    private void ToggleGameObject(GameObject gameItem)
    {
        gameItem.SetActive(!gameItem.activeSelf);
    }

    //Utility Coroutines
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
}
