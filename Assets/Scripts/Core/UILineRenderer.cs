using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class UILinePoint
{
    public Vector2 position;
    public Vector2 curvePointMin, curvePointMax;
    public float thickness = 1;

    public UILinePoint(Vector2 position)
    {
        this.position = position;
    }

    public UILinePoint(Vector2 position, Vector2 curvePointMin, Vector2 curvePointMax)
    {
        this.position = position;
        this.curvePointMin = curvePointMin;
        this.curvePointMax = curvePointMax;
    }
}

public class UILineRenderer : Graphic
{
    public List<UILinePoint> pointList = new ();
    [HideInInspector] public Vector2[] points;
    public float thickness;
    [Range(1,100)] public int iterations;
    
    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vh.Clear();
        Render(ref vh,ref vertex);
    }

    void Render(ref VertexHelper vh,ref UIVertex vertex)
    {
        int t = 0;
        points = Line.Create(pointList,iterations);
        for (int i = 1; i < points.Length; i++)
        {
            CreateVertex(ref vh,ref vertex,i-1,true);
            CreateVertex(ref vh,ref vertex,i,false);

            vh.AddTriangle(t, t + 1, t + 2);
            vh.AddTriangle(t, t + 2, t + 3);
            
            t += 4;
        }
    }
    
    void CreateVertex(ref VertexHelper vh,ref UIVertex vertex,int i,bool positive)
    {
        int currentIndex = i;
        int currentPointIndex = i / iterations;
        int lastIndex = i - 1;
        int nextIndex = i + 1;
        
        Vector2 t0 = (points[currentIndex != points.Length-1 ? nextIndex : currentIndex] - points[currentIndex]).normalized;
        Vector2 t1 = (points[lastIndex != -1 ? lastIndex : currentIndex] - points[currentIndex]).normalized;
            
        Vector2 normalized = Vector2.Perpendicular(t0-t1).normalized;
        Vector2 c =
            normalized *
            (
                1 +
                Mathf.SmoothStep(pointList[currentPointIndex].thickness, 0, (float) currentIndex / iterations - currentPointIndex) + 
                Mathf.SmoothStep( 0, pointList[currentPointIndex < pointList.Count-1 ? currentPointIndex+1 : currentPointIndex].thickness, (float) currentIndex / iterations - currentPointIndex) + 
                (currentIndex == 0 || currentIndex == points.Length - 1 
                    ? 0 
                    : Mathf.SmoothStep(0, pointList[currentPointIndex].thickness, (180 - Vector2.Angle(t0, t1)) / 180))) * thickness;

        vertex.position = points[currentIndex] + (positive ? -c : c);
        vh.AddVert(vertex);
        vertex.position = points[currentIndex] + (positive ? c : -c);
        vh.AddVert(vertex);
    }
}
