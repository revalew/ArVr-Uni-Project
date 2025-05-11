using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class TextureFixer : EditorWindow
{
    private string prefabFolderPath = "Assets/FREE_Assets";
    private string textureFolderPath = "Assets/FREE_Assets";
    private bool searchRecursively = true;
    private bool autoFixMaterials = true;
    private string targetShader = "Standard";
    private bool convertToURP = false;

    [MenuItem("Tools/Texture Fixer")]
    public static void ShowWindow()
    {
        GetWindow<TextureFixer>("Texture Fixer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Napraw brakujące tekstury", EditorStyles.boldLabel);
        
        prefabFolderPath = EditorGUILayout.TextField("Ścieżka do prefabów:", prefabFolderPath);
        textureFolderPath = EditorGUILayout.TextField("Ścieżka do tekstur:", textureFolderPath);
        searchRecursively = EditorGUILayout.Toggle("Szukaj we wszystkich podkatalogach", searchRecursively);
        autoFixMaterials = EditorGUILayout.Toggle("Automatycznie napraw materiały", autoFixMaterials);
        
        GUILayout.Space(10);
        GUILayout.Label("Napraw błędne shadery", EditorStyles.boldLabel);
        
        convertToURP = EditorGUILayout.Toggle("Konwertuj do URP", convertToURP);
        
        if (!convertToURP)
        {
            targetShader = EditorGUILayout.TextField("Docelowy shader:", targetShader);
        }
        
        if (GUILayout.Button("Napraw błędne shadery (Hidden/InternalErrorShader)"))
        {
            FixErrorShaders();
        }

        GUILayout.Space(10);
        
        if (GUILayout.Button("Znajdź i napraw brakujące tekstury"))
        {
            FixMissingTextures();
        }
        
        if (GUILayout.Button("Konwertuj tekstury do aktualnego pipeline'u"))
        {
            ConvertTexturesToCurrentPipeline();
        }
    }

    private void FixErrorShaders()
    {
        // Znajdź wszystkie materiały w projekcie
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        int fixedCount = 0;
        
        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            if (material != null && material.shader != null)
            {
                // Sprawdź czy shader to Hidden/InternalErrorShader
                if (material.shader.name == "Hidden/InternalErrorShader")
                {
                    // Zapisz oryginalne tekstury
                    Texture mainTex = null;
                    Color color = Color.white;
                    
                    if (material.HasProperty("_MainTex"))
                        mainTex = material.GetTexture("_MainTex");
                        
                    if (material.HasProperty("_Color"))
                        color = material.GetColor("_Color");
                    
                    // Ustaw nowy shader
                    if (convertToURP)
                    {
                        material.shader = Shader.Find("Universal Render Pipeline/Lit");
                        
                        // Przypisz tekstury do właściwości URP
                        if (mainTex != null)
                            material.SetTexture("_BaseMap", mainTex);
                            
                        material.SetColor("_BaseColor", color);
                    }
                    else
                    {
                        material.shader = Shader.Find(targetShader);
                        
                        // Przypisz tekstury do właściwości standardowego shadera
                        if (mainTex != null)
                            material.SetTexture("_MainTex", mainTex);
                            
                        material.SetColor("_Color", color);
                    }
                    
                    EditorUtility.SetDirty(material);
                    fixedCount++;
                    
                    Debug.Log($"Naprawiono shader dla materiału: {material.name} w {path}");
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"Naprawiono {fixedCount} materiałów z błędnymi shaderami.");
    }

    private void FixMissingTextures()
    {
        // Znajdź wszystkie prefaby
        string[] prefabPaths = searchRecursively ? 
            Directory.GetFiles(prefabFolderPath, "*.prefab", SearchOption.AllDirectories) : 
            Directory.GetFiles(prefabFolderPath, "*.prefab");
            
        // Znajdź wszystkie tekstury
        Dictionary<string, Texture> textureDict = new Dictionary<string, Texture>();
        string[] texturePaths = searchRecursively ?
            Directory.GetFiles(textureFolderPath, "*.png", SearchOption.AllDirectories) :
            Directory.GetFiles(textureFolderPath, "*.png");
            
        // Dodaj też tekstury .jpg
        string[] jpgTexturePaths = searchRecursively ?
            Directory.GetFiles(textureFolderPath, "*.jpg", SearchOption.AllDirectories) :
            Directory.GetFiles(textureFolderPath, "*.jpg");
            
        // Połącz tablice
        List<string> allTexturePaths = new List<string>();
        allTexturePaths.AddRange(texturePaths);
        allTexturePaths.AddRange(jpgTexturePaths);
        
        // Zapisz tekstury do słownika dla szybkiego wyszukiwania
        foreach (string texturePath in allTexturePaths)
        {
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
            if (texture != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(texturePath);
                textureDict[fileName.ToLower()] = texture;
            }
        }
        
        int fixedCount = 0;
        
        // Napraw każdy prefab
        foreach (string prefabPath in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) continue;
            
            bool prefabModified = false;
            
            // Znajdź wszystkie Renderery
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];
                    if (material == null) continue;
                    
                    // Sprawdź, czy materiał ma brakujące tekstury lub błędny shader
                    if (IsMaterialMissingTextures(material) || material.shader.name == "Hidden/InternalErrorShader")
                    {
                        if (autoFixMaterials)
                        {
                            // Napraw shader jeśli jest błędny
                            if (material.shader.name == "Hidden/InternalErrorShader")
                            {
                                if (convertToURP)
                                    material.shader = Shader.Find("Universal Render Pipeline/Lit");
                                else
                                    material.shader = Shader.Find(targetShader);
                                
                                prefabModified = true;
                            }
                            
                            // Spróbuj dopasować teksturę na podstawie nazwy materiału
                            string materialName = material.name.ToLower().Replace(" ", "_").Replace("_mat", "");
                            
                            Texture matchingTexture = null;
                            
                            // Próbuj różnych wariantów nazwy
                            if (textureDict.TryGetValue(materialName, out matchingTexture) ||
                                textureDict.TryGetValue(materialName + "_diffuse", out matchingTexture) ||
                                textureDict.TryGetValue(materialName + "_albedo", out matchingTexture))
                            {
                                // Dostosuj shader i przypisz teksturę
                                if (convertToURP)
                                {
                                    material.shader = Shader.Find("Universal Render Pipeline/Lit");
                                    material.SetTexture("_BaseMap", matchingTexture);
                                }
                                else
                                {
                                    material.shader = Shader.Find(targetShader);
                                    material.SetTexture("_MainTex", matchingTexture);
                                }
                                
                                prefabModified = true;
                                fixedCount++;
                            }
                        }
                    }
                }
            }
            
            if (prefabModified)
            {
                // Zapisz zmiany w prefabie
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }
        
        Debug.Log($"Naprawiono {fixedCount} materiałów z brakującymi teksturami.");
    }
    
    private bool IsMaterialMissingTextures(Material material)
    {
        // Sprawdź dominujący kolor materiału - jeśli jest magenta, prawdopodobnie brakuje tekstury
        if (material.HasProperty("_Color"))
        {
            Color color = material.GetColor("_Color");
            if (Mathf.Approximately(color.r, 1f) && Mathf.Approximately(color.g, 0f) && Mathf.Approximately(color.b, 1f))
            {
                return true;
            }
        }
        
        // Sprawdź czy shader jest domyślnym shaderem lub czy brakuje głównej tekstury
        if (material.shader.name == "Standard" && material.HasProperty("_MainTex"))
        {
            return material.GetTexture("_MainTex") == null;
        }
        
        return false;
    }
    
    private void ConvertTexturesToCurrentPipeline()
    {
        // Znajdź wszystkie materiały
        string[] materialPaths = searchRecursively ?
            Directory.GetFiles(prefabFolderPath, "*.mat", SearchOption.AllDirectories) :
            Directory.GetFiles(prefabFolderPath, "*.mat");
            
        int convertedCount = 0;
        
        foreach (string materialPath in materialPaths)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null) continue;
            
            // Sprawdź, czy materiał używa niewspieranego shadera
            if (material.shader.name.Contains("Standard") || 
                material.shader.name.Contains("Legacy") || 
                material.shader.name == "Hidden/InternalErrorShader")
            {
                // Zapamiętaj oryginalne tekstury
                Texture mainTex = null;
                Texture normalMap = null;
                Texture metallicMap = null;
                Color color = Color.white;
                
                if (material.HasProperty("_MainTex"))
                    mainTex = material.GetTexture("_MainTex");
                    
                if (material.HasProperty("_BumpMap"))
                    normalMap = material.GetTexture("_BumpMap");
                    
                if (material.HasProperty("_MetallicGlossMap"))
                    metallicMap = material.GetTexture("_MetallicGlossMap");
                    
                if (material.HasProperty("_Color"))
                    color = material.GetColor("_Color");
                
                // Zmień shader na URP/Lit
                material.shader = Shader.Find("Universal Render Pipeline/Lit");
                
                // Przypisz tekstury do nowych właściwości
                if (mainTex != null)
                    material.SetTexture("_BaseMap", mainTex);
                    
                if (normalMap != null)
                    material.SetTexture("_BumpMap", normalMap);
                    
                if (metallicMap != null)
                    material.SetTexture("_MetallicGlossMap", metallicMap);
                
                material.SetColor("_BaseColor", color);
                
                EditorUtility.SetDirty(material);
                convertedCount++;
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"Przekonwertowano {convertedCount} materiałów do aktualnego potoku graficznego.");
    }
}