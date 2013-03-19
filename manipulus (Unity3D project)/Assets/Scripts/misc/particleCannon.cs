using UnityEngine;
using System.Collections;

public class particleCannon : MonoBehaviour {
	
	public GameObject particle;
	public int particleCount = 70;
	private int currentCount = 0;
	private bool onlyOnce = true;

	// Use this for initialization
	void Start () {
	
	}
	
	void OnTriggerEnter(Collider other) {
		if(other.tag == "proxyobject" && onlyOnce)
		{
			onlyOnce = false;
			spawnObjects();
		}
	}
	
	private void spawnObjects() {
		if(currentCount < particleCount)
		{
			GameObject clone;
			clone = (GameObject)Instantiate(particle, this.transform.position , Quaternion.identity);
			clone.GetComponent<Rigidbody>().velocity = clone.transform.TransformDirection(new Vector3(Random.Range(-10.0f, 10.0f), 0, Random.Range(-10.0f, 10.0f)));
			clone.transform.parent = this.transform;
			currentCount++;
			StartCoroutine(waitAndContinue());
		}else{
			GetComponent<MeshRenderer>().enabled = false;
		}
	}
	
	IEnumerator waitAndContinue(){
		yield return new WaitForSeconds (0.1f);
		spawnObjects();
	}
}
