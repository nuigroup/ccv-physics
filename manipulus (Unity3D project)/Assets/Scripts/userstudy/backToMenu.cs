using UnityEngine;
using System.Collections;

public class backToMenu : MonoBehaviour {
	private bool counting = false;
	private float startTime;
	private float nowTime;
	private Renderer mat;
	private Color startColor = Color.red;
	private Color endColor = Color.green;
	
	void Start () {
		mat = GetComponent<Renderer>();
	}
	
	void OnTriggerStay(Collider other) {
		if(other.tag == "proxyobject")
		{
			if(!counting)
			{
				startTime = Time.realtimeSinceStartup;
				nowTime = startTime;
				counting = true;
				mat.material.SetColor("_TintColor", startColor);
			}else{
				nowTime = Time.realtimeSinceStartup - startTime;
				Color colorNow = Color.Lerp(startColor, endColor, nowTime);
				mat.material.SetColor("_TintColor", colorNow);
				if(nowTime > 2.0f)
				{
					Application.LoadLevel(0);
				}
			}
		}
	}
	
	void OnTriggerExit(Collider other) {
		if(other.tag == "proxyobject")
		{
			counting = false;
			mat.material.SetColor("_TintColor", startColor);
		}
	}
}
