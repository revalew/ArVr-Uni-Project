using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class PrefabFixer : EditorWindow
{
    private string prefabFolderPath = "Assets";
    private bool recursive = true;
    private bool showLog = true;
    private Vector2 scrollPos;
    private List<string> logMessages = new List<string>();

    [MenuItem("Tools/Prefab Fixer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabFixer>("Prefab Fixer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Naprawianie uszkodzonych prefabów", EditorStyles.boldLabel);
        
        prefabFolderPath = EditorGUILayout.TextField("Folder z prefabami:", prefabFolderPath);
        recursive = EditorGUILayout.Toggle("Wyszukiwanie rekursywne:", recursive);
        showLog = EditorGUILayout.Toggle("Pokaż szczegółowy log:", showLog);
        
        if (GUILayout.Button("Napraw problematyczne prefaby"))
        {
            FixPrefabs();
        }
        
        if (GUILayout.Button("Usuń brakujące komponenty z prefabów"))
        {
            RemoveMissingScripts();
        }

        if (GUILayout.Button("Napraw referencje GameObjectInspector"))
        {
            FixGameObjectInspectorReferences();
        }
        
        if (showLog && logMessages.Count > 0)
        {
            GUILayout.Label("Log:", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
            foreach (string message in logMessages)
            {
                GUILayout.Label(message);
            }
            EditorGUILayout.EndScrollView();
            
            if (GUILayout.Button("Wyczyść log"))
            {
                logMessages.Clear();
            }
        }
    }
    
    private void AddLog(string message)
    {
        if (showLog)
        {
            logMessages.Add(message);
            Debug.Log(message);
        }
    }
    
    private void FixPrefabs()
    {
        logMessages.Clear();
        AddLog("Rozpoczynam naprawę prefabów...");
        
        // Znajdź wszystkie prefaby
        string[] prefabPaths = GetPrefabPaths();
        AddLog($"Znaleziono {prefabPaths.Length} prefabów do sprawdzenia.");
        
        int fixedCount = 0;
        
        foreach (string prefabPath in prefabPaths)
        {
            try
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                
                if (prefab == null)
                {
                    AddLog($"Błąd: Nie można załadować prefabu: {prefabPath}");
                    continue;
                }
                
                bool prefabModified = false;
                
                // Sprawdź uszkodzone materiały
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    if (renderer == null) continue;
                    
                    Material[] materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        Material material = materials[i];
                        if (material != null && material.shader != null && material.shader.name == "Hidden/InternalErrorShader")
                        {
                            materials[i] = new Material(Shader.Find("Standard"));
                            prefabModified = true;
                            AddLog($"Naprawiono uszkodzony materiał w prefabie: {prefabPath}");
                        }
                    }
                    
                    if (prefabModified)
                    {
                        renderer.sharedMaterials = materials;
                    }
                }
                
                // Sprawdź uszkodzone animatory
                Animator[] animators = prefab.GetComponentsInChildren<Animator>(true);
                foreach (Animator animator in animators)
                {
                    if (animator == null) continue;
                    
                    if (animator.runtimeAnimatorController == null)
                    {
                        AddLog($"Znaleziono animator bez kontrolera w prefabie: {prefabPath}");
                        
                        // Wyłącz animator zamiast usuwać
                        animator.enabled = false;
                        prefabModified = true;
                        AddLog($"Wyłączono animator bez kontrolera w prefabie: {prefabPath}");
                    }
                }
                
                // Sprawdź problemy z transformacjami
                Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
                foreach (Transform transform in transforms)
                {
                    // Sprawdź, czy transform ma prawidłowego rodzica
                    if (transform != prefab.transform && transform.parent == null)
                    {
                        transform.SetParent(prefab.transform);
                        prefabModified = true;
                        AddLog($"Naprawiono osierocony transform w prefabie: {prefabPath}");
                    }
                    
                    // Sprawdź skalę - jeśli jest zerowa, ustaw na 1
                    if (transform.localScale == Vector3.zero)
                    {
                        transform.localScale = Vector3.one;
                        prefabModified = true;
                        AddLog($"Naprawiono zerową skalę transformu w prefabie: {prefabPath}");
                    }
                }
                
                // Zapisz zmiany w prefabie jeśli były modyfikacje
                if (prefabModified)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                    fixedCount++;
                }
            }
            catch (System.Exception e)
            {
                AddLog($"Błąd podczas przetwarzania prefabu {prefabPath}: {e.Message}");
            }
        }
        
        AddLog($"Zakończono naprawę prefabów. Naprawiono {fixedCount} prefabów.");
        AssetDatabase.Refresh();
    }
    
    private void RemoveMissingScripts()
    {
        logMessages.Clear();
        AddLog("Rozpoczynam usuwanie brakujących skryptów...");
        
        // Znajdź wszystkie prefaby
        string[] prefabPaths = GetPrefabPaths();
        AddLog($"Znaleziono {prefabPaths.Length} prefabów do sprawdzenia.");
        
        int totalRemovedComponents = 0;
        int fixedPrefabsCount = 0;
        
        foreach (string prefabPath in prefabPaths)
        {
            try
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                
                if (prefab == null)
                {
                    AddLog($"Błąd: Nie można załadować prefabu: {prefabPath}");
                    continue;
                }
                
                int removedComponents = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab);
                
                if (removedComponents > 0)
                {
                    totalRemovedComponents += removedComponents;
                    fixedPrefabsCount++;
                    PrefabUtility.SavePrefabAsset(prefab);
                    AddLog($"Usunięto {removedComponents} brakujących skryptów z prefabu: {prefabPath}");
                }
            }
            catch (System.Exception e)
            {
                AddLog($"Błąd podczas przetwarzania prefabu {prefabPath}: {e.Message}");
            }
        }
        
        AddLog($"Zakończono usuwanie brakujących skryptów. Usunięto {totalRemovedComponents} skryptów z {fixedPrefabsCount} prefabów.");
        AssetDatabase.Refresh();
    }

    private void FixGameObjectInspectorReferences()
    {
        logMessages.Clear();
        AddLog("Naprawianie błędów w GameObjectInspector...");
        
        // Wykonaj czyszczenie inspektora przez wybranie pustego obiektu
        Selection.activeGameObject = null;
        
        // Odśwież wszystkie widoki edytora
        EditorUtility.RequestScriptReload();
        
        // Wymus regenerację inspektorów
        foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
        {
            if (window.titleContent.text == "Inspector")
            {
                window.Repaint();
            }
        }
        
        AddLog("Zakończono naprawianie błędów w GameObjectInspector. Zrestartuj edytor Unity, jeśli błędy nadal występują.");
    }
    
    private string[] GetPrefabPaths()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", recursive ? new[] { prefabFolderPath } : null);
        string[] prefabPaths = new string[prefabGuids.Length];
        
        for (int i = 0; i < prefabGuids.Length; i++)
        {
            prefabPaths[i] = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
        }
        
        return prefabPaths;
    }
}