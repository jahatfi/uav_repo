using UnityEngine;
using System.Collections;

public class TankCameraController : MonoBehaviour {
	public GameObject Tank;
	private Vector3 offset;
	// Use this for initialization
	void Start () {
		offset = transform.position - Tank.transform.position;
	}
	
	void LateUpdate () {
		transform.position = Tank.transform.position + offset; 
	}
}
