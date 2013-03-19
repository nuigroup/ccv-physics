/*
 * Implementation based on xTUIO (www.xtuio.com)
 * @author Rasmus H
 * @version 1.0
 */
using UnityEngine;
using System.Collections;
using TUIO;
using System.Collections.Generic;

public class mtManager : MonoBehaviour {
	
	public GameObject proxyObject;
	private GameObject copyObject;
	
	private mtConnector tuioInput;
	private float cameraPixelWidth;
	private float cameraPixelHeight;
	private bool didChange;
	
	//events
	private ArrayList eventQueue = new ArrayList();
	private Object eventQueueLock = new Object(); 
	public Dictionary<long,mtEvent> activeEvents = new Dictionary<long,mtEvent>(100);
	
	// Use this for initialization
	void Start () {
		didChange = false;
		tuioInput = new mtConnector(this);
		cameraPixelWidth = 13.3F;
		cameraPixelHeight = 10;
		Debug.Log("Screen Width: " + cameraPixelWidth + " Screen Height: " + cameraPixelHeight);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if(didChange)
		{
			//critical sections are locked to avoid errors while multi threading
			lock (eventQueueLock) {
				foreach (mtEvent anEvent in eventQueue) {
				
					//new events: are raycastet and object is added to stage
					if (anEvent.eventState == mtEventState.Began) {					
						RaycastHit hit = new RaycastHit();
						if (Physics.Raycast(anEvent.screenPosition, -Vector3.up,out hit, Mathf.Infinity)){
							float objHeight = proxyObject.transform.localScale.y / 2;
							//~ Debug.Log(objHeight);
							anEvent.zPos = hit.point.y + objHeight;
							//~ anEvent.zPos = 0;
						}else{
							Debug.Log("mtManager: Nothing hit, maybe add a groundplane with default layer");
							anEvent.zPos = 0;
						}
						//creating clone object
						Vector3 pos = new Vector3(anEvent.screenPosition.x, anEvent.zPos, anEvent.screenPosition.z);
						copyObject = (GameObject)Instantiate(proxyObject,  pos, Quaternion.identity);
						copyObject.GetComponent<mtObject>().eventID = anEvent.eventID;
					}
					//existing events: getting objects with corresponding ids and move them in x y (unity x z)
					if (anEvent.eventState == mtEventState.Moved) {
						GameObject[] clones = GameObject.FindGameObjectsWithTag("proxyobject");
						foreach(GameObject clone in clones)
						{	
							if(clone.GetComponent<mtObject>().eventID == anEvent.eventID)
							{
								Vector3 pos = new Vector3(anEvent.screenPosition.x, anEvent.zPos, anEvent.screenPosition.z);
								//clone.transform.position = pos;
								clone.GetComponent<Rigidbody>().MovePosition(pos);
							}
						}
					}				
					//ended events: deleting objects from stage
					if (anEvent.eventState == mtEventState.Ended) {
						GameObject[] clones = GameObject.FindGameObjectsWithTag("proxyobject");
						foreach(GameObject clone in clones)
						{	
							if(clone.GetComponent<mtObject>().eventID == anEvent.eventID)
							{
								Destroy(clone);
							}
						}
					}
				}
				eventQueue.Clear();	
			}
			didChange = false;
		}
	}
	
	void OnApplicationQuit() {
    	if (tuioInput != null) { 
			tuioInput.disconnect();
    	}
    }
	
	void OnDisable() {
		if (tuioInput != null) { 
			tuioInput.disconnect();
    	}	
		GameObject[] clones = GameObject.FindGameObjectsWithTag("proxyobject");
		foreach(GameObject clone in clones)
		{
			Destroy(clone);
		}
	}
	
	public void newCursor(TuioCursor cursor) {
		
		mtEvent newEvent = new mtEvent(cursor.getSessionID()); 
		newEvent.tuioPosition = new Vector2(cursor.getX(),(1.0f - cursor.getY()));
		//switching y and z (y is up axis by default in unity)
		newEvent.screenPosition = new Vector3(cursor.getX() * cameraPixelWidth, 0,(1.0f - cursor.getY()) * cameraPixelHeight); 
		newEvent.lastScreenPosition = newEvent.screenPosition;
		newEvent.eventState = mtEventState.Began;
		
		if (activeEvents.ContainsKey(cursor.getSessionID())) {
			//Already on list, remove old - add new 
			activeEvents.Remove(cursor.getSessionID());
		}
		activeEvents.Add( cursor.getSessionID(), newEvent );
		// queue it up for processing
		lock (eventQueueLock) eventQueue.Add(newEvent);
		didChange = true;
	}
	public void updateCursor(TuioCursor cursor) {

		if (!activeEvents.ContainsKey(cursor.getSessionID())) return;
		mtEvent anEvent = activeEvents[cursor.getSessionID()];
		anEvent.lastScreenPosition = anEvent.screenPosition;
		
		anEvent.screenPosition = new Vector3(cursor.getX() * cameraPixelWidth, 0,(1.0f - cursor.getY()) * cameraPixelHeight); 
		anEvent.tuioPosition = new Vector2(cursor.getX(),(1.0f - cursor.getY()));
		anEvent.eventState = mtEventState.Moved;

		lock (eventQueueLock) eventQueue.Add(anEvent);
		didChange = true;
	}
	public void removeCursor(TuioCursor cursor) {
		
		if (!activeEvents.ContainsKey(cursor.getSessionID())) return;
		mtEvent anEvent = activeEvents[cursor.getSessionID()];
		anEvent.eventState = mtEventState.Ended;	
		lock (eventQueueLock) { 
			eventQueue.Add(anEvent);
		}		
		activeEvents.Remove( cursor.getSessionID() );	
		didChange = true;
	}
}
