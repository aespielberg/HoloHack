using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HoloToolkit.Unity;



public class ForceFieldCreator : Singleton<ForceFieldCreator>, IInputHandler, ISourceStateHandler
{
    private int totalNumForceFields;


    private IInputSource currentInputSource;

    internal void DecrementForceFieldCount()
    {
        this.totalNumForceFields--;
    }

    private uint currentInputSourceId;
    public GameObject cylinderPrefab;
    public int maxNumForceFields = 5;
    public int forceFieldLifeSpan = 10;

    //TODO: re-org into start paint and end paint functions?

    private List<GameObject> subMeshes; //TODO: How do I clear this?  Will have to add this in the future
    private Material material;







    public void OnInputDown(InputEventData eventData)
    {

        if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
        {
            // The input source must provide positional data for this script to be usable
            return;
        }
        Debug.Log("Start hold");

        currentInputSource = eventData.InputSource;
        currentInputSourceId = eventData.SourceId;

        //TODO: modularize into a util function?
        Vector3 handPosition;
        currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);

        handPosition += Camera.main.transform.forward * 0.5f;



        //now spawn a point in that area:
        if (this.totalNumForceFields < this.maxNumForceFields)
        {
            GameObject meshGameObject = (GameObject)Instantiate(cylinderPrefab);
            HandDraggable handDraggable = meshGameObject.AddComponent<HandDraggable>(); //make it draggable
            handDraggable.IsDraggingEnabled = true;
            this.totalNumForceFields++;
            Destroy(meshGameObject, forceFieldLifeSpan);
            /*
            MeshFilter subMeshFilter = meshGameObject.AddComponent<MeshFilter>();
            subMeshFilter.transform.position = handPosition;
            subMeshFilter.mesh = mesh;
            MeshRenderer subMeshRenderer = meshGameObject.AddComponent<MeshRenderer>();
            subMeshRenderer.materials[0] = material;
            */

            meshGameObject.transform.localScale = new Vector3(0.1f, 0.01f, 0.1f);
            meshGameObject.transform.localPosition = handPosition;


            subMeshes.Add(meshGameObject);
        }
    }

    public void OnInputUp(InputEventData eventData)
    {

        currentInputSource = null;
        currentInputSourceId = 0;
    }

    public void OnSourceDetected(SourceStateEventData eventData)
    {
        //throw new NotImplementedException();
    }

    public void OnSourceLost(SourceStateEventData eventData)
    {
        //throw new NotImplementedException();
    }

    // Use this for initialization
    void Start()
    {

        subMeshes = new List<GameObject>();
        material = Resources.Load("Materials/DefaultMaterial", typeof(Material)) as Material;
        this.Enable(true);
    }

    // Update is called once per frame
    void Update()
    {


    }

    public void Enable(bool enabled)
    {
        if (enabled)
        {
            InputManager.Instance.PushModalInputHandler(gameObject);
            this.enabled = true;
        }
        else if (this.enabled)
        {
            InputManager.Instance.PopModalInputHandler();
            this.enabled = false;
        }
    }

    public ForceFieldCreator()
    {
        currentInputSource = null;
        currentInputSourceId = 0;
        this.totalNumForceFields = 0;
    }

}
