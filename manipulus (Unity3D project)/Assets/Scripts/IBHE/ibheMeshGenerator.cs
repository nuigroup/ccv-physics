using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* ################################## 
Generates a mesh from the vertex data
 ################################### */
public class ibheMeshGenerator : MonoBehaviour {
	//variables
	//public GameObject connection;
	private ibheConnector dataScript;
	private MeshCollider mcol;
	public int widthP;
	public int heightP;
	public bool updateShit = false;
	private float[] heights;
	
	// Use this for initialization
	void Start () {
		dataScript = (ibheConnector)GetComponentInChildren<ibheConnector>();
		mcol = GetComponent<MeshCollider>();
		generateMesh();
	}
	
	// Update is called once per frame
	void Update () {
		
		heights = dataScript.heightData;
		widthP = dataScript.widthPoints;
		heightP = dataScript.heightPoints;
		
		if(updateShit)
		{
			float startTime = Time.realtimeSinceStartup;
			generateMesh();
			float endTime = Time.realtimeSinceStartup;
			float finalTime = endTime - startTime;
			Debug.Log("Calculation time:" + finalTime);		
		}
	}

	//generate a Mesh
	private void generateMesh()
	{
		
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		
		int width = 64;
		int height = 48;
		int y = 0;
		int x = 0;
		//200x * 200z * 30y (y = up-axis)
		Vector3 size = new Vector3(13.3f, 2, 10.0f);
	
		// Build vertices and UVs
		Vector3[] vertices = new Vector3[height * width];
		Vector2[] uv = new Vector2[height * width];
		
		
		Vector2 uvScale = new Vector2 (1.0f / ((float)width - 1.0f), 1.0f / ((float)height - 1.0f));
		Vector3 sizeScale = new Vector3 (size.x / ((float)width - 1.0f), size.y, size.z / ((float)height - 1.0f));
		
		for (y=0;y<height;y++)
		{
			for (x=0;x<width;x++)
			{
				//var pixelHeight = heightMap.GetPixel(x, y).grayscale;
				//vertices[y*width + x] = new Vector3 (x, 0, y);
				//uv[y*width + x] = new Vector2(x, y);
				Vector3 vertex = new Vector3 (x, 0, y);
				vertices[y*width + x] = Vector3.Scale(sizeScale, vertex);
				uv[y*width + x] = Vector2.Scale(new Vector2 (x, y), uvScale);
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
		mcol.sharedMesh = null;
		mcol.sharedMesh = mesh;
	}
	
	private void updateMesh()
	{
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		
		int width = widthP;
		int height = heightP;
		int y = 0;
		int x = 0;
		int j = 0;
		Vector3 size = new Vector3(13.3f, 2, 10.0f);
		
		// Build vertices and UVs
		Vector3[] vertices = new Vector3[height * width];
		Vector3 sizeScale = new Vector3 (size.x / ((float)width - 1.0f), size.y, size.z / ((float)height - 1.0f));
		
		for (y=0;y<height;y++)
		{
			for (x=0;x<width;x++)
			{
				Vector3 vertex = new Vector3 (x, heights[j], y);
				vertices[y*width + x] = Vector3.Scale(sizeScale, vertex);
				j++;
			}
		}
		mesh.vertices = vertices;
		//mesh.RecalculateNormals();
		
		mcol.sharedMesh = null;
		mcol.sharedMesh = mesh;
	}
}
