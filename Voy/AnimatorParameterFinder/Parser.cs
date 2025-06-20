#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace Voy.AviParamFinder
{
    public class Parser : MonoBehaviour
    {
        [HideInInspector]
        public AnimatorController animator;

        [HideInInspector]
        public string parameter = "";

        [HideInInspector]
        public string rParam = "";

        [HideInInspector]
        public bool findAndReplace = false;

        private int replacedParameter = -1;

        private List<string> foundLocations;
        private AnimatorControllerParameterType type;
        //private byte blendTreeDepthExceededCount = 0;

        public List<string> GetLocations()
        {
            return foundLocations;
        }

        public List<string> parse()
        {
            //Debug.Log("Parsing Initated");

            if (animator == null)
            {
                //Debug.Log("Animator not set, cannot parse.");
                return null;
            }

            if (parameter == null)
            {
                //Debug.Log("Parameter is null... how even?");
                return null;
            }

            if (parameter == "")
            {
                Debug.LogWarning("Parameter is empty.");
                return null;
            }

            {
                bool hasParam = false;
                //int i = 0;

                for (int i = 0; i < animator.parameters.Length; i++)
                //foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    if (animator.parameters[i].name == parameter)
                    {
                        type = animator.parameters[i].type;
                        hasParam = true;

                        if (findAndReplace)
                        {
                            animator.AddParameter(rParam, animator.parameters[i].type);
                            replacedParameter = i;
                        }

                        break;
                    }
                    //i++;
                }

                if (hasParam == false)
                {
                    //Debug.LogWarning("Parameter is not in this Animator Controller.");
                    return null;
                }

                if (findAndReplace)
                {
                    animator.RemoveParameter(replacedParameter);
                }
            }

            foundLocations = new List<string>();

            //Debug.Log("Beginning Search for " + parameter);

            // youtu.be/QV-DZtN2IMU

            for (int iLayer = 0; iLayer < animator.layers.Length; iLayer++)
            //foreach (AnimatorControllerLayer layer in animator.layers)
            {
                string currentLocation = animator.layers[iLayer].name;
                //Debug.Log("parsing layer: " + currentLocation);
                AnimatorStateMachine stateMachine = animator.layers[iLayer].stateMachine;
                StartCoroutine(parseStateMachine(stateMachine, currentLocation, true));

            }

            //Debug.Log(foundLocations.Count);

            return foundLocations;
        }

        private IEnumerator parseStateMachine(AnimatorStateMachine stateMachine, string location, bool isRoot = false)
        {
            string currentLocation = location;

            //Debug.Log(location + " SM: AST:" + stateMachine.anyStateTransitions.Length.ToString() + ", ET:" + stateMachine.entryTransitions.Length.ToString() + ", SM:" + stateMachine.stateMachines.Length.ToString() + ", S:" + stateMachine.states.Length.ToString());


            if (!isRoot)
            {
                currentLocation = location + "/" + stateMachine.name;
            }

            ////Debug.Log("parsing stateMachine: " + currentLocation);

            parseAnystateTransitions(ref stateMachine, currentLocation);

            parseEntryTransitions(stateMachine, currentLocation);

            parseStates(stateMachine.states, currentLocation);

            yield return null;
            ////Debug.Log("Returned from yield");

            foreach (ChildAnimatorStateMachine childMachine in stateMachine.stateMachines)
            {
                parseStateMachine(childMachine.stateMachine, currentLocation);
            }

            yield break;
        }

        private void parseStates(ChildAnimatorState[] states, string location)
        {
            for (int idx = 0; idx < states.Length; idx++)
            //foreach (ChildAnimatorState state in states)
            {
                if (states[idx].state != null)
                    parseState(states[idx].state, location);
                else
                {
                    //Debug.Log(location + ": somehow, this state is null.");
                }

            }
        }

        private void parseState(AnimatorState state, string location)
        {
            string currentLocation = location + "/" + state.name;

            if (type == AnimatorControllerParameterType.Float)
            {
                if (state.motion != null)
                {
                    if (state.motion.GetType() == typeof(BlendTree))
                    {
                        //Debug.Log(currentLocation + ": BlendTree Found.");
                        parseBlendTree((BlendTree)state.motion, currentLocation, true);
                        return;
                    }

                    if (state.cycleOffsetParameter == parameter)
                    {
                        foundLocations.Add(currentLocation + "/Cycle Offset (" + btoo(state.cycleOffsetParameterActive) + ")");
                        if (findAndReplace) state.cycleOffsetParameter = rParam;
                    }
                    if (state.mirrorParameter == parameter)
                    {
                        foundLocations.Add(currentLocation + "/Mirror (" + btoo(state.mirrorParameterActive) + ")");
                        if (findAndReplace) state.mirrorParameter = rParam;
                    }
                    if (state.speedParameter == parameter)
                    {
                        foundLocations.Add(currentLocation + "/Speed (" + btoo(state.speedParameterActive) + ")");
                        if (findAndReplace) state.speedParameter = rParam;
                    }
                    if (state.timeParameter == parameter)
                    {
                        foundLocations.Add(currentLocation + "/Time (" + btoo(state.timeParameterActive) + ")");
                        if (findAndReplace) state.timeParameter = rParam;
                    }
                }
            }

            //int bIdx = 0;
            
            for (int bIdx = 0; bIdx < state.behaviours.Length; bIdx++)
            //foreach (StateMachineBehaviour behaviour in state.behaviours)
            {
                string behaviourLocation = (currentLocation + "/Behaviour " + bIdx + "/");

#if VRC_SDK_VRCSDK3

                if (behaviour.GetType() == typeof(VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver))
                {
                    VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver driver = (VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver)behaviour;

                    foreach (VRC.SDKBase.VRC_AvatarParameterDriver.Parameter param in driver.parameters)
                    {
                        if (param.name == parameter || param.source == parameter)
                            foundLocations.Add(behaviourLocation + "VRC Avatar Parameter Driver");
                    }
                }

#endif

#if CVR_CCK_EXISTS

                if (state.behaviours[bIdx].GetType() == typeof(ABI.CCK.Components.AnimatorDriver))
                {
                    ABI.CCK.Components.AnimatorDriver driver = (ABI.CCK.Components.AnimatorDriver)state.behaviours[bIdx];

                    for (int tIdx = 0; tIdx < driver.EnterTasks.Count; tIdx++)
                    //foreach (ABI.CCK.Components.AnimatorDriverTask task in driver.EnterTasks)
                    {
                        bool isAName = driver.EnterTasks[tIdx].aName == parameter;
                        bool isBName = driver.EnterTasks[tIdx].bName == parameter;
                        bool isCName = driver.EnterTasks[tIdx].cName == parameter;
                        bool isTargetName = driver.EnterTasks[tIdx].targetName == parameter;

                        if (isAName || isBName || isCName || isTargetName)
                        {
                            foundLocations.Add(behaviourLocation + "CVR Animator Driver");
                        }

                        if (findAndReplace)
                        {
                            if (isAName) driver.EnterTasks[tIdx].aName = rParam;
                            if (isBName) driver.EnterTasks[tIdx].bName = rParam;
                            if (isCName) driver.EnterTasks[tIdx].cName = rParam;
                            if (isTargetName) driver.EnterTasks[tIdx].targetName = rParam;
                        }
                    }
                }

#endif
                //bIdx++;
            }

            //Debug.Log("ST:" + state.transitions.Length.ToString());

            for (int idx = 0; idx < state.transitions.Length; idx++)
            //foreach (AnimatorStateTransition transition in state.transitions)
            {
                string transitionLocation = currentLocation + "/transition " + idx;
                if (parseStateTransiton(ref state.transitions[idx])) foundLocations.Add(transitionLocation);
            }
        }

        // Bool to On/Off
        private string btoo(bool b)
        {
            if (b) return "On";

            return "Off";
        }

        private void parseBlendTree(BlendTree blendTree, string location, bool isRoot = false)
        {
            if (blendTree == null) return;

            string currentLocation = location + "/" + blendTree.name;

            if (isRoot) currentLocation = location;

            bool isUsedHere = false;

            if (blendTree.blendParameter == parameter) isUsedHere = true;
            if (blendTree.blendParameterY == parameter) isUsedHere = true;

            if (isUsedHere) foundLocations.Add(location);

            //yield return null;

            foreach (ChildMotion childBlend in blendTree.children)
            {
                if (childBlend.directBlendParameter == parameter)
                {
                    foundLocations.Add(currentLocation + "/" + childBlend.motion.name);
                }

                if (childBlend.motion.GetType() == typeof(BlendTree))
                {
                    //blendTreeDepth++;
                    parseBlendTree(blendTree, location);

                }

            }

        }

        private void parseAnystateTransitions(ref AnimatorStateMachine stateMachine, string location)
        {
            //int transitionIndex = 0;

            for (int transitionIndex = 0; transitionIndex < stateMachine.anyStateTransitions.Length; transitionIndex++)
            //foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
            {
                string currentLocation = location + "/transition " + transitionIndex;
                //Debug.Log("parsing: " + currentLocation);
                if (parseStateTransiton(ref stateMachine.anyStateTransitions[transitionIndex]))
                {
                    //Debug.Log("found!");
                    foundLocations.Add(currentLocation);
                }
            }
        }
        private void parseEntryTransitions(AnimatorStateMachine stateMachine, string location)
        {
            int transitionIndex = 1;
            foreach (AnimatorTransition transition in stateMachine.entryTransitions)
            {
                string currentLocation = location + "/entry transition " + transitionIndex;
                //Debug.Log("parsing: " + currentLocation);
                if (parseTranstion(transition))
                {
                    //Debug.Log("found!");
                    foundLocations.Add(currentLocation);
                }
                transitionIndex++;
            }
        }

        private bool parseStateTransiton(ref AnimatorStateTransition transition)
        {
            bool result = false;

            for (int conIndex = 0; conIndex < transition.conditions.Length; conIndex++)
            //foreach (AnimatorCondition condition in transition.conditions)
            {
                result = parseAnimatorCondition(ref transition.conditions[conIndex]);
            }

            return result;
        }

        private bool parseTranstion(AnimatorTransition transition)
        {
            bool result = false;

            for (int idx = 0; idx < transition.conditions.Length; idx++)
            //foreach (AnimatorCondition condition in transition.conditions)
            {
                result = parseAnimatorCondition(ref transition.conditions[idx]);
            }

            return result;
        }

        private bool parseAnimatorCondition(ref AnimatorCondition condition)
        {
            if (condition.parameter == parameter)
            {
                if(findAndReplace)
                {
                    condition.parameter = rParam;
                }
                return true;
            }

            return false;
        }

    }
}

#endif