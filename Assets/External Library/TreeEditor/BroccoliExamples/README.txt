Broccoli Tree Creator Examples v1.0

[BroccoliTreeFactoryScene]
Contains a basic Broccoli pipeline to begin building vegetation prefabs. Jus select the "Broccoli Tree Factory" GameObject on the Hierarchy Inspector, then on the inspector window click on "Open Tree Editor Window". On the Broccoli Tree Editor Window you can create new preview by selecting "Generate New Preview".

[RuntimeScene]
The script to generate trees by clicking on the sphere surface is on the "SceneController" GameObject. If you are planning to run the example on a standalone build (outside the editor) make sure you add the "Nature/Tree Creator Bark" and "Nature/Tree Creator Leaves" shaders to the "Always Included Shaders" array on the Graphics option of the Project Settings.


The folder containing the examples (BroccoliExamples) can be safely removed from your project.
