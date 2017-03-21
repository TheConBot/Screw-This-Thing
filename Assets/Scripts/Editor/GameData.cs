using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class GameData : ScriptableObject {

    [SerializeField] private bool enableDebugMode;

    public List<ItemData> itemList = new List<ItemData>();

}
