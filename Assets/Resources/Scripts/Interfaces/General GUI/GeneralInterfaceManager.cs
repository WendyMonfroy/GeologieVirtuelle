using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class GeneralInterfaceManager : MonoBehaviour
{
    public Player player;
    public GameObject pointer; // reference on the UIPointer
    public GameObject button; // bouton to switch between immersion and model modes 
    private GameObject interfaceCanvas;
    private GameObject loadingCanvas;

    public GameObject saveButton;

    // general interface position and rotation offset
    private Vector3 positionOffset;
    private Quaternion rotationOffset;

    // game status variables
    private bool simulationRuning;
    private bool immersionMode;


    // Start is called before the first frame update
    void Start()
    {
        interfaceCanvas = transform.GetChild(0).gameObject;
        loadingCanvas = transform.GetChild(1).gameObject;

        loadingCanvas.SetActive(false);

        positionOffset = new Vector3(-1f, 1.3f, 0.9f);
        rotationOffset = new Quaternion(0.1f, 0f, 0f, 1f);

        immersionMode = true;
        simulationRuning = false;
        interfaceCanvas.SetActive(false);

        // event initialization
        EventManager.StartListening("DestroyReaminingMeshes", ShowInterface);
        EventManager.StartListening("ShowHideInterface", ShowHide);
        EventManager.StartListening("SavingDone", StopSavingAnim);
        EventManager.StartListening("RestoreMeasures", StartLoadAnim);
        EventManager.StartListening("LoadingComplete", StopLoadAnim);
    }


    //first activation 
    void ShowInterface(EventParam nullParam)
    {
        // called when starting the simulation
        simulationRuning = true;
        ShowHide(null);
    }

    // UI activation/deactivation
    public void ShowHide(EventParam nullParam)
    {
        // called when the paleyer press the associated button on the oculus touch
        if (simulationRuning == true)
        {
            interfaceCanvas.SetActive(!interfaceCanvas.activeSelf);
            if (interfaceCanvas.activeSelf == true)
            {
                // set position and rotation of the interface in front of the player
                interfaceCanvas.transform.position = player.hmdTransform.position + new Vector3(player.hmdTransform.forward.x * 1.3f, -0.3f, player.hmdTransform.forward.z * 1.3f);
                interfaceCanvas.transform.eulerAngles = new Vector3(5f, player.hmdTransform.eulerAngles.y, 0f);

                // force UIPointer activation when the general interface is active
                pointer.SetActive(true);
            }
            else
            {
                // force UIPointer deactivation when the general interface is hidden
                pointer.SetActive(false);
            }
        }
    }

    // linked to UI button
    public void BackToLobby()
    {
        // called when simulation is ending
        EventManager.TriggerEvent("BackToLobby", null);
        interfaceCanvas.SetActive(false);
        simulationRuning = false;

        // reset immersion mode
        immersionMode = true;
        button.transform.GetChild(0).GetComponent<Text>().text = "Passer en mode maquette";
    }

    // linked to sitch button
    public void SwitchBetweenImersionAdnModel()
    {
        // called when the player presses the associated button on the interface

        if (immersionMode == true)
        {
            // switch from immersion to model mode
            EventManager.TriggerEvent("SwitchBetweenImmersionAndModel", new EventParam(0));
            button.transform.GetChild(0).GetComponent<Text>().text = "Passer en mode immersion";
            immersionMode = false;

            // set interface position
            interfaceCanvas.transform.localPosition = positionOffset;
            interfaceCanvas.transform.localRotation = rotationOffset;
        }
        else
        {
            // switch from model to immersion mode
            EventManager.TriggerEvent("SwitchBetweenImmersionAndModel", new EventParam(1));
            button.transform.GetChild(0).GetComponent<Text>().text = "Passer en mode maquette";
            immersionMode = true;

            // set interface position and rotation in front of the player
            interfaceCanvas.transform.position = player.hmdTransform.position + new Vector3(player.hmdTransform.forward.x * 1.3f, -0.3f, player.hmdTransform.forward.z * 1.3f);
            interfaceCanvas.transform.eulerAngles = new Vector3(5f, player.hmdTransform.eulerAngles.y, 0f);
        }
    }

    // method linked to the UI button
    public void Save()
    {
        EventManager.TriggerEvent("StartMarkerSave", null);

        saveButton.transform.GetChild(0).gameObject.SetActive(false);
        saveButton.transform.GetChild(1).gameObject.SetActive(true);

        saveButton.GetComponentInChildren<Animator>().SetBool("Animate", true);
    }

    private void StopSavingAnim(EventParam param)
    { // stops animation when saving is done
        saveButton.GetComponentInChildren<Animator>().SetBool("Animate", false);

        saveButton.transform.GetChild(0).gameObject.SetActive(true);
        saveButton.transform.GetChild(1).gameObject.SetActive(false);
    }

    // start animation when loading measures
    private void StartLoadAnim(EventParam paran)
    {
        Debug.Log("c'est un début de chargement");
        loadingCanvas.SetActive(true);
        loadingCanvas.transform.position = player.hmdTransform.position + new Vector3(player.hmdTransform.forward.x * 1.2f, -0.3f, player.hmdTransform.forward.z * 1.2f);
        loadingCanvas.transform.eulerAngles = new Vector3(5f, player.hmdTransform.eulerAngles.y, 0f);
        loadingCanvas.transform.GetChild(0).GetChild(1).GetComponent<Animator>().SetBool("Animate", true);
    }

    private void StopLoadAnim(EventParam paran)
    { // stops animation when loading is done
        Debug.Log("c'est une fin de chargement");
        loadingCanvas.SetActive(false);
        loadingCanvas.transform.GetChild(0).GetChild(1).GetComponent<Animator>().SetBool("Animate", false);
    }
}
