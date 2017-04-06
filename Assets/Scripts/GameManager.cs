using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(InputManager))]
public class GameManager : SingletonMonoBehaviour<GameManager>
{

    [SerializeField]
    private GameData data;
    private List<ItemData> items = new List<ItemData>();
    private List<GameObject> gameItems = new List<GameObject>();

    public static bool DebugEnabled;
    public struct GameInformation
    {
        public float roundTime;
        public int maxRound;
        public int minRound
        {
            get
            {
                return 1;
            }
        }
        private int _currentRound;
        public int currentRound
        {
            get
            {
                return _currentRound;
            }
            set
            {
                _currentRound = Mathf.Clamp(_currentRound, minRound, maxRound);
            }
        }
        public int currentIndex;
        public int tapsThisRound;
        public int tapGoal;
        public ItemData currentItem;

    }
    public GameInformation status;

    private void Start()
    {
        StartGame();
    }

    private List<ItemData> SortListByRound(List<ItemData> list)
    {
        list.Sort(delegate (ItemData a, ItemData b) { return a.roundNumber.CompareTo(b.roundNumber); });
        return list;
    }

    private void StartGame()
    {
        items = data.itemList;
        items = SortListByRound(items);
        status.maxRound = items.Count;
        status.currentIndex = 0;
        InstantiateItems();
        SpawnNewItem();
    }

    private void EndRound()
    {
        ToggleGameItem();
        status.currentIndex++;
        SpawnNewItem();
    }

    private void InstantiateItems()
    {
        foreach (var item in items)
        {
            GameObject gameItem = Instantiate(item.itemPrefab);
            gameItems.Add(gameItem);
            gameItem.SetActive(false);
        }
    }

    private void SpawnNewItem()
    {
        status.currentItem = items[status.currentIndex];
        status.currentRound = status.currentItem.roundNumber;
        status.roundTime = status.currentItem.roundTime;
        status.tapGoal = status.currentItem.tapGoal;
        status.tapsThisRound = 0;
        ToggleGameItem();
        gameItems[status.currentIndex].name = status.currentItem.displayName;
    }

    public void ScreenTapped()
    {
        status.tapsThisRound++;
        Debug.Log("Tap Status: " + status.tapsThisRound + "/" + status.tapGoal);
        //Camera shake and other effects here
        if(TapGoalReached())
        {
            EndRound();
        }
    }

    private bool TapGoalReached()
    {
        if (status.tapsThisRound >= status.tapGoal)
        {
            return true;
        }
        return false;
    }

    private void ToggleGameItem()
    {
        gameItems[status.currentIndex].SetActive(!gameItems[status.currentIndex].activeSelf);
    }
}
