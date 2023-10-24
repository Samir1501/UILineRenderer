using System.Collections.Generic;
using UnityEngine;

public static class Line
{
    private static Vector2[] points;

    public static Vector2[] Create(List<UILinePoint> pointList, int iterations)
    {
        points = new Vector2[(pointList.Count - 1) * iterations + 1];
        int t = 0;
        for (int p = 0; p < pointList.Count-1; p++)
        {
            for (float i = 0; i < iterations; i++)
            {
                float j = i / iterations;
                if(t < points.Length) points[t] = CalculateCubicBezierPoint(j,
                    pointList[p].position,
                    pointList[p].position + pointList[p].curvePointMax,
                    pointList[p+1].position + pointList[p+1].curvePointMin,
                    pointList[p+1].position);
                t++;
            }
        }
        points[^1] = pointList[^1].position;
        return GetPoints();
    }

    public static Vector2[] GetPoints()
    {
        return points;
    }
    
    private static Vector2 CalculateCubicBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float a = 1 - t;
        float tt = t * t;
        float aa = a * a;
        float aaa = aa * a;
        float ttt = tt * t;
        
        Vector2 p = aaa * p0; 
        p += 3 * aa * t * p1; 
        p += 3 * a * tt * p2; 
        p += ttt * p3; 
        
        return p;
    }
}
