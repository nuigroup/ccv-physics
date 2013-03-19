using UnityEngine;
using System.Collections;

public class ibheTuioEvent {
	
	public mtEventState eventState;

	public long eventID; // the unique ID for this touch, it will be the same throught the life of this touch

	public Vector2 tuioPosition; // the 2d position of this touch normalized to 0..1,0..1
	public Vector3 lastScreenPosition; // the most recent 2d position of this touch on the screen
	public Vector3 screenPosition; // the 2d position of this touch on the screen
	
	public float[] heights;
	
	public float xPos;
	public float yPos;
	public int width;
	public int height;
	
	public float minHeight;

	public ibheTuioEvent(long id)
	{
		this.eventID = id;
	}	
}