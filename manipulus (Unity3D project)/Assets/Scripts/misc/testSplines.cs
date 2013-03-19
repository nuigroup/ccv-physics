using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class testSplines : MonoBehaviour {

	BSplineSurface mySurface;
	public Vector3[,] resultGrid;
	public Vector3[,] controlGrid;
	
	// Use this for initialization
	void Start () {
		mySurface = new BSplineSurface();
		mySurface.NI = 16;
		mySurface.NJ = 12;
		mySurface.RESOLUTIONI = 32;
		mySurface.RESOLUTIONJ = 24;
		mySurface.Init();		
		mySurface.InitRandomGrid();
		mySurface.Calculate();
		resultGrid = mySurface.outputGrid;
		controlGrid = mySurface.controlGrid;
		generateMesh();
		updateMesh();
	}
	
	// Update is called once per frame
	void Update () {
		int j = 0;
		//CONTROL DRAWS
		for(int i=0; i<mySurface.NI; i++)
		{
			for(j=0; j<mySurface.NJ; j++)
			{
				Debug.DrawLine(controlGrid[i, j], controlGrid[i+1, j], Color.red);
				Debug.DrawLine(controlGrid[i, j], controlGrid[i, j+1], Color.red);	
			}
			Debug.DrawLine(controlGrid[i, j], controlGrid[i+1, j], Color.red);
		}
		for(int i=0; i<mySurface.NJ; i++)
		{
			Debug.DrawLine(controlGrid[mySurface.NI, i], controlGrid[mySurface.NI, i+1], Color.red);	
		}
		
		//init control points (random z)
		Vector3[,] inpGrid = new Vector3[mySurface.NI+1,mySurface.NJ+1];
		for(int i=0; i<=mySurface.NI; i++)
		{
			for(j=0; j<=mySurface.NJ; j++)
			{
				inpGrid[i, j].x = i;
				inpGrid[i, j].y = j;
				inpGrid[i, j].z = Random.value;
			}
		}
		
		mySurface.InitGrid(inpGrid);
		
		float startTime = Time.realtimeSinceStartup;
			mySurface.Calculate();
		float endTime = Time.realtimeSinceStartup;
		float finalTime = endTime - startTime;
		Debug.Log("Calculation time:" + finalTime);
		updateMesh();
	}
	
	//generate a Mesh
	private void generateMesh()
	{
		
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		
		int width = mySurface.RESOLUTIONI;
		int height = mySurface.RESOLUTIONJ;
		int y = 0;
		int x = 0;
	
		// Build vertices and UVs
		Vector3[] vertices = new Vector3[height * width];
		Vector2[] uv = new Vector2[height * width];
		
		for (y=0;y<height;y++)
		{
			for (x=0;x<width;x++)
			{
				vertices[y*width + x] = new Vector3 (x, 0, y);
				uv[y*width + x] = new Vector2(x, y);
			}
		}
		
		// Assign them to the mesh
		mesh.vertices = vertices;
		mesh.uv = uv;
	
		// Build triangle indices: 3 indices into vertex array for each triangle
		int[] triangles = new int[(height - 1) * (width - 1) * 6];
		int index = 0;
		for (y=0;y<height-1;y++)
		{
			for (x=0;x<width-1;x++)
			{
				// For each grid cell output two triangles
				triangles[index++] = (y     * width) + x;
				triangles[index++] = ((y+1) * width) + x;
				triangles[index++] = (y     * width) + x + 1;
	
				triangles[index++] = ((y+1) * width) + x;
				triangles[index++] = ((y+1) * width) + x + 1;
				triangles[index++] = (y     * width) + x + 1;
			}
		}
		// And assign them to the mesh
		mesh.triangles = triangles;
			
		// Auto-calculate vertex normals from the mesh
		mesh.RecalculateNormals();	
	}
	
	private void updateMesh()
	{
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		
		int width = mySurface.RESOLUTIONI;
		int height = mySurface.RESOLUTIONJ;
		int y = 0;
		int x = 0;
		
		// Build vertices and UVs
		Vector3[] vertices = new Vector3[height * width];
		
		
		for (y=0;y<height;y++)
		{
			for (x=0;x<width;x++)
			{
				vertices[y*width + x] = resultGrid[x, y];
			}
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		MeshCollider mcol = GetComponent<MeshCollider>();
		mcol.sharedMesh = null;
		mcol.sharedMesh = mesh;
		
	}
}