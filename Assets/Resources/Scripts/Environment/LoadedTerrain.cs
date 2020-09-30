using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class LoadedTerrain : MonoBehaviour
{
    public GameObject player;
    private Vector3 playerLastPosition;

    private GameObject selectedTerrain;
    private GameObject teleportTerrain;
    private GameObject cloneTPTerrain;
    private int subTerrainCount;

    private Vector3 spawnPoint;
    private int index;
    private GameObject mesh;

    private Vector3 scale1o1; // scale 1 out of 1
    private Vector3 scale1o100; // scale 1 out of 100

    void Start()
    {
        spawnPoint = Vector3.zero;

        scale1o1 = new Vector3(1f, 1f, 1f);
        scale1o100 = new Vector3(0.01f, 0.01f, 0.01f);

        // custom event listener
        EventManager.StartListening("PositionClickedOnTerrain", SaveSpawnPosition); // save spawn location
        EventManager.StartListening("StartSimulation", StartSimulation); // quit lobby to simulation
        EventManager.StartListening("BackToLobby", BackToLobby); // quit simulation to lobby

        // events to switch between immersion and model modes
        EventManager.StartListening("SwitchBetweenImmersionAndModel", SwitchBetweenImmersionAndModel);
    }

    void StartSimulation(EventParam param)
    {
        // take the chosen game object as a child 
        selectedTerrain = param.getGameObjectParam();
        selectedTerrain.transform.SetParent(transform);

        // create teleport terrain as a duplication of the chosen terrain for teleportation
        teleportTerrain = new GameObject();
        teleportTerrain.name = "TeleportTerrain";
        teleportTerrain.transform.SetParent(gameObject.transform);

        // set the terrain scale back to 1/1
        subTerrainCount = selectedTerrain.transform.childCount;
        for (int i=0; i< subTerrainCount; i++)
        {
            selectedTerrain.transform.GetChild(i).position = new Vector3(0f, 0f, 0f); // conpensate the translation in lobby
            selectedTerrain.transform.GetChild(i).transform.localScale = scale1o1; // reset the terrain scale
            selectedTerrain.transform.GetChild(i).transform.tag = "TerrainMeasure"; // disable spawn point selection and enable measure events (cf MeasurePointerHandler.cs)

            // clone the terrain to set it as a teleport area
            cloneTPTerrain = Instantiate(selectedTerrain.transform.GetChild(i).gameObject, teleportTerrain.transform);
            cloneTPTerrain.AddComponent<TeleportArea>();
            cloneTPTerrain.GetComponent<MeshRenderer>().enabled = false; // don't display teleport area effect
            cloneTPTerrain.transform.Translate(new Vector3(0f, 0.01f, 0f)); // lift teleport area to put it properly above the terrain mesh collider
        }
        // set player initial position to the position of the previously clicked triangle
        spawnPoint = mesh.GetComponent<MeshFilter>().mesh.vertices[mesh.GetComponent<MeshFilter>().mesh.triangles[index*3]];
        player.transform.localPosition = spawnPoint;

        // make sure the terrain cannot be moved around while in immersion mode
        transform.GetComponent<Throwable>().attachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand;

        // send an event to the MeshLoader to destroy every remaining meshes
        EventManager.TriggerEvent("DestroyReaminingMeshes", null);
    }


    void SwitchBetweenImmersionAndModel(EventParam param)
    {
        subTerrainCount = selectedTerrain.transform.childCount;
        // param = 0 ->  switch from immersion to model mode
        if (param.getIntParam() == 0)
        {
            // save player position for later use
            playerLastPosition = player.transform.localPosition;
            // set player position 
            player.transform.localPosition = new Vector3(0f, -1.3f, -0.9f);

            // deactivate teleport terrain in model mode
            teleportTerrain.SetActive(false);

            // set terrain scale to 1/100
            transform.localScale = scale1o100;
            transform.localPosition = new Vector3(1, 0.9f, 0);

            // activate the interactable script to allow terrain transform modification
            transform.GetComponent<Interactable>().enabled = true;
            transform.GetComponent<Throwable>().attachmentFlags = Hand.AttachmentFlags.ParentToHand;
        }
        // param = 1 -> switch from model to immersion mode
        else
        {
            // activate teleport terrain in immersion mode
            teleportTerrain.SetActive(true);

            // set terrain scale to 1
            transform.localScale = scale1o1;
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;

            // restore previous player position on the terrain
            player.transform.localPosition = playerLastPosition;

            // deactivate the interactable script and deactivate terrain movements
            transform.GetComponent<Interactable>().enabled = false;
            transform.GetComponent<Throwable>().attachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand; // this flag don't allow movement
        }
    }

    void SaveSpawnPosition(EventParam param)
    {
        // saves the triangle index of the clicked mesh (game object)
        index = param.getIntParam();
        mesh = param.getGameObjectParam();
        EventManager.TriggerEvent("LobbyHintMessage", new EventParam("Un point de départ à bien été sélectionné sur le terrain."));
    }

    void BackToLobby(EventParam param)
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = scale1o1;
        player.transform.localPosition = new Vector3(0f, -1.3f, -1.9f);
        Destroy(teleportTerrain); // destroy the teleport area elements
        EventManager.TriggerEvent("TerrainFromSimulationToLobby", new EventParam(selectedTerrain));
    }
}
