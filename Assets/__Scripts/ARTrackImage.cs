using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTrackImage : MonoBehaviour
{
    private ARTrackedImageManager ARTrackedImageManager;

    private void Awake()
    {
        ARTrackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void OnEnable()
    {
        ARTrackedImageManager.trackedImagesChanged += OnTrackedImageChanged;
    }

    private void OnDisable()
    {
        ARTrackedImageManager.trackedImagesChanged -= OnTrackedImageChanged;
    }

    private void OnTrackedImageChanged(ARTrackedImagesChangedEventArgs obj)
    {
        foreach(var trackedImage in obj.added)
        {
            Debug.Log($"Wykryto obraz: {trackedImage} (added)");
                    Debug.Log($"WYKRYTO obraz {trackedImage.referenceImage.name}, GO: {trackedImage.gameObject.name}, dzieci: {trackedImage.transform.childCount}");
            UpdateObjectPosition(trackedImage);
        }

        foreach (var trackedImage in obj.updated)
        {
            Debug.Log($"Wykryto obraz: {trackedImage} (updated)");
            UpdateObjectPosition(trackedImage);
        }
    }

    private void UpdateObjectPosition(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            Debug.Log($"UpdateObjectPosition: {trackedImage.trackingState} (active)");

            trackedImage.transform.position = trackedImage.transform.position;
            trackedImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log($"UpdateObjectPosition: {trackedImage.trackingState} (NOT active)");

            trackedImage.gameObject.SetActive(false);
        }
    }
}
