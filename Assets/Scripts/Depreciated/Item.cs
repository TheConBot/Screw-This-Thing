using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour {

    public enum SoundType
    {
        Organic,
        Metal
    }

    public int tapCount { private get; set; }
    public SoundType soundType;
    private int tapLimit;
    private Rigidbody body;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
    }

	private void Update()
    {
        //taps = gameInput.taps;
        if(tapCount >= tapLimit)
        {
            Boom();
        }
    }

    private void Boom()
    {
        body.isKinematic = false;
        body.AddExplosionForce(100f, transform.position * 2, 100);
    }
}
