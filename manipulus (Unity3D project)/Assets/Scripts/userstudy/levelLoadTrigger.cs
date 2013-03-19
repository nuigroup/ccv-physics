using UnityEngine;
using System.Collections;

public class levelLoadTrigger : MonoBehaviour {
	
	public int levelNumber = 0;
	
	void OnTriggerEnter(Collider other) {
		if(levelNumber == 8)
			Application.Quit();
		else
			Application.LoadLevel(levelNumber);
	}
	
	void OnMouseDown() {
		if(levelNumber == 8)
			Application.Quit();
		else
			Application.LoadLevel(levelNumber);
	}
}
