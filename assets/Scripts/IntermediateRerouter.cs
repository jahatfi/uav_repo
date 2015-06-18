using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//One of these for each of the four re-routing panels
//This script does this actual re-routing
public class IntermediateRerouter : MonoBehaviour {
	public int slaveID;
	private Text uavLabel;
	private UAVController uavScript;  //Need this to update the waypoints
	private ReroutingManager master;

	public void Awake(){
		master = ReroutingManager.instance;
		uavLabel = gameObject.GetComponentInChildren<Text> ();
	}
	public void EnablePanel(int id){
		uavScript = GameObject.Find(string.Format("UAV"+id.ToString())).GetComponent<UAVController>();
		//uavLabel.text = "Rerouting UAV #" + id.ToString();
		if(uavScript == null) Debug.Log(string.Format(("Script ref is NULL!")));
		//Debug.Log("In slave, ID is: "+uavScript.getUAVNumber ().ToString());
		//Debug.Log(string.Format ("In Reroute() of slave #{0}", slaveID));
	}
	public void Confirm(int routeNumber){
		uavScript.ConfirmNewRoute (routeNumber);
		master.SlaveFinished (slaveID, uavScript.getID());
	}
	public void Cancel(){
		uavScript.GetRouteDrawer().CancelRoute ();
	}
	public void Preview(int routeNumber){
		//uavScript.myRouteDrawer.PreviewRoute
		List<Transform> temp = uavScript.GetRoute (routeNumber);
		uavScript.GetRouteDrawer ().PreviewRoute (ref temp);
	}
	//Allows the user to click the panel to make it the "active window"
	public void Selected(){
		master.SetCurrentWorker (slaveID);
	}

	public void Exit(){
		Cancel ();
		master.SlaveFinished (slaveID, uavScript.getID());
	}
	
}
