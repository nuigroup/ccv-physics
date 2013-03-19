using UnityEngine;
using System.Collections;

public class colorTrigger : MonoBehaviour {
	public bool listenOrange = false;
	public int orangeCount = 0;
	public int blueCount = 0;
	public colorTrigger otherTrigger;
	
	public GUIStyle triggerStyle;
	private bool isCompleted = false;
	private float startTime;
	private float endTime;
	private float finalTime;
	private bool displayOnce = false;
	
	// Use this for initialization
	void Start () {
		startTime = Time.realtimeSinceStartup;
		finalTime = 0.0f;
	}
	
	void OnGUI() {
		if(isCompleted)
		{
			GUI.Label(new Rect(300, 420, 800, 800), "Task Completed!", triggerStyle);
			GUI.Label(new Rect(290, 520, 800, 800), "Your time:" + finalTime.ToString() + " s" , triggerStyle);
		}
	}
	
	void OnTriggerEnter(Collider other) {
	
		if(!isCompleted)
		{
			if(listenOrange)
			{
				if(other.tag == "ocube")
				{
					orangeCount++;
					if(orangeCount == 8 && otherTrigger.blueCount == 8 || otherTrigger.orangeCount == 8 && blueCount == 8)
					{
						levelCompleted();	
					}
				}		
			}else{
				if(other.tag == "bcube")
				{
					blueCount++;
					if(orangeCount == 8 && otherTrigger.blueCount == 8 || otherTrigger.orangeCount == 8 && blueCount == 8)
					{
						levelCompleted();	
					}
				}			
			}	
		}
	}	
	
	void OnTriggerExit(Collider other) {
		if(listenOrange)
		{
			if(other.tag == "ocube")
			{
				orangeCount--;
			}		
		}else{
			if(other.tag == "bcube")
			{
				blueCount--;
			}			
		}	
	}
	
	public void startLevel(){
		startTime = Time.realtimeSinceStartup;
		otherTrigger.startTime = Time.realtimeSinceStartup;
	}
	
	private void levelCompleted() {
		if(!otherTrigger.displayOnce)
		{
			endTime = Time.realtimeSinceStartup;
			finalTime = endTime - startTime;
			isCompleted = true;
			displayOnce = true;
			GameObject menuRef = GameObject.FindWithTag("GUIMENU");
			menuRef.SendMessage("writeData", finalTime.ToString());
			StartCoroutine(waitAndLoad());
		}
	}
	
	IEnumerator waitAndLoad(){
		yield return new WaitForSeconds(1);
		Application.CaptureScreenshot("navigation.png");
		yield return new WaitForSeconds(5);
		Application.LoadLevel(0);
	}
}
