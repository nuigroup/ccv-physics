using UnityEngine;
using System.Collections;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshCollider))]
public class ibheMeshObject : MonoBehaviour {

	public Mesh mesh;
	public long thisID;
	private MeshCollider mcol;
	
	// Use this for initialization
	void Start () {
		mcol = GetComponent<MeshCollider>();
	}
	
	//generate a Mesh
	public void generateMesh(int _width, int _height, float[] _heights)
	{
		
		if(_width < 2 || _height < 2)
			return;
		
		mesh = GetComponent<MeshFilter>().mesh;
		int width = _width;
		int height = _height;
		int y = 0;
		int x = 0;
		
		//200x * 200z * 30y (y = up-axis)
		float realWidth = ((float)width * 40) / 320;
		float realHeight = ((float)height * 30) / 240;
		Vector3 size = new Vector3(realWidth, -4, realHeight);
		//Debug.Log("rW_" + realWidth + "rH_" + realHeight + " vS_" + size);
		// Build vertices
		Vector3[] vertices = new Vector3[height * width];
		Vector3 sizeScale = new Vector3 (size.x / ((float)width - 1.0f), size.y, size.z / ((float)height - 1.0f));
		Vector2[] uv = new Vector2[height*width];
		Vector2 uvScale = new Vector2(1.0F / (width-1), 1.0F / (height - 1));
		//translation
		//Matrix mat = Matrix.Translate(xTrans, -3.0F, yTrans);
		
		int i = 0;		
		for (y=0;y<height;y++)
		{
			for (x=0;x<width;x++)
			{
				Vector3 vertex = new Vector3 (x, _heights[i++]/255.0f , y);
				vertices[y*width + x] = Vector3.Scale(sizeScale, vertex);
				uv[y*width+x] = Vector2.Scale(new Vector2(x, y), uvScale);
			}
		}
		
//		for(y=0; y<vertices.Length; y++)
//		{
//		   vertices[y] = mat.TransformVector( vertices[y] );	
//		}
		
	
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
				
				/*
				 * BACKUP (FACES UP)
				 * 
				triangles[index++] = (y     * width) + x;
				triangles[index++] = ((y+1) * width) + x;
				triangles[index++] = (y     * width) + x + 1;
	
				triangles[index++] = ((y+1) * width) + x;
				triangles[index++] = ((y+1) * width) + x + 1;
				triangles[index++] = (y     * width) + x + 1;
				*/
			}
		}
		
		mesh.Clear();
		// Assign them to the mesh
		mesh.vertices = vertices;
		mesh.uv = uv;
		// And assign them to the mesh
		mesh.triangles = triangles;
		
			
		// Auto-calculate vertex normals from the mesh
		mesh.RecalculateNormals();	
		mcol.sharedMesh = null;
		mcol.sharedMesh = mesh;
	}
}