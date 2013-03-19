/*
 * Implementation based on xTUIO (www.xtuio.com)
 * @author Rasmus H
 * @version 1.0
 */
using UnityEngine;
using System.Collections;
using TUIO;
using System.Collections.Generic;

public class ibheTuioManager : MonoBehaviour {
	
	public GameObject proxyObject;
	public GameObject splineObject;
	private GameObject copyObject;
	private ibheTuioConnector tuioInput;
	public float cameraPixelWidth;
	public float cameraPixelHeight;
	private bool didChange;
	public bool bsplinemode = false;
	private float lastTime = 0;
	private float currentTime = 0;
	
	//calibration
	public float xTCalib;
	public float yTCalib;
	
	//events
	private ArrayList eventQueue;
	private Object eventQueueLock;
	public Dictionary<long,ibheTuioEvent> activeEvents;
	//object list
	public Dictionary<long, GameObject> activeObjects;

	// Use this for initialization
	void Start () {
		eventQueue = new ArrayList();
		eventQueueLock = new Object(); 
		activeEvents = new Dictionary<long,ibheTuioEvent>(100);
		activeObjects = new Dictionary<long, GameObject>(100);
		
		didChange = false;
		tuioInput = new ibheTuioConnector();
		tuioInput.collectEvents = true;
		//cameraPixelWidth = Camera.main.pixelWidth;
		//cameraPixelHeight = Camera.main.pixelHeight;
		cameraPixelWidth = 13.33333333333333F;
		cameraPixelHeight = 10;
		Debug.Log("Screen Width: " + cameraPixelWidth + " Screen Height: " + cameraPixelHeight);
		xTCalib = 0.0F;
		yTCalib = 0.0F;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
		calibrate();	
		
		lastTime = currentTime;
		currentTime = Time.time;
	  if(currentTime - lastTime > 0)
	  {
		
		processEvents();
			
		if(didChange)
		{
			//critical sections are locked to avoid errors while multi threading
			lock (eventQueueLock) {
				foreach (ibheTuioEvent anEvent in eventQueue) {
				
					//new events: are raycastet and object is added to stage
					if (anEvent.eventState == mtEventState.Began) {			
						if (!activeObjects.ContainsKey(anEvent.eventID)) {
							try
							{
								//NOCH NEN RAY CAST EINBAUN
									//distance vom niedrigsten punkt zum hit point translaten
								if(!bsplinemode)
								{
									copyObject = (GameObject)Instantiate(proxyObject, Vector3.zero , Quaternion.identity);
									//copyObject.transform.position = new Vector3(anEvent.xPos, -2.9f, anEvent.yPos);
									activeObjects.Add(anEvent.eventID, copyObject);
									copyObject.GetComponent<ibheMeshObject>().generateMesh( anEvent.width, anEvent.height, anEvent.heights );
									copyObject.GetComponent<Rigidbody>().MovePosition(new Vector3(anEvent.xPos, -2.9f, anEvent.yPos));
								}else{
									copyObject = (GameObject)Instantiate(splineObject, Vector3.zero , Quaternion.identity);
									//copyObject.transform.position = new Vector3(anEvent.xPos, -2.9f, anEvent.yPos);
									activeObjects.Add(anEvent.eventID, copyObject);
									copyObject.GetComponent<ibheBsplineObject>().generateMesh(anEvent.width, anEvent.height, anEvent.heights);
									copyObject.GetComponent<Rigidbody>().MovePosition(new Vector3(anEvent.xPos, -2.9f, anEvent.yPos));
								}
							}
							catch (KeyNotFoundException)
							{
								Debug.Log("An object with " + anEvent.eventID + " already exists.");
							}
						}
						
					}
					//existing events: getting objects with corresponding ids and move them in x y (unity x z)
					if (anEvent.eventState == mtEventState.Moved) {
						//passing verts and tris to object on the list
						if (activeObjects.ContainsKey(anEvent.eventID)) {
							try
							{
								if(!bsplinemode)
								{
									copyObject = (GameObject)activeObjects[anEvent.eventID];
									//copyObject.transform.position = new Vector3(anEvent.xPos, -2.9f, anEvent.yPos);
									copyObject.GetComponent<ibheMeshObject>().generateMesh(anEvent.width, anEvent.height, anEvent.heights );
									copyObject.GetComponent<Rigidbody>().MovePosition(new Vector3(anEvent.xPos, -2.9f, anEvent.yPos));
								}else{
									copyObject = (GameObject)activeObjects[anEvent.eventID];
									//copyObject.transform.position = new Vector3(anEvent.xPos, -2.9f, anEvent.yPos);
									copyObject.GetComponent<ibheBsplineObject>().generateMesh(anEvent.width, anEvent.height, anEvent.heights );
									copyObject.GetComponent<Rigidbody>().MovePosition(new Vector3(anEvent.xPos, -2.9f, anEvent.yPos));
								}
							}
							catch (KeyNotFoundException)
							{
								Debug.Log("Error while finding object: " + anEvent.eventID);
							}		
						}
					}				
					//ended events: deleting objects from stage
					if (anEvent.eventState == mtEventState.Ended) {
						
						if (activeObjects.ContainsKey(anEvent.eventID)) {						
							try
							{
								Destroy((GameObject)activeObjects[anEvent.eventID]);
								activeObjects.Remove(anEvent.eventID);
							}
							catch (KeyNotFoundException)
							{
								Debug.Log("Error while removing object: " + anEvent.eventID);
							}						
						}	
					}
				}
				eventQueue.Clear();	
			}
			didChange = false;
		}
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
		if (activeObjects != null)
		{
			activeObjects.Clear();
			GameObject[] clones = GameObject.FindGameObjectsWithTag("proxyobject");
			foreach(GameObject clone in clones)
			{
				Destroy(clone);
			}
		}
	}
	
	public void newCursor(TuioCursor cursor) {
		
		ibheTuioEvent newEvent = new ibheTuioEvent(cursor.getSessionID()); 
		newEvent.tuioPosition = new Vector2(cursor.getX(),(1.0f - cursor.getY()));
		//switching y and z (y is up axis by default in unity)
		newEvent.screenPosition = new Vector3(cursor.getX() * cameraPixelWidth, 0,(1.0f - cursor.getY()) * cameraPixelHeight); 
		newEvent.lastScreenPosition = newEvent.screenPosition;
		newEvent.eventState = mtEventState.Began;
		newEvent.xPos = cursor.getHXpos() * cameraPixelWidth;
		newEvent.yPos = (1.0f - cursor.getHYpos()) * cameraPixelHeight;
		newEvent.width = cursor.getWidth();
		newEvent.height = cursor.getHeight();
		if( cursor.getHeightPoints() != null)
			newEvent.heights = cursor.getHeightPoints().ToArray();
		
		float min = 250.0F;
		for(int i=0; i < newEvent.heights.Length; i++)
		{
			if(((newEvent.heights[i] / 255.0F)*-4.0F) < min)
				min = (newEvent.heights[i] / 255.0F) * -4.0F;
		}
		newEvent.minHeight = min;
		
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
		ibheTuioEvent anEvent = activeEvents[cursor.getSessionID()];
		anEvent.lastScreenPosition = anEvent.screenPosition;
		anEvent.screenPosition = new Vector3(cursor.getX() * cameraPixelWidth, 0,(1.0f - cursor.getY()) * cameraPixelHeight); 
		anEvent.tuioPosition = new Vector2(cursor.getX(),(1.0f - cursor.getY()));
		anEvent.eventState = mtEventState.Moved;
		anEvent.xPos = cursor.getHXpos() * cameraPixelWidth;
		anEvent.yPos = (1.0f - cursor.getHYpos()) * cameraPixelHeight;
		anEvent.width = cursor.getWidth();
		anEvent.height = cursor.getHeight();
		if( cursor.getHeightPoints() != null)
			anEvent.heights = cursor.getHeightPoints().ToArray();
		
		float min = 250.0F;
		for(int i=0; i < anEvent.heights.Length; i++)
		{
			if(((anEvent.heights[i] / 255.0F)*-4.0F) < min)
				min = (anEvent.heights[i] / 255.0F) * -4.0F;
		}
		anEvent.minHeight = min;
		//calibrate
		//Matrix mat = Matrix.Translate(xTCalib, yTCalib, 0.0F);
		//anEvent.xPos = mat.TransformVector( anEvent.xPos );
		
		lock (eventQueueLock) eventQueue.Add(anEvent);
		didChange = true;
	}
	
	public void removeCursor(TuioCursor cursor) {
		
		if (!activeEvents.ContainsKey(cursor.getSessionID())) return;
		ibheTuioEvent anEvent = activeEvents[cursor.getSessionID()];
		anEvent.eventState = mtEventState.Ended;	
		lock (eventQueueLock) { 
			eventQueue.Add(anEvent);
		}		
		activeEvents.Remove( cursor.getSessionID() );	
		didChange = true;
	}
	
	//pulling events out of tuio input
	public void processEvents()
	{
		ArrayList events = tuioInput.getAndClearCursorEvents();
		// go through the events and dispatch
		foreach (mtCursorEvent cursorEvent in events) {
			if (cursorEvent.state == mtCursorState.Add) {
				newCursor(cursorEvent.cursor);
				continue;
			}
			if (cursorEvent.state == mtCursorState.Update) {
				updateCursor(cursorEvent.cursor);
				continue;
			}
			if (cursorEvent.state == mtCursorState.Remove) {
				removeCursor(cursorEvent.cursor);
				continue;
			}
		}
		//finishFrame();
	}
	
	public void calibrate()
	{
		if(Input.GetKeyDown(KeyCode.UpArrow))
			yTCalib += 0.01F;
		
		if(Input.GetKeyDown(KeyCode.W))
			cameraPixelHeight += 0.01F;
			
		if(Input.GetKeyDown(KeyCode.DownArrow))
			yTCalib -= 0.01F;
		
		if(Input.GetKeyDown(KeyCode.S))
			cameraPixelHeight -= 0.01F;
		
		if(Input.GetKeyDown(KeyCode.LeftArrow))
			xTCalib -= 0.01F;
		
		if(Input.GetKeyDown(KeyCode.A))
			cameraPixelWidth -= 0.01F;
		
		if(Input.GetKeyDown(KeyCode.RightArrow))
			xTCalib += 0.01F;
			
		if(Input.GetKeyDown(KeyCode.D))
			cameraPixelWidth += 0.01F;
		
		if(Input.GetKey("space"))
		{
			xTCalib = 0.0F;
			yTCalib = 0.0F;
			cameraPixelWidth = 13.333333F;
			cameraPixelHeight = 10;
		}
	}
}
