using UnityEngine;
using System.Collections;

public class Make : MonoBehaviour
{
	public GameObject targetStartObject;
	public GameObject targetEndObject;
	public GameObject cloneObject;
	public  int nCount = 5;
	public float fWidth = 0.3f;
	public float fWeight = 0.4f;
	public float fDrag = 1.0f;
	public float fAngDrag = 0.5f;
	public float temp1 = 0.7f;
	public float temp2 = 10.0f;
	private GameObject prevObj;

	void Start ()
	{
		int i;
		
		for (i = 0; i< nCount; i++) {
			GameObject temp;
			temp = (GameObject)Instantiate (cloneObject);
			temp.name = "JointObj" + i;
			Vector3 vecDir = targetEndObject.transform.position - targetStartObject.transform.position;
			//temp.transform.position = Vector3.Lerp(targetStartObject.transform.position, targetEndObject.transform.position, (float)i / nCount);
			temp.transform.position = targetStartObject.transform.position + vecDir.normalized * fWidth * i * 1.2f;
			temp.transform.eulerAngles = new Vector3 (0, 0, 0);
			temp.transform.localScale = new Vector3 (fWidth, fWidth, fWidth);
			temp.transform.parent = targetStartObject.transform;
			if (i == 0) {
				temp.rigidbody.isKinematic = true;
				temp.rigidbody.mass = fWeight;
			} else {
				temp.rigidbody.mass = prevObj.rigidbody.mass * temp1;
				temp.rigidbody.drag = fDrag;
				temp.rigidbody.angularDrag = fAngDrag;
				temp.hingeJoint.connectedBody = prevObj.rigidbody;
//				temp.rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX;
			}
			prevObj = temp;
		}
		//targetEndObject.renderer.enabled = false;
	}
	
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.A)) {
			prevObj.rigidbody.AddForce (Random.insideUnitSphere.normalized * temp2);
		}
	}
}
