using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scorekeeper : Singleton<Scorekeeper> {

    private int timeRemaining = 60;
    public int lifeRemaining = 3;

    public int TimeRemaining { get { return timeRemaining; } }

	// Use this for initialization
	void Start () {
        InvokeRepeating("DecrementTimer", 0, 1.0f);
    }
	
	// Update is called once per frame
	void Update () {
		if (lifeRemaining <= 0)
        {
            SceneManager.LoadScene("gameover", LoadSceneMode.Single);
        }else if (timeRemaining <= 0)
        {
            SceneManager.LoadScene("winnerv3", LoadSceneMode.Single);
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
