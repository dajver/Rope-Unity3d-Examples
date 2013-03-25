//RopeSimulator.cs
using UnityEngine;

using System.Collections;

public class RopeSimulator: MonoBehaviour
{

	public Transform  endObject;
	public float  radius = 0.1f;
	public float  mass = 10.0f;
	public bool lockStart = true;
	public bool lockEnd = false;
	public float drag = 1.0f;
	private float resoloution = 1.8f;
	private float springConstant = 100.0f;
	private float springFrictionConstant = 0.5f;
	private RopeSpring[] springs;
	private GameObject[] knots;
	private int numKnots = 0;

	void Start ()
	{

		if (! endObject)
			return;

		Destroy (collider);

		if (gameObject.GetComponent<Renderer> ())
			gameObject.GetComponent<Renderer> ().enabled = false;

		Destroy (endObject.collider);

		if (endObject.gameObject.GetComponent<Renderer> ())
			endObject.gameObject.GetComponent<Renderer> ().enabled = false;

		numKnots = (int)(Vector3.Distance (transform.position, endObject.position) * resoloution);

		Debug.Log (endObject.position);

		float springLength = Vector3.Distance (transform.position, endObject.position) / (float)(numKnots - 1);

		float knotMass = mass / (float)numKnots;

        

		knots = new GameObject[numKnots];

		springs = new RopeSpring[numKnots - 1];

        

		for (int i = 0; i < numKnots; i++) {

			GameObject knot = GameObject.CreatePrimitive (PrimitiveType.Sphere);

			knot.name = "Knot-" + i + "-" + gameObject.name;

			//knot.transform.parent=transform;

			knot.transform.localScale = Vector3.one * radius * 2;

			knot.AddComponent<Rigidbody> ();

			knot.rigidbody.mass = knotMass;

			if (i == 0 && lockStart)
				knot.rigidbody.isKinematic = true;

			if (i == numKnots - 1 && lockEnd)
				knot.rigidbody.isKinematic = true;

			knot.transform.position = Vector3.Lerp (transform.position, endObject.position, (float)i / (float)(numKnots - 1));

			knots [i] = knot;

		}

		//knots[0].transform.parent=transform;

        

		for (int i = 0; i < numKnots - 1; i++) {

			springs [i] = new RopeSpring (knots [i], knots [i + 1], 

                springConstant, springLength, springFrictionConstant);

		}

	}

    

	// Update is called once per frame

	void FixedUpdate ()
	{

        

		if (Input.GetButton ("Fire1") && ! Input.GetKey (KeyCode.LeftShift))

			MoveKnot (knots [0]);

        

		if (Input.GetButton ("Fire2") && ! Input.GetKey (KeyCode.LeftShift))

			MoveKnot (knots [numKnots - 1]);

        

		if (Input.GetButtonDown ("Fire1") && Input.GetKey (KeyCode.LeftShift))

			knots [0].rigidbody.isKinematic = !knots [0].rigidbody.isKinematic;

        

		if (Input.GetButtonDown ("Fire2") && Input.GetKey (KeyCode.LeftShift))

			knots [numKnots - 1].rigidbody.isKinematic = !knots [numKnots - 1].rigidbody.isKinematic;

        

        

		if (! endObject)
			return;

		for (int i=0; i<numKnots-1; i++) {

			springs [i].solve (drag, Time.deltaTime);

		}

	}

	void MoveKnot (GameObject knot)
	{

		Ray ray;

		float dist;

		Plane plane;

		ray = Camera.main.ScreenPointToRay (Input.mousePosition);

		plane = new Plane (-Camera.main.transform.forward, knot.transform.position);

		if (plane.Raycast (ray, out dist)) {

			knot.transform.position = ray.GetPoint (dist);

		}

	}

}