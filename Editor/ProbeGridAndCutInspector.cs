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
        EditorGUILayout.Space(20f);
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
            if (ProbeCount <= 100000) Grid.Generate();
            else _=EditorUtility.DisplayDialog("Aborting","ProbeGridAndCut Cannot handle more than 100,000 probes", "Ok");
        }

        //========================================Cut Probes on Tagged Boundaries Section========================================
        EditorGUILayout.Space(20f);
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
        }

        //========================================Cut Probes on Objects Section========================================
        EditorGUILayout.Space(20f);
        EditorGUILayout.LabelField("Max Size of Objects That Cut Probes", EditorStyles.boldLabel);

        Grid.ObjectMaxSize = EditorGUILayout.FloatField(new GUIContent("Object Max Size", "Make more than one object, but less than two"), Grid.ObjectMaxSize);

        if (GUILayout.Button("Cut Probes Inside Objects"))
        {
            Grid.CutInsideObjects();
        }
    }
}