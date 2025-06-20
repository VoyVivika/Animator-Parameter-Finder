#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Animations;

namespace Voy.AviParamFinder
{
    public class AnimatorParameterFinder : EditorWindow
    {
        const string VERSION = "1.0.1";
        const string CREDIT = "Animator Parameter Finder by VoyVivika";
        const string GITLINK = "(https://github.com/VoyVivika/Animator-Parameter-Finder)";
        
        enum ErrorType
        {
            NONE = 0,
            EMPTY_ANIMATOR = 1,
            EMPTY_PARAMETER = 2,
            NOT_FOUND = 3
        }

        ErrorType currentError = ErrorType.NONE;
        AnimatorController animator;
        string parameter = "";
        List<string> locations = null;
        bool findParam = false;
        Vector2 scroll = new Vector2();
        bool findAndReplace = false;
        string rParameter = "";

        [MenuItem("Voy/Animator Parameter Finder")]
        public static void ShowUI()
        {
            EditorWindow wnd = GetWindow<AnimatorParameterFinder>();
            wnd.titleContent = new GUIContent("Parameter Finder");
        }

        public void OnGUI()
        {
            EditorGUILayout.HelpBox(CREDIT + "\n" + VERSION + " " + GITLINK, MessageType.Info);
            animator = (AnimatorController)EditorGUILayout.ObjectField(GUIContent.none, animator, typeof(AnimatorController), false);
            EditorGUILayout.Space();
            GUILayout.Label("Parameter to Find");
            parameter = EditorGUILayout.TextField(parameter);

            EditorGUILayout.Space();
            //findAndReplace = GUILayout.Toggle(findAndReplace, "Find and Replace");

            string findParamText = "Find Parameter";

            if(findAndReplace)
            {
                GUILayout.Label("Replacement Parameter");
                rParameter = EditorGUILayout.TextField(rParameter);
                if (findAndReplace && rParameter != null && rParameter != "") findParamText = "Find & Replace Parameter";
                EditorGUILayout.Space();
            }

            findParam = (GUILayout.Button(findParamText));


            if (findParam)
            {
                findParam = false;
                //Debug.Log("Looking for Parameter: " + parameter);
                findParameter();
            }

            EditorGUILayout.Space();

            if (locations == null || locations.Count <= 0)
            {
                switch (currentError)
                {
                    case ErrorType.EMPTY_ANIMATOR:
                        EditorGUILayout.HelpBox("Please Select an Animator to Search Through.", MessageType.Warning);
                        break;
                    case ErrorType.EMPTY_PARAMETER:
                        EditorGUILayout.HelpBox("Please Enter the Name of a Parameter to Find in the Animator.", MessageType.Warning);
                        break;
                    case ErrorType.NOT_FOUND:
                        EditorGUILayout.HelpBox("Could not find the Parameter in this Animator.", MessageType.Error);
                        break;
                    default:
                        EditorGUILayout.HelpBox("This List is Currently Empty. Select an Animator & Enter a Parameter Name and then Press the Find Button Above.", MessageType.Info );
                        break;
                }
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
            locations = null;

            if (animator == null)
            {
                currentError = ErrorType.EMPTY_ANIMATOR;
                return;
            }

            if (parameter == null || parameter == "")
            {
                currentError = ErrorType.EMPTY_PARAMETER;
                return;
            }

            // The things I have to do for corountines.
            // Update: this works, this feels really hacky but this is pretty much the only option I have.
            GameObject gameObject = new GameObject();
            Parser parser = gameObject.AddComponent<Parser>();
            parser.animator = animator;
            parser.parameter = parameter;

            if(findAndReplace && rParameter != "")
            {
                parser.rParam = rParameter;
                parser.findAndReplace = true;
            }

            //Parser parser = new Parser(animator, parameter, maxBlendCount); // removed because I needed coroutine support.
            if (parser == null) Debug.Log("WHY IS THIS NULL!?! I JUST MADE IT!?!");
            if (parser.parse() != null) locations = parser.GetLocations();
            else
            {
                //Debug.Log("Could not find usage of parameter :(");
                currentError = ErrorType.NOT_FOUND;
            }

            // time to delete this garbage I had to add for coroutine support.
            DestroyImmediate(parser);
            DestroyImmediate(gameObject);
        }
    }
}

#endif