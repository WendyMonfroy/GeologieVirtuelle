using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintDisplayManager : MonoBehaviour
{
    // recieve message events to display on the lobby interface
    void Start()
    {
        EventManager.StartListening("LobbyErrorMessage", DisplayErrorMessage);
        EventManager.StartListening("LobbyHintMessage", DisplayHintMessage);
    }

    void DisplayErrorMessage(EventParam message)
    {
        gameObject.GetComponent<Text>().color = new Color(238, 82, 83); //pas effectif
        SetText(message.getStringParam());
    }
    void DisplayHintMessage(EventParam message)
    {
        gameObject.GetComponent<Text>().color = new Color(200, 214, 229);
        SetText(message.getStringParam());
    }
    void SetText(string messageText)
    {
        gameObject.GetComponent<Text>().text = messageText;
    }
}
