using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
public class GuessWho : MonoBehaviour
{
    public bool isWaitingForResponse = false;
    public TrialType trialType = 0;
    public bool giveFeedback = false;
    public int correctFacenumber = 0;
    public Vector3 correctPosition = Vector3.zero;
    public Socket waitingClient = null;
    public float trialStartTime = 0.0f;
    private Camera mainCamera;
    public GameObject crossHair;
    public GameObject panelBase;
    public GameObject responseOrbBase;
    public GameObject UIBase;
    private UIRotator UIComponent;
    private Vector3 centeredPosition;
    public GameObject centeredFace;
    public bool isMessageDisplayed = false;
    public GameObject shakeTest;
    private Color crossHairColor;
    public GameObject hiddenCrosshair;

    public enum TrialType : int
    {
        STARTUP_TRIAL = 0,
        LOCALIZE_LAST_VOICE = 4,		// Command syntax error
        LOCALIZE_BY_ID = 2,
        ID_BY_LOCATION = 1,
        ID_LAST_VOICE = 3,
        ALL_FINISHED = 5,
    };

    public Dictionary<TrialType, string> displayMessages = new Dictionary<TrialType, string>();

    public enum ErrorMessage : int
    {
        UNKNOWN_TRIAL_TYPE = 1,
        UNKNOWN_FEEDBACK_TYPE = 2,
        UNKNOWN_POSITION_VALUE = 3,
        UNKNOWN_FACE_VALUE = 4,
        UNKNOWN_CORRECT_VALUE = 5,
    };

    // Use this for initialization
    void Start()
    {
        mainCamera = Camera.main;
        UIComponent = UIBase.GetComponent<UIRotator>();
        displayMessages.Add(TrialType.STARTUP_TRIAL, "Find a comfortable upright position and pull the trigger to begin.");
        displayMessages.Add(TrialType.LOCALIZE_LAST_VOICE, "WHERE did the LAST sound come from?");
        displayMessages.Add(TrialType.LOCALIZE_BY_ID, "WHERE is this PERSON located?");
        displayMessages.Add(TrialType.ID_BY_LOCATION, "WHO is at this LOCATION?");
        displayMessages.Add(TrialType.ID_LAST_VOICE, "WHO did the LAST sound come from?");
        displayMessages.Add(TrialType.ALL_FINISHED, "Trials Finished.");
        centeredPosition = centeredFace.transform.position;
        hiddenCrosshair.SetActive(false);
        //crossHairColor = crossHair.GetComponent<Renderer>().material.GetColor("_Color");
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch));
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger)  || Input.GetKey(KeyCode.Space)) {
            hiddenCrosshair.SetActive(true);
            crossHair.SetActive(false);
            hiddenCrosshair.transform.SetPositionAndRotation(crossHair.transform.position, crossHair.transform.rotation);
        }
        else
        {
            hiddenCrosshair.SetActive(false);
            crossHair.SetActive(true);
        }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            triggerPressed();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            triggerPressed();
        }
    }
    void triggerPressed()
    {
        //shakeTest.GetComponent<ResponseOrb>().Shake();
        if (isWaitingForResponse)
        {
            bool returnHitInformation = false;
            string message = "GuessWho,0,";
            if (trialType == TrialType.STARTUP_TRIAL)
            {
                returnHitInformation = true;
            }
            else
            {
                //Ray r = new Ray(mainCamera.transform.position, crossHair.transform.position - mainCamera.transform.position);
                Ray r = new Ray(UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye), crossHair.transform.position - UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye));
                RaycastHit[] hits = Physics.RaycastAll(r, 5);
                float elapsedTime = Time.time - trialStartTime;
                message += elapsedTime + ",";                
                foreach (RaycastHit rh in hits)
                {
                    GameObject hitObject = rh.collider.gameObject;
                    if (hitObject.GetComponent<PanelDetails>() != null)
                    {
                        if (trialType == TrialType.LOCALIZE_LAST_VOICE || trialType == TrialType.LOCALIZE_BY_ID)
                            return;
                        message += hitObject.GetComponent<PanelDetails>().ID;
                        returnHitInformation = true;
                        isMessageDisplayed = false;

                        if (trialType == GuessWho.TrialType.ID_BY_LOCATION) {
                            SetHighlightedOrb(Vector3.zero);

                        }
                        if (giveFeedback)
                        {
                            panelBase.GetComponent<PanelControls>().getPanelByID(correctFacenumber).GetComponent<PanelDetails>().Shake();
                        }
                        break;
                    }
                    else if (hitObject.GetComponent<ResponseOrb>() != null)
                    {
                        if (trialType == TrialType.ID_BY_LOCATION || trialType == TrialType.ID_LAST_VOICE)
                            return;
                        //hitTarget = hitObject;
                        Vector3 position = hitObject.transform.position;
                        position = HelperFunctions.UnityXYZToFLT(position);
                        message += "[" + position.x + "," + position.y + "," + position.z + "]";
                        returnHitInformation = true;
                        isMessageDisplayed = false;
                        
                        if (giveFeedback)
                        {
                            responseOrbBase.GetComponent<ResponseOrbsController>().returnNearestOrb(correctPosition).GetComponent<ResponseOrb>().Shake();
                        }
                        break;
                    }
                }
            }
            if (!returnHitInformation)
                return;
            UIComponent.hideMessage();
            
            isWaitingForResponse = false;
            panelBase.GetComponent<PanelControls>().SetPanelFormatStandard();
            GetComponent<SocketCommunicationHandler>().sendMessage(message, waitingClient);
        }
    }

    public void DisplayMessage(TrialType tType)
    {
        isMessageDisplayed = true;
        UIComponent.setMessage(displayMessages[tType]);
    }

    public GameObject SetCenterFace(int id)
    {
        if (id < 0) {
            panelBase.GetComponent<PanelControls>().SetPanelFormatStandard();
            return null;
        }
        GameObject returned = panelBase.GetComponent<PanelControls>().SetPanelFormatCentered(id);
        if (returned != null)
            returned.transform.position = centeredFace.transform.position;
        return returned;
    }

    public GameObject SetHighlightedOrb(Vector3 location) {
        
        return responseOrbBase.GetComponent<ResponseOrbsController>().HighlightOrbByPosition(location);
    }
}