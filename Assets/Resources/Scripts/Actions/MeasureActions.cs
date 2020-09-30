using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;

/* MeasureActions script 
 * 
 *      deals with actions not associated to laser pointer
 */

public class MeasureActions : MonoBehaviour
{
    // simulation and measure status
    private bool simulationStatus;
    private bool pointerIsActive;
    private string currentActiveMeasure;
    private GameObject currentHand;

    // reference to the action
    public SteamVR_Action_Boolean activateMeasurePointer;
    public SteamVR_Action_Boolean displayResults;
    public SteamVR_Action_Boolean releaseGrabbedElement;

    // reference to the hand
    public SteamVR_Input_Sources rightHand;
    public SteamVR_Input_Sources leftHand;

    public GameObject measureLaserPointer;
    private Quaternion pointerAngle;

    // Start is called before the first frame update
    void Start()
    {
        // variable initialisation
        simulationStatus = false;
        pointerIsActive = false;
        currentHand = null;
        currentActiveMeasure = null;

        measureLaserPointer.SetActive(pointerIsActive);
        pointerAngle = new Quaternion(0.38f, 0f, 0f, 1f);

        // controler event listeners initialisation
        activateMeasurePointer.AddOnStateDownListener(ActivatePointer, rightHand);
        activateMeasurePointer.AddOnStateDownListener(ActivatePointer, leftHand);

        displayResults.AddOnStateUpListener(DisplayResults, rightHand);
        displayResults.AddOnStateUpListener(DisplayResults, leftHand);

        releaseGrabbedElement.AddOnStateUpListener(ReleaseElement, rightHand);
        releaseGrabbedElement.AddOnStateUpListener(ReleaseElement, leftHand);


        // custom event listeners initialisation
        EventManager.StartListening("StartSimulation", StartSimulation);
        EventManager.StartListening("UpdateMeasureMode", UpdateMeasureMode);
        EventManager.StartListening("ForceMeasurePointerActivation", ForceMeasurePointerActivation);
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
        pointerIsActive = true;

        // set pointer to the oposite hand
        if (handParam.getGameObjectParam().name == rightHand.ToString())
        {
            SetPointerToHand(GameObject.Find(leftHand.ToString()));
        }
        else
        {
            SetPointerToHand(GameObject.Find(rightHand.ToString()));
        }

        measureLaserPointer.gameObject.SetActive(pointerIsActive);
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
    void ActivatePointer(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (simulationStatus == true)
        {
            // update pointer status mode
            pointerIsActive = !pointerIsActive;

            // set the laser pointer on the source hand
            SetPointerToHand(GameObject.Find(source.ToString()));

            // activate/deactivate laser pointer on the source hand
            measureLaserPointer.gameObject.SetActive(pointerIsActive);
        }
        
    }

    void DisplayResults(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        Debug.Log("DisplayResults_" + currentActiveMeasure);
        EventManager.TriggerEvent("DisplayResults_" + currentActiveMeasure, null);
    }

    void ReleaseElement(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        EventManager.TriggerEvent("ReleaseGrabbedElement", null);
    }
}
