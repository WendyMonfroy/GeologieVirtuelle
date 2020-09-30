//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using UnityEngine;
using System.Collections;

namespace Valve.VR.Extras
{
    public class SteamVR_MeasureLaserPointer : MonoBehaviour
    {
        public SteamVR_Behaviour_Pose pose;

        // controler event for measure UI interaction
        public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractUI");

        // controler envent for measures
        public SteamVR_Action_Boolean placePointOnTerrain = SteamVR_Input.GetBooleanAction("PlacePoint");
        public SteamVR_Action_Boolean placePlaneOnTerrain = SteamVR_Input.GetBooleanAction("PlacePlane");
        public SteamVR_Action_Boolean placeMarker = SteamVR_Input.GetBooleanAction("PlaceMarker");
        public SteamVR_Action_Boolean placeWulffElement = SteamVR_Input.GetBooleanAction("PlaceWulffElement");

        // 
        public SteamVR_Action_Boolean grabElement = SteamVR_Input.GetBooleanAction("GrabGrip");
        // add a display results on pointer to show result of the targeted measure ?
        public SteamVR_Action_Boolean displayResults = SteamVR_Input.GetBooleanAction("DisplayResults");


        public bool active = true;
        public Color color;
        public float thickness = 0.002f;
        public Color clickColor = Color.green;
        public GameObject holder;
        public GameObject pointer;
        bool isActive = false;
        public bool addRigidBody = false;
        public Transform reference;

        // UI
        public event MeasurePointerEventHandler MeasurePointerIn;
        public event MeasurePointerEventHandler MeasurePointerOut;
        public event MeasurePointerEventHandler MeasurePointerClick;

        // measures
        public event MeasurePointerEventHandler PlacePointClick;
        public event MeasurePointerEventHandler PlacePlaneClick;
        public event MeasurePointerEventHandler PlaceMarkerClick;
        public event MeasurePointerEventHandler PlaceWulffElementClick;

        // grab
        public event MeasurePointerEventHandler GrabElementClick;
        public event MeasurePointerEventHandler ReleaseElement;

        // results
        public event MeasurePointerEventHandler DisplayTargetResultClick;

        // display request
        //public event MeasurePointerEventHandler DisplayResultsClick;

        Transform previousContact = null;


        private void Start()
        {
            if (pose == null)
                pose = this.GetComponent<SteamVR_Behaviour_Pose>();
            if (pose == null)
                Debug.LogError("No SteamVR_Behaviour_Pose component found on this object", this);

            if (interactWithUI == null)
                Debug.LogError("No ui interaction action has been set on this component.", this);


            holder = new GameObject();
            holder.transform.parent = this.transform;
            holder.transform.localPosition = Vector3.zero;
            holder.transform.localRotation = Quaternion.identity;

            pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pointer.transform.parent = holder.transform;
            pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
            pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
            pointer.transform.localRotation = Quaternion.identity;
            BoxCollider collider = pointer.GetComponent<BoxCollider>();
            if (addRigidBody)
            {
                if (collider)
                {
                    collider.isTrigger = true;
                }
                Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
                rigidBody.isKinematic = true;
            }
            else
            {
                if (collider)
                {
                    Object.Destroy(collider);
                }
            }
            Material newMaterial = new Material(Shader.Find("Unlit/Color"));
            newMaterial.SetColor("_Color", color);
            pointer.GetComponent<MeshRenderer>().material = newMaterial;
        }

        public virtual void OnMeasurePointerIn(MeasurePointerEventArgs e)
        {
            if (MeasurePointerIn != null)
                MeasurePointerIn(this, e);
        }

        public virtual void OnMeasurePointerClick(MeasurePointerEventArgs e)
        {
            if (MeasurePointerClick != null)
                MeasurePointerClick(this, e);
        }

        public virtual void OnMeasurePointerOut(MeasurePointerEventArgs e)
        {
            if (MeasurePointerOut != null)
                MeasurePointerOut(this, e);
        }

