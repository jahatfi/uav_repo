using UnityEngine;
using System.Collections.Generic;


public class DrawRoutes : MonoBehaviour {
	public Material material;
	private LineRenderer lineRenderer;
	private UAVController uav;
	private int lastWaypoint;
	private int count; 
	private Color myColor;
	DrawRoutes thisScript;


	// Use this for initialization
	void Start () {
		thisScript = GetComponent<DrawRoutes> ();
		lastWaypoint = 0;
		uav = (UAVController)GetComponentInParent(typeof(UAVController));
		lineRenderer = GetComponent<LineRenderer> ();
		lineRenderer.SetWidth (8f, 8f);
		lineRenderer.sharedMaterial = material;
		lineRenderer.SetPosition (0, uav.myCam.transform.position);

		SetVertices (ref uav.waypoints, 0);
	}
	
	// Update is called once per frame
	void Update () {
		if (!uav.isDone ()) {

			lineRenderer.SetPosition (0, uav.myCam.transform.position);
			//Check to see if the currentWaypoint has changed.
			//If so, we need to update the positions so we don't draw ones we've already been to.

			if (lastWaypoint != uav.GetCurrentWaypoint ()) {
				lastWaypoint = uav.GetCurrentWaypoint ();

				//Shift all the elements of the array.  
				//Note that if the route is looped we grab the next one in the waypoints array (mod waypoints.length)
				//If the route is not looped, remove the position at index [1]
				if (uav.loop) {
					for (int i = 1; i < count; ++i)
						lineRenderer.SetPosition (i, uav.waypoints [(lastWaypoint + i - 1) % uav.waypoints.Count].transform.position);
				} else {
					--count;
					lineRenderer.SetVertexCount (count);
					for (int i = 1; i < count; ++i)
						lineRenderer.SetPosition (i, uav.waypoints [(lastWaypoint + i - 1)].transform.position);
				}
				lineRenderer.SetPosition (0, uav.myCam.transform.position);
			}
		} else { //thisScript.enabled = false;
			this.enabled = false;
			LineRenderer.Destroy (lineRenderer);
		}
	}

	public void PreviewRoute(ref List<Transform> temp){
		//PROBABLY MAKE THESE TO STAND OUT
		lineRenderer.SetWidth (16f, 16f);
		//lineRenderer.sharedMaterial = material;
		SetVertices (ref temp, 0);
	}

	public void CancelRoute(){
		SetVertices (ref uav.waypoints, uav.GetCurrentWaypoint());
	}

	public void ConfirmNewRoute(int currentWaypoint){
		lineRenderer.SetWidth (8f, 8f);
		SetVertices (ref uav.waypoints, currentWaypoint);
		//uav.SetCurrentWaypoint (0);
	}

	void SetVertices(ref List<Transform> temp, int startingIndex){
		//I may need to code this differently depending on whether or not the route is looped
		if (!uav.loop) {
			count = temp.Count + 1;
			lineRenderer.SetVertexCount (count);
			for (int i = 1 + startingIndex; i <= temp.Count; ++i)
				lineRenderer.SetPosition (i, temp [i - 1].transform.position);
		} else {
			count = temp.Count;
			lineRenderer.SetVertexCount (count);
			for (int i = startingIndex; i < temp.Count; ++i)
				lineRenderer.SetPosition (i, temp [i - 1].transform.position);
		}
	}
}
