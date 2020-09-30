using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchSimulation : MonoBehaviour
{
    void Start()
    {
        
    }

    public void StartSimulationMode()
    {
        // trigger custom event when the button is pressed
        EventManager.TriggerEvent("SendSignalToStartSimulation", null); // event sent to MeshLoader
    }
}
