using UnityEngine;
using System.Collections;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshCollider))]
public class ibheBsplineObject : MonoBehaviour {

	public Mesh mesh;
	public long thisID;
	private MeshCollider mcol;
	BSplineSurface surface;
	
	// Use this for initialization
	void Start () {
		mcol = GetComponent<MeshCollider>();
		surface = new BSplineSurface();
	}
	
	//generate a Mesh
	public void generateMesh(int _width, int _height, float[] _heights)
	{
		
		/*
		 * INPUT INTO BSplineSurface
		 */
		
		if(_width < 2 || _height < 2)
			return;
		
		mesh = GetComponent<MeshFilter>().mesh;
		int width = _width;
		int height = _height;
		int y = 0;
		int x = 0;
		
		surface.NI = width;
		surface.NJ = height;
		surface.TI = 2;
		surface.TJ = 2;
		surface.RESOLUTIONI = width * 2;
		surface.RESOLUTIONJ = height * 2;
		surface.Init();
		
		int i = 0;	
		//scaling to match sizes
		float realWidth = ((float)width * 40) / 320;
		float realHeight = ((float)height * 40) / 240;
		Vector3 size = new Vector3(realWidth, -4, realHeight);
		Vector3 sizeScale = new Vector3 (size.x / ((float)width - 1.0f), size.y, size.z / ((float)height - 1.0f));
		
		//creating control net
		Vector3[,] inControlGrid = new Vector3[width+1,height+1];
		//init control points (random z)
		for (y=0;y<height;y++)
		{
			for (x=0;x<width;x++)
			{
				Vector3 vertex = new Vector3 (x, _heights[i++]/255.0f , y);
				inControlGrid[x, y]  = Vector3.Scale(sizeScale, vertex);
			}
		}
		//last point double? (CHECK IT)
		for (x=0;x<width;x++)
		{
			Vector3 lastvertex = new Vector3 (x, 0, y);
			inControlGrid[x,height]  = Vector3.Scale(sizeScale, lastvertex);
		}
		for (y=0;y<height;y++)
		{
			Vector3 lastvertex = new Vector3 (x, 0, y);
			inControlGrid[width,y]  = Vector3.Scale(sizeScale, lastvertex);
		}
		
		surface.controlGrid = inControlGrid;
		surface.Calculate();
		
		/*
		 * OUTPUT AND MESH GENERATION
		 */
		
		// Build vertices and UVs
		width = surface.RESOLUTIONI;
		height = surface.RESOLUTIONJ;
		Vector3[] vertices = new Vector3[height * width];
		Vector3[,] resultGrid = surface.outputGrid;
		
		for (y=0;y<height;y++)
		{
			for (x=0;x<width;x++)
			{
				vertices[y*width + x] = resultGrid[x, y];
			}
		}
		
		// Build triangle indices: 3 indices into vertex array for each triangle
		int[] triangles = new int[(height - 1) * (width - 1) * 6];
		int index = 0;
		for (y=0;y<height-1;y++)
		{
			for (x=0;x<width-1;x++)
			{
				// For each grid cell output two triangles
				triangles[index++] = (y     * width) + x;
				triangles[index++] = (y     * width) + x + 1;
				triangles[index++] = ((y+1) * width) + x;				
	
				triangles[index++] = ((y+1) * width) + x;
				triangles[index++] = (y     * width) + x + 1;
				triangles[index++] = ((y+1) * width) + x + 1;
			}
		}
		mesh.Clear();
		// Assign them to the mesh
		mesh.vertices = vertices;
		// And assign them to the mesh
		mesh.triangles = triangles;
		
			
		// Auto-calculate vertex normals from the mesh
		mesh.RecalculateNormals();	
		mcol.sharedMesh = null;
		mcol.sharedMesh = mesh;
	}
}