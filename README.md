# Animator Parameter Finder
This Utility will Seek Through a Provided Unity Animator Controller and try to find any usage of a Parameter of the Specified Name. This is useful if you are trying to delete a Parameter but may be un-aware where it may also be being used.

This Utility is confirmed to work in:
- Unity 2021.3.45f1
- Unity 2022.3.22f1

This Utility will search for Parameters in:
- Transitions
- Entry Transtions
- Anystate Transitions
- States
   - Cycle Offset
   - Mirror
   - Speed
   - Time
- Blend Trees
- Animator State Behaviours in the Following Game Specific SDKs/CCKs:
   - ChilloutVR CCK
      - Animator Driver
   - VRChat SDK
      - VRC Avatar Parameter Driver