using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UILineRenderer))]
public class UILineRendererEditor : Editor
{
    private static UILineSettings uiLineSettings;
    private UILineRenderer uiLineRenderer;
       
    private int _hoverIndex;
    private int _nearestHandle;
    private Canvas canvas;
    private Vector2 mousePosition => MouseUtility.GetMouseCanvasPosition(canvas);
    
    private SerializedProperty pointList;
    private SerializedProperty points;
    private SerializedProperty thickness;
    private SerializedProperty iterations;

    private void OnEnable()
    {
        Tools.current = Tool.None;
        uiLineSettings = UILineSettings.uiLineSettings;
        uiLineRenderer = (UILineRenderer) serializedObject.targetObject;
        canvas = uiLineRenderer.GetComponentInParent<Canvas>();
        
        pointList = serializedObject.FindProperty("pointList");
        points = serializedObject.FindProperty("points");
        thickness = serializedObject.FindProperty("thickness");
        iterations = serializedObject.FindProperty("iterations");
    }
    
    private void OnSceneGUI()
    {
        Handles.matrix = uiLineRenderer.transform.localToWorldMatrix;
        serializedObject.Update();

        Handles.color = uiLineSettings.curveHandeSettings.handleColor;

        if (Event.current.type == EventType.MouseMove) Repaint();

        Vector2[] _points = new Vector2[points.arraySize];
        for (int i = 0; i < points.arraySize; i++)
            _points[i] = points.GetArrayElementAtIndex(i).vector2Value;
        CustomHandles.LineHandle(_points,mousePosition,new []{new Action<int,Vector2>(AddPoint)});
        
        for (int i = 0; i < uiLineRenderer.pointList.Count*3; i+=3)
        {
            int t = i / 3;
            
            SerializedProperty point = pointList.GetArrayElementAtIndex(t);
            SerializedProperty position = point.FindPropertyRelative("position");
            SerializedProperty curvePointMin = point.FindPropertyRelative("curvePointMin");
            SerializedProperty curvePointMax = point.FindPropertyRelative("curvePointMax");
            SerializedProperty pointThickness = point.FindPropertyRelative("thickness");

            Vector2 pointPosition = position.vector2Value;
            Vector2 min = curvePointMin.vector2Value;
            Vector2 max = curvePointMax.vector2Value;
            
            CustomHandles.CurveHandle(i+1,ref pointPosition,ref min,ref max);

            position.vector2Value = pointPosition;
            curvePointMin.vector2Value = min;
            curvePointMax.vector2Value = max;

            Vector2 prev = uiLineRenderer.points[uiLineRenderer.points.Length > 0 && t * uiLineRenderer.iterations - 1 > 0 && t * uiLineRenderer.iterations - 1 < uiLineRenderer.points.Length ? t * uiLineRenderer.iterations - 1 : 0];
            Vector2 next = uiLineRenderer.points[t * uiLineRenderer.iterations < uiLineRenderer.points.Length-2 ? t * uiLineRenderer.iterations + 1 : ^1];
            Vector2 normal = (next - prev).normalized;

            if (Event.current.control)
                pointThickness.floatValue = CustomHandles.RadiusHandle(position.vector2Value, normal, pointThickness.floatValue, thickness.floatValue);
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void AddPoint(int selectedIndex, Vector2 selectedPosition)
    {
        if(iterations == null || pointList == null) return;
        int p = selectedIndex / iterations.intValue;
        pointList.InsertArrayElementAtIndex(p);
        SerializedProperty addedPoint = pointList.GetArrayElementAtIndex(p+1);
        addedPoint.FindPropertyRelative("position").vector2Value = selectedPosition;
    }
}
