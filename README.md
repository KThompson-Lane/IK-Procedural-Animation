# IK-Procedural-Animation
## Build instructions
This project was developed in Unity 2021.3 as such compatibility cannot be guaranteed with any other version of unity.
1. Clone or download this repository
2. Add the root folder as a project in Unity Hub
3. Open the project in Unity 
4. Open the scene `Test Workflow` located within `Assets/Scenes/TestWorkflow`
5. Run the project in play mode
## Solution structure
- This solution contains two assemblies, `ProceduralAnimation.Editor` and `ProceduralAnimation.Runtime`, these assembles contain editor and runtime scripts respectively.
- The `Runtime` assembly contains the `Spider Controller` script and folders containing the `IK` scripts, `Motion` scripts and `Second Order Systems` scripts. 
- Each C# file is documented with a mixture of inline and XML comments
- The `TestWorkflow` scene contains a Spider game object which has been initialised to demonstrate the created procedural animations
  - These animations can be modified using the values exposed by the components attached to the Spider game object
  - The spider can be controlled by moving the `Target` game object around the environment.
  - The scene also contains a `TestChain` which demonstrates the IK solver in isolation using the `TestTarget` object
