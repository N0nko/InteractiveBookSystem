using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediaManager : MonoBehaviour
{
    public MediaPlayer mediaPlayer;
    // Start is called before the first frame update
    void Awake()
    {
        Init(); 
    }
    private void Init()
    {
        mediaPlayer = GetComponent<MediaPlayer>();
    }

    private void OnEnable()
    {
        if (mediaPlayer == null)
            Init();
        else {
            mediaPlayer.Rewind(false);
            mediaPlayer.Play();
        }
    }
    void Update()
    {
        
    }
}
