using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections;




public class SocketCommunicationHandler : MonoBehaviour
{
	
	Socket serverSocket;
	//UdpClient listener = new UdpClient();
	List<Socket> clients = new List<Socket>();
	List<Thread> clientThreads = new List<Thread>();
	//bool clientConnected = false;
	List<MessageContainer> clientMessages = new List<MessageContainer>();
	Object messageListLock = new Object();
	private string messageMatchingExpression = "(.+)\\((.*)\\)";
	private Dictionary<string, Vector3> sourcesToInitOnRender = new Dictionary<string, Vector3>();
	Thread mainThread;
	public Material highlightMat;
	public Material defaultMat;
	private List<GameObject> disabledObjects = new List<GameObject>();
	public GameObject cursor;
	public StimulusPlayer stimulusPlayer;
    public bool clientDisconnect = false;



	// Use this for initialization
	void Awake()
	{
        //UnityEngine.Debug.Log("jdjdj");
		messageListLock = new Object();
		clientMessages = new List<MessageContainer>();
		mainThread = new Thread(WaitForConnections);
		mainThread.Start();
        //Thread monitor = new Thread(() => MonitorThreads());
		//UnityEngine.Debug.Log(stimulusPlayer);
	}

	// Update is called once per frame
	void Update()
	{
		MessageContainer messageC = null;
		bool empty = true;
		lock (messageListLock)
		{
			if (clientMessages.Count > 0)
			{
				messageC = clientMessages[0];
				clientMessages.RemoveAt(0);
				empty = false;
			}


		}
        if (clientDisconnect)
        {
            clientDisconnect = false;
            gameObject.GetComponent<SLABCommunication>().Reset();

        }
		if (empty)
			return;
		else
			ProcessMessage(messageC);





	}
	private void WaitForConnections()
	{
		//Debug.Log("wait for client");
		//listener = new UdpClient(43201);
		IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
		//Debug.Log("wait for client");
		IPAddress ipAddress = ipHostInfo.AddressList[0];
		//Debug.Log("wait for client");
		IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 43201);
		//listener.Connect(localEndPoint);
		//Debug.Log("wait for client");
		serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		//Debug.Log("wait for client");
		serverSocket.Bind(localEndPoint);
		//Debug.Log("wait for client");
		serverSocket.Listen(10);
		//Debug.Log("wait for client");

