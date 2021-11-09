using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProbeGridAndCut))]
public class ProbeGridAndCutInspector : Editor
{
    ProbeGridAndCut Grid;
    string plannedProbes;
    string text;
    float probeCount;
    float allProbeCount=0f;
    bool showDanger=false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Grid = (ProbeGridAndCut)target;

        // ========================================Create Light Probe Grid Section========================================
        //EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Number of Light Probes on each axis", EditorStyles.boldLabel);

        Grid.probesInX = EditorGUILayout.IntField(new GUIContent("X", "Minimum is 2"), Grid.probesInX);
        Grid.probesInY = EditorGUILayout.IntField(new GUIContent("Y", "Minimum is 2"), Grid.probesInY);
        Grid.probesInZ = EditorGUILayout.IntField(new GUIContent("Z", "Minimum is 2"), Grid.probesInZ);

        //Counting number of probes planned and displaying planned/Current number of probes
        probeCount = Grid.probesInX * Grid.probesInY * Grid.probesInZ;
        plannedProbes = "Probes Planned/Current:  " + probeCount + " / " + Grid.probePositions.Count.ToString();
        EditorGUILayout.LabelField(plannedProbes);

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

        //========================================Only cut on Static Objects========================================
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Check if you want to make cuts only on static objects", EditorStyles.boldLabel);
        Grid.onlyStatic = EditorGUILayout.Toggle("Static Objects Only?", Grid.onlyStatic);

        //========================================Cut Probes on Tagged Boundaries Section========================================
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Object Tags that mark the grid boundary", EditorStyles.boldLabel);

        for (int i = 0; i < Grid.BoundaryTags.Count; i++)
        {
            text = "Tag " + (i + 1).ToString();
            Grid.BoundaryTags[i] = EditorGUILayout.TagField(text, Grid.BoundaryTags[i]);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Tag"))
        {
            Grid.BoundaryTags.Add("Untagged");
        }

        if (GUILayout.Button("Remove Tag"))
        {
            if (Grid.BoundaryTags.Count > 0) Grid.BoundaryTags.RemoveAt(Grid.BoundaryTags.Count - 1);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Cut Probes Outside Tagged Boundaries"))
        {
            Grid.CutTaggedObjects();
            Grid.UpdateProbes();
        }

        //========================================Cut Probes on Objects Section========================================
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Size of rays (yellow lines) that test proximity with objects", EditorStyles.boldLabel);

        Grid.rayTestSize = EditorGUILayout.FloatField(new GUIContent("Ray test size", "Make more than one object, but less than two"), Grid.rayTestSize);

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

        //========================================Make Everything Section========================================
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Make everything", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Generate probes, cut bondaries, cut inside and cut outside");

        if (GUILayout.Button("Make Everything"))
        {
            Grid.Generate();
            Grid.CutTaggedObjects();
            Grid.CutInsideObjects();
            Grid.CutFarFromObject();

            //Dont generate if number of probes is too high
            if (Grid.probePositions.Count <= 100000)
                Grid.UpdateProbes();
            else _ = EditorUtility.DisplayDialog("Aborting", "ProbeGridAndCut Cannot handle more than 100,000 probes", "Ok");
        }

        //========================================Make Everything for Evety ProbeGridAndCut Section========================================
        showDanger = EditorGUILayout.Toggle("Show Dangerous Button",showDanger);
        if (showDanger)
        {
            EditorGUILayout.LabelField("Make Everthing for Every ProbeGridAndCut in the Scene", EditorStyles.boldLabel);
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
    }

    private void OnDisable()
    {
        Grid = (ProbeGridAndCut)target;
        Grid.SaveVariables();
    }

    private void OnDestroy ()
    {
        Grid = (ProbeGridAndCut)target;
        Grid.SaveVariables();
    }

    private void OnEnable()
    {
        Grid = (ProbeGridAndCut)target;
        Grid.LoadVariables();
    }
}