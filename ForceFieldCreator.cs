using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HoloToolkit.Unity;
using System.Linq;

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
    public int maxNumForceFields = 100;
    public int forceFieldLifeSpan = 30;
    public float totalForceFieldArea = 10f;
    private const float startArea = 0.25f * (float)Math.PI;

    //TODO: re-org into start paint and end paint functions?

    private List<GameObject> subMeshes; //TODO: How do I clear this?  Will have to add this in the future
    private Material material;

    public float CurrentTotalForceFieldArea
    {
        get
        {
            float totalArea = 0f;
            foreach (GameObject forceField in subMeshes)
            {
                totalArea += this.getForceFieldArea(forceField);
            }
            return totalArea;

        }


    }

    public float StartArea
    {
        get { return startArea; }
    }





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

        //get total force field area
        float totalArea = CurrentTotalForceFieldArea;
        

        //now spawn a point in that area:
        if (this.totalNumForceFields < this.maxNumForceFields && totalArea + startArea <= totalForceFieldArea) //make sure we can add another force field and it doesn't go over the limit
        {

            GameObject meshGameObject = (GameObject)Instantiate(cylinderPrefab);
            HandDraggableAndScalable handDraggable = meshGameObject.AddComponent<HandDraggableAndScalable>(); //make it draggable
            handDraggable.IsDraggingAndScalingEnabled = true;
            this.totalNumForceFields++;
            Destroy(meshGameObject, forceFieldLifeSpan);
            /*
            MeshFilter subMeshFilter = meshGameObject.AddComponent<MeshFilter>();
            subMeshFilter.transform.position = handPosition;
            subMeshFilter.mesh = mesh;
            MeshRenderer subMeshRenderer = meshGameObject.AddComponent<MeshRenderer>();
            subMeshRenderer.materials[0] = material;
            */

            meshGameObject.transform.localScale = new Vector3(0.5f, 0.01f, 0.5f);
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
        //cleanup all destroyed objects
        subMeshes = subMeshes.Where(x => x != null).ToList();

        //TODO: get all areas?

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

    private float getForceFieldArea(GameObject gameObject)
    {
        return (float)(Math.Pow(gameObject.transform.localScale.x, 2) * Math.PI);
    }

}
