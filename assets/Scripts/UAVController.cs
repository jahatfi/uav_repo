using UnityEngine;
using System;
using System.Collections.Generic;
using TactorTest;
using Tdk;
using UnityEngine.UI;

public class UAVController : MonoBehaviour {

	public DrawRoutes myRouteDrawer;  //Assign in inspector

	//Variables for use in the status panel
	public Button firstButton;
	public Button secondButton;
	public Button thirdButton;
	public Slider fuel;
	public Image fuelFill;
	private Color MinColor = Color.red;
	private Color MaxColor = Color.green;
	public bool fuelLeak = false;

	private List<double> reactionTimes;
	private List<int> userInput; //1,2, or 3, depending on which button is selected.  0 if no selection is made.
	private int buttonIndex = -1;  //Keeps track of which waypoint the user's input corresponds to.  
	public float buttonDuration;  //Determine how long of a window the user has to click on of the buttons.  Assign in the inspector
	private float buttonTimer;
	//This is nearly the same as the currentWaypoint, but this seperate variable allows the user to click the 
	//button a few seconds AFTER the UAV has left the previous waypoint.  It ensures that the input is accurate. :)
	//It starts at -1 and will be incremented each time the UAV approached the next waypoint

	public List<Transform> waypoints;        // List of Waypoints.  Populate via inspector
	public List<bool> showObjectsOfInterest; //Indicates which objects will become visible, populate via inspector
	//so that each waypoint has an associated Object of Interest.
	public float patrolSpeed = 3f;       // The walking speed between Waypoints
	private float originalpatrolSpeed;
	public bool  loop = true;       // Do you want to keep repeating the Waypoints?
	//For dampingLook, I recommend using a value on the order of 0.1 - 1.0
	public float dampingLook = .05f;          // How slowly to turn
	public float pauseDuration = 2;   // How long to pause at a Waypoint
	public float distanceToPulse;

	private float rotateTime;
	public float rotationTimer;
	private float curTime;
	private int currentWaypoint = 0;
	public Camera myCam;
	private bool done;

	/* Classes containing tactors methods, note that we to make them static 
	 * 	so each UAV can access them, and we need 2, one for each tactor board.
	 */
	static TestClass[] tactor_classes = {new TestClass(), new TestClass()};
	public int tactorBoardNumber;
	public string portID;
	static int[] connectingBoardIDs = {0,0};
	private int myConnectingID;
	public int connectingType;
	public int gain;
	public int frequency;
	public int delay;
	public int duration;
	public int tactor_id;
	private bool pulsed;
	private int id;


	// Use this for initialization
	void Start(){
		reactionTimes = new List<double> ();
		for (int i = 0; i < waypoints.Count; ++i) 
			reactionTimes.Add (0f);
		userInput = new List<int> ();
		for (int i = 0; i < waypoints.Count; ++i)
			userInput.Add (0);

		//firstButton.image.color = secondButton.image.color = thirdButton.image.color = Color.grey;
		firstButton.interactable = secondButton.interactable = thirdButton.interactable = false;
		//firstButton.onClick

		originalpatrolSpeed = patrolSpeed;
		rotateTime = -rotationTimer;
		pulsed = false;

		//Ensure each UAV has the correct connecting ID
		id = getUAVNumber ();
		if( id < 9) myConnectingID = connectingBoardIDs[0];
		else myConnectingID = connectingBoardIDs[1];
		Debug.Log (string.Format ("List sizes:\nReaction Times: {0} \nUserInput: {1}, \nshowObjectsOfInterest: {2}", reactionTimes.Count, userInput.Count, showObjectsOfInterest.Count));
	}

