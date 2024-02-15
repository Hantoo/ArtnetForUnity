using System;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]

public class ArtPackageImport : MonoBehaviour
{

    static AddRequest JsonRequest;
    static AddRequest EditorCoRequest;
    static ListRequest ListPackages;
    // Start is called before the first frame update
    static ArtPackageImport()
    {
        ListPackages = Client.List();
        EditorApplication.update += ListProgress;
        JsonRequest = Client.Add("com.unity.nuget.newtonsoft-json");
        EditorCoRequest = Client.Add("com.unity.editorcoroutines");
       
    }

    static void ListProgress()
    {
        if (ListPackages.IsCompleted)
        {
            bool HasJson = false;
            bool HasEditorCoRoutines = false;
            if (ListPackages.Status == StatusCode.Success)
                foreach (var package in ListPackages.Result) {
                    if (package.name == "com.unity.nuget.newtonsoft-json") HasJson = true;
                    if (package.name == "com.unity.editorcoroutines") HasEditorCoRoutines = true;
                        }
            
            else if (ListPackages.Status >= StatusCode.Failure)
                Debug.Log(ListPackages.Error.message);

            if(!HasJson)  EditorApplication.update += JsonProgress;
            if(!HasEditorCoRoutines)  EditorApplication.update += EditorCoRProgress;
                  
                EditorApplication.update -= ListProgress;

        }
    }
    static void JsonProgress()
    {
        if (JsonRequest.IsCompleted)
        {
            if (JsonRequest.Status == StatusCode.Success)
                Debug.Log("Installed: " + JsonRequest.Result.packageId);
            else if (JsonRequest.Status >= StatusCode.Failure)
                Debug.Log(JsonRequest.Error.message);

            EditorApplication.update -= JsonProgress;
        }
    }    
    static void EditorCoRProgress()
    {
        if (EditorCoRequest.IsCompleted)
        {
            if (EditorCoRequest.Status == StatusCode.Success)
                Debug.Log("Installed: " + EditorCoRequest.Result.packageId);
            else if (EditorCoRequest.Status >= StatusCode.Failure)
                Debug.Log(EditorCoRequest.Error.message);

            EditorApplication.update -= EditorCoRProgress;
        }
    }

    // Update is called once per frame

}
