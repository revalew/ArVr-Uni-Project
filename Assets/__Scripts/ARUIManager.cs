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
    
    [Header("Rotation Control")]
    public GameObject rotationSliderPanel;
    public Slider rotationSlider;
    public Text rotationValueText;
    public ModelRotationController rotationController;

    [Tooltip("Pozycja slidera (0 = lewa strona, 1 = prawa strona)")]
    [Range(0, 1)]
    public int sliderPosition = 1;

    [Tooltip("Przesunięcie slidera od góry ekranu (procent wysokości)")]
    [Range(0.05f, 0.4f)]
    public float sliderTopMargin = 0.2f;

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

        // Dodatkowo obsłuż pozycję slidera
        if (rotationSliderPanel != null)
        {
            RectTransform sliderRect = rotationSliderPanel.GetComponent<RectTransform>();
            if (sliderRect != null)
            {
                // bool isPortrait = Screen.height > Screen.width;
                // float sideMargin = Screen.width * sideMarginPercent;
                float margin = Screen.height * 0.05f; // 5% wysokości ekranu
                
                if (sliderPosition == 0) // Lewa strona
                {
                    sliderRect.anchorMin = new Vector2(0, 0.5f);
                    sliderRect.anchorMax = new Vector2(0, 0.5f);
                    sliderRect.pivot = new Vector2(0, 0.5f);
                    
                    if (isPortrait)
                    {
                        // Pionowo po lewej stronie
                        sliderRect.anchoredPosition = new Vector2(sideMargin, margin);
                        sliderRect.rotation = Quaternion.Euler(0, 0, 90);
                        sliderRect.sizeDelta = new Vector2(80, Screen.height * 0.3f);
                    }
                    else
                    {
                        // Poziomo po lewej stronie
                        sliderRect.anchoredPosition = new Vector2(sideMargin, margin);
                        sliderRect.rotation = Quaternion.identity;
                        sliderRect.sizeDelta = new Vector2(Screen.width * 0.3f, 80);
                    }
                }
                else // Prawa strona
                {
                    sliderRect.anchorMin = new Vector2(1, 0.5f);
                    sliderRect.anchorMax = new Vector2(1, 0.5f);
                    sliderRect.pivot = new Vector2(1, 0.5f);
                    
                    if (isPortrait)
                    {
                        // Pionowo po prawej stronie
                        sliderRect.anchoredPosition = new Vector2(-sideMargin, margin);
                        sliderRect.rotation = Quaternion.Euler(0, 0, -90);
                        sliderRect.sizeDelta = new Vector2(80, Screen.height * 0.3f);
                    }
                    else
                    {
                        // Poziomo po prawej stronie
                        sliderRect.anchoredPosition = new Vector2(-sideMargin, margin);
                        sliderRect.rotation = Quaternion.identity;
                        sliderRect.sizeDelta = new Vector2(Screen.width * 0.3f, 80);
                    }
                }
            }
        }

        // Obsługa slidera rotacji
        UpdateSliderLayout();
    }

    private void UpdateSliderLayout()
    {
        if (rotationSliderPanel == null) return;
        
        RectTransform sliderRect = rotationSliderPanel.GetComponent<RectTransform>();
        if (sliderRect == null) return;
        
        bool isPortrait = Screen.height > Screen.width;
        float sideMargin = Screen.width * 0.05f; // 5% szerokości ekranu
        float topMargin = Screen.height * sliderTopMargin;
        
        // Reset rotacji i skali
        sliderRect.localScale = Vector3.one;
        
        if (isPortrait)
        {
            // Tryb pionowy - slider przy bocznej krawędzi, obrócony o 90 stopni
            if (sliderPosition == 0) // Lewa strona
            {
                // Ustaw anchory do lewej krawędzi, wyśrodkowany w pionie
                sliderRect.anchorMin = new Vector2(0, 0.5f);
                sliderRect.anchorMax = new Vector2(0, 0.5f);
                sliderRect.pivot = new Vector2(0.5f, 0.5f);
                
                // Obróć slider o 90 stopni
                sliderRect.localRotation = Quaternion.Euler(0, 0, 90);
                
                // Ustaw pozycję przy lewej krawędzi
                sliderRect.anchoredPosition = new Vector2(40, 0);
                
                // Ustaw rozmiar (zamień szerokość z wysokością po obrocie)
                sliderRect.sizeDelta = new Vector2(200, 60);
            }
            else // Prawa strona
            {
                // Ustaw anchory do prawej krawędzi, wyśrodkowany w pionie
                sliderRect.anchorMin = new Vector2(1, 0.5f);
                sliderRect.anchorMax = new Vector2(1, 0.5f);
                sliderRect.pivot = new Vector2(0.5f, 0.5f);
                
                // Obróć slider o -90 stopni (przeciwnie do ruchu wskazówek zegara)
                sliderRect.localRotation = Quaternion.Euler(0, 0, -90);
                
                // Ustaw pozycję przy prawej krawędzi
                sliderRect.anchoredPosition = new Vector2(-40, 0);
                
                // Ustaw rozmiar (zamień szerokość z wysokością po obrocie)
                sliderRect.sizeDelta = new Vector2(200, 60);
            }
        }
        else
        {
            // Tryb poziomy - slider na górze ekranu
            // Bez obrotu (poziomy slider)
            sliderRect.localRotation = Quaternion.identity;
            
            // Ustaw anchory do górnej krawędzi, wyśrodkowany w poziomie
            sliderRect.anchorMin = new Vector2(0.5f, 1);
            sliderRect.anchorMax = new Vector2(0.5f, 1);
            sliderRect.pivot = new Vector2(0.5f, 1);
            
            // Ustaw pozycję poniżej górnej krawędzi
            sliderRect.anchoredPosition = new Vector2(0, -topMargin);
            
            // Ustaw rozmiar (szerszy w poziomie)
            sliderRect.sizeDelta = new Vector2(400, 60);
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

        // Inicjalizacja kontrolera rotacji
        if (rotationController == null && rotationSlider != null)
        {
            rotationController = gameObject.GetComponent<ModelRotationController>();
            if (rotationController == null)
            {
                rotationController = gameObject.AddComponent<ModelRotationController>();
            }
            
            rotationController.rotationSlider = rotationSlider;
            rotationController.rotationValueText = rotationValueText;
            rotationController.trackImageScript = trackImageScript;
            rotationController.placeObjectScript = placeObjectScript;
            
            // Przypisz kontroler rotacji do skryptów AR
            if (trackImageScript != null)
                trackImageScript.rotationController = rotationController;
                
            if (placeObjectScript != null)
                placeObjectScript.rotationController = rotationController;
        }
    }
}