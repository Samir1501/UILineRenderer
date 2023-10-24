using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public struct CurveHandeSettings
{
    public float gizmoSize;
    
    public Color handleColor;
    public Color connectionPointColor;
    public Color silhouetteColor;
    public Color hoverColor;
    public Color handleLineColor;
    public Color lineColor;

    public CurveHandeSettings(float gizmoSize = 0.1f)
    {
        this.gizmoSize = gizmoSize;
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
    private static Vector2 prevMousePosition;

    private static Vector2 prevPointPosition;
    private static Vector2 tempPoint;
    private static Vector2 tempMin;
    private static Vector2 tempMax;

    private static Vector2 calcPoint;
    private static Vector2 calcMin;
    private static Vector2 calcMax;
    
    private static int _hoverIndex;
    private static int _nearestHandle;

    public static float RadiusHandle(Vector2 position, Vector2 normal, float value, float multiplier,CurveHandeSettings curveHandeSettings)
    {
        Handles.color = curveHandeSettings.handleColor;
        float scaledValue = value * multiplier;
        float newScaledValue = Handles.RadiusHandle(Quaternion.LookRotation(normal), position, scaledValue+multiplier)-multiplier;
        return newScaledValue / multiplier;
    }
    
    public static void CurveHandle(int index, ref Vector2 pointPosition, ref Vector2 min, ref Vector2 max,CurveHandeSettings curveHandeSettings)
    {
        int i = index + 1;
        float size = HandleUtility.GetHandleSize(pointPosition) * curveHandeSettings.gizmoSize;

        _hoverIndex = HandleUtility.nearestControl;

        if (Event.current.type == EventType.Repaint)
        {
            EventType eventType = EventType.Repaint;
            
            if (_nearestHandle == i)
            {
                Handles.color = curveHandeSettings.silhouetteColor;
                Handles.DrawSolidDisc(prevPointPosition, Vector3.forward, size);
                Handles.DrawDottedLine(pointPosition, prevPointPosition, size);
            }
            
            Handles.color = _hoverIndex == i-1 || _nearestHandle == i-1 ? curveHandeSettings.hoverColor : Event.current.control && (_hoverIndex == i+1 || _nearestHandle == i+1) ? curveHandeSettings.hoverColor : curveHandeSettings.handleLineColor;
            Handles.DrawLine(pointPosition, pointPosition + min);
            Handles.color = _hoverIndex == i-1 || _nearestHandle == i-1 ? curveHandeSettings.hoverColor : Event.current.control && (_hoverIndex == i+1 || _nearestHandle == i+1) ? curveHandeSettings.hoverColor : curveHandeSettings.handleColor;
            Handles.DrawSolidDisc(pointPosition+min,Vector3.forward,size);
            Handles.color = Color.clear;
            Handles.CircleHandleCap(i-1, pointPosition + min,Quaternion.identity, size,eventType);
            
            Handles.color = _hoverIndex == i+1 || _nearestHandle == i+1 ? curveHandeSettings.hoverColor : Event.current.control && (_hoverIndex == i-1 || _nearestHandle == i-1) ? curveHandeSettings.hoverColor : curveHandeSettings.handleLineColor;
            Handles.DrawLine(pointPosition, pointPosition + max);
            Handles.color = _hoverIndex == i+1 || _nearestHandle == i+1 ? curveHandeSettings.hoverColor : Event.current.control && (_hoverIndex == i-1 || _nearestHandle == i-1) ? curveHandeSettings.hoverColor : curveHandeSettings.handleColor;
            Handles.DrawSolidDisc(pointPosition+max,Vector3.forward,size);
            Handles.color = Color.clear;
            Handles.CircleHandleCap(i+1, pointPosition + max,Quaternion.identity, size,eventType);
            
            if (!Event.current.control)
            {
                Handles.color = _hoverIndex == i || _nearestHandle == i ? curveHandeSettings.hoverColor : curveHandeSettings.connectionPointColor;
                Handles.DrawSolidDisc(pointPosition, Vector3.forward, size);
                Handles.color = Color.clear;
                Handles.CircleHandleCap(i, pointPosition, Quaternion.identity, size, eventType);
            }
            
            Handles.color = curveHandeSettings.handleColor;
        }
        
        if (Event.current.type == EventType.Layout)
        {
            EventType eventType = EventType.Layout;
            
            if (!Event.current.control)
                Handles.CircleHandleCap(i, pointPosition, Quaternion.identity, size, eventType);

            Handles.DrawLine(pointPosition, pointPosition + min);
            Handles.CircleHandleCap(i-1, pointPosition + min,Quaternion.identity, size,eventType);
            
            Handles.DrawLine(pointPosition, pointPosition + max);
            Handles.CircleHandleCap(i+1, pointPosition + max,Quaternion.identity, size,eventType);
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            _nearestHandle = HandleUtility.nearestControl;
            
            if (_nearestHandle == i)
                tempPoint = prevPointPosition = pointPosition;
            else if (_nearestHandle == i - 1)
                tempMin = min;
            else if(_nearestHandle == i+1)
                tempMax = max;
            
            prevMousePosition = Event.current.mousePosition;
        }
        if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            if (_nearestHandle == i)
                pointPosition = prevPointPosition = calcPoint;
            else if (_nearestHandle == i - 1)
                min = calcMin;
            else if(_nearestHandle == i+1)
                max = calcMax;
            
            prevMousePosition = Vector2.zero;
            _nearestHandle = 0;
        }
        if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {
            if (_nearestHandle == i)
            {
                float moveX = HandleUtility.CalcLineTranslation(prevMousePosition, Event.current.mousePosition,
                    (Vector3)min, Vector3.right);
                float moveY = HandleUtility.CalcLineTranslation(prevMousePosition, Event.current.mousePosition,
                    (Vector3)min, Vector3.up);
                    
                prevMousePosition = Event.current.mousePosition;
                    
                tempPoint.x += moveX;
                tempPoint.y += moveY;

                Vector2 absTemp = new Vector2(Mathf.Abs(prevPointPosition.x - tempPoint.x), Mathf.Abs(prevPointPosition.y - tempPoint.y));
                
                calcPoint.x = Event.current.shift && absTemp.x < absTemp.y ? prevPointPosition.x : tempPoint.x;
                calcPoint.y = Event.current.shift && absTemp.x > absTemp.y ? prevPointPosition.y : tempPoint.y;

                pointPosition = calcPoint;
            }
            else if (_nearestHandle == i-1)
            {
                float moveX = HandleUtility.CalcLineTranslation(prevMousePosition, Event.current.mousePosition,
                    (Vector3)min, Vector3.right);
                float moveY = HandleUtility.CalcLineTranslation(prevMousePosition, Event.current.mousePosition,
                    (Vector3)min, Vector3.up);
                    
                prevMousePosition = Event.current.mousePosition;
                    
                tempMin.x += moveX;
                tempMin.y += moveY;

                Vector2 absTemp = new Vector2(Mathf.Abs(tempMin.x), Mathf.Abs(tempMin.y));
                
                calcMin.x = Event.current.shift && absTemp.x < absTemp.y ? 0 : tempMin.x;
                calcMin.y = Event.current.shift && absTemp.x > absTemp.y ? 0 : tempMin.y;

                min = calcMin;
                
                if(Event.current.control)
                    max = Vector2.Reflect(new Vector2(-min.x, min.y), Vector2.up);
            }
            else if(_nearestHandle == i+1)
            {
                float moveX = HandleUtility.CalcLineTranslation(prevMousePosition, Event.current.mousePosition,
                    (Vector3)max, Vector3.right);
                float moveY = HandleUtility.CalcLineTranslation(prevMousePosition, Event.current.mousePosition,
                    (Vector3)max, Vector3.up);
                    
                prevMousePosition = Event.current.mousePosition;
                    
                tempMax.x += moveX;
                tempMax.y += moveY;
                
                Vector2 absTemp = new Vector2(Mathf.Abs(tempMax.x), Mathf.Abs(tempMax.y));

                calcMax.x = Event.current.shift && absTemp.x < absTemp.y ? 0 : tempMax.x;
                calcMax.y = Event.current.shift && absTemp.x > absTemp.y ? 0 : tempMax.y;

                max = calcMax;
                
                if(Event.current.control)
                    min = Vector2.Reflect(new Vector2(-max.x, max.y), Vector2.up);
            }
        }
    }
}
