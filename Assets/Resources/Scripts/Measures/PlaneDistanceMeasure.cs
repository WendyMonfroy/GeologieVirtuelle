using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneDistanceMeasure : MonoBehaviour
{
    public Camera cam;
    public GameObject terrain;
    public string terrainName;

    private int clickCount;
    private PlaneDistanceClass planeDistanceInstance;
    private List<PlaneDistanceClass> instanceList;

    private Transform cloneParent;

    private GameObject refPlane;
    private GameObject clonePlane;
    private GameObject refLine;
    private GameObject normalLine;

    // Start is called before the first frame update
    void Start()
    {
        refPlane = transform.GetChild(0).gameObject;
        refLine = transform.GetChild(1).gameObject;
        cloneParent = transform.GetChild(2);

        instanceList = new List<PlaneDistanceClass>();

        // event initialisation
        EventManager.StartListening("PlaceDistancePlaneClickEvent", PlacePlaneClickRecieved);
        EventManager.StartListening("DisplayResults_Plane Distance", CalculateAndDisplay);

        EventManager.StartListening("StartPlaneDistSave", StartSaving);
        EventManager.StartListening("RestorePlaneDistance", Restore);

        EventManager.StartListening("StartSimulation", StartSimulation);
        EventManager.StartListening("BackToLobby", BackToLobby);
    }

    void StartSimulation(EventParam param)
    {

        // save the instantiated elements as terrain child
        cloneParent.SetParent(terrain.transform);
        cloneParent.gameObject.SetActive(true);

        if (terrainName == null || terrainName != param.getGameObjectParam().name)
        {
            // this is a new terrain
            terrainName = param.getGameObjectParam().name;

            // destroy the previously instantiated elements if they exist
            for (int i = 0; i < cloneParent.childCount; i++)
            {
                Destroy(cloneParent.GetChild(i).gameObject);
            }
            instanceList.Clear();
        }

        clickCount = 0;
    }

    void BackToLobby(EventParam param)
    {
        clickCount = 0;

        // get back the instanciated elements
        cloneParent.SetParent(transform);
        cloneParent.localScale = Vector3.one;
        for (int i = 0; i < cloneParent.childCount; i++)
        {
            cloneParent.GetChild(i).localScale = Vector3.one;
        }

        cloneParent.gameObject.SetActive(false);
    }

    // process click event
    public void PlacePlaneClickRecieved(EventParam positionParam)
    {
        if (clickCount == 0)
        {
            planeDistanceInstance = new PlaneDistanceClass();
            instanceList.Add(planeDistanceInstance);
        }

        if (clickCount < 2)
        {
            clonePlane = Instantiate(refPlane, cloneParent);
            clonePlane.SetActive(true);
            clonePlane.transform.position = positionParam.getPointParam();

            if (clickCount == 1)
            {
                normalLine = Instantiate(refLine, clonePlane.transform);
                normalLine.SetActive(true);
                normalLine.transform.localPosition = Vector3.zero;
                normalLine.transform.eulerAngles = Vector3.zero;
                normalLine.name = "Arrow";
            }

            planeDistanceInstance.AddElement(clonePlane);
        }

        clickCount += 1;
    }

    // calculate and display measure result
    public void CalculateAndDisplay(EventParam param)
    {
        if (planeDistanceInstance != null)
        {
            planeDistanceInstance.Calculate();
            planeDistanceInstance.Display();
            if (clickCount == 2)
                clickCount = 0;
        }
    }


    // save methods
    private void StartSaving(EventParam param)
    {
        StartCoroutine("Save");
    }

    IEnumerator Save()
    {
        for (int i = 0; i < instanceList.Count; i++)
        {
            EventManager.TriggerEvent("Save_PlaneDistance", new EventParam(instanceList[i].SaveInstance()));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }

        EventManager.TriggerEvent("StartPointAngleSave", null);
    }

    // restore saved measures method
    private void Restore(EventParam jsonStringParam)
    {
        // create an new measure instance from the JSON string
        planeDistanceInstance = JsonUtility.FromJson<PlaneDistanceClass>(jsonStringParam.getStringParam());
        instanceList.Add(planeDistanceInstance);

        // instantiate 2 planes
        GameObject p1 = Instantiate(refPlane, cloneParent);
        p1.SetActive(true);

        normalLine = Instantiate(refLine, p1.transform);
        normalLine.SetActive(true);
        normalLine.transform.localPosition = Vector3.zero;
        normalLine.transform.eulerAngles = Vector3.zero;

        GameObject p2 = Instantiate(refPlane, cloneParent);
        p2.SetActive(true);

        normalLine = Instantiate(refLine, p2.transform);
        normalLine.SetActive(true);
        normalLine.transform.localPosition = Vector3.zero;
        normalLine.transform.eulerAngles = Vector3.zero;

        // set the non serializable fields of the measure instance
        planeDistanceInstance.RestoreInstance(p1, p2);
    }
}

