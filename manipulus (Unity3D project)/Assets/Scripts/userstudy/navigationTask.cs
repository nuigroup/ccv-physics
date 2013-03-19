using UnityEngine;
using System.Collections;

public class navigationTask : MonoBehaviour {
	
	public GUIStyle triggerStyle;
	private bool isCompleted = false;
	private float startTime;
	private float endTime;
	private float finalTime;

	// Use this for initialization
	void Start () {
		startTime = Time.realtimeSinceStartup;
		finalTime = 0.0f;
	}
	
	void OnGUI() {
		if(isCompleted)
		{
			GUI.Label(new Rect(310, 350, 800, 800), "Task Completed!", triggerStyle);
			GUI.Label(new Rect(290, 450, 800, 800), "Your time:" + finalTime.ToString() + " s" , triggerStyle);
		}
	}
	
	void OnTriggerEnter(Collider other) {
		if(other.tag == "triggerObject" && !isCompleted)
			levelCompleted();
	}
	
	public void startLevel(){
		startTime = Time.realtimeSinceStartup;
	}
	
	private void levelCompleted(){
		endTime = Time.realtimeSinceStartup;
		finalTime = endTime - startTime;
		isCompleted = true;
		GameObject menuRef = GameObject.FindWithTag("GUIMENU");
		menuRef.SendMessage("writeData", finalTime.ToString());
		StartCoroutine(waitAndLoad());
	}
	
	IEnumerator waitAndLoad(){
		yield return new WaitForSeconds(1);
		Application.CaptureScreenshot("navigation.png");
		yield return new WaitForSeconds(5);
		Application.LoadLevel(0);
	}
}
