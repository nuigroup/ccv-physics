using UnityEngine;
using System.Collections;

public class resetPosition : MonoBehaviour {

	void OnTriggerEnter(Collider other) {
		if(other.tag != "proxyobject")
		{
			
			if(Application.loadedLevel == 4)
			{
				other.rigidbody.Sleep();
				other.transform.localPosition = new Vector3(11.6f, -3.8f, 8.0f);
				other.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
			}else if(Application.loadedLevel == 6){
				other.transform.localPosition = new Vector3(6.6f, -3 , 4.8f);
				other.transform.localRotation = Quaternion.identity;			
			}else{
				other.rigidbody.Sleep();
				other.transform.localPosition = new Vector3(6.6f, -3 , 4.8f);
				other.transform.localRotation = Quaternion.identity;
			}
		}
	}
}
