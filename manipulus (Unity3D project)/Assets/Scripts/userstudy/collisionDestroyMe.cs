using UnityEngine;
using System.Collections;

public class collisionDestroyMe : MonoBehaviour {

	void OnTriggerEnter(Collider other) {
		Destroy(other.gameObject);
	}	
	
}
