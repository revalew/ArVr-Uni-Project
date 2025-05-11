# ArVr-Uni-Project

<br/>

AR project for the uni course - an iOS App made in Unity

<br/><br/>

## Project Requirements

- Detect an image in space

- Tell user (somehow) that the image was detected

- Show a 3D model

- Play an animation on the model

<br/><br/><br/>

## AR/VR Lab Instructions (English Translation of [`AR_Lab_Instructions.pdf`](./AR_Lab_Instructions.pdf))

<br/>

### 1. Project Preparation:

a. Install AR Foundation and ARCore/ARKit:

   - AR Foundation (version depends on project, e.g., 5.0+)

   - ARCore XR Plugin (for Android)

   - ARKit XR Plugin (for iOS)

b. Configure XR Plugin Management:

   - Go to Edit > Project Settings > XR Plug-in Management

   - Enable Plugin Providers:

     - For Android: enable ARCore

     - For iOS: enable ARKit

c. Set the platform

d. For Android:

   - File > Build Settings, select "Android"

   - Switch Platform
   
   - Player Settings:
   
   - Other Settings:
   
   - Minimum API Level: Android 7.0 (API 24) or higher

   - Turn off Automatic Graphics API selection
   
   - Scripting Backend: IL2CPP
   
   - Target Architectures: ARMv7 / ARM64

e. For iOS:
   
   - File > Build Settings, select "iOS"
   
   - Switch Platform
   
   - Player Settings:
   
   - Other Settings:
   
   - Turn off Automatic Graphics API selection
   
   - Check "Camera Usage Description" (e.g., "Application requires access to the camera")

f. Add AR Session components:

   - In hierarchy: Menu Create > XR > AR Session

   - Add AR Session Origin (Create > XR > AR Session Origin)

<br/><br/>

### 2. Plane Detection and Object Spawning by Touch

Step 1: Prepare the scene

1. In hierarchy window, create (or make sure you have):

   - AR Session

   - AR Session Origin

2. To AR Session Origin add components:

   - AR Plane Manager (for detecting planes, you can set Plane Prefab by selecting it from the project)

   - AR Raycast Manager (for detecting touch on planes)

3. Create a prefab of the object to spawn

Step 2: Create the script

1. Create a new script "ARPlaceObject"

2. Implement the code for detecting touch, raycast to plane, and spawning object ([`./Assets/Scripts/ARTrackImage.cs`](./Assets/Scripts/ARTrackImage.cs))

3. Connect the script to AR Session Origin

<br/>

```csharp
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
```

<br/><br/>

### 3. Image Detection and Object Spawning

Step 1: Prepare a reference image

1. Get an image:

   - Create a clear image (e.g., PNG, 500x500 px) with unique pattern

   - Save it in the project folder (e.g., "AssetsImages/Marker.png")

2. Create a Reference Image Library:

   - In Project Menu Create > XR > Reference Image Library

   - Add to "Reference Library"

   - Add the image "Marker.png", set "Physical Size" (e.g., 0.1 m x 0.1 m)

Step 2: Configure the scene

1. To AR Session Origin add component AR Tracked Image Manager

2. In Inspector:

   - In "Serialized Library" assign "ARImageLibrary"

   - In "Tracked Image Prefab" assign prefab "SpawnObject" (if you have created it)

Step 3: Script for adding the model

1. Create a new script "ARTrackImage"

2. Implement code to track images and handle image detection events
   
Step 4: Script for spawning the object

1. Create a new script "ARPlaceObject"

2. Open the script (right-click) and implement the code ([`./Assets/Scripts/ARPlaceObject.cs`](./Assets/Scripts/ARPlaceObject.cs))

<br/>

```csharp
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARPlaceObject : MonoBehaviour
{
    public GameObject objectToSpawn;
    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new();

    private void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;

            if (raycastManager.Raycast(touchPosition, hits, TrackableType.Planes))
            {
                Pose hitPose = hits[0].pose;
                Instantiate(objectToSpawn, hitPose.position, hitPose.rotation);
            }
        }
    }
}

```

<br/>

Step 5: Connect the script to AR Session Origin

