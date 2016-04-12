using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour {

    //Music & Sounds
    private AudioSource musicPlayer;
    private AudioClip[] music;

    // Use this for initialization
    void Awake () {
        musicPlayer = gameObject.AddComponent<AudioSource>();
        music = Resources.LoadAll <AudioClip> ("Audio/Music/");

        if (!musicPlayer.playOnAwake)
        {
            musicPlayer.clip = music[Random.Range(0, music.Length)];
            musicPlayer.Play();
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (!musicPlayer.isPlaying)
        {
            musicPlayer.clip = music[Random.Range(0, music.Length)];
            musicPlayer.Play();
        }
	}
}
