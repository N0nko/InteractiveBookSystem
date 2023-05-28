using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekCursor : MonoBehaviour
{
    public GameObject target;
    public RSPointCounter pointCounter;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        target.SetActive(pointCounter.cursorImage.gameObject.activeInHierarchy);
    }
}
