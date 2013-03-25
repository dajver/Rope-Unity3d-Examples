using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum RopeType
{
	Null,
	Line,
	Prefab,
	Mesh
}

public enum BuildAxis
{
	PosX,
	NegX,
	PosY,
	NegY,
	PosZ,
	NegZ
}

public enum LongAxis
{
	X,
	Y,
	Z
}

public enum ConstraintPlane
{
	None,
	X_Y,
	Y_Z,
	Z_X
}

public enum JointClamping
{
	None,
	Limited,
	Complete
}

public class Rope : MonoBehaviour 
{	
	public RopeType type = RopeType.Prefab;
	public bool generatePreview = true;
	public GameObject ropeEnd = null;
	public bool FreezeBeg = true;
	public bool FreezeEnd = true;
	public JointClamping clampType = JointClamping.None;
	public float clampError = 0.5f;
	public ConstraintPlane constraintPlane = ConstraintPlane.None;
	public int linkCount = 10;
	public float ropeRadius = 0.25f;
	public bool includeCapsuleColliders = true;
	public bool includeSphereColliders = false;
	public int MinMaxTwist = 30;
	public float jointMass = 0.1f;
	public float jointDrag = 0.1f;
	public bool jointsHaveGravity = true;
	public PhysicMaterial ropePhysicMaterial = null;
	public int meshDetail = 4;
	public GameObject prefab = null;
	public Material lineMaterial = null;
	public int lineMaterialRepeat = 10;
	public float altRotation = 90;
	public float scale = 1.0f;
	
	//private BezierPath bezierPath = new BezierPath();
	private float jointGap = 0;
	private float endDistance = 0;
	private Vector3 jointHeading = Vector3.zero;
	private GameObject jointParent = null;
	private List<GameObject> jointConnections = new List<GameObject>();
	
	// Mesh Properties
	private List<Vector3> verts = new List<Vector3>();
	private Vector2[] uvs;
	private int[] tris;
	private Mesh mesh;
	
	// Line Properties
	private LineRenderer line = null;
	
	void OnDrawGizmos()
	{
		Gizmos.color = new Color(0.3f,0.7f,0.5f,0.5f);
		try{
			Gizmos.DrawLine(transform.position, ropeEnd.transform.position);
			
			foreach(GameObject jc in jointConnections)
				Gizmos.DrawWireSphere(jc.transform.position, ropeRadius);
		
			for(int i = 1; i < jointConnections.Count; i++)
				Gizmos.DrawLine(jointConnections[i-1].transform.position, jointConnections[i].transform.position);
		}catch{ }
	}
	
	void OnDrawGizmosSelected()
	{
		if(generatePreview && !Application.isPlaying && ropeEnd != null)
			MakeDemo();
		
		foreach(Vector3 vert in verts)
		{
			Gizmos.DrawWireSphere(vert, 0.05f);
		}
	}
	
	void MakeDemo()
	{
		DestroyDemoRope();
		jointGap = Vector3.Distance(transform.position, ropeEnd.transform.position)/linkCount;
		jointHeading = (transform.position - ropeEnd.transform.position).normalized;
		
		PlaceJointConnections();
		
		switch(type)
		{
			case RopeType.Line:
				BuildLine();
				break;
			case RopeType.Prefab:
				PlacePrefabs();
				break;
			case RopeType.Mesh:
				BuildMesh();
				break;
			default:
				break;
		}
	}
	
	GameObject demo;
	void DestroyDemoRope()
	{
		demo = GameObject.Find("TempRope");
		
		if(demo != null)
		{
			for(int i = 0; i < demo.transform.childCount; i++)
			{
				DestroyImmediate(demo.transform.GetChild(i).gameObject);
			}
			
			DestroyImmediate(demo);
		}
		jointConnections.Clear();
	}
	