        // event for point placement 
        public virtual void OnPlacePointerClick(MeasurePointerEventArgs e)
        {
            if (PlacePointClick != null)
                PlacePointClick(this, e);
        }

        // event for plane placement 
        public virtual void OnPlacePlanePointerClick(MeasurePointerEventArgs e)
        {
            if (PlacePlaneClick != null)
                PlacePlaneClick(this, e);
        }

        // event for marker placement
        public virtual void OnPlaceMarkerClick(MeasurePointerEventArgs e)
        {
            if (PlaceMarkerClick != null)
                PlaceMarkerClick(this, e);
        }

        // event for Wulff element placement
        public virtual void OnPlaceWulffElementClick(MeasurePointerEventArgs e)
        {
            if (PlaceWulffElementClick != null)
                PlaceWulffElementClick(this, e);
        }

        // event to grab an element to move
        public virtual void OnGradElementClick(MeasurePointerEventArgs e)
        {
            if (GrabElementClick != null)
                GrabElementClick(this, e);
        }
        // event to release grabbed element
        public virtual void OnReleaseElement(MeasurePointerEventArgs e)
        {
            if (ReleaseElement != null)
                ReleaseElement(this, e);
        }

        // event to display the results of the target
        public virtual void OnDisplayResultClick(MeasurePointerEventArgs e)
        {
            if (DisplayTargetResultClick != null)
                DisplayTargetResultClick(this, e);
        }


