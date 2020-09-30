using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.Extras;

public class WulffMeasure : MonoBehaviour
{
    public Camera cam;
    public GameObject terrain; // reference to the terrain element
    public string terrainName;
    private bool immersionModeIsActive; // simulation mode status

    private int clickCount; // 
    private WulffClass wulffInstance; // current Wulff instance
    private List<WulffClass> instancesList; // list of instances

    private Transform cloneParent;

    private GameObject refElipse;
    private GameObject cloneElipse;

    private GameObject wulffSphere;


    // Start is called before the first frame update
    void Start()
    {
        immersionModeIsActive = true;

        clickCount = 0;
        instancesList = new List<WulffClass>();

        refElipse = transform.GetChild(0).gameObject;
        cloneParent = transform.GetChild(1);

        // event initialisation
        EventManager.StartListening("PlaceWulffElementClickEvent", PlaceWulffElementClickRecieved);
        EventManager.StartListening("DisplayResults_Wulff", CalculateAndDisplay);

        EventManager.StartListening("StartWulffSave", StartSaving);
        EventManager.StartListening("RestoreWulff", Restore);

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
            instancesList.Clear();
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
            if (cloneParent.GetChild(i).name.Contains("Selection"))
                cloneParent.GetChild(i).localScale = new Vector3(1.5f, 0.2f, 1.5f);
            else
                cloneParent.GetChild(i).localScale = Vector3.one * 0.5f;
        }

        cloneParent.gameObject.SetActive(false);
    }

    // process click event
    void PlaceWulffElementClickRecieved(EventParam positionParam)
    {
        if (clickCount == 0)
        {
            wulffInstance = new WulffClass();

            cloneElipse = Instantiate(refElipse, cloneParent);
            cloneElipse.SetActive(true);
            cloneElipse.transform.position = positionParam.getPointParam();

            wulffInstance.AddElement(cloneElipse);
        }
        else if (clickCount == 1)
        {
            cloneElipse.tag = "Untagged";
            
            DrawWulffSphere(cloneElipse.transform.localPosition);
            wulffSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            wulffInstance.AddElement(wulffSphere);
        }
        clickCount += 1;
    }

    // method to draw the Wulff Sphere 
    // taken from the 2018 GeolVir3D project
    void DrawWulffSphere(Vector3 position)
    {
        wulffSphere = new GameObject("WulffSphere");
        if (immersionModeIsActive == true)
            wulffSphere.transform.localPosition = position + new Vector3(0f, 0.8f, 0f);
        else
            wulffSphere.transform.localPosition = position + new Vector3(0f, 0.008f, 0f);

        wulffSphere.transform.SetParent(cloneParent);
        wulffSphere.AddComponent<MeshFilter>();
        wulffSphere.AddComponent<MeshRenderer>();

        int space = 50;
        int nb_parallels = 10;
        int nb_meridians = 20;

        Mesh wulffMesh = wulffSphere.GetComponent<MeshFilter>().mesh;
        MeshRenderer wulffRenderer = wulffSphere.GetComponent<MeshRenderer>();
        int nb_vert = 2 * space * nb_parallels + 2 * space * nb_meridians;

        Vector3[] vert = new Vector3[nb_vert];
        Color[] col = new Color[nb_vert];
        int[] indices = new int[nb_vert];

        float pas_rad = 2 * Mathf.PI / space;

        // Drawing the parallels
        for (int id_par = 0; id_par < nb_parallels; ++id_par)
        {
            float posy = 2 * id_par / (float)nb_parallels - 1f;
            float radius = Mathf.Cos(Mathf.Asin(posy));

            for (int step = 0; step < space; ++step)
            {
                float angle = step * pas_rad;
                float next_angle = (step + 1) * pas_rad;
                int stepindex = 2 * (id_par * space + step);

                vert[stepindex] = new Vector3(radius * Mathf.Cos(angle), posy, radius * Mathf.Sin(angle));
                vert[stepindex + 1] = new Vector3(radius * Mathf.Cos(next_angle), posy, radius * Mathf.Sin(next_angle));
                if (id_par % 5 == 0)
                {
                    col[stepindex] = Color.black;
                    col[stepindex + 1] = Color.black;
                }
                else
                {
                    col[stepindex] = Color.grey;
                    col[stepindex + 1] = Color.grey;
                }
                indices[stepindex] = stepindex;
                indices[stepindex + 1] = stepindex + 1;
            }
        }
        // Drawing the meridians
        for (int id_mer = 0; id_mer < nb_meridians; ++id_mer)
        {
            float angle_mer = id_mer * 2 * Mathf.PI / nb_meridians;

            for (int step = 0; step < space; ++step)
            {
                float angle = step * pas_rad;
                float next_angle = (step + 1) * pas_rad;
                int stepindex = 2 * space * nb_parallels + 2 * (id_mer * space + step);

                vert[stepindex] = new Vector3(Mathf.Cos(angle_mer) * Mathf.Cos(angle), Mathf.Sin(angle), -Mathf.Sin(angle_mer) * Mathf.Cos(angle));
                vert[stepindex + 1] = new Vector3(Mathf.Cos(angle_mer) * Mathf.Cos(next_angle), Mathf.Sin(next_angle), -Mathf.Sin(angle_mer) * Mathf.Cos(next_angle));
                if (id_mer % 5 == 0)
                {
                    col[stepindex] = Color.black;
                    col[stepindex + 1] = Color.black;
                }
                else
                {
                    col[stepindex] = Color.grey;
                    col[stepindex + 1] = Color.grey;
                }
                indices[stepindex] = stepindex;
                indices[stepindex + 1] = stepindex + 1;
            }
        }

        wulffMesh.vertices = vert;
        wulffMesh.colors = col;
        wulffRenderer.material.SetColor("_Color", Color.black);

        wulffMesh.SetIndices(indices, MeshTopology.Lines, 0, true);
    }

    // calculate and display measure result
    void CalculateAndDisplay(EventParam param)
    {
        if (param == null && wulffInstance != null && clickCount > 1)
        {
            instancesList.Add(wulffInstance);

            wulffInstance.Calculate();
            wulffInstance.Display();
            clickCount = 0;
        }
        else if (param != null)
        {
            //WulffClass targetInstance = param.getGameObjectParam();
        }
        else if (clickCount < 2)
            EventManager.TriggerEvent("ShowHideResults", null);
    }

    // save methods
    private void StartSaving(EventParam param)
    {
        StartCoroutine("Save");
    }

    IEnumerator Save()
    {
        for (int i = 0; i < instancesList.Count; i++)
        {
            EventManager.TriggerEvent("Save_Wulff", new EventParam(instancesList[i].SaveInstance()));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }

        EventManager.TriggerEvent("StartPathSave", null);
    }

    // restore saved measures method
    private void Restore(EventParam jsonStringParam)
    {
        // create an new measure instance from the JSON string
        wulffInstance = JsonUtility.FromJson<WulffClass>(jsonStringParam.getStringParam());
        instancesList.Add(wulffInstance);

        // instantiate 2 planes
        cloneElipse = Instantiate(refElipse, cloneParent);
        cloneElipse.SetActive(true);

        DrawWulffSphere(cloneElipse.transform.localPosition);
        wulffSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // set the non serializable fields of the measure instance
        wulffInstance.RestoreInstance(cloneElipse, wulffSphere);
    }
}

