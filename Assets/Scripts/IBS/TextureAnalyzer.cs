using System.Collections.Generic;
using UnityEngine;

public class TextureAnalyzer : MonoBehaviour
{
    public List<bool> ProcessTexture(Texture2D texture, float pixelValue, out int topLeft, out int topRight, out int botLeft, out int botRight)
    {
        topLeft = -1;
        topRight = -1;
        botLeft = -1;
        botRight = -1;

        if (texture == null)
        {
            return null;
        }

        List<bool> validPoints = new List<bool>();
        Dictionary<Vector2, List<Vector2>> groups = new Dictionary<Vector2, List<Vector2>>();
        Color pixelColor = Color.black;

        for (int h = 0; h < texture.height; h++)
        {
            for (int w = 0; w < texture.width; w++)
            {
                pixelColor = texture.GetPixel(w, h);
                Vector2 point = new Vector2(w, h);
                bool isValid = pixelColor.r > pixelValue || pixelColor.g > pixelValue || pixelColor.b > pixelValue;
                validPoints.Add(isValid);

                if (isValid)
                {
                    List<Vector2> group = null;
                    foreach (var neighbor in GetNeighbors(point))
                    {
                        if (groups.TryGetValue(neighbor, out var existingGroup))
                        {
                            if (group == null)
                            {
                                group = existingGroup;
                            }
                            else if (group != existingGroup)
                            {
                                group.AddRange(existingGroup);
                                foreach (var p in existingGroup)
                                {
                                    groups[p] = group;
                                }
                            }
                        }
                    }

                    if (group == null)
                    {
                        group = new List<Vector2> { point };
                    }

                    group.Add(point);
                    groups[point] = group;
                }
            }
        }

        List<Vector2> largestGroup = new List<Vector2>();
        foreach (var group in groups.Values)
        {
            if (group.Count > largestGroup.Count)
            {
                largestGroup = group;
            }
        }

        for (int i = 0; i < validPoints.Count; i++)
        {
            int w = i % texture.width;
            int h = i / texture.width;
            Vector2 point = new Vector2(w, h);
            validPoints[i] = largestGroup.Contains(point);
        }

        FindCorners(largestGroup, texture.width, texture.height, out topLeft, out topRight, out botLeft, out botRight);

        return validPoints;
    }

    private IEnumerable<Vector2> GetNeighbors(Vector2 point)
    {
        yield return point + Vector2.up;
        yield return point + Vector2.down;
        yield return point + Vector2.left;
        yield return point + Vector2.right;
    }

    private void FindCorners(List<Vector2> points, int textureWidth, int textureHeight, out int topLeft, out int topRight, out int botLeft, out int botRight)
    {
        topLeft = -1;
        topRight = -1;
        botLeft = -1;
        botRight = -1;

        Vector2 tl = new Vector2(float.MaxValue, float.MaxValue), tr = new Vector2(float.MinValue, float.MaxValue), bl = new Vector2(float.MaxValue, float.MinValue), br = new Vector2(float.MinValue, float.MinValue);
        float bias = 0.5f;

        foreach (Vector2 point in points)
        {
            int h = (int)point.y;
            int w = (int)point.x;

            if (w + (bias * h) < tl.x + (bias * tl.y))
            {
                tl.x = w; tl.y = h;
                topLeft = w + h * textureWidth;
            }
            if (w - (bias * h) > tr.x - (bias * tr.y))
            {
                tr.x = w; tr.y = h;
                topRight = w + h * textureWidth;
            }
            if (w + (bias * (textureHeight - h)) < bl.x + (bias * (textureHeight - bl.y)))
            {
                bl.x = w; bl.y = h;
                botLeft = w + h * textureWidth;
            }
            if (w - (bias * (textureHeight - h)) > br.x - (bias * (textureHeight - br.y)))
            {
                br.x = w; br.y = h;
                botRight = w + h * textureWidth;
            }
        }
    }

}