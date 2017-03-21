using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager> {

    public static bool DebugEnabled;

    [SerializeField] private ScriptableObject GameData;


}
