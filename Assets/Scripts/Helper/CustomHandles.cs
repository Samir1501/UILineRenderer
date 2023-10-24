using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class CurveHandeSettings
{
    [Range(0.01f,10)]
    public float gizmoSize;
    
    public Color handleColor;
    public Color connectionPointColor;
    public Color silhouetteColor;
    public Color hoverColor;
    public Color handleLineColor;
    public Color lineColor;

    public CurveHandeSettings()
    {
        gizmoSize = 0.1f;
        handleColor = new Color(0, 0.6f, 1, 1);
        connectionPointColor = new Color(0.5f, 0.8f, 0, 1);
        silhouetteColor = new Color(1, 0.9f, 0.6f, 0.2f);
        hoverColor = new Color(1, 0.9f, 0.6f, 1);
        handleLineColor = new Color(0, 0, 0, 1);
        lineColor = new Color(0, 0.7f, 1, 1);
    }
}

public static class CustomHandles
{
    private static readonly CurveHandeSettings CurveHandeSettings;
    private static Vector2 _prevMousePosition;
    private static float _gizmoSize;

    private static Vector2 _prevPointPosition;
    private static Vector2 _tempPoint;
    private static Vector2 _tempMin;
    private static Vector2 _tempMax;

    private static Vector2 _calcPoint;
    private static Vector2 _calcMin;
    private static Vector2 _calcMax;
    
    private static int _hoverIndex;
    private static int _nearestHandle;
    
    private static int _selectedIndex;
    private static Vector2 _selectedPosition;

    static CustomHandles()
    {
        CurveHandeSettings = UILineSettings.uiLineSettings.curveHandeSettings;
    }

    private static void CalculateGizmoSize()
    {
        _gizmoSize = HandleUtility.GetHandleSize(Vector3.zero) * CurveHandeSettings.gizmoSize;
    }

    public static float RadiusHandle(Vector2 position, Vector2 normal, float value, float multiplier)
    {
        CalculateGizmoSize();
        Handles.color = CurveHandeSettings.handleColor;
        float scaledValue = value * multiplier;
        float newScaledValue = Handles.RadiusHandle(Quaternion.LookRotation(normal), position, scaledValue+multiplier)-multiplier;
        return newScaledValue / multiplier;
    }
    
    public static void LineHandle(Vector2[] points, Vector2 mousePosition,Action<int,Vector2>[] actions)
    {
        CalculateGizmoSize();
        bool hoverLine = false;

        for (int i = 1; i < points.Length; i++)
        {
            Vector2 prev = points[i - 1];
            Vector2 current = points[i];
            
            var distance = HandleUtility.DistancePointLine(mousePosition, prev, current);
            hoverLine = distance < 1f;
            _selectedPosition = HandleUtility.ProjectPointLine(mousePosition, prev, current);
            _selectedIndex = i;
            if(hoverLine) break;
        }

        if (hoverLine == false)
            _selectedPosition = mousePosition;

   
        if (Event.current.type == EventType.MouseDown && Event.current.shift && Event.current.button == 0)
        {
            foreach (Action<int,Vector2> action in actions)
            {
                action.Invoke(_selectedIndex,_selectedPosition);
            }
            Event.current.Use();
        }

        if (Event.current.shift)
        {
            Handles.DrawSolidDisc(_selectedPosition, Vector3.forward, _gizmoSize);
            Handles.color = hoverLine ? CurveHandeSettings.hoverColor : CurveHandeSettings.lineColor;
            for (int i = 1; i < points.Length; i++)
            {
                Vector2 prev = points[i - 1];
                Vector2 current = points[i];
                Handles.DrawLine(prev, current, 2);
            }
        }

        if (Event.current.shift && Event.current.type == EventType.Layout)
        {
            Handles.CircleHandleCap(0,_selectedPosition,Quaternion.identity, 1,EventType.Layout);
        }
    }
    
