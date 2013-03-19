using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class BSpline : MonoBehaviour {

	public int n = 2; // Degree of the curve (order?!)	
	public int[] nV; // Node vector
	public List<Vector3> controlPoints;
	public Vector3[] cachedControlPoints; // cached control points
	public List<Vector3> resultPoints;
	
	void Start()
	{
		controlPoints = new List<Vector3>();
		CacheControlPoints();
		nV = new int[cachedControlPoints.Length + 5];
		createNodeVector();	
	}
	// Recursive deBoor algorithm.
	public Vector3 deBoor(int r, int i, float u)
	{
		if(r == 0)
		{
			return cachedControlPoints[i];
		}
		else
		{
			float pre = (u - nV[i + r]) / (nV[i + n + 1] - nV[i + r]); // Precalculation
			return ((deBoor(r - 1, i, u) * (1 - pre)) + (deBoor(r - 1, i + 1, u) * (pre)));
		}
	}
	//internal knots
	public void createNodeVector()
	{
		int knoten = 0;
		for(int i = 0; i < (n + cachedControlPoints.Length + 1); i++) // n+m+1 = nr of nodes
		{
			if(i > n)
			{
				if(i <= cachedControlPoints.Length)
				{
					nV[i] = ++knoten;
				}
				else
				{
					nV[i] = knoten;
				}
			}
			else {
				nV[i] = knoten;
			}
		}
	}	
	private void CacheControlPoints()
	{	
		cachedControlPoints = new Vector3[controlPoints.Count];
		int i = 0;
		foreach(Vector3 cp in controlPoints)
		{
			cachedControlPoints[i++] = cp;	
		}
	}	
	public void AddControlPoint(Vector3 cp)
	{
		controlPoints.Add(cp);		
	}
	
	void Update()
	{
		
		if(controlPoints.Count <= 0) return;
		// Cached the control points
		CacheControlPoints();
		if(cachedControlPoints.Length <= 0) return;
		
		// Initialize node vector.
		nV = new int[cachedControlPoints.Length + n + 1];
		createNodeVector();
		
		// Draw the bspline lines
		
		Vector3 start = cachedControlPoints[0];
		Vector3 end = Vector3.zero;
		resultPoints.Clear();
		for(float i = 0.0f; i < nV[n + cachedControlPoints.Length]; i += 0.1f)
		{
			for(int j = 0; j < cachedControlPoints.Length; j++)
			{
				if(i >= j)
				{
					end = deBoor(n, j, i);
				}
			}	
			Debug.DrawLine(start, end, Color.red);
			resultPoints.Add(end);
			start = end;			
		}
		
		start = cachedControlPoints[0];
		for(int j = 0; j < cachedControlPoints.Length; j++)
		{	
			end = cachedControlPoints[j];
			Debug.DrawLine(start, end, Color.blue);
			start = end;
		}
	}	
}
