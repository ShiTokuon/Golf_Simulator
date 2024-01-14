using UnityEngine;

namespace Kamgam.Terrain25DLib
{
	public static class IMGUIUtils
    {
        public static void DrawLabel(string text, string tooltip = null, Color? color = null, bool bold = false, bool wordwrap = true, bool richText = true, Texture icon = null, GUIStyle style = null, params GUILayoutOption[] options)
        {
            if (!color.HasValue)
                color = GUI.skin.label.normal.textColor;

            if (style == null)
                style = new GUIStyle(GUI.skin.label);
            if (bold)
                style.fontStyle = FontStyle.Bold;
            else
                style.fontStyle = FontStyle.Normal;

            style.normal.textColor = color.Value;
            style.hover.textColor = color.Value;
            style.wordWrap = wordwrap;
            style.richText = richText;
            style.imagePosition = ImagePosition.ImageLeft;

            var content = new GUIContent(text);
            if (tooltip != null)
                content.tooltip = tooltip;
            if (icon != null)
            {
                GUILayout.Space(16);
                var position = GUILayoutUtility.GetRect(content, style, options);
                GUI.DrawTexture(new Rect(position.x - 16, position.y, 16, 16), icon);
                GUI.Label(position, content, style);
            }
            else
            {
                GUILayout.Label(content, style, options);
            }
        }
    }
}