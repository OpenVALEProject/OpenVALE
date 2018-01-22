﻿using UnityEngine;
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
    public bool clientDisconnect = false;
    private List<string> currentSourceList = new List<string>();
    public bool waitingForResponse = false;
    public string responseToSend = "";
    public GameObject Highlighter;
    private ConfigurationUtil.AudioEngineType currentEngine;

	// Use this for initialization
	void Awake()
	{
        //UnityEngine.Debug.Log("jdjdj");
		messageListLock = new Object();
		clientMessages = new List<MessageContainer>();
		mainThread = new Thread(WaitForConnections);
		mainThread.Start();
        currentEngine = ConfigurationUtil.engineType;
        
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
		IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
		IPAddress ipAddress = ipHostInfo.AddressList[0];
		IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 43201);
		serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		serverSocket.Bind(localEndPoint);
		serverSocket.Listen(10);

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
            float xF = 0;
            float yF = 0;
            float zF = 0;

            Renderer re;
			Material[] mats;
            string[] replyCheck;
            int replyCode = 0;


            GameObject soundObject;
            switch (match.Groups[1].Value.Trim().ToLower())
            {
                case "loadhrtf":
                    reply = SLABCommunication.sendMessageToSlab(mC.message);
                    replyCheck = reply.Split(',');
                    
                    if (int.TryParse(replyCheck[1], out replyCode))
                    {
                        if (replyCode > 0)
                            reply = "loadHRTF," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE + "," + replyCode;
                        else
                            reply = "loadHRTF," + (int)ERRORMESSAGES.ErrorType.ERR_AS_SLABERRORCODE + "," + replyCode;

                    }
                    else {
                        reply = "loadHRTF," + (int)ERRORMESSAGES.ErrorType.ERR_AS_HRTFFILELOADFAILURE;
                    }

                    break;
                case "setsourcehrtf":
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    int srcID;
                    int hrtfID;

                    if (!int.TryParse(paramList[0], out srcID))
                    {
                        reply = "setSourceHRTF," + (int)ERRORMESSAGES.ErrorType.ERR_AS_SRCIDMUSTBEINTEGER;
                        break;
                    }
                    if (!int.TryParse(paramList[1], out hrtfID))
                    {
                        reply = "setSourceHRTF," + (int)ERRORMESSAGES.ErrorType.ERR_AS_HRTFIDMUSTBEINTEGER;
                        break;
                    }
                    if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer) {
                        reply = SLABCommunication.sendMessageToSlab("switchHRTF(" + srcID + "," + hrtfID + ")");
                        replyCheck = reply.Split(',');

                        if (int.TryParse(replyCheck[1], out replyCode))
                        {
                            if (replyCode == 0)
                            {
                                reply = "setSourceHRTF," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE;
                            }
                            else
                            {
                                reply = "setSourceHRTF," + (int)ERRORMESSAGES.ErrorType.ERR_AS_SLABERRORCODE + "," + replyCode;
                                break;
                            }
                        }
                        else {
                            reply = "setSourceHRTF," + (int)ERRORMESSAGES.ErrorType.ERR_AS_FAILEDTOPARSESLABRESPONSE;
                        }
                    }
                    break;
                case "sethrtf":
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    int sourceNumber;

                    // If hrtf ID load hrtf for all sources
                    if (int.TryParse(paramList[0], out sourceNumber))
                    {
                        bool success = true;
                        foreach (string s in currentSourceList)
                        {
                            reply = SLABCommunication.sendMessageToSlab("switchsrchrtf(" + s + "," + sourceNumber + ")");
                            if (!reply.Trim().Split()[1].Equals("0")) {
                                success = false;
                            }
                        }
                        if (success)
                        {
                            reply = "setHRTF," + reply.Trim().Split()[2];
                        }
                        else
                            reply = "setHRTF,0";
                    }
                    // else load the hrtf filename
                    else {

                        reply = SLABCommunication.sendMessageToSlab("loadHRTF(" + paramList[0] + ")");
                        if (!reply.Trim().Split()[1].Equals("0"))
                        {
                            reply = "setHRTF," + reply.Trim().Split()[2];
                        }
                        else
                            reply = "setHRTF,0";

                    }

                    break;

                case "addaudiosource":

                    paramList = match.Groups[2].Value.Trim().Split(',');
                    //allocwavesrc
                    //Get position information in FLT
                    x = paramList[1].Trim('[');
                    y = paramList[2];
                    z = paramList[3].Trim(']');
                    xF = 0;
                    yF = 0;
                    zF = 0;
                    int sourceHRTFID;
                    if (!int.TryParse(paramList[4], out sourceHRTFID))
                    {
                        reply = "addAudioSource," + ERRORMESSAGES.ErrorType.ERR_AS_HRTFIDMUSTBEINTEGER;
                        break;
                    }
                    if (!float.TryParse(x, out xF))
                    {
                        reply = "addAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                        break;
                    }
                    if (!float.TryParse(y, out yF))
                    {
                        reply = "addAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                        break;
                    }
                    if (!float.TryParse(z, out zF))
                    {
                        reply = "addAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                        break;
                    }
                    Vector3 FLTLocation = HelperFunctions.UnityXYZToFLT(new Vector3(xF, yF, zF));
                    // Allocate a wav source
                    if (paramList[0].ToLower().Equals("wav")) {
                        string fname = paramList[5].Split('=')[1];
                        if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3)
                        {
                            reply = SLABCommunication.sendMessageToSlab("allocWaveSrc " + paramList[4] + "");
                        }
                        else if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer)
                        {

                            reply = SLABCommunication.sendMessageToSlab("allocateWaveSource(" + fname + ",0,"+sourceHRTFID +","+ "0)");
                        }
                        replyCheck = reply.Split(',');

                        if (int.TryParse(replyCheck[1], out replyCode))
                        {
                            if (replyCode > 0)
                            {
                                reply = "addAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE + "," + replyCode;
                            }
                            else
                            {
                                reply = "addAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_SLABERRORCODE + "," + replyCode;
                                break;
                            }
                        }
                        else
                        {
                            reply = "addAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_FAILEDTOPARSESLABRESPONSE;
                        }

                    }
                    // Allocate Generator Source
                    if (paramList[0].ToLower().Equals("noisegen"))
                    {
                        if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3)
                        {
                            reply = SLABCommunication.sendMessageToSlab("allocSigGenSrc N,1.0,1000,0,1");
                        }
                        else if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer) {
                            
                            reply = SLABCommunication.sendMessageToSlab("allocateSigGenSource(N,1.0," +sourceHRTFID+")");
                        }
                        replyCheck = reply.Split(',');

                        if (int.TryParse(replyCheck[1], out replyCode))
                        {
                            if (replyCode > 0)
                            {
                                reply = "addAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE + "," + replyCode;
                            }
                            else
                            {
                                reply = "addAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_SLABERRORCODE + "," + replyCode;
                                break;
                            }
                        }
                        else
                        {
                            reply = "addAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_FAILEDTOPARSESLABRESPONSE;
                        }
                    }
                    // Allocate ASIO Source
                    if (paramList[0].ToLower().Equals("asio"))
                    {
                        int numberOfChannels;
                        

                        if (int.TryParse(paramList[5], out numberOfChannels))
                        {

                            if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3)
                            {
                                reply = SLABCommunication.sendMessageToSlab("allocAsioSrc()");
                                string[] replySplit = reply.Trim().Split(',');
                                // Check success 
                                if (replySplit[1].Trim().Equals("0"))
                                {
                                    //reply = SLABCommunication.sendMessageToSlab("enableSrc " + replySplit[2].Trim().Trim(';') + ",1");
                                    //SLABCommunication.sendMessageToSlab("updateSrcXYZ(" + replySplit[2].Trim().Trim(';') + "," + x + "," + y + "," + z + ")");
                                    reply = "addaudiosource," + replySplit[2].Trim().Trim(';');
                                }
                                else
                                {
                                    reply = "addaudiosource,0";
                                }
                            }
                            else if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer)
                            {
                                reply = SLABCommunication.sendMessageToSlab("allocateASIOSource(" + sourceHRTFID + "," + numberOfChannels + ")");
                                replyCheck = reply.Split(',');

                                if (int.TryParse(replyCheck[1], out replyCode))
                                {
                                    if (replyCode > 0)
                                    {
                                        reply = "addAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE + "," + replyCode;
                                       
                                    }
                                    else
                                    {
                                        reply = "addAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_SLABERRORCODE + "," + replyCode;
                                        break;
                                    }
                                }
                                else
                                {
                                    reply = "addAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_FAILEDTOPARSESLABRESPONSE;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            reply = "addAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_PARAMETEROUTOFRANGE;
                            break;
                        }
                        
                    }
                    /*
                    string presentReply = SLABCommunication.sendMessageToSlab("presentSource(" + replyCode + "," + FLTLocation.x + "," + FLTLocation.y + "," + FLTLocation.z);
                    replyCheck = presentReply.Split(',');

                    if (int.TryParse(replyCheck[1], out replyCode)) {
                        if (replyCode != 0) {
                            reply = "addAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_FAILEDINITIALIZELOCATION;
                        }
                    }
                    */

                    break;
                case "enablesrc":
                    //enablesrc
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    //UnityEngine.Debug.Log(paramList[1].ToLower().Length);

                    if (paramList[1].ToLower().Equals("t"))
                    { 
                        if(ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3)
                            reply = SLABCommunication.sendMessageToSlab("enableSrc " + paramList[0] + ",1");
                        else if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer)
                            reply = SLABCommunication.sendMessageToSlab("enableSource(" + paramList[0] + ",1)");
                    }
                    else if (paramList[1].ToLower().Equals("f"))
                    {
                        if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3)
                            reply = SLABCommunication.sendMessageToSlab("enableSrc " + paramList[0] + ",0");
                        else if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer)
                            reply = SLABCommunication.sendMessageToSlab("enableSource(" + paramList[0] + ",0)");
                    }
                    else reply = "-1";
                    reply = reply.Trim().Split(',')[0].Trim() + "," + reply.Trim().Split(',')[1].TrimStart();

                    break;
                case "startrendering":
                    //start
                    if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3)
                    {
                        reply = SLABCommunication.sendMessageToSlab("start");
                        if (reply.Trim().Split(',')[1].Trim().Equals("0;"))
                        {
                            gameObject.GetComponent<SLABCommunication>().isRendering = true;

                        }
                    }
                    else if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer)
                    {
                        reply = SLABCommunication.sendMessageToSlab("startRendering(0,0)");

                        replyCheck = reply.Split(',');

                        if (int.TryParse(replyCheck[1], out replyCode))
                        {
                            if (replyCode == 0)
                            {
                                reply = "startRendering," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE;
                            }
                            else
                            {
                                reply = "startRendering," + (int)ERRORMESSAGES.ErrorType.ERR_AS_SLABERRORCODE + "," + replyCode;
                                break;
                            }
                        }
                        else
                        {
                            reply = "startRendering," + (int)ERRORMESSAGES.ErrorType.ERR_AS_FAILEDTOPARSESLABRESPONSE;
                            break;
                        }
                        
                    }
                    SLABCommunication.sendMessageToSlab("presentSource(1,0,10,0)");
                    SLABCommunication.sendMessageToSlab("muteSource(1,0)");
                    //File.WriteAllText(".\\logtext.txt", reply.Trim().Split(',')[1].Trim());

                    /*
                    reply = SLABCommunication.sendMessageToSlab(mC.message);
                    foreach (string key in sourcesToInitOnRender.Keys)
                    {
                        Vector3 loc = sourcesToInitOnRender[key];
                        //SLABCommunication.sendMessageToSlab("presentSource(" + key + "," + loc.x + "," + loc.y + "," + loc.z + ")");
                        //SLABCommunication.sendMessageToSlab("setSourceGain(" + key + ", 0)");
                        //SLABCommunication.sendMessageToSlab("muteSource(" + key + ",0)");
                    }
                    sourcesToInitOnRender.Clear();
                    */
                    break;
                case "endrendering":
                //stop
                case "reset":
                    //exit and restart
                    gameObject.GetComponent<SLABCommunication>().Reset();
                    reply = "1";
                    break;
                case "definefront":
                    UnityEngine.VR.InputTracking.Recenter();
                    reply = "1";
                    break;
                case "adjustsourcelevel":
                    //adjsrcgain

                    paramList = match.Groups[2].Value.Trim().Split(',');
                    reply = SLABCommunication.sendMessageToSlab("adjsrcgain " + paramList[0] + "," + paramList[1] + " ");
                    reply = reply.Trim().Split(',')[0].Trim() + "," + reply.Trim().Split(',')[1].TrimStart();
                    break;
                case "adjustoveralllevel":
                //adjsrcgain for each current source
                case "adjustsourceposition":
                    //presentsrcxyz
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    x = paramList[1].Trim('[');
                    y = paramList[2];
                    z = paramList[3].Trim(']');
                    reply = SLABCommunication.sendMessageToSlab("updateSrcXYZ " + paramList[0].Trim().Trim(';') + "," + x + "," + y + "," + z + "");
                    reply = reply.Trim().Split(',')[0].Trim() + "," + reply.Trim().Split(',')[1].TrimStart();
                    break;
                case "muteaudiosource":
                    paramList = match.Groups[2].Value.Trim().Split(',');


                    if (paramList[1].ToLower().Equals("t"))
                    {
                        if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3)
                        {
                            reply = SLABCommunication.sendMessageToSlab("muteSrc " + paramList[0] + ",1");
                        }
                        else if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer)
                        {
                            reply = SLABCommunication.sendMessageToSlab("muteSource" + paramList[0] + ",1");
                        }

                    }
                    else if (paramList[1].ToLower().Equals("f"))
                    {
                        if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.AudioServer3)
                        {
                            reply = SLABCommunication.sendMessageToSlab("muteSrc " + paramList[0] + ",0");
                        }
                        else if (ConfigurationUtil.engineType == ConfigurationUtil.AudioEngineType.SLABServer)
                        {
                            reply = SLABCommunication.sendMessageToSlab("muteSource" + paramList[0] + ",0");
                        }
                    }
                    else
                        reply = "muteAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_CMDSYN;
                    break;
                //mutesrc
                case "setleds":
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    int ledIndex = 0;
                    GameObject ledHarness = null;
                    int speakerID;
                    if (paramList[0].Contains("["))
                    {

                        ledIndex = 3;
                        x = paramList[0].Trim('[');
                        y = paramList[1];
                        z = paramList[2].Trim(']');


                        xF = 0;
                        yF = 0;
                        zF = 0;
                        if (!float.TryParse(x, out xF))
                        {
                            reply = "setleds," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                            break;
                        }
                        if (!float.TryParse(y, out yF))
                        {
                            reply = "setleds," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                            break;
                        }
                        if (!float.TryParse(z, out zF))
                        {
                            reply = "setleds," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                            break;
                        }

                        ledHarness = GetComponent<ALFLeds>().getNearestSpeaker(HelperFunctions.FLTToUnityXYZ(new Vector3(xF, yF, zF)));

                    }
                    else if (int.TryParse(paramList[0], out speakerID))
                    {
                        ledIndex = 1;
                        ledHarness = GetComponent<ALFLeds>().getSpeakerByID(paramList[0]);

                    }
                    else {
                        reply = "setleds," + (int)ERRORMESSAGES.ErrorType.ERR_AS_CMDSYN;
                    }

                    int maskW;
                    int maskX; //= paramList[ledIndex].Trim('[');
                    int maskY; //= paramList[ledIndex];
                    int maskZ; //= paramList[ledIndex].Trim(']');
                    if (!int.TryParse(paramList[ledIndex].Trim('['), out maskW))
                    {
                        reply = "setleds," + (int)ERRORMESSAGES.ErrorType.ERR_AS_MASKPARSEFAILURE;
                        break;
                    }
                    if (!int.TryParse(paramList[ledIndex + 1], out maskX))
                    {
                        reply = "setleds," + (int)ERRORMESSAGES.ErrorType.ERR_AS_MASKPARSEFAILURE;
                        break;
                    }
                    if (!int.TryParse(paramList[ledIndex + 2], out maskY))
                    {
                        reply = "setleds," + (int)ERRORMESSAGES.ErrorType.ERR_AS_MASKPARSEFAILURE;
                        break;
                    }
                    if (!int.TryParse(paramList[ledIndex + 3].Trim(']'), out maskZ))
                    {
                        reply = "setleds," + (int)ERRORMESSAGES.ErrorType.ERR_AS_MASKPARSEFAILURE;
                        break;
                    }

                    if (ledHarness != null)
                    {
                        ledHarness.GetComponent<LEDControls>().HighlightLEDs(System.Convert.ToBoolean(maskW), System.Convert.ToBoolean(maskX), System.Convert.ToBoolean(maskY), System.Convert.ToBoolean(maskZ), true);
                    }
                    else {
                        reply = "setleds," + (int)ERRORMESSAGES.ErrorType.ERR_AS_CMDNOTRECOGNIZED;
                    }
                    reply = "setleds," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE;
                    break;
                case "showfreecursor":
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    if (paramList[0].ToLower().Equals("head"))
                    {
                        if (paramList[1].ToLower().Equals("t"))
                        {
                            ConfigurationUtil.currentCursorType = ConfigurationUtil.CursorType.crosshair;
                            ConfigurationUtil.currentCursorAttachment = ConfigurationUtil.CursorAttachment.hmd;

                        }
                        else if (paramList[1].ToLower().Equals("f"))
                        {

                            GetComponent<SLABCommunication>().TurnOffCursor();
                            ConfigurationUtil.currentCursorType = ConfigurationUtil.CursorType.none;
                            ConfigurationUtil.currentCursorAttachment = ConfigurationUtil.CursorAttachment.hmd;

                        }
                        else
                        {
                            reply = "showfreecursor," + (int)ERRORMESSAGES.ErrorType.ERR_AS_BOOLPARSEFAILURE;
                        }
                    }
                    else if (paramList[0].ToLower().Equals("hand"))
                    {
                        if (paramList[1].ToLower().Equals("t"))
                        {
                            ConfigurationUtil.currentCursorType = ConfigurationUtil.CursorType.crosshair;
                            ConfigurationUtil.currentCursorAttachment = ConfigurationUtil.CursorAttachment.hand;

                        }
                        else if (paramList[1].ToLower().Equals("f"))
                        {

                            GetComponent<SLABCommunication>().TurnOffCursor();
                            ConfigurationUtil.currentCursorType = ConfigurationUtil.CursorType.none;
                            ConfigurationUtil.currentCursorAttachment = ConfigurationUtil.CursorAttachment.hand;

                        }
                        else
                        {
                            reply = "showfreecursor," + (int)ERRORMESSAGES.ErrorType.ERR_AS_BOOLPARSEFAILURE;
                        }


                    }
                    else {
                        reply = "showfreecursor," + (int)ERRORMESSAGES.ErrorType.ERR_AS_PARAMETEROUTOFRANGE;
                    }
                    reply = "showfreecursor," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE;

                    break;
                case "showsnappedcursor":
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    if (paramList[0].ToLower().Equals("head"))
                    {
                        if (paramList[1].ToLower().Equals("t"))
                        {
                            ConfigurationUtil.currentCursorType = ConfigurationUtil.CursorType.snapped;
                            ConfigurationUtil.currentCursorAttachment = ConfigurationUtil.CursorAttachment.hmd;

                        }
                        else if (paramList[1].ToLower().Equals("f"))
                        {

                            GetComponent<SLABCommunication>().TurnOffCursor();
                            ConfigurationUtil.currentCursorType = ConfigurationUtil.CursorType.none;
                            ConfigurationUtil.currentCursorAttachment = ConfigurationUtil.CursorAttachment.hmd;

                        }
                        else
                        {
                            reply = "showsnappedcursor," + (int)ERRORMESSAGES.ErrorType.ERR_AS_BOOLPARSEFAILURE;
                        }
                    }
                    else if (paramList[0].ToLower().Equals("hand"))
                    {
                        if (paramList[1].ToLower().Equals("t"))
                        {
                            ConfigurationUtil.currentCursorType = ConfigurationUtil.CursorType.snapped;
                            ConfigurationUtil.currentCursorAttachment = ConfigurationUtil.CursorAttachment.hand;

                        }
                        else if (paramList[1].ToLower().Equals("f"))
                        {

                            GetComponent<SLABCommunication>().TurnOffSnappedCursor();
                            ConfigurationUtil.currentCursorType = ConfigurationUtil.CursorType.none;
                            ConfigurationUtil.currentCursorAttachment = ConfigurationUtil.CursorAttachment.hand;

                        }
                        else
                        {
                            reply = "showsnappedcursor," + (int)ERRORMESSAGES.ErrorType.ERR_AS_BOOLPARSEFAILURE;
                        }


                    }
                    else
                    {
                        reply = "showsnappedcursor," + (int)ERRORMESSAGES.ErrorType.ERR_AS_PARAMETEROUTOFRANGE;
                    }
                    reply = "showsnappedcursor," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE;
                    break;
                case "addlogtag":
                case "waitforresponse":
                    ConfigurationUtil.waitingForResponse = true;
                    ConfigurationUtil.waitingClient = mC.sender;
                    ConfigurationUtil.waitStartTime = Time.time;
                    reply = "";
                    break;
                case "waitforrecenter":
                    ConfigurationUtil.waitingForRecenter = true;
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    Vector3 target = Vector3.zero;
                    float tol = 0;
                    if (paramList[0].Contains("["))
                    {
                        x = paramList[0].Trim('[');
                        y = paramList[1];
                        z = paramList[2].Trim(']');
                        xF = 0;
                        yF = 0;
                        zF = 0;
                        if (!float.TryParse(x, out xF))
                        {
                            reply = "waitForRecenter," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                            break;
                        }
                        if (!float.TryParse(y, out yF))
                        {
                            reply = "waitForRecenter," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                            break;
                        }
                        if (!float.TryParse(z, out zF))
                        {
                            reply = "waitForRecenter," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                            break;
                        }
                        target = new Vector3(xF, yF, zF);
                        if (!float.TryParse(paramList[3], out tol)) {
                            reply = "waitForRecenter," + (int)ERRORMESSAGES.ErrorType.ERR_AS_PARAMETEROUTOFRANGE;
                            break;

                        }
                    }
                    else if (int.TryParse(paramList[0], out speakerID))
                    {
                        
                        target = GetComponent<ALFLeds>().getSpeakerByID(paramList[0]).transform.position;
                        if (!float.TryParse(paramList[1], out tol))
                        {
                            reply = "waitForRecenter," + (int)ERRORMESSAGES.ErrorType.ERR_AS_PARAMETEROUTOFRANGE;
                            break;

                        }
                    }
                    else
                    {
                        reply = "waitForRecenter," + (int)ERRORMESSAGES.ErrorType.ERR_AS_CMDSYN;
                    }
                    ConfigurationUtil.waitingClient = mC.sender;
                    ConfigurationUtil.recenterPosition = target;
                    ConfigurationUtil.recenterTolerance = tol;
                    reply = "";
                    
                    break;
                case "getheadorientation":
                case "gethead6dof":
                case "getnearestspeaker":
                    paramList = match.Groups[2].Value.Trim().Split(',');

                    x = paramList[0].Trim('[');
                    y = paramList[1];
                    z = paramList[2].Trim(']');
                    
                    float xFl = 0;
                    float yFl = 0;
                    float zFl = 0;
                    if (!float.TryParse(x, out xFl))
                    {
                        reply = "getNearestSpeaker," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                        break;
                    }
                    if (!float.TryParse(y, out yFl))
                    {
                        reply = "getNearestSpeaker," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                        break;
                    }
                    if (!float.TryParse(z, out zFl))
                    {
                        reply = "getNearestSpeaker," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                        break;
                    }
                    reply = "getNearestSpeaker," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE + "," + GetComponent<ALFLeds>().getNearestSpeakerID(HelperFunctions.FLTToUnityXYZ( new Vector3(xFl, yFl, zFl)));
                    
                    break;
                case "getspeakerposition":
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    if (GetComponent<ALFLeds>().getSpeakerByID(paramList[0])!= null) {
                        Vector3 positionVector = HelperFunctions.UnityXYZToFLT(GetComponent<ALFLeds>().getSpeakerByID(paramList[0]).transform.position);
                        reply = "getSpeakerPosition," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE + ",["+positionVector.x+ ","+positionVector.y+ ","+positionVector.z + "]";
                        
                        break;
                    }
                    reply = "getSpeakerPosition," + (int)ERRORMESSAGES.ErrorType.ERR_AS_SPEAKERNOTFOUND;
                    break;
                case "getcurrenthrtf":
                case "highlightlocation":
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    Vector3 locationToMoveHighlighter = Vector3.zero;
                    int colorBaseLocation = 0;
                    Color color = Color.clear;
                    if (paramList.Length < 2)
                    {
                        Highlighter.SetActive(false);
                        reply = "highlightLocation," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE;
                        break;
                    }
                    else {
                        Highlighter.SetActive(true);
                        if (paramList[0].Contains("["))
                        {
                            x = paramList[0].Trim('[');
                            y = paramList[1];
                            z = paramList[2].Trim(']');
                            xF = 0;
                            yF = 0;
                            zF = 0;
                            if (!float.TryParse(x, out xF))
                            {
                                reply = "highlightLocation," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                                break;
                            }
                            if (!float.TryParse(y, out yF))
                            {
                                reply = "highlightLocation," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                                break;
                            }
                            if (!float.TryParse(z, out zF))
                            {
                                reply = "highlightLocation," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                                break;
                            }
                            locationToMoveHighlighter = HelperFunctions.FLTToUnityXYZ(new Vector3(xF, yF, zF));
                            colorBaseLocation = 3;
                        }
                        else if (int.TryParse(paramList[0], out speakerID))
                        {
                            locationToMoveHighlighter = GetComponent<ALFLeds>().getSpeakerByID(speakerID.ToString()).transform.position;
                            colorBaseLocation = 1;
                        }
                        else {
                            reply = "highlightLocation," + (int)ERRORMESSAGES.ErrorType.ERR_AS_CMDSYN;
                            break;
                        }
                        x = paramList[colorBaseLocation].Trim('[');
                        y = paramList[colorBaseLocation+1];
                        z = paramList[colorBaseLocation+2].Trim(']');
                        float rF = 0;
                        float gF = 0;
                        float bF = 0;
                        if (!float.TryParse(x, out rF))
                        {
                            reply = "highlightLocation," + (int)ERRORMESSAGES.ErrorType.ERR_AS_COLORPARSEFAILURE;
                            break;
                        }
                        if (!float.TryParse(y, out gF))
                        {
                            reply = "highlightLocation," + (int)ERRORMESSAGES.ErrorType.ERR_AS_COLORPARSEFAILURE;
                            break;
                        }
                        if (!float.TryParse(z, out bF))
                        {
                            reply = "highlightLocation," + (int)ERRORMESSAGES.ErrorType.ERR_AS_COLORPARSEFAILURE;
                            break;
                        }
                        color = new Color(rF, gF, bF,0.7f);
                        Highlighter.GetComponent<Renderer>().material.SetColor("_Color", color);
                        Highlighter.transform.position = locationToMoveHighlighter;
                        reply = "highlightLocation," + (int)ERRORMESSAGES.ErrorType.ERR_AS_NONE;
                        
                    }


                    break;
                case "displaymessage":
                  
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
                    
                    
                    
                    //MeshRenderer cursorRenderer = cursorObject.GetComponent<MeshRenderer>();
                    //cursorRenderer.material.SetColor(0, new Color(cursorRenderer.material.color.r, cursorRenderer.material.color.g,cursorRenderer.material.color.b, 0.0f));
                    reply = "1";
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
					reply = match.Groups[1].Value.Trim().ToLower() + "," + (int)ERRORMESSAGES.ErrorType.ERR_AS_CMDNOTRECOGNIZED;


					break;






			}
			if (!reply.Equals(""))
			{
                /*
                reply = reply.Replace(";", string.Empty);
                if (ConfigurationUtil.isDebug)
					LogSystem.Log("Sent : " + reply);
				NetworkStream n = new NetworkStream(mC.sender);
				StreamWriter writer = new StreamWriter(n,ASCIIEncoding.ASCII);
				writer.WriteLine(reply);
				writer.Flush();
				writer.Close();
				n.Close();
                */
                sendMessage(reply, mC.sender);
			}





		}
		else
			//Debug.Log("Unknown Command");
			LogSystem.Log("Unable to parse command");



	}
    public void sendMessage(string message, Socket client) {
        string reply = message.Replace(";", string.Empty);
        if (ConfigurationUtil.isDebug)
            LogSystem.Log("Sent : " + reply);
        NetworkStream n = new NetworkStream(client);
        StreamWriter writer = new StreamWriter(n, ASCIIEncoding.ASCII);
        writer.WriteLine(reply);
        writer.Flush();
        writer.Close();
        n.Close();


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
