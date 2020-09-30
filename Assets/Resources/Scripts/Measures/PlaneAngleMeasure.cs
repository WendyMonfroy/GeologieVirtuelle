using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneAngleMeasure : MonoBehaviour
{
    public Camera cam;
    public GameObject terrain;
    public string terrainName;

    private int clickCount;
    private PlaneAngleClass planeAngleInstance;
    private List<PlaneAngleClass> instanceList;

    private Transform cloneParent;

    private GameObject refPlane;
    private GameObject clonePlane;

    private GameObject refNormal;
    private GameObject cloneNormal;

    // Start is called before the first frame update
    void Start()
    {
        refPlane = transform.GetChild(0).gameObject;
        refNormal = transform.GetChild(1).gameObject;
        cloneParent = transform.GetChild(2);

        instanceList = new List<PlaneAngleClass>();

        // event initialisation
        EventManager.StartListening("PlaceAnglePlaneClickEvent", PlacePlaneClickRecieved);
        EventManager.StartListening("DisplayResults_Plane Angle", CalculateAndDisplay);

        EventManager.StartListening("StartPlaneAngleSave", StartSaving);
        EventManager.StartListening("RestorePlaneAngle", Restore);

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
            planeAngleInstance = new PlaneAngleClass();
            instanceList.Add(planeAngleInstance);
        }

        if (clickCount < 2)
        {
            clonePlane = Instantiate(refPlane, cloneParent);
            clonePlane.SetActive(true);
            clonePlane.transform.position = positionParam.getPointParam();

            cloneNormal = Instantiate(refNormal, clonePlane.transform);
            cloneNormal.SetActive(true);
            cloneNormal.transform.localPosition = Vector3.zero;
            cloneNormal.transform.eulerAngles = Vector3.zero;

            planeAngleInstance.AddElement(clonePlane);
        }

        clickCount += 1;
    }

    // calculate and display measure result
    public void CalculateAndDisplay(EventParam param)
    {
        if (planeAngleInstance != null)
        {
            planeAngleInstance.Calculate();
            planeAngleInstance.Display();
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
            EventManager.TriggerEvent("Save_PlaneAngle", new EventParam(instanceList[i].SaveInstance()));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }

        EventManager.TriggerEvent("SavingDone", null);
    }

    // restore saved measures method
    private void Restore(EventParam jsonStringParam)
    {
        // create an new measure instance from the JSON string
        planeAngleInstance = JsonUtility.FromJson<PlaneAngleClass>(jsonStringParam.getStringParam());
        instanceList.Add(planeAngleInstance);

        // instantiate 2 planes
        GameObject p1 = Instantiate(refPlane, cloneParent);
        p1.SetActive(true);

        cloneNormal = Instantiate(refNormal, p1.transform);
        cloneNormal.SetActive(true);
        cloneNormal.transform.localPosition = Vector3.zero;
        cloneNormal.transform.eulerAngles = Vector3.zero;

        GameObject p2 = Instantiate(refPlane, cloneParent);
        p2.SetActive(true);

        cloneNormal = Instantiate(refNormal, p2.transform);
        cloneNormal.SetActive(true);
        cloneNormal.transform.localPosition = Vector3.zero;
        cloneNormal.transform.eulerAngles = Vector3.zero;

        // set the non serializable fields of the measure instance
        planeAngleInstance.RestoreInstance(p1, p2);
    }
}

// Plan distance class
public class PlaneAngleClass : IMeasureData<GameObject>, IMeasureManager
{
    static int elementNumber = 0;

    [SerializeField] private int id;
    private GameObject planeA;
    private GameObject planeB;

    [SerializeField] private Vector3 planeAPosition;
    [SerializeField] private Quaternion planeARotation;
    [SerializeField] private Vector3 planeBPosition;
    [SerializeField] private Quaternion planeBRotation;

    [SerializeField] private float angle;

    public PlaneAngleClass()
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

            // corect the arrow position and rotation
            planeB.transform.GetChild(0).localPosition = Vector3.zero;
            planeB.transform.GetChild(0).localRotation = new Quaternion(0,0,0,0);
        }
    }

    public void Calculate()
    {
        if (planeB != null)
        {
            Vector3 normA = (planeA.transform.GetChild(0).TransformPoint(planeA.transform.GetChild(0).localPosition + Vector3.up)) - planeA.transform.position;
            Vector3 normB = (planeB.transform.GetChild(0).TransformPoint(planeB.transform.GetChild(0).localPosition + Vector3.up)) - planeB.transform.position;

            angle = Vector3.Angle(normA, normB);
        }
        else
        {
            angle = 0;
        }
    }

    public void Display()
    {
        EventManager.TriggerEvent("DisplayResult", new EventParam("Angle: " + angle.ToString() + "°"));

        if (planeB == null)
            EventManager.TriggerEvent("ErrorMessage", new EventParam("Placez les deux plans avant de demander l'affichage de la mesure d'angle."));
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
