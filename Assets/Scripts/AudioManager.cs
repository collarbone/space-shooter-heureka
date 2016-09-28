﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice;

public class AudioManager : Singleton<AudioManager> {

    [SerializeField]
    private GameObject _audioSourcePrefab = null;
	[SerializeField]
	private GameObject _audioSourceTTSPrefab = null;
	[SerializeField]
	private Voice _speakerVoice;

    private List<AudioSource> _currentlyPlaying = new List<AudioSource>();

	public enum AppAudioClip {
		Explosion,
		AcquireWeapon,
		AcquireSpeedup,
		Shoot
	}

    private bool _muted = false;
    public bool Muted {
        set {
            _muted = value;
        }
        get {
            return _muted;
        }
    }

    private string ClipPath(AppAudioClip clip) {

        string path = null;
        if (clip == AppAudioClip.Explosion) {
			path = "Audio/explosion_player";
        }
		else if (clip == AppAudioClip.AcquireWeapon) {
			path = "Audio/object_attached";
		}
		else if (clip == AppAudioClip.AcquireSpeedup) {
			path = "Audio/transition1";
		}
		else if (clip == AppAudioClip.Shoot) {
			path = "Audio/weapon_enemy";
		}
		return path;
    }

    protected AudioManager() {}


    public void playClip(AppAudioClip clip) {
        if (_muted) {
            Debug.Log("AudioManager Muted, not playing anything");
            return;
        }
		Debug.Log("AudioManager play clip " + clip);

        string path = ClipPath(clip);
		Debug.Assert(path != null);

		if (path != null) {
			//AudioClip playMe = (AudioClip)Resources.Load(path, typeof(AudioClip));//Resources.Load(path) as AudioClip;
			AudioClip playMe = Resources.Load<AudioClip>(path);
			Debug.Assert(playMe != null);
			if (playMe != null) {
				Debug.Log("AudioManager clip loaded " + playMe);

				//we have to use separate audiosources per clip, for polyphony
				GameObject audioSource = Instantiate(_audioSourcePrefab) as GameObject;
				audioSource.transform.SetParent(this.transform);
				
				
				AudioSource source = audioSource.GetComponent<AudioSource>();
				Debug.Assert(source != null);
				_currentlyPlaying.Add(source);
				
				source.clip = playMe;
				
				source.Play();//or PlayOneShot
				
				//cleanup after finished
				float timeInSecs = playMe.length;
				Debug.Assert (timeInSecs > 0.0f);
				if (timeInSecs <= 0.0f) {
					//some safety programming. important is that the clip get's cleaned up at some point
					Debug.LogWarning("[AudioManager]: clip length reported " + timeInSecs);
					timeInSecs = 10.0f;
				}
				StartCoroutine(cleanUpFinished(audioSource, timeInSecs));
			}
        }

    }

    private IEnumerator cleanUpFinished(GameObject source, float secs) {
        yield return new WaitForSeconds(secs); 
        Destroy(source);
    }

	public void speak(string speech) {
		bool isNative = false;
		float rate = 1.0f;
		float vol = 1.0f;
		float pitch = 1.0f;
		if (isNative)
		{
			//Speaker.SpeakNative(speech, _speakerVoice, rate, vol, pitch);
		}
		else
		{
			GameObject audioSource = Instantiate(_audioSourceTTSPrefab) as GameObject;
			audioSource.transform.SetParent(this.transform);
			
			
			AudioSource source = audioSource.GetComponent<AudioSource>();

			Speaker.Speak(speech, source, _speakerVoice, true, rate, vol, "", pitch);
		}
	}
}
