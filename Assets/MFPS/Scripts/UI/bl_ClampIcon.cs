using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_ClampIcon : MonoBehaviour
{
    public bool isStatic = true;
    public Texture2D Icon;


    /// <summary>
    /// Should the icon be drawn at a different position relative to the GameObject. Expected coordinates are in world space
    /// </summary>
    public Vector3 Offset;
    private float size = 50;

    /// <summary>
    /// The icon can be tinted with this property
    /// </summary>
    public Color m_Color = Color.white;

    void OnGUI()
    {
        if (isStatic)
        {
            if (!bl_UtilityHelper.GetCursorState)
                return;
        }
        if (Camera.main == null)
            return;

        Vector3 position = transform.position + Offset;
        Plane plane = new Plane(Camera.main.transform.forward, Camera.main.transform.position);

        //If the object is behind the camera, then don't draw it
        if (plane.GetSide(position) == false)
        {
            return;
        }

        //Calculate the 2D position of the position where the icon should be drawn
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(position);

        //The viewportPoint coordinates are between 0 and 1, so we have to convert them into screen space here
        Vector2 drawPosition = new Vector2(viewportPoint.x * Screen.width, Screen.height * (1 - viewportPoint.y));

        float clampBorder = 12;

        //Clamp the position to the edge of the screen in case the icon would be drawn outside the screen
        drawPosition.x = Mathf.Clamp(drawPosition.x, clampBorder, Screen.width - clampBorder);
        drawPosition.y = Mathf.Clamp(drawPosition.y, clampBorder, Screen.height - clampBorder);

        GUI.color = m_Color;
        GUI.DrawTexture(new Rect(drawPosition.x - size * 0.5f, drawPosition.y - size * 0.5f, size, size), Icon);
    }

    public void SetTempIcon(Texture2D icon, float time, int _size)
    {
        Icon = icon;
        Destroy(gameObject, time);
        size = _size;
    }
}