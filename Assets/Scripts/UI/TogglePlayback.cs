using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TogglePlayback : MonoBehaviour
{
   public MediaPlayer mediaPlayer;
    public Image buttonIcon;
    public Sprite play, pause;
    bool playing = true;
    private void Pause()
    {
        mediaPlayer.Pause();
        buttonIcon.sprite = play;
    }
    private void Play()
    {
        mediaPlayer.Play();
        buttonIcon.sprite = pause;
    }
    public void TogglePlay() {
        if (playing)
            Pause();
        else
            Play();
        playing = !playing;
    }
}
