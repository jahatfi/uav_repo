using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class Initializer : MonoBehaviour {
	//This singleton class opens the file that determines which waypoints will be visited by each UAV.
	//Each UAV will then retrieve that info by calling a function in this script.
	//In addition, the class reads in which targets will appear.
	public List<List<int>> waypointsToVisit;
	public List<List<bool>> targetsToAppear;

	//Singleton stuff
	private static Initializer _instance;
	public static Initializer instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = GameObject.FindObjectOfType<Initializer>();
				
				//Tell unity not to destroy this object when loading a new scene!
				DontDestroyOnLoad(_instance.gameObject);
			}
			
			return _instance;
		}
	}
	// Use this for initialization
	void Awake () {
		if (_instance == null) {
			//initialize the lists.
			waypointsToVisit = new List<List<int>> ();
			targetsToAppear = new List<List<bool>> ();
			//Create the StreamReader
			StreamReader reader = File.OpenText ("configuration.csv");
			//The first line is just the heading - trash it.
			string line = reader.ReadLine ();
			//Debug.Log(line);
			//Now here's the waypoint data stored in a table
			for (int i = 0; i < 16; ++i) {
				line = reader.ReadLine ();
				//Debug.Log(line);
				waypointsToVisit.Add (new List<int> ());
				string[] items = line.Split (',');
				for(int j = 1; j < items.Length; ++j)
					if(items[j] != "") waypointsToVisit [i].Add (int.Parse(items[j]));
			}
			//Trash one newline, and one line of heading
			line = reader.ReadLine ();
			line = reader.ReadLine ();

			for (int i = 0; i < 16; ++i) {
				line = reader.ReadLine ();
				targetsToAppear.Add (new List<bool> ());
				string[] items = line.Split (',');
				for(int j = 1; j < items.Length; ++j)
					if(items[j] != "")  targetsToAppear [i].Add (Convert.ToBoolean (items[j]));
			}
			/*
			string temp;
			for(int i = 0; i < waypointsToVisit.Count; ++i){
				temp = "UAV #" + i + " will visit waypoints: ";
				foreach(int j in waypointsToVisit[i]) temp += j + " ";
				Debug.Log(temp);
			}

			for(int i = 0; i < waypointsToVisit.Count; ++i){
				temp = "Visibliity of targets for UAV #" + i + ": ";
				foreach(bool j in targetsToAppear[i]) temp += j + " ";
				Debug.Log(temp);
			}
			*/
		}
	}

	// Pass the ID's of the waypoints back to the UAV script
	public List<int> GetWaypoints(int uav_id){
		//uav_id: (1-16)
		return waypointsToVisit [uav_id - 1];
	}

	public List<bool> GetTargetVisibiliity(int uav_id){
		return targetsToAppear [uav_id - 1];
	}

}