		while (true)
		{
			//Debug.Log("wait for client");

			//byte[] data = listener.Receive(ref groupEP);

			//string dataString = Encoding.ASCII.GetString(data,0,data.Length);

			Socket tempSock = serverSocket.Accept();
			clients.Add(tempSock);

			//Debug.Log(dataString);
            
			Thread t = new Thread(() => ReadClientSocket(tempSock));
			clientThreads.Add(t);
			t.Start();
            


		}




	}
    void MonitorThreads() {
       
        while (true) {
            Thread.Sleep(500);

            while (clientThreads.Count == 0) {
                Thread.Sleep(500);
            
            }


            foreach (Thread t1 in clientThreads) {
               
                if (!t1.IsAlive) {
                    clientThreads.Remove(t1);
                
                }
            
            
            }
            if (clientThreads.Count == 0) {
                
                UnityEngine.Debug.Log("Reset Called");
            
            }
            

        
        
        
        
        }
    
    
    }

	private void ReadClientSocket(Socket s)
	{
		NetworkStream n = new NetworkStream(s);
		//MessageContainer mC;//= new MessageContainer();


		using (StreamReader reader = new StreamReader(n, Encoding.UTF8))
		{
           
			while (s.Connected)
			{
				UnityEngine.Debug.Log("Waiting for message");
				//UnityEngine.Debug.Log(incomingMessage);
				string incomingMessage = reader.ReadLine();
				UnityEngine.Debug.Log(incomingMessage);
                if (incomingMessage == null)
                    break;
				lock (messageListLock)
				{
					//Debug.Log(incomingMessage);
                    
					    clientMessages.Add(new MessageContainer(s, incomingMessage));



				}


			}



		}
        UnityEngine.Debug.Log("falling out of thread");
        clientDisconnect = true;
        //gameObject.GetComponent<SLABCommunication>().Reset();



	}
	private void ProcessMessage(MessageContainer mC)
	{
		///Debug.Log(mC.message);
        
		Match match = Regex.Match(mC.message, messageMatchingExpression);
		//foreach (Group mess in match.Groups)
		//{
		//	Debug.Log(mess.Value);

		//}

		if (match.Success)
		{
			if (ConfigurationUtil.isDebug)
				LogSystem.Log("Received : " + match.Groups[0].Value);
			string reply = "";

			string[] paramList;
			string message;
			string x;
			string y;
			string z;
			string visible;
			Vector3 pos;
			Vector3 position;
			float slabX;
			float slabY;
			float slabZ;

			Renderer re;
			Material[] mats;


			GameObject soundObject;
			switch (match.Groups[1].Value.Trim())
			{

				case "loadHRTF":
				case "muteSource":
				case "switchHRTF":


					//Debug.Log("Sending SLAB MESSAGE)");

					reply = SLABCommunication.sendMessageToSlab(mC.message);

					break;
                case "reset":
                    gameObject.GetComponent<SLABCommunication>().Reset();
                    reply = "1";
                    break;
                case"useWand":
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    if (paramList[0].ToUpper().Equals("T"))
					{
                        ConfigurationUtil.isUseWand = true;
					}
					else
					{
                        ConfigurationUtil.isUseWand = false;

					}
                    reply = "1";
                    break;

                case "hideCursor":
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    UnityEngine.Debug.Log("Hiding the cursor");
                    GameObject cursorObject = GameObject.FindGameObjectWithTag("Cursor");
                    Crosshair3D c3D = cursorObject.GetComponent<Crosshair3D>();
                    if (paramList[0].ToUpper().Equals("T"))
                    {
                        c3D.hideCursor();
                    }
                    else {
                        c3D.showCursor();
                    
                    }
                    
                    //MeshRenderer cursorRenderer = cursorObject.GetComponent<MeshRenderer>();
                    //cursorRenderer.material.SetColor(0, new Color(cursorRenderer.material.color.r, cursorRenderer.material.color.g,cursorRenderer.material.color.b, 0.0f));
                    reply = "1";
                    break;
				case "startRendering":
					reply = SLABCommunication.sendMessageToSlab(mC.message);
					foreach (string key in sourcesToInitOnRender.Keys)
					{
						Vector3 loc = sourcesToInitOnRender[key];
						SLABCommunication.sendMessageToSlab("presentSource(" + key + "," + loc.x + "," + loc.y + "," + loc.z + ")");
						SLABCommunication.sendMessageToSlab("setSourceGain(" + key + ", 0)");
						//SLABCommunication.sendMessageToSlab("muteSource(" + key + ",0)");
					}
                    sourcesToInitOnRender.Clear();
                    break;
				case "addASIOSource":
					// addASIOSource (nhtf,channel, location, visible) 
					paramList = match.Groups[2].Value.Trim().Split(',');
					message = "allocateASIOSource(" + paramList[0].Trim() +
						"," + paramList[1].Trim() +
						")";
					x = paramList[2].Trim('[');
					y = paramList[3];
					z = paramList[4].Trim(']');
					visible = paramList[5];
					pos = new Vector3(float.Parse(y) * -1, float.Parse(z), float.Parse(x));


					soundObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					soundObject.transform.Translate(pos);
					soundObject.transform.localScale *= .1f;
					re = soundObject.GetComponent<Renderer>();
					mats = re.materials;
					mats[0] = defaultMat;
					re.materials = mats;
					soundObject.tag = "SoundSource";
					if (!visible.ToUpper().Equals("T"))
					{

						soundObject.SetActive(false);
						

					}
					if (stimulusPlayer != null)
					{
						stimulusPlayer.CreateSound(soundObject);
					}
					

					position = new Vector3(float.Parse(y) * -1, float.Parse(z), float.Parse(x));
					//reply = SLABCommunication.sendMessageToSlab(message);


					if (float.Parse(reply.Split(',')[1]) > 0)
					{
						//Debug.Log("present source");
						slabX = position.z;
						slabY = -position.x;
						slabZ = position.y;
						sourcesToInitOnRender.Add(reply.Split(',')[1], new Vector3(slabX, slabY, slabZ));
						SLABCommunication.sendMessageToSlab("enableSource(" + reply.Split(',')[1].Trim() + ",1)");
						WorldVariables.AddSLABObbject(reply.Split(',')[1], soundObject);
						//SLABCommunication.sendMessageToSlab("presentSource("+reply.Split(',')[1].Trim()+"," + slabX + "," + slabY + "," + slabZ + ")");
					}

					break;
				case "addNoiseSource":
					//addNoiseSource ( amplitude,nHRTF,location, visible ) 
					paramList = match.Groups[2].Value.Trim().Split(',');
					message = "allocateSigGenSource(N," + paramList[0].Trim() +
						"," + paramList[1].Trim() +
						")";
					x = paramList[2].Trim('[');
					y = paramList[3];
					z = paramList[4].Trim(']');
					visible = paramList[5];

					pos = new Vector3(float.Parse(y) * -1, float.Parse(z), float.Parse(x));

					soundObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					soundObject.transform.Translate(pos);
					soundObject.transform.localScale *= .1f;
					re = soundObject.GetComponent<Renderer>();
					mats = re.materials;
					mats[0] = defaultMat;
					re.materials = mats;
					soundObject.tag = "SoundSource";
					if (!visible.ToUpper().Equals("T"))
					{

						soundObject.SetActive(false);


					}

					position = new Vector3(float.Parse(y) * -1, float.Parse(z), float.Parse(x));
					reply = SLABCommunication.sendMessageToSlab(message);


					if (float.Parse(reply.Split(',')[1]) > 0)
					{
						//Debug.Log("present source");
						slabX = position.z;
						slabY = -position.x;
						slabZ = position.y;
						sourcesToInitOnRender.Add(reply.Split(',')[1], new Vector3(slabX, slabY, slabZ));
						SLABCommunication.sendMessageToSlab("enableSource(" + reply.Split(',')[1].Trim() + ",1)");
						WorldVariables.AddSLABObbject(reply.Split(',')[1], soundObject);
						//SLABCommunication.sendMessageToSlab("presentSource("+reply.Split(',')[1].Trim()+"," + slabX + "," + slabY + "," + slabZ + ")");
					}

					break;
				case "addWAVSource":
					paramList = match.Groups[2].Value.Trim().Split(',');
					//sendMessageToSlab("allocateWaveSource(" + wavName + ",1,1,0)");
					message = "allocateWaveSource(" + paramList[0].Trim() +
										"," + paramList[1].Trim() +
										"," + paramList[2].Trim() +
										"," + paramList[3].Trim() + ")";
					x = paramList[4].Trim('[');
					y = paramList[5];
					z = paramList[6].Trim(']');
					visible = paramList[7];
					pos = new Vector3(float.Parse(y) * -1, float.Parse(z), float.Parse(x));

					soundObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					soundObject.transform.Translate(pos);
					soundObject.transform.localScale *= .0801f;
					re = soundObject.GetComponent<Renderer>();
					mats = re.materials;
					mats[0] = defaultMat;
					re.materials = mats;
					soundObject.tag = "SoundSource";
					if (!visible.ToUpper().Equals("T"))
					{

						soundObject.SetActive(false);


					}

					position = new Vector3(float.Parse(y) * -1, float.Parse(z), float.Parse(x));
					reply = SLABCommunication.sendMessageToSlab(message);


					if (float.Parse(reply.Split(',')[1]) > 0)
					{
						//Debug.Log("present source");
						slabX = position.z;
						slabY = -position.x;
						slabZ = position.y;
						sourcesToInitOnRender.Add(reply.Split(',')[1], new Vector3(slabX, slabY, slabZ));
						SLABCommunication.sendMessageToSlab("enableSource(" + reply.Split(',')[1].Trim() + ",1)");
						WorldVariables.AddSLABObbject(reply.Split(',')[1], soundObject);
						//SLABCommunication.sendMessageToSlab("presentSource("+reply.Split(',')[1].Trim()+"," + slabX + "," + slabY + "," + slabZ + ")");
					}
					//SLABCommunication.sendMessageToSlab(message);
					//Debug.Log(reply);

					break;
				case "adjustSourceLevel":
					//adjustSourceLevel(src, relativeLevel)
					paramList = match.Groups[2].Value.Trim().Split(',');
					message = "adjustSourceGain(" + paramList[0].Trim() +
										"," + paramList[1].Trim() + ")";
					reply = SLABCommunication.sendMessageToSlab(message);

					break;
				case "adjustSourcePosition":
					//adjustSourcePosition(src, pos)
					paramList = match.Groups[2].Value.Trim().Split(',');


					x = paramList[1].Trim('[');
					y = paramList[2];
					z = paramList[3].Trim(']');


					pos = new Vector3(float.Parse(y) * -1, float.Parse(z), float.Parse(x));

					slabX = pos.z;
					slabY = -pos.x;
					slabZ = pos.y;

					message = "updateSource(" + paramList[0].Trim() +
						"," + slabX +"," + slabY +	"," + slabZ + ")";
						//pos = new Vector3(float.Parse(y) * -1, float.Parse(z), float.Parse(x));
					soundObject = WorldVariables.GetSLABObbject(paramList[0].Trim());
					soundObject.transform.localPosition = pos;
					reply = SLABCommunication.sendMessageToSlab(message);

					break;
				case "showSource":
					//showSource(src, on/off)
					paramList = match.Groups[2].Value.Trim().Split(',');
					soundObject = WorldVariables.GetSLABObbject(paramList[0].Trim());
					if (paramList[1].ToUpper().Equals("T"))
					{

						soundObject.SetActive(true);

					}
					else
					{
						soundObject.SetActive(false);
					}
					reply = "1";

					break;
				case "highlightSource":
					//highlightSource(src)
					paramList = match.Groups[2].Value.Trim().Split(',');
					soundObject = WorldVariables.GetSLABObbject(paramList[0].Trim());
					re = soundObject.GetComponent<Renderer>();
					mats = re.materials;
					if (paramList[1].ToUpper().Equals("T"))
					{
						mats[0] = highlightMat;
					}
					else
					{
						mats[0] = defaultMat;
					}
					re.materials = mats;
					reply = "1";
					break;
				case "showScene":
					// showScene(on/off)  
					paramList = match.Groups[2].Value.Trim().Split(',');
					GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();


					//if (!g.tag.Equals("SoundSource") && !g.tag.Equals("Cursor"))

					if (paramList[0].ToUpper().Equals("T"))
					{
						foreach (GameObject g0 in disabledObjects)
							g0.SetActive(true);
					}
					else
					{
						foreach (GameObject g in gameObjects)
						{
							if (g.layer == 8)
							{
								g.SetActive(false);
								disabledObjects.Add(g);
							}
						}

					}
					reply = "1";
					break;
				case "showCursor":
					//showCursor(T/F)
					paramList = match.Groups[2].Value.Trim().Split(',');
					GameObject gO = GameObject.FindGameObjectWithTag("Cursor");
					if (paramList[0].ToUpper().Equals("T"))
					{
						cursor.SetActive(true);
					}
					else
					{
						cursor.SetActive(false);
					}

					reply = "1";
					break;

				case "htBoresight":
					UnityEngine.VR.InputTracking.Recenter();
					reply = "1";
					break;
				case "getLocalizationResponse":
					WorldVariables.waitingForLocalizationResponse = true;
					WorldVariables.waitingClient = mC.sender;
					UnityEngine.Debug.Log("localization response");
					//UnityEngine.Debug.Log("localization reposne");
					//StartCoroutine("returnLocalization",mC);
					
					reply = "";
					break;
                case "getLocalizationResponseLAE":
                    WorldVariables.waitingForLocalizationResponseLAE = true;
                    WorldVariables.waitingClient = mC.sender;
                    //UnityEngine.Debug.Log("localization response");
                    //UnityEngine.Debug.Log("localization reposne");
                    //StartCoroutine("returnLocalization",mC);

                    reply = "";
                    break;
                case "getLocalizationResponseLAESpeaker":
                    WorldVariables.waitingForLocalizationResponseLAESpeaker = true;
                    WorldVariables.waitingClient = mC.sender;
                    //UnityEngine.Debug.Log("localization response");
                    //UnityEngine.Debug.Log("localization reposne");
                    //StartCoroutine("returnLocalization",mC);

                    reply = "";
                    break;
				case "getHeadOrientation":
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

					//NetworkStream n = new NetworkStream(mC.sender);
                    //StreamWriter writer = new StreamWriter(n, ASCIIEncoding.ASCII);
                    Vector3 ori = gameObject.GetComponent<SLABCommunication>().getListenerOrientation();
                    reply = ori.x.ToString() + "," + ori.y.ToString() + "," + ori.z.ToString();

					//writer.WriteLine(reply);
					//writer.Flush();
					//writer.Close();
					//n.Close();

					break;
                case "getHeadOrientationLAE":
                    camera = GameObject.FindGameObjectWithTag("MainCamera");
                    roll = camera.transform.localEulerAngles.z;
                    while (roll > 180)
                    {
                        roll = roll - 360;

                    }
                    while (roll < -180)
                    {
                        roll = roll + 360;

                    }
                    pitch = camera.transform.localEulerAngles.x;
                    while (pitch > 180)
                    {
                        pitch = pitch - 360;

                    }
                    while (pitch < -180)
                    {
                        pitch = pitch + 360;

                    }
                    yaw = -camera.transform.localEulerAngles.y;
                    while (yaw > 180)
                    {
                        yaw = yaw - 360;

                    }
                    while (yaw < -180)
                    {
                        yaw = yaw + 360;

                    }

                    //NetworkStream n = new NetworkStream(mC.sender);
                    //StreamWriter writer = new StreamWriter(n, ASCIIEncoding.ASCII);
                    //Vector3 ori = gameObject.GetComponent<SLABCommunication>().getListenerOrientation();
                    reply = yaw.ToString() + "," + pitch.ToString();

                    //writer.WriteLine(reply);
                    //writer.Flush();
                    //writer.Close();
                    //n.Close();

                    break;
				default:
					if (ConfigurationUtil.isDebug)
						LogSystem.Log("Unable to process command" + match.Groups[0].Value);
					reply = "-1";

					break;






			}
			if (!reply.Equals(""))
			{
				if (ConfigurationUtil.isDebug)
					LogSystem.Log("Sent : " + reply);
				NetworkStream n = new NetworkStream(mC.sender);
				StreamWriter writer = new StreamWriter(n,ASCIIEncoding.ASCII);
				writer.WriteLine(reply);
				writer.Flush();
				writer.Close();
				n.Close();

			}





		}
		else
			//Debug.Log("Unknown Command");
			LogSystem.Log("Unable to parse command");



	}
	void OnDestroy()
	{
		foreach (Socket s in clients)
		{
			s.Close();
		}
		foreach (Thread t in clientThreads)
		{
			t.Abort();

		}

		mainThread.Abort();
		serverSocket.Close();



	}
	IEnumerator returnLocalization(MessageContainer mC){
		UnityEngine.Debug.Log("Stuck in loop");
		while(!Input.GetButtonDown("Fire1"))
			UnityEngine.Debug.Log("Stuck in loop");
			yield return new WaitForSeconds(.01f);
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

		NetworkStream n = new NetworkStream(mC.sender);
		StreamWriter writer = new StreamWriter(n, Encoding.UTF8);
		string reply = yaw.ToString() + "," + pitch.ToString() + "," + roll.ToString();

		writer.WriteLine(reply);
		writer.Flush();
		writer.Close();
		n.Close();

	
	
	
	
	
	
	
	
	
	
	}
	public class MessageContainer
	{

		public MessageContainer(Socket fromSocket, string incomingMessage)
		{
			message = incomingMessage;
			sender = fromSocket;
		}

		public string message;
		public Socket sender;



	}
}
