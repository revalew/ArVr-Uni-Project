using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ARUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Button togglePlaneButton;
    public Button toggleImageButton;
    public Text planeButtonText;
    public Text imageButtonText;
    
    [Header("AR Script References")]
    public ARPlaceObject placeObjectScript;
    public ARTrackImage trackImageScript;
    
    [Header("UI Settings")]
    public Color buttonColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color textColor = Color.white;
    public int fontSize = 24;
    
    [Header("Button Layout")]
    [Tooltip("Rozmiar przycisków (szerokość x wysokość)")]
    public Vector2 buttonSize = new Vector2(350, 100);
    
    [Tooltip("Margines od dolnej krawędzi (procent wysokości ekranu)")]
    [Range(0.01f, 0.3f)]
    public float bottomMarginPercent = 0.1f;
    
    [Tooltip("Margines od bocznych krawędzi (procent szerokości ekranu)")]
    [Range(0.01f, 0.3f)]
    public float sideMarginPercent = 0.1f;
    
    // Cached canvas reference
    private Canvas mainCanvas;
    private CanvasScaler canvasScaler;
    
    void Start()
    {
        // Upewnij się, że mamy EventSystem
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            Debug.Log("Utworzono brakujący EventSystem");
        }
        
        // Znajdź Canvas i dodaj CanvasScaler, jeśli potrzeba
        SetupCanvasScaler();
        
        // Sprawdź i zainicjuj przyciski
        InitializeUI();
        
        // Podłącz skrypty AR
        ConnectARScripts();
        
        // Ustaw początkowy układ UI
        UpdateUILayout();
        
        // Nasłuchuj zmiany orientacji ekranu
        // Unity nie ma bezpośredniego zdarzenia na zmianę orientacji w Runtime,
        // więc będziemy sprawdzać w Update
    }
    
    void Update()
    {
        // Sprawdzenie orientacji i aktualizacja układu UI
        // Można to zoptymalizować, aby nie sprawdzać co klatkę
        UpdateUILayout();
    }
    
    private void SetupCanvasScaler()
    {
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas != null)
        {
            canvasScaler = mainCanvas.GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                canvasScaler = mainCanvas.gameObject.AddComponent<CanvasScaler>();
            }
            
            // Ustaw responsywne skalowanie
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.matchWidthOrHeight = 0.5f; // Balans między szerokością a wysokością
            
            Debug.Log("Canvas i CanvasScaler zostały skonfigurowane");
        }
        else
        {
            Debug.LogWarning("Nie znaleziono obiektu Canvas w scenie!");
        }
    }
    
    private void InitializeUI()
    {
        // Sprawdź i napraw przycisk płaszczyzn
        if (togglePlaneButton != null)
        {
            // Ustawienia przycisku
            Image buttonImage = togglePlaneButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = buttonColor;
            }
            
            // Upewnij się, że tekst jest widoczny
            if (planeButtonText != null)
            {
                planeButtonText.color = textColor;
                planeButtonText.fontSize = fontSize;
                
                // Ustaw domyślny tekst, jeśli jest pusty
                if (string.IsNullOrEmpty(planeButtonText.text))
                {
                    planeButtonText.text = "Wyłącz wykrywanie płaszczyzn";
                }
            }
            
            // Dopasuj rozmiar przycisku
            RectTransform rectTransform = togglePlaneButton.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = buttonSize;
            }
        }
        
        // Sprawdź i napraw przycisk obrazów
        if (toggleImageButton != null)
        {
            // Ustawienia przycisku
            Image buttonImage = toggleImageButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = buttonColor;
            }
            
            // Upewnij się, że tekst jest widoczny
            if (imageButtonText != null)
            {
                imageButtonText.color = textColor;
                imageButtonText.fontSize = fontSize;
                
                // Ustaw domyślny tekst, jeśli jest pusty
                if (string.IsNullOrEmpty(imageButtonText.text))
                {
                    imageButtonText.text = "Wyłącz wykrywanie obrazów";
                }
            }
            
            // Dopasuj rozmiar przycisku
            RectTransform rectTransform = toggleImageButton.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = buttonSize;
            }
        }
        
        Debug.Log("UI zostało zainicjowane");
    }
    
    private void UpdateUILayout()
    {
        // Sprawdź bieżącą orientację ekranu
        bool isPortrait = Screen.height > Screen.width;
        
        // Oblicz marginesy
        float bottomMargin = Screen.height * bottomMarginPercent;
        float sideMargin = Screen.width * sideMarginPercent;
        
        if (isPortrait)
        {
            // Tryb pionowy - przyciski na dole ekranu, jeden obok drugiego
            // Dopasuj pozycje przycisków
            if (togglePlaneButton != null)
            {
                RectTransform rectTransform = togglePlaneButton.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Lewy przycisk - pozycjonowany przy dolnej krawędzi, od lewej
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 0);
                    rectTransform.pivot = new Vector2(0, 0);
                    rectTransform.anchoredPosition = new Vector2(sideMargin, bottomMargin);
                }
            }
            
            if (toggleImageButton != null)
            {
                RectTransform rectTransform = toggleImageButton.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Prawy przycisk - pozycjonowany przy dolnej krawędzi, od prawej
                    rectTransform.anchorMin = new Vector2(1, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    rectTransform.pivot = new Vector2(1, 0);
                    rectTransform.anchoredPosition = new Vector2(-sideMargin, bottomMargin);
                }
            }
        }
        else
        {
            // Tryb poziomy - przyciski na dole ekranu, jeden obok drugiego
            if (togglePlaneButton != null)
            {
                RectTransform rectTransform = togglePlaneButton.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Lewy przycisk - pozycjonowany przy dolnej krawędzi, od lewej
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 0);
                    rectTransform.pivot = new Vector2(0, 0);
                    rectTransform.anchoredPosition = new Vector2(sideMargin, bottomMargin);
                }
            }
            
            if (toggleImageButton != null)
            {
                RectTransform rectTransform = toggleImageButton.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Prawy przycisk - pozycjonowany przy dolnej krawędzi, od prawej
                    rectTransform.anchorMin = new Vector2(1, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    rectTransform.pivot = new Vector2(1, 0);
                    rectTransform.anchoredPosition = new Vector2(-sideMargin, bottomMargin);
                }
            }
        }
    }
    
    private void ConnectARScripts()
    {
        // Połącz przyciski z odpowiednimi skryptami AR
        if (togglePlaneButton != null && placeObjectScript != null)
        {
            // Usuń wszystkie istniejące listenery - zapobiega duplikatom
            togglePlaneButton.onClick.RemoveAllListeners();
            
            // Dodaj nowy listener
            togglePlaneButton.onClick.AddListener(() => {
                placeObjectScript.TogglePlaneDetection();
                Debug.Log("Przycisk płaszczyzn kliknięty");
            });
            
            // Ustaw referencje w ARPlaceObject
            placeObjectScript.togglePlaneDetectionButton = togglePlaneButton;
            placeObjectScript.toggleButtonText = planeButtonText;
            
            Debug.Log("Połączono przycisk płaszczyzn ze skryptem ARPlaceObject");
        }
        else
        {
            Debug.LogWarning("Brakujące referencje dla przycisku płaszczyzn!");
        }
        
        if (toggleImageButton != null && trackImageScript != null)
        {
            // Usuń wszystkie istniejące listenery - zapobiega duplikatom
            toggleImageButton.onClick.RemoveAllListeners();
            
            // Dodaj nowy listener
            toggleImageButton.onClick.AddListener(() => {
                trackImageScript.ToggleImageTracking();
                Debug.Log("Przycisk obrazów kliknięty");
            });
            
            // Ustaw referencje w ARTrackImage
            trackImageScript.toggleImageTrackingButton = toggleImageButton;
            trackImageScript.toggleButtonText = imageButtonText;
            
            Debug.Log("Połączono przycisk obrazów ze skryptem ARTrackImage");
        }
        else
        {
            Debug.LogWarning("Brakujące referencje dla przycisku obrazów!");
        }
    }
}