# Animator Parameter Finder
This Utility will Seek Through a Provided Unity Animator Controller and try to find any usage of a Parameter of the Specified Name. This is useful if you are trying to delete a Parameter but may be un-aware where it may also be being used.

If you like this Utility, please feel free to [Support me via Ko-Fi](https://ko-fi.com/voyvivika)

![Screenshot of the Utility](https://github.com/user-attachments/assets/86dc78f8-1cbf-4c0b-9b1d-b7b5cd7c33c1)

This Utility is confirmed to work in:
- Unity 2021.3.45f1
- Unity 2022.3.22f1

Notes:
- This requires having some kind of scene open, because this needs to create a throw-away GameObject (Unity Limitation).
   - You probably don't want to use this while a Prefab is Open.
   - You probably don't want to use this while the Humanoid Rig Editor is open.

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