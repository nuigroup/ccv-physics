using UnityEngine;
using System.Collections;

public class SineSurface : MonoBehaviour {
	
	public float scale = 10.0F;
	public float speed = 1.0F;
	public Vector3 meshSize = new Vector3(13.0F, 1.0F, 10.0F);
	public int widthVerts = 50;
	public int heightVerts = 50;
	private Vector3[] baseHeight;
	
	void  Start() {
		createSurface(widthVerts, heightVerts, meshSize);	
	}
	
	// Update is called once per frame
	void Update () {
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		
		if(baseHeight == null)
			baseHeight = mesh.vertices;
		
		Vector3[] vertices = new Vector3[baseHeight.Length];
		for(int i=0; i < vertices.Length; i++)
		{
			Vector3 vertex = baseHeight[i];
			vertex.y += Mathf.Sin(Time.time * speed + baseHeight[i].x + baseHeight[i].y + baseHeight[i].z) * scale;
			vertices[i] = vertex;
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
	}
	
	void createSurface(int _width, int _height, Vector3 size) {
		
		int width = Mathf.Min(_width, 255);
		int height = Mathf.Min(_height, 255);
		int y = 0;
		int x = 0;
		
		Vector3[] vertices = new Vector3[height * width];
		Vector2[] uv = new Vector2[height*width];
		Vector4[] tangents = new Vector4[height*width];
		
		Vector2 uvScale = new Vector2(1.0F / (width-1), 1.0F / (height - 1));
		Vector3 sizeScale = new Vector3(size.x / (width-1), size.y, size.z / (height-1));
		
		for(y = 0; y < height; y++)
		{
			for(x = 0; x < width; x++)
			{
				Vector3 vertex = new Vector3(x, 0, y);
				vertices[y*width+x] = Vector3.Scale(sizeScale, vertex);
				uv[y*width+x] = Vector2.Scale(new Vector2(x, y), uvScale);
				
				Vector3 vertexL = new Vector3( x-1, 0, y );
				Vector3 vertexR = new Vector3( x+1, 0, y );
				Vector3 tan = Vector3.Scale( sizeScale, vertexR - vertexL ).normalized;
				tangents[y*width+x] = new Vector4( tan.x, tan.y, tan.z, -1.0F);
			}
		}
		
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
		
		// Assign them to the mesh
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		mesh.tangents = tangents;
		
	}
}