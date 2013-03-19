using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (LineRenderer))]
public class mtParticleProxy : MonoBehaviour {

	public GameObject proxyObject;
	public long thisID;
	public bool lineEnabled;
	private ArrayList activeObjects;
	
	// Use this for initialization
	void Start () {
		lineEnabled = true;
		activeObjects = new ArrayList();
	}
	
	public void generateParticles(Vector3[] verts, Vector3 sPosi) {
		
		//delete all current proxies
		//for each contour point ray cast into scene
		//instantiate a proxy that goes from 0 to hitpoint

		if(activeObjects != null)
		{
			foreach(GameObject clone in activeObjects)
			{
				Destroy(clone);
			}
		}
		for(int i = 0; i < verts.Length; i++)
		{
			//determine height
			RaycastHit hit = new RaycastHit();
			Vector3 thePoint = Camera.main.WorldToScreenPoint(verts[i]);
			Ray ray = Camera.main.ScreenPointToRay(thePoint);

			if (Physics.Raycast(ray, out hit)){
				verts[i].y = hit.point.y + 2.5f;
				//Debug.DrawLine (ray.origin, hit.point);
			}else{
				Debug.Log("mtParticleProxy: Nothing hit, maybe add a groundplane with default layer");
				verts[i].y = -1;
			}
		
			GameObject copy;
			copy = (GameObject)Instantiate(proxyObject, verts[i], Quaternion.identity);
			Transform posi = copy.GetComponent<Transform>();
			posi.parent = this.transform;
			copy.GetComponent<Rigidbody>().MovePosition(sPosi+verts[i]);
			activeObjects.Add(copy);
		}
	
	}
	
	public void drawLine(Vector3[] cords)
	{
		LineRenderer lineRenderer = GetComponent<LineRenderer>();
		int lengthOfLineRenderer = cords.Length;
		lineRenderer.SetVertexCount(lengthOfLineRenderer);
		int i = 0;
		while (i < lengthOfLineRenderer) {
			Vector3 pos = new Vector3(cords[i].x, 0, cords[i].z);
			lineRenderer.SetPosition(i, pos);
			i++;
		}
	}
}