	void Awake() {
		if (showObjectsOfInterest.Count != waypoints.Count)
			Debug.Log (string.Format ("showObjectsOfInterest.Count != waypoints.Count"));
			
		/* Tactor class - the first UAV for each board will instantiate the static tactor classes.
		 * Only the very first UAV will call initializeTI(), as it must only be called once.
		 */
		if(tactor_id == 1){
			if(tactorBoardNumber == 1){
				//tactor_classes[0] = new TestClass();

				if (Tdk.TdkInterface.InitializeTI() == 0) print (string.Format("Successfully initialized!"));
				else{
					print (string.Format("Could not be initialized!"));
					print (string.Format(Tdk.TdkDefines.GetLastEAIErrorString()));
				}
				connectingBoardIDs[0] = Tdk.TdkInterface.Connect(portID,
		                                             connectingType,
		                                             //This line didn't work:(int)Tdk.TdkDefines.DeviceTypes.WinUsb,
		                                             IntPtr.Zero);
				checkForConnectingErrors(connectingBoardIDs[0],tactorBoardNumber);
			}
			//print (string.Format("TBN is: {0}",tactorBoardNumber));
			else if(tactorBoardNumber == 2){
				//tactor_classes[1] = new TestClass();
				connectingBoardIDs[1] = Tdk.TdkInterface.Connect(portID,
				                                             connectingType,
				                                             //This line didn't work:(int)Tdk.TdkDefines.DeviceTypes.WinUsb,
				                                             IntPtr.Zero);
				//checkForConnectingErrors(connectingBoardIDs[1],tactorBoardNumber);
			}


			//print (String.Format("Array of connecting IDs is now: {0},{1}",connectingBoardIDs[0], connectingBoardIDs[1]));
		}
	}
	/*
	void Update(){
		if (!done) {
			if (fuel.value == 0) {
				myCam.gameObject.rigidbody.useGravity = true;

				Vector3 moveDirection = getDirectionToTarget ();
				//myCam.gameObject.rigidbody.velocity =(moveDirection * .1F);
				myCam.gameObject.rigidbody.AddForce (moveDirection * 100F);
				done = true;
				//Debug.Log (String.Format ("Out of fuel, falling."));
			}
		}
	}
*/

	// Update is called once per frame
	void LateUpdate () {
		if (!done) {
			//Check if the buttons are interactable. (if one is, they all are.)  If so, check the timer to see if they need to be disabled.
			if(firstButton.interactable){
				buttonTimer += Time.deltaTime;
				if(buttonTimer > buttonDuration){
					firstButton.interactable = secondButton.interactable = thirdButton.interactable = false;
				}
			}
			/*
			if(fuel.value == 0){
				myCam.gameObject.rigidbody.useGravity = true;
				Vector3 moveDirection = getDirectionToTarget();
				myCam.gameObject.rigidbody.AddForce(moveDirection * .5F);
				done = true;
				Debug.Log(String.Format("Out of fuel, falling."));
			}
*/
			//Route not completed, continue patroling
			if (currentWaypoint < waypoints.Count) {
				//SmoothLookAt() must be called prior to patrol() b/c patrol changes the value of currentWaypoint
				SmoothLookAt ();
				patrol ();

			} else {    
				if (loop)
					currentWaypoint = 0;
				else { 
					done = true;
					Debug.Log (string.Format ("All Done!"));
					ProcessResults results = ProcessResults.instance;
					results.ReceiveResults (id, ref showObjectsOfInterest, ref userInput, ref reactionTimes);
				}
			}
		}

		UpdateFuel ();
	}

	private void UpdateFuel(){
		float myFuel;
		if(fuelLeak) myFuel = fuel.value - 4*Time.deltaTime;
		else myFuel = fuel.value - 2*Time.deltaTime;
		fuel.value = myFuel;
		if (myFuel < 33)
			fuelFill.color = Color.red;
		else if (myFuel < 67)
			fuelFill.color = Color.yellow;
	}
	
