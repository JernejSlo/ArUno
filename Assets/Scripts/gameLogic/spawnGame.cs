using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlaneSpawner : MonoBehaviour
{
    public List<ARPlane> detectedPlanes = new List<ARPlane>();

    public delegate void PlanesChangedDelegate();
    public event PlanesChangedDelegate OnPlanesDetected;

    private ARPlaneManager arPlaneManager;

    void Start()
    {
        arPlaneManager = GetComponent<ARPlaneManager>();
        arPlaneManager.planesChanged += OnPlanesChanged;
    }

    void OnDestroy()
    {
        arPlaneManager.planesChanged -= OnPlanesChanged;
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs eventArgs)
    {
        foreach (var plane in eventArgs.added)
        {
            detectedPlanes.Add(plane);
            Debug.Log("Plane added: " + plane.trackableId);
        }

        if (detectedPlanes.Count > 0)
        {
            OnPlanesDetected?.Invoke();
        }
    }
}
