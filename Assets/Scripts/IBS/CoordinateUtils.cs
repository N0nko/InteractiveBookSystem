//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.NetworkInformation;
//using UnityEngine;

//public class CoordinateUtils : MonoBehaviour 
//{
//    public static Vector3[] LocalCoordinates(Vector3[] pointsToProcess, Matrix4x4 world, BoxCollider[] areasOfInterest, List<Vector2> validPoints, Texture2D uvmap, GameObject[] pins, bool calibrating, Vector2 tr, Vector2 tl, Vector2 br, out Vector3 top)
//    {
//        List<Vector3> WorldCoordinates = new List<Vector3>();
//        List<Vector3> valid = new List<Vector3>();
//        //   top = Vector3.zero;//Vector3.one * -999;
//        valid.Clear();
//        diff.Clear();
//        int count = 0;
//        Vector3 A = Vector2.zero, B = Vector2.zero, C = Vector2.zero;
//        List<float> pinDistance = new List<float>();
//        List<Vector3> bestPins = new List<Vector3>();
//        foreach (GameObject pin in pins)
//        {
//            bestPins.Add(pin.transform.position);
//            pinDistance.Add(Mathf.Infinity);

//        }
//        //if (pointsToProcess.Length != validPoints.Count)
//        //{
//        //    Debug.Log("Frame mismatch: " + pointsToProcess.Length + " " + validPoints.Count);
//        //    return null;
//        //}
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

//    public static Vector3 WorldToLocal(Vector3 world, Matrix4x4 worldToLocal)
//    {
//        return worldToLocal.MultiplyPoint3x4(world);
//    }

//    public static void UpdateConstraint(Vector3 pos, BoxCollider areaOfInterest, Vector3 n)
//    {
//        areaOfInterest.transform.rotation = Quaternion.LookRotation(n);
//        areaOfInterest.transform.position = pos;
//    }

//    static Vector3[] SortListV3OnX(List<Vector3> data)
//    {
//        if (data.Count < 2) return data.ToArray();  //Or throw error, or return null

//        //data = data.OrderBy(x => x.y).ToList();
//        data = data.OrderByDescending(y => y.y).ToList();
//        Debug.Log(data[0] + " " + data.Count);

//        return data.ToArray();
//    }
//}
