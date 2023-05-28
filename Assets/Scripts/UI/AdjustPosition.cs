using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AdjustPosition : MonoBehaviour
{
    private bool adjustThis;
    public string prefix;
    
    public bool AdjustThis { get => adjustThis; set => adjustThis = value; }

    // Start is called before the first frame update
    void Start()
    {
        if (!File.Exists(Application.streamingAssetsPath + "/Resources/" + prefix + "Transform.txt"))
        {
            ProvisionFiles();
            Invoke("SavePosition", 2);
        }
        else
            LoadPosition();
    }
    //method to move transform 2d x and y using arrow key inputs up, down, left, right
    public void IncrementPosition(float x, float y, float z)
    {
        transform.localPosition += new Vector3(x, y, z);
        SavePosition();
    }

    void ProvisionFiles()
    {
        if (!Directory.Exists(Application.streamingAssetsPath + "/Resources"))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath + "/Resources");
        }
        if (!File.Exists(Application.streamingAssetsPath + "/Resources/" + prefix + "Transform.txt"))
            File.Create(Application.streamingAssetsPath + "/Resources/" + prefix + "Transform.txt");

    }

    void SavePosition()
    {


        StreamWriter sw = new StreamWriter(Application.streamingAssetsPath + "/Resources/" + prefix + "Transform.txt");
        string data = transform.localPosition.x + ";" + transform.localPosition.y + ";" + transform.localPosition.z;
        data = data.Replace(',', '.');
        sw.WriteLine(data);
        sw.Close();
    }

    void LoadPosition()
    {
        StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/Resources/" + prefix + "Transform.txt");
        string line = sr.ReadLine();
        string[] values = line.Split(';');
        transform.localPosition = new Vector3(parseFloat(values[0]), parseFloat(values[1]), parseFloat(values[2]));
        sr.Close();
    }


    public void DeleteData()
    {
        if (File.Exists(Application.streamingAssetsPath + "/Resources/" + prefix + "Transform.txt"))
            File.Delete(Application.streamingAssetsPath + "/Resources/" + prefix + "Transform.txt");
    }

    float parseFloat(string s)
    {
        float parsedFloat = 0;
        float.TryParse(s, out parsedFloat);
        return parsedFloat;
    }

    // Update is called once per frame
    void Update()
    {

        if (AdjustThis == false)
            return;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            IncrementPosition(.01f, 0, 0);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            IncrementPosition(-.01f, 0, 0);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            IncrementPosition(0, -.01f, 0);

        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            IncrementPosition(0, .01f, 0);

        }
        if (Input.GetKey(KeyCode.Z))
        {
            IncrementPosition(0, 0, -.01f);

        }
        if (Input.GetKey(KeyCode.X))
        {
            IncrementPosition(0, 0, .01f);

        }
 
        


    }
}
