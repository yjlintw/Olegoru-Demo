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
	private Dictionary<int, FingerUI> fingerPoints;
	private Dictionary<int, MarkerUI> markerPoints;
	float lastUpdateTime;
	float CLEARTIME = 0.5f;

	
	// Use this for initialization
	void Start () {
		fingerPoints = new Dictionary<int, FingerUI>();
		markerPoints = new Dictionary<int, MarkerUI>();
		lastUpdateTime = Time.time;
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
				clearAllFingers();
			}

			if (jo["markers"] != null) {
				updateMarkers((JSONArray)jo["markers"]);
			} else {
				clearAllMarkers();
			}
			lastUpdateTime = Time.time;
		}

		if (Time.time - lastUpdateTime > CLEARTIME) {
			clearAllMarkers();
			clearAllFingers();
		}
	}
	void updateFingers(JSONClass fingers) {
		foreach (string k in fingers.keys) {
			int id = fingers[k]["id"].AsInt;
			float x = fingers[k]["x"].AsFloat;
			float y = 1080 - fingers[k]["y"].AsFloat;
			int action = fingers[k]["action"].AsInt;
			// Debug.Log( "Finger " + id + ",  Action: " + action);
			Finger finger = new Finger(id, x, y);
			FingerUI fingerUI;
			RectTransform t;
			switch (action) {
				case 1:
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
					fingerPoints[id] = fingerUI;
					break;
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
				GameObject.DestroyImmediate(fingerUI.gameObject);
				fingerPoints.Remove(key);
			}
		}
	}
	void updateMarkers(JSONArray markers) {
		for (int i = 0; i < markers.Count; i++) {
			// Debug.Log("Marker " + markers[i]["id"].AsInt);
			int id = markers[i]["id"].AsInt;
			float x = markers[i]["center"]["x"].AsFloat;
			float y = 1080 - markers[i]["center"]["y"].AsFloat;
			float angle = markers[i]["angle"].AsFloat;
			Marker marker = new Marker(id, x, y, angle);
			MarkerUI markerUI;
			if (markerPoints.ContainsKey(id)) {
				markerUI = markerPoints[id];
			} else {
				markerUI = GameObject.Instantiate<MarkerUI>(markerPrefab);
				markerUI.transform.SetParent(canvas.transform, false);
				markerUI.SetMarker(marker);
				markerPoints[id] = markerUI;
			}
			RectTransform t = markerUI.GetComponent<RectTransform>();
			t.anchoredPosition = new Vector2(x, y);
			t.eulerAngles = new Vector3(0, 0, angle);
		}

		List<int> keys = new List<int>(markerPoints.Keys);	
		foreach(var key in keys) {
			bool found = false;
			for (int i = 0; i < markers.Count; i++) {
				int id = markers[i]["id"].AsInt;
				if (id == key) {
					found = true;
					continue;
				}
			}
			if (!found) {
				MarkerUI markerUI = markerPoints[key];
				GameObject.DestroyImmediate(markerUI.gameObject);
				markerPoints.Remove(key);
			}
		}
	}

	void clearAllMarkers() {
		List<int> keys = new List<int>(markerPoints.Keys);	
		foreach(var key in keys) {
			MarkerUI markerUI = markerPoints[key];
			GameObject.DestroyImmediate(markerUI.gameObject);
		}

		markerPoints.Clear();
	}

	void clearAllFingers() {
		List<int> keys = new List<int>(fingerPoints.Keys);	
		foreach(var key in keys) {
			FingerUI fingerUI = fingerPoints[key];
			GameObject.DestroyImmediate(fingerUI.gameObject);
		}

		fingerPoints.Clear();
	}
}


