using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ARPlaceObject : MonoBehaviour
{
    public GameObject objectToSpawn;
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private List<ARRaycastHit> hits = new();
    
    // Lista utworzonych obiektów
    private List<GameObject> spawnedObjects = new List<GameObject>();
    
    [Header("Plane Detection Settings")]
    public Button togglePlaneDetectionButton;
    public Text toggleButtonText;
    public string enablePlaneDetectionText = "Włącz wykrywanie płaszczyzn";
    public string disablePlaneDetectionText = "Wyłącz wykrywanie płaszczyzn";
    private bool isPlaneDetectionEnabled = true;
    
    [Header("Label Settings")]
    public bool showLabels = true;
    public Color labelColor = Color.white;
    public int labelFontSize = 50;
    public float labelHeightOffset = 0.1f;
    public float labelCharacterSize = 0.003f;

    private void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        
        // Upewnij się, że mamy przycisk
        if (togglePlaneDetectionButton != null)
        {
            togglePlaneDetectionButton.onClick.RemoveAllListeners();
            togglePlaneDetectionButton.onClick.AddListener(TogglePlaneDetection);
            UpdateToggleButtonText();
        }
        else
        {
            Debug.LogWarning("Przycisk Toggle nie został przypisany w inspektorze!");
        }
    }

    private void Update()
    {
        // Wykrywanie dotyku i raycast tylko jeśli wykrywanie płaszczyzn jest włączone
        if (isPlaneDetectionEnabled && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Sprawdź, czy dotyk nie trafił w UI
            if (IsPointerOverUI(Input.GetTouch(0).position))
            {
                return; // Jeśli dotyk trafił w UI, ignoruj go dla AR
            }
            
            Vector2 touchPosition = Input.GetTouch(0).position;

            if (raycastManager.Raycast(touchPosition, hits, TrackableType.Planes))
            {
                Pose hitPose = hits[0].pose;
                GameObject spawnedObject = Instantiate(objectToSpawn, hitPose.position, hitPose.rotation);
                spawnedObject.name = objectToSpawn.name + "_" + spawnedObjects.Count;
                
                // Dodaj obiekt do listy
                spawnedObjects.Add(spawnedObject);
                
                // Dodaj etykietę z nazwą prefaba
                if (showLabels)
                {
                    AddLabelToObject(spawnedObject, objectToSpawn.name);
                }
            }
        }
    }
    
    // Sprawdza, czy dotyk trafił w element UI
    private bool IsPointerOverUI(Vector2 position)
    {
        if (EventSystem.current == null)
            return false;
            
        // Konwertuj pozycję dotyku na pointer event data
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = position;
        
        // Przygotuj listę wyników
        List<RaycastResult> results = new List<RaycastResult>();
        
        // Raycast przez UI
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        
        // Zwróć true, jeśli raycast trafił w UI
        return results.Count > 0;
    }
    
    // Dodaje etykietę z nazwą nad obiektem - ZAKTUALIZOWANA METODA
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

    // Przełącza wykrywanie płaszczyzn
    public void TogglePlaneDetection()
    {
        isPlaneDetectionEnabled = !isPlaneDetectionEnabled;
        
        Debug.Log("Przycisk płaszczyzn naciśnięty. Nowy stan: " + isPlaneDetectionEnabled);
        
        // Włącz/wyłącz komponent ARPlaneManager
        if (planeManager != null)
        {
            // Włącz/wyłącz detekcję nowych płaszczyzn
            planeManager.enabled = isPlaneDetectionEnabled;
            
            // Pokaż/ukryj istniejące płaszczyzny
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(isPlaneDetectionEnabled);
            }
            
            // Wyczyść wszystkie utworzone obiekty przy wyłączeniu
            if (!isPlaneDetectionEnabled)
            {
                ClearAllSpawnedObjects();
            }
        }
        
        UpdateToggleButtonText();
    }
    
    // Usuwa wszystkie utworzone obiekty
    private void ClearAllSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        
        spawnedObjects.Clear();
        Debug.Log("Wyczyszczono wszystkie utworzone obiekty");
    }
    
    // Aktualizuje tekst przycisku
    private void UpdateToggleButtonText()
    {
        if (toggleButtonText != null)
        {
            toggleButtonText.text = isPlaneDetectionEnabled 
                ? disablePlaneDetectionText 
                : enablePlaneDetectionText;
        }
    }
}