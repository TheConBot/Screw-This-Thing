using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ItemManager : MonoBehaviour {

    public int taps;
    private int tapLimit = 50;
    private Rigidbody body;

    private GameInput gameInput;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        gameInput = Camera.main.GetComponent<GameInput>();
    }

	private void Update()
    {
        taps = gameInput.taps;
        if(taps >= tapLimit)
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
