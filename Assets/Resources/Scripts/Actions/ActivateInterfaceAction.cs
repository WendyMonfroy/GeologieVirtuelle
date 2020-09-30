using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ActivateInterfaceAction : MonoBehaviour
{
    // reference to the action
    public SteamVR_Action_Boolean generalInterfaceOnOff;
    public SteamVR_Action_Boolean measureInterfaceOnOff;

    // reference to the hand
    public SteamVR_Input_Sources rightHand;
    public SteamVR_Input_Sources leftHand;

    //reference to the canvas
    public GameObject generalUI;
    public GameObject measureUI;


    // Start is called before the first frame update
    void Start()
    {
        generalInterfaceOnOff.AddOnStateUpListener(ShowHideGeneralInterface, rightHand);
        generalInterfaceOnOff.AddOnStateUpListener(ShowHideGeneralInterface, leftHand);

        measureInterfaceOnOff.AddOnStateUpListener(ShowHideMeasureInterface, rightHand);
        measureInterfaceOnOff.AddOnStateUpListener(ShowHideMeasureInterface, leftHand);
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
