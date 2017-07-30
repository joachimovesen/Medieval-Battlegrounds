using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicController : MonoBehaviour {

    public AudioSource mainSource, backupSource;
    public AudioClip[] tracks;

    int trackIndex = 0;
    float initialVolume;

    bool fading = false;
    float fadeTime = 6f;
    int currentLoops = 0; // The amount of times the current trakc has looped
    int loopsBeforeChange = 2; // How many times to loop the clip before changing track

    private void Start()
    {
        initialVolume = mainSource.volume;
        trackIndex = Random.Range(0, tracks.Length);

        mainSource.clip = tracks[trackIndex];
        mainSource.Play();
    }

    private void Update()
    {
        if(fading)
        {
            mainSource.volume = Mathf.MoveTowards(mainSource.volume, 0, (initialVolume/fadeTime) * Time.deltaTime);
            if(!mainSource.isPlaying)
            {
                fading = false;
                trackIndex = (trackIndex + 1) % tracks.Length;
                currentLoops = 0;
                mainSource.clip = tracks[trackIndex];
                mainSource.Play();
            }
        }
        else
        {
            mainSource.volume = Mathf.MoveTowards(mainSource.volume, initialVolume, (initialVolume / fadeTime) * Time.deltaTime);

            if(currentLoops == loopsBeforeChange)
            {
                if (mainSource.clip.length <= mainSource.time + fadeTime)
                    fading = true;
            }
            else
            {
                if(!mainSource.isPlaying)
                {
                    currentLoops++;
                    mainSource.Play();
                }
            }
        }
    }

}
