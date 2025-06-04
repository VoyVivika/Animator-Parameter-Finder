using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Animations;

#if UNITY_EDITOR

namespace Voy.AviParamFinder
{
    public class AnimatorParameterFinder : EditorWindow
    {
        const string VERSION = "1.0.0";
        const string CREDIT = "AnimatorParameterFinder by VoyVivika";

        AnimatorController animator;
        string parameter = "Parameter Name";
        List<string> locations = null;
        bool findParam = false;
        Vector2 scroll = new Vector2();

        [MenuItem("Voy/Animator Parameter Finder")]
        public static void ShowUI()
        {
            EditorWindow wnd = GetWindow<AnimatorParameterFinder>();
            wnd.titleContent = new GUIContent("Parameter Finder");
        }

        public void OnGUI()
        {
            EditorGUILayout.HelpBox("This Utility Aims to help find where a Parameter is being used in your Unity Animator", MessageType.Info);
            animator = (AnimatorController)EditorGUILayout.ObjectField(GUIContent.none, animator, typeof(AnimatorController), false);
            parameter = EditorGUILayout.TextField(parameter);
            EditorGUILayout.Space();

            findParam = (GUILayout.Button("Find Parameter"));
            
            if (findParam)
            {
                findParam = false;
                Debug.Log("Looking for Parameter: " + parameter);
                findParameter();
            }

            EditorGUILayout.Space();

            if (locations == null || locations.Count <= 0)
            {
                EditorGUILayout.HelpBox("This List is Currently Empty. Select an Animator & Enter a Parameter Name and then Press the Find Button Above.", MessageType.Info);
            }
            else
            {
                scroll = GUILayout.BeginScrollView(scroll, true, false);

                foreach (string location in locations)
                {
                    EditorGUILayout.HelpBox(location, MessageType.None);
                }

                GUILayout.EndScrollView();
            }
        }

        public void findParameter()
        {
            Parser parser = new Parser(animator, parameter);
            if (parser == null) Debug.Log("WHY IS THIS NULL!?! I JUST MADE IT!?!");
            if (parser.parse() != null) locations = parser.GetLocations();
            else Debug.Log("Could not find usage of parameter :(");
        }
    }
}

#endif