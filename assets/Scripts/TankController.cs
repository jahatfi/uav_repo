using UnityEngine;
using System.Collections;

public class TankController : MonoBehaviour {

	public Transform[] waypoints;        // List of Waypoints.  Populate via inspector
	public float dampingLook = .05f;          // How slowly to turn
	public float patrolSpeed = 3f;       // The walking speed between Waypoints
	public int currentWaypoint = 0;
	public bool loop;
	private float rotateTime;
	public float rotationTimer;
	public Transform[] Wheels;
	public Transform[] Sprockets;

	//private int wheelRadius = 15;
	//private int sprocketRadius = 22;

	// Use this for initialization
	void Start () {
		rotateTime = -rotationTimer;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		patrol();
		if(currentWaypoint < waypoints.GetLength(0)){
			
			SmoothLookAt();
		}
		else{    
			if(loop) currentWaypoint = 0;
			else Debug.Log(string.Format("All Done!"));
		}
	}

	void  patrol (){
		if (currentWaypoint < waypoints.GetLength (0)) {
			Vector3 target = waypoints [currentWaypoint].position;		//Vector3 target = waypoint[currentWaypoint].position;
			target.y = transform.position.y; // Keep waypoint at character's height
			Vector3 moveDirection = target - transform.position;

			if (moveDirection.magnitude < 2){
				++currentWaypoint;
				rotateTime = Time.time;
			}
			else if(Time.time - rotateTime > rotationTimer){        
				transform.position += (moveDirection.normalized * patrolSpeed * Time.deltaTime);
				/*
				float distanceTraveled = patrolSpeed * Time.deltaTime;
				float rotationInRadians = distanceTraveled / wheelRadius;
				float rotationInDegrees = rotationInRadians * Mathf.Rad2Deg;
				//wheel.transform.Rotate(rotationInDegrees, 0, 0);
				Vector3 axis = new Vector3(1,0,0);
				// Or whatever axis is appropriate.

				foreach(Transform wheel in Wheels){
					//wheel.transform.Rotate(Vector3.right, Space.Self); 
					//wheel.RotateAround(wheel.position, new Vector3(1,0,0),rotationInDegrees);
				}
				rotationInRadians = distanceTraveled / sprocketRadius;
				rotationInDegrees = rotationInRadians * Mathf.Rad2Deg;
				foreach(Transform sprocket in Sprockets) sprocket.transform.Rotate(axis, rotationInDegrees*10,  Space.Self);
				*/
			}
		}
	}

	void SmoothLookAt ()
	{
		// Create a vector from the camera towards the player.
		//Debug.Log (currentWaypoint);
		Vector3 relPlayerPosition =  waypoints[currentWaypoint].position - transform.position ;
		//Debug.Log(relPlayerPosition);
		
		// Create a rotation based on the relative position of the waypoint being the forward vector.
		Quaternion lookAtRotation = Quaternion.LookRotation(relPlayerPosition, Vector3.up);
		
		// Lerp the camera's rotation between it's current rotation and the rotation that looks at the player.
		transform.rotation = Quaternion.Lerp(transform.rotation, lookAtRotation, dampingLook * Time.deltaTime);
	}
}
