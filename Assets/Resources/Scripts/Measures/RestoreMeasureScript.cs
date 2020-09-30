using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RestoreMeasureScript : MonoBehaviour
{
    private string folderPath;
    private int loadDone;

    // save files
    FileStream marker;
    FileStream wulff;
    FileStream pathDistance;
    FileStream planeDistance;
    FileStream pointsAngle;
    FileStream planeAngle;

    // Start is called before the first frame update
    void Start()
    {
        loadDone = 0;

        EventManager.StartListening("RestoreMeasures", RestoreMeasureElements);
    }

    private void RestoreMeasureElements(EventParam pathParam)
    {
        folderPath = Directory.GetCurrentDirectory() + @"\Sauvegardes" + @"\" + pathParam.getStringParam();

        if (File.Exists(folderPath + @"\marker.txt"))
        {
            // open the proper files
            marker = File.OpenRead(folderPath + @"\marker.txt");
            wulff = File.OpenRead(folderPath + @"\wulff.txt");
            pathDistance = File.OpenRead(folderPath + @"\pathDistance.txt");
            planeDistance = File.OpenRead(folderPath + @"\planeDistance.txt");
            pointsAngle = File.OpenRead(folderPath + @"\pointsAngle.txt");
            planeAngle = File.OpenRead(folderPath + @"\planeAngle.txt");

            StartCoroutine("RestoreMarker");
            StartCoroutine("RestoreWulff");
            StartCoroutine("RestorePath");
            StartCoroutine("RestorePlaneDistance");
            StartCoroutine("RestorePointsAngle");
            StartCoroutine("RestorePlaneAngle");
        }
        else
        {
            EventManager.TriggerEvent("LoadingComplete", new EventParam("Pas de mesures à charger"));
        }
    }

    private void LoadDone()
    {
        loadDone += 1;

        // send event when loading is done
        if (loadDone == 6)
            EventManager.TriggerEvent("LoadingComplete", null);
    }

    IEnumerator RestoreMarker()
    {
        yield return new WaitForSeconds(3f);
        foreach (string line in File.ReadLines(folderPath+ @"\marker.txt"))
        {
            // send event to the Marker class
            EventManager.TriggerEvent("RestoreMarker", new EventParam(line));

            yield return new WaitForEndOfFrame();
        }
        marker.Close();
        LoadDone();
    }

    IEnumerator RestoreWulff()
    {
        yield return new WaitForSeconds(0.1f);
        foreach (string line in File.ReadLines(folderPath + @"\wulff.txt"))
        {
            // send event to the Marker class
            EventManager.TriggerEvent("RestoreWulff", new EventParam(line));

            yield return new WaitForEndOfFrame();
        }
        wulff.Close();
        LoadDone();
    }

    IEnumerator RestorePath()
    {
        yield return new WaitForSeconds(0.1f);
        foreach (string line in File.ReadLines(folderPath + @"\pathDistance.txt"))
        {
            // send event to the Marker class
            EventManager.TriggerEvent("RestorePath", new EventParam(line));

            yield return new WaitForEndOfFrame();
        }
        pathDistance.Close();
        LoadDone();
    }

    IEnumerator RestorePlaneDistance()
    {
        yield return new WaitForSeconds(0.1f);
        foreach (string line in File.ReadLines(folderPath + @"\planeDistance.txt"))
        {
            // send event to the Marker class
            EventManager.TriggerEvent("RestorePlaneDistance", new EventParam(line));

            yield return new WaitForEndOfFrame();
        }
        planeDistance.Close();
        LoadDone();
    }

    IEnumerator RestorePointsAngle()
    {
        yield return new WaitForSeconds(0.1f);
        foreach (string line in File.ReadLines(folderPath + @"\pointsAngle.txt"))
        {
            // send event to the Marker class
            EventManager.TriggerEvent("RestorePointsAngle", new EventParam(line));

            yield return new WaitForEndOfFrame();
        }
        pointsAngle.Close();
        LoadDone();
    }

    IEnumerator RestorePlaneAngle()
    {
        yield return new WaitForSeconds(0.1f);
        foreach (string line in File.ReadLines(folderPath + @"\planeAngle.txt"))
        {
            // send event to the Marker class
            EventManager.TriggerEvent("RestorePlaneAngle", new EventParam(line));

            yield return new WaitForEndOfFrame();
        }
        planeAngle.Close();
        LoadDone();
    }
}
