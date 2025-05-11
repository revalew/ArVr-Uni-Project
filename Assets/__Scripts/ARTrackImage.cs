using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

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
    
    [Header("UI Settings")]
    // Przycisk do włączania/wyłączania śledzenia obrazów
    public Button toggleImageTrackingButton;
    public Text toggleButtonText;
    public string enableTrackingText = "Włącz wykrywanie obrazów";
    public string disableTrackingText = "Wyłącz wykrywanie obrazów";
    private bool isImageTrackingEnabled = true;
    
    [Header("Label Settings")]
    public bool showLabels = true;
    public Color labelColor = Color.white;
    public int labelFontSize = 50;
    public float labelHeightOffset = 0.1f;
    public float labelCharacterSize = 0.003f;
    
    // Lista etykiet do śledzenia
    private List<GameObject> createdLabels = new List<GameObject>();

    private void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void Start()
    {
        // Ustawienie przycisku toggle
        if (toggleImageTrackingButton != null)
        {
            toggleImageTrackingButton.onClick.RemoveAllListeners();
            toggleImageTrackingButton.onClick.AddListener(ToggleImageTracking);
            UpdateToggleButtonText();
        }
    }

    private void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Ignorujemy zdarzenia jeśli śledzenie obrazów jest wyłączone
        if (!isImageTrackingEnabled) return;
        
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
                prefabInstance.name = $"{imageName}_Model"; // Nadaj unikalną nazwę
                
                // Dodaj etykietę z nazwą obrazu/prefabu jeśli włączone
                if (showLabels)
                {
                    AddLabelToObject(prefabInstance, imageName);
                }
                
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
    
    // ZAKTUALIZOWANA METODA - Dodaje etykietę z nazwą nad obiektem
    private void AddLabelToObject(GameObject obj, string labelText)
    {
        // Sprawdź czy model ma dedykowany punkt zakotwiczenia
        Transform labelAnchor = obj.transform.Find("LabelAnchor");
        
        GameObject labelObj = new GameObject($"{labelText}_Label");
        
        if (labelAnchor != null)
        {
            // Użyj dedykowanego punktu zakotwiczenia
            labelObj.transform.SetParent(labelAnchor);
            labelObj.transform.localPosition = Vector3.zero;
            Debug.Log($"Użyto dedykowanego punktu zakotwiczenia dla {labelText}");
        }
        else
        {
            // Użyj automatycznego pozycjonowania (jak wcześniej)
            labelObj.transform.SetParent(obj.transform);
            
            // Znajdź wszystkie renderery
            Renderer[] allRenderers = obj.GetComponentsInChildren<Renderer>();
            
            if (allRenderers.Length > 0)
            {
                // Oblicz całkowite bounds dla wszystkich rendererów
                Bounds totalBounds = CalculateTotalBounds(allRenderers);
                
                // Dynamiczny offset bazujący na skali modelu
                float modelScale = (totalBounds.size.x + totalBounds.size.y + totalBounds.size.z) / 3f;
                float dynamicOffset = Mathf.Max(0.2f, modelScale * 0.5f);
                
                // Ustaw pozycję etykiety
                labelObj.transform.localPosition = new Vector3(0, totalBounds.size.y + dynamicOffset, 0);
            }
            else
            {
                // Fallback
                labelObj.transform.localPosition = new Vector3(0, 1f, 0);
            }
        }
        
        // Dodaj TextMesh
        TextMesh textMesh = labelObj.AddComponent<TextMesh>();
        textMesh.text = labelText;
        textMesh.fontSize = labelFontSize;
        textMesh.characterSize = labelCharacterSize;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.LowerCenter;
        textMesh.color = labelColor;
        
        // Dodaj obrót w stronę kamery
        labelObj.AddComponent<LabelFaceCamera>();
    }
    
    private Bounds CalculateTotalBounds(Renderer[] renderers)
    {
        if (renderers.Length == 0)
            return new Bounds(Vector3.zero, Vector3.zero);
        
        // Inicjalizuj bounds używając pierwszego renderera
        Bounds bounds = renderers[0].bounds;
        Matrix4x4 localToWorld = renderers[0].transform.localToWorldMatrix;
        Matrix4x4 worldToLocal = renderers[0].transform.parent.worldToLocalMatrix;
        
        // Konwertuj bounds do przestrzeni lokalnej rodzica
        Vector3 center = worldToLocal.MultiplyPoint3x4(bounds.center);
        
        // Utwórz nowe bounds w przestrzeni lokalnej
        Bounds localBounds = new Bounds(center, bounds.size);
        
        // Dodaj pozostałe renderery
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds = renderers[i].bounds;
            localToWorld = renderers[i].transform.localToWorldMatrix;
            worldToLocal = renderers[i].transform.parent.worldToLocalMatrix;
            
            // Konwertuj punkty bounds do przestrzeni lokalnej rodzica
            Vector3[] corners = GetBoundsCorners(bounds);
            foreach (Vector3 corner in corners)
            {
                Vector3 worldCorner = localToWorld.MultiplyPoint3x4(corner);
                Vector3 localCorner = worldToLocal.MultiplyPoint3x4(worldCorner);
                localBounds.Encapsulate(localCorner);
            }
        }
        
        return localBounds;
    }

    private Vector3[] GetBoundsCorners(Bounds bounds)
    {
        Vector3[] corners = new Vector3[8];
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;
        
        corners[0] = center + new Vector3(extents.x, extents.y, extents.z);
        corners[1] = center + new Vector3(extents.x, extents.y, -extents.z);
        corners[2] = center + new Vector3(extents.x, -extents.y, extents.z);
        corners[3] = center + new Vector3(extents.x, -extents.y, -extents.z);
        corners[4] = center + new Vector3(-extents.x, extents.y, extents.z);
        corners[5] = center + new Vector3(-extents.x, extents.y, -extents.z);
        corners[6] = center + new Vector3(-extents.x, -extents.y, extents.z);
        corners[7] = center + new Vector3(-extents.x, -extents.y, -extents.z);
        
        return corners;
    }

    // Przełącza wykrywanie obrazów
    public void ToggleImageTracking()
    {
        isImageTrackingEnabled = !isImageTrackingEnabled;
        
        Debug.Log("Przycisk obrazów naciśnięty. Nowy stan: " + isImageTrackingEnabled);
        
        // Włącz/wyłącz komponent ARTrackedImageManager
        if (trackedImageManager != null)
        {
            trackedImageManager.enabled = isImageTrackingEnabled;
        }
        
        // Wyczyść wszystkie obiekty przy wyłączeniu śledzenia
        if (!isImageTrackingEnabled)
        {
            ClearAllTrackedObjects();
        }
        
        UpdateToggleButtonText();
    }
    
    // Usuwa wszystkie śledzone obiekty
    private void ClearAllTrackedObjects()
    {
        // Kopiuj klucze, aby uniknąć problemu z modyfikacją podczas iteracji
        string[] keys = new string[spawnedObjects.Count];
        spawnedObjects.Keys.CopyTo(keys, 0);
        
        foreach (string key in keys)
        {
            if (spawnedObjects.TryGetValue(key, out GameObject obj) && obj != null)
            {
                Destroy(obj);
            }
        }
        
        // Wyczyść słownik
        spawnedObjects.Clear();
        
        // Wyczyść listę etykiet
        foreach (GameObject label in createdLabels)
        {
            if (label != null)
            {
                Destroy(label);
            }
        }
        createdLabels.Clear();
        
        Debug.Log("Wyczyszczono wszystkie śledzone obiekty");
    }
    
    // Aktualizuje tekst przycisku
    private void UpdateToggleButtonText()
    {
        if (toggleButtonText != null)
        {
            toggleButtonText.text = isImageTrackingEnabled 
                ? disableTrackingText 
                : enableTrackingText;
            
            Debug.Log("Zaktualizowano tekst przycisku na: " + toggleButtonText.text);
        }
        else
        {
            Debug.LogWarning("Brak referencji do tekstu przycisku!");
        }
    }
}