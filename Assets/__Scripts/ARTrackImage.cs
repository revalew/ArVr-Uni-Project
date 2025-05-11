using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTrackImage : MonoBehaviour
{
    [System.Serializable]
    public class ImagePrefabPair
    {
        public string imageName;
        public GameObject prefabToSpawn;
    }

    // List of image name and prefab pairs
    public List<ImagePrefabPair> imagePrefabPairs = new List<ImagePrefabPair>();
    
    // Dictionary to map detected images to instantiated prefabs
    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();
    
    private ARTrackedImageManager trackedImageManager;

    private void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Handle added tracked images
        foreach (var trackedImage in eventArgs.added)
        {
            Debug.Log($"Image detected: {trackedImage.referenceImage.name} (added)");
            UpdateImageTracking(trackedImage);
        }

        // Handle updated tracked images
        foreach (var trackedImage in eventArgs.updated)
        {
            UpdateImageTracking(trackedImage);
        }

        // Handle removed tracked images
        foreach (var trackedImage in eventArgs.removed)
        {
            // Remove the associated prefab
            if (spawnedObjects.TryGetValue(trackedImage.referenceImage.name, out GameObject prefab))
            {
                Destroy(prefab);
                spawnedObjects.Remove(trackedImage.referenceImage.name);
            }
        }
    }

    private void UpdateImageTracking(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        
        // Get the prefab to instantiate based on the tracked image
        GameObject prefabToSpawn = GetPrefabForImage(imageName);
        
        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"No prefab found for image: {imageName}");
            return;
        }

        // Either spawn a new prefab for this image or update the existing one
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            // If we haven't instantiated a prefab for this image yet
            if (!spawnedObjects.TryGetValue(imageName, out GameObject prefabInstance))
            {
                // Create the prefab
                prefabInstance = Instantiate(prefabToSpawn, trackedImage.transform.position, trackedImage.transform.rotation);
                
                // Play animation if the prefab has an Animator component
                Animator animator = prefabInstance.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.enabled = true;
                    // Play the default animation or a specific one
                    // animator.Play("AnimationName"); // Uncomment and specify animation name if needed
                }
                
                // Add to dictionary
                spawnedObjects.Add(imageName, prefabInstance);
                
                Debug.Log($"Spawned prefab for image: {imageName}");
            }
            else
            {
                // Update position and rotation of existing prefab
                prefabInstance.transform.position = trackedImage.transform.position;
                prefabInstance.transform.rotation = trackedImage.transform.rotation;
                prefabInstance.SetActive(true);
            }
        }
        else
        {
            // If the image is not being tracked, disable the prefab
            if (spawnedObjects.TryGetValue(imageName, out GameObject prefabInstance))
            {
                prefabInstance.SetActive(false);
            }
        }
    }

    private GameObject GetPrefabForImage(string imageName)
    {
        // Find the prefab that matches the image name
        foreach (var pair in imagePrefabPairs)
        {
            if (pair.imageName == imageName)
            {
                return pair.prefabToSpawn;
            }
        }
        
        return null;
    }
}