using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

#if UNITY_EDITOR

namespace Voy.AviParamFinder
{
    public class Parser
    {
        private AnimatorController animator;
        private string parameter;
        private List<string> foundLocations;
        private byte blendTreeDepth = 0;
        private AnimatorControllerParameterType type;
        private byte blendTreeDepthExceededCount = 0;
        private bool blendTreeDepthExceeded = false;
        private byte BlendTreeMaxDepth = 16;

        public List<string> GetLocations()
        {
            return foundLocations;
        }

        public Parser(AnimatorController anim, string param, byte maxBlendCount)
        {
            animator = anim;
            parameter = param;
            BlendTreeMaxDepth = maxBlendCount;
            Debug.Log("Parser Created");
        }

        public List<string> parse()
        {
            Debug.Log("Parsing Initated");

            if (animator == null)
            {
                Debug.Log("Animator not set, cannot parse.");
                return null;
            }

            if (parameter == null)
            {
                Debug.Log("Parameter is null... how even?");
                return null;
            }

            // I'm not bothering to add more than 5 spaces, if you have a better way of handling this, let me know.
            if (parameter == "" || parameter == " " || parameter == "   " || parameter == "    ")
            {
                Debug.Log("Parameter is empty, not bothering to parse nothing.");
                return null;
            }

            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == parameter) type = param.type;
            }

            foundLocations = new List<string>();

            Debug.Log("Beginning Search for " + parameter);

            // youtu.be/QV-DZtN2IMU

            foreach (AnimatorControllerLayer layer in animator.layers)
            {
                string currentLocation = layer.name;
                Debug.Log("parsing layer: " + currentLocation);
                AnimatorStateMachine stateMachine = layer.stateMachine;
                parseStateMachine(stateMachine, currentLocation, true);

            }

            Debug.Log(foundLocations.Count);

