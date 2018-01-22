using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum ManagedState
{
    SubID, PIN
}

public class Manager : MonoBehaviour
{
    public GameObject blackoutSphere;
    public TextEdit textEditor;
    public Canvas canvas;
    public Text errorText;
    public string nextScene;
    public bool getPin;

    // *** PIN/ID *** //
    private int number;  // used to store PIN to set subID and pin
    private int subID;
    private int pin;

    private ManagedState state;
    private BlackoutScript blkScript;

    private void Awake()
    {
        // setup event code
        blkScript = blackoutSphere.GetComponent<BlackoutScript>();
        blkScript.OnBlackedout += StartBlackedOutCoroutine;
    }

    void StartBlackedOutCoroutine()
    {
        StartCoroutine(OnBlackedout());
    }

    public void Start()
    {
        // set initial state
        state = ManagedState.SubID;
    }

    public void PinEntered(int num)
    {
        // this sphere has the blackout script on it...
        // this will return control here once it is finished
        blackoutSphere.SetActive(true);
        blkScript.Blackout();
        number = num;
    }

    private void Retry()
    {
        // change state
        state = ManagedState.SubID;

        textEditor.ChangeText("Enter Subject ID");
        errorText.enabled = true;

        // show scene again
        blkScript.UndoBlackout();
    }

    private void LoadNewScene()
    {
        if (true)  // dummy check for valid subID & PIN
        {
            // load new scene after setting PIN and subID on DDOL scene
            LoginInfo li = FindObjectOfType<LoginInfo>();
            li.Id = subID;
            li.Pin = pin;
            SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        }
        else
        {
            Retry();
        }
    }

    IEnumerator OnBlackedout()
    {
        // we have now finished blacking out, so we can do anything to 
        // stuff outside of the black sphere around the camera...

        if (state == ManagedState.SubID) // first time through
        {
            subID = number;

            if (!getPin)  // if we don't need the PIN, jump straight to next scene
                LoadNewScene();

            // setup the PIN UI
            textEditor.ChangeText("Enter Your PIN");

            state = ManagedState.PIN;

            // may have been enabled to alert user
            errorText.enabled = false;

            // pause for a second
            yield return new WaitForSeconds(1);

            // show scene again
            blkScript.UndoBlackout();
        }

        else if (state == ManagedState.PIN)
        {
            pin = number;

            // check the values
            LoadNewScene();
        }
    }
}
