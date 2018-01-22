/*
 * Used to communicate with the external Audio Spatialization Tools
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;
using System.Xml.XPath;
using UnityEngine.EventSystems;

public class SLABCommunication : MonoBehaviour
{
    
	public static int port = 1112;
	private static TcpClient slabConnection;
    private static UdpClient slabUDPConnection;
    private static NetworkStream slabStream;
    private static NetworkStream slabUDPStream;
    private static Process slabProcess;
	public float soundDelay = 1.1f;
	public float nextSound = 0.0f;
	private bool mute = true;
	private GameObject soundSource;
	private List<GameObject> soundSourceList;
	private bool init = true;
	public bool triggerFeedback = false;
	private bool feedbackFinished = true;
	private static string HRTFDir = ConfigurationUtil.HRTFDir;
	private static string wavDir = ConfigurationUtil.wavDir;
	private static string wavName = ConfigurationUtil.wavName;
	private static string outDevice = ConfigurationUtil.IODevice;
	private static string channelMap = ConfigurationUtil.channelMap;
	private static string outChannelMap = ConfigurationUtil.outChannelMap;
	private static string FIRTaps = ConfigurationUtil.FIRTaps;
    public GameObject cameraTransform;
    public GameObject joystickCam;
    public GameObject occCam;
    private string spatialAudioServerDirectory = ConfigurationUtil.spatialAudioServer;
    public bool isRendering = false;
    private GameObject currentHighlightedObject = null;
    public GameObject crossHair;
    public float crossHairDepth;
    public float viveCrossHairDepth;
    public GameObject panelBase;
    public static Dictionary<int, SourceInformation> currentSources = new Dictionary<int, SourceInformation>();
    private bool enteredTolerance = false;
    private float toleranceTime = 0.0f;
    public Transform VIVEOffset;

	// Use this for initialization
	void Start()
	{
        if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer)
        {
            foreach (var process in Process.GetProcessesByName("WinAudioServer"))
            {
                process.Kill();
            }
            UnityEngine.Debug.Log(spatialAudioServerDirectory + "\\WinAudioServer.exe");
            slabProcess = new Process();
            slabProcess.StartInfo.FileName = spatialAudioServerDirectory + "\\WinAudioServer.exe";
            slabProcess.StartInfo.WorkingDirectory = spatialAudioServerDirectory + "\\";
        }
        else if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3) {
            if (ConfigurationUtil.spatialAudioServer[0].Equals('.'))
            {
                ConfigurationUtil.spatialAudioServer= ConfigurationUtil.spatialAudioServer.TrimStart('.');
                ConfigurationUtil.spatialAudioServer= ConfigurationUtil.spatialAudioServer.TrimStart('\\');
                ConfigurationUtil.spatialAudioServer= Path.Combine(System.Environment.CurrentDirectory, ConfigurationUtil.spatialAudioServer);
            }
            spatialAudioServerDirectory = ConfigurationUtil.spatialAudioServer;
            
            slabProcess = new Process();
            slabProcess.StartInfo.FileName = spatialAudioServerDirectory + "\\AudioServer3.exe";
            slabProcess.StartInfo.WorkingDirectory = spatialAudioServerDirectory + "\\";
        }
        slabProcess.Start();

		Thread.Sleep(500);
        while (true)
        {
            try
            {
                slabConnection = new TcpClient("127.0.0.1", port);
                if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3)
                    slabUDPConnection = new UdpClient("127.0.0.1", 11000);
            }
            catch (System.Exception e) {
                continue;
            
            }
            if (slabConnection.Connected)
            {
                UnityEngine.Debug.Log("Connected");
                LogSystem.Log("Connected to IPSS");
                break;

            }

        }

		slabStream = slabConnection.GetStream();
        

        Thread.Sleep(2000);
        if (HRTFDir[0].Equals('.'))
        {
            HRTFDir= HRTFDir.TrimStart('.');
            HRTFDir = HRTFDir.TrimStart('\\');
            HRTFDir = Path.Combine(System.Environment.CurrentDirectory, HRTFDir);
        }
        if (wavDir[0].Equals('.'))
        {
            wavDir = wavDir.TrimStart('.');
            wavDir = wavDir.TrimStart('\\');
            wavDir = Path.Combine(System.Environment.CurrentDirectory, wavDir);
        }
        if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer)
        {
            sendMessageToSlab("setHRTFPath(" + HRTFDir + ")");
            sendMessageToSlab("defineASIOOutChMap(" + channelMap + " )");
            sendMessageToSlab("defineASIOChMap(" + outChannelMap + ")");
            if (!outDevice.Equals(""))
            {
                UnityEngine.Debug.Log("ASIO");
                sendMessageToSlab("selectOutDevice(" + outDevice + ")");
            }
            sendMessageToSlab("setWavePath(" + wavDir + ")");
            sendMessageToSlab("setFIRTaps(" + FIRTaps + ")");
        }
        else if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3)
        {
            
            sendMessageToSlab("hrtfPath " + HRTFDir + "");
            if (!outDevice.Equals(""))
            {
                UnityEngine.Debug.Log("ASIO");
            }
            sendMessageToSlab("defAsioInChMap " + channelMap + "");
            sendMessageToSlab("wavePath " + wavDir + "");
            string slabDirectory = Path.Combine(spatialAudioServerDirectory, "slab3d");
            UnityEngine.Debug.Log(sendMessageToSlab("slabRoot " + slabDirectory));
            sendMessageToSlab("binPath bin");
            sendMessageToSlab("setFirTaps " + FIRTaps + "");
        }
    }

    public void Reset() {
        if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer)
        {
            sendMessageToSlab("freeSources()");
            Thread.Sleep(500);
            sendMessageToSlab("setHRTFPath(" + HRTFDir + ")");
            sendMessageToSlab("setWavePath(" + wavDir + ")");
            sendMessageToSlab("setFIRTaps(" + FIRTaps + ")");
        }
        else if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3) {
            string slabResponse = "";
            slabResponse = sendMessageToSlab("isRendering");
            if (slabResponse.Trim().Trim(';').Split(':')[1].Trim().Equals("0")) {
                return;
            }
            sendMessageToSlab("Stop");
            Thread.Sleep(1000);
            do
            {
                slabResponse = sendMessageToSlab("isRendering");
                Thread.Sleep(1000);
            }
            while (slabResponse.Trim().Trim(';').Split(':')[1].Trim().Equals("1"));

            sendMessageToSlab("Start");
            Thread.Sleep(500);
            sendMessageToSlab("hrtfPath " + HRTFDir + "");
            sendMessageToSlab("defAsioInChMap " + channelMap + "");
            sendMessageToSlab("wavePath " + wavDir + "");
            string slabDirectory = Path.Combine(spatialAudioServerDirectory, "slab3d");

            sendMessageToSlab("slabRoot " + slabDirectory);
            sendMessageToSlab("binPath bin");
            sendMessageToSlab("setFirTaps " + FIRTaps + "");
    }

        currentSources = new Dictionary<int, SourceInformation>();
    }

    public static void StartAudioEngine() {
        if(slabProcess != null && !slabProcess.HasExited )
            slabProcess.Kill();
        
        if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer)
        {
            foreach (var process in Process.GetProcessesByName("WinAudioServer"))
            {
                process.Kill();
            }
            UnityEngine.Debug.Log(ConfigurationUtil.spatialAudioServer + "\\WinAudioServer.exe");
            slabProcess = new Process();
            slabProcess.StartInfo.FileName = ConfigurationUtil.spatialAudioServer + "\\WinAudioServer.exe";
            slabProcess.StartInfo.WorkingDirectory = ConfigurationUtil.spatialAudioServer + "\\";
        }
        else if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3)
        {
            UnityEngine.Debug.Log(ConfigurationUtil.spatialAudioServer + "AudioServer3.exe");
            slabProcess = new Process();
            slabProcess.StartInfo.FileName = ConfigurationUtil.spatialAudioServer + "\\AudioServer3.exe";
            slabProcess.StartInfo.WorkingDirectory = ConfigurationUtil.spatialAudioServer + "\\";
        }
        slabProcess.Start();
        Thread.Sleep(500);
        while (true)
        {
            try
            {
                slabConnection = new TcpClient("127.0.0.1", port);
                slabUDPConnection = new UdpClient("127.0.0.1", 11000);
            }
            catch (System.Exception e)
            {
                continue;

            }
            if (slabConnection.Connected)
            {
                UnityEngine.Debug.Log("Connected");
                LogSystem.Log("Connected to IPSS");
                break;
            }
        }

        slabStream = slabConnection.GetStream();
        Thread.Sleep(2000);
        sendMessageToSlab("setHRTFPath(" + HRTFDir + ")");
        sendMessageToSlab("defineASIOOutChMap(" + channelMap + " )");
        sendMessageToSlab("defineASIOChMap(" + outChannelMap + ")");
        if (!outDevice.Equals(""))
        {
            sendMessageToSlab("selectOutDevice(" + outDevice + ")");
        }
        sendMessageToSlab("setWavePath(" + wavDir + ")");
        sendMessageToSlab("setFIRTaps(" + FIRTaps + ")");
        currentSources = new Dictionary<int, SourceInformation>();
        
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();  
            #endif
        }
        GameObject camera;
        float yaw;
        float pitch;
        float roll;
        camera = joystickCam;

        if (!ConfigurationUtil.useRift && !ConfigurationUtil.useVive)
        {


            if (Input.GetKey(KeyCode.A))
            {
                camera.transform.Rotate(Vector3.up, -.1f);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                camera.transform.Rotate(Vector3.up, .1f);
            }
            else if (Input.GetKey(KeyCode.W))
            {
                camera.transform.Rotate(Vector3.right, -.1f);

            }
            else if (Input.GetKey(KeyCode.S))
            {
                camera.transform.Rotate(Vector3.right, .1f);

            }

            //GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");

            roll = camera.transform.rotation.eulerAngles.z;
            while (roll > 180)
            {
                roll = roll - 360;

            }
            while (roll < -180)
            {
                roll = roll + 360;

            }
            pitch = camera.transform.rotation.eulerAngles.x;
            while (pitch > 180)
            {
                pitch = pitch - 360;

            }
            while (pitch < -180)
            {
                pitch = pitch + 360;

            }
            yaw = camera.transform.rotation.eulerAngles.y;
            while (yaw > 180)
            {
                yaw = yaw - 360;

            }
            while (yaw < -180)
            {
                yaw = yaw + 360;

            }
            if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer)
                sendMessageToSlab("setListenerPosition(" + yaw + "," + -1 * pitch + "," + -1 * roll + ")");
            else if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3)
                sendMessageToSlab("updateLstOrientation " + yaw + "," + -pitch + "," + -roll);
        }
        else
        {
            Vector3 orientationVector = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head).eulerAngles;

            roll = orientationVector.z;
            while (roll > 180)
            {
                roll = roll - 360;

            }
            while (roll < -180)
            {
                roll = roll + 360;

            }
            pitch = orientationVector.x;
            while (pitch > 180)
            {
                pitch = pitch - 360;

            }
            while (pitch < -180)
            {
                pitch = pitch + 360;

            }
            yaw = orientationVector.y;
            while (yaw > 180)
            {
                yaw = yaw - 360;

            }
            while (yaw < -180)
            {
                yaw = yaw + 360;

            }

            Vector3 cameraPosition = VIVEOffset.position + UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head);
            if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer)
            {
                sendMessageToSlab("setListenerPosition(" + yaw + "," + -1 * pitch + "," + -1 * roll + ")");

                Vector3 relativePosition = Vector3.zero;
                foreach (SourceInformation S in currentSources.Values)
                {
                    relativePosition = S.FLTPosition - HelperFunctions.UnityXYZToFLT(cameraPosition);
                    sendMessageToSlab("updateSource(" + S.sourceID + "," + relativePosition.x + "," + relativePosition.y + "," + relativePosition.z);
                }
            }
            else if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3)
            {
                Vector3 FLTCameraPosition = HelperFunctions.UnityXYZToFLT(cameraPosition);
                sendMessageToSlab("updateLst6DOF " + FLTCameraPosition.x + "," + -FLTCameraPosition.y + "," + FLTCameraPosition.z + "," + yaw + ", " + -pitch + ", " + -roll);
            }
            if (currentHighlightedObject != null)
            {
                currentHighlightedObject.GetComponent<LEDControls>().HighlightLEDs(false, false, false, false);

            }
        }

        if (ConfigurationUtil.currentCursorAttachment == ConfigurationUtil.CursorAttachment.hand) {

            if (ConfigurationUtil.currentCursorType == ConfigurationUtil.CursorType.crosshair)
            {
                if (!crossHair.activeSelf)
                {
                    crossHair.SetActive(true);
                }

                // Vector3 camerPosition = camera.transform.position;
                // Vector3 cameraForward = camera.transform.forward;

                
                if (ConfigurationUtil.useRift)
                {
                    crossHair.transform.position = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
                    crossHair.transform.position = crossHair.transform.position + ((OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward).normalized) * crossHairDepth;
                    
                }
                else if (ConfigurationUtil.useVive)
                {
                    crossHair.transform.position = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye) + VIVEOffset.position;
                    crossHair.transform.position = crossHair.transform.position + ((SteamVR_Controller.Input(3).transform.rot * Vector3.forward).normalized * viveCrossHairDepth);
                    //UnityEngine.Debug.Log("Crosshair transform : " + crossHair.transform.position);
                }
                crossHair.transform.LookAt(crossHair.transform.position + (UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye) * Vector3.forward), UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye) * Vector3.up);
                crossHair.transform.Rotate(Vector3.right, -90);
           
            }
            else if (ConfigurationUtil.currentCursorType == ConfigurationUtil.CursorType.snapped)
            {
                Vector3 intersectionLocation = Vector3.zero;
                if (ConfigurationUtil.useRift)
                {
                    intersectionLocation = (OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward).normalized * 2.08f;
                }
                if (ConfigurationUtil.useVive) {
                    intersectionLocation = (SteamVR_Controller.Input(3).transform.rot * Vector3.forward).normalized * 2.08f;
                }
                currentHighlightedObject = GetComponent<ALFLeds>().getNearestSpeaker(intersectionLocation);
                if (currentHighlightedObject != null)
                {
                    currentHighlightedObject.GetComponent<LEDControls>().HighlightLEDs(true, true, true, true);
                }
            }
        }
   
        if (ConfigurationUtil.currentCursorAttachment == ConfigurationUtil.CursorAttachment.hmd) {
            
            if (ConfigurationUtil.currentCursorType == ConfigurationUtil.CursorType.crosshair) {
                if (!crossHair.activeSelf) {
                    crossHair.SetActive(true);
                }
                crossHair.transform.position = camera.transform.position;
                crossHair.transform.position = transform.position + camera.transform.forward * crossHairDepth;
                crossHair.transform.LookAt(crossHair.transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation * Vector3.up);
                crossHair.transform.Rotate(Vector3.right, -90);
            }
            else if(ConfigurationUtil.currentCursorType == ConfigurationUtil.CursorType.snapped)
            {
                
                Vector3 intersectionLocation = camera.transform.forward.normalized * 2.08f;
                currentHighlightedObject = GetComponent<ALFLeds>().getNearestSpeaker(intersectionLocation);
                if (currentHighlightedObject != null)
                {
                    currentHighlightedObject.GetComponent<LEDControls>().HighlightLEDs(true, true, true, true);
                }
            }

        }
        if (ConfigurationUtil.waitingForRecenter) {
            Vector3 targetAlignment = (ConfigurationUtil.recenterPosition - camera.transform.position).normalized;
            UnityEngine.Debug.Log(camera.transform.forward);
            if (Mathf.Acos(Vector3.Dot(camera.transform.forward, targetAlignment)) < ConfigurationUtil.recenterTolerance)
            {
                
                if (!enteredTolerance)
                {
                    enteredTolerance = true;
                    toleranceTime = Time.time;
                    //UnityEngine.Debug.Log("*/ in!");
                }
                if (ConfigurationUtil.waitingForResponse)
                {
                    if (ConfigurationUtil.useRift && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
                    {
                        ConfigurationUtil.waitingForResponse = false;
                    }
                    else if (ConfigurationUtil.useVive && SteamVR_Controller.Input(3).GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                    {
                        ConfigurationUtil.waitingForResponse = false;
                    }
                    else if (!ConfigurationUtil.useRift && !ConfigurationUtil.useVive && Input.GetKeyDown(KeyCode.Space))
                    {
                        ConfigurationUtil.waitingForResponse = false;
                    }
                    else
                        return;

                }
                else {
                    if ((Time.time - toleranceTime) > 0.5f)
                    {
                        enteredTolerance = false;
                        if (Mathf.Acos(Vector3.Dot(camera.transform.forward, targetAlignment)) > ConfigurationUtil.recenterTolerance)
                        {
                            return;
                        }
                       
                    }
                    else {

                        return;
                    }
                    
                }
                    
                string message = "waitForRecenter," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE + ","+ (Time.time - ConfigurationUtil.waitStartTime);
                GetComponent<SocketCommunicationHandler>().sendMessage(message, ConfigurationUtil.waitingClient);
                ConfigurationUtil.waitingForRecenter = false;
                ConfigurationUtil.recenterTolerance = 0;
                ConfigurationUtil.recenterPosition = new Vector3(0, 0, 0);
                ConfigurationUtil.waitingClient = null;
            }

            return;


        }
        

        
        if (ConfigurationUtil.useRift && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)){
            TriggerPressed();
        }
        else if (ConfigurationUtil.useVive && SteamVR_Controller.Input(3).GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {            
            TriggerPressed();
        }
        else if (!ConfigurationUtil.useRift && !ConfigurationUtil.useVive &&Input.GetKeyDown(KeyCode.Space)) {
            TriggerPressed();
        if (ConfigurationUtil.useRift ){
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                TriggerPressed();
            }
            if (OVRInput.GetDown(OVRInput.Button.Two)) {

                EscapeButtonPressed();
            }
        }
        else if (ConfigurationUtil.useVive )
        {
            if (SteamVR_Controller.Input(3).GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                TriggerPressed();
            }
            if (SteamVR_Controller.Input(3).GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
            {
                EscapeButtonPressed();
            }
        }
        else if (!ConfigurationUtil.useRift && !ConfigurationUtil.useVive ) {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TriggerPressed();
            }
            if (Input.GetKeyDown(KeyCode.Q)) {
                EscapeButtonPressed();
            }
         }
        
	}
    private void TriggerPressed() {

        if (ConfigurationUtil.waitingForSubjectNum) {
            Vector3 origin = Vector3.zero;
            Vector3 toDirection = Vector3.zero;
            if (!ConfigurationUtil.useRift && !ConfigurationUtil.useVive && Input.GetKeyDown(KeyCode.Space)) {
                origin = Vector3.zero;
                toDirection = Camera.main.transform.forward;
                
            }
            else if (ConfigurationUtil.useRift || ConfigurationUtil.useVive)
            {
                if (ConfigurationUtil.currentCursorAttachment == ConfigurationUtil.CursorAttachment.hand)
                {
                    if (ConfigurationUtil.currentCursorType == ConfigurationUtil.CursorType.crosshair)
                    {
                        if (ConfigurationUtil.useRift)
                        {
                            origin = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
                            toDirection = (crossHair.transform.position - UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye)).normalized * 2.08f;
                        }
                        else if (ConfigurationUtil.useVive)
                        {
                            origin = VIVEOffset.position + UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
                            toDirection = (crossHair.transform.position - (VIVEOffset.position + UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye))).normalized * 2.08f;
                          

                        }
                    }
                }
                else if (ConfigurationUtil.currentCursorAttachment == ConfigurationUtil.CursorAttachment.hmd)
                {
                    if (ConfigurationUtil.currentCursorType != ConfigurationUtil.CursorType.crosshair)
                    {
                        origin = VIVEOffset.position + UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
                        toDirection = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye) * Vector3.forward;
                    }
                    else
                    {
                        origin = VIVEOffset.position + UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
                        toDirection = (crossHair.transform.position - (VIVEOffset.position + UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye))).normalized;
                    }
                }
            }
            Ray r = new Ray(origin, toDirection);
            RaycastHit[] hits = Physics.RaycastAll(r, 50);
            UnityEngine.UI.Button possibleButton;
            foreach (RaycastHit RCH in hits)
            {
                possibleButton = RCH.collider.gameObject.GetComponent<UnityEngine.UI.Button>();
                if (possibleButton != null)
                {
                    possibleButton.onClick.Invoke();
                }
            }
        }
        if (ConfigurationUtil.waitingForResponse)
        {

            string message = "waitForResponse," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE + ",";
            Vector3 intersectionPoint = Vector3.zero;
            if (ConfigurationUtil.useRift || ConfigurationUtil.useVive) {
                if (ConfigurationUtil.currentCursorAttachment == ConfigurationUtil.CursorAttachment.hand)
                {
                    if (ConfigurationUtil.currentCursorType == ConfigurationUtil.CursorType.snapped)
                    {
                        if (ConfigurationUtil.useRift)
                        {
                            intersectionPoint = (OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward).normalized * 2.08f;
                        }
                        else if (ConfigurationUtil.useVive) {
                            intersectionPoint = (SteamVR_Controller.Input(3).transform.rot * Vector3.forward).normalized * 2.08f;
                        }
                    }
                    else
                        intersectionPoint = (crossHair.transform.position - UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye)).normalized * 2.08f;
                }
                else if (ConfigurationUtil.currentCursorAttachment == ConfigurationUtil.CursorAttachment.hmd)
                {
                    if (ConfigurationUtil.currentCursorType == ConfigurationUtil.CursorType.snapped)
                        intersectionPoint = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye) * Vector3.forward * 2.08f;
                    else
                        intersectionPoint = (crossHair.transform.position - UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye)).normalized * 2.08f;
                }
            }
            else
            {
                intersectionPoint = Camera.main.transform.forward.normalized * 2.08f;
            }
            string spkID = GetComponent<ALFLeds>().getNearestSpeakerID(intersectionPoint);
            message += spkID;
            message += ",";
            float respTime = Time.time - ConfigurationUtil.waitStartTime;
            message += respTime + "";

            GetComponent<SocketCommunicationHandler>().sendMessage(message, ConfigurationUtil.waitingClient);
            ConfigurationUtil.waitingForResponse = false;
            ConfigurationUtil.waitingClient = null;
            ConfigurationUtil.waitStartTime = 0.0f;

        }
    }


	

	void OnDestroy()
	{

		slabProcess.Kill();

	}
    public Vector3 getListenerOrientation() {
        GameObject camera;
        if (!ConfigurationUtil.useRift)
        {
            camera = joystickCam;
        }
        else
        {
            camera = occCam;
        }
        float roll = camera.transform.rotation.eulerAngles.z;
        while (roll > 180)
        {
            roll = roll - 360;
        }
        while (roll < -180)
        {
            roll = roll + 360;
        }
        float pitch = camera.transform.rotation.eulerAngles.x;
        while (pitch > 180)
        {
            pitch = pitch - 360;
        }
        while (pitch < -180)
        {
            pitch = pitch + 360;
        }
        float yaw = camera.transform.rotation.eulerAngles.y;
        while (yaw > 180)
        {
            yaw = yaw - 360;
        }
        while (yaw < -180)
        {
            yaw = yaw + 360;
        }
        return new Vector3(yaw, -1 * pitch, -1 * roll);
    }
    public static Vector3 getObjectOrientation(GameObject g)
    {
        GameObject camera;
        camera = g;
        float roll = camera.transform.rotation.eulerAngles.z;
        while (roll > 180)
        {
            roll = roll - 360;
        }
        while (roll < -180)
        {
            roll = roll + 360;
        }
        float pitch = camera.transform.rotation.eulerAngles.x;
        while (pitch > 180)
        {
            pitch = pitch - 360;
        }
        while (pitch < -180)
        {
            pitch = pitch + 360;
        }
        float yaw = camera.transform.rotation.eulerAngles.y;
        while (yaw > 180)
        {
            yaw = yaw - 360;
        }
        while (yaw < -180)
        {
            yaw = yaw + 360;
        }
        return new Vector3(yaw, -1 * pitch, -1 * roll);
    }
    public void AddSourceInformation(SourceInformation SI) {
        currentSources.Add(SI.sourceID, SI);

    }
    public void UpdateSourcePosition(int id, Vector3 FLTPosition)
    {
        currentSources[id].FLTPosition = FLTPosition;

    }
    public static string sendMessageToSlab(string message, bool isUDP = false)
	{   if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3)
        {
            if (isUDP)
            {
                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                slabUDPConnection.Send(messageBytes, messageBytes.Length);
                return "0";
            }
            string sendMessage = message + (char)3;
            StreamWriter s = new StreamWriter(slabStream);
            s.Write(sendMessage);
            s.Flush();

            string response = string.Empty;
            StreamReader r = new StreamReader(slabStream);
            char[] buff = new char[256];
            r.Read(buff, 0, 256);

            response = new string(buff);
            response = response.Replace(((char)0x00).ToString(), string.Empty);
            return response;
        }
        else if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer) {
            string sendMessage = message + (char)3;
            StreamWriter s = new StreamWriter(slabStream);
            s.Write(sendMessage);
            s.Flush();
            string response = string.Empty;
            StreamReader r = new StreamReader(slabStream);
            response = r.ReadLine();
            return response;

        }
        return "";
	}
	IEnumerator wait(float sec)
	{
		yield return new WaitForSeconds(sec);
	}
    public void TurnOnCursor()
    {
        crossHair.SetActive(true);
    }
    public void TurnOffCursor() {
        crossHair.SetActive(false);
    }
    public void TurnOffSnappedCursor() {
        if (currentHighlightedObject == null)
            return;
        currentHighlightedObject.GetComponent<LEDControls>().HighlightLEDs(false, false, false, false);
        currentHighlightedObject = null;
    }
    
}
