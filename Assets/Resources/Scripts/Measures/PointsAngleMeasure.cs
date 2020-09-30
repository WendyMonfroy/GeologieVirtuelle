using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsAngleMeasure : MonoBehaviour
{
    public Camera cam;
    public GameObject terrain;
    public string terrainName;

    private int clickCount;
    private PointsAngleClass angleInstance;
    private List<PointsAngleClass> instanceList;

    private Transform cloneParent;

    private GameObject refPoint;
    private GameObject clonePoint;

    private GameObject refLine;
    private GameObject angleLine;
    private Vector3[] anglePositions;

    // Start is called before the first frame update
    void Start()
    {
        refPoint = transform.GetChild(0).gameObject;
        refLine = transform.GetChild(1).gameObject;
        cloneParent = transform.GetChild(2);

        anglePositions = new Vector3[3];
        instanceList = new List<PointsAngleClass>();

        // event initialisation
        EventManager.StartListening("PlaceAnglePointClickEvent", PlacePointClickRecieved);
        EventManager.StartListening("DisplayResults_Point Angle", CalculateAndDisplay);

        EventManager.StartListening("StartUpdatingLines", StartUpdateCoroutine);
        EventManager.StartListening("StopUpdatingLines", StopUpdateCoroutine);

        EventManager.StartListening("StartPointAngleSave", StartSaving);
        EventManager.StartListening("RestorePointsAngle", Restore);

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
            if (cloneParent.GetChild(i).name.Contains("Sphere"))
                cloneParent.GetChild(i).localScale = Vector3.one * 0.1f;
            else
                cloneParent.GetChild(i).GetComponent<LineRenderer>().widthMultiplier = 0.03f;
        }

        cloneParent.gameObject.SetActive(false);
    }

    // process click event
    private void PlacePointClickRecieved(EventParam positionParam)
    {
        if (clickCount == 0)
        {
            angleInstance = new PointsAngleClass();
            instanceList.Add(angleInstance);
        }

        if (clickCount < 3)
        {
            clonePoint = Instantiate(refPoint, cloneParent);
            clonePoint.SetActive(true);
            clonePoint.transform.position = positionParam.getPointParam();

            angleInstance.AddElement(clonePoint);

            anglePositions[clickCount] = clonePoint.transform.localPosition;

            clickCount += 1;
        }

        if (clickCount == 3)
        {
            angleLine = Instantiate(refLine, cloneParent);
            angleLine.SetActive(true);
            angleLine.name = "newLine";
            angleLine.GetComponent<LineRenderer>().positionCount = 3;
            angleLine.GetComponent<LineRenderer>().SetPositions(anglePositions);

            angleInstance.AddLine(angleLine);
        }
    }

    // calculate and display measure result
    private void CalculateAndDisplay(EventParam param)
    {
        if (angleInstance != null)
        {
            angleInstance.Calculate();
            angleInstance.Display();

            clickCount = 0;
        }
    }

    void StartUpdateCoroutine(EventParam param)
    {
        StartCoroutine("UpdateLinePositionsCoroutine");
    }

    void StopUpdateCoroutine(EventParam param)
    {
        StopCoroutine("UpdateLinePositionsCoroutine");
    }

    IEnumerator UpdateLinePositionsCoroutine()
    {
        for (; ; )
        {
            for (int i = 0; i < instanceList.Count; i++)
            {
                instanceList[i].UptadeLinePosition();
            }

            yield return new WaitForSeconds(0.1f);
        }
    }


    // save measure methods
    private void StartSaving(EventParam param)
    {
        StartCoroutine("Save");
    }

    IEnumerator Save()
    {
        for (int i = 0; i < instanceList.Count; i++)
        {
            EventManager.TriggerEvent("Save_PointsAngle", new EventParam(instanceList[i].SaveInstance()));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }

        EventManager.TriggerEvent("StartPlaneAngleSave", null);
    }

    // restore saved measures method
    private void Restore(EventParam jsonStringParam)
    {
        // create an new measure instance from the JSON string
        angleInstance = JsonUtility.FromJson<PointsAngleClass>(jsonStringParam.getStringParam());
        instanceList.Add(angleInstance);

        // instantiate enough point to render the measure
        List<GameObject> pointsList = new List<GameObject>();
        for (int i = 0; i < 3; i++)
        {
            clonePoint = Instantiate(refPoint, cloneParent);
            pointsList.Add(clonePoint);
        }
        // instantiate a new line for the angle measure
        angleLine = Instantiate(refLine, cloneParent);

        // set the non serializable fields of the measure instance
        angleInstance.RestoreInstance(pointsList, angleLine);
    }
}

// Point angle class 
public class PointsAngleClass : IMeasureData<GameObject>, IMeasureManager
{
    private static int angleNumber = 0;

    [SerializeField] private int id;
    private List<GameObject> points;
    private GameObject line;

    [SerializeField] private List<Vector3> pointsPosition;

    [SerializeField] private float angle;

    // constructor
    public PointsAngleClass()
    {
        id = angleNumber;
        angleNumber += 1;
        points = new List<GameObject>();
        pointsPosition = new List<Vector3>();
    }

    // add new elements to the measure instance
    public void AddElement(GameObject newPoint)
    {
        if (points.Count < 3)
        {
            newPoint.name = "Sphere_" + id.ToString();
            points.Add(newPoint);
            pointsPosition.Add(newPoint.transform.localPosition);
        }
    }

    public void AddLine(GameObject newLine)
    {
        line = newLine;
    }

    // update instance when the gameObjects are modified
    public void UptadeLinePosition()
    {
        if (line != null)
        {
            line.GetComponent<LineRenderer>().SetPosition(0, points[0].transform.position);
            line.GetComponent<LineRenderer>().SetPosition(1, points[1].transform.position);
            line.GetComponent<LineRenderer>().SetPosition(2, points[2].transform.position);
        }
    }

    public void Calculate()
    {
        // generate 2 vectors
        Vector3 vect1 = points[1].transform.localPosition - points[0].transform.localPosition;
        Vector3 vect2 = points[1].transform.localPosition - points[2].transform.localPosition;

		// calculate angle between the 2 vectors
		angle = Vector3.Angle(vect1, vect2);
    }

    public void Display()
    {
        EventManager.TriggerEvent("DisplayResult", new EventParam("Angle: "+angle.ToString()+"°"));

        if (points.Count == 0)
            EventManager.TriggerEvent("ErrorMessage", new EventParam("Placez les trois points avant de demander l'affichage de la mesure d'angle."));
    }

    // saving method
    public string SaveInstance()
    {
        pointsPosition[0] = points[0].transform.localPosition;
        pointsPosition[1] = points[1].transform.localPosition;
        pointsPosition[2] = points[2].transform.localPosition;

        return JsonUtility.ToJson(this);
    }

    // restoring method
    public void RestoreInstance(List<GameObject> newPoint, GameObject newLine)
    {
        line = newLine;
        points = newPoint;
        for (int i = 0; i < 3; i++)
        {
            points[i].SetActive(true);
            points[i].name = "Sphere_" + id.ToString();
            points[i].transform.localPosition = pointsPosition[i];
        }
        line.SetActive(true);
        line.name = "Line_" + id.ToString();
        line.GetComponent<LineRenderer>().positionCount = pointsPosition.Count;
        line.GetComponent<LineRenderer>().SetPositions(pointsPosition.ToArray());
    }
}