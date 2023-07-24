using UnityEngine;
using UnityEditor;

namespace ProbeGridAndCut
{
    [CustomEditor(typeof(ProbeGridAndCut))]
    public class ProbeGridAndCutEditor : Editor
    {
        ProbeGridAndCut Grid;

        // Variables from Monobehavior class
        SerializedProperty probesInX;
        SerializedProperty probesInY;
        SerializedProperty probesInZ;

        SerializedProperty onlyStatic;
        SerializedProperty showYellowLines;
        SerializedProperty somethingChanged;
        SerializedProperty rayTestSizeInsideObject;
        SerializedProperty rayTestSizeFarObject;
        SerializedProperty BoundaryTags;
        SerializedProperty contrast;

        SerializedProperty probeCount;

        // Foldouts
        static bool generateGridFoldout = true;
        static bool optionsFoldout = true;
        static bool cutByGeometry = true;
        static bool cutByLight = true;
        static bool makeEverything = true;

        // Keep this folded
        bool dangerSectionFoldout = false;

        // Layout
        GUIStyle foldoutStyle;
        GUIStyle redButton;
        GUIStyle boldButton;
        Color defaultBackgroundColor;
        Color red = new Color(1f, 0.3f, 0.3f);

        // Temp Variables
        string text;
        float probesPlanned;
        float allCreatedProbes = 0f;
        float allProbesInScene = 0f;
        bool oldSomethingChanged = false;

        void OnEnable()
        {
            Grid = (ProbeGridAndCut)target;

            probesInX = serializedObject.FindProperty("probesInX");
            probesInY = serializedObject.FindProperty("probesInY");
            probesInZ = serializedObject.FindProperty("probesInZ");

            onlyStatic = serializedObject.FindProperty("onlyStatic");
            showYellowLines = serializedObject.FindProperty("showYellowLines");
            somethingChanged = serializedObject.FindProperty("somethingChanged");
            rayTestSizeInsideObject = serializedObject.FindProperty("rayTestSizeInsideObject");
            rayTestSizeFarObject = serializedObject.FindProperty("rayTestSizeFarObject");
            BoundaryTags = serializedObject.FindProperty("BoundaryTags");
            contrast = serializedObject.FindProperty("contrast");

            probeCount = serializedObject.FindProperty("probeCount");
            defaultBackgroundColor = GUI.backgroundColor;
        }

        void DefineStyles()
        {
#if UNITY_5_6 || UNITY_2017_1 || UNITY_2017_2 || UNITY_2017_3 || UNITY_2017_4 || UNITY_2018_1 || UNITY_2018_2 || UNITY_2018_3 || UNITY_2018_4
            foldoutStyle = EditorStyles.foldout;
#else
            foldoutStyle = new GUIStyle(EditorStyles.foldoutHeader);
#endif
            redButton = new GUIStyle(GUI.skin.button);
            redButton.normal.textColor = Color.white;
            redButton.hover.textColor = Color.white;
            redButton.active.textColor = Color.white;
            redButton.fontStyle = FontStyle.Bold;

            boldButton = new GUIStyle(GUI.skin.button);
            boldButton.fontStyle = FontStyle.Bold;
        }

