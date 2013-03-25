using UnityEngine;
using System.Collections;

/*==========================
==  Physics Based 3D Rope ==
==  File: Rope_Tube.js    ==
==  By: Jacob Fletcher    ==
==  Use and alter Freely  ==
============================
How To Use:
 ( BASIC )
 1. Simply add this script to the object you want a rope teathered to
 3. Assign the other end of the rope as the "Target" object in this script
 4. Play and enjoy!
 
 (About Character Joints)
 Sometimes your rope needs to be very limp and by that I mean NO SPRINGY EFFECT.
 In order to do this, you must loosen it up using the swingAxis and twist limits.
 For example, On my joints in my drawing app, I set the swingAxis to (0,0,1) sense
 the only axis I want to swing is the Z axis (facing the camera) and the other settings to around -100 or 100.
*/

// Require a Rigidbody
[RequireComponent (typeof (Rigidbody))]

class Rope_Tube : MonoBehaviour
{

	public Transform target;
	public Material material;
	public float ropeWidth = 0.5f;
	public float resolution = 0.5f;
	public float ropeDrag = 0.1f;
	public float ropeMass = 0.5f;
	public int radialSegments = 6;
	public bool startRestrained = true;
	public bool endRestrained = false;
	public bool useMeshCollision = false;

	// Private Variables (Only change if you know what your doing)
	private Vector3[] segmentPos;
	private GameObject[] joints;
	private GameObject tubeRenderer;
	private TubeRenderer line;
	private int segments = 4;
	private bool rope = false;

	//Joint Settings
	public Vector3 swingAxis = Vector3.up;
	public float lowTwistLimit = 0.0f;
	public float highTwistLimit = 0.0f;
	public float swing1Limit  = 20.0f;

	void OnDrawGizmos()
	{
		if(target)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine (transform.position, target.position);
			Gizmos.DrawWireSphere ((transform.position+target.position)/2,ropeWidth);
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere (transform.position, ropeWidth);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere (target.position, ropeWidth);
		}
		else 
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere (transform.position, ropeWidth);   
		}
	}

	void Awake()
	{
		if(target)
		{
			BuildRope();
		}
		else 
		{
			Debug.LogError("You must have a gameobject attached to target: " + this.name,this);   
		}
	}

	void LateUpdate()
	{
		if(target)
		{
			// Does rope exist? If so, update its position
			if(rope)
			{
				line.SetPoints(segmentPos, ropeWidth, Color.white);

				line.enabled = true;
				segmentPos[0] = transform.position;

				for(int s=1;s<segments;s++)
				{
					segmentPos[s] = joints[s].transform.position;
				}
			}
		}
	}



	void BuildRope()
	{
		tubeRenderer = new GameObject("TubeRenderer_" + gameObject.name);
		line = tubeRenderer.AddComponent(typeof(TubeRenderer)) as TubeRenderer;
		line.useMeshCollision = useMeshCollision;

		// Find the amount of segments based on the distance and resolution
		// Example: [resolution of 1.0 = 1 joint per unit of distance]
		segments = Mathf.RoundToInt(Vector3.Distance(transform.position,target.position)*resolution);
		if(material) 
		{
			material.SetTextureScale("_MainTex", new Vector2(1,segments+2));
			if(material.GetTexture("_BumpMap"))
				material.SetTextureScale("_BumpMap", new Vector2(1,segments+2));
		}
		line.vertices = new TubeVertex[segments];
		line.crossSegments = radialSegments;
		line.material = material;
		segmentPos = new Vector3[segments];
		joints = new GameObject[segments];
		segmentPos[0] = transform.position;
		segmentPos[segments-1] = target.position;

		// Find the distance between each segment
		int segs = segments-1;
		Vector3 seperation = ((target.position - transform.position)/segs);

		for(int s=0;s < segments;s++)
		{
			// Find the each segments position using the slope from above
			Vector3 vector = (seperation*s) + transform.position;   
			segmentPos[s] = vector;

			//Add Physics to the segments
			AddJointPhysics(s);
		}

		// Attach the joints to the target object and parent it to this object
		CharacterJoint end = target.gameObject.AddComponent(typeof(CharacterJoint)) as CharacterJoint;
		end.connectedBody = joints[joints.Length-1].transform.rigidbody;
		end.swingAxis = swingAxis;
		
		SoftJointLimit sjl;
		
		sjl = end.lowTwistLimit;
		sjl.limit = lowTwistLimit;
		end.lowTwistLimit = sjl;
		
		sjl = end.highTwistLimit;
		sjl.limit = highTwistLimit;
		end.highTwistLimit = sjl;
		
		sjl = end.swing1Limit;
		sjl.limit = swing1Limit;
		end.swing1Limit = sjl;
		
		target.parent = transform;

		if(endRestrained)
		{
			end.rigidbody.isKinematic = true;
		}

		if(startRestrained)
		{
			transform.rigidbody.isKinematic = true;
		}

		// Rope = true, The rope now exists in the scene!
		rope = true;
	}

	void AddJointPhysics(int n)
	{
		joints[n] = new GameObject("Joint_" + n);
		joints[n].transform.parent = transform;
		Rigidbody rigid = joints[n].AddComponent(typeof(Rigidbody)) as Rigidbody;
		if(!useMeshCollision)
		{
			SphereCollider col = joints[n].AddComponent(typeof(SphereCollider)) as SphereCollider;
			col.radius = ropeWidth;
		}
		CharacterJoint ph = joints[n].AddComponent(typeof(CharacterJoint)) as CharacterJoint;
		ph.swingAxis = swingAxis;
		
		SoftJointLimit sjl;
		
		sjl = ph.lowTwistLimit;
		sjl.limit = lowTwistLimit;
		ph.lowTwistLimit = sjl;
		
		sjl = ph.highTwistLimit;
		sjl.limit = highTwistLimit;
		ph.highTwistLimit = sjl;
		
		sjl = ph.swing1Limit;
		sjl.limit = swing1Limit;
		ph.swing1Limit = sjl;
		//ph.breakForce = ropeBreakForce; <--------------- TODO

		joints[n].transform.position = segmentPos[n];

		rigid.drag = ropeDrag;
		rigid.mass = ropeMass;

		if(n==0)
		{     
			ph.connectedBody = transform.rigidbody;
		} 
		else
		{
			ph.connectedBody = joints[n-1].rigidbody;   
		}
	}
}