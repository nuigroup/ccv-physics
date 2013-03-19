using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TUIO;

public class mtConnector : TuioListener {
	
	public List<TuioCursor> activeCursors;
	private mtManager mtM;
	private TuioClient client;
	private bool cursorDidChange;
	
	public mtConnector(mtManager mang)
	{
		client = new TuioClient(3333);
		client.addTuioListener(this);
		client.connect();
		cursorDidChange = false;
		activeCursors = client.getTuioCursors();
		Debug.Log("Commencing TUIO connection");
		mtM = mang;
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
		cursorDidChange = true;
		if(mtM) mtM.newCursor(c);
	}
	public void updateTuioCursor(TuioCursor c) {
		cursorDidChange = true;
		if(mtM) mtM.updateCursor(c);
	}
	public void removeTuioCursor(TuioCursor c) {
		cursorDidChange = true;
		if(mtM) mtM.removeCursor(c);
	}
	
	// this is the end of a single frame
	// we only really need to call the frame end if something actually happened this frame
	public void refresh(TuioTime ftime) {
	
		if (!cursorDidChange) return;
		cursorDidChange = false;
	}

}