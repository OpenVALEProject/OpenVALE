using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Xml.XPath;

public class SLABCommunication : MonoBehaviour
{
	public int port = 1112;
	private TcpClient slabConnection;
	private static NetworkStream slabStream;
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


	//private string response;

	// Use this for initialization
	void Awake()
	{

		foreach (var process in Process.GetProcessesByName("AudioServer3 (32 bit)"))
		{
			process.Kill();


		}
        slabProcess = new Process();
		slabProcess.StartInfo.FileName = "D:\\Development\\Spatial Audio Server\\Server\\bin\\Release\\AudioServer3.exe";
        slabProcess.StartInfo.WorkingDirectory = "D:\\Development\\Spatial Audio Server\\Server\\bin\\Release\\";


		//UnityEngine.Debug.Log("Should have waited longer?");
		slabProcess.Start();

		Thread.Sleep(2000);
        while (true)
        {
            try
            {
                slabConnection = new TcpClient("127.0.0.1", port);
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
        WorldVariables.clearObjects();

        Thread.Sleep(500);

        sendMessageToSlab("setHRTFPath(" + HRTFDir + ")");
        sendMessageToSlab("setWavePath(" + wavDir + ")");
        sendMessageToSlab("setFIRTaps(" + FIRTaps + ")");

        Thread.Sleep(2000);

        sendMessageToSlab("setHRTFPath(" + HRTFDir + ")");
        sendMessageToSlab("defineASIOOutChMap(" + channelMap + " )");
        sendMessageToSlab("defineASIOChMap(" + outChannelMap + ")");
        sendMessageToSlab("selectOutDevice(" + outDevice + ")");
        sendMessageToSlab("setWavePath(" + wavDir + ")");
        sendMessageToSlab("setFIRTaps(" + FIRTaps + ")");
    }

	// Update is called once per frame
	void Update()
    {
        //UnityEngine.Debug.Log(getListenerOrientation());
		if (Application.loadedLevelName.Contains("ALFaaaaaaaaaaaaaaaaaaa"))
		{
			if (triggerFeedback && feedbackFinished)
			{
				feedbackFinished = false;
				StartCoroutine("giveFeedback");



			}

			if (init)
			{

                /*
				//positions = gameObject.GetComponent("WorldMaker");
				WorldMaker w = gameObject.GetComponent<WorldMaker>();
				soundSourceList = w.sphereLocations;
				//GameObject source = GameObject.FindGameObjectWithTag("SoundSource");
				UnityEngine.Debug.Log(soundSourceList.Count);
				soundSource = soundSourceList[272]; //soundSourceList[chooser.Next(soundSourceList.Count)];
				//float slabX = source.transform.localPosition.z; 
				//float slabY = source.transform.localPosition.x;
				//float slabZ = source.transform.localPosition.y;

				float slabX = soundSource.transform.position.z;
				float slabY = -soundSource.transform.position.x;
				float slabZ = soundSource.transform.position.y;
				Renderer re = soundSource.GetComponent<Renderer>();
				re.material.color = new Color(1.0f, 0.0f, 0.0f);
				sendMessageToSlab("presentSource(1," + slabX + "," + slabY + "," + slabZ + ")");
				sendMessageToSlab("muteSource(1,0)");
				init = false;
                */
			}

			if (Time.time > nextSound && feedbackFinished)
			{

				nextSound = Time.time + soundDelay;
				if (mute)
				{
					//sendMessageToSlab("muteSource(1,1)");
					//soundSource = soundSourceList[chooser.Next(soundSourceList.Count)];
					//float slabX = source.transform.localPosition.z; 
					//float slabY = source.transform.localPosition.x;
					//float slabZ = source.transform.localPosition.y;
					//float slabX = soundSource.transform.position.z;
					//float slabY = soundSource.transform.position.x;
					//float slabZ = soundSource.transform.position.y;

					//sendMessageToSlab("presentSource(1," + slabX + "," + slabY + "," + slabZ + ")");


				}
				else
				{
					//ypos = ypos * -1;


					//sendMessageToSlab("muteSource(1,0)");
				}

				mute = !mute;

			}

		}
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
		sendMessageToSlab("setListenerPosition(" + yaw + "," + -1 * pitch + "," + -1 * roll + ")");


	}

	/*
	void Update()
	{
		if (triggerFeedback)
		{


		}

		if (init)
		{


			//positions = gameObject.GetComponent("WorldMaker");
			WorldMaker w = gameObject.GetComponent<WorldMaker>();
			soundSourceList = w.sphereLocations;
			//GameObject source = GameObject.FindGameObjectWithTag("SoundSource");
			soundSource = soundSourceList[chooser.Next(soundSourceList.Count)];
			//float slabX = source.transform.localPosition.z; 
			//float slabY = source.transform.localPosition.x;
			//float slabZ = source.transform.localPosition.y;

			float slabX = soundSource.transform.position.z;
			float slabY = soundSource.transform.position.x;
			float slabZ = soundSource.transform.position.y;

			sendMessageToSlab("presentSource(1," + slabX + "," + slabY + "," + slabZ + ")");

			init = false;
		}


		if (Time.time > nextSound)
		{

			nextSound = Time.time + soundDelay;
			if (mute)
			{
				sendMessageToSlab("muteSource(1,1)");
				soundSource = soundSourceList[chooser.Next(soundSourceList.Count)];
				//float slabX = source.transform.localPosition.z; 
				//float slabY = source.transform.localPosition.x;
				//float slabZ = source.transform.localPosition.y;
				float slabX = soundSource.transform.position.z;
				float slabY = soundSource.transform.position.x;
				float slabZ = soundSource.transform.position.y;

				sendMessageToSlab("presentSource(1," + slabX + "," + slabY + "," + slabZ + ")");


			}
			else
			{
				//ypos = ypos * -1;

				GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
				float roll = camera.transform.localEulerAngles.z;
				while (roll > 180)
				{
					roll = roll - 360;

				}
				while (roll < -180)
				{
					roll = roll + 360;

				}
				float pitch = camera.transform.localEulerAngles.x;
				while (pitch > 180)
				{
					pitch = pitch - 360;

				}
				while (pitch < -180)
				{
					pitch = pitch + 360;

				}
				float yaw = camera.transform.localEulerAngles.y;
				while (yaw > 180)
				{
					yaw = yaw - 360;

				}
				while (yaw < -180)
				{
					yaw = yaw + 360;

				}
				//soundSource = Random.
				sendMessageToSlab("setListenerPosition(" + yaw + "," + pitch + "," + roll + ")");
				sendMessageToSlab("muteSource(1,0)");
			}
			mute = !mute;

		}



	}
	 * */
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

	public static string sendMessageToSlab(string message)
	{
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
	IEnumerator wait(float sec)
	{

		yield return new WaitForSeconds(sec);

	}





}
