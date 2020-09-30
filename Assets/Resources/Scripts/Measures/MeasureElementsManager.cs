using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureElementsManager : MonoBehaviour
{
    // reference to the VR camera
    public Camera cam;

    private List<GameObject> measureElementParents;

    // save of the elements local scales
    private Vector3 markerScale;
    private Vector3 sphereScale;
    private Vector3 planeScale;
    private float lineDepth;
    private Vector3 wulffSelectionScale;


    // Start is called before the first frame update
    void Start()
    {
        measureElementParents = new List<GameObject>();

        markerScale = Vector3.one;
        sphereScale = Vector3.one;
        planeScale = Vector3.one;
        lineDepth = 0.03f;
        wulffSelectionScale = new Vector3(1.5f, 0.2f, 1.5f);

        // custom event when the simulation has started
        EventManager.StartListening("DestroyReaminingMeshes", SimulationStarted); 

        // events to switch between immersion and model modes
        EventManager.StartListening("SwitchBetweenImmersionAndModel", SwitchBetweenImmersionAndModel);
        EventManager.StartListening("BackToLobby", BackToLobby);
    }

    void SimulationStarted(EventParam param)
    {
        // get the measure elements parents after about a 1 second delay to ensure that the children list has been updated
        Invoke("GetMeasureElementParents", 0.8f);
    }

    void GetMeasureElementParents()
    {
        // get the measure elements parents
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name.Contains("Instance"))
            {
                measureElementParents.Add(transform.GetChild(i).gameObject);
            }
        }
    }

    void SwitchBetweenImmersionAndModel(EventParam intParam)
    {
        if (intParam.getIntParam() == 0)
        {
            // switch from imersion to model mode

            StartCoroutine("CheckCoroutine"); // start updating element size

        }
        else
        {
            // switch from model to imersion mode

            StopCoroutine("CheckCoroutine"); // stop updating element size

            // reset elements sizes
            for (int i=0; i<measureElementParents.Count; i++)
            {
                ResetScale(measureElementParents[i].transform);
            }

        }
    }

    void BackToLobby(EventParam param)
    {
        StopAllCoroutines();
    }

    void ResetScale(Transform parent)
    {
        // reset local scale of measure elements
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name.Contains("Line"))
            {
                parent.GetChild(i).GetComponent<LineRenderer>().widthMultiplier = lineDepth;
            }

            else if (parent.GetChild(i).name.Contains("Plane"))
            {
                parent.GetChild(i).transform.localScale = planeScale;
            }

            else if (parent.GetChild(i).name.Contains("Marker"))
            {
                parent.GetChild(i).transform.localScale = markerScale;
            }

            else if (parent.GetChild(i).name.Contains("Sphere"))
            {
                if (parent.GetChild(i).name.Contains("Wulff"))
                    parent.GetChild(i).transform.localScale = sphereScale * 0.5f;
                else
                    parent.GetChild(i).transform.localScale = sphereScale * 0.1f;
            }

            else if (parent.GetChild(i).name.Contains("Selection"))
            {
                parent.GetChild(i).transform.localScale = wulffSelectionScale;
            }
        }
    }

    // set local scale of measure elements according to their screen size
    void CheckSizeAndResize(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            // calculate the element screen size according to the element local size and terrain size
            Vector3 screenSize = cam.WorldToScreenPoint(parent.GetChild(i).transform.position) + (cam.ScreenToWorldPoint(Vector3.up) * parent.GetChild(i).transform.localScale.x * transform.localScale.x * 100) - cam.WorldToScreenPoint(parent.GetChild(i).transform.position);

            if (parent.GetChild(i).name.Contains("Line"))
            {
                parent.GetChild(i).GetComponent<LineRenderer>().widthMultiplier = 0.01f;
            }

            else if (parent.GetChild(i).name.Contains("Plane"))
            {
                if ((screenSize.y < 8 && screenSize.y > 1) || screenSize.y > 10)
                {
                    // update element scale if too small or too big
                    parent.GetChild(i).transform.localScale += Vector3.one * (10 - screenSize.y) / 100;
                }
                else if (screenSize.y <= 1)
                {
                    // reset scale when screen size go wrong
                    parent.GetChild(i).transform.localScale = planeScale;
                }
            }

            else if (parent.GetChild(i).name.Contains("Marker"))
            {
                screenSize = cam.WorldToScreenPoint(parent.GetChild(i).transform.position) + (cam.ScreenToWorldPoint(Vector3.up) * 2.2f * parent.GetChild(i).transform.localScale.y * transform.localScale.x * 100) - cam.WorldToScreenPoint(parent.GetChild(i).transform.position); // 2.2 factor for marker height

                if ((screenSize.y < 15 && screenSize.y > 0) || screenSize.y > 25)
                {
                    // update element scale if too small or too big
                    parent.GetChild(i).transform.localScale += Vector3.one * (20 - screenSize.y) / 100;
                }
                else if (screenSize.y <= 0)
                {
                    // reset scale when screen size go wrong
                    parent.GetChild(i).transform.localScale = markerScale * 5;
                }
            }

            else if (parent.GetChild(i).name.Contains("Sphere"))
            {
                if ((screenSize.y < 2 && screenSize.y > 1) || screenSize.y > 3)
                {
                    // update element scale if too small or too big
                    parent.GetChild(i).transform.localScale += Vector3.one * (3 - screenSize.y) / 100;
                }
                else if (screenSize.y <= 1)
                {
                    // reset scale when screen size go wrong
                    parent.GetChild(i).transform.localScale = sphereScale;
                }
            }

            else if (parent.GetChild(i).name.Contains("Selection"))
            {
                if ((screenSize.y < 5 && screenSize.y > 1) || screenSize.y > 7)
                {
                    // update element scale if too small or too big
                    parent.GetChild(i).transform.localScale += (Vector3.right * (6 - screenSize.y) / 100 + Vector3.forward * (6 - screenSize.y) / 100);
                }
                else if (screenSize.y <= 1)
                {
                    // reset scale when screen size go wrong
                    parent.GetChild(i).transform.localScale = wulffSelectionScale;
                }
            }
        }
    }

    IEnumerator CheckCoroutine()
    {
        for (; ; )
        {
            for (int i=0; i < measureElementParents.Count; i++)
            {
                CheckSizeAndResize(measureElementParents[i].transform);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
