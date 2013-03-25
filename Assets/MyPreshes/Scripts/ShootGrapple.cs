using UnityEngine;
using System.Collections;

public class ShootGrapple : MonoBehaviour
{
	public Transform prefabGrapple;
	
	void Update ()
	{
		if (Input.GetButtonDown ("Fire1")) {
			Transform InstanceGrapple = Instantiate (prefabGrapple, transform.position, Quaternion.identity) as Transform;
			InstanceGrapple.rigidbody.AddForce (0,Input.mousePosition.y,Input.mousePosition.y);
		}
	}
}
