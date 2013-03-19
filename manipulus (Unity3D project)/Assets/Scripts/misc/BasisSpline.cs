using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class BasisSpline {

	public int n = 3; // Degree of the curve 
	private int[] nV; // Node vector
	private Vector3[] cachedControlPoints; // cached control points
	
	public BasisSpline()
	{
		
	}
	
	// Recursive deBoor algorithm.
	private Vector3 deBoor(int r, int i, float u)
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
	private void createNodeVector()
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

	
	public Vector3[] Calculate(Vector3[] controlPoints)
	{
		cachedControlPoints = controlPoints;
		
		if(cachedControlPoints.Length <= 0) return null;
		
		if(n > 4) return null;
		
		Vector3[] resultPoints = new Vector3[(cachedControlPoints.Length*10)-(n*10)];
		
		// Initialize node vector.
		nV = new int[cachedControlPoints.Length + 5];
		createNodeVector();
		
		int c = 0;
		// Draw the bspline lines
		Vector3 end = Vector3.zero;
		for(float i = 0.0f; i < nV[n + cachedControlPoints.Length]; i += 0.1f)
		{
			for(int j = 0; j < cachedControlPoints.Length; j++)
			{
				if(i >= j)
				{
					end = deBoor(n, j, i);
				}
			}	
			resultPoints[c++] = end;			
		}
		
		return resultPoints;
	}
	
	public Vector3 CalcSingle(Vector3[] controlPoints, float t)
	{
		cachedControlPoints = controlPoints;
		
		if(cachedControlPoints.Length <= 0) return Vector3.zero;
		
		if(n > 4) return Vector3.zero;
		
		// Initialize node vector.
		nV = new int[cachedControlPoints.Length + 5];
		createNodeVector();
		
		// Draw the bspline lines
		Vector3 end = Vector3.zero;
		for(int j = 0; j < cachedControlPoints.Length; j++)
		{
			if(t >= j)
			{
				Debug.Log(t);
				end += deBoor(n, j, t);
			}
		}	
		
		return end;				
	}
}
