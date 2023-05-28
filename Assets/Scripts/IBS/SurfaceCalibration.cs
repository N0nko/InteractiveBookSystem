//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using UnityEngine;

//public class SurfaceCalibration : MonoBehaviour
//{
//    public BoxCollider[] areasOfInterest;
//    public GameObject[] pins;



//    //
//    //Vector3 centerPos;
//    //List<Vector3> sortedVectors = new List<Vector3>();

//    Vector3[] LocalCoordinates(Vector3[] pointsToProcess, Matrix4x4 world, BoxCollider[] areasOfInterest, GameObject[] pins, bool calibrating, List<Vector2> validPoints, Vector2 tl, Vector2 tr, Vector2 bl, Vector2 br, Texture2D uvmap, out Vector3 top, out List<Vector3> diff, out float left, out float right)
//    {
//        List<Vector3> WorldCoordinates = new List<Vector3>();
//        List<Vector3> valid = new List<Vector3>();
//        diff = new List<Vector3>();

//        int count = 0;
//        Vector3 A = Vector2.zero, B = Vector2.zero, C = Vector2.zero;
//        List<float> pinDistance = new List<float>();
//        List<Vector3> bestPins = new List<Vector3>();
//        foreach (GameObject pin in pins)
//        {
//            bestPins.Add(pin.transform.position);
//            pinDistance.Add(Mathf.Infinity);

//        }
       
//        for (int i = 0; i < pointsToProcess.Length; i++)
//        {
//            //calib
//            if (validPoints[i] == Vector2.zero)
//                diff.Add(Vector3.zero);
//            else
//            {
//                Vector3 world_v = world.MultiplyPoint3x4(pointsToProcess[i]);
//                WorldCoordinates.Add(world_v);

//                if (i == (int)(tr.x + tr.y * uvmap.width))
//                    A = world_v;
//                if (i == (int)(tl.x + tl.y * uvmap.width))
//                    B = world_v;
//                if (i == (int)(br.x + br.y * uvmap.width))
//                    C = world_v;


//                //  += world_v;


//                count++;

//                if (world_v == Vector3.zero)
//                    continue;
//                if (calibrating)
//                    for (int x = 0; x < pins.Length; x++)
//                    {
//                        float distance = Vector3.Distance(pins[x].transform.position, world_v);
//                        if (distance < pinDistance[x])
//                        {
//                            bestPins[x] = world_v;
//                            pinDistance[x] = distance;
//                        }


//                    }

//                diff.Add(vertices[i]);

//                if (IsInsideBounds(world_v, areasOfInterest[0]))
//                {

//                    valid.Add(world_v);

//                }

//                if (IsInsideBounds(world_v, areasOfInterest[1]))
//                {
//                    if (WorldToLocal(world_v, areasOfInterest[1].transform.worldToLocalMatrix).x < 0)
//                        left++;
//                    else
//                        right++;
//                }
//            }
//        }
//        if (calibrating == false && valid.Count != 0)
//        {
//            top = valid.OrderBy(x => x.y).First();

//        }

//        Plane plane = new Plane(bestPins[0], bestPins[1], bestPins[2]);

//        //Vector3 n1 = (B - A).normalized;
//        //Vector3 n2 = (C - B).normalized;
//        n = plane.normal;

//        //  centerPos /= count;



//        if (calibrating)
//        {
//            UpdateConstraint(((bestPins[0] + bestPins[2]) / 2) - n * 0.08f);

//            areasOfInterest[0].transform.localScale = new Vector3(Vector3.Distance(pins[0].transform.position, pins[3].transform.position) * 1.05f, Vector3.Distance(pins[0].transform.position, pins[1].transform.position) * 1.05f, 0.06f);

//            for (int i = 0; i < pins.Length; i++)
//            {
//                pins[i].transform.position = bestPins[i];
//                centerPos += bestPins[i];

//            }
//            centerPos /= bestPins.Count;

//        }

//        return WorldCoordinates.ToArray();

