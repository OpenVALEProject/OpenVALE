using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;
using System.Xml.XPath;

public class SLABCommunication : MonoBehaviour
{
    
	public int port = 1112;
	private TcpClient slabConnection;
    private static UdpClient slabUDPConnection;
    private static NetworkStream slabStream;
    private static NetworkStream slabUDPStream;
    private Process slabProcess;
	public float soundDelay = 1.1f;
	public float nextSound = 0.0f;
	private bool mute = true;
	//private int ypos = 10;
	private GameObject soundSource;
	private List<GameObject> soundSourceList;
	private bool init = true;
	private System.Random chooser = new System.Random();
	public bool triggerFeedback = false;
	private bool feedbackFinished = true;
	private string HRTFDir = ConfigurationUtil.HRTFDir;
	//private string HRTFName = ConfigurationUtil.HRTFName;
	private string wavDir = ConfigurationUtil.wavDir;
	private string wavName = ConfigurationUtil.wavName;
	private string outDevice = ConfigurationUtil.IODevice;
	private string channelMap = ConfigurationUtil.channelMap;
	private string outChannelMap = ConfigurationUtil.outChannelMap;
	private string FIRTaps = ConfigurationUtil.FIRTaps;
    public GameObject cameraTransform;
    public GameObject joystickCam;
    public GameObject occCam;
    private string spatialAudioServerDirectory = ConfigurationUtil.spatialAudioServer;
    public bool isRendering = false;
    private GameObject currentHighlightedObject = null;
    public GameObject crossHair;
    public float crossHairDepth;
    public GameObject panelBase;
    private Dictionary<int, SourceInformation> currentSources = new Dictionary<int, SourceInformation>();
	//private string response;

