/*
 * Implementation based on xTUIO (www.xtuio.com)
 * @author Rasmus H
 * @version 1.0
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TUIO;

public enum mtCursorState {
	Add,
	Update,
	Remove
};

public class mtCursorEvent 
{
	public TuioCursor cursor;
	public mtCursorState state;
	
	public mtCursorEvent(TuioCursor c,mtCursorState s) {
		cursor = c;
		state = s;
	}
}

public class mtCntrConnector : TuioListener {
	
	public ArrayList activeCursorEvents = new ArrayList();
	public bool collectEvents = false;
	
	private TuioClient client;
	private object objectSync = new object();
	
	public mtCntrConnector()
	{
		client = new TuioClient(3333);
		client.addTuioListener(this);
		client.connect();
		Debug.Log("Commencing TUIO connection");
	}
	
	public ArrayList getAndClearCursorEvents() {
		ArrayList bufferList;
		lock(objectSync) {
			bufferList = new ArrayList(activeCursorEvents);
			activeCursorEvents.Clear();
		}
		return bufferList;
	}
	
	public void disconnect() 
	{
		client.disconnect();
		client.removeTuioListener(this);
	}

	public bool isConnected()
	{
		return client.isConnected();
	}
	
	// required implementations	
	public void addTuioObject(TuioObject o) {}
	public void updateTuioObject(TuioObject o) {}
	public void removeTuioObject(TuioObject o) {}
	
	// for now we are only interested in cursor objects, ie touch events
	public void addTuioCursor(TuioCursor c) {
		lock(objectSync) {
			if (collectEvents) activeCursorEvents.Add(new mtCursorEvent(c,mtCursorState.Add));
		}
	}
	public void updateTuioCursor(TuioCursor c) {
		lock(objectSync) {
			if (collectEvents) activeCursorEvents.Add(new mtCursorEvent(c,mtCursorState.Update));
		}
	}
	public void removeTuioCursor(TuioCursor c) {
		lock(objectSync) {
			if (collectEvents) activeCursorEvents.Add(new mtCursorEvent(c,mtCursorState.Remove));
		}
	}
	
	// this is the end of a single frame
	// we only really need to call the frame end if something actually happened this frame
	public void refresh(TuioTime ftime) {
		
	}

}