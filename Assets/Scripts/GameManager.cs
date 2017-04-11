using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(InputManager))]
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    //Private Vars
    [SerializeField]
    private GameData data;
    private List<ItemData> items = new List<ItemData>();
    private List<GameObject> gameItems = new List<GameObject>();
    private IEnumerator timerCoroutine;
    //Public Static Vars
    public static bool DebugEnabled;
    //Public Vars
    public struct GameInformation
    {
        public float RoundTime;
        public int MaxRound;
        public int MinRound
        {
            get
            {
                return 1;
            }
        }
        private int currentRound;
        public int CurrentRound
        {
            get
            {
                return currentRound;
            }
            set
            {
                currentRound = Mathf.Clamp(value, MinRound, MaxRound);
            }
        }
        public int CurrentIndex;
        public int TapsThisRound;
        public int TapGoal;
        public ItemData CurrentItem;
        public GameObject CurrentGameItem;
        public bool isPlaying;
        public bool isTransitioning;
    }
    public GameInformation status;
    //Public UI Vars
    public Text titleText;
    public Text roundText;
    public Text timeText;
    public Text tapText;
    public CanvasGroup titlePanel;
    public CanvasGroup gamePanel;

    private void Start()
    {
        StartGame();
    }

    private IEnumerator StartRound()
    {
        StartCoroutine(TransitionGameView(titlePanel, gamePanel));
        while (status.isTransitioning)
        {
            yield return null;
        }
        status.isPlaying = true;
        timerCoroutine = Timer();
        StartCoroutine(timerCoroutine);
    }

    private IEnumerator EndRound()
    {
        StopCoroutine(timerCoroutine);
        StartCoroutine(TransitionGameView(gamePanel, titlePanel));
        while (status.isTransitioning)
        {
            yield return null;
        }
        status.isPlaying = false;
        ToggleGameItem(status.CurrentGameItem);
        status.CurrentIndex++;
        SpawnNewItem();
    }

    private void StartGame()
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

    private void InstantiateItems()
    {
        items = data.itemList;
        items = SortListByRound(items);
        status.MaxRound = items.Count;
        status.CurrentIndex = 0;
        foreach (var item in items)
        {
            GameObject gameItem = Instantiate(item.itemPrefab);
            gameItems.Add(gameItem);
            gameItem.SetActive(false);
        }
    }

    private void SpawnNewItem()
    {
        status.CurrentItem = items[status.CurrentIndex];
        status.CurrentGameItem = gameItems[status.CurrentIndex];
        status.CurrentRound = status.CurrentItem.roundNumber;
        status.RoundTime = status.CurrentItem.roundTime;
        status.TapGoal = status.CurrentItem.tapGoal;
        status.TapsThisRound = 0;
        ToggleGameItem(status.CurrentGameItem);

        titleText.text = ("Screw This " + status.CurrentItem.displayName + "!").ToUpper();
        roundText.text = "Round " + status.CurrentRound;
        timeText.text = status.RoundTime.ToString("F2");
        tapText.text = status.TapsThisRound + "/" + status.TapGoal;
    }

    private IEnumerator Timer()
    {
        while (status.RoundTime > 0)
        {
            status.RoundTime -= Time.deltaTime;
            timeText.text = status.RoundTime.ToString("F2");
            yield return new WaitForEndOfFrame();
        }

        if (!status.isTransitioning && status.isPlaying)
        {
            EndGame();
        }

    }

    public void ScreenTapped()
    {
        if (status.isTransitioning)
        {
            return;
        }
        Handheld.Vibrate();
        if (status.isPlaying)
        {
            status.TapsThisRound++;
            tapText.text = status.TapsThisRound + "/" + status.TapGoal;
            //TODO: Camera shake and other effects here
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

    private bool TapGoalReached()
    {
        if (status.TapsThisRound >= status.TapGoal)
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
        status.isTransitioning = true;
        while (fadeIn.alpha != 1 && fadeOut.alpha != 0)
        {
            fadeIn.alpha += Time.deltaTime;
            fadeOut.alpha -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        status.isTransitioning = false;
    }

    private List<ItemData> SortListByRound(List<ItemData> list)
    {
        list.Sort(delegate (ItemData a, ItemData b) { return a.roundNumber.CompareTo(b.roundNumber); });
        return list;
    }
}
