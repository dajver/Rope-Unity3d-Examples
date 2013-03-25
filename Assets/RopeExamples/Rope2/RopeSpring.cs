//RopeSpring.cs

using UnityEngine;

using System.Collections;

 

public class RopeSpring {

    public Rigidbody mass1;

    public Rigidbody mass2;

 

    private float springConstant;

    private float springLength;

    private float frictionConstant;

 

    public RopeSpring(GameObject m1, GameObject m2, float sc, float sl, float fc){

        springConstant = sc;

        springLength = sl;

        frictionConstant = fc;

 

        mass1 = m1.rigidbody;

        mass2 = m2.rigidbody;

    }

 

    public void solve(float drag, float timestep)

    {

        

        Vector3 springVector =      // Vector Between The Two Masses

            mass1.transform.TransformPoint(mass1.centerOfMass) -

            mass2.transform.TransformPoint(mass2.centerOfMass); 

        

        float r = springVector.magnitude;               // Distance Between The Two Masses

        Vector3 d = Vector3.one * drag;

 

        Vector3 force=Vector3.zero;                         // Force Initially Has A Zero Value

        

        if (r != 0)                         // To Avoid A Division By Zero... Check If r Is Zero The Spring Force Is Added To The Force     

            force += -(springVector / r) * (r - springLength) * springConstant;

        

        // The Friction Force Is Added To The force With This Addition We Obtain The Net Force Of The Spring

        force += -(mass1.velocity - mass2.velocity) * frictionConstant;

        

        mass1.AddForce(force * drag);                   // Force Is Applied To mass1

        mass2.AddForce(-force * drag);                  // The Opposite Of Force Is Applied To mass2

    }

}