using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UILineRenderer))]
public class UILineRendererEditor : Editor
{
    private static UILineSettings uiLineSettings;
       
    private int _hoverIndex;
    private int _nearestHandle;
    private Canvas canvas;
    private Vector2 mousePosition => MouseUtility.GetMouseCanvasPosition(canvas);
    
    private int selectedIndex;
    private Vector2 selectedPosition;
    private SerializedProperty pointList;
    private SerializedProperty points;
    private SerializedProperty thickness;
    private SerializedProperty iterations;

    private void OnEnable()
    {
        Tools.current = Tool.None;
        uiLineSettings = UILineSettings.uiLineSettings;
    }
    
    private void OnSceneGUI()
    {
        UILineRenderer uiLineRenderer = (UILineRenderer) serializedObject.targetObject;
        canvas = uiLineRenderer.GetComponentInParent<Canvas>();
        Handles.matrix = uiLineRenderer.transform.localToWorldMatrix;
        serializedObject.Update();

        Handles.color = uiLineSettings.curveHandeSettings.handleColor;
        pointList = serializedObject.FindProperty("pointList");
        points = serializedObject.FindProperty("points");
        thickness = serializedObject.FindProperty("thickness");
        iterations = serializedObject.FindProperty("iterations");
       
        
        if (Event.current.type == EventType.MouseMove) Repaint();
        
        LineHandle();
        
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
            
            CustomHandles.CurveHandle(i+1,ref pointPosition,ref min,ref max,uiLineSettings.curveHandeSettings);

            position.vector2Value = pointPosition;
            curvePointMin.vector2Value = min;
            curvePointMax.vector2Value = max;

            Vector2 prev = uiLineRenderer.points[uiLineRenderer.points.Length > 0 && t * uiLineRenderer.iterations - 1 > 0 && t * uiLineRenderer.iterations - 1 < uiLineRenderer.points.Length ? t * uiLineRenderer.iterations - 1 : 0];
            Vector2 next = uiLineRenderer.points[t * uiLineRenderer.iterations < uiLineRenderer.points.Length-2 ? t * uiLineRenderer.iterations + 1 : ^1];
            Vector2 normal = (next - prev).normalized;

            if (Event.current.control)
            {
                pointThickness.floatValue = CustomHandles.RadiusHandle(position.vector2Value, normal, pointThickness.floatValue, thickness.floatValue,uiLineSettings.curveHandeSettings);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void AddPoint()
    {
        if(iterations == null || pointList == null) return;
        int p = selectedIndex / iterations.intValue;
        pointList.InsertArrayElementAtIndex(p);
        SerializedProperty addedPoint = pointList.GetArrayElementAtIndex(p+1);
        addedPoint.FindPropertyRelative("position").vector2Value = selectedPosition;
    }
    
    private void LineHandle()
    {
        bool hoverLine = false;

        for (int j = 1; j < points.arraySize; j++)
        {
            Vector2 prev = points.GetArrayElementAtIndex(j - 1).vector2Value;
            Vector2 current = points.GetArrayElementAtIndex(j).vector2Value;
            
            var distance = HandleUtility.DistancePointLine(mousePosition, prev, current);
            hoverLine = distance < 1f;
            selectedPosition = HandleUtility.ProjectPointLine(mousePosition, prev, current);
            selectedIndex = j;
            if(hoverLine) break;
        }

        if (hoverLine == false)
            selectedPosition = mousePosition;

   
        if (Event.current.type == EventType.MouseDown && Event.current.shift && Event.current.button == 0)
        {
            AddPoint();
            Event.current.Use();
        }

        if (Event.current.shift)
        {
            Handles.DrawSolidDisc(selectedPosition, Vector3.forward, 20f);
            Handles.color = hoverLine ? uiLineSettings.curveHandeSettings.hoverColor : uiLineSettings.curveHandeSettings.lineColor;
            for (int i = 1; i < points.arraySize; i++)
            {
                Vector2 prev = points.GetArrayElementAtIndex(i - 1).vector2Value;
                Vector2 current = points.GetArrayElementAtIndex(i).vector2Value;
                Handles.DrawLine(prev, current, 2);
            }
        }

        if (Event.current.shift && Event.current.type == EventType.Layout)
        {
            Handles.CircleHandleCap(0,selectedPosition,Quaternion.identity, 1,EventType.Layout);
        }
    }
}
