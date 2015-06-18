using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//One of these for each of the four re-routing panels
//This script does this actual re-routing
public class ReroutingSlave : MonoBehaviour {
	public int slaveID;
	public Text uavLabel;
	public Text listOfWaypoints;
	public Button previewButton;
	public Button confirmButton;
	public Button cancelButton;
	private UAVController uavScript;  //Need this to update the waypoints
	private List<Transform> newWaypoints;
	private ReroutingManager master;

	public void Awake(){
		master = ReroutingManager.instance;
		newWaypoints = new List<Transform> ();
	}

	public void Reroute(int id){
		uavScript = GameObject.Find(string.Format("UAV"+id.ToString())).GetComponent<UAVController>();
		uavLabel.text = "Rerouting UAV #" + id.ToString();
		Debug.Log("In slave, ID is: "+uavScript.getUAVNumber ().ToString());
		Debug.Log(string.Format ("In Reroute() of slave #{0}", slaveID));
	}

	public void Confirm(){
		if (newWaypoints.Count > 0) {
			uavScript.ConfirmNewRoute (ref newWaypoints);
			//uavScript.waypoints = newWaypoints;
			Debug.Log (string.Format ("In confirm() of slave #{0}", slaveID));
			master.SlaveFinished (slaveID);
		}
	}

	public void Cancel(){
		//Check to see if the preview option was selected.
		foreach (Transform w in newWaypoints)
			Destroy(w.gameObject, 0F);
		newWaypoints.Clear ();
		uavScript.myRouteDrawer.CancelRoute ();
		Debug.Log(string.Format("In Cancel() of slave #{0}", slaveID));
	}

	public void Preview(){
		if (newWaypoints == null) 	Debug.Log (string.Format ("array is null"));
		uavScript.myRouteDrawer.PreviewRoute (ref newWaypoints);
		//uavScript.waypoints = newWaypoints;
		Debug.Log(string.Format ("In Preview() of slave #{0}", slaveID));
	}


	//Allows the user to click the panel to make it the "active window"
	public void Selected(){
		master.setCurrentWorker (slaveID);
	}

	public void addNewWaypoint(Transform t){
		newWaypoints.Add (t);
	}
}
