using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediaController : MonoBehaviour
{
    MediaPlayer mediaPlayer;
    // Start is called before the first frame update
    void Start()
    {
        mediaPlayer = GetComponent<MediaPlayer>();
    }

    public void StopAndRewind()
    {
        Stop();
        Rewind();

    }
     void Stop()
    {
        if (mediaPlayer != null)
        {
            mediaPlayer.Stop();
        }
    }

     void Rewind()
    {
        if (mediaPlayer != null)
        {
            mediaPlayer.Control.SeekToFrame(0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
