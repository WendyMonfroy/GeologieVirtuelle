using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class MeasureInterfaceManager : MonoBehaviour
{
    // player variables
    public Player player;
    private GameObject hand;
    private GameObject previousHand;

    // status variable
    private bool simulationRuning;

    // measure canvas varaibles
    private GameObject interfaceCanvas;
    private Vector3 interfacePosition;
    private Quaternion interfaceRotation;
    private GameObject descriptionCanvas;

    private GameObject markerCanvas;
    private GameObject resultCanvas;

    // button variables
    private List<GameObject> buttonList;
    private GameObject currentButton; // set when a button is hovered
    private GameObject selectedButton; // set when a button is clicked

    // Start is called before the first frame update
    void Start()
    {
        previousHand = null;

        simulationRuning = false;

        interfaceCanvas = transform.GetChild(0).gameObject;
        interfacePosition = new Vector3(0f, 0.08f, 0);
        interfaceRotation = new Quaternion(1f, 0f, 0f, 1f);
        descriptionCanvas = interfaceCanvas.transform.GetChild(1).gameObject; // save the center description canvas

        markerCanvas = transform.GetChild(1).gameObject;
        resultCanvas = transform.GetChild(2).gameObject;

        buttonList = new List<GameObject>();
        for (int i=0; i<interfaceCanvas.transform.GetChild(0).childCount; i++)
        {
            buttonList.Add(interfaceCanvas.transform.GetChild(0).GetChild(i).gameObject);
        }
        currentButton = null;
        selectedButton = null;

        interfaceCanvas.SetActive(false);
        descriptionCanvas.SetActive(false);
        markerCanvas.SetActive(false);
        resultCanvas.SetActive(false);

        // event listener to activate the measure interface
        EventManager.StartListening("ShowHideMeasureInterface", ShowHide);
        EventManager.StartListening("ShowHideMarkerEditorInterface", ShowHideMarkerInterface);
        // event listeners to update simulation status
        EventManager.StartListening("StartSimulation", StratSimulation);
        EventManager.StartListening("BackToLobby", StopSimulation);

        // event listener for GUI button status
        EventManager.StartListening("MeasureButtonHovered", DisplayButtonDescription);
        EventManager.StartListening("MeasureButtonClicked", SaveClickedButton);
        EventManager.StartListening("MeasureButtonExited", HideButtonDescription);

        // event listener for result request
        EventManager.StartListening("DisplayResult", DisplayResult);
        EventManager.StartListening("ShowHideResults", SwitchResultDisplay);
    }

    void ShowHide(EventParam handParam)
    {
        hand = handParam.getGameObjectParam();

        if (simulationRuning == true)
        {
            if (hand != previousHand)
            {
                interfaceCanvas.SetActive(true);

                // attach interface to hand
                interfaceCanvas.transform.SetParent(hand.transform);
                interfaceCanvas.transform.localPosition = interfacePosition;
                interfaceCanvas.transform.localRotation = interfaceRotation;
                
                // update hand
                previousHand = hand;
            } 
            else
            {
                // show hidden interface or hide shown interface
                interfaceCanvas.SetActive(!interfaceCanvas.activeSelf);
            }
        }
        if (interfaceCanvas.activeSelf == true)
        {
            // force measure pointer activation
            EventManager.TriggerEvent("ForceMeasurePointerActivation", new EventParam(hand));
        }
    }

    public void ShowHideMarkerInterface(EventParam param)
    {
        markerCanvas.SetActive(!markerCanvas.activeSelf);

        markerCanvas.transform.position = player.hmdTransform.position + new Vector3(player.hmdTransform.forward.x * 1.2f, -0.1f, player.hmdTransform.forward.z * 0.6f);
        markerCanvas.transform.eulerAngles = Vector3.up * player.hmdTransform.eulerAngles.y;
    }

    void StratSimulation(EventParam param)
    {
        // update simulation status
        simulationRuning = true;
    }

    void StopSimulation(EventParam param)
    {
        // update simulation status
        interfaceCanvas.SetActive(false);
        simulationRuning = false;
    }

    void DisplayButtonDescription(EventParam buttonParam)
    {
        // check if the hovered button is part of the measure interface
        if (buttonList.Contains(buttonParam.getGameObjectParam()))
        {
            // activate the center description part of the measure UI
            descriptionCanvas.SetActive(true);

            // update the current button
            currentButton = buttonParam.getGameObjectParam();

            // set the center description part of the measure UI to hovered button content
            descriptionCanvas.transform.GetChild(0).GetComponent<Image>().sprite = currentButton.GetComponent<Image>().sprite;
            descriptionCanvas.transform.GetChild(1).GetComponent<Text>().text = currentButton.transform.GetChild(0).GetComponent<Text>().name;
            descriptionCanvas.transform.GetChild(2).GetComponent<Text>().text = currentButton.transform.GetChild(0).GetComponent<Text>().text;
        }
    }

    void HideButtonDescription(EventParam buttonParam)
    {
        // check if the exited button is part of the measure interface
        if (buttonList.Contains(buttonParam.getGameObjectParam()))
        {
            // update the current button
            currentButton = buttonParam.getGameObjectParam();
            if (selectedButton == null)
                descriptionCanvas.SetActive(false);
            else
            {
                // set the center description part of the measure UI to clicked button content
                descriptionCanvas.transform.GetChild(0).GetComponent<Image>().sprite = selectedButton.GetComponent<Image>().sprite;
                descriptionCanvas.transform.GetChild(1).GetComponent<Text>().text = selectedButton.transform.GetChild(0).GetComponent<Text>().name;
                descriptionCanvas.transform.GetChild(2).GetComponent<Text>().text = selectedButton.transform.GetChild(0).GetComponent<Text>().text;
            }
        }
    }

    void SaveClickedButton(EventParam buttonParam)
    {
        if (buttonList.Contains(buttonParam.getGameObjectParam()))
        {
            selectedButton = buttonParam.getGameObjectParam();

            // force result canvas deactivation when selecting a new measure
            resultCanvas.SetActive(false);

            // trigger custom event to activate the measure
            EventManager.TriggerEvent("UpdateMeasureMode", new EventParam(selectedButton.name));
        }
    }

    void DisplayResult(EventParam param)
    {
        if (resultCanvas.activeSelf == false || resultCanvas.transform.GetChild(0).GetChild(0).GetComponent<Text>().text != param.getStringParam())
        {
            // set result canvas transform
            resultCanvas.transform.SetParent(previousHand.transform);
            resultCanvas.transform.localPosition = interfacePosition + Vector3.up * 0.01f;
            resultCanvas.transform.localRotation = interfaceRotation;
            // set canvas text
            resultCanvas.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = param.getStringParam();

            resultCanvas.SetActive(true);
        }
        else if (resultCanvas.transform.GetChild(0).GetChild(0).GetComponent<Text>().text == param.getStringParam())
        {
            resultCanvas.SetActive(false);
        }
    }

    void SwitchResultDisplay(EventParam param)
    {
        resultCanvas.SetActive(!resultCanvas.activeSelf);
    }
}