	void Start()
	{
		DestroyDemoRope();
		
		endDistance = Vector3.Distance(transform.position, ropeEnd.transform.position);
		jointGap = endDistance/linkCount;
		jointHeading = (transform.position - ropeEnd.transform.position).normalized;
		
		try{ gameObject.AddComponent<Rigidbody>(); gameObject.rigidbody.isKinematic = FreezeBeg; } catch { }
		try{ ropeEnd.AddComponent<Rigidbody>(); ropeEnd.rigidbody.isKinematic = FreezeEnd; } catch { }
		
		switch(constraintPlane)
		{
			case ConstraintPlane.X_Y:
				ropeEnd.rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;
				rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;	
				break;
			case ConstraintPlane.Y_Z:
				ropeEnd.rigidbody.constraints = RigidbodyConstraints.FreezePositionX;
				rigidbody.constraints = RigidbodyConstraints.FreezePositionX;
				break;
			case ConstraintPlane.Z_X:
				ropeEnd.rigidbody.constraints = RigidbodyConstraints.FreezePositionY;
				rigidbody.constraints = RigidbodyConstraints.FreezePositionY;	
				break;
			default:
				break;
		}
		
		PlaceJointConnections();
		
		switch(type)
		{
			case RopeType.Line:
				BuildLine();
				break;
			case RopeType.Prefab:
				PlacePrefabs();
				break;
			case RopeType.Mesh:
				BuildMesh();
				break;
			default:
				break;
		}
	}
	
