using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

	public static SoundManager instance;

	public AudioSource audioSource;
	public AudioClip score, buttonClick, jump, dead, kick;	



	void Awake(){
		instance = this;
		audioSource = GetComponent<AudioSource> ();
	}

	public void PlaySound(AudioClip clip){
		audioSource.PlayOneShot(clip);
	}
}
