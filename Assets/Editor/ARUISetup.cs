using UnityEngine;
using UnityEngine.UI;

// Pomocniczy skrypt do szybkiego utworzenia interfejsu użytkownika
public class ARUISetup : MonoBehaviour
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Create UI Buttons")]
    public static void CreateARUI()
    {
        // Sprawdź, czy mamy już Canvas
        Canvas existingCanvas = GameObject.FindFirstObjectByType<Canvas>();
        
        if (existingCanvas != null)
        {
            Debug.Log("Canvas już istnieje. Dodaję przyciski do istniejącego Canvas.");
            AddButtonsToCanvas(existingCanvas.gameObject);
            return;
        }
        
        // Utwórz Canvas
        GameObject canvasObj = new GameObject("AR UI Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Dodaj skalowanie UI
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        
        // Dodaj raycaster dla interakcji
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Dodaj przyciski
        AddButtonsToCanvas(canvasObj);
        
        Debug.Log("Utworzono canvas z przyciskami AR. Przypisz je do odpowiednich komponentów AR.");
    }
    
    private static void AddButtonsToCanvas(GameObject canvasObj)
    {
        // Utwórz przycisk dla płaszczyzn
        GameObject planeToggleBtn = CreateButton(canvasObj.transform, "TogglePlaneDetectionButton", "Wyłącz wykrywanie płaszczyzn", new Vector2(0, 100));
        
        // Utwórz przycisk dla obrazów
        GameObject imageToggleBtn = CreateButton(canvasObj.transform, "ToggleImageTrackingButton", "Wyłącz wykrywanie obrazów", new Vector2(0, 0));
        
        Debug.Log("Utworzono przyciski. Przypisz je do komponentów ARPlaceObject i ARTrackImage.");
    }
    
    private static GameObject CreateButton(Transform parent, string name, string text, Vector2 position)
    {
        // Utwórz obiekt przycisku
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        // Dodaj komponent przycisku i obrazu
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Ustaw pozycję i rozmiar
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(300, 60);
        
        // Dodaj komponent Button
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        button.colors = colors;
        
        // Dodaj tekst do przycisku
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 24;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        
        // Ustaw rozmiar i pozycję tekstu
        RectTransform textRectTransform = textObj.GetComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.offsetMin = Vector2.zero;
        textRectTransform.offsetMax = Vector2.zero;
        
        return buttonObj;
    }
#endif
}