        public override void OnInspectorGUI()
        {
            DefineStyles();

            serializedObject.Update();
            Grid = (ProbeGridAndCut)target;

            // Create Light Probe Grid
            EditorGUILayout.Separator();
            generateGridFoldout = EditorGUILayout.Foldout(generateGridFoldout, "Grid Generation", foldoutStyle);
            if (generateGridFoldout) GenerateProbes();

            // Options
            EditorGUILayout.Separator();
            optionsFoldout = EditorGUILayout.Foldout(optionsFoldout, "Options", foldoutStyle);
            if (optionsFoldout) Options();

            // Cut Probes Based on Geometry
            EditorGUILayout.Separator();
            cutByGeometry = EditorGUILayout.Foldout(cutByGeometry, "Cut Probes by Colliders", foldoutStyle);
            if (cutByGeometry)
            {
                CutTaggedBoundaries();

                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Cut Probes by testing objects around");
                CutProbesInsideObjects();
                CutProbesFarFromObjects();

                EditorGUILayout.Separator();
                MakeAllColliders();
            }

            // Cut Probes Based on Baked Light
            EditorGUILayout.Separator();
            cutByLight = EditorGUILayout.Foldout(cutByLight, "Cut Probes by Baked Light", foldoutStyle);
            if (cutByLight) CutByLight();

            // Make Everything
            EditorGUILayout.Separator();
            makeEverything = EditorGUILayout.Foldout(makeEverything, "Make Everything", foldoutStyle);
            if (makeEverything) MakeEverything();

            // Danger Section
            EditorGUILayout.Separator();
            dangerSectionFoldout = EditorGUILayout.Foldout(dangerSectionFoldout, "DANGER ZONE", foldoutStyle);
            if (dangerSectionFoldout) DangerZone();

            // Repaint Scene window if something changed
            if (somethingChanged.boolValue != oldSomethingChanged)
            {
                oldSomethingChanged = somethingChanged.boolValue;
                EditorWindow view = EditorWindow.GetWindow<SceneView>();
                view.Repaint();
            }

            // Save probe count
            probeCount.intValue = Grid.probeCount;

            // Apply all settings for serialized objects
            serializedObject.ApplyModifiedProperties();
        }

        void GenerateProbes()
        {
            EditorGUILayout.PropertyField(probesInX, new GUIContent("Probes in X", "Minimum is 2"));
            EditorGUILayout.PropertyField(probesInY, new GUIContent("Probes in Y", "Minimum is 2"));
            EditorGUILayout.PropertyField(probesInZ, new GUIContent("Probes in Z", "Minimum is 2"));

            // Limit the minimum to 2
            if (probesInX.intValue < 2) probesInX.intValue = 2;
            if (probesInY.intValue < 2) probesInY.intValue = 2;
            if (probesInZ.intValue < 2) probesInZ.intValue = 2;

            // Counting number of probes planned and displaying planned/Current number of probes
            probesPlanned = probesInX.intValue * probesInY.intValue * probesInZ.intValue;
            text = "Probes Planned/Current:  " + probesPlanned + " / " + probeCount.intValue.ToString();
            EditorGUILayout.LabelField(text);

            // Display as warning if number is too big
            if (probesPlanned > 10000 && probesPlanned <= 100000) EditorGUILayout.HelpBox("WARNING: More than 10,000 probes can cause slowdowns", MessageType.Warning);
            if (probesPlanned > 100000) EditorGUILayout.HelpBox("DANGER: ProbeGridAndCut is not designed to handle more than 100,000 probes ", MessageType.Error);

            if (GUILayout.Button("Generate Light Probes Grid",
                boldButton))
            {
                // Display a message if number of probes is too high
                if (probesPlanned > 100000)
                {
                    if (!EditorUtility.DisplayDialog("Warning", "These values will lead to more than 100,000 probes.\n\n" +
                                                                "It's recommended to save before.\n\n" +
                                                                "Do you want to continue?\n\n" +
                                                                "Tip: To improve performance, disable Show Wireframe in the Light Probe Group Component and disable Show Yellow Lines in ProbeGridAndCut",
                                                     "Continue",
                                                     "Close"))
                    {
                        return;
                    }

                }

                Grid.Generate();
                Grid.UpdateProbes();
                somethingChanged.boolValue = !somethingChanged.boolValue;
            }
        }

        void Options()
        {
            // Only cut on Static Objects Section
            onlyStatic.boolValue = EditorGUILayout.Toggle(new GUIContent("Static Objects Only?", "Check if you want to make cuts only on static objects"), onlyStatic.boolValue);

            // Draw Yellow Lines
            showYellowLines.boolValue = EditorGUILayout.Toggle(new GUIContent("Show Yellow Lines?", "Check if you want to draw the test lines"), showYellowLines.boolValue);
        }

