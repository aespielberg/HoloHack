using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionModeManager : Singleton<InteractionModeManager> {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//check if we're gazing an object
        //if we're gazing an object, disable forcefieldcreator
        //otherwise, enable forcefieldcrerator

        if (GazeManager.Instance.IsGazingAtObject)
        {
            GameObject gazedObject = GazeManager.Instance.HitObject;
            if (gazedObject.layer == 8) //forced to be cylinders
            {
                ForceFieldCreator.Instance.Enable(false);
            }  
        }else
        {
            ForceFieldCreator.Instance.Enable(true);
        }

	}
}
