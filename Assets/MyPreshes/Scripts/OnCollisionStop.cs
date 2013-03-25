using UnityEngine;
using System.Collections;

public class OnCollisionStop3 : MonoBehaviour
{
	void OnCollisionEnter (Collision collision)
	{
		rigidbody.isKinematic = true;
		var bob = GameObject.Find ("Shooter");
		bob.rigidbody.AddForce (0, 250, 250);
		StartCoroutine(DestroyObj());
	}
	
	IEnumerator DestroyObj ()
	{
		yield return new WaitForSeconds (1.5f);
		Destroy (gameObject);
	}
	
	void Update ()
	{
		var bob = GameObject.Find ("Shooter");
		if (this.transform.position.y <= bob.transform.position.y) {
			Destroy (gameObject);
		}
		if (this.transform.position.y > bob.transform.position.y + 10) {
			Destroy (gameObject);
		}
	}
}
