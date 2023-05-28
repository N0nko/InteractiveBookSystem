using System;
using UnityEngine;
using Unity.Collections;
using Intel.RealSense;
using UnityEngine.Rendering;
using UnityEngine.Assertions;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Jobs;

using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.UI;
using System.Collections;
using Game;
using TMPro;
using System.IO;
using Stream = Intel.RealSense.Stream;
using UnityEngine.SceneManagement;
using System.Globalization;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RSPointCounter : MonoBehaviour
{
    public RsFrameProvider Source;
    private Mesh mesh;
    private Texture2D uvmap;
    public int count = 5;
    public BoxCollider[] areasOfInterest;
    public GameObject[] pins;
    [NonSerialized]
    private Vector3[] vertices;
    Matrix4x4 localToWorld;
    FrameQueue q;
    Vector3 n;
    public Material pointCloudTexture;
    public Image cursorImage;

    Vector3[] referenceFrame;
    Coroutine waitForTurn, waitForExpire;
    bool coroutineRunning, cursorOff;
    List<Vector3> diff = new List<Vector3>();
    Vector3 top = Vector3.one * -999;


    bool startReading;

    public Vector2 pointer;
    //public Transform anchor;

    bool controlCursor = true;

    public Transform visualiser;
    [HideInInspector]
    public bool hasCursor;
    Vector2 cursorPos, lastCursorPos;
    
    public ImageController imageController;
    Canvas canvas;
    float width = 3840, height = 2160;
    public float surfaceDistance = 7f;
    public float yBias = 0.8f;
    public CyclePages cyclePages;
    public int left, right, maxLeft, maxRight;

    public TextMeshProUGUI debugLeft, debugRight, anchorLeft, anchorRight;
    public GameObject debugPanel;
    private bool calibrate;
    public float pageTurnThreshold = 150;


    public TextureAnalyzer textureAnalyzer;






    void Start()
    {
        canvas = cyclePages.GetComponent<Canvas>();
        width = canvas.pixelRect.width;
        height = canvas.pixelRect.height;

        // timestamp = UnityEngine.Time.time;
        localToWorld = transform.localToWorldMatrix;
        Source.OnStart += OnStartStreaming;
        Source.OnStop += Dispose;

        LateStart();
        if (Application.isEditor)
            controlCursor = false;

    }
    bool constraintUpdated = false;
    void UpdateConstraint(Vector3 pos)
    {
        areasOfInterest[0].transform.rotation = Quaternion.LookRotation(n);
        areasOfInterest[0].transform.position = pos;
    }


    IEnumerator Recalibrate() {

        ProvisionFiles();
        Debug.LogWarning("startingCalibration");
        //yield return new WaitForSeconds(4f);
        ResetPlane();
        cyclePages.gameObject.SetActive(false);
        yield return new WaitForSeconds(1);

        calibrating = true;
        yield return new WaitForSeconds(2);
        calibrating = false;
        Debug.LogWarning("savingData");
        SaveRotation();
        SavePosition();
        SaveScale();
        cyclePages.gameObject.SetActive(true);

    }
    void LateStart()
    {
       
        if (!File.Exists(Application.streamingAssetsPath + "/Resources/Rotation.txt") || !File.Exists(Application.streamingAssetsPath + "/Resources/Transform.txt"))
        {

            StartCoroutine(Recalibrate());
        }
        else
        {
           // yield return new WaitForSeconds(1);
           // yield return new WaitForSeconds(1);
            LoadRotation();
            LoadPosition();
            LoadScale();
            cyclePages.gameObject.SetActive(true);
        }
       

    }

    private void OnStartStreaming(PipelineProfile obj)
    {
        q = new FrameQueue(1);

        using (var depth = obj.Streams.FirstOrDefault(s => s.Stream == Stream.Depth && s.Format == Format.Z16).As<VideoStreamProfile>())
            ResetMesh(depth.Width, depth.Height);

        Source.OnNewSample += OnNewSample;
    }

    private void ResetMesh(int width, int height)
    {
        Assert.IsTrue(SystemInfo.SupportsTextureFormat(TextureFormat.RGFloat));
        uvmap = new Texture2D(width, height, TextureFormat.RGFloat, false, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
        };
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_UVMap", uvmap);

        if (mesh != null)
            mesh.Clear();
        else
            mesh = new Mesh()
            {
                indexFormat = IndexFormat.UInt32,
            };

        vertices = new Vector3[width * height];

        var indices = new int[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            indices[i] = i;

        mesh.MarkDynamic();
        mesh.vertices = vertices;

        var uvs = new Vector2[width * height];
        Array.Clear(uvs, 0, uvs.Length);
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                uvs[i + j * width].x = i / (float)width;
                uvs[i + j * width].y = j / (float)height;
            }
        }

        mesh.uv = uvs;

        mesh.SetIndices(indices, MeshTopology.Points, 0, false);
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10f);

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void OnDestroy()
    {
        if (q != null)
        {
            q.Dispose();
            q = null;
        }

        if (mesh != null)
            Destroy(null);
    }

    private void Dispose()
    {
        Source.OnNewSample -= OnNewSample;

        if (q != null)
        {
            q.Dispose();
            q = null;
        }
    }

    private void OnNewSample(Frame frame)
    {
        if (q == null)
            return;
        try
        {
            if (frame.IsComposite)
            {
                using (var fs = frame.As<FrameSet>())
                using (var points = fs.FirstOrDefault<Points>(Stream.Depth, Format.Xyz32f))
                {
                    if (points != null)
                    {
                        q.Enqueue(points);
                    }
                }

            }

            if (frame.Is(Extension.Points))
            {
                q.Enqueue(frame);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }



    List<Vector2> avgFrames = new List<Vector2>();


    public bool calibrating;
    float pixelValue = 0.55f;



    float inactivityTimer = 0, inactivityTimerLength = 60 * 4;
    private void Update()
    {
        if (cursorOff)
            if (inactivityTimer < inactivityTimerLength)
                inactivityTimer += Time.deltaTime;
            else
            {
                inactivityTimer = 0;
                cyclePages.GoTo(0);
            }
        else
            inactivityTimer = 0;

        if (!calibrating)
        {
            UpdatePageTurn();
            //   UpdateCursorPosition();
        }
    }
 


    List<bool> validPoints;
    int tl, tr, br, bl;
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleDebugPanel();

        if (validPoints == null || calibrating)
        {
            validPoints = textureAnalyzer.ProcessTexture((Texture2D)pointCloudTexture.mainTexture, pixelValue, out tl, out tr, out br, out bl);
            if (validPoints == null)
            {
                return;
            }
        }

        UpdatePointCloudData();
        UpdateCursorPosition();
        //CleanUpAverageFrames();
    }

    private void ToggleDebugPanel()
    {
        debugPanel.SetActive(!debugPanel.activeSelf);
    }

    Vector3[] worldVectorFrame;
    private void UpdatePointCloudData()
    {
        if (q != null && q.PollForFrame<Points>(out Points points))
        {
            using (points)
            {
                UpdateMeshData(points);
                if(validPoints != null)
                worldVectorFrame = LocalCoordinates(vertices, localToWorld).ToArray();
                //UpdateReferenceFrame(worldVectorFrame);

                
                UpdateMeshVertices(vertices);
            }
        }
    }


    private void UpdateMeshData(Points points)
    {
        if (points.Count != mesh.vertexCount)
        {
            using (var p = points.GetProfile<VideoStreamProfile>())
                ResetMesh(p.Width, p.Height);
        }

        if (points.TextureData != IntPtr.Zero)
        {
            uvmap.LoadRawTextureData(points.TextureData, points.Count * sizeof(float) * 2);
            uvmap.Apply();
        }

        if (points.VertexData != IntPtr.Zero)
        {
            points.CopyVertices(vertices);
        }
    }
    private void UpdateMeshVertices(Vector3[] newVertices)
    {
        //Flip the Y - axis of the mesh vertices
        for (int i = 0; i < newVertices.Length; i++)
        {
            newVertices[i].y = -newVertices[i].y;
        }

        mesh.vertices = newVertices;
        mesh.UploadMeshData(false);
    }
    //private void UpdateReferenceFrame(Vector3[] worldVectorFrame)
    //{
    //    if (referenceFrame == null)
    //    {
    //        referenceFrame = worldVectorFrame;
    //        return;
    //    }
    //}
    private enum PageTurnDirection { None, Left, Right }
    

    private void UpdatePageTurn()
    {
        PageTurnDirection currentDirection = PageTurnDirection.None;
        maxLeft = (int)left;
        maxRight = (int)right;

        debugLeft.text = "Left: " + left;
        debugRight.text = "Right: " + right;

        if (left > pageTurnThreshold/2)
        {
            currentDirection = PageTurnDirection.Left;
        }
        else if (right > pageTurnThreshold/2)
        {
            currentDirection = PageTurnDirection.Right;
        }
        else
        {
            currentDirection = PageTurnDirection.None;
        }

        if (currentDirection != PageTurnDirection.None && !turnCoroutineRunning)
        {
            StartCoroutine(CheckForPageTurn(2, currentDirection));
        }
        left = right = 0;
    }

    private bool turnCoroutineRunning = false;

    private IEnumerator CheckForPageTurn(float waitTime, PageTurnDirection direction)
    {
        turnCoroutineRunning = true;
        float startTime = Time.timeSinceLevelLoad;
     //   Debug.LogWarning("StartTurn " + direction);
        yield return new WaitUntil(() =>
        {
            if (direction == PageTurnDirection.Left && maxRight > pageTurnThreshold)
            {
                cyclePages.Inc(-1);
              //   Debug.LogWarning("LeftTurn");
                return true;
            }
            else if (direction == PageTurnDirection.Right && maxLeft > pageTurnThreshold)
            {
                cyclePages.Inc(1);
             //    Debug.LogWarning("RightTurn");
                return true;
            }
            else if (Time.timeSinceLevelLoad - startTime >= waitTime)
            {
              
                return true;
            }
            //if(maxLeft != 0 || maxRight != 0)
        //    Debug.LogWarning("waitingForTurn " + direction+ ": " + maxLeft + " " + maxRight);
            return false;
        });

        yield return new WaitForSeconds(0.5f);

        direction = PageTurnDirection.None;
        turnCoroutineRunning = false;
    }




    static bool IsInsideBounds(Vector3 worldPos, BoxCollider bc)
    {
        Vector3 localPos = bc.transform.InverseTransformPoint(worldPos);
        Vector3 delta = localPos - bc.center + bc.size * 0.5f;
        return Vector3.Max(Vector3.zero, delta) == Vector3.Min(delta, bc.size);
    }

    private List<Vector3> LocalCoordinates(Vector3[] pointsToProcess, Matrix4x4 world)
    {
        int HIDPointCount = 0;
        List<Vector3> worldCoordinates = new List<Vector3>();
        topPoint = Vector3.zero;
        for (int i = 0; i < pointsToProcess.Length; i++)
        {
            Vector3 point = pointsToProcess[i];
            Vector3 worldPoint = world.MultiplyPoint3x4(point);
            worldCoordinates.Add(worldPoint);

            if (!calibrating && validPoints[i])
            {
                CalculatePageTurn(worldPoint);

                if (IsInsideBounds(worldPoint, areasOfInterest[0].GetComponent<BoxCollider>()))
                {
                    UpdateCursorData(worldPoint);
                    HIDPointCount++;
                }
            }
        }
        cursorOff = HIDPointCount < 20;
             

        if (calibrating && worldVectorFrame != null)
        {
            UpdatePlaneData();
        }

        return worldCoordinates;
    }

    Vector3 topPoint = Vector3.zero;
    //float yBias = 0.8f; // Adjust this value to change the bias strength for the y position
  

    void UpdateCursorData(Vector3 worldPoint)
    {
        float zBias = 1 - yBias;
        // Convert the world point to the local point within areasOfInterest[0]
        Vector3 localPoint = areasOfInterest[0].transform.InverseTransformPoint(worldPoint);

        float currentPointWeight = worldPoint.y * yBias + worldPoint.z * zBias;
        float topPointWeight = topPoint.y * yBias + topPoint.z * zBias;

        if (topPoint == Vector3.zero || currentPointWeight < topPointWeight)
        {
            topPoint = worldPoint;
        }
    }


    private float smoothSpeed = 20f;
    private Vector3 previousCursorPosition;
    private void UpdateCursorPosition()
    {
        cursorImage.gameObject.SetActive(!cursorOff);

        pins[4].transform.position = topPoint;

        // Convert the top point to the local point within areasOfInterest[0]
        Vector3 localTopPoint = areasOfInterest[0].transform.InverseTransformPoint(topPoint);

        // Calculate the aspect ratio of the areasOfInterest[0] GameObject
        float areaAspectRatio = areasOfInterest[0].transform.localScale.x / areasOfInterest[0].transform.localScale.y;

        // Calculate the aspect ratio of the screen
        float screenAspectRatio = width / height;

        // Scale the localTopPoint.x by the aspect ratio difference
        localTopPoint.x *= areaAspectRatio / screenAspectRatio;

        // Calculate the new 2D cursor's position based on the scaled local top point
        Vector3 newCursorPosition = new Vector3(localTopPoint.x * width, -localTopPoint.y * height, 0);

        // Lerp between the previous position and the new position
        cursorImage.transform.localPosition = Vector3.Lerp(previousCursorPosition, newCursorPosition, smoothSpeed * Time.deltaTime);

        // Update the previous cursor position
        previousCursorPosition = cursorImage.transform.localPosition;
    }


    private void CalculatePageTurn(Vector3 worldPoint)
    {
        if (IsInsideBounds(worldPoint, areasOfInterest[1]))
        {
            if (worldPoint.x < 0)
            {
                left++;
            }
            else if (worldPoint.x > 0)
            {
                right++;
            }
        }
    }

    float currentWidth = 0;
    float currentHeight = Mathf.Infinity;
    private void UpdatePlaneData()
    {
        Vector3 newTL = worldVectorFrame[tl];
        Vector3 newTR = worldVectorFrame[tr];
        Vector3 newBL = worldVectorFrame[bl];
        Vector3 newBR = worldVectorFrame[br];

        float diagL = Vector3.Distance(newTL, newBR);
        float diagR = Vector3.Distance(newTR, newBL);

        float maxDistanceThreshold = 0.5f; // You can adjust this value based on your requirements
        if (Vector3.Distance(newTL, pins[0].transform.position) > maxDistanceThreshold ||
            Vector3.Distance(newTR, pins[1].transform.position) > maxDistanceThreshold ||
            Vector3.Distance(newBL, pins[2].transform.position) > maxDistanceThreshold ||
            Vector3.Distance(newBR, pins[3].transform.position) > maxDistanceThreshold)
        {
            ResetPlane();
         }

        // Check if the new arrangement is more square
        if (Mathf.Abs(diagL - diagR) < Mathf.Abs(currentWidth - currentHeight))
        {
            pins[0].transform.position = newTL;
            pins[1].transform.position = newTR;
            pins[2].transform.position = newBL;
            pins[3].transform.position = newBR;

            Plane plane = new Plane(newTR, newBL, newBR);
            n = plane.normal;

            // Calculate the average position of pins[0] and pins[2]
            Vector3 averagePosition = (newTR + newBR) / 2;
            areasOfInterest[0].transform.position = averagePosition - n.normalized / surfaceDistance;

            float width = Vector3.Distance(pins[0].transform.position, pins[1].transform.position);
            float height = Vector3.Distance(pins[1].transform.position, pins[2].transform.position);

            // Apply scale to the areasOfInterest[0] GameObject
            Vector3 scale = new Vector3(width, height, areasOfInterest[0].transform.localScale.z);
            areasOfInterest[0].transform.localScale = scale;

            // Apply rotation based on the plane normal
            areasOfInterest[0].transform.rotation = Quaternion.FromToRotation(Vector3.forward, n);

            // Calculate z rotation based on the top-left and top-right points
            Vector3 topLeftToTopRight = newTR - newTL;
            float zRotation = Mathf.Atan2(topLeftToTopRight.y, topLeftToTopRight.x) * Mathf.Rad2Deg;

            // Flip the rotation if it's more than 90 degrees in either direction
            if (zRotation < -90)
            {
                zRotation += 180;
            }
            else if (zRotation > 90)
            {
                zRotation -= 180;
            }

            // Apply z rotation to the areasOfInterest[0] GameObject
            areasOfInterest[0].transform.Rotate(0, 0, zRotation, Space.Self);

            // Update currentWidth and currentHeight
            currentWidth = diagL;
            currentHeight = diagR;
        }
    }

    private void SetPositionToFarAway(GameObject obj)
    {
        obj.transform.position = Vector3.one * 20000;
    }

    public void ResetPlane()
    {
        currentWidth = 0;
        currentHeight = Mathf.Infinity;

        foreach (GameObject pin in pins)
        {
            SetPositionToFarAway(pin);
        }

        SetPositionToFarAway(areasOfInterest[0].gameObject);
    }






    void ProvisionFiles()
    {
        if (!Directory.Exists(Application.streamingAssetsPath + "/Resources"))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath + "/Resources");
        }
        if (!File.Exists(Application.streamingAssetsPath + "/Resources/Transform.txt"))
            File.Create(Application.streamingAssetsPath + "/Resources/Transform.txt");

        if (!File.Exists(Application.streamingAssetsPath + "/Resources/Rotation.txt"))
            File.Create(Application.streamingAssetsPath + "/Resources/Rotation.txt");

        if (!File.Exists(Application.streamingAssetsPath + "/Resources/Scale.txt"))
            File.Create(Application.streamingAssetsPath + "/Resources/Scale.txt");
    }

    //method to save transform.position to a file
    void SavePosition()
    {


        StreamWriter sw = new StreamWriter(Application.streamingAssetsPath + "/Resources/Transform.txt");
        string data = Mathf.Round(areasOfInterest[0].transform.localPosition.x * 100f) / 100f + ";" + Mathf.Round(areasOfInterest[0].transform.localPosition.y * 100f) / 100f + ";" + Mathf.Round(areasOfInterest[0].transform.localPosition.z * 100f) / 100f;
        data = data.Replace(',', '.');
        sw.WriteLine(data);
        sw.Close();
    }
    //method to load transform.position from a file
    void LoadPosition()
    {
        StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/Resources/Transform.txt");
        string line = sr.ReadLine();
        string[] values = line.Split(';');
        areasOfInterest[0].transform.localPosition = new Vector3(parseFloat(values[0]), parseFloat(values[1]), parseFloat(values[2]));
        sr.Close();
    }
    //method to save transform.rotation to a file
    void SaveRotation()
    {


        StreamWriter sw = new StreamWriter(Application.streamingAssetsPath + "/Resources/Rotation.txt");
        string data = areasOfInterest[0].transform.rotation.x + ";" + areasOfInterest[0].transform.rotation.y + ";" + areasOfInterest[0].transform.rotation.z + ";" + areasOfInterest[0].transform.rotation.w;
        data = data.Replace(',', '.');
        sw.WriteLine(data);
        sw.Close();
    }
    //method to load transform.rotation from a file
    void LoadRotation()
    {
        StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/Resources/Rotation.txt");
        string line = sr.ReadLine();

        string[] values = line.Split(';');

        areasOfInterest[0].transform.rotation = new Quaternion(parseFloat(values[0]), parseFloat(values[1]), parseFloat(values[2]), parseFloat(values[3]));
        sr.Close();
    }
    //method to save transform.scale to a file
    void SaveScale()
    {

        StreamWriter sw = new StreamWriter(Application.streamingAssetsPath + "/Resources/Scale.txt");
        string data = areasOfInterest[0].transform.localScale.x + ";" + areasOfInterest[0].transform.localScale.y + ";" + areasOfInterest[0].transform.localScale.z;
        data = data.Replace(',', '.');
        sw.WriteLine(data);
        sw.Close();
    }
    //method to load transform.scale from a file
    void LoadScale()
    {
        StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/Resources/Scale.txt");
        string line = sr.ReadLine();
        string[] values = line.Split(';');
        areasOfInterest[0].transform.localScale = new Vector3(parseFloat(values[0]), parseFloat(values[1]), parseFloat(values[2]));
        sr.Close();
    }

    public void DeleteData()
    {
        //if (File.Exists(Application.streamingAssetsPath + "/Resources/Transform.txt"))
        //    File.Delete(Application.streamingAssetsPath + "/Resources/Transform.txt");
        //if (File.Exists(Application.streamingAssetsPath + "/Resources/Rotation.txt"))
        //    File.Delete(Application.streamingAssetsPath + "/Resources/Rotation.txt");
        //if (File.Exists(Application.streamingAssetsPath + "/Resources/Scale.txt"))
        //    File.Delete(Application.streamingAssetsPath + "/Resources/Scale.txt");

        //SceneManager.LoadScene(1);

        ToggleDebugPanel();
         currentWidth = 0;
         currentHeight = Mathf.Infinity;
        StartCoroutine(Recalibrate());
    }

    float parseFloat(string s)
    {
        float f = 0;
        float.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out f);
        return f;

    }
    static Vector3[] SortListV3OnX(List<Vector3> data)
    {
        if (data.Count < 2) return data.ToArray();  //Or throw error, or return null

        //data = data.OrderBy(x => x.y).ToList();
        data = data.OrderByDescending(y => y.y).ToList();
        Debug.Log(data[0] + " " + data.Count);

        return data.ToArray();
    }


}
