using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class PlaneSpawner : MonoBehaviour
{
    public GameObject objectToSpawn;  // Prefab to spawn
    public GameObject menuPanel;      // Reference to the menu panel

    private ARRaycastManager arRaycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        // Hide the menu panel at the start
        menuPanel.SetActive(true);
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    var hitPose = hits[0].pose;

                    // Hide the menu when a plane is detected and touched
                    menuPanel.SetActive(false);

                    // Spawn the object at the hit position
                    Instantiate(objectToSpawn, hitPose.position, hitPose.rotation);
                }
            }
        }
    }
}