	void FixedUpdate()
	{
		switch(clampType)
		{
			case JointClamping.Complete:
				for(int c = 1; c < jointConnections.Count; c++)
				{
					jointConnections[c-1].transform.position = Vector3Clamp(jointConnections[c-1].transform.position, jointConnections[c].transform.position, jointGap + clampError);
				}
				break;
			case JointClamping.Limited:
				if(FreezeBeg)
				{
					ropeEnd.transform.position = Vector3Clamp(ropeEnd.transform.position, transform.position, endDistance + clampError);
				}
				else if(FreezeEnd)
				{
					transform.position = Vector3Clamp(transform.position, ropeEnd.transform.position, endDistance + clampError);
				}
				else
				{
					ropeEnd.transform.position = Vector3Clamp(ropeEnd.transform.position, transform.position, endDistance + clampError);
				}
				break;
			default:
				break;
		}
	}
	
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space))
			DestroyRope();
		
		if(type == RopeType.Line)
		{
			for(int i = 0; i < jointConnections.Count; i++)
			{
				line.SetPosition(i, jointConnections[i].transform.position);
			}
		}
	}
	
	void DestroyRope()
	{
		foreach(GameObject go in jointConnections)
			DestroyImmediate(go);
		
		DestroyImmediate(jointParent);
		
		jointConnections.Clear();
	}
	
	void PlacePrefabs()
	{
		try{ DestroyImmediate(GetComponent<LineRenderer>()); line = null; } catch {}
		
		if(type == RopeType.Prefab && prefab != null)
		{
			for(int i = 0; i < linkCount; i++)
			{
				GameObject tPrefab = (GameObject)Instantiate((Object)prefab);
				
				tPrefab.transform.position = jointConnections[i].transform.position + (jointHeading/2 * jointGap);
				tPrefab.transform.LookAt(transform.position);
				tPrefab.transform.parent = jointConnections[i].transform;
				tPrefab.transform.Rotate(0,0,altRotation * i);
				tPrefab.transform.localScale *= scale;
			}
		}
	}
	
	Vector3 tVect = Vector3.zero;
	public float angle = 0;
	void BuildMesh()
	{
		try{ DestroyImmediate(GetComponent<LineRenderer>()); line = null; } catch {}
		
		if(type == RopeType.Mesh)
		{
		}
	}
	
	void BuildLine()
	{
		line = GetComponent<LineRenderer>();
		
		if(line==null)
			line = gameObject.AddComponent<LineRenderer>();
			
		if(jointConnections.Count>0)
		{
			line.SetVertexCount(jointConnections.Count);
			line.SetWidth(ropeRadius*2,ropeRadius*2);
			
			try
			{
				lineMaterial.SetTextureScale("_MainTex",new Vector3(lineMaterialRepeat,1));
				line.renderer.sharedMaterial = lineMaterial;
			} catch { Debug.LogWarning("lineMaterial needs a material assigned to it!"); }
			
			for(int i = 0; i < jointConnections.Count; i++)
			{
				line.SetPosition(i, jointConnections[i].transform.position);
			}
		}
	}
	
	void PlaceJointConnections()
	{	
		if(Application.isPlaying)
			jointParent = new GameObject("Rope");
		else
			jointParent = new GameObject("TempRope");
		
		jointParent.transform.position = ropeEnd.transform.position;
		jointParent.transform.LookAt(transform.position);
		
		GameObject tJC;
		ConfigurableJoint tCJ;
		SoftJointLimit sjl;
		JointDrive jd;
		CapsuleCollider cc;
		SphereCollider sc;
		
		for(int i = 0; i <= linkCount; i++)
		{
			tJC = new GameObject("Connection_"+i.ToString());
			tJC.transform.position = ropeEnd.transform.position + (jointHeading * jointGap * i);
			tJC.transform.LookAt(transform.position + jointHeading);
			
			tJC.transform.parent = jointParent.transform;
			tJC.AddComponent<Rigidbody>();
			tJC.rigidbody.drag = jointDrag;
			tJC.rigidbody.useGravity = jointsHaveGravity;
			
			switch(constraintPlane)
			{
				case ConstraintPlane.X_Y:
					tJC.rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;
					break;
				case ConstraintPlane.Y_Z:
					tJC.rigidbody.constraints = RigidbodyConstraints.FreezePositionX;
					break;
				case ConstraintPlane.Z_X:
					tJC.rigidbody.constraints = RigidbodyConstraints.FreezePositionY;
					break;
				default:
					break;
			}
			
			if(includeSphereColliders)
			{
				sc = tJC.AddComponent<SphereCollider>();
				sc.radius = ropeRadius;
				sc.material = ropePhysicMaterial;
			}
			
			if(includeCapsuleColliders && i<linkCount)
			{
				cc = tJC.AddComponent<CapsuleCollider>();
				cc.center = new Vector3(0,0,jointGap/2);
				cc.height = jointGap * 1.33f;
				cc.direction = 2;
				cc.radius = ropeRadius;
				cc.material = ropePhysicMaterial;
			}
			
			tCJ = tJC.AddComponent<ConfigurableJoint>();
			try{tCJ.connectedBody = jointConnections[i-1].rigidbody; }catch{ tCJ.connectedBody = ropeEnd.rigidbody; }
			
			//tCJ.swingAxis = new Vector3(1,1,1);
			tCJ.xMotion = ConfigurableJointMotion.Locked;
			tCJ.yMotion = ConfigurableJointMotion.Locked;
			tCJ.zMotion = ConfigurableJointMotion.Locked;
			
			tCJ.angularZMotion = ConfigurableJointMotion.Limited;
			sjl = new SoftJointLimit(){ limit = MinMaxTwist };
			tCJ.angularZLimit = sjl;
		
			
			jd = new JointDrive() { mode = JointDriveMode.Position };
			tCJ.xDrive = jd;
			tCJ.yDrive = jd;
			tCJ.zDrive = jd; 
			
			tCJ.projectionMode = JointProjectionMode.PositionOnly;
			tCJ.projectionDistance = 0.1f;
			
			
			if(i == linkCount)
			{
				tCJ = tJC.AddComponent<ConfigurableJoint>();
					
				tCJ.connectedBody = rigidbody;
				tCJ.xMotion = ConfigurableJointMotion.Locked;
				tCJ.yMotion = ConfigurableJointMotion.Locked;
				tCJ.zMotion = ConfigurableJointMotion.Locked;
				
				tCJ.angularZMotion = ConfigurableJointMotion.Limited;
				sjl = new SoftJointLimit(){ limit = MinMaxTwist };
				tCJ.angularZLimit = sjl;
				
				jd = new JointDrive() { mode = JointDriveMode.Position };
				tCJ.xDrive = jd;
				tCJ.yDrive = jd;
				tCJ.zDrive = jd; 
				
				tCJ.projectionMode = JointProjectionMode.PositionOnly;
				tCJ.projectionDistance = 0.1f;	
			}
			
			jointConnections.Add(tJC);
		}
	}
	
	Vector3 Vector3Clamp(Vector3 objectToClamp, Vector3 aroundWhatsPosition, float maximumDistance)
    {
        return new Vector3(Mathf.Clamp(objectToClamp.x, aroundWhatsPosition.x - maximumDistance, aroundWhatsPosition.x + maximumDistance), Mathf.Clamp(objectToClamp.y, aroundWhatsPosition.y - maximumDistance, aroundWhatsPosition.y + maximumDistance), Mathf.Clamp(objectToClamp.z, aroundWhatsPosition.z - maximumDistance, aroundWhatsPosition.z + maximumDistance));
    }
}