//    }
//    static bool IsInsideBounds(Vector3 worldPos, BoxCollider bc)
//    {
//        Vector3 localPos = bc.transform.InverseTransformPoint(worldPos);
//        Vector3 delta = localPos - bc.center + bc.size * 0.5f;
//        return Vector3.Max(Vector3.zero, delta) == Vector3.Min(delta, bc.size);
//    }


//    void UpdateConstraint(Vector3 pos)
//    {
//        areasOfInterest[0].transform.rotation = Quaternion.LookRotation(n);
//        areasOfInterest[0].transform.position = pos;

//    }


//    public void ProvisionFiles()
//    {
//        if (!Directory.Exists(Application.streamingAssetsPath + "/Resources"))
//        {
//            Directory.CreateDirectory(Application.streamingAssetsPath + "/Resources");
//        }
//        if (!File.Exists(Application.streamingAssetsPath + "/Resources/Transform.txt"))
//            File.Create(Application.streamingAssetsPath + "/Resources/Transform.txt");

//        if (!File.Exists(Application.streamingAssetsPath + "/Resources/Rotation.txt"))
//            File.Create(Application.streamingAssetsPath + "/Resources/Rotation.txt");

//        if (!File.Exists(Application.streamingAssetsPath + "/Resources/Scale.txt"))
//            File.Create(Application.streamingAssetsPath + "/Resources/Scale.txt");
//    }

//    //method to save transform.position to a file
//    public void SavePosition()
//    {


//        StreamWriter sw = new StreamWriter(Application.streamingAssetsPath + "/Resources/Transform.txt");
//        string data = Mathf.Round(areasOfInterest[0].transform.localPosition.x * 100f) / 100f + ";" + Mathf.Round(areasOfInterest[0].transform.localPosition.y * 100f) / 100f + ";" + Mathf.Round(areasOfInterest[0].transform.localPosition.z * 100f) / 100f;
//        data = data.Replace(',', '.');
//        sw.WriteLine(data);
//        sw.Close();
//    }
//    //method to load transform.position from a file
//    public void LoadPosition()
//    {
//        StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/Resources/Transform.txt");
//        string line = sr.ReadLine();
//        string[] values = line.Split(';');
//        areasOfInterest[0].transform.localPosition = new Vector3(parseFloat(values[0]), parseFloat(values[1]), parseFloat(values[2]));
//        sr.Close();
//    }
//    //method to save transform.rotation to a file
//    public void SaveRotation()
//    {


//        StreamWriter sw = new StreamWriter(Application.streamingAssetsPath + "/Resources/Rotation.txt");
//        string data = areasOfInterest[0].transform.rotation.x + ";" + areasOfInterest[0].transform.rotation.y + ";" + areasOfInterest[0].transform.rotation.z + ";" + areasOfInterest[0].transform.rotation.w;
//        data = data.Replace(',', '.');
//        sw.WriteLine(data);
//        sw.Close();
//    }
//    //method to load transform.rotation from a file
//    public void LoadRotation()
//    {
//        StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/Resources/Rotation.txt");
//        string line = sr.ReadLine();

//        string[] values = line.Split(';');

//        areasOfInterest[0].transform.rotation = new Quaternion(parseFloat(values[0]), parseFloat(values[1]), parseFloat(values[2]), parseFloat(values[3]));
//        sr.Close();
//    }
//    //method to save transform.scale to a file
//    public void SaveScale()
//    {

//        StreamWriter sw = new StreamWriter(Application.streamingAssetsPath + "/Resources/Scale.txt");
//        string data = areasOfInterest[0].transform.localScale.x + ";" + areasOfInterest[0].transform.localScale.y + ";" + areasOfInterest[0].transform.localScale.z;
//        data = data.Replace(',', '.');
//        sw.WriteLine(data);
//        sw.Close();
//    }
//    //method to load transform.scale from a file
//    public void LoadScale()
//    {
//        StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/Resources/Scale.txt");
//        string line = sr.ReadLine();
//        string[] values = line.Split(';');
//        areasOfInterest[0].transform.localScale = new Vector3(parseFloat(values[0]), parseFloat(values[1]), parseFloat(values[2]));
//        sr.Close();
//    }

//    float parseFloat(string s)
//    {
//        float f = 0;
//        float.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out f);
//        return f;

//    }
//}
