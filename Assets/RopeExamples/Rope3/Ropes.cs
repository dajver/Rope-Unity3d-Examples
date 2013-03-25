using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Ropes : MonoBehaviour {

	private float shakleLength=0.5f;
	private float ropeThickness = 0.1f;
	private int shakleCount=5;

	[SerializeField]
    public GameObject box; // NGui Sprite
	[SerializeField]
	public bool isBalloon = false;

	private GameObject ropeContainer;

	public void Start ()
	{

	// check if container already present
	Transform tf =transform.Find("New Game Object");

	if(tf!=null)
	{
		ropeContainer =transform.Find("New Game Object").gameObject as GameObject;
		// detach box form game object to prevent destroying it
		box.transform.parent=this.transform;

		// container is present, destroy old and proceed to renew
		GameObject.DestroyImmediate(ropeContainer);
		ropeContainer=null;
	}

	// create container
	ropeContainer=new GameObject();
	ropeContainer.transform.parent=this.transform;
	ropeContainer.transform.position=this.transform.position;
	ropeContainer.layer=this.gameObject.layer;

	GameObject shakle;

	// master shakle
	shakle=createRopeShakle(ropeContainer,shakleLength,0);

	for (int i=1;i<shakleCount;i++)
	{
		shakle=createRopeShakle(shakle,shakleLength,shakleLength);

	}

	// use instance of ropesprite present on stage
	///box = Instantiate(box) as GameObject;
	Vector3 shaklePosition = shakle.transform.position;
	Vector3 boxPosition = box.transform.position;

	// box position is last shakle position
	boxPosition=shaklePosition;

	if(isBalloon)
	{
		boxPosition.y+=shakleLength;
	}else{

		boxPosition.y-=shakleLength;
	}
	box.transform.position=boxPosition;

	box.transform.parent=shakle.transform;
	box.layer=this.gameObject.layer;

	// if box has hinge joint destor it
	DestroyImmediate (box.GetComponent<HingeJoint>());

	// add hinge joint to box
	HingeJoint hingeJoint=box.AddComponent<HingeJoint>();
	hingeJoint.connectedBody=shakle.rigidbody;

		// lets get the balloon going
		if(isBalloon)
		{
			box.rigidbody.useGravity=false;

			// if box has constant force first destroy it
			DestroyImmediate (box.GetComponent<ConstantForce>());
			box.AddComponent<ConstantForce>();

			// force up
			box.constantForce.force=new Vector3(0,1,0);
		}else{
			// if box has constant force first destroy it
			DestroyImmediate (box.GetComponent<ConstantForce>());
			box.rigidbody.useGravity=true;
		}

	}

	GameObject createRopeShakle(GameObject master,float length,float yOffset)
	{

		// empty pivot object
		GameObject shakle = new GameObject();
		shakle.transform.parent = master.transform;
		shakle.layer=this.gameObject.layer;

		// the primitive object placed within the emty pivot object
		GameObject shaklePrimitive=GameObject.CreatePrimitive(PrimitiveType.Cube);
		Vector3 primitivePosition = shaklePrimitive.transform.position;

		if(isBalloon)
		{
			primitivePosition = new Vector3(0,(float)(length*.5),0);

		}else{
			primitivePosition = new Vector3(0,(float)-(length*.5),0);
		}

   		shaklePrimitive.transform.position = primitivePosition;

   		shaklePrimitive.transform.parent = shakle.transform;
   		shaklePrimitive.transform.localScale=new Vector3(ropeThickness,length,ropeThickness);

   		shaklePrimitive.layer=this.gameObject.layer;

   		// set position of shakle does not work on empty shakle:GameObject?
		if(isBalloon)
		{
			shakle.transform.position = new Vector3(master.transform.position.x,master.transform.position.y+yOffset,master.transform.position.z);
		}else{
			shakle.transform.position = new Vector3(master.transform.position.x,master.transform.position.y-yOffset,master.transform.position.z);
		}

   		// create the rigid body on the rope segment
		shakle.AddComponent<Rigidbody>();
		shakle.rigidbody.mass=.01f;

		// add hinge joint to shakle
		HingeJoint hingeJoint=shakle.AddComponent<HingeJoint>();
		hingeJoint.connectedBody=master.rigidbody;
		hingeJoint.axis=Vector3.forward;

		shakle.rigidbody.useGravity=true;
		shakle.rigidbody.constraints=RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX |RigidbodyConstraints.FreezeRotationY;

   		return shakle;
	}

}