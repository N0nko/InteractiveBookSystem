using Intel.RealSense;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class RsStreamTextureRenderer : MonoBehaviour
{
    private static TextureFormat Convert(Format lrsFormat)
    {
        switch (lrsFormat)
        {
            case Format.Z16: return TextureFormat.R16;
            case Format.Disparity16: return TextureFormat.R16;
            case Format.Rgb8: return TextureFormat.RGB24;
            case Format.Rgba8: return TextureFormat.RGBA32;
            case Format.Bgra8: return TextureFormat.BGRA32;
            case Format.Y8: return TextureFormat.Alpha8;
            case Format.Y16: return TextureFormat.R16;
            case Format.Raw16: return TextureFormat.R16;
            case Format.Raw8: return TextureFormat.Alpha8;
            case Format.Disparity32: return TextureFormat.RFloat;
            case Format.Yuyv:
            case Format.Bgr8:
            case Format.Raw10:
            case Format.Xyz32f:
            case Format.Uyvy:
            case Format.MotionRaw:
            case Format.MotionXyz32f:
            case Format.GpioRaw:
            case Format.Any:
            default:
                throw new ArgumentException(string.Format("librealsense format: {0}, is not supported by Unity", lrsFormat));
        }
    }

    private static int BPP(TextureFormat format)
    {
        switch (format)
        {
            case TextureFormat.ARGB32:
            case TextureFormat.BGRA32:
            case TextureFormat.RGBA32:
                return 32;
            case TextureFormat.RGB24:
                return 24;
            case TextureFormat.R16:
                return 16;
            case TextureFormat.R8:
            case TextureFormat.Alpha8:
                return 8;
            default:
                throw new ArgumentException("unsupported format {0}", format.ToString());

        }
    }

    public RsFrameProvider Source;

    [System.Serializable]
    public class TextureEvent : UnityEvent<Texture> { }

    public Stream _stream;
    public Format _format;
    public int _streamIndex;

    public FilterMode filterMode = FilterMode.Point;

    protected Texture2D texture;
    public bool visualDebug = true;

    [Space]
    public TextureEvent textureBinding;

    FrameQueue q;
    Predicate<Frame> matcher;

    void Start()
    {
        Source.OnStart += OnStartStreaming;
        Source.OnStop += OnStopStreaming;
    }

    void OnDestroy()
    {
        if (texture != null)
        {
            Destroy(texture);
            texture = null;
        }

        if (q != null)
        {
            q.Dispose();
        }
    }

    protected void OnStopStreaming()
    {
        Source.OnNewSample -= OnNewSample;
        if (q != null)
        {
            q.Dispose();
            q = null;
        }
    }

    public void OnStartStreaming(PipelineProfile activeProfile)
    {
        q = new FrameQueue(1);
        matcher = new Predicate<Frame>(Matches);
        Source.OnNewSample += OnNewSample;
    }

    private bool Matches(Frame f)
    {
        using (var p = f.Profile)
            return p.Stream == _stream && p.Format == _format && (p.Index == _streamIndex || _streamIndex == -1);
    }

    void OnNewSample(Frame frame)
    {
        try
        {
            if (frame.IsComposite)
            {
                using (var fs = frame.As<FrameSet>())
                using (var f = fs.FirstOrDefault(matcher))
                {
                    if (f != null)
                        q.Enqueue(f);
                    return;
                }
            }

            if (!matcher(frame))
                return;

            using (frame)
            {
                q.Enqueue(frame);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            // throw;
        }

    }

    bool HasTextureConflict(VideoFrame vf)
    {
        return !texture ||
            texture.width != vf.Width ||
            texture.height != vf.Height ||
            BPP(texture.format) != vf.BitsPerPixel;
    }

    protected void LateUpdate()
    {
        if (q != null)
        {
            VideoFrame frame;
            if (q.PollForFrame<VideoFrame>(out frame))
                using (frame)
                    ProcessFrame(frame);
        }
    }

    private void ProcessFrame(VideoFrame frame)
    {
        if (HasTextureConflict(frame))
        {
            if (texture != null)
            {
                Destroy(texture);
            }

            using (var p = frame.Profile)
            {
                bool linear = (QualitySettings.activeColorSpace != ColorSpace.Linear)
                    || (p.Stream != Stream.Color && p.Stream != Stream.Infrared);
                texture = new Texture2D(frame.Width, frame.Height, Convert(p.Format), false, linear)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = filterMode
                };
            }

            textureBinding.Invoke(texture);
        }

        texture.LoadRawTextureData(frame.Data, frame.Stride * frame.Height);
       int tl,tr,br,bl;
        tr = tl = br = bl = 0;
        if(visualDebug)
        ProcessTexture(texture,  pixelValue, out tl, out tr, out br, out bl);
        texture.Apply();
    }
    float pixelValue = 0.55f;

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
            texture.SetPixel(w, h, Color.white);
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
        texture.SetPixel((int)tl.x, (int)tl.y, Color.red);
        texture.SetPixel((int)tr.x, (int)tr.y, Color.blue);
        texture.SetPixel((int)bl.x, (int)bl.y, Color.green);
        texture.SetPixel((int)br.x, (int)br.y, Color.magenta);
    }


    void processTexture()
    {
        List<bool> validPoints = new List<bool>();
        Vector2 tl = new Vector2(float.MaxValue, float.MaxValue), tr = new Vector2(float.MinValue, float.MaxValue), bl = new Vector2(float.MaxValue, float.MinValue), br = new Vector2(float.MinValue, float.MinValue);
        Color pixelColor = Color.black;

        float bias = 0.5f; // Adjust this value to change the weight bias (range: 0-1)

        for (int h = 0; h < texture.height; h++)
        {
            for (int w = 0; w < texture.width; w++)
            {
                pixelColor = texture.GetPixel(w, h);

                if (pixelColor.r > pixelValue || pixelColor.g > pixelValue || pixelColor.b > pixelValue)
                {
                    texture.SetPixel(w, h, Color.white);

                   // int index = w + h * texture.width;

                    if (w + (bias * h) < tl.x + (bias * tl.y))
                    {
                        tl.x = w; tl.y = h;

                    }
                    if (w - (bias * h) > tr.x - (bias * tr.y))
                    {
                        tr.x = w; tr.y = h;

                    }
                    if (w + (bias * (texture.height - h)) < bl.x + (bias * (texture.height - bl.y)))
                    {
                        bl.x = w; bl.y = h;

                    }
                    if (w - (bias * (texture.height - h)) > br.x - (bias * (texture.height - br.y)))
                    {
                        br.x = w; br.y = h;

                    }
                    validPoints.Add(true);
                }
                else
                {
                    validPoints.Add(false);
                   // texture.SetPixel(w, h, Color.black);
                }
            }
        }

        texture.SetPixel((int)tl.x, (int)tl.y, Color.red);
        texture.SetPixel((int)tr.x, (int)tr.y, Color.blue);
        texture.SetPixel((int)bl.x, (int)bl.y, Color.green);
        texture.SetPixel((int)br.x, (int)br.y, Color.magenta);
    }




    List<Vector2> getLargestGroup(List<Vector2> points)
    {
        if (points == null)
            return null;

        Debug.Log("points: " + points.Count);
        List<Vector2> largestGroup = new List<Vector2>();
        List<Vector2> tempList = new List<Vector2>();
        List<List<Vector2>> groups = new List<List<Vector2>>();
        Vector2[] tempArray = new Vector2[points.Count];
        points.CopyTo(tempArray);
        tempList.AddRange(tempArray);



        while (tempList[0] == Vector2.zero && tempList.Count > 1)
        {

            tempList.RemoveAt(0);
        }
        if (tempList.Count < 2)
            return null;




        groups.Add(new List<Vector2>());
        groups[0].Add(tempList[0]);
        tempList.RemoveAt(0);


        while (tempList.Count > 0)
        {
            if (tempList[0] == Vector2.zero)
            {
                tempList.RemoveAt(0);
                continue;
            }
            bool found = false;
            for (int i = 0; i < groups.Count; i++)
            {

                for (int l = 0; l < groups[i].Count; l++)
                {

                    if (isConnected(tempList[0], groups[i][l]))
                    {
                        groups[i].Add(tempList[0]);
                        tempList.RemoveAt(0);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    groups.Add(new List<Vector2>());
                    groups[groups.Count - 1].Add(tempList[0]);
                    tempList.RemoveAt(0);
                }
            }

        }

        foreach (List<Vector2> group in groups)
        {
            if (group.Count > largestGroup.Count)
                largestGroup = group;
        }

        Debug.Log("largest group: " + largestGroup.Count);
        Debug.Log("groups: " + groups.Count);


        int removed = 0;
        for (int i = 0; i < points.Count; i++)
        {
            if (!largestGroup.Contains(points[i]))
            {
                points[i] = Vector2.zero;
                removed++;
            }
        }
        Debug.Log("removed: " + removed);
        Debug.Log("points:" + points.Count);

        return points;
    }
    //return true if two points are connected
    bool isConnected(Vector2 p1, Vector2 p2)
    {
        if (Mathf.Abs(p1.x - p2.x) <= 1 && Mathf.Abs(p1.y - p2.y) <= 1)
            return true;
        return false;
    }




}