	void  patrol (){

		Vector3 moveDirection = getDirectionToTarget();
		if (moveDirection.magnitude < distanceToPulse) {
			//Debug.Log(String.Format("Approaching waypoint: {0}", currentWaypoint));
			if (!pulsed) {

				//enable buttons and increment buttonIndex
				firstButton.interactable = secondButton.interactable = thirdButton.interactable = true;
				++buttonIndex;
				buttonTimer = 0f;

				//Pulse appropriate tactor when nearing waypoint, slow down to observe area
				tactor_classes [tactorBoardNumber - 1].PulseTactor (ref myConnectingID, ref tactor_id, ref duration, ref delay, ref frequency, ref gain);
				pulsed = true;
				//patrolSpeed = 20;

				//Determine whether or not to make the Object of Interest visible

				if (showObjectsOfInterest [currentWaypoint]) {
					GameObject temp = waypoints [currentWaypoint].GetChild (0).gameObject;
					FixRotation other = (FixRotation)temp.GetComponent (typeof(FixRotation));
					other.setFlag (id, true);
				}

			}

			//version 3.0, don't pause over waypoint, simply fly over it.
			if (moveDirection.magnitude < 20) {
				//We only need to disable the object if it was visible in the first place
				if (showObjectsOfInterest [currentWaypoint]) {
					GameObject temp = waypoints [currentWaypoint].GetChild (0).gameObject;
					FixRotation other = (FixRotation)temp.GetComponent (typeof(FixRotation));
					other.setFlag (id, false);
					currentWaypoint++;
					//Debug.Log(String.Format("Updating waypoint, it is now: {0}", currentWaypoint));
					pulsed = false;
				}
			}
		}
				//end version 3.0

				/*
			//Version 2.0, pause over: pause over the Waypoint
			if (curTime == 0) curTime = Time.time; 

			//Obsesrvation time complete: disable the object (as needed), increase speed, set next waypoint as new target
			else if ((Time.time - curTime) >= pauseDuration) {

				//We only need to disable the object if it was visible in the first place
				if(showObjectsOfInterest[currentWaypoint]){
					GameObject temp = waypoints[currentWaypoint].GetChild(0).gameObject;
					FixRotation other = (FixRotation) temp.GetComponent(typeof(FixRotation));
					other.setFlag(id, false);
				}

				currentWaypoint++;
				patrolSpeed = originalpatrolSpeed;
				curTime = 0;
				rotateTime = Time.time;
				//Debug.Log (string.Format ("Checkpoint!"));
				//firstButton.interactable = secondButton.interactable = thirdButton.interactable = false;


			}
			// End versinon 2.0

		} 
		else if(Time.time - rotateTime > rotationTimer){   
*/

		//Quaternion rotation = Quaternion.LookRotation(moveDirection);
		//transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * dampingLook);
		transform.position += (moveDirection.normalized * patrolSpeed * Time.deltaTime);
		//myCam.transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * dampingLook);
		myCam.transform.position += (moveDirection.normalized * patrolSpeed * Time.deltaTime);
		//pulsed = false;
		//disable buttons again
		//if(id ==1) ClearButton[id-1].enabled = false;

	}

	private Vector3 getDirectionToTarget(){
		Vector3 target = waypoints [currentWaypoint].position;		//Vector3 target = waypoint[currentWaypoint].position;
		target.y = transform.position.y; // Keep waypoint at character's height
		Vector3 moveDirection = target - transform.position;
		return moveDirection;
	}



	void SmoothLookAt ()
	{

		// Create a vector from the camera towards the player.
		//Debug.Log (currentWaypoint);
		Vector3 relPlayerPosition =  waypoints[currentWaypoint].GetChild(0).position - myCam.transform.position ;
		//Debug.Log(relPlayerPosition);

		// Create a rotation based on the relative position of the waypoint being the forward vector.
		Quaternion lookAtRotation = Quaternion.LookRotation(relPlayerPosition, Vector3.up);

		// Lerp the camera's rotation between it's current rotation and the rotation that looks at the player.
		transform.rotation = Quaternion.Lerp(transform.rotation, lookAtRotation, dampingLook * Time.deltaTime);
		//transform.Rotate (new Vector3 (2f, 0f, 0f), Space.Self);
	}

	void OnDestroy(){
		//print (string.Format ("I am become Death, Destroyer of Worlds."));
		if (tactorBoardNumber == 1 && tactor_id == 1) {
			Tdk.TdkInterface.ShutdownTI (); 
		}
	}

	public int getUAVNumber(){
		return tactor_id + ((tactorBoardNumber-1) * 8);
	}

	public bool isDone(){
		return done;
	}


	//If the UAV enters the NoFlyZone, the enemy will shoot it down.  Pulse tactor and disable camera.
	void OnTriggerEnter(Collider other){
		//Destroy (other.gameObject);
		if (other.gameObject.tag == "NoFlyZone") {
			Debug.Log(string.Format("Entering NFZ!"));
			tactor_classes[tactorBoardNumber-1].PulseTactor (ref myConnectingID, ref tactor_id, ref duration, ref delay, ref frequency, ref gain);
			myCam.gameObject.SetActive(false);
			//myCam.gameObject.rigidbody.useGravity = true;
		}
	}