            return foundLocations;
        }

        private void parseStateMachine(AnimatorStateMachine stateMachine, string location, bool isRoot = false)
        {
            string currentLocation = location;

            Debug.Log(location + " SM: AST:" + stateMachine.anyStateTransitions.Length.ToString() + ", ET:" + stateMachine.entryTransitions.Length.ToString() + ", SM:" + stateMachine.stateMachines.Length.ToString() + ", S:" + stateMachine.states.Length.ToString());


            if (!isRoot)
            {
                currentLocation = location + "/" + stateMachine.name;
            }

            //Debug.Log("parsing stateMachine: " + currentLocation);

            parseAnystateTransitions(stateMachine, currentLocation);

            parseEntryTransitions(stateMachine, currentLocation);

            parseStates(stateMachine.states, currentLocation);

            foreach (ChildAnimatorStateMachine childMachine in stateMachine.stateMachines)
            {
                parseStateMachine(childMachine.stateMachine, currentLocation);
            }
        }

        private void parseStates(ChildAnimatorState[] states, string location)
        {
            foreach (ChildAnimatorState state in states)
            {
                if (state.state != null)
                    parseState(state.state, location);
                else
                    Debug.Log(location + ": somehow, this state is null.");
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
                        Debug.Log(currentLocation + ": BlendTree Found.");
                        parseBlendTree((BlendTree)state.motion, currentLocation, true);
                        if (blendTreeDepth >= BlendTreeMaxDepth) blendTreeDepthExceededCount++;
                        blendTreeDepth = 0;
                        blendTreeDepthExceeded = false;
                        return;
                    }

                    if (state.cycleOffsetParameter == parameter) foundLocations.Add(currentLocation + "/Cycle Offset (" + btoo(state.cycleOffsetParameterActive) + ")");
                    if (state.mirrorParameter == parameter) foundLocations.Add(currentLocation + "/Mirror (" + btoo(state.mirrorParameterActive) + ")");
                    if (state.speedParameter == parameter) foundLocations.Add(currentLocation + "/Speed (" + btoo(state.speedParameterActive) + ")");
                    if (state.timeParameter == parameter) foundLocations.Add(currentLocation + "/Time (" + btoo(state.timeParameterActive) + ")");
                }
            }

            int bIdx = 0;
            foreach (StateMachineBehaviour behaviour in state.behaviours)
            {
                string behaviourLocation = (currentLocation + "/Behaviour " + bIdx + "/");

#if VRC_SDK_VRCSDK3

                if (behaviour.GetType() == typeof(VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver))
                {
                    VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver driver = (VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver)behaviour;

                    foreach(VRC.SDKBase.VRC_AvatarParameterDriver.Parameter param in driver.parameters)
                    {
                        if(param.name == parameter || param.source == parameter)
                            foundLocations.Add(behaviourLocation + "VRC Avatar Parameter Driver");
                    }
                }

#endif

#if CVR_CCK_EXISTS

                if (behaviour.GetType() == typeof(ABI.CCK.Components.AnimatorDriver))
                {
                    ABI.CCK.Components.AnimatorDriver driver = (ABI.CCK.Components.AnimatorDriver)behaviour;

                    foreach (ABI.CCK.Components.AnimatorDriverTask task in driver.EnterTasks)
                    {
                        if (task.aName == parameter || task.bName == parameter || task.cName == parameter || task.targetName == parameter)
                            foundLocations.Add(behaviourLocation + "CVR Animator Driver");
                    }
                }

#endif
                if (bIdx < int.MaxValue) bIdx++;
            }

            Debug.Log("ST:" + state.transitions.Length.ToString());
            int idx = 0;
            foreach (AnimatorStateTransition transition in state.transitions)
            {
                string transitionLocation = currentLocation + "/transition " + idx;
                if (parseStateTransiton(transition)) foundLocations.Add(transitionLocation);
                idx++;
            }
        }

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

            if (blendTreeDepthExceeded) return;

            foreach (ChildMotion childBlend in blendTree.children)
            {
                if (childBlend.directBlendParameter == parameter)
                {
                    foundLocations.Add(location + "/" + childBlend.motion.name);
                }

                if (childBlend.motion.GetType() == typeof(BlendTree))
                {
                    if (blendTreeDepth < BlendTreeMaxDepth)
                    {
                        blendTreeDepth++;
                        parseBlendTree(blendTree, location);
                    }
                    else
                    {
                        Debug.Log("BlendTree Depth Exceeded! We are not continuing!");
                        blendTreeDepthExceeded = true;
                        continue;
                    }
                }

            }


        }

        private void parseAnystateTransitions(AnimatorStateMachine stateMachine, string location)
        {
            int transitionIndex = 0;
            foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
            {
                string currentLocation = location + "/transition " + transitionIndex;
                Debug.Log("parsing: " + currentLocation);
                if (parseStateTransiton(transition))
                {
                    Debug.Log("found!");
                    foundLocations.Add(currentLocation);
                }
                transitionIndex++;
            }
        }
        private void parseEntryTransitions(AnimatorStateMachine stateMachine, string location)
        {
            int transitionIndex = 1;
            foreach (AnimatorTransition transition in stateMachine.entryTransitions)
            {
                string currentLocation = location + "/entry transition " + transitionIndex;
                Debug.Log("parsing: " + currentLocation);
                if (parseTranstion(transition))
                {
                    Debug.Log("found!");
                    foundLocations.Add(currentLocation);
                }
                transitionIndex++;
            }
        }

        private bool parseStateTransiton(AnimatorStateTransition transition)
        {
            bool result = false;
            foreach (AnimatorCondition condition in transition.conditions)
            {
                result = parseAnimatorCondition(condition);
            }

            return result;
        }

        private bool parseTranstion(AnimatorTransition transition)
        {
            bool result = false;
            foreach (AnimatorCondition condition in transition.conditions)
            {
                result = parseAnimatorCondition(condition);
            }

            return result;
        }

        private bool parseAnimatorCondition(AnimatorCondition condition)
        {
            if (condition.parameter == parameter)
            {
                return true;
            }

            return false;
        }

    }
}

#endif