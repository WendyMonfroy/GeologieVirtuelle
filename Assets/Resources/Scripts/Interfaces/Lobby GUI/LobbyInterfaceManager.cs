using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyInterfaceManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.GetChild(1).gameObject.SetActive(false);

        EventManager.StartListening("StartSimulation", HideInterface);
        EventManager.StartListening("BackToLobby", ShowInterface);
    }

    void HideInterface(EventParam param)
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }

    void ShowInterface(EventParam param)
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void StartSimulationMode()
    {
        // trigger custom event when the button is pressed
        EventManager.TriggerEvent("SendSignalToStartSimulation", null); // event sent to MeshLoader
    }

    public void LoadSavedMeasures()
    {
        EventManager.TriggerEvent("LoadMeshAndMeasures", null);
    }

    public void QuitApp()
    {
        transform.GetChild(1).gameObject.SetActive(true);
    }

    public void QuitAppConfirmed()
    {
        Application.Quit();
    }
    public void QuitAppCanceled()
    {
        transform.GetChild(1).gameObject.SetActive(false);
    }
}
