using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProbeGridAndCut))]
public class ProbeGridAndCutInspector : Editor
{
    ProbeGridAndCut Grid;
    string plannedProbes;
    string tagText;
    float ProbeCount;


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Grid = (ProbeGridAndCut)target;
        GUIStyle TextFieldStyles = new GUIStyle(EditorStyles.label);

        // ========================================Create Light Probe Grid Section========================================
        //EditorGUILayout.Space(20f);
        EditorGUILayout.LabelField("Number of Light Probes on each axis", EditorStyles.boldLabel);

        Grid.probesInX = EditorGUILayout.IntField(new GUIContent("X", "Minimum is 2"), Grid.probesInX);
        Grid.probesInY = EditorGUILayout.IntField(new GUIContent("Y", "Minimum is 2"), Grid.probesInY);
        Grid.probesInZ = EditorGUILayout.IntField(new GUIContent("Z", "Minimum is 2"), Grid.probesInZ);

        ProbeCount = Grid.probesInX * Grid.probesInY * Grid.probesInZ;
        plannedProbes = "Probes Planned/Current:  " + ProbeCount + " / " + Grid.probePositions.Count.ToString();
        EditorGUILayout.LabelField(plannedProbes, TextFieldStyles);

        //Display as warning if number is too big
        if (ProbeCount > 10000 && ProbeCount<=100000) EditorGUILayout.HelpBox("WARNING: More than 10,000 probes can cause slowdowns", MessageType.Warning);
        if (ProbeCount > 100000) EditorGUILayout.HelpBox("DANGER: ProbeGridAndCut can't handle more than 100,000 probes ", MessageType.Error);

        if (GUILayout.Button("Generate Light Probes Grid"))
        {
            if (ProbeCount <= 100000)
            {
                Grid.Generate();
                Grid.UpdateProbes();
            }
            else _= EditorUtility.DisplayDialog("Aborting", "ProbeGridAndCut Cannot handle more than 100,000 probes", "Ok");
        }

        //========================================Only cut on Static Objects========================================
        //EditorGUILayout.Space(20f);
        EditorGUILayout.LabelField("Check if you want to make cuts only on static objects", EditorStyles.boldLabel);
        Grid.onlyStatic = EditorGUILayout.Toggle("Static Objects Only?", Grid.onlyStatic);

        //========================================Cut Probes on Tagged Boundaries Section========================================
        //EditorGUILayout.Space(20f);
        EditorGUILayout.LabelField("Object Tags that mark the grid boundary", EditorStyles.boldLabel);

        for (int i = 0; i < Grid.BoundaryTags.Count; i++)
        {
            tagText = "Tag " + (i + 1).ToString();
            Grid.BoundaryTags[i] = EditorGUILayout.TagField(tagText, Grid.BoundaryTags[i]);
        }

        EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Tag"))
            {
                Grid.BoundaryTags.Add("Empty      ");
            }

            if (GUILayout.Button("Remove Tag"))
            {
                if (Grid.BoundaryTags.Count > 0) Grid.BoundaryTags.RemoveAt(Grid.BoundaryTags.Count-1);
            }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Cut Probes Outside Tagged Boundaries"))
        {
            Grid.CutTaggedObjects();
            Grid.UpdateProbes();
        }

        //========================================Cut Probes on Objects Section========================================
        //EditorGUILayout.Space(20f);
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
        EditorGUILayout.Space(20f);
        EditorGUILayout.LabelField("Make everything", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Generate probes, cut bondaries, cut inside and cut outside");
        //EditorGUILayout.LabelField("cut inside objects and cut far from objects");

        var style = new GUIStyle(GUI.skin.button);
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;
        style.hover.textColor = Color.white;
        style.active.textColor = Color.white;
        GUI.backgroundColor= Color.red;
        if (GUILayout.Button("Make Everything", style))
        {
            Grid.Generate();
            Grid.CutTaggedObjects();
            Grid.CutInsideObjects();
            Grid.CutFarFromObject();

            //Display as warning if number of probes is too big
            if (Grid.probePositions.Count<=100000)
                Grid.UpdateProbes();
            else _ = EditorUtility.DisplayDialog("Aborting", "ProbeGridAndCut Cannot handle more than 100,000 probes", "Ok");
        }
    }
}