	void checkForConnectingErrors(int boardID, int boardNumber){
		if (boardID > -1) {
			print (string.Format ("Connected to board {0}", boardNumber));
			tactor_classes[boardNumber-1].PulseTactor (ref boardID, ref tactor_id, ref duration, ref delay, ref frequency, ref gain);
			//print (String.Format("{0},{1},{2},{3},{4},{5},{6}",tactorBoardNumber,connectingBoardIDs[0], tactor_id, duration, delay, frequency, gain));
		}
		else{
			print (string.Format("Could not connect to board {0}",boardNumber));
			print (string.Format(Tdk.TdkDefines.GetLastEAIErrorString()));
		}
	}


	public int GetCurrentWaypoint(){
		return currentWaypoint;
	}


	public void SetCurrentWaypoint(int newCurrentWaypoint){
		currentWaypoint = newCurrentWaypoint;
	}


	public void ButtonClicked(int response){
		//Immediately update the timer to increase accuracy of results 
		buttonTimer += Time.deltaTime;

		//Disable buttons
		firstButton.interactable = secondButton.interactable = thirdButton.interactable = false;

		//record results
		userInput [buttonIndex] = response;
		reactionTimes[buttonIndex] = buttonTimer;
		if (id == 1)
			Debug.Log (String.Format ("Reaction time for waypoint #{0} is {1}", buttonIndex + 1, reactionTimes[buttonIndex]));

	}

	public void ConfirmNewRoute(ref List<Transform> newWaypoints){
		//Find the difference in the # of waypoints from originally planned
		int listSizeDiff = currentWaypoint + newWaypoints.Count - waypoints.Count;
		//Need to expand the lists
		if (listSizeDiff > 0) {
			for (int i = 0; i < listSizeDiff; ++i) {
				reactionTimes.Add (0);
				userInput.Add (0);
				showObjectsOfInterest.Add (false);
			}
		}
		//Need to shrink the lists
		else if(listSizeDiff < 0){
			listSizeDiff = Mathf.Abs(listSizeDiff);
			int index = waypoints.Count - listSizeDiff;
			reactionTimes.RemoveRange(index, listSizeDiff);
			userInput.RemoveRange(index, listSizeDiff);
			showObjectsOfInterest.RemoveRange(index, listSizeDiff);
		}

		//Update list of waypoints to vist, being sure to not delete the ones already visited.
		//Remove unvisited waypoints
		waypoints.RemoveRange (currentWaypoint, waypoints.Count - currentWaypoint);
		//Append list of new waypoints
		waypoints.AddRange (newWaypoints);
		myRouteDrawer.ConfirmNewRoute (currentWaypoint);

		for (int i = currentWaypoint; i < waypoints.Count; ++i) {
			if (waypoints [i].gameObject.tag == "Waypoint")
				showObjectsOfInterest [i] = true;
		}

		Debug.Log (string.Format ("List sizes:\nReaction Times: {0} \nUserInput: {1}, \nshowObjectsOfInterest: {2}", reactionTimes.Count, userInput.Count, showObjectsOfInterest.Count));
	}
}

	//Draw the route of the UAV.  I have since moved this funcationality to it's own script
	/*
	void drawRoute(){
		int tempWaypoint = currentWaypoint;
		Debug.DrawLine (myCam.transform.position, waypoints [tempWaypoint].transform.position, getColor(), 0, false);

		if (!loop) {
			for (int i = tempWaypoint; i < waypoints.Count-1; i++) {
				Debug.DrawLine (waypoints [i].transform.position, waypoints [i + 1].transform.position, getColor (), 0, false);
			}
		}

		else{
			for (int i = tempWaypoint; i < waypoints.Count-2+tempWaypoint; i++) {
				Debug.DrawLine (waypoints [i%waypoints.Count].transform.position, waypoints [(i + 1)%waypoints.Count].transform.position, getColor (), 0, false);
			}
		}
	}
	*/


