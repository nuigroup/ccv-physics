using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RACOM;

/* ################################## 
Sets up OSC connection and
Converts the data into usable Vector3 array and a vector3 list
 ################################### */
public class ibheConnector : MonoBehaviour {
	
	public raComClient client;
	
	public List<Vector3> raVertices;
	public Vector3[] vertexData;
	public int widthPoints;
	public int heightPoints;
	public List<float> raHeight;
	public float[] heightData;
	private float cameraPixelWidth;
	private float cameraPixelHeight;
	
	private float xTCalib;
	private float yTCalib;
	
	// Use this for initialization
	void Start () {
		
		client = new raComClient(3333);
		client.connect();
		Debug.Log("raCom connected");
		raVertices = new List<Vector3>();
		cameraPixelWidth = 13.3333333333F;
		cameraPixelHeight = 10;
		xTCalib = 0.0F;
		yTCalib = 0.0F;
		widthPoints = 0;
		heightPoints = 0;
		heightData = new float[3072];
	}
	
	// Update is called once per frame
	void Update () {		
		
		calibrate();

		if(client.dataPoints != null)
		{
			/*
			//Debug.Log((int)client.dataPoints[1]);
			widthPoints = (int)client.dataPoints[2];
			heightPoints = (int)client.dataPoints[3];
			
			for (int i=4;i<(int)client.dataPoints[1];i++)
			{
				heightData[i] = (float)client.dataPoints[i];	
			}
			*/
			
			raVertices.Clear();
			for (int i=2;i<=(int)client.dataPoints[1];i+=3)
			{
				if(i+3 <= (int)client.dataPoints[1])
				{
						//x height y
					raVertices.Add(new Vector3 ((float)client.dataPoints[i] * cameraPixelWidth, -0.8f - ((float)client.dataPoints[i+1] * 4), (1.0f - (float)client.dataPoints[i+2]) * cameraPixelHeight));
						
				}
			}
		   	//widthPoints = (int)client.dataPoints[2];
			//heightPoints = (int)client.dataPoints[3];
			vertexData = raVertices.ToArray();

			//calibrate
			Matrix mat = Matrix.Translate(xTCalib, yTCalib, 0.0F);
			for(int i=0; i<vertexData.Length; i++)
			{
				vertexData[i] = mat.TransformVector( vertexData[i] );
			}
			
			
		}else{
			Debug.Log("raCom no data");
		}
	}
	
	void OnDisable () {
		client.disconnect();
		Debug.Log("raCom disconnected");
	}
	
	public void calibrate()
	{
		if(Input.GetKeyDown(KeyCode.UpArrow))
			yTCalib += 0.1F;
		
		if(Input.GetKeyDown(KeyCode.W))
			cameraPixelHeight++;
			
		if(Input.GetKeyDown(KeyCode.DownArrow))
			yTCalib -= 0.1F;
		
		if(Input.GetKeyDown(KeyCode.S))
			cameraPixelHeight--;
		
		if(Input.GetKeyDown(KeyCode.LeftArrow))
			xTCalib -= 0.1F;
		
		if(Input.GetKeyDown(KeyCode.A))
			cameraPixelWidth--;
		
		if(Input.GetKeyDown(KeyCode.RightArrow))
			xTCalib += 0.1F;
			
		if(Input.GetKeyDown(KeyCode.D))
			cameraPixelWidth++;
		
		if(Input.GetKey("space"))
		{
			xTCalib = 0.0F;
			yTCalib = 0.0F;
			cameraPixelWidth = 1024;
			cameraPixelHeight = 768;
		}
	}
}