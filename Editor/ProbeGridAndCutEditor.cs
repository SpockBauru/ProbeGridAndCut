using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(ProbeGridAndCut))]
public class ProbeGridAndCutEditor : Editor
{
    ProbeGridAndCut Grid;

    // Variables from Monobehavior class
    SerializedProperty probesInX;
    SerializedProperty probesInY;
    SerializedProperty probesInZ;

    SerializedProperty onlyStatic;
    SerializedProperty somethingChanged;
    SerializedProperty rayTestSize;
    SerializedProperty BoundaryTags;
    SerializedProperty probeCount;

    // Temp Variables
    string text;
    float probesPlanned;
    float allProbeCount = 0f;

    // Keep this folded
    bool showDanger = false;

    void OnEnable()
    {
        Grid = (ProbeGridAndCut)target;

        probesInX = serializedObject.FindProperty("probesInX");
        probesInY = serializedObject.FindProperty("probesInY");
        probesInZ = serializedObject.FindProperty("probesInZ");

        onlyStatic = serializedObject.FindProperty("onlyStatic");
        somethingChanged = serializedObject.FindProperty("somethingChanged");
        rayTestSize = serializedObject.FindProperty("rayTestSize");
        BoundaryTags = serializedObject.FindProperty("BoundaryTags");
        probeCount = serializedObject.FindProperty("probeCount");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Grid = (ProbeGridAndCut)target;

        // ========================================Create Light Probe Grid Section========================================
        probesInX.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(probesInX.isExpanded, "Number of Light Probes on each axis");
        if (probesInX.isExpanded)
        {
            EditorGUILayout.PropertyField(probesInX, new GUIContent("Probes in X", "Minimum is 2"));
            EditorGUILayout.PropertyField(probesInY, new GUIContent("Probes in Y", "Minimum is 2"));
            EditorGUILayout.PropertyField(probesInZ, new GUIContent("Probes in Z", "Minimum is 2"));

            // Fool proof (I always do that...)
            if (probesInX.intValue < 2) probesInX.intValue = 2;
            if (probesInY.intValue < 2) probesInY.intValue = 2;
            if (probesInZ.intValue < 2) probesInZ.intValue = 2;

            // Counting number of probes planned and displaying planned/Current number of probes
            probesPlanned = Grid.probesInX * Grid.probesInY * Grid.probesInZ;
            text = "Probes Planned/Current:  " + probesPlanned + " / " + probeCount.intValue.ToString();
            EditorGUILayout.LabelField(text);

            // Display as warning if number is too big
            if (probesPlanned > 10000 && probesPlanned <= 100000) EditorGUILayout.HelpBox("WARNING: More than 10,000 probes can cause slowdowns", MessageType.Warning);
            if (probesPlanned > 100000) EditorGUILayout.HelpBox("DANGER: ProbeGridAndCut can't handle more than 100,000 probes ", MessageType.Error);

            if (GUILayout.Button("Generate Light Probes Grid"))
            {
                // Dont generate if number of probes is too high
                if (probesPlanned <= 100000)
                {
                    Grid.Generate();
                    Grid.UpdateProbes();
                    somethingChanged.boolValue = !somethingChanged.boolValue;
                }
                else _ = EditorUtility.DisplayDialog("Aborting", "ProbeGridAndCut Cannot handle more than 100,000 probes", "Ok");
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        //========================================Only cut on Static Objects Section========================================
        EditorGUILayout.Separator();
        onlyStatic.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(onlyStatic.isExpanded, "Check if you want to make cuts only on static objects");
        if (onlyStatic.isExpanded)
            onlyStatic.boolValue = EditorGUILayout.Toggle(new GUIContent("Static Objects Only?"), onlyStatic.boolValue);

        EditorGUILayout.EndFoldoutHeaderGroup();

        //========================================Cut Probes on Tagged Boundaries Section========================================
        EditorGUILayout.Space(10f);
        BoundaryTags.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(BoundaryTags.isExpanded, "Cut probes by object Tags");
        if (BoundaryTags.isExpanded)
        {
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

            if (GUILayout.Button(new GUIContent("Cut Probes Outside Tagged Boundaries", "Test from the group center to each probe. The probe is removed if found any object tag.")))
            {
                Grid.CutTaggedObjects();
                Grid.UpdateProbes();
                somethingChanged.boolValue = !somethingChanged.boolValue;
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        //========================================Cut Probes on Objects Section========================================
        EditorGUILayout.Separator();
        rayTestSize.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(rayTestSize.isExpanded, "Cut Probes by testing objects around");
        if (rayTestSize.isExpanded)
        {
            EditorGUILayout.LabelField("Size of rays (yellow lines) that test proximity with objects");
            EditorGUILayout.DelayedFloatField(rayTestSize, new GUIContent("Ray test size", "Make more than one object, but less than two"));
            if (GUILayout.Button(new GUIContent("Cut Probes Inside Objects", "Cut if all yellow lines pass through the same object")))
            {
                Grid.CutInsideObjects();
                Grid.UpdateProbes();
                somethingChanged.boolValue = !somethingChanged.boolValue;
            }
            if (GUILayout.Button(new GUIContent("Cut Probes Far From Objects", "Don't cut if one yellow line pass through an object")))
            {
                Grid.CutFarFromObject();
                Grid.UpdateProbes();
                somethingChanged.boolValue = !somethingChanged.boolValue;
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        //========================================Make Everything Section========================================
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Generate probes, cut bondaries, cut inside and cut outside", EditorStyles.boldLabel);
        if (GUILayout.Button("Make Everything"))
        {
            Grid.Generate();
            Grid.CutTaggedObjects();
            Grid.CutInsideObjects();
            Grid.CutFarFromObject();

            // Dont generate if number of probes is too high
            if (Grid.probeCount <= 100000) Grid.UpdateProbes();
            else _ = EditorUtility.DisplayDialog("Aborting", "ProbeGridAndCut Cannot handle more than 100,000 probes", "Ok");

            somethingChanged.boolValue = !somethingChanged.boolValue;
        }

        //========================================Make Everything for Evety ProbeGridAndCut Section========================================
        EditorGUILayout.Separator();
        showDanger = EditorGUILayout.Toggle("Show Dangerous Button", showDanger);
        if (showDanger)
        {
            EditorGUILayout.LabelField("Make Everthing for Every ProbeGridAndCut in the Scene", EditorStyles.boldLabel);
            if (GUILayout.Button("Make Everything for Everyone"))
            {
                allProbeCount = 0;
                ProbeGridAndCut[] foundInstances = Object.FindObjectsOfType<ProbeGridAndCut>();
                for (int i = 0; i < foundInstances.Length; i++)
                {
                    foundInstances[i].Generate();
                    foundInstances[i].CutTaggedObjects();
                    foundInstances[i].CutInsideObjects();
                    foundInstances[i].CutFarFromObject();
                    foundInstances[i].UpdateProbes();
                    allProbeCount += foundInstances[i].probeCount;
                }
                somethingChanged.boolValue = !somethingChanged.boolValue;
            }
            text = "All Probes Generated: " + allProbeCount.ToString();
            EditorGUILayout.LabelField(text);
        }

        //Save probe count
        probeCount.intValue = Grid.probeCount;

        // Apply all settings for serialized objects
        serializedObject.ApplyModifiedProperties();
    }

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
        int count = Object.FindObjectsOfType<ProbeGridAndCut>().Length - 1;
        if (count > 0) instance.name = instance.name + " (" + count + ")";

        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(instance, menuCommand.context as GameObject);

        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);

        // Activate and set to rename
        Selection.activeObject = instance;
    }
}