        private void Update()
        {
            if (!isActive)
            {
                isActive = true;
                this.transform.GetChild(0).gameObject.SetActive(true);
            }

            float dist = 100f;

            Ray raycast = new Ray(transform.position, transform.forward);
            RaycastHit hit;
            bool bHit = Physics.Raycast(raycast, out hit);

            if (previousContact && previousContact != hit.transform)
            {
                MeasurePointerEventArgs args = new MeasurePointerEventArgs();
                args.fromInputSource = pose.inputSource;
                args.distance = 0f;
                args.point = new Vector3(0, 0, 0);
                args.flags = 0;
                args.triangleIndex = hit.triangleIndex;
                args.target = previousContact;
                OnMeasurePointerOut(args);
                previousContact = null;
            }
            if (bHit && previousContact != hit.transform)
            {
                MeasurePointerEventArgs argsIn = new MeasurePointerEventArgs();
                argsIn.fromInputSource = pose.inputSource;
                argsIn.distance = hit.distance;
                argsIn.point = hit.point;
                argsIn.flags = 0;
                argsIn.triangleIndex = hit.triangleIndex;
                argsIn.target = hit.transform;
                OnMeasurePointerIn(argsIn);
                previousContact = hit.transform;
            }
            if (!bHit)
            {
                previousContact = null;
            }
            if (bHit && hit.distance < 100f)
            {
                dist = hit.distance;
            }

            // UI click event
            if (bHit && interactWithUI.GetStateDown(pose.inputSource))
            {
                MeasurePointerEventArgs argsClick = new MeasurePointerEventArgs();
                argsClick.fromInputSource = pose.inputSource;
                argsClick.distance = hit.distance;
                argsClick.point = hit.point;
                argsClick.flags = 0;
                argsClick.triangleIndex = hit.triangleIndex;
                argsClick.target = hit.collider.transform;
                OnMeasurePointerClick(argsClick);
            }

            // PlacePoint click event
            if (bHit && placePointOnTerrain.GetStateDown(pose.inputSource))
            {
                MeasurePointerEventArgs argsPointClick = new MeasurePointerEventArgs();
                argsPointClick.fromInputSource = pose.inputSource;
                argsPointClick.distance = hit.distance;
                argsPointClick.point = hit.point;
                argsPointClick.flags = 0;
                argsPointClick.triangleIndex = hit.triangleIndex;
                argsPointClick.target = hit.collider.transform;
                OnPlacePointerClick(argsPointClick);
            }

            // PlacePlane click event 
            if (bHit && placePlaneOnTerrain.GetStateDown(pose.inputSource))
            {
                MeasurePointerEventArgs argsPlaneClick = new MeasurePointerEventArgs();
                argsPlaneClick.fromInputSource = pose.inputSource;
                argsPlaneClick.distance = hit.distance;
                argsPlaneClick.point = hit.point;
                argsPlaneClick.flags = 0;
                argsPlaneClick.triangleIndex = hit.triangleIndex;
                argsPlaneClick.target = hit.collider.transform;
                OnPlacePlanePointerClick(argsPlaneClick);
            }

            // PlaceMarker click event
            if (bHit && placeMarker.GetStateUp(pose.inputSource))
            {
                MeasurePointerEventArgs argsMarkerClick = new MeasurePointerEventArgs();
                argsMarkerClick.fromInputSource = pose.inputSource;
                argsMarkerClick.distance = hit.distance;
                argsMarkerClick.point = hit.point;
                argsMarkerClick.flags = 0;
                argsMarkerClick.triangleIndex = hit.triangleIndex;
                argsMarkerClick.target = hit.collider.transform;
                OnPlaceMarkerClick(argsMarkerClick);
            }

            // PlaceWulffElement click event
            if (bHit && placeWulffElement.GetStateUp(pose.inputSource))
            {
                MeasurePointerEventArgs argsWulffClick = new MeasurePointerEventArgs();
                argsWulffClick.fromInputSource = pose.inputSource;
                argsWulffClick.distance = hit.distance;
                argsWulffClick.point = hit.point;
                argsWulffClick.flags = 0;
                argsWulffClick.triangleIndex = hit.triangleIndex;
                argsWulffClick.target = hit.collider.transform;
                OnPlaceWulffElementClick(argsWulffClick);
            }

            // Grab element click event
            if (bHit && grabElement.GetStateDown(pose.inputSource))
            {
                MeasurePointerEventArgs argsGrabClick = new MeasurePointerEventArgs();
                argsGrabClick.fromInputSource = pose.inputSource;
                argsGrabClick.distance = hit.distance;
                argsGrabClick.point = hit.point;
                argsGrabClick.flags = 0;
                argsGrabClick.triangleIndex = hit.triangleIndex;
                argsGrabClick.target = hit.collider.transform;
                OnGradElementClick(argsGrabClick);
            }
            if (bHit && grabElement.GetStateUp(pose.inputSource))
            {
                MeasurePointerEventArgs argsGrabRelease = new MeasurePointerEventArgs();
                argsGrabRelease.fromInputSource = pose.inputSource;
                argsGrabRelease.distance = hit.distance;
                argsGrabRelease.point = hit.point;
                argsGrabRelease.flags = 0;
                argsGrabRelease.triangleIndex = hit.triangleIndex;
                argsGrabRelease.target = hit.collider.transform;
                OnReleaseElement(argsGrabRelease);
            }

            // DisplayResults click event
            if (bHit && displayResults.GetStateUp(pose.inputSource))
            {
                MeasurePointerEventArgs argsResultClick = new MeasurePointerEventArgs();
                argsResultClick.fromInputSource = pose.inputSource;
                argsResultClick.distance = hit.distance;
                argsResultClick.point = hit.point;
                argsResultClick.flags = 0;
                argsResultClick.triangleIndex = hit.triangleIndex;
                argsResultClick.target = hit.collider.transform;
                OnDisplayResultClick(argsResultClick);
            }

            if (interactWithUI != null && interactWithUI.GetState(pose.inputSource))
            {
                pointer.transform.localScale = new Vector3(thickness * 2f, thickness * 2f, dist);
                pointer.GetComponent<MeshRenderer>().material.color = clickColor;
            }
            else
            {
                pointer.transform.localScale = new Vector3(thickness, thickness, dist);
                pointer.GetComponent<MeshRenderer>().material.color = color;
            }
            pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);
        }
    }

    public struct MeasurePointerEventArgs
    {
        public SteamVR_Input_Sources fromInputSource;
        public uint flags;
        public int triangleIndex;
        public float distance;
        public Vector3 point;
        public Transform target;
    }

    public delegate void MeasurePointerEventHandler(object sender, MeasurePointerEventArgs e);
}