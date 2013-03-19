using UnityEngine;
using System.Collections;

/* ################################## 
Generates proxy objects (user defined) at the vertex locations
 ################################### */
public class ibheProxyObjects : MonoBehaviour {

	//public GameObject connection;
	public GameObject proxyObject;	
	private Transform instantPos;
	private GameObject copy;
	private ibheConnector dataScript;
	private Vector3[] raData;		
	
	// Use this for initialization
	void Start () {
		dataScript = (ibheConnector)GetComponentInChildren<ibheConnector>();
		instantPos = GetComponentInChildren<Transform>();
	}
	
	// Late Update is called once per frame but after all update function calls (to make sure vectors are updatet first)
	void LateUpdate () {
		raData = dataScript.vertexData;
		if(raData != null)
		{
			if(proxyObject == null)
			{
				Debug.Log("ibheProxyObjects.cs: Assign an object to be instantiated");
			}else{
				
				GameObject[] clones = GameObject.FindGameObjectsWithTag("proxyobject");
				foreach(GameObject clone in clones)
				{
					Destroy(clone);
				}
				
				foreach(Vector3 raVector in raData)
				{ 
					copy = (GameObject)Instantiate(proxyObject, raVector, Quaternion.identity);
					copy.GetComponent<Transform>().parent = instantPos;
				}		
				
			}
		}		
	} 
}
