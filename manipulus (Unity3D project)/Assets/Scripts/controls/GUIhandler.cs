using UnityEngine;
using System.Collections;

public class GUIhandler : MonoBehaviour {

	public GameObject GUIOBJECT;
	//ONLY USED FOR INSTANTIATING GUImenu (so there are no dublicates)
	
	// Use this for initialization
	void Awake () {	
		if(GameObject.FindGameObjectsWithTag("GUIMENU").Length == 0)
			Instantiate(GUIOBJECT, Vector3.zero, Quaternion.identity);
	}
}
