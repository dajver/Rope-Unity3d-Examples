using UnityEngine;
using System.Collections;

public class MyLineRenderer : MonoBehaviour {

    [SerializeField]
    private GameObject rope1, rope2;
    private Vector3 fingerPos;
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButton(0))
        {
            fingerPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 11));
            rope1.GetComponent<LineRenderer>().SetPosition(1, fingerPos);
            rope2.GetComponent<LineRenderer>().SetPosition(1, fingerPos);
            //rope1.transform.position = new Vector3(fingerPos.x, fingerPos.y, fingerPos.z);
            //rope2.transform.position = rope1.transform.position;
        }
	}
}