        void CutTaggedBoundaries()
        {
            EditorGUILayout.LabelField("Cut probes outside tagged boundaries");

            for (int i = 0; i < BoundaryTags.arraySize; i++)
            {
                var tag = BoundaryTags.GetArrayElementAtIndex(i);
                tag.stringValue = EditorGUILayout.TagField("Tag " + i, tag.stringValue);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Tag"))
            {
                BoundaryTags.InsertArrayElementAtIndex(BoundaryTags.arraySize);
            }
            if (GUILayout.Button("Remove Tag"))
            {
                int size = BoundaryTags.arraySize - 1;
                if (size > 0) BoundaryTags.DeleteArrayElementAtIndex(size);
            }
            EditorGUILayout.EndHorizontal();


            if (GUILayout.Button(new GUIContent("Cut Outside Tagged Boundaries",
                                                "Test from the group center to each probe. The probe is cut if found any object tag.")))
            {
                Grid.CutTaggedObjects();
                Grid.UpdateProbes();
                somethingChanged.boolValue = !somethingChanged.boolValue;
            }
        }

        void CutProbesInsideObjects()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(rayTestSizeInsideObject, new GUIContent("Object Size", "Make more than the size of objects"));
            if (rayTestSizeInsideObject.floatValue < 0) rayTestSizeInsideObject.floatValue = 0;

            if (GUILayout.Button(new GUIContent("  Cut Inside Objects   ",
                                                "Cut if all yellow lines pass through the same object")))
            {
                Grid.CutInsideObjects();
                Grid.UpdateProbes();
                somethingChanged.boolValue = !somethingChanged.boolValue;
            }
            EditorGUILayout.EndHorizontal();
        }

        void CutProbesFarFromObjects()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(rayTestSizeFarObject, new GUIContent("Distance from objects", "Make more than distance between two probes"));
            if (rayTestSizeFarObject.floatValue < 0) rayTestSizeFarObject.floatValue = 0;

            if (GUILayout.Button(new GUIContent("Cut Far From Objects",
                                                "Cut if no yellow line pass through an object")))
            {
                Grid.CutFarFromObject();
                Grid.UpdateProbes();
                somethingChanged.boolValue = !somethingChanged.boolValue;
            }
            EditorGUILayout.EndHorizontal();
        }

        void MakeAllColliders()
        {
            EditorGUILayout.LabelField("Generate, cut bondaries, cut inside and outside");
            if (GUILayout.Button("Make All Colliders",
                boldButton))
            {
                // Display a message if number of probes is too high
                if (probesPlanned > 100000)
                {
                    if (!EditorUtility.DisplayDialog("Warning", "These values will lead to more than 100,000 probes.\n\n" +
                                                                "It's recommended to save before.\n\n" +
                                                                "Do you want to continue?\n\n" +
                                                                "Tip: To improve performance, disable Show Wireframe in the Light Probe Group Component and disable Show Yellow Lines in ProbeGridAndCut",
                                                     "Continue",
                                                     "Close"))
                    {
                        return;
                    }

                }

                Grid.Generate();
                Grid.CutTaggedObjects();
                Grid.CutInsideObjects();
                Grid.CutFarFromObject();
                Grid.UpdateProbes();

                somethingChanged.boolValue = !somethingChanged.boolValue;
            }
        }


        void CutByLight()
        {
            contrast.floatValue = EditorGUILayout.Slider(new GUIContent("Color difference", "Minimum color difference between probes"), contrast.floatValue, 0, 0.2f);
            if (GUILayout.Button("Cut Probes with Low Color Difference"))
            {
                ChooseBake();
                Grid.CutByLight();
                Grid.UpdateProbes();
                somethingChanged.boolValue = !somethingChanged.boolValue;
            }
        }

        void ChooseBake()
        {
            int option = EditorUtility.DisplayDialogComplex("Warning", "Cut by Lighting only works if you Generate Lighting.\n\n" +
                                                                       "Normal Bake: The same as click on Generate Light.\n" +
                                                                       "Simple Bake: Reduced quality for faster bake. Will destroy lightmaps.\n" +
                                                                       "Just Cut: Use this if you baked the full grid before.\n",
                                                            "Normal Bake",
                                                            "Just Cut",
                                                            "Simple Bake");
            switch (option)
            {
                case 0:
                    Grid.BakeLighting();
                    break;
                case 1:
                    break;
                case 2:
                    Grid.SimpleBake();
                    break;
            }
        }

