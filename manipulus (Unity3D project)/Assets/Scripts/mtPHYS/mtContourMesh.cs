using UnityEngine;
using System.Collections;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshCollider))]
public class mtContourMesh : MonoBehaviour {

	public Mesh mesh;
	public long thisID;
	public bool lineEnabled;
	private MeshCollider mcol;
	
	// Use this for initialization
	void Start () {
		mesh = GetComponent<MeshFilter>().mesh;
		lineEnabled = true;
		mcol = GetComponent<MeshCollider>();
	}
	
	public void recalcMesh(Vector3[] verts, int[] tris) {
		
		mesh.Clear();
		mesh.vertices = verts;
		
		Vector2[] uvs = new Vector2[verts.Length];
		int i = 0;
		while (i < uvs.Length) {
			uvs[i] = new Vector2(verts[i].x, verts[i].z);
			i++;
		}
		mesh.uv = uvs;
		mesh.triangles = tris;
		
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		//dynamic mesh colliders needs to be nulled first (strange unity behaviour)
		mcol.sharedMesh = null;
		mcol.sharedMesh = mesh;
	}
	
	public void drawLine(Vector3[] cords)
	{
		LineRenderer lineRenderer = GetComponent<LineRenderer>();
		int lengthOfLineRenderer = cords.Length;
		lineRenderer.SetVertexCount(lengthOfLineRenderer);
		int i = 0;
		while (i < lengthOfLineRenderer) {
			Vector3 pos = new Vector3(cords[i].x, 0, cords[i].z);
			lineRenderer.SetPosition(i, pos);
			i++;
		}
	}
}