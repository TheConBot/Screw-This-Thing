﻿using UnityEngine;

[CreateAssetMenu()]
public class ItemData : ScriptableObject
{
    public int roundNumber;
    public int itemScale;
    public float roundTime;
    public string displayName;
    public int tapGoal;
    public GameObject itemPrefab;
    public AudioClip[] introVOSounds;
    public AudioClip[] tapSounds;
    public AudioClip[] endSounds;
}
