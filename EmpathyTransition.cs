using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class EmpathyTransition : MonoBehaviour
{

    public Button empathy;

    // Use this for initialization
    void Start()
    {
        empathy = gameObject.GetComponent<Button>();
        empathy.onClick.AddListener(EmpathyTransitionClick);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void EmpathyTransitionClick()
    {
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }
}