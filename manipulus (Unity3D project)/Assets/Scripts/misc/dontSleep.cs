using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
public class dontSleep : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		GetComponent<Rigidbody>().WakeUp();
	}
}
