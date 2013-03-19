/*
 * Implementation based on xTUIO (www.xtuio.com)
 * @author Rasmus H
 * @version 1.0
 */
using UnityEngine;
using System.Collections;
using TUIO;
using System.Collections.Generic;

public class mtPartManager : MonoBehaviour {
	
	public GameObject proxyObject;
	private GameObject copyObject;
	private mtCntrConnector tuioInput;
	public float cameraPixelWidth;
	public float cameraPixelHeight;
	private bool didChange;
	private float lastTime = 0;
	private float currentTime = 0;
	
	//calibration
	public float xTCalib;
	public float yTCalib;
	
	//events
	private ArrayList eventQueue;
	private Object eventQueueLock;
	public Dictionary<long,mtCntrEvent> activeEvents;
	//object list
	public Dictionary<long, GameObject> activeObjects;
	
	// Use this for initialization
	void Start () {
		eventQueue = new ArrayList();
		eventQueueLock = new Object(); 
		activeEvents = new Dictionary<long,mtCntrEvent>(100);
		activeObjects = new Dictionary<long, GameObject>(100);
		
		didChange = false;
		tuioInput = new mtCntrConnector();
		tuioInput.collectEvents = true;
		//cameraPixelWidth = Camera.main.pixelWidth;
		//cameraPixelHeight = Camera.main.pixelHeight;
		cameraPixelWidth = 13.3F;
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
				foreach (mtCntrEvent anEvent in eventQueue) {
				
					//new events: are raycastet and object is added to stage
					if (anEvent.eventState == mtEventState.Began) {					
						
						//If the object is not on the list add it
						if (!activeObjects.ContainsKey(anEvent.eventID)) {
							try
							{
								copyObject = (GameObject)Instantiate(proxyObject, Vector3.zero, Quaternion.identity);
								activeObjects.Add(anEvent.eventID, copyObject);
							}
							catch (KeyNotFoundException)
							{
								Debug.Log("An object with " + anEvent.eventID + " already exists.");
							}
						}
						
					}
					//existing events: getting objects with corresponding ids and move them in x y (unity x z)
					if (anEvent.eventState == mtEventState.Moved) {
						
						if(anEvent.contour != null)
						{
							//visual debug (draws a 2D line to show contour / feedback)
							Vector3[] lineRndr = new Vector3[anEvent.contour.Length+1];
							for(int i=0; i<=anEvent.contour.Length; i++)
							{
								if(i == anEvent.contour.Length)
								{
									lineRndr[i] = new Vector3(anEvent.contour[0].x + anEvent.screenPosition.x, 0, anEvent.contour[0].z + anEvent.screenPosition.z);
								}else{
									lineRndr[i] = new Vector3(anEvent.contour[i].x + anEvent.screenPosition.x, 0, anEvent.contour[i].z + anEvent.screenPosition.z);
								}
							} 	
							
							//passing verts and tris to object on the list
							if (activeObjects.ContainsKey(anEvent.eventID)) {
								try
								{
									copyObject = (GameObject)activeObjects[anEvent.eventID];
									copyObject.GetComponent<mtParticleProxy>().generateParticles(anEvent.contour, anEvent.screenPosition);
									if(copyObject.GetComponent<mtParticleProxy>().lineEnabled)
									{
										copyObject.GetComponent<mtParticleProxy>().drawLine(lineRndr);
									}
									
								}
								catch (KeyNotFoundException)
								{
									Debug.Log("Error while finding object: " + anEvent.eventID);
								}		
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
		
		mtCntrEvent newEvent = new mtCntrEvent(cursor.getSessionID()); 
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
		mtCntrEvent anEvent = activeEvents[cursor.getSessionID()];
		anEvent.eventState = mtEventState.Moved;
		
		if(cursor.getContour() != null)
		{
			List<TuioPoint> contr = cursor.getContour();
			List<Vector3> cntrPoints = new List<Vector3>();	
			foreach(TuioPoint pnt in contr)
			{
				float xC = pnt.getX();
				float yC = pnt.getY();
				cntrPoints.Add(new Vector3(xC * cameraPixelWidth, 0, (1.0f - yC ) * cameraPixelHeight));
			}
			anEvent.contour = cntrPoints.ToArray();
			
			//calibrate and CoM (center of mass)
			float xCoM = 0.0F;
			float yCoM = 0.0F;
			Matrix mat = Matrix.Translate(xTCalib, yTCalib, 0.0F);
			for(int i=0; i<anEvent.contour.Length; i++)
			{
				anEvent.contour[i] = mat.TransformVector( anEvent.contour[i] );
				xCoM += anEvent.contour[i].x;
				yCoM += anEvent.contour[i].y;
			}	
			xCoM /= anEvent.contour.Length;
			yCoM /= anEvent.contour.Length;
			
			//translate to 0
			for(int i=0; i<anEvent.contour.Length; i++)
			{
				anEvent.contour[i].x -= xCoM;
				anEvent.contour[i].y -= yCoM;
			}	
			
			anEvent.lastScreenPosition = anEvent.screenPosition;
			anEvent.screenPosition = new Vector3(xCoM, 0, yCoM); 
			
		}
		
		lock (eventQueueLock) eventQueue.Add(anEvent);
		didChange = true;
	}
	public void removeCursor(TuioCursor cursor) {
		
		if (!activeEvents.ContainsKey(cursor.getSessionID())) return;
		mtCntrEvent anEvent = activeEvents[cursor.getSessionID()];
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
			cameraPixelHeight += 0.1F;
			
		if(Input.GetKeyDown(KeyCode.DownArrow))
			yTCalib -= 0.01F;
		
		if(Input.GetKeyDown(KeyCode.S))
			cameraPixelHeight -= 0.1F;
		
		if(Input.GetKeyDown(KeyCode.LeftArrow))
			xTCalib -= 0.01F;
		
		if(Input.GetKeyDown(KeyCode.A))
			cameraPixelWidth -= 0.1F;
		
		if(Input.GetKeyDown(KeyCode.RightArrow))
			xTCalib += 0.01F;
			
		if(Input.GetKeyDown(KeyCode.D))
			cameraPixelWidth += 0.1F;
		
		if(Input.GetKey("space"))
		{
			xTCalib = 0.0F;
			yTCalib = 0.0F;
			cameraPixelWidth = 13.333333F;
			cameraPixelHeight = 10;
		}
	}
}
