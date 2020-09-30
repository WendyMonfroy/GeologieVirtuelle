using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PathDistanceMeasure : MonoBehaviour
{
    public Camera cam;
    public GameObject terrain;
    public string terrainName;

    private int clickCount;
    private Path pathInstance;
    private List<Path> instanceList;

    private Transform cloneParent;

    private GameObject refPoint;
    private GameObject clonePoint;

    private GameObject refLine;
    private GameObject pathLine;
    private List<Vector3> pathPositions;

    void Start()
    {
        clickCount = 0;
        refPoint = transform.GetChild(0).gameObject;
        refLine = transform.GetChild(1).gameObject;
        cloneParent = transform.GetChild(2);

        instanceList = new List<Path>();

        // event initialisation
        EventManager.StartListening("PlacePathPointClickEvent", PlacePointClickRecieved);
        EventManager.StartListening("DisplayResults_Path Distance", CalculateAndDisplay);

        EventManager.StartListening("StartUpdatingLines", StartUpdateCoroutine);
        EventManager.StartListening("StopUpdatingLines", StopUpdateCoroutine);

        EventManager.StartListening("StartPathSave", StartSaving);
        EventManager.StartListening("RestorePath", Restore);

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
            // this event is produced for the first time
            pathInstance = new Path();            
            pathPositions = new List<Vector3>();

            instanceList.Add(pathInstance);
        }

        if (clickCount == 1)
        {
            // instanciate a line and add it to the path instance
            pathLine = Instantiate(refLine, cloneParent);
            pathLine.SetActive(true);
            pathLine.name = "newLine";

            pathInstance.AddLine(pathLine);
        }

        // duplicate the reference Point game object and set its position
        clonePoint = Instantiate(refPoint, cloneParent);
        clonePoint.SetActive(true);
        clonePoint.transform.position = positionParam.getPointParam();

        // add the new Point to the pathInstance list
        pathInstance.AddElement(clonePoint);

        // add the point to the line renderer
        pathPositions.Add(clonePoint.transform.localPosition);

        if (clickCount > 0)
        {
            if (clickCount > 1)
            {
                pathLine.GetComponent<LineRenderer>().positionCount += 1;
                pathLine.GetComponent<LineRenderer>().SetPosition(pathLine.GetComponent<LineRenderer>().positionCount-1, pathPositions[pathLine.GetComponent<LineRenderer>().positionCount-1]);
            }
            //pathLine.GetComponent<LineRenderer>().SetPositions(pathPositions.ToArray());
        }
        
        // update the number of click eveents recieved
        clickCount += 1;
    }

    // calculate and display measure result
    private void CalculateAndDisplay(EventParam param)
    {
        if (pathInstance != null)
        {
            pathInstance.Calculate();
            pathInstance.Display();
            clickCount = 0;
        }
    }

    // coroutines and methods to update the lines between the points
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
            for (int i=0; i<instanceList.Count; i++)
            {
                instanceList[i].UptadeLinePosition();
            }

            yield return new WaitForSeconds(0.1f);
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
            EventManager.TriggerEvent("Save_Path", new EventParam(instanceList[i].SaveInstance()));
            yield return new WaitForEndOfFrame();
        }
        // start saving next measure
        EventManager.TriggerEvent("StartPlaneDistSave", null);
    }

    private void Restore(EventParam jsonStringParam)
    {
        // create an new path instance from the JSON string
        pathInstance = JsonUtility.FromJson<Path>(jsonStringParam.getStringParam());
        instanceList.Add(pathInstance);

        // instantiate enough point to render the path
        int n = pathInstance.GetPointsNumber();
        List<GameObject> pointsList = new List<GameObject>();
        for (int i=0; i<n; i++)
        {
            clonePoint = Instantiate(refPoint, cloneParent);
            pointsList.Add(clonePoint);
        }
        // instantiate a new line for the path
        pathLine = Instantiate(refLine, cloneParent);

        // set the non serializable fields of the path instance
        pathInstance.RestoreInstance(pointsList, pathLine);
    }
}


// Path class
public class Path : IMeasureData<GameObject>, IMeasureManager
{
    // number of instances
    static int pathNumber = 0;

    // instance elements
    [SerializeField] private int id;
    private List<GameObject> points; // unity game objects are not serializable
    [SerializeField] private List<Vector3> pointsPosition;
    private GameObject line;

    // result
    [SerializeField] private float distance;

    public Path()
    {
        id = pathNumber;
        pathNumber += 1;

        points = new List<GameObject>();
        pointsPosition = new List<Vector3>();

        distance = 0;
    }

    // add new elements to the measure instance
    public void AddElement(GameObject newPoint)
    {
        newPoint.name = "PathSphere_" + id.ToString();
        points.Add(newPoint);
        pointsPosition.Add(newPoint.transform.localPosition);
    }

    public void AddLine(GameObject newLine)
    {
        line = newLine;
        line.name = "Line_" + id.ToString();
    }

    public void UptadeLinePosition()
    {   
        if (points.Count > 0)
        {
            for (int p = 0; p < points.Count; p++)
            {
                line.GetComponent<LineRenderer>().SetPosition(p, points[p].transform.localPosition);
                pointsPosition[p] = points[p].transform.localPosition;
            }
        }
    }

    // calculate result
    public void Calculate()
    {
        distance = 0;

        for (int i = 0; i < points.Count - 1; i++)
        {
            // sum of distances between point i and i+1
            distance += Mathf.Sqrt((points[i + 1].transform.localPosition.x - points[i].transform.localPosition.x) * (points[i + 1].transform.localPosition.x - points[i].transform.localPosition.x)
                + (points[i + 1].transform.localPosition.y - points[i].transform.localPosition.y) * (points[i + 1].transform.localPosition.y - points[i].transform.localPosition.y)
                + (points[i + 1].transform.localPosition.z - points[i].transform.localPosition.z) * (points[i + 1].transform.localPosition.z - points[i].transform.localPosition.z));
        }
    }

    public void Display()
    {
        // send custom event to display measure result
        EventManager.TriggerEvent("DisplayResult", new EventParam("Distance: "+distance.ToString()+"m"));

        if (points.Count == 0)
            EventManager.TriggerEvent("ErrorMessage", new EventParam("Placez au moins un point avant de demander l'affichage de la mesure de distance."));
    }

    // saving method
    public string SaveInstance()
    {
        return JsonUtility.ToJson(this);
    }

    public int GetPointsNumber() // used to instantiate spheres for measure restoration
    {
        return pointsPosition.Count;
    }

    // restoring method
    public void RestoreInstance(List<GameObject> newPoint, GameObject newLine)
    {
        line = newLine;
        points = newPoint;
        for (int i=0; i<pointsPosition.Count; i++)
        {
            points[i].SetActive(true);
            points[i].name = "PathSphere_" + id.ToString();
            points[i].transform.localPosition = pointsPosition[i];
        }
        line.SetActive(true);
        line.name = "Line_" + id.ToString();
        line.GetComponent<LineRenderer>().positionCount = pointsPosition.Count;
        line.GetComponent<LineRenderer>().SetPositions(pointsPosition.ToArray());
    }
}