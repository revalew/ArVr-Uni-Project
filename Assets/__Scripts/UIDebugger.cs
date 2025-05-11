using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showButtonOutlines = true;
    public bool logButtonClicks = true;
    public bool fixCommonUIIssues = true;
    
    [Header("UI References")]
    public Button[] uiButtons;
    public Text[] uiTexts;
    
    private Color originalColor;
    
    // Słownik oryginalne kolory przycisków -> string z nazwą
    private Dictionary<Button, Color> buttonOriginalColors = new Dictionary<Button, Color>();

    void Start()
    {
        // Znajdź wszystkie przyciski i teksty, jeśli nie zostały przypisane
        if (uiButtons == null || uiButtons.Length == 0)
        {
            uiButtons = FindObjectsOfType<Button>();
        }
        
        if (uiTexts == null || uiTexts.Length == 0)
        {
            uiTexts = FindObjectsOfType<Text>();
        }
        
        // Dodaj debugowanie do każdego przycisku
        foreach (Button button in uiButtons)
        {
            if (button != null)
            {
                // Zapisz oryginalny kolor
                Image buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonOriginalColors[button] = buttonImage.color;
                    
                    // Dodaj outline dla lepszej widoczności
                    if (showButtonOutlines)
                    {
                        AddOutlineToButton(button);
                    }
                }
                
                // Dodaj debugowanie kliknięć
                if (logButtonClicks)
                {
                    button.onClick.AddListener(() => {
                        string buttonName = button.name;
                        Text buttonText = button.GetComponentInChildren<Text>();
                        if (buttonText != null && !string.IsNullOrEmpty(buttonText.text))
                        {
                            buttonName += " (" + buttonText.text + ")";
                        }
                        
                        Debug.Log("Kliknięto przycisk: " + buttonName);
                        FlashButton(button);
                    });
                }
            }
        }
        
        // Napraw typowe problemy z UI
        if (fixCommonUIIssues)
        {
            EnsureEventSystemExists();
            FixTextComponentSettings();
        }
        
        PrintUIHierarchy();
    }
    
    void Update()
    {
        // Dodaj debugowanie dotyku
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;
            
            // Sprawdź, czy dotyk trafił w UI
            if (IsPointerOverUI(touchPosition))
            {
                Debug.Log("Dotyk trafił w UI: " + touchPosition);
            }
            else
            {
                Debug.Log("Dotyk NIE trafił w UI: " + touchPosition);
            }
        }
    }
    
    private void AddOutlineToButton(Button button)
    {
        // Dodaj wyróżnienie przycisku
        Outline outline = button.GetComponent<Outline>();
        if (outline == null)
        {
            outline = button.gameObject.AddComponent<Outline>();
        }
        
        outline.effectColor = Color.yellow;
        outline.effectDistance = new Vector2(2, 2);
    }
    
    private void FlashButton(Button button)
    {
        // Efekt błysku po kliknięciu przycisku
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            ColorBlock colors = button.colors;
            buttonImage.color = colors.pressedColor;
            
            // Przywróć oryginalny kolor po chwili
            Invoke("ResetButtonColor", 0.2f);
        }
    }
    
    private void ResetButtonColor()
    {
        foreach (var pair in buttonOriginalColors)
        {
            Image buttonImage = pair.Key.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = pair.Value;
            }
        }
    }
    
    private bool IsPointerOverUI(Vector2 position)
    {
        if (EventSystem.current == null)
            return false;
            
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = position;
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        
        return results.Count > 0;
    }
    
    private void EnsureEventSystemExists()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            Debug.Log("Utworzono brakujący EventSystem");
        }
    }
    
    private void FixTextComponentSettings()
    {
        foreach (Text text in uiTexts)
        {
            if (text != null)
            {
                // Upewnij się, że tekst jest widoczny
                if (text.color.a < 0.5f)
                {
                    Color color = text.color;
                    color.a = 1.0f;
                    text.color = color;
                    Debug.Log("Naprawiono przezroczystość tekstu: " + text.name);
                }
                
                // Upewnij się, że czcionka jest przypisana
                if (text.font == null)
                {
                    text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    Debug.Log("Przypisano domyślną czcionkę do: " + text.name);
                }
                
                // Sprawdź, czy tekst mieści się w polu
                if (text.text.Length > 0 && text.preferredWidth > text.rectTransform.rect.width * 1.5f)
                {
                    text.resizeTextForBestFit = true;
                    text.resizeTextMinSize = 10;
                    text.resizeTextMaxSize = text.fontSize;
                    Debug.Log("Włączono automatyczne dopasowanie tekstu: " + text.name);
                }
            }
        }
    }
    
    private void PrintUIHierarchy()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        
        Debug.Log("=== UI HIERARCHY DEBUG ===");
        foreach (Canvas canvas in canvases)
        {
            Debug.Log($"Canvas: {canvas.name}, RenderMode: {canvas.renderMode}, PixelPerfect: {canvas.pixelPerfect}, Order: {canvas.sortingOrder}");
            
            PrintChildren(canvas.transform, 1);
        }
        Debug.Log("=== END UI HIERARCHY ===");
    }
    
    private void PrintChildren(Transform parent, int depth)
    {
        string indent = new string(' ', depth * 2);
        
        foreach (Transform child in parent)
        {
            string info = $"{indent}- {child.name}";
            
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                info += $" [Pos: {rectTransform.anchoredPosition}, Size: {rectTransform.sizeDelta}]";
            }
            
            if (child.gameObject.activeSelf)
            {
                Button button = child.GetComponent<Button>();
                if (button != null)
                {
                    Text buttonText = button.GetComponentInChildren<Text>();
                    info += $" [Button, Text: '{(buttonText != null ? buttonText.text : "none")}']";
                }
                
                Text text = child.GetComponent<Text>();
                if (text != null && button == null)
                {
                    info += $" [Text: '{text.text}', Color: {text.color}, Size: {text.fontSize}]";
                }
            }
            else
            {
                info += " [INACTIVE]";
            }
            
            Debug.Log(info);
            
            // Rekurencyjnie wyświetl dzieci
            if (child.childCount > 0)
            {
                PrintChildren(child, depth + 1);
            }
        }
    }
}