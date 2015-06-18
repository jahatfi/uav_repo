using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


//There should just be one of this class.  It determines which slave to activate
public class ReroutingManager : MonoBehaviour {
	//These variables are necessary for dropping new waypoints
	public Transform newWaypointPrefab;  //Assign in inspector.  I use this to differentiate user-defined waypoints from predefined ones.
	public Transform objectPrefab; //Assign in inspector.  (yellow cubes originally.)
	RaycastHit hit;
	public Camera mainCamera; //Assign in inspector.  It's the top-down, orthographic cam.
	Vector2 mousePosition;
	public Transform parent;  //Serves as a parent for all new waypoints. Assign in inspector.  This keeps the hiearchy clean.


	//private bool[] availablePanels = new bool[4];  //false (by default) means panel[i] is free
	public GameObject[] myPanels;   //assign in inspector
	public ReroutingSlave[] mySlaves; //assign in inspector
	private int currentWorker;
	private Queue<int> pendingPlans;

	//Singleton stuff
	private static ReroutingManager _instance;
	public static ReroutingManager instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = GameObject.FindObjectOfType<ReroutingManager>();
				
				//Tell unity not to destroy this object when loading a new scene!
				DontDestroyOnLoad(_instance.gameObject);
			}
			
			return _instance;
		}
	}

	// Use this for initialization
	void Awake () {
		currentWorker = -1;
		pendingPlans = new Queue<int> ();
		for (int i = 0; i < 4; ++i){
			myPanels [i].SetActive (false);
			//Debug.Log(availablePanels[i].ToString());
		}
	}
/*
	void Update(){
		if (Time.time > 5f)
			myPanels [0].SetActive (true);
	}
*/
	//Invoked when user clicks the status panel of a UAV requiring rerouting.
	//Enables one of the four panels if one is available, otherwise, queues the request
	public void RerouteUAV(int id){
		Debug.Log (string.Format ("In rerouting manager: reroute function."));
		for(int i = 0; i < 4; ++i){
			if(!myPanels[i].activeSelf){
				//This means we've found a panel not active, so we can use this one
				myPanels[i].SetActive(true);
				mySlaves[i].Reroute(id);
				currentWorker = i;
				return;
			}
		}
		//All panels are currently in use with other replanning tasks. Enqueue this plan.
		pendingPlans.Enqueue (id);
		return;
	}

	//Slaves will invoke this method to return the use of the panel to the manager
	//The manager checks to see if there are any pending plans that require a free panel,
	//and assigns one if needed
	public void SlaveFinished(int slave){
		//Assign one of the pending plans to the newly available panel.
		if (pendingPlans.Count > 0) {
			mySlaves[slave].Reroute (pendingPlans.Dequeue ());
		} else
			myPanels [slave].SetActive (false);
		Debug.Log(string.Format("Worker bee #{0} finished!", slave));
		currentWorker = -1;
	}

	public void OnMapClick(){
		if(currentWorker != -1){
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			mousePosition = Input.mousePosition;
			//Vector3 = mainCamera.ScreenToWorldPoint(
			//Debug.Log (mousePosition);
			if(Physics.Raycast(ray, out hit, 2000)){
				if(hit.collider.tag == "Ground"){
					Debug.Log (string.Format("Hit the ground at a distance of " + hit.distance));
					Vector3 pos = mainCamera.ScreenToWorldPoint(mousePosition);
					Debug.Log(pos);
					pos.y = 130;
					Transform waypoint = (Transform)GameObject.Instantiate(newWaypointPrefab, pos, Quaternion.identity);
					waypoint.gameObject.layer = 10;
					Transform target = (Transform)GameObject.Instantiate(objectPrefab, new Vector3(pos.x, pos.y - 5, pos.z), Quaternion.identity);
					target.parent = waypoint;
					waypoint.parent = parent;
					mySlaves[currentWorker].addNewWaypoint(waypoint);
				}
				else if(hit.collider.tag == "Waypoint"){
					mySlaves[currentWorker].addNewWaypoint(hit.transform);
				}
			}
		}
	}

	//Allows the user to click the panel to make it the "active window"
	public void setCurrentWorker(int w){
		if (w != currentWorker) {
			mySlaves [currentWorker].Cancel ();
			currentWorker = w;
			Debug.Log (string.Format ("Current worker is now: {0}", currentWorker));
		}
	}
}





























