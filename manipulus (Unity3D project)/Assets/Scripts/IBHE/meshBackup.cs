using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* ################################## 
Generates a mesh from the vertex data
 ################################### */
public class meshBackup : MonoBehaviour {
	//variables
	//public GameObject connection;
	private ibheConnector dataScript;
	public Vector3[] ibheVertices;	
	private int[] ibheTriangles;
	private MeshCollider mcol;
	private int widthP;
	private int heightP;
	
	// Use this for initialization
	void Start () {
		dataScript = (ibheConnector)GetComponentInChildren<ibheConnector>();
		mcol = GetComponent<MeshCollider>();
	}
	
	// Update is called once per frame
	void Update () {
		ibheVertices = dataScript.vertexData;
		widthP = dataScript.widthPoints;
		//heightP = dataScript.heightPoints;
		
		if(ibheVertices.Length > 0)
		{
			Mesh mesh = GetComponent<MeshFilter>().mesh;
			if(mesh == null)
			{
				Debug.Log("ibheMeshGenerator.cs: Assign a meshfilter to the gameobject");
			}else{
				mesh.Clear();
				//calculate triangles
				if(ibheVertices.Length < 2)
				{
					Debug.Log("ibheMeshGenerator.cs: Less than 3 vertices -> no mesh generation");	
				}else{
					ibheTriangles = calcTris(ibheVertices);
					mesh.vertices = ibheVertices;
					mesh.normals = ibheVertices;
					mesh.triangles = ibheTriangles;
					mesh.RecalculateBounds();
					mcol.sharedMesh = null;
					mcol.sharedMesh = mesh;
				}		
			}
		}
	}
	
	//triangle calculation
	private int[] calcTris(Vector3[] verts)
	{
		int count = verts.Length;
		int triangleCount = (count) * 3;
		int[] triangl = new int[triangleCount];
		Debug.Log(triangleCount);
		int j = 0;
		//int r = 1;
		//
		//
		for(int i = 0; i < count; i++)
		{
				
			if(j+6 < triangleCount)
				{
					triangl[j] = i+1;
					triangl[j+1] = i;
					triangl[j+2] = i+widthP;
					
					triangl[j+3] = i+1;
					triangl[j+4] = i+widthP;
					triangl[j+5] = i+1+widthP;
						
					j+=6;
				}
		
		}
		return triangl;
		/*
		int count = verts.Length;
		int triangleCount = (count - 2) * 3;
		int[] triangl = new int[triangleCount];
		int j = 0;
		for(int i = 0; i < count; i++)
		{
			if((j + 2) < triangleCount)
			{
				if(i%2 == 0)
				{
					triangl[j] = i+2;
					triangl[j+1] = i+1;
					triangl[j+2] = i;
					j+=3;
				}else{
					triangl[j] = i;
					triangl[j+1] = i+1;
					triangl[j+2] = i+2;
					j+=3;
				}
			}
		}
		return triangl;
		*/
	}
}

//
//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//
///* ################################## 
//Generates a mesh from the vertex data
// ################################### */
//public class ibheMeshGenerator : MonoBehaviour {
//	//variables
//	//public GameObject connection;
//	private ibheConnector dataScript;
//	private MeshCollider mcol;
//	private int widthP;
//	private int heightP;
//	private float[] heights;
//	BSplineSurface mySurface;
//	public Vector3[,] resultGrid;
//	public Vector3[,] controlGrid;
//	
//	// Use this for initialization
//	void Start () {
//		dataScript = (ibheConnector)GetComponentInChildren<ibheConnector>();
//		mcol = GetComponent<MeshCollider>();
//		mySurface = new BSplineSurface();
//		mySurface.NI = 16;
//		mySurface.NJ = 12;
//		mySurface.RESOLUTIONI = 52;
//		mySurface.RESOLUTIONJ = 39;
//		mySurface.Init();		
//		mySurface.InitRandomGrid();
//		mySurface.Calculate();
//		resultGrid = mySurface.outputGrid;
//		controlGrid = mySurface.controlGrid;
//		generateMesh();
//		updateMesh();
//	}
//	
//	// Update is called once per frame
//	void Update () {
//		
//		heights = dataScript.heightData;
//		
//		//init control points (random z)
//		Vector3[,] inpGrid = new Vector3[mySurface.NI+1,mySurface.NJ+1];
//		for(int i=0; i<=mySurface.NI; i++)
//		{
//			for(int j=0; j<=mySurface.NJ; j++)
//			{
//				inpGrid[i, j].x = i;
//				inpGrid[i, j].y = j;
//				inpGrid[i, j].z = heights[i*j+j];
//			}
//		}
//		
//		mySurface.InitGrid(inpGrid);
//		
//		float startTime = Time.realtimeSinceStartup;
//			mySurface.Calculate();
//		float endTime = Time.realtimeSinceStartup;
//		float finalTime = endTime - startTime;
//		Debug.Log("Calculation time:" + finalTime);
//		updateMesh();
//	}
//	
//	//generate a Mesh
//	private void generateMesh()
//	{
//		
//		Mesh mesh = GetComponent<MeshFilter>().mesh;
//		
//		int width = mySurface.RESOLUTIONI;
//		int height = mySurface.RESOLUTIONJ;
//		int y = 0;
//		int x = 0;
//	
//		// Build vertices and UVs
//		Vector3[] vertices = new Vector3[height * width];
//		Vector2[] uv = new Vector2[height * width];
//		
//		for (y=0;y<height;y++)
//		{
//			for (x=0;x<width;x++)
//			{
//				vertices[y*width + x] = new Vector3 (x, 0, y);
//				uv[y*width + x] = new Vector2(x, y);
//			}
//		}
//		
//		// Assign them to the mesh
//		mesh.vertices = vertices;
//		mesh.uv = uv;
//	
//		// Build triangle indices: 3 indices into vertex array for each triangle
//		int[] triangles = new int[(height - 1) * (width - 1) * 6];
//		int index = 0;
//		for (y=0;y<height-1;y++)
//		{
//			for (x=0;x<width-1;x++)
//			{
//				// For each grid cell output two triangles
//				triangles[index++] = (y     * width) + x;
//				triangles[index++] = ((y+1) * width) + x;
//				triangles[index++] = (y     * width) + x + 1;
//	
//				triangles[index++] = ((y+1) * width) + x;
//				triangles[index++] = ((y+1) * width) + x + 1;
//				triangles[index++] = (y     * width) + x + 1;
//			}
//		}
//		// And assign them to the mesh
//		mesh.triangles = triangles;
//			
//		// Auto-calculate vertex normals from the mesh
//		mesh.RecalculateNormals();	
//	}
//	
//	private void updateMesh()
//	{
//		Mesh mesh = GetComponent<MeshFilter>().mesh;
//		
//		int width = mySurface.RESOLUTIONI;
//		int height = mySurface.RESOLUTIONJ;
//		int y = 0;
//		int x = 0;
//		
//		// Build vertices and UVs
//		Vector3[] vertices = new Vector3[height * width];
//		
//		
//		for (y=0;y<height;y++)
//		{
//			for (x=0;x<width;x++)
//			{
//				vertices[y*width + x] = resultGrid[x, y];
//			}
//		}
//		mesh.vertices = vertices;
//		mesh.RecalculateNormals();
//		mcol.sharedMesh = null;
//		mcol.sharedMesh = mesh;
//		
//	}
//}
