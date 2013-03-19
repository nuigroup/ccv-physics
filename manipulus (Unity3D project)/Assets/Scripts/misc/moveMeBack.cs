using UnityEngine;
using System.Collections;

public class moveMeBack : MonoBehaviour {
	
	public Vector3 start;
	public Vector3 now;
	public Transform transf;
	public float smooth = 0.6F;
	public bool moveBack = false;
	public Rigidbody rigidb;
	
	// Use this for initialization
	void Start () {
		transf = this.GetComponent<Transform>();
		rigidb = this.GetComponent<Rigidbody>();
		start = transf.position;
	}
	
	// Update is called once per frame
	void Update () {
		now = transf.position;
		
		float dist = Vector3.Distance(now, start);
		
		if(dist > 150)
			moveBack = true;
		
		if(dist < 20)
			moveBack = false;
			
		
		if(moveBack)
		{
			rigidb.isKinematic = true;
			transf.position = Vector3.Lerp(now, start, Time.deltaTime * smooth);
		}else{
			rigidb.isKinematic = false;
		}
	}
}
