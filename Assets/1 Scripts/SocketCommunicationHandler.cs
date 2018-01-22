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
    public bool clientDisconnect = false;
    private List<string> currentSourceList = new List<string>();
    public bool waitingForResponse = false;
    public string responseToSend = "";
    public GameObject Highlighter;
    private ConfigurationUtil.AudioEngineType currentEngine;
    public UIRotator UIDisplay;

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

            Thread tConnectMonitor = new Thread(() => DisconnectMonitor(tempSock));
            clientThreads.Add(tConnectMonitor);
            tConnectMonitor.Start();
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
				//UnityEngine.Debug.Log("Waiting for message");
				//UnityEngine.Debug.Log(incomingMessage);
				string incomingMessage = reader.ReadLine();
				
                if (incomingMessage == null)
                    break;
				lock (messageListLock)
				{
					//Debug.Log(incomingMessage);
        		    clientMessages.Add(new MessageContainer(s, incomingMessage));

				}
                
			}
            
		}
        //UnityEngine.Debug.Log("falling out of thread");
        //clientDisconnect = true;
        //gameObject.GetComponent<SLABCommunication>().Reset();



	}
    private void DisconnectMonitor(Socket s)
    {
        //NetworkStream n = new NetworkStream(s);
        //MessageContainer mC;//= new MessageContainer();


       
        while (s.Connected)
        {
            Thread.Sleep(300);
        }

       
        //UnityEngine.Debug.Log("falling out of thread");
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
                    Vector3 FLTLocation = new Vector3(xF, yF, zF);
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
                        SourceInformation newSource = new SourceInformation();
                        newSource.sourceID = replyCode;
                        newSource.hrtfID = sourceHRTFID;
                        newSource.FLTPosition = FLTLocation;
                        GetComponent<SLABCommunication>().AddSourceInformation(newSource);

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
                        SourceInformation newSource = new SourceInformation();
                        newSource.sourceID = replyCode;
                        newSource.hrtfID = sourceHRTFID;
                        newSource.FLTPosition = FLTLocation;
                        GetComponent<SLABCommunication>().AddSourceInformation(newSource);
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
                                // number fo channels is number of sources for the asio device
                                // reply becomes source 1
                                // call allocatelinkedsource(which source, whichChannel, which hrtf)
                                // whichChannel is 
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
                                SourceInformation newSource = new SourceInformation();
                                newSource.sourceID = replyCode;
                                newSource.hrtfID = sourceHRTFID;
                                newSource.FLTPosition = FLTLocation;
                                GetComponent<SLABCommunication>().AddSourceInformation(newSource);
                                int sourceID = replyCode;
                                for (int otherSources = 1; otherSources < numberOfChannels; otherSources++)
                                {
                                    string linkedReply;
                                    linkedReply = SLABCommunication.sendMessageToSlab("allocateLinkedSource(" + sourceID + "," + otherSources+ "," + sourceHRTFID+ ")");
                                    replyCheck = linkedReply.Split(',');

                                    if (int.TryParse(replyCheck[1], out replyCode))
                                    {
                                        if (replyCode > 0)
                                        {
                                            reply += ("," + replyCode);

                                        }
                                        else
                                        {
                                            reply = "allocateLinkedSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_SLABERRORCODE + "," + replyCode;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        reply = "allocateLinkedSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_FAILEDTOPARSESLABRESPONSE;
                                        break;
                                    }
                                    SourceInformation newLinkedSource = new SourceInformation();
                                    newLinkedSource.sourceID = replyCode;
                                    newLinkedSource.hrtfID = sourceHRTFID;
                                    newLinkedSource.FLTPosition = FLTLocation;
                                    GetComponent<SLABCommunication>().AddSourceInformation(newLinkedSource);
                                }
                                
                            }
                        }
                        else
                        {
                            reply = "addAudioSource," + (int)ERRORMESSAGES.ErrorType.ERR_AS_PARAMETEROUTOFRANGE;
                            break;
                        }
                        
                    }
                    sourcesToInitOnRender.Add(replyCode.ToString(),FLTLocation);
                    //string presentReply = SLABCommunication.sendMessageToSlab("presentSource(" + replyCode + "," + FLTLocation.x + "," + FLTLocation.y + "," + FLTLocation.z+ ")");
                    //replyCheck = presentReply.Split(',');
                    //SLABCommunication.sendMessageToSlab("muteSource(1,0)");
                    /*
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
                    Vector3 positionValue;
                    foreach (string s in sourcesToInitOnRender.Keys)
                    {
                        positionValue = sourcesToInitOnRender[s];
                        SLABCommunication.sendMessageToSlab("presentSource(" + s + "," + positionValue.x+"," + positionValue.y + "," + positionValue.z + ")");
                    }
                    sourcesToInitOnRender.Clear();
                    //SLABCommunication.sendMessageToSlab("muteSource(1,0)");
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
                    reply = "reset,0";
                    break;
                case "definefront":
                    UnityEngine.VR.InputTracking.Recenter();
                    reply = "definefront,0";
                    break;
                case "adjustsourcelevel":
                    //adjsrcgain
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    reply = SLABCommunication.sendMessageToSlab("adjustSourceGain(" + paramList[0] + "," + paramList[1] + ")");
                    reply = "adjustSourceLevel," + reply.Trim().Split(',')[1].TrimStart();
                    break;
                case "adjustoveralllevel":
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    //adjsrcgain for each current source
                    foreach (SourceInformation si in GetComponent<SLABCommunication>().currentSources.Values) {
                        reply = SLABCommunication.sendMessageToSlab("adjustSourceGain(" + si.sourceID + "," + paramList[0] + "");

                    }
                    reply = "adjustoveralllevel,0";
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
                            reply = SLABCommunication.sendMessageToSlab("muteSource(" + paramList[0] + ",1)");
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
                            reply = SLABCommunication.sendMessageToSlab("muteSource(" + paramList[0] + ",0)");
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
                    
                    Vector3 orientationHead = Vector3.zero;
                    if (ConfigurationUtil.useRift)
                    {
                        orientationHead = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye).eulerAngles;
                    }
                    else {
                        orientationHead = Camera.main.transform.rotation.eulerAngles;
                    }
                    reply = "getHeadOrientation,0," + "[" + orientationHead.x + "," + orientationHead.y + "," + orientationHead.z + "]";
                    break;
                case "gethead6dof":
                    Vector3 position6DOF = Vector3.zero;
                    Vector3 orientation6DOF = Vector3.zero;
                    if (ConfigurationUtil.useRift) {
                        position6DOF = UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.CenterEye);
                        orientation6DOF = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye).eulerAngles;
                    }
                    else
                    {
                        position6DOF = Camera.main.transform.position;
                        orientation6DOF = Camera.main.transform.rotation.eulerAngles;
                    }
                    if (position6DOF != null)
                    position6DOF = HelperFunctions.UnityXYZToFLT(position6DOF);
                    reply = "getHead6DOF,0," + "[" + position6DOF.x + "," + position6DOF.y + "," + position6DOF.z + "]," + "[" + orientation6DOF.x + "," + orientation6DOF.y + "," + orientation6DOF.z + "]";
                    break;
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
                case "adjustsourceposition":
                    //adjustSourcePosition(src, pos)
                    
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    Vector3 newSourcePosition = Vector3.zero;
                    if (paramList[1].Contains("["))
                    {
                        x = paramList[1].Trim('[');
                        y = paramList[2];
                        z = paramList[3].Trim(']');
                        xF = 0;
                        yF = 0;
                        zF = 0;
                        if (!float.TryParse(x, out xF))
                        {
                            reply = "adjustSourcePosition," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                            UnityEngine.Debug.Log("Failed adjustSourcePosition : ");
                            break;
                        }
                        if (!float.TryParse(y, out yF))
                        {
                            reply = "adjustSourcePosition," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                            UnityEngine.Debug.Log("Failed adjustSourcePosition : ");
                            break;
                        }
                        if (!float.TryParse(z, out zF))
                        {
                            reply = "adjustSourcePosition," + (int)ERRORMESSAGES.ErrorType.ERR_AS_XYZPARSEFAILURE;
                            UnityEngine.Debug.Log("Failed adjustSourcePosition : ");
                            break;
                        }
                        newSourcePosition = new Vector3(xF, yF, zF);
                      
                    }
                    else if (int.TryParse(paramList[1], out speakerID))
                    {

                        newSourcePosition = GetComponent<ALFLeds>().getSpeakerByID(paramList[0]).transform.position;
                        newSourcePosition = HelperFunctions.UnityXYZToFLT(newSourcePosition);
                        
                    }
                    
                    else
                    {
                        reply = "adjustSourcePosition," + (int)ERRORMESSAGES.ErrorType.ERR_AS_CMDSYN;
                        break;
                    }
                    int adjustSourceID = 0;
                    if (int.TryParse(paramList[0], out adjustSourceID))
                    {

                        GetComponent<SLABCommunication>().UpdateSourcePosition(adjustSourceID, newSourcePosition);
                        
                    }
                    else
                    {
                        reply = "adjustSourcePosition," + (int)ERRORMESSAGES.ErrorType.ERR_AS_SRCIDMUSTBEINTEGER;
                        break;
                    }
                    //pos = new Vector3(float.Parse(y) * -1, float.Parse(z), float.Parse(x));

                    //slabX = pos.z;
                    //slabY = -pos.x;
                    //slabZ = pos.y;


                    message = "updateSource(" + paramList[0].Trim() +
                        "," + newSourcePosition.x+ "," + newSourcePosition.y+ "," + newSourcePosition.z+ ")";
                    //UnityEngine.Debug.Log(message);
                    //pos = new Vector3(float.Parse(y) * -1, float.Parse(z), float.Parse(x));
                    //soundObject = WorldVariables.GetSLABObbject(paramList[0].Trim());
                    //soundObject.transform.localPosition = pos;

                    string adjustSourceReply = SLABCommunication.sendMessageToSlab(message);
                    replyCheck = adjustSourceReply.Split(',');
                    //UnityEngine.Debug.Log(adjustSourceReply);
                    reply = "adjustSourcePosition,";
                    if (int.TryParse(replyCheck[1], out replyCode))
                    {
                        if (replyCode == 0)
                        {
                            reply += ("," + replyCode);

                        }
                        else
                        {
                            reply = "adjustSourcePosition," + (int)ERRORMESSAGES.ErrorType.ERR_AS_SLABERRORCODE + "," + replyCode;
                            break;
                        }
                    }
                    else
                    {
                        reply = "adjustSourcePosition," + (int)ERRORMESSAGES.ErrorType.ERR_AS_FAILEDTOPARSESLABRESPONSE;
                        break;
                    }

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
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    if (paramList.Length > 0)
                    {
                        UIDisplay.setMessage(paramList[0]);
                    }
                    else
                    {
                        UIDisplay.hideMessage();
                    }
                    reply = "displayMessage,0";
                    break;
             
				

				
				case "adjustSourceLevel":
					//adjustSourceLevel(src, relativeLevel)
					paramList = match.Groups[2].Value.Trim().Split(',');
					message = "adjustSourceGain(" + paramList[0].Trim() +
										"," + paramList[1].Trim() + ")";
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
                case "guesswho":
                   
                    GuessWho componentGuessWho = GetComponent<GuessWho>();
                    paramList = match.Groups[2].Value.Trim().Split(',');
                    GuessWho.TrialType tType = GuessWho.TrialType.LOCALIZE_LAST_VOICE;
                    int trialINT = 0;
                    if (int.TryParse(paramList[0], out trialINT))
                    {
                        switch (trialINT) {
                            case 4:
                                tType = GuessWho.TrialType.LOCALIZE_LAST_VOICE;
                                break;
                            case 2:
                                tType = GuessWho.TrialType.LOCALIZE_BY_ID;
                                break;
                            case 1:
                                tType = GuessWho.TrialType.ID_BY_LOCATION;
                                break;
                            case 3:
                                tType = GuessWho.TrialType.ID_LAST_VOICE;
                                break;
                            case 5:
                                tType = GuessWho.TrialType.ALL_FINISHED;
                                break;
                            case 0:
                                tType = GuessWho.TrialType.STARTUP_TRIAL;
                                break;
                            default:
                                reply = "GuessWho," + (int)GuessWho.ErrorMessage.UNKNOWN_TRIAL_TYPE + "," + trialINT;
                                break;
                        }
                        componentGuessWho.trialType = tType;
                    }
                    else {
                        reply = "GuessWho," + (int)GuessWho.ErrorMessage.UNKNOWN_TRIAL_TYPE + "," + paramList[0];
                        break;
                    }
                    if (paramList[1].ToLower().Equals("t"))
                    {
                        componentGuessWho.giveFeedback = true;
                    }
                    else if (paramList[1].ToLower().Equals("f")) {
                        componentGuessWho.giveFeedback = false;
                    }
                    else {
                        reply = "GuessWho," + (int)GuessWho.ErrorMessage.UNKNOWN_FEEDBACK_TYPE + "," + paramList[1];
                        break;
                    }
                    int correctFace;
                    if (paramList[2].Contains("["))
                    {
                        x = paramList[2].Trim('[');
                        y = paramList[3];
                        z = paramList[4].Trim(']');
                        xF = 0;
                        yF = 0;
                        zF = 0;
                        if (!float.TryParse(x, out xF))
                        {
                            reply = "GuessWho," + (int)GuessWho.ErrorMessage.UNKNOWN_POSITION_VALUE + "," + x;
                            break;
                        }
                        if (!float.TryParse(y, out yF))
                        {
                            reply = "GuessWho," + (int)GuessWho.ErrorMessage.UNKNOWN_POSITION_VALUE + "," + y;
                            break;
                        }
                        if (!float.TryParse(z, out zF))
                        {
                            reply = "GuessWho," + (int)GuessWho.ErrorMessage.UNKNOWN_POSITION_VALUE + "," + z;
                            break;
                        }

                        componentGuessWho.correctPosition = HelperFunctions.FLTToUnityXYZ(new Vector3(xF, yF, zF));
                        componentGuessWho.DisplayMessage(tType);

                    }
                    else
                    {
                        reply = "GuessWho," + (int)GuessWho.ErrorMessage.UNKNOWN_CORRECT_VALUE + "," + paramList[2];
                        break;
                    }
                    if (int.TryParse(paramList[5], out correctFace))
                    {
                        componentGuessWho.correctFacenumber = correctFace;

                    }
                    else
                    {
                        reply = "GuessWho," + (int)GuessWho.ErrorMessage.UNKNOWN_CORRECT_VALUE + "," + paramList[5];
                        break;
                    }

                    if (tType == GuessWho.TrialType.STARTUP_TRIAL)
                    {
                        componentGuessWho.DisplayMessage(GuessWho.TrialType.STARTUP_TRIAL);
                        reply = "";

                    }
                    if (tType == GuessWho.TrialType.ID_BY_LOCATION) {
                        componentGuessWho.DisplayMessage(GuessWho.TrialType.ID_BY_LOCATION);
                        componentGuessWho.SetCenterFace(-1);
                        componentGuessWho.SetHighlightedOrb(componentGuessWho.correctPosition);
                        
                    }
                    if (tType == GuessWho.TrialType.ID_LAST_VOICE)
                    {
                        componentGuessWho.DisplayMessage(GuessWho.TrialType.ID_LAST_VOICE);
                        componentGuessWho.SetCenterFace(-1);
                        componentGuessWho.SetHighlightedOrb(Vector3.zero);

                    }
                    if (tType == GuessWho.TrialType.LOCALIZE_BY_ID)
                    {
                        componentGuessWho.DisplayMessage(GuessWho.TrialType.LOCALIZE_BY_ID);
                        componentGuessWho.SetCenterFace(componentGuessWho.correctFacenumber);
                        componentGuessWho.SetHighlightedOrb(Vector3.zero);

                    }
                    if (tType == GuessWho.TrialType.LOCALIZE_LAST_VOICE)
                    {
                        componentGuessWho.DisplayMessage(GuessWho.TrialType.LOCALIZE_LAST_VOICE);
                        componentGuessWho.SetCenterFace(-1);
                        componentGuessWho.SetHighlightedOrb(Vector3.zero);
                    }
                    if (tType != GuessWho.TrialType.ALL_FINISHED)
                    {
                        componentGuessWho.trialStartTime = Time.time;
                        componentGuessWho.isWaitingForResponse = true;
                        componentGuessWho.waitingClient = mC.sender;
                        reply = "";
                    }
                    else {
                        componentGuessWho.DisplayMessage(GuessWho.TrialType.ALL_FINISHED);
                        componentGuessWho.isWaitingForResponse = false;
                        gameObject.GetComponent<SLABCommunication>().Reset();
                        reply = "GuessWho,0,Complete";
                    }
                    
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
