using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectOfInterestController : MonoBehaviour {

	public float durationVisible;
	public List<float> timesVisible = new List<float> ();
	private int count;
	private bool done;

	// Use this for initialization
	void Start () {
		count = 0;
		renderer.enabled = false;
		done = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (!done) {
			if (Time.time > timesVisible [count]) {
				renderer.enabled = true;
				transform.Rotate (new Vector3 (15, 30, 45) * Time.deltaTime);
				this.enabled = true;
//				Debug.Log (string.Format ("Enabling OOI!"));
				if (Time.time > (timesVisible [count] + durationVisible)) {
					renderer.enabled = false;
					count++;
					if(count == timesVisible.Count) done = true;
					//Debug.Log (string.Format ("Disabling OOI! Count is now:{0}", count));
				}
			}
		}
			
	}
}
