using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

#if UNITY_EDITOR
public class SliderCreator : EditorWindow
{
    [MenuItem("Tools/Create Rotation Slider")]
    static void CreateSlider()
    {
        // Znajdź Canvas - jeśli nie istnieje, utwórz go
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("AR UI Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Utwórz panel slidera
        GameObject sliderPanel = new GameObject("RotationSliderPanel");
        sliderPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = sliderPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1);
        panelRect.anchorMax = new Vector2(0.5f, 1);
        panelRect.pivot = new Vector2(0.5f, 1);
        panelRect.anchoredPosition = new Vector2(0, -100); // 100px od góry
        panelRect.sizeDelta = new Vector2(400, 60);
        
        // Dodaj tło dla slidera
        Image panelImage = sliderPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        // Utwórz slider
        GameObject sliderObj = new GameObject("RotationSlider");
        sliderObj.transform.SetParent(sliderPanel.transform, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0.5f);
        sliderRect.anchorMax = new Vector2(1, 0.5f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(-40, 0); // Przesunięcie w lewo, żeby zrobić miejsce dla tekstu
        sliderRect.sizeDelta = new Vector2(-100, 20);
        
        // Dodaj komponent Slider
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 360;
        slider.value = 0;
        slider.wholeNumbers = true; // Tylko wartości całkowite
        
        // Utwórz tło slidera
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1);
        
        // Utwórz wypełnienie slidera
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillRect = fillArea.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0.5f);
        fillRect.anchorMax = new Vector2(1, 0.5f);
        fillRect.sizeDelta = new Vector2(-10, 10);
        fillRect.anchoredPosition = Vector2.zero;
        
        // Utwórz wypełnienie
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillObjRect = fill.AddComponent<RectTransform>();
        fillObjRect.anchorMin = Vector2.zero;
        fillObjRect.anchorMax = new Vector2(1, 1);
        fillObjRect.sizeDelta = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.7f, 1, 1);
        slider.fillRect = fillObjRect;
        
        // Utwórz uchwyt slidera
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = new Vector2(0, 0.5f);
        handleAreaRect.anchorMax = new Vector2(1, 0.5f);
        handleAreaRect.sizeDelta = new Vector2(-10, 20);
        handleAreaRect.anchoredPosition = Vector2.zero;
        
        // Utwórz uchwyt
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(30, 30);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(0.7f, 0.7f, 0.7f, 1);
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        
        // Utwórz tekst dla wartości rotacji
        GameObject valueTextObj = new GameObject("Value Text");
        valueTextObj.transform.SetParent(sliderPanel.transform, false);
        RectTransform valueTextRect = valueTextObj.AddComponent<RectTransform>();
        valueTextRect.anchorMin = new Vector2(1, 0.5f);
        valueTextRect.anchorMax = new Vector2(1, 0.5f);
        valueTextRect.pivot = new Vector2(1, 0.5f);
        valueTextRect.anchoredPosition = new Vector2(-10, 0);
        valueTextRect.sizeDelta = new Vector2(80, 30);
        
        // Dodaj komponent Text
        Text valueText = valueTextObj.AddComponent<Text>();
        valueText.text = "0°";
        valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        valueText.fontSize = 24;
        valueText.alignment = TextAnchor.MiddleRight;
        valueText.color = Color.white;
        
        // Zaktualizuj ARUIManager, żeby korzystał z slidera
        ARUIManager uiManager = Object.FindObjectOfType<ARUIManager>();
        if (uiManager != null)
        {
            SerializedObject serializedObject = new SerializedObject(uiManager);
            
            // Dodaj panele do menedżera UI
            SerializedProperty rotationSliderPanelProp = serializedObject.FindProperty("rotationSliderPanel");
            if (rotationSliderPanelProp != null)
            {
                rotationSliderPanelProp.objectReferenceValue = sliderPanel;
            }
            
            SerializedProperty rotationSliderProp = serializedObject.FindProperty("rotationSlider");
            if (rotationSliderProp != null)
            {
                rotationSliderProp.objectReferenceValue = slider;
            }
            
            SerializedProperty rotationValueTextProp = serializedObject.FindProperty("rotationValueText");
            if (rotationValueTextProp != null)
            {
                rotationValueTextProp.objectReferenceValue = valueText;
            }
            
            serializedObject.ApplyModifiedProperties();
            
            // Wyświetl informację dla użytkownika
            if (rotationSliderPanelProp == null || rotationSliderProp == null || rotationValueTextProp == null)
            {
                Debug.LogWarning("Nie wszystkie właściwości znaleziono w ARUIManager. Upewnij się, że dodałeś wymagane pola.");
            }
            else
            {
                Debug.Log("Slider rotacji został pomyślnie utworzony i podłączony do ARUIManager.");
            }
        }
        else
        {
            Debug.LogWarning("Nie znaleziono ARUIManager w scenie. Dodaj go, a następnie ręcznie przypisz referencje.");
        }
    }
}
#endif