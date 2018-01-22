using UnityEngine;
using System.Collections;
using System.IO;

public static class LogSystem {

	// Use this for initialization
	public static void Log(string s){
		using (StreamWriter w = File.AppendText("UnityServerLog.txt")){
			//Debug.Log("Logged soemthing");
			
			w.WriteLine(s);
		
		
		
		
		}
	
	
	
	}

	public static void ClearLog(){
		if(File.Exists("UnityServerLog.txt"))
			File.Delete("UnityServerLog.txt");
	
	
	}






}
