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

        AnimatorController animator;
        string parameter = "Parameter Name";
        List<string> locations = null;
        bool findParam = false;
        Vector2 scroll = new Vector2();
        byte maxBlendCount = 16;

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
            parameter = EditorGUILayout.TextField(parameter);

            /*
            GUILayout.Label("Maximum Blend Tree Depth: "+ maxBlendCount);
            maxBlendCount = (byte)GUILayout.HorizontalSlider((float)maxBlendCount, 0, 255);
            EditorGUILayout.Space(16);
            
            if(maxBlendCount >= 32)
            {
                EditorGUILayout.HelpBox("You're setting this kind of high. Having this value too high may stall Unity for too long and cause it to close/crash if your Animator makes heavy usage of BlendTrees, especially Direct Blend Trees!", MessageType.Warning);
            }

            EditorGUILayout.Space(16);
            */

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

            // The things I have to do for corountines.
            // Update: this works, this feels really hacky but this is pretty much the only option I have.
            GameObject gameObject = new GameObject();
            Parser parser = gameObject.AddComponent<Parser>();
            parser.animator = animator; parser.parameter = parameter;

            //Parser parser = new Parser(animator, parameter, maxBlendCount); // removed because I needed coroutine support.
            if (parser == null) Debug.Log("WHY IS THIS NULL!?! I JUST MADE IT!?!");
            if (parser.parse() != null) locations = parser.GetLocations();
            else Debug.Log("Could not find usage of parameter :(");

            // time to delete this garbage I had to add for coroutine support.
            DestroyImmediate(parser);
            DestroyImmediate(gameObject);
        }
    }
}

#endif