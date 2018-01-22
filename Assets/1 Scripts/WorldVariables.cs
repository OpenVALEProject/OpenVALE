using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

public static class WorldVariables{
	public static Dictionary<string,GameObject> slabSourceIDToGameObject = new Dictionary<string,GameObject>();
	public static bool waitingForLocalizationResponse = false;
    public static bool waitingForLocalizationResponseLAE = false;
    public static bool waitingForLocalizationResponseLAESpeaker = false;
	public static Socket waitingClient = null;
    public static Dictionary<int, SourceInformation> Sources= new Dictionary<int, SourceInformation>();
    public static void AddSLABObbject(string key, GameObject visibleObject){

        if (!slabSourceIDToGameObject.ContainsKey(key))
            slabSourceIDToGameObject.Add(key, visibleObject);
        else
            slabSourceIDToGameObject[key] = visibleObject;
	
	}
	public static GameObject GetSLABObbject(string key)
	{


		return slabSourceIDToGameObject[key];

	}
    public static void clearObjects() {

        foreach (string s in slabSourceIDToGameObject.Keys) {

            Object.Destroy(slabSourceIDToGameObject[s]);
        
        
        
        }
        slabSourceIDToGameObject.Clear();
    
    
    
    }

	
}