    public static void CurveHandle(int index, ref Vector2 pointPosition, ref Vector2 min, ref Vector2 max)
    {
        CalculateGizmoSize();
        int i = index + 1;

        _hoverIndex = HandleUtility.nearestControl;

        if (Event.current.type == EventType.Repaint)
        {
            EventType eventType = EventType.Repaint;
            
            if (_nearestHandle == i)
            {
                Handles.color = CurveHandeSettings.silhouetteColor;
                Handles.DrawSolidDisc(_prevPointPosition, Vector3.forward, _gizmoSize);
                Handles.DrawDottedLine(pointPosition, _prevPointPosition, _gizmoSize);
            }
            
            Handles.color = _hoverIndex == i-1 || _nearestHandle == i-1 ? CurveHandeSettings.hoverColor : Event.current.control && (_hoverIndex == i+1 || _nearestHandle == i+1) ? CurveHandeSettings.hoverColor : CurveHandeSettings.handleLineColor;
            Handles.DrawLine(pointPosition, pointPosition + min);
            Handles.color = _hoverIndex == i-1 || _nearestHandle == i-1 ? CurveHandeSettings.hoverColor : Event.current.control && (_hoverIndex == i+1 || _nearestHandle == i+1) ? CurveHandeSettings.hoverColor : CurveHandeSettings.handleColor;
            Handles.DrawSolidDisc(pointPosition+min,Vector3.forward,_gizmoSize);
            Handles.color = Color.clear;
            Handles.CircleHandleCap(i-1, pointPosition + min,Quaternion.identity, _gizmoSize,eventType);
            
            Handles.color = _hoverIndex == i+1 || _nearestHandle == i+1 ? CurveHandeSettings.hoverColor : Event.current.control && (_hoverIndex == i-1 || _nearestHandle == i-1) ? CurveHandeSettings.hoverColor : CurveHandeSettings.handleLineColor;
            Handles.DrawLine(pointPosition, pointPosition + max);
            Handles.color = _hoverIndex == i+1 || _nearestHandle == i+1 ? CurveHandeSettings.hoverColor : Event.current.control && (_hoverIndex == i-1 || _nearestHandle == i-1) ? CurveHandeSettings.hoverColor : CurveHandeSettings.handleColor;
            Handles.DrawSolidDisc(pointPosition+max,Vector3.forward,_gizmoSize);
            Handles.color = Color.clear;
            Handles.CircleHandleCap(i+1, pointPosition + max,Quaternion.identity, _gizmoSize,eventType);
            
            if (!Event.current.control)
            {
                Handles.color = _hoverIndex == i || _nearestHandle == i ? CurveHandeSettings.hoverColor : CurveHandeSettings.connectionPointColor;
                Handles.DrawSolidDisc(pointPosition, Vector3.forward, _gizmoSize);
                Handles.color = Color.clear;
                Handles.CircleHandleCap(i, pointPosition, Quaternion.identity, _gizmoSize, eventType);
            }
            
            Handles.color = CurveHandeSettings.handleColor;
        }
        
        if (Event.current.type == EventType.Layout)
        {
            EventType eventType = EventType.Layout;
            
            if (!Event.current.control)
                Handles.CircleHandleCap(i, pointPosition, Quaternion.identity, _gizmoSize, eventType);

            Handles.DrawLine(pointPosition, pointPosition + min);
            Handles.CircleHandleCap(i-1, pointPosition + min,Quaternion.identity, _gizmoSize,eventType);
            
            Handles.DrawLine(pointPosition, pointPosition + max);
            Handles.CircleHandleCap(i+1, pointPosition + max,Quaternion.identity, _gizmoSize,eventType);
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            _nearestHandle = HandleUtility.nearestControl;
            
            if (_nearestHandle == i)
                _tempPoint = _prevPointPosition = pointPosition;
            else if (_nearestHandle == i - 1)
                _tempMin = min;
            else if(_nearestHandle == i+1)
                _tempMax = max;
            
            _prevMousePosition = Event.current.mousePosition;
        }
        if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            if (_nearestHandle == i)
                pointPosition = _prevPointPosition = _calcPoint;
            else if (_nearestHandle == i - 1)
                min = _calcMin;
            else if(_nearestHandle == i+1)
                max = _calcMax;
            
            _prevMousePosition = Vector2.zero;
            _nearestHandle = 0;
        }
        if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {
            if (_nearestHandle == i)
            {
                float moveX = HandleUtility.CalcLineTranslation(_prevMousePosition, Event.current.mousePosition,
                    (Vector3)min, Vector3.right);
                float moveY = HandleUtility.CalcLineTranslation(_prevMousePosition, Event.current.mousePosition,
                    (Vector3)min, Vector3.up);
                    
                _prevMousePosition = Event.current.mousePosition;
                    
                _tempPoint.x += moveX;
                _tempPoint.y += moveY;

                Vector2 absTemp = new Vector2(Mathf.Abs(_prevPointPosition.x - _tempPoint.x), Mathf.Abs(_prevPointPosition.y - _tempPoint.y));
                
                _calcPoint.x = Event.current.shift && absTemp.x < absTemp.y ? _prevPointPosition.x : _tempPoint.x;
                _calcPoint.y = Event.current.shift && absTemp.x > absTemp.y ? _prevPointPosition.y : _tempPoint.y;

                pointPosition = _calcPoint;
            }
            else if (_nearestHandle == i-1)
            {
                float moveX = HandleUtility.CalcLineTranslation(_prevMousePosition, Event.current.mousePosition,
                    (Vector3)min, Vector3.right);
                float moveY = HandleUtility.CalcLineTranslation(_prevMousePosition, Event.current.mousePosition,
                    (Vector3)min, Vector3.up);
                    
                _prevMousePosition = Event.current.mousePosition;
                    
                _tempMin.x += moveX;
                _tempMin.y += moveY;

                Vector2 absTemp = new Vector2(Mathf.Abs(_tempMin.x), Mathf.Abs(_tempMin.y));
                
                _calcMin.x = Event.current.shift && absTemp.x < absTemp.y ? 0 : _tempMin.x;
                _calcMin.y = Event.current.shift && absTemp.x > absTemp.y ? 0 : _tempMin.y;

                min = _calcMin;
                
                if(Event.current.control)
                    max = Vector2.Reflect(new Vector2(-min.x, min.y), Vector2.up);
            }
            else if(_nearestHandle == i+1)
            {
                float moveX = HandleUtility.CalcLineTranslation(_prevMousePosition, Event.current.mousePosition,
                    (Vector3)max, Vector3.right);
                float moveY = HandleUtility.CalcLineTranslation(_prevMousePosition, Event.current.mousePosition,
                    (Vector3)max, Vector3.up);
                    
                _prevMousePosition = Event.current.mousePosition;
                    
                _tempMax.x += moveX;
                _tempMax.y += moveY;
                
                Vector2 absTemp = new Vector2(Mathf.Abs(_tempMax.x), Mathf.Abs(_tempMax.y));

                _calcMax.x = Event.current.shift && absTemp.x < absTemp.y ? 0 : _tempMax.x;
                _calcMax.y = Event.current.shift && absTemp.x > absTemp.y ? 0 : _tempMax.y;

                max = _calcMax;
                
                if(Event.current.control)
                    min = Vector2.Reflect(new Vector2(-max.x, max.y), Vector2.up);
            }
        }
    }
}
