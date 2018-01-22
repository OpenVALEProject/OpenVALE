/************************************************************************************

Filename    :   Crosshair3D.cs
Content     :   An example of a 3D cursor in the world based on player view
Created     :   June 30, 2014
Authors     :   Andrew Welch

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.


************************************************************************************/

// uncomment this to test the different modes.
#define CROSSHAIR_TESTING

using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.IO;
using System.Text;				// required for Coroutines
using System;

public class Crosshair3D : MonoBehaviour
{

	// NOTE: three different crosshair methods are shown here.  The most comfortable for the
	// user is going to be when the crosshair is located in the world (or slightly in front of)
	// the position where the user's gaze is.  Positioning the cursor a fixed distance from the
	// camera inbetween the camera and the player's gaze will be uncomfortable and unfocused.
	public enum CrosshairMode
	{
		INTERSECT = 0,			// cursor positions itself in 3D based on raycasts into the scene
		NEAREST = 1,		// similar to Dynamic but cursor is only visible for objects in a specific layer
		//FixedDepth = 2,			// cursor positions itself based on camera forward and draws at a fixed depth
	}


	public CrosshairMode mode = CrosshairMode.NEAREST;
	public int objectLayer = 8;
	public float offsetFromObjects = 0.1f;
	public float fixedDepth = 3.0f;
    private float baseFixedDepth;
	//public OVRCameraRig cameraController = null;
    public Camera cameraController = null;
	public GameObject JoystickCamera;
	private Transform thisTransform = null;
	private Material crosshairMaterial = null;
	//private Vector3 mouseCenter = new Vector3(0,0,0); 
	//public Vector3 screenOffset = new Vector3(0,0,0);
	public bool useMouse = true;
    //private GameObject gameController;
    //private WiiMoteData moteData;
    public GameObject gunController;
    public bool useGun = true;
    public Vector3 yPositionOffset = new Vector3(0,0,0);
    public GameObject gunModel;
    private GameObject hitTarget = null;
    public Material highlightMat;
    public Material defaultMat;
	//private SLABCommunication slab;
	//private bool init = false;
	//public WorldMaker w;
	//private GameObject selectedSpeaker = null;

	//private Experiment experiment;
    private GameObject currentNearest = null;
	public GameObject testObject;
    ///public bool isVisible = true;
	/// <summary>
	/// Initialize the crosshair
	/// </summary>
	/// 
		
	
	void Awake()
	{
		thisTransform = transform;
		if (cameraController == null)
		{
			Debug.LogError("ERROR: missing camera controller object on " + name);
			enabled = false;
			return;
		}
		// clone the crosshair material
		crosshairMaterial = GetComponent<Renderer>().material;
		//mouseCenter = Input.mousePosition;
	}
	
    void Start() {
        baseFixedDepth = fixedDepth;
        //gameController = GameObject.FindGameObjectWithTag("GameController");
        //moteData = gameController.GetComponent<WiiMoteData>();
		//experiment = GameObject.FindGameObjectWithTag("GameController").GetComponent<Experiment>();
        if (gunController == null) {

            useGun = false;
        
        }
        if (useGun) { 
            //trackedController.TriggerClicked += new ClickedEventHandler(DoClick);
            //gunController.GetComponent<SteamVR_TrackedController>().TriggerClicked += new ClickedEventHandler(FireShot) ;
        
        }

    }

	/// <summary>
	/// Cleans up the cloned material
	/// </summary>
	void OnDestroy()
	{
		if (crosshairMaterial != null)
		{
			Destroy(crosshairMaterial);
		}
	}

