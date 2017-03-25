using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class TrackerHub : MonoBehaviour {
	public UDPReceiver input;
	public Canvas canvas;
	public FingerUI fingerPointPrefab;
	public MarkerUI markerPrefab;
	public MarkerGO markerGoPrefab;
	private Dictionary<int, FingerUI> fingerPoints;
	private Dictionary<int, MarkerUI> markerPoints;

	private Dictionary<int, MarkerGO> markerGOs;

	public MarkerGO activeMarker;

	private bool waitSpeech = false;
	// float lastUpdateTime;
	const float CLEARTIME = 0.5f;

	
	// Use this for initialization
	void Start () {
		fingerPoints = new Dictionary<int, FingerUI>();
		markerPoints = new Dictionary<int, MarkerUI>();
		markerGOs = new Dictionary<int, MarkerGO>();
		// lastUpdateTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		string newMsg = input.getLatestUDPPacket();
		// Debug.Log(newMsg == string.Empty);
		if (newMsg != string.Empty) {
			JSONNode jo = JSON.Parse(newMsg);
			if (jo["fingers"] != null) {
				updateFingers((JSONClass)jo["fingers"]);
			} else {
				// clearAllFingers();
			}

			if (jo["markers"] != null) {
				updateMarkers((JSONClass)jo["markers"]);
			} else {
				// clearAllMarkers();
			}
			// lastUpdateTime = Time.time;
		}

		if (waitSpeech) {
			Camera.main.backgroundColor = new Color32(200, 100, 100, 255);
		} else {
			Camera.main.backgroundColor = new Color32(0, 0, 0, 255);
		}
	}
	void updateFingers(JSONClass fingers) {
		foreach (string k in fingers.keys) {
			int id = fingers[k]["id"].AsInt;
			float x = fingers[k]["x"].AsFloat;
			float y = 900 - fingers[k]["y"].AsFloat;
			int action = fingers[k]["action"].AsInt;
			// Debug.Log( "Finger " + id + ",  Action: " + action);
			Finger finger = new Finger(id, x, y);
			FingerUI fingerUI;
			RectTransform t;
			switch (action) {
				case 1:
				case 2:
					if (fingerPoints.ContainsKey(id)) {
						fingerUI = fingerPoints[id];
					} else {
						fingerUI = GameObject.Instantiate<FingerUI>(fingerPointPrefab);
						fingerUI.transform.SetParent(canvas.transform, false);
						fingerUI.SetFinger(finger);
						fingerPoints[id] = fingerUI;
					}
					t = fingerUI.GetComponent<RectTransform>();
					t.anchoredPosition = new Vector2(x, y);
					break;
				case 3:
					if (fingerPoints.ContainsKey(id)) {
						fingerUI = fingerPoints[id];
						fingerUI.gameObject.SetActive(false);
						GameObject.DestroyImmediate(fingerUI.gameObject);
						fingerPoints.Remove(id);
					}
					break;
			}
		}
		// Debug.Log(fingerPoints.Count);
		List<int> keys = new List<int>(fingerPoints.Keys);	
		foreach(var key in keys) {
			if (fingers[key.ToString()] == null) {
				FingerUI fingerUI = fingerPoints[key];
				fingerUI.gameObject.SetActive(false);
				GameObject.DestroyImmediate(fingerUI.gameObject);
				fingerPoints.Remove(key);
			}
		}
	}

	void updateMarkers(JSONClass markers) {
		foreach (string k in markers.keys) {
			// Debug.Log("Marker " + markers[i]["id"].AsInt);
			int id = markers[k]["id"].AsInt;
			float x = markers[k]["center"]["x"].AsFloat;
			float y = 900 - markers[k]["center"]["y"].AsFloat;
			float angle = markers[k]["angle"].AsFloat;
			int action = markers[k]["action"].AsInt;
			Marker marker = new Marker(id, x, y, angle);
			MarkerUI markerUI;
			MarkerGO markerGO;
			RectTransform t;
			Transform trans;
			switch (action) {
				case 1:
				case 2:
					if (markerPoints.ContainsKey(id)) {
						markerUI = markerPoints[id];
						markerGO = markerGOs[id];
						markerGO.setActive(true);
					} else {
						markerUI = GameObject.Instantiate<MarkerUI>(markerPrefab);
						markerGO = GameObject.Instantiate<MarkerGO>(markerGoPrefab);
						markerUI.transform.SetParent(canvas.transform, false);
						markerGOs[id] = markerGO;
						markerUI.SetMarker(marker);
						markerGO.marker = marker;
						markerPoints[id] = markerUI;
					}
					t = markerUI.GetComponent<RectTransform>();
					t.anchoredPosition = new Vector2(x, y);
					t.eulerAngles = new Vector3(0, 0, angle);
					trans = markerGO.transform;
					Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(x, y, 10));
					trans.position = pos;
					trans.eulerAngles = new Vector3(0, 0, angle);
					break;
				case 3:
					if (markerPoints.ContainsKey(id)) {
						markerUI = markerPoints[id];
						markerGO = markerGOs[id];
						markerUI.gameObject.SetActive(false);
						markerGO.setActive(false);
						// GameObject.DestroyImmediate(markerUI.gameObject);
						// GameObject.DestroyImmediate(markerGO.gameObject);
						// markerPoints.Remove(id);
						// markerGOs.Remove(id);
					}
					break;
				
			}
		}
	}

	void clearAllMarkers() {
		List<int> keys = new List<int>(markerPoints.Keys);	
		foreach(var key in keys) {
			MarkerUI markerUI = markerPoints[key];
			GameObject.DestroyImmediate(markerUI.gameObject);
			MarkerGO markerGO = markerGOs[key];
			GameObject.DestroyImmediate(markerGO.gameObject);
		}

		markerPoints.Clear();
		markerGOs.Clear();
	}

	void clearAllFingers() {
		List<int> keys = new List<int>(fingerPoints.Keys);	
		foreach(var key in keys) {
			FingerUI fingerUI = fingerPoints[key];
			GameObject.DestroyImmediate(fingerUI.gameObject);
		}

		fingerPoints.Clear();
	}

	public void setActiveMarker(int id) {
		MarkerGO go = markerGOs[id];
		if (go != null) {
			activeMarker = go;
			activeMarker.setHighlight();
		}
		// Renderer r = activeMarker.GetComponent<Renderer>();
		// r.material.color = new Color32(200, 200, 0, 255);
	}

	public void removeActiveMarker() {
		if (activeMarker) {
			activeMarker.unsetHighlight();
			activeMarker = null;
		}
	}

	public void setHotword() {
		waitSpeech = true;
	}

	public void unsetHotword() {
		waitSpeech = false;
	}
}


