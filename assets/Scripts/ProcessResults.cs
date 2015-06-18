using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

//So this class DEFINITELY needs to be a singleton
public class ProcessResults : MonoBehaviour {

	public int uavs = 16;
	public int waypointsVisited = 4;
	public string fileName;
	public string userID;
	
	private bool[,] visibleObjects; //True means an object was visible, and vice-versa
	private int[,] userResponses;
	private double[,] responseTimes; 
	/*
	0: No Response
	1: User says an object was present
	2: User says no object was present
	3: User was unsure
	*/
	private int[,] finalResults;
	/*
	0: No response -10 points
	1: False Positive  -5 points
	2: False Negative  -5 points
	3: User was unsure -3 points
	4: Correct response +10 points
	*/
	private int count = 0;  //Keep track of the number of responses recieved from the uav scripts


	//Singleton stuff
	private static ProcessResults _instance;
	public static ProcessResults instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = GameObject.FindObjectOfType<ProcessResults >();
				
				//Tell unity not to destroy this object when loading a new scene!
				DontDestroyOnLoad(_instance.gameObject);
			}
			
			return _instance;
		}
	}
	void Awake() 
	{
		if(_instance == null)
		{
			//If I am the first instance, make me the Singleton
			count = 0;
			_instance = this;
			DontDestroyOnLoad(this);
			visibleObjects = new bool[uavs,waypointsVisited];
			userResponses = new int[uavs,waypointsVisited]; 
			finalResults = new int[uavs,waypointsVisited];
			responseTimes = new double[uavs, waypointsVisited];
			//Debug.Log(String.Format("Waking up processor. uav: {0} Waypoints: {1}", uavs, waypointsVisited));
		}
		else
		{
			//If a Singleton already exists and you find
			//another reference in scene, destroy it!
			if(this != _instance)
				Destroy(this.gameObject);
		}
	}

	public void ReceiveResults(int id, ref List<bool> visible, ref List<int> results, ref List<double> reactionTimes){
		++count;
		Debug.Log(String.Format("Recieving results from UAV #"+id));
		for (int i = 0; i < waypointsVisited; ++i) {
			visibleObjects [id-1,i] = visible[i];
			userResponses [id-1,i] = results[i];
			responseTimes [id-1,i] = reactionTimes[i];
		}
		if (count == uavs) AnalyzeResults ();
		//AnalyzeResults ();
	}

	private void AnalyzeResults(){
		Debug.Log(String.Format("Analyzing results!"));
		/*
	0: No response -10 points
	1: False Positive  -5 points
	2: False Negative  -5 points
	3: User was unsure -3 points
	4: Correct response +10 points
	*/
		int noResponse = 0;
		int falsePositive = 0;
		int falseNegative = 0;
		int unsure = 0;
		int correct = 0;

		// DO math and stuff here
		for (int i = 0; i < uavs; ++i) {
			for (int j = 0; j < waypointsVisited; ++j) {


				//There was an object visible
				if (visibleObjects [i, j]) {
					switch (userResponses [i, j]) {
					case 0: //The user didn't give a response
						finalResults [i, j] = 0;
						++noResponse;
						break;
					case 1: //The user saw the object
						finalResults [i, j] = 4;
						++correct;
						break;
					case 2: //The user didn't see it
						finalResults [i, j] = 2;
						++falseNegative;
						break;
					case 3: //The user wan't sure
						finalResults [i, j] = 3;
						++unsure;
						break;
					default:
						Debug.Log (string.Format ("Unexpected value in results file."));
						break;
					}
				}

				//There was no object visible
				else {
					switch (userResponses [i, j]) {
					case 0: //The user didn't give a response
						finalResults [i, j] = 0;
						++noResponse;
						break;
					case 1: //The user mistakenly thought they saw an object
						finalResults [i, j] = 1;
						++falsePositive;
						break;
					case 2: //The user correctly stated that there was no object present
						finalResults [i, j] = 4;
						++correct;
						break;
					case 3: //The user wan't sure
						finalResults [i, j] = 3;
						++unsure;
						break;
					default:
						Debug.Log (string.Format ("Unexpected value in results file."));
						break;
					}//switch
				}//else
			}//j
		}//i


		DateTime currentTime = DateTime.Now;
		using (StreamWriter sr = new StreamWriter(fileName)) {
			sr.WriteLine ("Results file for: " + userID);
			sr.WriteLine ("Time: " + currentTime.ToString ());
			sr.WriteLine ("# of correct responses: {0}", correct);
			sr.WriteLine ("# of false positive responses: {0}", falsePositive);
			sr.WriteLine ("# of false negative responses: {0}", falseNegative);
			sr.WriteLine ("# of 'unsure' responses: {0}", unsure);
			sr.WriteLine ("# of non-responses: {0}\r\n", noResponse);
			sr.WriteLine ("Detailed results:");
			sr.WriteLine ("Each row represents a UAV, each column, the waypoints each one visited in order.");
			sr.WriteLine ("0: No response -10 points?");
			sr.WriteLine ("1: False Positive  -5 points?");
			sr.WriteLine ("2: False Negative  -5 points?");
			sr.WriteLine ("3: User was unsure -3 points?");
			sr.WriteLine ("4: Correct response +10 points?\r\n");

			sr.Write ("Waypoint#:");
			for (int i = 0; i < waypointsVisited; ++i)
				sr.Write (i + " ");
			sr.WriteLine ("\r\n{0,18}", "+-+-+-+-+");

			for (int i = 0; i < uavs; ++i) {
				sr.Write ("UAV #{0,2}: ", i);
				//sr.WriteLine("UAV #{0}: ", i);
				for (int j = 0; j < waypointsVisited; ++j) {
					sr.Write ("|" + finalResults [i, j]);
				}
				//sr.WriteLine("|");
				sr.WriteLine ("|\r\n{0,18}", "+-+-+-+-+");
			}

			ChatScript chatHistory = ChatScript.instance;

			if (chatHistory) {
				List<string> messages = chatHistory.getMessages ();
				sr.WriteLine ("\r\n\r\nChat History:");
				foreach (string m in messages)
					sr.WriteLine (m);
			} else
				Debug.Log ("chatHistory is null!");

			sr.Close();
		}
		//Now write the response times to another file
		using (StreamWriter sr = new StreamWriter("Response_times_for_"+ userID+".txt")) {
			//sr.WriteLine ("Response times for: " + userID);
			sr.WriteLine ("Time: " + currentTime.ToString ()+"\r\n");
			//#TODO	DON"T LEAVE THIS HARDCODED! #
			sr.Write ("Waypoint#:  0     1     2     3");
			sr.WriteLine ("\r\n{0,34}", "+-----+-----+-----+-----+");
			
			for (int i = 0; i < uavs; ++i) {
				sr.Write ("UAV #{0,2}: ", i);
				//sr.WriteLine("UAV #{0}: ", i);
				for (int j = 0; j < waypointsVisited; ++j) {
					sr.Write ("|{0,5:0.000}", responseTimes [i, j]);
				}
				//sr.WriteLine("|");
				sr.WriteLine ("|\r\n{0,34}", "+-----+-----+-----+-----+");
			}
		}

		//Now for the .csv files
		//Now write the response times to another file
		using (StreamWriter sr = new StreamWriter("Response_times_for_"+ userID+".csv")) {
			//sr.WriteLine ("Response times for: " + userID);
			sr.WriteLine ("Time: " + currentTime.ToString ()+"\r\n");
			sr.Write ("Waypoint#:,");
			for(int i = 0; i < waypointsVisited; ++i) sr.Write("{0},", i);
			sr.WriteLine ("");
			
			for (int i = 0; i < uavs; ++i) {
				sr.Write ("UAV #{0}:, ", i);
				//sr.WriteLine("UAV #{0}: ", i);
				for (int j = 0; j < waypointsVisited; ++j) {
					sr.Write ("{0,5:0.000},", responseTimes [i, j]);
				}
				sr.WriteLine ("");
			}
		}

		//Now write the response codes to another file
		using (StreamWriter sr = new StreamWriter("Responses_for_"+ userID+".csv")) {
			//sr.WriteLine ("Response times for: " + userID);
			sr.WriteLine ("Time: " + currentTime.ToString ()+"\r\n");
			sr.Write ("Waypoint#:,");
			for(int i = 0; i < waypointsVisited; ++i) sr.Write("{0},", i);
			sr.WriteLine ("");
			
			for (int i = 0; i < uavs; ++i) {
				sr.Write ("UAV #{0}:, ", i);
				//sr.WriteLine("UAV #{0}: ", i);
				for (int j = 0; j < waypointsVisited; ++j) {
					sr.Write ("{0},", finalResults [i, j]);
				}
				sr.WriteLine ("");
			}
		}

		Debug.Log (string.Format ("Done writing to files."));
	}
}