// Plan distance class
public class PlaneDistanceClass : IMeasureData<GameObject>, IMeasureManager
{
    static int elementNumber = 0;

    [SerializeField] private int id;
    private GameObject planeA;
    private GameObject planeB;

    [SerializeField] private Vector3 planeAPosition;
    [SerializeField] private Quaternion planeARotation;
    [SerializeField] private Vector3 planeBPosition;
    [SerializeField] private Quaternion planeBRotation;

    [SerializeField] private float distance;

    public PlaneDistanceClass()
    {
        id = elementNumber;
        elementNumber += 1;
    }

    // add new elements to the measure instance
    public void AddElement(GameObject elt)
    {
        if (planeA == null)
        {
            planeA = elt;
            planeA.name = "PlaneA_" + id.ToString();
        }
        else
        {
            planeB = elt;
            planeB.name = "PlaneB_" + id.ToString();
            planeB.transform.localPosition = planeA.transform.localPosition + new Vector3(0f, 0.2f, 0f);
            planeB.transform.localRotation = planeA.transform.localRotation;
            planeB.gameObject.tag = "VerticalyMovable";

            // corect the arrow position and rotation
            planeB.transform.GetChild(0).localPosition = Vector3.zero;
            planeB.transform.GetChild(0).localRotation = new Quaternion(0,0,0,0);

            // deactivate rotation for plane A
            planeA.tag = "VerticalyMovable";
        }
    }

    public void Calculate()
    {
        if (planeB != null)
        {
            distance = Mathf.Sqrt((planeB.transform.localPosition.x - planeA.transform.localPosition.x) * (planeB.transform.localPosition.x - planeA.transform.localPosition.x)
                + (planeB.transform.localPosition.y - planeA.transform.localPosition.y) * (planeB.transform.localPosition.y - planeA.transform.localPosition.y)
                + (planeB.transform.localPosition.z - planeA.transform.localPosition.z) * (planeB.transform.localPosition.z - planeA.transform.localPosition.z));
        }
        else
        {
            distance = -1;
        }
    }

    // method to update line 
    public void UptadeLinePosition(GameObject line)
    {
        line.GetComponent<LineRenderer>().SetPosition(0, planeA.transform.position);
        line.GetComponent<LineRenderer>().SetPosition(1, planeB.transform.position);

        planeAPosition = planeA.transform.localPosition;
        planeBPosition = planeB.transform.localPosition;
    }

    public void Display()
    {
        EventManager.TriggerEvent("DisplayResult", new EventParam("Distance normale: " + distance.ToString() + "m"));

        if (planeB == null)
            EventManager.TriggerEvent("ErrorMessage", new EventParam("Placez les deux plans avant de demander l'affichage de la mesure de distance."));
    }

    // saving method
    public string SaveInstance()
    {
        planeAPosition = planeA.transform.localPosition;
        planeARotation = planeA.transform.localRotation;

        planeBPosition = planeB.transform.localPosition;
        planeBRotation = planeB.transform.localRotation;

        return JsonUtility.ToJson(this);
    }

    // restoring method
    public void RestoreInstance(GameObject p1, GameObject p2)
    {
        planeA = p1;
        planeA.name = "PlaneA_" + id.ToString();
        planeA.transform.localPosition = planeAPosition;
        planeA.transform.localRotation = planeARotation;
        planeA.SetActive(true);

        planeB = p2;
        planeB.name = "PlaneB_" + id.ToString();
        planeB.transform.localPosition = planeBPosition;
        planeB.transform.localRotation = planeBRotation;
        planeB.SetActive(true);
    }
}
