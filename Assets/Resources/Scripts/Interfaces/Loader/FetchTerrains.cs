using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FetchTerrains : MonoBehaviour
{
    // button to duplicate (contains all necesary elements)
    public GameObject refButton; 
    // new button instance
    private GameObject newButton;

    private string directoryPath;
    private string[] fileNameParsed;
    private List<string> files;
    // dictionnaire pour association bouton/terrain(nom)
    private Dictionary<GameObject, string> terrainButtons;

    private Transform scrollView;
    //private List<GameObject> childButton;

    void Start()
    {
        // set directory path
        directoryPath = Directory.GetCurrentDirectory() + @"\Maillages";
        
        // scroll view is used as the parent of all terrain buttons
        scrollView = gameObject.GetComponent<Transform>().GetChild(1).GetChild(0);

        files = new List<string>();
        terrainButtons = new Dictionary<GameObject, string>();

        // custom event listener
        EventManager.StartListening("ButtonClicked", SendLoadMeshEvent);

        GetTerrains();
    }

    void GetTerrains()
    {
        // check if the directory exists
        if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\Maillages"))
        {
            EventManager.TriggerEvent("LobbyErrorMessage", new EventParam("Le répertoire de dépot des terrains n'existe pas."));
            Debug.Log("le dossier n'existe pas");
        }
        else
        {
            // get a list of .obj files is the chosen directory and its children
            files = Directory.EnumerateFiles(directoryPath, "*.obj", SearchOption.AllDirectories).ToList();

            // save the association button-file
            terrainButtons = new Dictionary<GameObject, string>();

            // create a new button for each terrain found
            for (int i = 0; i < files.Count(); i++)
            {
                newButton = Instantiate(refButton);
                newButton.SetActive(true);
                newButton.name = "btn" + i;
                newButton.GetComponent<Transform>().SetParent(scrollView);
                newButton.GetComponent<Transform>().localScale = new Vector3(1f, 1f, 1f);
                newButton.GetComponent<Transform>().localRotation = new Quaternion(0f, 0f, 0f, 0f);
                newButton.GetComponent<Transform>().localPosition = new Vector3(0f, 70 - i * 70f, -0.002f);

                // disable the button when the file is too big

                // set the text displayed in the button to the file name without the extention
                newButton.GetComponent<Transform>().GetChild(0).GetComponent<Text>().text = new FileInfo(files[i]).Name.Trim().Split('.')[0];

                // associate the button to its corresponding terrains file
                terrainButtons.Add(newButton, files[i]);
            }
        }
    }

    public void RefreshFiles()
    {
        Debug.Log("fichiers rafraichis : " + files.Count());
        // destroy previous butons 
        if (files.Count() > 0)
        {
            for (int i = 0; i < files.Count(); i++)
            {
                Debug.Log("bouton : " + scrollView.GetChild(i+2).name);
                Destroy(scrollView.GetChild(i+2).gameObject);
            }
            files.Clear();
            terrainButtons.Clear();
        }
        
        // call the GetTerrain method
        GetTerrains();
    }

    void SendLoadMeshEvent(EventParam buttonParam)
    {
        // check if the clicked button is in the dictionary and trigger a custom event with the associated file path
        if (terrainButtons.ContainsKey(buttonParam.getGameObjectParam()))
            EventManager.TriggerEvent("LoadChosenMesh", new EventParam(terrainButtons[buttonParam.getGameObjectParam()]));
    }
}
