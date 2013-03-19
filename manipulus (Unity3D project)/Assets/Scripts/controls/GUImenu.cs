using UnityEngine;
using System.Collections;

public class GUImenu : MonoBehaviour {
	
	public GameObject contourTUIO;
	public GameObject regularTUIO;
	public GameObject ibheMESH;
	public GameObject ibhePROXY;
	public GameObject particlePROXY;
	public GameObject ibheMeshPROXY;
	
	private GameObject activeMode;
	
	private bool showMenu = false;
	private int modusInt = 0;
	private string[] toolbarStrings = new string[] { "None", "IBHE", "IBHE-M", "Contour", "Proxy", "Particle", "IBHE-P"};
	private bool toggleGrid = false;
	
	private bool nomodeON = false;
	private bool contourON = false;
	private bool ibheON = false;
	private bool ibheMeshON = false;
	private bool proxyON = false;
	private bool particleON = false;
	private bool meshProxyON = false;
	
	private float xStart;
	private float yStart;
	private float sWidth;
	private float sHeight;
	
	private LineRenderer lineRenderer;
	
	//compile
	void Awake() {
		DontDestroyOnLoad(transform.gameObject);
	}
	
	// Use this for initialization
	void Start () {
		xStart = 0.0f;
		yStart = 0.0f;
		sWidth = 0.0f;
		sHeight = 0.0f;
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.SetVertexCount(5);
		lineRenderer.SetPosition(0, new Vector3(xStart, 5, yStart));
		lineRenderer.SetPosition(1, new Vector3(xStart, 5, yStart+sHeight));
		lineRenderer.SetPosition(2, new Vector3(xStart+sWidth, 5, yStart+sHeight));
		lineRenderer.SetPosition(3, new Vector3(xStart+sWidth, 5, yStart));
		lineRenderer.SetPosition(4, new Vector3(xStart, 5, yStart));
	}
	
	// Update is called once per frame
	void Update () {
		
		if(Input.GetKeyDown(KeyCode.Return))
		{
			if(showMenu)
				showMenu = false;
			else
				showMenu = true;
		}
		
		if(!showMenu || !toggleGrid)
			lineRenderer.enabled = false;		
		
		switch(modusInt)
		{
		case 0:
				noMode();
			break;
			
		case 1:
				initIBHE();
			break;
			
		case 2:
				noMode(); //taken out
			break;
			
		case 3:
				initContour();
			break;
			
		case 4:
				initProxy();
			break;
			
		case 5:
				initParticle();
			break;
			
		case 6:
				initMeshProxy();
			break;
				
		}
			
		if(toggleGrid)
			showGrid();
		
	}
	
	void OnGUI() {
		
		if(showMenu)
		{
			modusInt = GUI.Toolbar(new Rect(55, 135, 500, 30), modusInt, toolbarStrings);
			toggleGrid = GUI.Toggle(new Rect(55, 190, 100, 30), toggleGrid, "Grid");
		}
	}
	
	void OnLevelWasLoaded() {
		contourON = false;
		ibheON = false;
		ibheMeshON = false;
		proxyON = false;
		particleON = false;
		meshProxyON = false;
		nomodeON = false;
	}
	

	private void noMode() {
		if(!nomodeON)
		{
			if(activeMode != null)
			{
				Destroy(activeMode);	
			}
			Debug.Log("starting no mode");	
			nomodeON = true;
		}	
		contourON = false;
		ibheON = false;
		ibheMeshON = false;
		proxyON = false;
		particleON = false;
		meshProxyON = false;
	}
	private void initContour() {
		
		if(!contourON)
		{
			if(activeMode != null)
			{
				Destroy(activeMode);	
			}
			Debug.Log("starting contour");	
			contourON = true;
			activeMode = (GameObject)Instantiate(contourTUIO, Vector3.zero, Quaternion.identity);	
		}
		nomodeON = false;
		ibheON = false;
		ibheMeshON = false;
		proxyON = false;
		particleON = false;
		meshProxyON = false;
	}
	private void initIBHEmesh(){
		if(!ibheMeshON)
		{
			if(activeMode != null)
			{
				Destroy(activeMode);	
			}
			Debug.Log("starting IBHE MESH");	
			ibheMeshON = true;
			activeMode = (GameObject)Instantiate(ibheMESH, Vector3.zero, Quaternion.identity);
		}
		nomodeON = false;
		contourON = false;
		ibheON = false;
		proxyON = false;
		particleON = false;
		meshProxyON = false;
	}
	