	// Use this for initialization
	void Awake()
	{

		foreach (var process in Process.GetProcessesByName("WinAudioServer"))
		{
			process.Kill();


		}
        UnityEngine.Debug.Log(spatialAudioServerDirectory + "\\WinAudioServer.exe");
        slabProcess = new Process();
        slabProcess.StartInfo.FileName = spatialAudioServerDirectory + "\\WinAudioServer.exe";
        //slabProcess.StartInfo.FileName = "D:\\Development\\Spatial Audio Server\\Server\\bin\\Release\\AudioServer3.exe";
        slabProcess.StartInfo.WorkingDirectory = spatialAudioServerDirectory + "\\";


		//UnityEngine.Debug.Log("Should have waited longer?");
		slabProcess.Start();

		Thread.Sleep(500);
        while (true)
        {
            try
            {
                slabConnection = new TcpClient("127.0.0.1", port);
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

        //string r;
        sendMessageToSlab("setHRTFPath(" + HRTFDir + ")");
        //sendMessageToSlab("loadHRTF(" + HRTFName + ")");
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
    public void Reset() {
        UnityEngine.Debug.Log("INSIDE RESET");
        //string r;

        sendMessageToSlab("freeSources()");
        Thread.Sleep(500);

        sendMessageToSlab("setHRTFPath(" + HRTFDir + ")");
        //sendMessageToSlab("loadHRTF(" + HRTFName + ")");
        sendMessageToSlab("setWavePath(" + wavDir + ")");
        sendMessageToSlab("setFIRTaps(" + FIRTaps + ")");
        soundSourceList.Clear();
    }

	// Update is called once per frame
	void Update()
    {
        //UnityEngine.Debug.Log(getListenerOrientation());

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
        if (!ConfigurationUtil.useRift)
        {
            //UnityEngine.Debug.Log(joystickCam.transform.rotation);
            
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

            sendMessageToSlab("setListenerPosition(" + yaw + "," + -1 * pitch + "," + -1 * roll + ")");

        }
        else
        {
            //UnityEngine.Debug.Log("WrongOne");


            //GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            Vector3 orientationVector = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head).eulerAngles;
            roll = orientationVector.z;//eulerAngles.z;
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
        }

        sendMessageToSlab("setListenerPosition(" + yaw + "," + -1 * pitch + "," + -1 * roll + ")");
        Vector3 cameraPosition = UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.Head);
        Vector3 relativePosition = Vector3.zero;
        foreach (SourceInformation S in currentSources.Values) {
            relativePosition = S.FLTPosition - HelperFunctions.UnityXYZToFLT(cameraPosition);
            sendMessageToSlab("updateSource(" + S.sourceID + "," + relativePosition.x + "," + relativePosition.y + "," + relativePosition.z);
        }

        if (ConfigurationUtil.currentCursorAttachment == ConfigurationUtil.CursorAttachment.none && ConfigurationUtil.currentCursorType == ConfigurationUtil.CursorType.none) {
            return;
        }
        if (currentHighlightedObject != null)
        {
            currentHighlightedObject.GetComponent<LEDControls>().HighlightLEDs(false, false, false, false);

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

                crossHair.transform.position = UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.CenterEye);
                //Debug.Log(OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch));
                crossHair.transform.position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch) + (OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward).normalized * crossHairDepth ;
                crossHair.transform.LookAt(crossHair.transform.position + UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye) * Vector3.forward, UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye) * Vector3.up);
                crossHair.transform.Rotate(Vector3.right, -90);
            }
            else if (ConfigurationUtil.currentCursorType == ConfigurationUtil.CursorType.snapped)
            {
                Vector3 intersectionLocation = (OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward).normalized * 2.08f;
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
            Vector3 targetAlignment = (ConfigurationUtil.recenterPosition - camera.transform.forward).normalized;
            UnityEngine.Debug.Log("Recenter : " + Mathf.Acos(Vector3.Dot(camera.transform.forward, targetAlignment)));
            if (Mathf.Acos(Vector3.Dot(camera.transform.forward, targetAlignment)) < ConfigurationUtil.recenterTolerance)
            {
                
                string message = "waitForRecenter," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE;
                GetComponent<SocketCommunicationHandler>().sendMessage(message, ConfigurationUtil.waitingClient);
                ConfigurationUtil.waitingForRecenter = false;
                ConfigurationUtil.recenterTolerance = 0;
                ConfigurationUtil.recenterPosition = new Vector3(0, 0, 0);
                ConfigurationUtil.waitingClient = null;
            }


        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (ConfigurationUtil.waitingForResponse) {

                string message = "waitForResponse,"+(int)ERRORMESSAGES.ErrorType.ERR_AS_NONE + ",[";
                Vector3 intersectionPoint = Vector3.zero;
                if (ConfigurationUtil.currentCursorAttachment == ConfigurationUtil.CursorAttachment.hand) { }
                else {
                    intersectionPoint = camera.transform.forward.normalized * 2.08f;
                }
                string spkID = GetComponent<ALFLeds>().getNearestSpeakerID(intersectionPoint);
                message += spkID;
                message += ",";
                float respTime = Time.time - ConfigurationUtil.waitStartTime;
                message += respTime + "]";
                
                GetComponent<SocketCommunicationHandler>().sendMessage(message, ConfigurationUtil.waitingClient);
                ConfigurationUtil.waitingForResponse = false;
                ConfigurationUtil.waitingClient = null;
                ConfigurationUtil.waitStartTime = 0.0f;

            }


        }


		//soundSource = Random.
        //if(isRendering)
		//    sendMessageToSlab("updateLstOrienation " + yaw + "," + -1 * pitch + "," + -1 * roll + "",true);


	}


	IEnumerator giveFeedback()
	{
		sendMessageToSlab("muteSource(1,1)");

		Renderer re = soundSource.GetComponent<Renderer>();
		Color c = re.material.color;
		re.material.color = new Color(0.0f, 1.0f, 0.0f);


		yield return new WaitForSeconds(5.0f);
		re.material.color = c;

		soundSource = soundSourceList[chooser.Next(soundSourceList.Count)];
		soundSource = soundSourceList[272];//
		float slabX = soundSource.transform.position.z;
		float slabY = -soundSource.transform.position.x;
		float slabZ = soundSource.transform.position.y;
		re = soundSource.GetComponent<Renderer>();
		re.material.color = new Color(1.0f, 0.0f, 0.0f);
        
		sendMessageToSlab("presentSource(1," + slabX + "," + slabY + "," + slabZ + ")");
		triggerFeedback = false;

		//UnityEngine.Debug.Log("Done with feedback");
		feedbackFinished = true;


	}

	void OnDestroy()
	{

		slabProcess.Kill();
		//slabStream.Close();
		//slabConnection.Close();

	}
    public Vector3 getListenerOrientation() {
        GameObject camera;
        if (!ConfigurationUtil.useRift)
        {
            //UnityEngine.Debug.Log(joystickCam.transform.rotation);
            camera = joystickCam;

        }
        else
        {
            //UnityEngine.Debug.Log("WrongOne");
            camera = occCam;
        }
        //GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
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
        //soundSource = Random.
        //sendMessageToSlab("setListenerPosition(" + yaw + "," + -1 * pitch + "," + -1 * roll + ")");
        return new Vector3(yaw, -1 * pitch, -1 * roll);
    }
    public static Vector3 getObjectOrientation(GameObject g)
    {
        GameObject camera;
        
        camera = g;
        //GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
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
        //soundSource = Random.
        //sendMessageToSlab("setListenerPosition(" + yaw + "," + -1 * pitch + "," + -1 * roll + ")");
        return new Vector3(yaw, -1 * pitch, -1 * roll);
    }
    public void AddSourceInformation(SourceInformation SI) {
        UnityEngine.Debug.Log("Source information ID : " + SI.sourceID);
        currentSources.Add(SI.sourceID, SI);

    }
    public static string sendMessageToSlab(string message, bool isUDP = false)
	{   if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3)
        {
            if (isUDP)
            {
                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                slabUDPConnection.Send(messageBytes, messageBytes.Length);
                //UnityEngine.Debug.Log("we");
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
            //UnityEngine.Debug.Log(sendMessage);
            //byte[] data = System.Text.Encoding.ASCII.GetBytes(sendMessage);
            //UnityEngine.Debug.Log(data.ToString());
            StreamWriter s = new StreamWriter(slabStream);
            s.Write(sendMessage);
            s.Flush();
            //slabStream.Write(data, 0, data.Length);
            //slabStream.Flush();


            string response = string.Empty;
            //data = new byte[256];
            StreamReader r = new StreamReader(slabStream);
            response = r.ReadLine();
            //int bytes = slabStream.Read(data, 0, data.Length);
            //response = "";// System.Text.Encoding.ASCII.GetString(data, 0, 1);
            return response;

        }
        return "";
	}
	IEnumerator wait(float sec)
	{
		yield return new WaitForSeconds(sec);
	}
    public void TurnOffCursor() {
        crossHair.SetActive(false);
    }
    public void TurnOffSnappedCursor() {
        currentHighlightedObject.GetComponent<LEDControls>().HighlightLEDs(false, false, false, false);
        currentHighlightedObject = null;
    }
}
