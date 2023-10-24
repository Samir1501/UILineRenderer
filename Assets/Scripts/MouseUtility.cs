#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class MouseUtility
{
    public static Vector2 GetMouseCanvasPosition(Canvas canvas)
    {
        if (canvas == null) return default;
        Vector2 mp = HandleUtility.GUIPointToScreenPixelCoordinate(Event.current.mousePosition);
        Vector2 size = canvas.renderingDisplaySize;
        mp *= EditorGUIUtility.pixelsPerPoint;
        var camera = SceneView.lastActiveSceneView.camera;
        Ray ray = camera.ScreenPointToRay(new Vector3( mp.x , mp.y , 0 ));
        if( new Plane(Vector3.forward,Vector3.zero).Raycast(ray,out float dist) )
        {
            Vector3 hitPoint = ray.origin + ray.direction * dist;
            hitPoint.x = -(size.x - hitPoint.x) + size.x/2 -0.4f;
            hitPoint.y = -(size.y - hitPoint.y) + size.y/2 -2.5f;
            hitPoint *= 1/canvas.scaleFactor;
            mp = hitPoint;
        }
        return mp;
    }
}
#endif