using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class SurfaceSpline {

	public int n = 2; // Degree
	public int m = 2; // Degree
	private int[] uV; // Node vector
	private int[] vV; // Node vector
	private Vector3[] cachedControlPointsU; // cached control points
	private Vector3[] cachedControlPointsV; // cached control points
	
	public SurfaceSpline()
	{
		
	}
	
	// Recursive deBoor algorithm. U direction
	private Vector3 deBoorU(int r, int i, float u)
	{
		if(r == 0)
		{
			return cachedControlPointsU[i];
		}
		else
		{
			float pre = (u - uV[i + r]) / (uV[i + n + 1] - uV[i + r]); // Precalculation
			return ((deBoorU(r - 1, i, u) * (1 - pre)) + (deBoorU(r - 1, i + 1, u) * (pre)));
		}
	}
	// Recursive deBoor algorithm. V direction
	private Vector3 deBoorV(int r, int i, float u)
	{
		if(r == 0)
		{
			return cachedControlPointsV[i];
		}
		else
		{
			float pre = (u - vV[i + r]) / (vV[i + n + 1] - vV[i + r]); // Precalculation
			return ((deBoorV(r - 1, i, u) * (1 - pre)) + (deBoorV(r - 1, i + 1, u) * (pre)));
		}
	}
	
	//internal knots U direction
	private void createUNodeVector()
	{
		int knoten = 0;
		for(int i = 0; i < (n + cachedControlPointsU.Length + 1); i++) // n+m+1 = nr of nodes
		{
			if(i > n)
			{
				if(i <= cachedControlPointsU.Length)
				{
					uV[i] = ++knoten;
				}
				else
				{
					uV[i] = knoten;
				}
			}
			else {
				uV[i] = knoten;
			}
		}
	}	
	
	//internal knots V direction
	private void createVNodeVector()
	{
		int knoten = 0;
		for(int i = 0; i < (n + cachedControlPointsV.Length + 1); i++) // n+m+1 = nr of nodes
		{
			if(i > n)
			{
				if(i <= cachedControlPointsV.Length)
				{
					vV[i] = ++knoten;
				}
				else
				{
					vV[i] = knoten;
				}
			}
			else {
				vV[i] = knoten;
			}
		}
	}	
	

	
	public Vector3[] Calculate(Vector3[] u, Vector3[] v)
	{
		cachedControlPointsU = u;
		cachedControlPointsV = v; 
		
		if(cachedControlPointsU.Length <= 0 || cachedControlPointsV.Length <= 0) return null;
		if(n > 4 || m > 4) return null;
		
		//Vector3[] resultPoints = new Vector3[(cachedControlPointsV.Length*10)-(n*10)];
		
		// Initialize node vector.
		uV = new int[cachedControlPointsU.Length + 5];
		vV = new int[cachedControlPointsV.Length + 5];
		createUNodeVector();
		createVNodeVector();
		
		Vector3[] resultPoints = new Vector3[(cachedControlPointsU.Length*10)-(n*10)];
		
		int c = 0;
		// Draw the bspline lines
		Vector3 uRes = Vector3.zero;
		Vector3 vRes = Vector3.zero;
		for(float i = 0.0f; i < uV[n + cachedControlPointsU.Length]; i += 0.1f)
		{
			for(int j = 0; j < cachedControlPointsU.Length; j++)
			{
				if(i >= j)
				{
					uRes = deBoorU(n, j, i);
					for(int k=0; k < cachedControlPointsV.Length; k++)
					{
						if(i >= k)
							vRes = deBoorV(m, k, i);
					}
				}
			}	
			resultPoints[c++] = uRes+vRes;			
		}
		
		return resultPoints;
	}	
}
