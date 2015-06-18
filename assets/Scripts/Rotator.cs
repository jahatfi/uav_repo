using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {

	//Attach this script to an object when you want it to rotate in place. 
	//If this object has a child and you also want the child to rotate in place, 
	//attach the FixRotation script to the child.  

	//NOTE:If you don't want the child to move at all, get the position and 
	//rotation in Awake(), and reset them in LateUpdate()


	// Update is called once per frame
	void Update () {
		transform.Rotate (new Vector3 (15, 30, 45) * Time.deltaTime);
	}
}
