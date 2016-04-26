using UnityEngine;
using System.Collections;
/*
 * @Author: Patrick Blanchard
 *
 */

public class AudioController : MonoBehaviour {
    /*
     * effect index legend:
     * (0) - EMP
     * (1) - Explosion
     * (2) - Explosion2
     * (3) - Moving
     * (4) - WeaponSound
     */

    //Music & Sounds
    private AudioSource musicPlayer;
    private AudioClip[] music;
    private int prevTrack;
    private int currTrack;

    public static AudioClip[] effect;

    // Use this for initialization
    void Awake () {
        musicPlayer = gameObject.AddComponent<AudioSource>();
        music       = Resources.LoadAll <AudioClip> ("Audio/Music/");
        effect      = Resources.LoadAll <AudioClip> ("Audio/Sounds/");

        if (!musicPlayer.playOnAwake)
        {
            currTrack = Random.Range(0, music.Length);
            prevTrack = currTrack;
            musicPlayer.clip = music[currTrack];
            musicPlayer.Play();
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (!musicPlayer.isPlaying)
        {
            if(prevTrack == currTrack)
            {
                currTrack = Random.Range(0, music.Length);
            }
            else
            {
                musicPlayer.clip = music[currTrack];
                musicPlayer.Play();
            }
            
        }
	}
}
