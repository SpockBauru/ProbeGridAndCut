using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProbeGridAndCut))]
public class ProbeGridAndCutInspector : Editor
{
    //Variables from Monobehavior class
    ProbeGridAndCut Grid;
    SerializedProperty probesInX;
    SerializedProperty probesInY;
    SerializedProperty probesInZ;

    SerializedProperty onlyStatic;
    SerializedProperty rayTestSize;
    SerializedProperty BoundaryTags;

    //Temp Variables
    string text;
    float probeCount;
    float allProbeCount = 0f;

    //Keep this unfold
    bool showDanger=false;

    private void OnEnable()
    {
        probesInX = serializedObject.FindProperty("probesInX");
        probesInY = serializedObject.FindProperty("probesInY");
        probesInZ = serializedObject.FindProperty("probesInZ");

        onlyStatic = serializedObject.FindProperty("onlyStatic");
        rayTestSize = serializedObject.FindProperty("rayTestSize");
        BoundaryTags = serializedObject.FindProperty("BoundaryTags");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Grid = (ProbeGridAndCut)target;
        serializedObject.Update();

        // ========================================Create Light Probe Grid Section========================================
        probesInX.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(probesInX.isExpanded, "Number of Light Probes on each axis");
        if (probesInX.isExpanded)
        {
            EditorGUILayout.DelayedIntField(probesInX, new GUIContent("X", "Minimum is 2"));
            EditorGUILayout.DelayedIntField(probesInY, new GUIContent("Y", "Minimum is 2"));
            EditorGUILayout.DelayedIntField(probesInZ, new GUIContent("Z", "Minimum is 2"));

            //Counting number of probes planned and displaying planned/Current number of probes
            probeCount = Grid.probesInX * Grid.probesInY * Grid.probesInZ;
            text = "Probes Planned/Current:  " + probeCount + " / " + Grid.probePositions.Count.ToString();
            EditorGUILayout.LabelField(text);

            //Display as warning if number is too big
            if (probeCount > 10000 && probeCount <= 100000) EditorGUILayout.HelpBox("WARNING: More than 10,000 probes can cause slowdowns", MessageType.Warning);
            if (probeCount > 100000) EditorGUILayout.HelpBox("DANGER: ProbeGridAndCut can't handle more than 100,000 probes ", MessageType.Error);

            if (GUILayout.Button("Generate Light Probes Grid"))
            {
                //Dont generate if number of probes is too high
                if (probeCount <= 100000)
                {
                    Grid.Generate();
                    Grid.UpdateProbes();
                }
                else _ = EditorUtility.DisplayDialog("Aborting", "ProbeGridAndCut Cannot handle more than 100,000 probes", "Ok");
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        //========================================Only cut on Static Objects========================================
        EditorGUILayout.Space(10f);
        onlyStatic.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(onlyStatic.isExpanded, "Check if you want to make cuts only on static objects");
        if (onlyStatic.isExpanded)
            EditorGUILayout.PropertyField(onlyStatic, new GUIContent("Static Objects Only?"));

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
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        //========================================Cut Probes on Objects Section========================================
        EditorGUILayout.Space(10f);
        rayTestSize.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(rayTestSize.isExpanded, "Cut Probes by testing objects around");
        if (rayTestSize.isExpanded)
        {
            EditorGUILayout.LabelField("Size of rays (yellow lines) that test proximity with objects");
            EditorGUILayout.DelayedFloatField(rayTestSize, new GUIContent("Ray test size", "Make more than one object, but less than two"));
            if (GUILayout.Button("Cut Probes Inside Objects"))
            {
                Grid.CutInsideObjects();
                Grid.UpdateProbes();
            }
            if (GUILayout.Button("Cut Probes Far From Objects"))
            {
                Grid.CutFarFromObject();
                Grid.UpdateProbes();
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        //========================================Make Everything Section========================================
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Generate probes, cut bondaries, cut inside and cut outside", EditorStyles.boldLabel);
        if (GUILayout.Button("Make Everything"))
        {
            Grid.Generate();
            Grid.CutTaggedObjects();
            Grid.CutInsideObjects();
            Grid.CutFarFromObject();

            //Dont generate if number of probes is too high
            if (Grid.probePositions.Count <= 100000) Grid.UpdateProbes();
            else _ = EditorUtility.DisplayDialog("Aborting", "ProbeGridAndCut Cannot handle more than 100,000 probes", "Ok");
        }

        //========================================Make Everything for Evety ProbeGridAndCut Section========================================
        EditorGUILayout.Space(10f);
        showDanger = EditorGUILayout.BeginFoldoutHeaderGroup(showDanger, "Show Dangerous Button");
        if (showDanger)
        {
            EditorGUILayout.LabelField("Make Everthing for Every ProbeGridAndCut in the Scene");
            if (GUILayout.Button("Make Everything for Every ProbeGridAndCut"))
            {
                ProbeGridAndCut[] foundInstances = Object.FindObjectsOfType<ProbeGridAndCut>();
                for (int i = 0; i < foundInstances.Length; i++)
                {
                    foundInstances[i].Generate();
                    foundInstances[i].CutTaggedObjects();
                    foundInstances[i].CutInsideObjects();
                    foundInstances[i].CutFarFromObject();
                    foundInstances[i].UpdateProbes();
                    allProbeCount += foundInstances[i].probePositions.Count;
                }
            }
            text = "All Probes Generated: " + allProbeCount.ToString();
            EditorGUILayout.LabelField(text);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        //Apply all settings
        serializedObject.ApplyModifiedProperties();
    }
}