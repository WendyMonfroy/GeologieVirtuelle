using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;

public class ControlerActionsScript : MonoBehaviour
{
    // simulation and measure status
    private bool simulationStatus;
    private bool measurePointerIsActive;
    private string currentActiveMeasure;
    private GameObject currentHand;

    // reference to the general interface action
    public SteamVR_Action_Boolean generalInterfaceOnOff;

    // reference to the measure actions
    public SteamVR_Action_Boolean measureInterfaceOnOff;
    public SteamVR_Action_Boolean activateMeasurePointer;
    public SteamVR_Action_Boolean displayResults;
    public SteamVR_Action_Boolean releaseGrabbedElement;

    // reference to the hands
    public SteamVR_Input_Sources rightHand;
    public SteamVR_Input_Sources leftHand;

    //reference to the canvas
    public GameObject generalUI;
    public GameObject measureUI;

    // reference to the laser pointers
    public GameObject UILaserPointer;
    public GameObject measureLaserPointer;
    private Quaternion pointerAngle;


    // Start is called before the first frame update
    void Start()
    {
        // variable initialisation
        simulationStatus = false;
        measurePointerIsActive = false;
        currentHand = null;
        currentActiveMeasure = null;

        measureLaserPointer.SetActive(measurePointerIsActive);
        pointerAngle = new Quaternion(0.38f, 0f, 0f, 1f);

        // controler event listeners initialisation
        generalInterfaceOnOff.AddOnStateUpListener(ShowHideGeneralInterface, rightHand);
        generalInterfaceOnOff.AddOnStateUpListener(ShowHideGeneralInterface, leftHand);

        measureInterfaceOnOff.AddOnStateUpListener(ShowHideMeasureInterface, rightHand);
        measureInterfaceOnOff.AddOnStateUpListener(ShowHideMeasureInterface, leftHand);

        activateMeasurePointer.AddOnStateDownListener(ActivateMeasurePointer, rightHand);
        activateMeasurePointer.AddOnStateDownListener(ActivateMeasurePointer, leftHand);

        displayResults.AddOnStateUpListener(DisplayResults, rightHand);
        displayResults.AddOnStateUpListener(DisplayResults, leftHand);

        releaseGrabbedElement.AddOnStateUpListener(ReleaseElement, rightHand);
        releaseGrabbedElement.AddOnStateUpListener(ReleaseElement, leftHand);


        // custom event listeners initialisation
        EventManager.StartListening("StartSimulation", StartSimulation);
        EventManager.StartListening("UpdateMeasureMode", UpdateMeasureMode);
        EventManager.StartListening("ForceMeasurePointerActivation", ForceMeasurePointerActivation);
    }


    // general interface methods
    void ShowHideGeneralInterface(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        EventManager.TriggerEvent("ShowHideInterface", null);
    }

    // measure interface methods
    void ShowHideMeasureInterface(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        EventManager.TriggerEvent("ShowHideMeasureInterface", new EventParam(GameObject.Find(source.ToString())));
    }


    void StartSimulation(EventParam param)
    {
        simulationStatus = true;
    }

    void UpdateMeasureMode(EventParam nameParam)
    {
        currentActiveMeasure = nameParam.getStringParam();
    }

    // froce measure pointer activation when displaying the measure interface
    void ForceMeasurePointerActivation(EventParam handParam)
    {
        measurePointerIsActive = true;

        // set pointer to the oposite hand
        if (handParam.getGameObjectParam().name == rightHand.ToString())
        {
            SetPointerToHand(GameObject.Find(leftHand.ToString()));
        }
        else
        {
            SetPointerToHand(GameObject.Find(rightHand.ToString()));
        }

        measureLaserPointer.gameObject.SetActive(measurePointerIsActive);
    }


    // method to set the measure pointer on the proper hand
    void SetPointerToHand(GameObject hand)
    {
        currentHand = hand;
        // set the laser pointer on the source hand
        measureLaserPointer.transform.SetParent(currentHand.transform);
        measureLaserPointer.transform.localPosition = Vector3.zero;
        measureLaserPointer.transform.localRotation = pointerAngle;
        measureLaserPointer.GetComponent<SteamVR_MeasureLaserPointer>().pose = currentHand.GetComponent<SteamVR_Behaviour_Pose>();
    }


    // activate/deactivate pointer when the associated button is pressed
    void ActivateMeasurePointer(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (simulationStatus == true)
        {
            // update pointer status mode
            measurePointerIsActive = !measurePointerIsActive;

            // set the laser pointer on the source hand
            SetPointerToHand(GameObject.Find(source.ToString()));

            // activate/deactivate laser pointer on the source hand
            measureLaserPointer.gameObject.SetActive(measurePointerIsActive);
        }

    }

    // display/hide result of the current active measure
    void DisplayResults(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        Debug.Log("DisplayResults_" + currentActiveMeasure);
        EventManager.TriggerEvent("DisplayResults_" + currentActiveMeasure, null);
    }

    // release grabbed element when controller button is released
    void ReleaseElement(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        EventManager.TriggerEvent("ReleaseGrabbedElement", null);
    }
}

