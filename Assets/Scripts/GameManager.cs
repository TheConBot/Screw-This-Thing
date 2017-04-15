using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(InputManager))]
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    //Private Vars
    [SerializeField]
    private GameData data;
    private GameObject currentGameItem;
    private IEnumerator timerEnumerator;
    private ItemData currentItem;
    private List<GameObject> gameItems = new List<GameObject>();
    private List<ItemData> items = new List<ItemData>();
    private bool isPlaying;
    private bool isTransitioning;
    private const int minRound = 1;
    private float roundTime;
    private int currentIndex;
    private int currentRound;
    private int maxRound;
    private int tapsThisRound;
    private int tapGoal;
    //Public Static Vars
    public static bool DebugEnabled;
    //Public Inspector Vars
    [Header("UI References")]
    public CanvasGroup gamePanel;
    public CanvasGroup titlePanel;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI tapText;
    [Header("GameItem Shake Options")]
    public float shakeDuration = .15f;
    public float shakeMinMagnitude = .1f;
    public float shakeMaxMagnitude = 1;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        InstantiateItems();
        titlePanel.alpha = 1;
        gamePanel.alpha = 0;
        SpawnNewItem();
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
        Handheld.Vibrate();
        isPlaying = true;
        timerEnumerator = Timer();
        StartCoroutine(timerEnumerator);
    }

    private IEnumerator EndRound()
    {
        Handheld.Vibrate();
        StopCoroutine(timerEnumerator);
        StartCoroutine(TransitionGameView(gamePanel, titlePanel));
        while (isTransitioning)
        {
            yield return null;
        }
        isPlaying = false;
        ToggleGameItem(currentGameItem);
        currentIndex++;
        SpawnNewItem();
    }

    private void InstantiateItems()
    {
        items = data.itemList;
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
        ToggleGameItem(currentGameItem);

        titleText.text = ("Screw This " + currentItem.displayName + "!").ToUpper();
        roundText.text = "Round " + currentRound;
        timeText.text = roundTime.ToString("F2");
        tapText.text = tapsThisRound + "/" + tapGoal;
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
        if (isTransitioning)
        {
            return;
        }
        if (isPlaying)
        {
            tapsThisRound++;
            tapText.text = tapsThisRound + "/" + tapGoal;
            Debug.Log(ScaledShakeMagnitude());
            //StartCoroutine(ShakeGameObject(currentGameItem, shakeDuration, ScaledShakeMagnitude()));
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
            // map value to [-1, 1]
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
            fadeIn.alpha += Time.deltaTime;
            fadeOut.alpha -= Time.deltaTime;
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
