using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveMeasuresScript : MonoBehaviour
{
    // variables
    private string saveDirectory = Directory.GetCurrentDirectory() + @"\Sauvegardes";
    private string saveSubDirectory;

    // save files
    StreamWriter marker;
    StreamWriter wulff;
    StreamWriter pathDistance;
    StreamWriter planeDistance;
    StreamWriter pointsAngle;
    StreamWriter planeAngle;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.StartListening("StartSimulation", SetSaveFolder);
        EventManager.StartListening("StartMarkerSave", InitSaving);
        EventManager.StartListening("SavingDone", CloseFiles);

        EventManager.StartListening("Save_Marker", SaveMarker);
        EventManager.StartListening("Save_Wulff", SaveWulff);
        EventManager.StartListening("Save_Path", SavePath);
        EventManager.StartListening("Save_PlaneDistance", SavePlaneDistance);
        EventManager.StartListening("Save_PointsAngle", SavePointsAngle);
        EventManager.StartListening("Save_PlaneAngle", SavePlaneAngle);

        if (!Directory.Exists(saveDirectory))
            Directory.CreateDirectory(saveDirectory);

    }

    // create saving directories if needed
    private void SetSaveFolder(EventParam param)
    {
        string terrainName = param.getGameObjectParam().name;

        if (!Directory.Exists(saveDirectory + @"\" + terrainName))
            Directory.CreateDirectory(saveDirectory + @"\" + terrainName);

        saveSubDirectory = saveDirectory + @"\" + terrainName;
        Debug.Log("chemin du dossier de sauvegarde : " + saveSubDirectory);

    }

    private void InitSaving(EventParam param)
    {
        // initialise save files
        marker = new StreamWriter(saveSubDirectory + @"\marker.txt", false);
        marker.AutoFlush = true;

        wulff = new StreamWriter(saveSubDirectory + @"\wulff.txt", false);
        wulff.AutoFlush = true;

        pathDistance = new StreamWriter(saveSubDirectory + @"\pathDistance.txt", false);
        pathDistance.AutoFlush = true;

        planeDistance = new StreamWriter(saveSubDirectory + @"\planeDistance.txt", false);
        planeDistance.AutoFlush = true;

        pointsAngle = new StreamWriter(saveSubDirectory + @"\pointsAngle.txt", false);
        pointsAngle.AutoFlush = true;

        planeAngle = new StreamWriter(saveSubDirectory + @"\planeAngle.txt", false);
        planeAngle.AutoFlush = true;
    }

    // safely close files when simulation stops
    private void CloseFiles(EventParam param)
    {
        marker.Close();
        wulff.Close();
        pathDistance.Close();
        planeDistance.Close();
        pointsAngle.Close();
        planeAngle.Close();
    }

    // write JSON stings in the proper file
    private void SaveMarker(EventParam param)
    {
        marker.WriteLine(param.getStringParam());
    }

    private void SaveWulff(EventParam param)
    {
        wulff.WriteLine(param.getStringParam());
    }

    private void SavePath(EventParam param)
    {
        pathDistance.WriteLine(param.getStringParam());
    }

    private void SavePlaneDistance(EventParam param)
    {
        planeDistance.WriteLine(param.getStringParam());
    }

    private void SavePointsAngle(EventParam param)
    {
        pointsAngle.WriteLine(param.getStringParam());
    }

    private void SavePlaneAngle(EventParam param)
    {
        planeAngle.WriteLine(param.getStringParam());
    }
}
