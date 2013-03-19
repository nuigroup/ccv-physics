using UnityEngine;
using System.Collections;

public class startCounting : MonoBehaviour {
	
	public GameObject target;

	void OnTriggerEnter(Collider other) {
		if(other.tag == "proxyobject")
		{
			target.SendMessage("startLevel");
			Destroy(this.gameObject);
		}
		if(Application.loadedLevel == 4 && other.tag == "triggerObject")
		{
			target.SendMessage("startLevel");
			Destroy(this.gameObject);
		}
	}
}
