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

    // Lista par obraz-prefab
    public List<ImagePrefabPair> imagePrefabPairs = new List<ImagePrefabPair>();
    
    // Słownik do śledzenia zinstancjonowanych obiektów
    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();
    
    private ARTrackedImageManager trackedImageManager;
    
    // Opcje orientacji modelu
    public enum ModelOrientation
    {
        FacingCamera,       // Model patrzy w stronę kamery/użytkownika
        FacingImageUp,      // Model patrzący w górę (domyślne zachowanie AR)
        FacingImageForward  // Model patrzy "do przodu" względem obrazu
    }
    
    // Wybór orientacji dla wszystkich modeli
    [Tooltip("Określa, jak model będzie zorientowany po wykryciu obrazu")]
    public ModelOrientation modelOrientation = ModelOrientation.FacingCamera;
    
    // Opcjonalny offset pozycji (gdyby model był zagłębiony w podłodze/obrazie)
    [Tooltip("Dodatkowe przesunięcie modelu w górę od pozycji obrazu")]
    public float heightOffset = 0.0f;

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
        // Obsługa dodanych obrazów
        foreach (var trackedImage in eventArgs.added)
        {
            Debug.Log($"Wykryto obraz: {trackedImage.referenceImage.name} (dodany)");
            UpdateImageTracking(trackedImage);
        }

        // Obsługa zaktualizowanych obrazów
        foreach (var trackedImage in eventArgs.updated)
        {
            UpdateImageTracking(trackedImage);
        }

        // Obsługa usuniętych obrazów
        foreach (var trackedImage in eventArgs.removed)
        {
            // Usuń powiązany prefab
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
        
        // Pobierz prefab odpowiadający wykrytemu obrazowi
        GameObject prefabToSpawn = GetPrefabForImage(imageName);
        
        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"Nie znaleziono prefabu dla obrazu: {imageName}");
            return;
        }

        // Obsługa w zależności od stanu śledzenia
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            // Jeśli jeszcze nie instancjonowaliśmy prefabu dla tego obrazu
            if (!spawnedObjects.TryGetValue(imageName, out GameObject prefabInstance))
            {
                // Pozycja z ewentualnym offsetem wysokości
                Vector3 position = trackedImage.transform.position;
                if (heightOffset != 0)
                {
                    position += trackedImage.transform.up * heightOffset;
                }
                
                // Ustal orientację w zależności od wybranej opcji
                Quaternion rotation;
                
                switch (modelOrientation)
                {
                    case ModelOrientation.FacingCamera:
                        // Model patrzy w stronę kamery (użytkownika)
                        Vector3 cameraPosition = Camera.main.transform.position;
                        Vector3 direction = cameraPosition - position;
                        direction.y = 0; // Ignorujemy różnicę wysokości
                        
                        if (direction != Vector3.zero)
                        {
                            rotation = Quaternion.LookRotation(direction);
                        }
                        else
                        {
                            // Fallback jeśli kamera jest dokładnie nad/pod obrazem
                            rotation = Quaternion.LookRotation(Camera.main.transform.forward);
                        }
                        break;
                        
                    case ModelOrientation.FacingImageForward:
                        // Model patrzy "do przodu" względem obrazu 
                        // (zakładając, że obraz ma określoną orientację)
                        rotation = trackedImage.transform.rotation * Quaternion.Euler(90f, 0f, 0f);
                        break;
                        
                    case ModelOrientation.FacingImageUp:
                    default:
                        // Domyślne zachowanie - model obrócony jak obraz
                        rotation = trackedImage.transform.rotation;
                        break;
                }
                
                // Utwórz prefab z odpowiednią orientacją
                prefabInstance = Instantiate(prefabToSpawn, position, rotation);
                
                // Uruchom animację, jeśli prefab ma komponent Animator
                Animator animator = prefabInstance.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.enabled = true;
                    // Odkomentuj, aby uruchomić konkretną animację
                    // animator.Play("NazwaAnimacji");
                }
                
                // Dodaj do słownika
                spawnedObjects.Add(imageName, prefabInstance);
                
                Debug.Log($"Utworzono prefab dla obrazu: {imageName}");
            }
            else
            {
                // Aktualizuj pozycję i rotację istniejącego prefabu
                Vector3 position = trackedImage.transform.position;
                if (heightOffset != 0)
                {
                    position += trackedImage.transform.up * heightOffset;
                }
                
                prefabInstance.transform.position = position;
                
                // Aktualizuj rotację w zależności od wybranej opcji
                switch (modelOrientation)
                {
                    case ModelOrientation.FacingCamera:
                        Vector3 cameraPosition = Camera.main.transform.position;
                        Vector3 direction = cameraPosition - position;
                        direction.y = 0; // Ignorujemy różnicę wysokości
                        
                        if (direction != Vector3.zero)
                        {
                            prefabInstance.transform.rotation = Quaternion.LookRotation(direction);
                        }
                        break;
                        
                    case ModelOrientation.FacingImageForward:
                        prefabInstance.transform.rotation = trackedImage.transform.rotation * Quaternion.Euler(90f, 0f, 0f);
                        break;
                        
                    case ModelOrientation.FacingImageUp:
                    default:
                        prefabInstance.transform.rotation = trackedImage.transform.rotation;
                        break;
                }
                
                prefabInstance.SetActive(true);
            }
        }
        else
        {
            // Jeśli obraz nie jest śledzony, wyłącz prefab
            if (spawnedObjects.TryGetValue(imageName, out GameObject prefabInstance))
            {
                prefabInstance.SetActive(false);
            }
        }
    }

    private GameObject GetPrefabForImage(string imageName)
    {
        // Znajdź prefab pasujący do nazwy obrazu
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