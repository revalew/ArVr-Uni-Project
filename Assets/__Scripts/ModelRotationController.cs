using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ModelRotationController : MonoBehaviour
{
    [Header("UI References")]
    public Slider rotationSlider;
    public Text rotationValueText;
    
    [Header("AR References")]
    public ARTrackImage trackImageScript;
    public ARPlaceObject placeObjectScript;
    
    [Header("Special Models")]
    public float spider1YRotation = 180f;
    public float watcherYOffset = -0.3f;
    
    void Start()
    {
        if (rotationSlider != null)
        {
            // Inicjalizacja slidera
            rotationSlider.minValue = 0f;
            rotationSlider.maxValue = 360f;
            rotationSlider.value = 0f;
            
            // Dodaj listener do slidera
            rotationSlider.onValueChanged.AddListener(OnRotationValueChanged);
            
            // Aktualizuj tekst wartości
            UpdateRotationValueText(rotationSlider.value);
        }
    }
    
    public void OnRotationValueChanged(float value)
    {
        // Aktualizuj tekst
        UpdateRotationValueText(value);
        
        // Aplikuj rotację do aktywnych modeli
        RotateActiveModels(value);
    }
    
    private void UpdateRotationValueText(float value)
    {
        if (rotationValueText != null)
        {
            rotationValueText.text = $"{Mathf.RoundToInt(value)}°";
        }
    }
    
    private void RotateActiveModels(float rotationY)
    {
        // Aplikuj rotację do obiektów z ARTrackImage
        if (trackImageScript != null && trackImageScript.spawnedObjects != null)
        {
            foreach (var pair in trackImageScript.spawnedObjects)
            {
                GameObject model = pair.Value;
                if (model != null)
                {
                    // Zachowaj rotację X i Z, zmień tylko Y
                    Vector3 currentRotation = model.transform.eulerAngles;
                    model.transform.eulerAngles = new Vector3(
                        currentRotation.x,
                        rotationY,
                        currentRotation.z
                    );
                }
            }
        }
        
        // Aplikuj rotację do obiektów z ARPlaceObject
        if (placeObjectScript != null && placeObjectScript.spawnedObjects != null)
        {
            foreach (var model in placeObjectScript.spawnedObjects)
            {
                if (model != null)
                {
                    // Zachowaj rotację X i Z, zmień tylko Y
                    Vector3 currentRotation = model.transform.eulerAngles;
                    model.transform.eulerAngles = new Vector3(
                        currentRotation.x,
                        rotationY,
                        currentRotation.z
                    );
                }
            }
        }
    }
    
    // Ta metoda może być wywoływana przez ARTrackImage lub ARPlaceObject
    // aby obsłużyć specjalne modele przy ich tworzeniu
    public void HandleSpecialModel(GameObject model)
    {
        if (model == null)
            return;
            
        // Obsługa modelu Spider_1
        if (model.name.Contains("Spider_1"))
        {
            // Aplikuj dodatkową rotację o 180 stopni
            Quaternion currentRotation = model.transform.rotation;
            model.transform.rotation = currentRotation * Quaternion.Euler(0f, spider1YRotation, 0f);
        }
        
        // Obsługa modelu Watcher
        if (model.name.Contains("Watcher"))
        {
            // Aplikuj offset w osi Y
            Vector3 currentPosition = model.transform.position;
            model.transform.position = new Vector3(
                currentPosition.x,
                currentPosition.y + watcherYOffset,
                currentPosition.z
            );
        }
    }
    
    // Resetuj slider do 0
    public void ResetRotation()
    {
        if (rotationSlider != null)
        {
            rotationSlider.value = 0f;
        }
    }
}