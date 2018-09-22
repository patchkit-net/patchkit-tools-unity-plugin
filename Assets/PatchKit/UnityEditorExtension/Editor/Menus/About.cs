﻿using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PatchKit.UnityEditorExtension.Menus
{
public class About : EditorWindow
{
    [MenuItem("Tools/PatchKit/About", false, -50)]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow(typeof(About), true, "About");
        Assert.IsNotNull(window);
        window.maxSize = new Vector2(400, 400);
        window.minSize = window.minSize;
    }

    public Texture Logo;

    private void OnGUI()
    {
        GUILayout.Label("\n");
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label(Logo, GUILayout.Height(150), GUILayout.Width(307));
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Label("About", EditorStyles.boldLabel);
        GUILayout.Label(
            "The PatchKit Unity Editor Extension is a Unity tool to automatic\n" +
            "application distribution directly from the Unity Editor.\n\n" +
            "Provide fast, comfortable and safe connection with API PatchKit.\n" +
            "The Extension allows uploading without leaving the editor, all you\n" +
            "have to do is log in and make the build. After that, version is\n" +
            "being sent to PatchKit.",
            GUILayout.Width(380));

        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            {
                if (GUILayout.Button("Contact", GUILayout.Width(170)))
                {
                    Application.OpenURL(
                        "http://docs.patchkit.net/contact.html");
                }

                if (GUILayout.Button("Documentation", GUILayout.Width(170)))
                {
                    Application.OpenURL("http://docs.patchkit.net/");
                }
            }
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Label("\nVersion 1.0.0", EditorStyles.centeredGreyMiniLabel);
    }
}
}