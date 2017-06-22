using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scorekeeper : Singleton<Scorekeeper> {

    private int timeRemaining = 300;

    public int TimeRemaining { get { return timeRemaining; } }

	// Use this for initialization
	void Start () {
        InvokeRepeating("DecrementTimer", 0, 1.0f);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void DecrementTimer()
    {
        timeRemaining--;
    }
}
