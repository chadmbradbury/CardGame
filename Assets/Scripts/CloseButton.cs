using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class CloseButton : MonoBehaviour
{
    public void CloseApp()
    {
        #if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
        {
            EditorApplication.ExitPlaymode();
        }
        #endif

        Application.Quit();
    }

    void Update()
    {
        if (Input.GetKey("escape"))
        {
            #if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
            {
                EditorApplication.ExitPlaymode();
            }
            #endif

            Application.Quit();
        }
    }
}
