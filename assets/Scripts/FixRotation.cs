using UnityEngine;
using System.Collections;

public class FixRotation : MonoBehaviour {
	//Attach this script to a child object when you want both the child
	//and parent to rotate in place.  Attach the Rotator script to the parent.

	/*I have since added additional functionality to this script, but the only
	 * parts necessary for the rotation aspect are 
	 * Vector3 position;
	 * in Awake(): position = transform.position; 
	 * in LateUpdate(): transform.position = position;
	 */
	Vector3 position;
	private bool[] visibilityFlags = new bool[16];  //One for each UAV, this object will be visible if at 
	//least one of the flags is set to "true"
	
	void Awake()
	{
		visibilityFlags = new bool[16];
		//Set the object invisible from the start
		renderer.enabled = false;
		position = transform.position;
		for (int i = 0; i < visibilityFlags.Length; ++i)
			visibilityFlags [i] = false;
	}
	
	void LateUpdate()
	{
		transform.position = position;
		for (int i = 0; i < visibilityFlags.Length; ++i){
			if (visibilityFlags [i] == true) {
				renderer.enabled = true;
				return;
			}
		}
		renderer.enabled = false;
	}

	public void setFlag(int index, bool val){
		visibilityFlags [index-1] = val;
	}
}
