using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class MarkerMeasure : MonoBehaviour
{
    public Camera cam;
    public GameObject terrain;
    public string terrainName;

    private int clickCount;
    private bool newMarkerAllowed;
    private Marker markerInstance;
    private List<Marker> instancesList; // list of instances

    private Transform cloneParent;

    private GameObject refMarker;
    private GameObject cloneMarker;

    private GameObject button;

    // Start is called before the first frame update
    void Start()
    {
        instancesList = new List<Marker>();

        refMarker = transform.GetChild(0).gameObject;
        cloneParent = transform.GetChild(1);

        // event initialisation
        EventManager.StartListening("PlaceMarkerClickEvent", PlaceMarkerClickRecieved);
        EventManager.StartListening("MarkerButtonClicked", UpdateMarker);

        EventManager.StartListening("StartMarkerSave", StartSaving);
        EventManager.StartListening("RestoreMarker", Restore);

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
            for (int i=0; i<cloneParent.childCount; i++)
            {
                Destroy(cloneParent.GetChild(i).gameObject);
            }
            instancesList.Clear();
        }

        clickCount = 0;
        newMarkerAllowed = true;
    }

    void BackToLobby(EventParam param)
    {
        clickCount = 0;
        newMarkerAllowed = false;

        // get back the instanciated elements
        cloneParent.SetParent(transform);
        cloneParent.localScale = Vector3.one;
        for (int i = 0; i < cloneParent.childCount; i++)
        {
            cloneParent.GetChild(i).localScale = Vector3.one;
        }

        cloneParent.gameObject.SetActive(false);
    }

    public void PlaceMarkerClickRecieved(EventParam pointParam)
    {
        if (newMarkerAllowed == true)
        {
            if (clickCount == 0)
            {
                markerInstance = new Marker();
                //saveInstance.AddInstanceToSave(markerInstance);
            }

            cloneMarker = Instantiate(refMarker, cloneParent);
            cloneMarker.SetActive(true);
            cloneMarker.transform.position = pointParam.getPointParam();

            markerInstance.AddElement(cloneMarker);
            instancesList.Add(markerInstance);
            clickCount += 1;

            // block new marker creation when setting current parameters
            newMarkerAllowed = false;
            EventManager.TriggerEvent("ShowHideMarkerEditorInterface", null);
        }
    }

    public void UpdateMarker(EventParam buttonParam)
    {
        button = buttonParam.getGameObjectParam();
        if (button.name.Contains("Color"))
        {
            // update color of the marker
            markerInstance.UpdateColor(button.GetComponent<Button>().colors.normalColor);
        }

        if (button.name == "Validate")
        {
            clickCount = 0;
            EventManager.TriggerEvent("ShowHideMarkerEditorInterface", null);

            // allow new marker creation after about 1 second
            Invoke("EnableNewMarker", 0.8f);
        }
    }

    public void EnableNewMarker()
    {
        // called when the marker edition is done
        newMarkerAllowed = true;
    }

    // seving methods
    private void StartSaving(EventParam param)
    {
        StartCoroutine("Save");
    }

    IEnumerator Save()
    {
        yield return new WaitForSeconds(0.1f);

        for (int i=0; i<instancesList.Count; i++)
        {
            EventManager.TriggerEvent("Save_Marker", new EventParam(instancesList[i].SaveInstance()));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }

        EventManager.TriggerEvent("StartWulffSave", null);
    }

    private void Restore(EventParam jsonStringParam)
    {
        // instanciate a new Marker game object and a new Marker class instance
        markerInstance = JsonUtility.FromJson<Marker>(jsonStringParam.getStringParam());

        cloneMarker = Instantiate(refMarker, cloneParent);
        cloneMarker.SetActive(true);

        markerInstance.AddElement(cloneMarker);
        instancesList.Add(markerInstance);

        // set 
        markerInstance.RestoreInstance();
    }
}


// Marker class
[System.Serializable]
public class Marker : IMeasureData<GameObject>, IMeasureManager
{
    // number of instances
    static int markerNumber = 0;

    // instance elements
    [SerializeField] private int id;
    private GameObject marker;
    [SerializeField] private Vector3 markerPosition;
    [SerializeField] private Color markerColor;

    public Marker()
    {
        id = markerNumber;
        markerNumber += 1;
    }

    // add new elements to the measure instance
    public void AddElement(GameObject mark)
    {
        mark.name = "Marker_" + id.ToString();
        marker = mark;
    }

    public void UpdateColor(Color color)
    {
        marker.GetComponent<MeshRenderer>().material.color = color;
    }

    public void Calculate()
    {
        // no calculation implementation for markers
    }

    public void Display()
    {
        // no display implementation for markers ?
    }

    public string SaveInstance()
    {
        markerPosition = marker.transform.localPosition;
        markerColor = marker.GetComponent<MeshRenderer>().material.color;

        return JsonUtility.ToJson(this);
    }

    public void RestoreInstance()
    {
        marker.transform.localPosition = markerPosition;
        marker.GetComponent<MeshRenderer>().material.color = markerColor;
    }
}