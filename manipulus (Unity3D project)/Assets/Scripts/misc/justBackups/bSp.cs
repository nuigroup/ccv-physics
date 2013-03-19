using UnityEngine;
using System.Collections;

public class bSp : MonoBehaviour {
	public float[] knoten;
	public Vector2[] controlPoints;
	public Vector2[] resultPoints;
	public int order;
	public int degree;
	public bool calculate;
	
	// Use this for initialization
	void Start () {
		order = 4;
		degree = order-1;
		calculate = false;
		knoten = new float[order+degree+1];
		buildKnots(order, degree, knoten);
		controlPoints = new Vector2[degree];
		resultPoints = new Vector2[20];
	}
	
	// Update is called once per frame
	void Update () {
		
		
		if(calculate)
		{
			int cnt = 0;
			for(float i=0; i<=1; i+=0.05f)
			{
				float sumX = 0.0f;
				float sumY = 0.0f;
				for(int k=0; k < degree; k++)
				{	
					float bResult = calcBspline(k, order, (float)i, knoten);
					sumX += controlPoints[k].x * bResult;
					sumY += controlPoints[k].y * bResult;
				}
				
				resultPoints[cnt].x = sumX;
				resultPoints[cnt].y = sumY;
				cnt++;
			}
			//draw control points
			for(int s=0; s+1<controlPoints.Length; s++)
			{
				Debug.DrawLine( new Vector3(controlPoints[s].x, 0, controlPoints[s].y), new Vector3(controlPoints[s+1].x, 0, controlPoints[s+1].y), Color.green);	
			}
			
			//draw results
			for(int j=0; j+1<resultPoints.Length; j++)
			{
				Debug.DrawLine( new Vector3(resultPoints[j].x, 0, resultPoints[j].y), new Vector3(resultPoints[j+1].x, 0, resultPoints[j+1].y), Color.red);	
			}
			
			
		}
	}
	
	public float calcBspline(int k, int m, float t, float[] knot)
	{
		float denom1, denom2, sum = 0.0f;
		
		if(m == 1)
		{
			if(t >= knot[k] && t < knot[k+1])
				return 1;
			else
				return 0;
		}
		
		denom1 = knot[k+m-1]-knot[k];
		if(denom1 != 0.0f)
			sum = (t-knot[k]) * calcBspline(k, m-1, t, knot) / denom1;
		
		denom2 = knot[k+m] - knot[k+1];
		if(denom2 != 0.0f)
			sum += (knot[k+m] - t) * calcBspline(k+1, m-1, t, knot) / denom2;
		return sum;
	}
	
	//build std knot vector for L+1 control points and B-splines of order m
	//uniform spaced
	public void buildKnots(int m, int L, float[] knot)
	{
		int i;
		if(L < (m-1)) return;
		for(i = 0; i <= L+m; i++)
		{
			if(i<m)knot[i]=0.0f;
			else if(i<=L)knot[i]= i-m+1;
			else knot[i] = L-m+2;
		}		
	}
	
	
	public void drawLine(Vector3[] cords)
	{
		LineRenderer lineRenderer = GetComponent<LineRenderer>();
		int lengthOfLineRenderer = cords.Length;
		lineRenderer.SetVertexCount(lengthOfLineRenderer);
		int i = 0;
		while (i < lengthOfLineRenderer) {
			lineRenderer.SetPosition(i, cords[i]);
			i++;
		}
	}
	
	
	
}

/*
			int cnt = 0;
			int cnt2 = 0;
			for(int i=0; i<20; i++)
			{
				xtvals[cnt++] = calcBspline(degree, order, i, knoten);
				ytvals[cnt2++] = i;
			}
			
			Vector3[] lineSeg = new Vector3[20];
			for(int i=0; i<20; i++)
			{
				lineSeg[i] = new Vector3(xtvals[i], 0, ytvals[i]);	
				//Debug.DrawLine( new Vector3(xtvals[i], 0, ytvals[i]), new Vector3(xtvals[i+1], 0, ytvals[i+1]), Color.red);
			}
			drawLine(lineSeg);
			*/