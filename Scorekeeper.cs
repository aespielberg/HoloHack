using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scorekeeper : Singleton<Scorekeeper> {

    private int timeRemaining = 300;
    private int lifeRemaining = 10;

    public int TimeRemaining { get { return timeRemaining; } }

	// Use this for initialization
	void Start () {
        InvokeRepeating("DecrementTimer", 0, 1.0f);
    }
	
	// Update is called once per frame
	void Update () {
		if (timeRemaining <=0 || lifeRemaining <= 0)
        {
            //TODO: load game over screen
        }
	}

    void DecrementTimer()
    {
        timeRemaining--;
    }

    public void Demerit()
    {
        lifeRemaining--;
    }
}