public class WulffClass : IMeasureData<GameObject>, IMeasureManager
{
    static int wulffNumber = 0;
    [SerializeField] private int id;

    GameObject selectionElement;
    GameObject wulffSphere;

    [SerializeField] private Vector3 wulffPosition;
    [SerializeField] private Quaternion wulffRotation;

    [SerializeField] private float pendage;
    [SerializeField] private float azimut;

    public WulffClass()
    {
        id = wulffNumber;
        wulffNumber += 1;
    }

    // add new elements to the measure instance
    public void AddElement(GameObject elt)
    {
        if (selectionElement == null)
        {
            selectionElement = elt;
            selectionElement.name = "Selection_" + id.ToString();
        }
        else
        {
            wulffSphere = elt;
            wulffSphere.name = "WulffSphere_" + id.ToString();
        }
    }

    public void Calculate()
    {
        if(selectionElement != null && wulffSphere != null)
        {
            // calculate mean plane normal vector
            Vector3 meanNormalVector;
            meanNormalVector = selectionElement.GetComponent<SphereCollisionScript>().CalculateMeanNormal();

            pendage = Vector3.Angle(Vector3.up, meanNormalVector);
            azimut = Vector3.Angle(Vector3.Cross(meanNormalVector, Vector3.up), Vector3.forward);
        }
    }

    public void Display()
    {
        EventManager.TriggerEvent("DisplayResult", new EventParam("Pendage: " + pendage.ToString() + "°\nAzimut: " + azimut.ToString()));
    }

    // saving method
    public string SaveInstance()
    {
        wulffPosition = wulffSphere.transform.localPosition;
        wulffRotation = wulffSphere.transform.localRotation;

        return JsonUtility.ToJson(this);
    }

    // restoring method
    public void RestoreInstance(GameObject selection, GameObject wulff)
    {
        selectionElement = selection;
        selectionElement.name = "Selection_" + id.ToString();
        selectionElement.transform.localPosition = wulffPosition;
        selectionElement.transform.localRotation = wulffRotation;
        selectionElement.SetActive(true);

        wulffSphere = wulff;
        wulffSphere.name = "WulffSphere_" + id.ToString();
        wulffSphere.transform.localPosition = wulffPosition + Vector3.up*0.8f;
        wulffSphere.transform.localRotation = wulffRotation;
        wulffSphere.SetActive(true);
    }
}