	/// <summary>
	/// Updates the position of the crosshair.
	/// </summary>
	void Update()
	{	//Debug.Log("something");
		//if(init){

		//slab = w.GetComponent<SLABCommunication>();
		//init = false;
		//}


		//Ray ray;
		//RaycastHit hit;

		// get the camera forward vector and position
		Vector3 cameraPosition;
		Vector3 cameraForward ;
		/*
        if(!ConfigurationUtil.useRift){

			cameraPosition =JoystickCamera.transform.position;
			cameraForward= JoystickCamera.transform.forward;
		
		}
         
		else{
         */
		cameraPosition = cameraController.transform.position;
	    cameraForward = cameraController.transform.forward;
		//}
         

		GetComponent<Renderer>().enabled = true;

        
            transform.position = cameraPosition;// new Vector3(0, 5.0, 0);

            //thisTransform.position = cameraPosition + cameraForward * fixedDepth;
            transform.position = transform.position + cameraForward * fixedDepth;

        if (ConfigurationUtil.isUseWand)
        {
            //thisTransform.position = thisTransform.position  +gunController.transform.forward* fixedDepth;
            transform.position = gunModel.transform.position + yPositionOffset + gunModel.transform.forward * fixedDepth;


        }
      

        
            
            
        thisTransform.LookAt(thisTransform.position + cameraController.transform.rotation * Vector3.forward,cameraController.transform.rotation * Vector3.up);
        thisTransform.Rotate(Vector3.right, -90);
		//DebugCommands.DrawRay(thisTransform.position, gunModel.transform.position + yPositionOffset , 50f);
		//DebugCommands.DrawRay(transform.position, cameraController.transform.position , 50f);  
        
            Ray r;
            //if(useGun){
            //	r = new Ray(gunModel.transform.position, thisTransform.position - gunModel.transform.position);
            //}else{
            //r = new Ray(gunModel.transform.position, transform.position - gunModel.transform.position);
            r = new Ray(cameraPosition, transform.position - cameraPosition);
            //}
            //ProjectPointLine(transform.position, cameraController.transform.position, (thisTransform.position-cameraController.transform.position)*50);
            //RaycastHit[] hits = Physics.RaycastAll(r,50);

            GameObject nearest = null;
            float nearDist = 200;
            
                foreach (GameObject g in GameObject.FindGameObjectsWithTag("SoundSource"))
                {
                    float dist = DistancePointLine(g.transform.position, r.origin, r.direction * 10);
                    if (dist < nearDist)
                    {
                        nearDist = dist;
                        nearest = g;


                    }



                }
                
                if (currentNearest != nearest)
                {
                    if (nearest == null) { return;}
                    else if (currentNearest == null) {

                        currentNearest = nearest;
 
                    
                    }
                    //soundObject = WorldVariables.GetSLABObbject(paramList[0].Trim());
                    if (ConfigurationUtil.isTestMode)
                    {
                        Renderer reNew = nearest.GetComponent<Renderer>();
                        Renderer reOld = currentNearest.GetComponent<Renderer>();
                        Material[] matsNew = reNew.materials;
                        Material[] matsOld = reOld.materials;

                        matsNew[0] = highlightMat;
                        reNew.materials = matsNew;


                        matsOld[0] = defaultMat;

                        reOld.materials = matsOld;
                    }
                    currentNearest = nearest;


                }
                if (!ConfigurationUtil.isUseWand && (Input.GetButtonDown("Fire1") || Input.GetMouseButtonDown(0)))
                {
                    FireShot();
                }

            
            
        

        //
        
		//bool sawSpeaker = false;
        
        //switch (mode)
        //{

        //    case CrosshairMode.INTERSECT:
        //        //Debug.Log("Hit: ");
        //if (Input.GetMouseButtonDown(0))]
        //bool detectHit = false;
        
        //if ((moteData.useWii && moteData.getMote().Button.b)|| Input.GetMouseButtonDown(0))
        //if(gameController.GetComponent<WiiMoteData>().getMote().Button.b)
        
          //foreach (RaycastHit rh in hits)
           // {
              //  GameObject hitObject = rh.collider.gameObject;
            //    ///Debug.Log("Target : " + hitObject.name);

                //if ((hitObject.tag.Equals("ConstantTarget") || hitObject.tag.Equals("ZeroTarget")) && hitObject.GetComponent<PlaySounds>().Active())
                //{
                //    hitTarget = hitObject;
                    //Debug.Log("Active Target : " + hitObject.name);
				//	break;

                //}
                //else
                //    hitTarget = null;


            
            //}
         

	}
	private float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
	{

		return Vector3.Magnitude(ProjectPointLine(point, lineStart, lineEnd) - point);

	}

