using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ForceFieldTransition : MonoBehaviour
{

    public Button forceField;

    // Use this for initialization
    void Start()
    {
        forceField = gameObject.GetComponent<Button>();
        forceField.onClick.AddListener(ForceFieldTransitionClick);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void ForceFieldTransitionClick()
    {
        SceneManager.LoadScene("BallDemoScene", LoadSceneMode.Single);
    }
}