	private void initIBHE(){
		if(!ibheON)
		{
			if(activeMode != null)
			{
				Destroy(activeMode);	
			}
			Debug.Log("starting IBHE PROXIES");	
			ibheON = true;
			activeMode = (GameObject)Instantiate(ibhePROXY, Vector3.zero, Quaternion.identity);
		}
		nomodeON = false;
		contourON = false;
		ibheMeshON = false;
		proxyON = false;
		particleON = false;
		meshProxyON = false;
	}
	
	private void initProxy(){
		if(!proxyON)
		{
			if(activeMode != null)
			{
				Destroy(activeMode);	
			}
			Debug.Log("starting regular proxy");	
			proxyON = true;
			activeMode = (GameObject)Instantiate(regularTUIO, Vector3.zero, Quaternion.identity);
		}
		nomodeON = false;
		contourON = false;
		ibheON = false;
		ibheMeshON = false;
		particleON = false;
		meshProxyON = false;
	}
	
	private void initParticle(){
		if(!particleON)
		{
			if(activeMode != null)
			{
				Destroy(activeMode);	
			}
			Debug.Log("starting particle proxy");	
			particleON = true;
			activeMode = (GameObject)Instantiate(particlePROXY, Vector3.zero, Quaternion.identity);
		}
		proxyON = false;
		nomodeON = false;
		contourON = false;
		ibheON = false;
		ibheMeshON = false;
		meshProxyON = false;
	}
	
	private void initMeshProxy(){
		if(!meshProxyON)
		{
			if(activeMode != null)
			{
				Destroy(activeMode);	
			}
			Debug.Log("starting IBHE MESH proxy");	
			meshProxyON = true;
			activeMode = (GameObject)Instantiate(ibheMeshPROXY, Vector3.zero, Quaternion.identity);
		}
		proxyON = false;
		nomodeON = false;
		contourON = false;
		ibheON = false;
		ibheMeshON = false;
		particleON = false;
	}
	
	private void showGrid() {
		
		switch(modusInt)
		{
		case 0:
				
			break;
			
		case 1:
				
			break;
			
		case 2:
				
			break;
			
		case 3:
			mtCntrManager script3 = activeMode.GetComponent<mtCntrManager>();
			xStart = script3.xTCalib;
			yStart = script3.yTCalib;
			sWidth = script3.cameraPixelWidth;
			sHeight = script3.cameraPixelHeight;
			break;
			
		case 4:
				
			break;
			
		case 5:
			mtPartManager script5 = activeMode.GetComponent<mtPartManager>();
			xStart = script5.xTCalib;
			yStart = script5.yTCalib;
			sWidth = script5.cameraPixelWidth;
			sHeight = script5.cameraPixelHeight;	
			break;
				
		}
		
		lineRenderer.enabled = true;
		lineRenderer.SetPosition(0, new Vector3(xStart, 3, yStart));
		lineRenderer.SetPosition(1, new Vector3(xStart, 3, yStart+sHeight));
		lineRenderer.SetPosition(2, new Vector3(xStart+sWidth, 3, yStart+sHeight));
		lineRenderer.SetPosition(3, new Vector3(xStart+sWidth, 3, yStart));
		lineRenderer.SetPosition(4, new Vector3(xStart, 3, yStart));
	}
}
