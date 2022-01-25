using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

public class VideoTooltipWindow : EditorWindow
{
    public string VideoClipFileURI;
    public GameObject tempGO;

    private VideoClip clip;
    private VideoPlayer player;
    private Texture currentRT;

    public static void ShowWindow()
    {
        GetWindow<VideoTooltipWindow>();


    }

    void OnGUI()
    {
        Repaint();

        if (clip == null)
        {
            //clip = Resources.Load<VideoClip>(VideoClipName);

            if (player == null && tempGO != null)
            {
                player = tempGO.AddComponent<VideoPlayer>();
                player.playOnAwake = false;
                player.url = VideoClipFileURI;
                //player.clip = clip;
                player.isLooping = true;

                player.Prepare();

                player.sendFrameReadyEvents = true;
                player.frameReady += Player_frameReady;
                player.Play();
            }

        }

        if(currentRT != null) EditorGUI.DrawPreviewTexture(new Rect(0, 0, position.width, position.height), currentRT);
    }

    void Update()
    {

    }

    private void Player_frameReady(VideoPlayer source, long frameIdx)
    {
        currentRT = source.texture;
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
        if(tempGO != null && player != null) KW_Extensions.SafeDestroy(player);
    }
}
