using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR.Extras;

public class PointerHandler : MonoBehaviour
{

    public SteamVR_LaserPointer laserPointer;


    void Awake()
    {
        // subscription to pointer events
        laserPointer.PointerIn += PointerInside;
        laserPointer.PointerOut += PointerOutside;
        laserPointer.PointerClick += PointerClick;

        //EventManager.StartListening("", ForceUIPointerActivation);
    }

    public void PointerClick(object sender, PointerEventArgs e)
    {
        if (e.target.tag == "Button" && e.target.GetComponent<Button>().interactable==true)
        {
            // invoke click event
            e.target.GetComponent<Image>().color = e.target.GetComponent<Button>().colors.pressedColor;
            e.target.GetComponent<Button>().onClick.Invoke();
            // trigger custom event with the button clicked
            EventManager.TriggerEvent("ButtonClicked", new EventParam(e.target.gameObject));
        }
        if (e.target.tag == "Terrain")
        {
            // trigger custom event with the pointed mesh and the triangle index
            EventManager.TriggerEvent("PositionClickedOnTerrain", new EventParam(e.target.gameObject, e.triangleIndex, e.point));
        }
    }

    public void PointerInside(object sender, PointerEventArgs e)
    {
        if (e.target.tag == "Button" && e.target.GetComponent<Button>().interactable == true)
        {
            //if (e.target.GetComponent<Image>().color != e.target.GetComponent<Button>().colors.pressedColor)
            e.target.GetComponent<Image>().color = e.target.GetComponent<Button>().colors.highlightedColor;
            EventManager.TriggerEvent("ButtonHovered", new EventParam(e.target.gameObject));
        }
    }

    public void PointerOutside(object sender, PointerEventArgs e)
    {
        if (e.target.tag == "Button" && e.target.GetComponent<Button>().interactable == true)
        {
            //if (e.target.GetComponent<Image>().color != e.target.GetComponent<Button>().colors.pressedColor)
            e.target.GetComponent<Image>().color = e.target.GetComponent<Button>().colors.normalColor;
            EventManager.TriggerEvent("ButtonExited", new EventParam(e.target.gameObject));
        }

        if (e.target.tag == "GeneralInterface")
        {
            EventManager.TriggerEvent("ReactivateMeasurePointer", null);
        }
    }

    //
    public void ForceUIPointerActivation(EventParam param)
    {

    }
}
