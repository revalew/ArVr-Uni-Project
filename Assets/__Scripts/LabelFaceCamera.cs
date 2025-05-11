using UnityEngine;

// Pomocniczy skrypt dla etykiet, który zapewnia, że zawsze będą zwrócone w stronę kamery
public class LabelFaceCamera : MonoBehaviour
{
    private Transform cameraTransform;
    
    void Start()
    {
        cameraTransform = Camera.main.transform;
    }
    
    void Update()
    {
        // Etykieta zawsze patrzy w stronę kamery
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
    }
}