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
    public Button enter;
    public Button backspace;
    public Button[] numberButtons;
    public int minNumsRequired = 1;
    public int maxNumsRequired = 4;
    public SocketCommunicationHandler sc;

    private string defaultText;

    private void Start()
    {
        defaultText = text.text;
    }

    // sets text and default text to the string txt
    public void ChangeText(string txt)
    {
        text.text = txt;
        defaultText = txt;

        CheckButtonInteraction();
    }

    private void ClearIfDefault()
    {
        if (text.text.Equals(defaultText))
            text.text = "";
    }

    public void AppendText(int num)
    {
        ClearIfDefault();
        if (text.text.Length < maxNumsRequired)
            text.text += num.ToString();
        
        CheckButtonInteraction();
    }

    private void CheckButtonInteraction()
    {
        // backspace control - if there is text, enable it; empty? disable it
        if (text.text.Length > 0)
            backspace.interactable = true;
        else
            backspace.interactable = false;

        // enter control - if text length is between min and max, enable it
        //                 (max is already forced by UI)
        if (text.text.Length >= minNumsRequired)
            enter.interactable = true;
        else
            enter.interactable = false;

        if (text.text.Length >= maxNumsRequired)
            InteractWithNumberButtons(false);
        else
            InteractWithNumberButtons(true);

        if (defaultText.Contains(text.text))
        {  // e.g., "Enter Subject ID" text is (at least in part) visible
            backspace.interactable = false;
            enter.interactable = false;

            InteractWithNumberButtons(true);
        }
    }

    private void InteractWithNumberButtons(bool value)
    {
        foreach (Button b in numberButtons)
            b.interactable = value;
    }

    public void OnEnter()
    {
        // >>> start enter function code
        string message = "getsubjectnumber," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE + "," + text.text;
        sc.sendMessage(message, ConfigurationUtil.waitingClient);
        ChangeText("Enter Subject ID");
        sc.SubjectNumberCanvas.SetActive(false);
        ConfigurationUtil.waitingForSubjectNum = false;
        ConfigurationUtil.waitingClient = null;
        
        // <<< end enter function code 

        CheckButtonInteraction();
    }

    public void OnBackspace()
    {
        text.text = text.text.Remove(text.text.Length - 1);
        CheckButtonInteraction();
    }
}
