using UnityEngine;
using System.Collections;

public class NewWaypoints : MonoBehaviour {

	public Transform prefab;
	RaycastHit hit;
	public Camera mainCamera;
	Vector2 mousePosition;
	public Transform parent;

	public void PlaceNewWaypoint(){
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
				Transform waypoint = (Transform)GameObject.Instantiate(prefab, pos, Quaternion.identity);
				waypoint.parent = parent;
			}
		}
	}


}
