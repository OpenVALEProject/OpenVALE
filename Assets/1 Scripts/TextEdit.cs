/* TextEdit.cs
 * 
 * This script provides handlers for the UI event system to call. This will set
 * the text accordingly to allow entering a PIN, then will return to a game 
 * controller when a valid PIN has been entered.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TextEdit : MonoBehaviour {
    public Text text;
    private string defaultText;
    public SocketCommunicationHandler sc;
    private void Start()
    {
        defaultText = text.text;
    }

    // sets text and default text to the string txt
    public void ChangeText(string txt)
    {
        text.text = txt;
        defaultText = txt;
    }

    private void ClearIfDefault()
    {
        if (text.text.Equals(defaultText))
            text.text = "";
    }

    public void AppendText(int num)
    {
        ClearIfDefault();
        if (text.text.Length < 4)
            text.text += num.ToString();
        else
            text.text = text.text.Remove(0,1) + num.ToString();
    }

    public void OnEnter()
    {
        if (text.text.Length != 4)
            return;
        string message = "getsubjectnumber," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE + "," + text.text;
        sc.sendMessage(message, ConfigurationUtil.waitingClient);
        sc.SubjectNumberCanvas.SetActive(false);
        ConfigurationUtil.waitingForSubjectNum = false;
        ConfigurationUtil.waitingClient = null;
    }
}
