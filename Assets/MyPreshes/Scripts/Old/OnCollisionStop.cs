using UnityEngine;
using System.Collections;

public class OnCollisionStop : MonoBehaviour
{
	void  OnCollisionEnter (Collision collision)
	{
		rigidbody.isKinematic = true;
		var bob = GameObject.Find ("Shooter");
		bob.transform.position = Vector3.Lerp (bob.transform.position, this.transform.position + new Vector3 (0, 1, 0), 1);
		Destroy (gameObject);

	}
}
