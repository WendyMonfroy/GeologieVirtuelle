using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private Dictionary<string, Action<EventParam>> eventDictionary;
    private static EventManager eventManager;

    public static EventManager instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;
                if (!eventManager)
                {
                    Debug.LogError("There needs to be one active EventManager script on a GameObject in your scene.");
                }
                else
                {
                    eventManager.Init();
                }
            }
            return eventManager;
        }
    }

    void Init()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<string, Action<EventParam>>();
        }
    }

    public static void StartListening(string eventName, Action<EventParam> listener)
    {
        Action<EventParam> thisEvent;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            //Add more event to the existing one
            thisEvent += listener;

            //Update the Dictionary
            instance.eventDictionary[eventName] = thisEvent;
        }
        else
        {
            //Add event to the Dictionary for the first time
            thisEvent += listener;
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, Action<EventParam> listener)
    {
        if (eventManager == null) return;
        Action<EventParam> thisEvent;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            //Remove event from the existing one
            thisEvent -= listener;

            //Update the Dictionary
            instance.eventDictionary[eventName] = thisEvent;
        }
    }

    public static void TriggerEvent(string eventName, EventParam eventParam)
    {
        Action<EventParam> thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(eventParam);
            // OR USE  instance.eventDictionary[eventName](eventParam);
        }
    }
}

//Re-usable structure/ Can be a class to. Add all parameters you need inside it
public class EventParam
{
    private int intParam;
    private float floatParam;
    private string stringParam;
    private Vector3 pointParam;
    private GameObject gameobjectParam;

    public int getIntParam() { return intParam; }
    public float getFloatParam() { return floatParam; }
    public string getStringParam() { return stringParam; }
    public Vector3 getPointParam() { return pointParam;  }
    public GameObject getGameObjectParam() { return gameobjectParam; }

    public EventParam(int i) { intParam = i; }
    public EventParam(float f) { floatParam = f; }
    public EventParam(string s) { stringParam = s; }
    public EventParam(Vector3 p) { pointParam = p; }
    public EventParam(GameObject b) { gameobjectParam = b; }
    public EventParam(GameObject t, int i)
    {
        gameobjectParam = t;
        intParam = i;
    }

    public EventParam(GameObject t, int i, Vector3 p)
    {
        gameobjectParam = t;
        intParam = i;
        pointParam = p;
    }
}