        void MakeEverything()
        {
            EditorGUILayout.LabelField("Generate, cut bondaries, cut inside, cut outside and cut based on light");
            if (GUILayout.Button("Make Everything",
                boldButton))
            {
                // Display a message if number of probes is too high
                if (probesPlanned > 100000)
                {
                    if (!EditorUtility.DisplayDialog("Warning",
                                                     "These values will lead to more than 100,000 probes.\n\n" +
                                                     "It's recommended to save before.\n\n" +
                                                     "Do you want to continue?\n\n" +
                                                     "Tip: To improve performance, disable Show Wireframe in the Light Probe Group Component and disable Show Yellow Lines in ProbeGridAndCut",
                                                     "Continue",
                                                     "Close"))
                    {
                        return;
                    }

                }

                Grid.Generate();
                Grid.CutTaggedObjects();
                Grid.CutInsideObjects();
                Grid.CutFarFromObject();
                Grid.UpdateProbes();
                ChooseBake();
                Grid.CutByLight();
                Grid.UpdateProbes();
                somethingChanged.boolValue = !somethingChanged.boolValue;
            }
        }

        void DangerZone()
        {
            // Count all Probes
            EditorGUILayout.LabelField("Count all Light Probes in the opened scenes");
            if (GUILayout.Button("Count All Probes"))
            {
                allProbesInScene = 0;
#if UNITY_2023_1_OR_NEWER
                LightProbeGroup[] probeInstances = Object.FindObjectsByType<LightProbeGroup>(FindObjectsSortMode.None);
#else
                LightProbeGroup[] probeInstances = Object.FindObjectsOfType<LightProbeGroup>();
#endif
                foreach (LightProbeGroup lightGroup in probeInstances)
                {
                    allProbesInScene += lightGroup.probePositions.Length;
                }
                somethingChanged.boolValue = !somethingChanged.boolValue;
            }
            text = "All Probes in the scene: " + allProbesInScene.ToString();
            EditorGUILayout.LabelField(text);

            // Make Everything for Everyone
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Make Everthing for Every ProbeGridAndCut in the Scene");
            GUI.backgroundColor = red;
            if (GUILayout.Button("Make Everything for Everyone",
                redButton))
            {
                allCreatedProbes = 0;
#if UNITY_2023_1_OR_NEWER
                ProbeGridAndCut[] gridAndCutInstances = Object.FindObjectsByType<ProbeGridAndCut>(FindObjectsSortMode.None);
#else
                ProbeGridAndCut[] gridAndCutInstances = Object.FindObjectsOfType<ProbeGridAndCut>();
#endif
                foreach (ProbeGridAndCut gridAndCut in gridAndCutInstances)
                {
                    gridAndCut.Generate();
                    gridAndCut.CutTaggedObjects();
                    gridAndCut.CutInsideObjects();
                    gridAndCut.CutFarFromObject();
                    gridAndCut.UpdateProbes();
                }

                ChooseBake();

                foreach (ProbeGridAndCut gridAndCut in gridAndCutInstances)
                {
                    gridAndCut.CutByLight();
                    gridAndCut.UpdateProbes();
                    allCreatedProbes += gridAndCut.probeCount;
                }

                somethingChanged.boolValue = !somethingChanged.boolValue;
            }
            GUI.backgroundColor = defaultBackgroundColor;
            text = "Probes Generated: " + allCreatedProbes.ToString();
            EditorGUILayout.LabelField(text);
        }

        // Add ProbeGridAndCut to the Menu
        [MenuItem("GameObject/Light/Probe Grid And Cut", false, 10)]
        static void CreateCustomGameObject(MenuCommand menuCommand)
        {
            // Search the prefab asset path
            string[] search = AssetDatabase.FindAssets("t:prefab ProbeGridAndCut");
            string path = AssetDatabase.GUIDToAssetPath(search[0]);

            // Instantiate prefab from path
            Object prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
            GameObject instance = (GameObject)Object.Instantiate(prefab);

            // Count number of instances in scene
            instance.name = instance.name.Replace("(Clone)", "");
#if UNITY_2023_1_OR_NEWER
            int count = Object.FindObjectsByType<ProbeGridAndCut>(FindObjectsSortMode.None).Length - 1;
#else
            int count = Object.FindObjectsOfType<ProbeGridAndCut>().Length - 1;
#endif
            if (count > 0) instance.name = instance.name + " (" + count + ")";

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(instance, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);

            // Activate and set to rename
            Selection.activeObject = instance;
        }
    }
}