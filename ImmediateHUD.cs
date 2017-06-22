using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmediateHUD : Singleton<ImmediateHUD> {

    public GUIStyle makeItPretty;

    void OnGUI()
    {
        // Make a background box
        GUI.Label(new Rect(10, 10, 200, 90), "Time Remaining: " + Scorekeeper.Instance.TimeRemaining, makeItPretty);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
