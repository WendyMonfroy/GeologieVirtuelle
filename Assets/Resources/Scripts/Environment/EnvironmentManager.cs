using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    private GameObject lobbyEnvironment;
    private GameObject modelEnvironment;

    void Start()
    {
        lobbyEnvironment = transform.GetChild(0).gameObject;
        modelEnvironment = transform.GetChild(2).gameObject;

        EventManager.StartListening("PositionClickedOnTerrain", PlaceSpawnMarker);

        EventManager.StartListening("StartSimulation", HideLobbyElements);
        EventManager.StartListening("BackToLobby", ShowLobbyElements);

        EventManager.StartListening("SwitchBetweenImmersionAndModel", ShowHideModelElements);
    }

    void HideLobbyElements(EventParam param)
    {
        // hide lobby elements when the simulation is running
        lobbyEnvironment.SetActive(false);
    }
    void ShowLobbyElements(EventParam param)
    {
        // show lobby elements when the simulation is not running
        lobbyEnvironment.SetActive(true);
    }
    void PlaceSpawnMarker(EventParam positionParam)
    {
        // activate and place the spawn marker on the mini terrain
        lobbyEnvironment.transform.GetChild(3).position = positionParam.getPointParam();
        lobbyEnvironment.transform.GetChild(3).gameObject.SetActive(true);
    }


    void ShowHideModelElements(EventParam intParam)
    {
        // if intParam = 0 we want to activate the model elements and hide them otherwise
        modelEnvironment.SetActive(intParam.getIntParam() == 0);
    }
}