	private Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
	{

		Vector3 rhs = point - lineStart;
		Vector3 vector2 = lineEnd - lineStart;
		float magnitude = vector2.magnitude;
		Vector3 lhs = vector2;
		if (magnitude > 1E-06f)
		{
			lhs = lhs / magnitude;


		}
		float num2 = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0f, magnitude);
		return (lineStart + ((Vector3)(lhs * num2)));


	}
    void FireShot() {
        if (!ConfigurationUtil.inputDevice.Equals("C"))
            return;

        Vector3 gunPos = gunController.transform.position;
        Vector3 slabPosition = new Vector3(gunPos.z, -gunPos.x, gunPos.y);


        //GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");

        //string reply = yaw.ToString() + "," + pitch.ToString() + "," + roll.ToString();
        Vector3 ori2 = SLABCommunication.getObjectOrientation(gunController);
        //Debug.Log(ori);
        string reply2 = ori2.x.ToString() + "," + ori2.y.ToString() + "," + ori2.z.ToString() + slabPosition.x.ToString() + "," + slabPosition.y.ToString() + "," + slabPosition.z.ToString();
        //LogSystem.Log("GetOrientationReponse : " + reply);
        UnityEngine.Debug.Log(reply2);
        if (WorldVariables.waitingForLocalizationResponseLAESpeaker)
        {
            slabPosition = new Vector3(currentNearest.transform.position.z, -currentNearest.transform.position.x, currentNearest.transform.position.y);
            WorldVariables.waitingForLocalizationResponseLAESpeaker = false;
            Socket sender2 = WorldVariables.waitingClient;
            //GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");


            NetworkStream n = new NetworkStream(sender2);
            StreamWriter writer = new StreamWriter(n, ASCIIEncoding.ASCII);
            //string reply = yaw.ToString() + "," + pitch.ToString() + "," + roll.ToString();
            //Vector3 ori = SLABCommunication.getObjectOrientation(gunController);
            Vector2 nearestSpeakerPostionXY = new Vector2(currentNearest.transform.position.z, -currentNearest.transform.position.x);
            float r = nearestSpeakerPostionXY.magnitude;
            Vector2 LAE = new Vector2((float)Math.Atan2(slabPosition.y, slabPosition.x), (float)Math.Atan2(slabPosition.z, r));
            //LAE = new Vector2((float)Math.Atan2(currentNearest.transform.position.z, r), (float)Math.Atan2(currentNearest.transform.position.y, currentNearest.transform.position.x) );
            //Debug.Log(ori);
            string reply = LAE.x.ToString() + "," + LAE.y.ToString();
            LogSystem.Log("GetOrientationReponseLAESpeaker : " + reply);
            writer.WriteLine(reply);
            writer.Flush();
            writer.Close();
            n.Close();


        }
        else {

            return;
        
        
        }
    
    
    }
    /*
    void FireShot(object sender, ClickedEventArgs e) {
        if (!ConfigurationUtil.inputDevice.Equals("W"))
            return;

        Vector3 gunPos  = gunController.transform.position;
        Vector3 slabPosition = new Vector3(gunPos.z,-gunPos.x,gunPos.y);

       
        //GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");

      //string reply = yaw.ToString() + "," + pitch.ToString() + "," + roll.ToString();
        Vector3 ori2 = SLABCommunication.getObjectOrientation(gunController);
        //Debug.Log(ori);
        string reply2 = ori2.x.ToString() + "," + ori2.y.ToString() + "," + ori2.z.ToString() + slabPosition.x.ToString() + "," + slabPosition.y.ToString() + "," + slabPosition.z.ToString();
        //LogSystem.Log("GetOrientationReponse : " + reply);
        UnityEngine.Debug.Log(reply2);
        if (WorldVariables.waitingForLocalizationResponse)
        {
            WorldVariables.waitingForLocalizationResponse = false;
            Socket sender2 = WorldVariables.waitingClient;
            //GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
           

            NetworkStream n = new NetworkStream(sender2);
            StreamWriter writer = new StreamWriter(n, ASCIIEncoding.ASCII);
            //string reply = yaw.ToString() + "," + pitch.ToString() + "," + roll.ToString();
            Vector3 ori = SLABCommunication.getObjectOrientation(gunController);
            //Debug.Log(ori);
            string reply = ori.x.ToString() + "," + ori.y.ToString() + "," + ori.z.ToString() + slabPosition.x.ToString() + "," + slabPosition.y.ToString() + "," + slabPosition.z.ToString();
            LogSystem.Log("GetOrientationReponse : " + reply);
            writer.WriteLine(reply);
            writer.Flush();
            writer.Close();
            n.Close();


        }
        else if (WorldVariables.waitingForLocalizationResponseLAESpeaker)
        {
            slabPosition = new Vector3(currentNearest.transform.position.z, -currentNearest.transform.position.x, currentNearest.transform.position.y);
            WorldVariables.waitingForLocalizationResponseLAESpeaker = false;
            Socket sender2 = WorldVariables.waitingClient;
            //GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");


            NetworkStream n = new NetworkStream(sender2);
            StreamWriter writer = new StreamWriter(n, ASCIIEncoding.ASCII);
            //string reply = yaw.ToString() + "," + pitch.ToString() + "," + roll.ToString();
            //Vector3 ori = SLABCommunication.getObjectOrientation(gunController);
            Vector2 nearestSpeakerPostionXY = new Vector2(currentNearest.transform.position.z,-currentNearest.transform.position.x);
            float r = nearestSpeakerPostionXY.magnitude;
            Vector2 LAE = new Vector2((float)Math.Atan2(slabPosition.y, slabPosition.x), (float)Math.Atan2(slabPosition.z, r));
            //LAE = new Vector2((float)Math.Atan2(currentNearest.transform.position.z, r), (float)Math.Atan2(currentNearest.transform.position.y, currentNearest.transform.position.x) );
            //Debug.Log(ori);
            string reply = LAE.x.ToString() + "," + LAE.y.ToString();
            LogSystem.Log("GetOrientationReponseLAESpeaker : " + reply);
            writer.WriteLine(reply);
            writer.Flush();
            writer.Close();
            n.Close();


        } 
    
    
    }
    */
    public void showCursor(){
        //GetComponent<MeshRenderer>().enabled = false;
        fixedDepth = baseFixedDepth;
    }

    public void hideCursor() {
        Debug.Log("Please Hide");
        //GetComponent<Renderer>().enabled = false;
        fixedDepth = 100.0f;
    
    
    }

}
