using UnityEngine;
//using UnityEngine.SceneManagement;
// using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class WorldMaker : MonoBehaviour {
	public List<GameObject> sphereLocations;
	public List<GameObject> completeSphereList;
	public GameObject speaker;
	private bool isSLABRendering {get;set;}


	// Use this for initialization
	void Awake () {
		
		isSLABRendering = false;
		
		string[] filedata = System.IO.File.ReadAllLines(".\\config\\ALFGrid.csv");
		float x ;
		float y;
		float z;
		//Dictionary<float,List<GameObject>> rings =  new Dictionary<float,List<GameObject>>();
		int i = 1;
		foreach(string line in filedata){
			//Debug.Log(line);
			string [] tempSplit = line.Trim().Split(',');
			 
			float.TryParse(tempSplit[0],out x);
			float.TryParse(tempSplit[1], out y);
			float.TryParse(tempSplit[2], out z);
		
			//GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			GameObject tempSphere = (GameObject) Instantiate(speaker,Vector3.zero,Quaternion.identity);
			Vector3 position = new Vector3( y* -2.333f,  z* 2.333f, x * 2.333f);
			

			tempSphere.transform.Translate(position);
			//tempSphere.transform.Rotate(Vector3.up,90,Space.World);
			//tempSphere.transform.Rotate(Vector3.forward, 90, Space.World);
			//tempSphere.transform.position = new Vector3(tempSphere.transform.position.x,tempSphere.transform.position.y,-1*tempSphere.transform.position.z);
			//if(SceneManager.GetActiveScene().name.Contains("Lattice"))
				//Debug.Log(SceneManager.GetActiveScene().name);
			
			tempSphere.transform.localScale *=   .08f;
			tempSphere.tag  = "SoundSource";
			tempSphere.name = i.ToString();
			i ++;
			tempSphere.transform.LookAt(Vector3.zero);
			//if(!rings.ContainsKey(position.y)){
			//	rings.Add(position.y,new List<GameObject>());
			
			//}

			//rings[position.y].Add(tempSphere);

			completeSphereList.Add(tempSphere);
			//if(tempSphere.transform.position.y>-1.2)
			sphereLocations.Add(tempSphere);
		}
		completeSphereList.RemoveAt(completeSphereList.Count-1);
		completeSphereList.RemoveAt(completeSphereList.Count - 1);
		completeSphereList.RemoveAt(completeSphereList.Count - 1);
		completeSphereList.RemoveAt(completeSphereList.Count - 1);
		completeSphereList.RemoveAt(completeSphereList.Count - 1);
		CreateLattice();


	
	}
	
	// Update is called once per frame
	void Update () {

	
	}



	//  Create Lattice for sphere given Deictionary of objects, where key is y elevations
	void CreateLattice()
	{	GameObject LatticeParent = GameObject.FindGameObjectWithTag("LatticeParent");
		//List<float> keys = new List<float>(rings.Keys);
		List<GameObject> done = new List<GameObject>();
		//Sort keys to start at bottom
		//keys.Sort();
		foreach(GameObject g in completeSphereList){
			List<GameObject> closest = FindClosest(g,6)
			;
			foreach(GameObject h in closest){

				if(!done.Contains(h)){
					CreateCylinder(g,h,LatticeParent);
				}
			}
			done.Add(g);

		
		}


	}

	//Creates cyclinder between two GameObjects
	void CreateCylinder(GameObject start, GameObject finish, GameObject parent = null){
		GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		Vector3 v3Start = start.transform.position;
		Vector3 v3Finish = finish.transform.position;
		
		c.transform.position = (v3Finish - v3Start)/2.0f + v3Start;

		Vector3 scale = c.transform.localScale;
		scale.y = (v3Finish - v3Start).magnitude/2.0f;
		scale.x = 0.03f;
		scale.z = 0.03f;
		c.transform.localScale = scale;
		c.transform.rotation = Quaternion.FromToRotation(Vector3.up,v3Finish - v3Start);
		if(parent!= null){
			c.transform.SetParent(parent.transform);
		
		}
	
	
	
	}

	List<GameObject> FindClosest(GameObject obj, int numToFind){
		List<GameObject> returnList = new List<GameObject>();
		foreach(GameObject current in completeSphereList){
			
			if(current == obj){
				continue;
			} 
			else if(returnList.Count < numToFind){

				returnList.Add(current);
				
				continue;
			
			} 
			else{
			
				float distance = (obj.transform.position - current.transform.position).magnitude;
				GameObject farthest = null;;
				float farthestDistance = -1;
				foreach(GameObject listObject in returnList){
					float dist = (obj.transform.position - listObject.transform.position).magnitude;
					if(dist > farthestDistance){
						farthest = listObject;
						farthestDistance = dist;
					
					
					}
				}
				if(distance< farthestDistance){
					returnList.Remove(farthest);
					returnList.Add(current);
				
				
				}
			
			
			}
		
		
		
		}



		return returnList;
	
	}
}

/*
	void CreateLattice(Dictionary<float,List<GameObject>> rings){
		List<float> keys = new List<float>(rings.Keys);
		
		//Sort keys to start at bottom
		keys.Sort();
		for (int i = 0; i < keys.Count; i++)
		{
			
			float currentRing = keys[i];
			//handle special case of lowest elevation
			if (i == 0)
			{
				
				GameObject currentSpeaker = rings[currentRing][0];
				foreach (GameObject speaker in rings[keys[i + 1]])
				{
					CreateCylinder(speaker, currentSpeaker);


				}
			
			}
			//handle special case of peak
			else if (i == keys.Count-1){
				
				GameObject currentSpeaker = rings[currentRing][0];
				foreach (GameObject speaker in rings[keys[i -1]])
				{
					CreateCylinder(speaker, currentSpeaker);


				}
			
			} 
			//All inner-Rings
			else{
				int size = rings[currentRing].Count;
				for (int j = 0; j < rings[currentRing].Count; j++)
				{
					if(j == 0){
						CreateCylinder(rings[currentRing][size - 1], rings[currentRing][0]);
						//CreateCylinder(rings[currentRing][j+1], rings[currentRing][0]);
					
					}else if (j == size-1){
						CreateCylinder(rings[currentRing][size - 1], rings[currentRing][size - 2]);
					
					}else{
						CreateCylinder(rings[currentRing][j], rings[currentRing][j+1]);
					
					
					}
					

				}
			
			}


		}
	
	
	}*/
