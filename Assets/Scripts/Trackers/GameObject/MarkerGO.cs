using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class MarkerGO : MonoBehaviour {
	public AudioClip[] soundclips;

	private Dictionary<string, int> audioMap = new Dictionary<string, int> {
		{"Lion", 0},
		{"Frog", 1},
		{"Monkey", 2},
		{"Wolf", 3},
		{"Rainforest", 4},
		{"Elephant", 5}
	};

	private int audioIndex = -1;
	private bool isEnabled = true;

	private AudioSource asource;
	private Renderer renderer;
	private ParticleSystem ps;
	public bool highlight;
	private bool setAudioFlag = false;
	public Marker marker {
		set; get;
	}

	void Start() {
		asource = gameObject.GetComponent<AudioSource>();
		renderer = gameObject.GetComponent<Renderer>();
		ps = gameObject.GetComponent<ParticleSystem>();
	}

	void Update() {
		if (!isEnabled) {
			return;
		}
		if (highlight) {
			ps.startColor = new Color32(168, 157, 54, 255);
			renderer.material.color = new Color32(200, 200, 0, 255);
			asource.volume = 0.2f;
			if (setAudioFlag) {
				asource.clip = soundclips[audioIndex];
				if (!asource.isPlaying) {
					asource.Play();
				}
				setAudioFlag = false;
			}
		} else {
			ps.startColor = new Color32(168, 157, 255, 255);
			if (asource.clip != null) {
				asource.volume = 1.0f;
				if (!asource.isPlaying) {
					asource.Play();
				}
				renderer.material.color = new Color32(0, 100, 200, 255);
			} else {
				renderer.material.color = new Color32(255, 255, 255, 255);
				asource.Pause();
			}
		}
	}

	public void setHighlight() {
		highlight = true;
	}

	public void unsetHighlight() {
		highlight = false;
	}

	public void setAudio(string input) {
		audioIndex = audioMap[input];
		setAudioFlag = true; 
	}

	public void setActive(bool value) {
		if (value) {
			isEnabled = true;
			if (asource.clip && !asource.isPlaying) {
				asource.Play();
			}
			ps.enableEmission = true;
			// renderer.enabled = true;
		} else {
			isEnabled = false;
			asource.Pause();
			ps.enableEmission = false;
			// renderer.enabled = false;
		}
	}
}
