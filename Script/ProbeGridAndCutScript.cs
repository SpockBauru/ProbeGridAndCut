using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ProbeGridAndCutScript : MonoBehaviour
{
#if UNITY_EDITOR
    // Number of probes on each axis of the grid
    public int probesInX = 5;
    public int probesInY = 5;
    public int probesInZ = 5;

    // Check if only static objects will be tested
    public bool onlyStatic = true;

    //Set editor to save if something change. Used in ProbeGridAndCutEditor.cs
    public bool somethingChanged = false;

    // List of tags that will be tested in raycastAll
    public List<string> BoundaryTags = new List<string> { "Untagged" };

    // Max size of raycast from each probe
    public float rayTestSize = 1f;

    //Light Probe Group in Editor
    LightProbeGroup probeGroup;

    //Internal List of Light Probe Positions
    private List<Vector3> probePositions;
    public int probeCount=0;

    void Start()
    {
        probeGroup = GetComponent<LightProbeGroup>();
        if (probeGroup == null) probeGroup = gameObject.AddComponent<LightProbeGroup>();
        probePositions = new List<Vector3>(probeGroup.probePositions);
        probeCount = probeGroup.probePositions.Length;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.lossyScale);
    }

    public void UpdateProbes()
    {
        // Update Light Probe Group in Editor
        probeGroup.probePositions = probePositions.ToArray();
        probeCount = probeGroup.probePositions.Length;
    }

    public void Generate()
    {
        probePositions.Clear();

        // Position relative to parent, between 0 and 1
        Vector3 position;

        // Calculating steps on each exis, lenght between 0 and 1
        float stepX = 1f / (probesInX - 1);
        float stepY = 1f / (probesInY - 1);
        float stepZ = 1f / (probesInZ - 1);

        // Start position relative to the parent center. (Relative parent size is always 1)
        Vector3 startPosition = new Vector3(-0.5f, -0.5f, -0.5f);

        // Populate probe position Array
        for (int x = 0; x < probesInX; x++)
        {
            for (int y = 0; y < probesInY; y++)
            {
                for (int z = 0; z < probesInZ; z++)
                {
                    position.x = startPosition.x + stepX * x;
                    position.y = startPosition.y + stepY * y;
                    position.z = startPosition.z + stepZ * z;
                    probePositions.Add(position);
                }
            }
        }
    }

    public void CutTaggedObjects()
    {
        Vector3 center = transform.position;
        Vector3 scale = transform.localScale;
        Vector3 position;
        Vector3 direction;
        float distance;

        // Array with all hits from the center to the probe
        RaycastHit[] hitAll;

        // Removing from the end of the list to the beginning
        for (int i = probePositions.Count - 1; i >= 0; i--)
        {
            // Scaling from probe relative position to object position
            position = Vector3.Scale(probePositions[i], scale);
            position += center;

            // Distance and direction for the Raycast
            distance = Vector3.Distance(center, position);
            direction = position - center;

            // Trace a raycast from the center to the probe. If hit a tagged object the probe is removed from the list.
            Debug.DrawLine(center, position, Color.yellow, 1);
            hitAll = Physics.RaycastAll(center, direction, distance);
            for (int j = 0; j < hitAll.Length; j++)
            {
                if (BoundaryTags.Contains(hitAll[j].transform.gameObject.tag) &&
                    !hitAll[j].transform.gameObject.CompareTag("Untagged") &&
                    (hitAll[j].collider.gameObject.isStatic || !onlyStatic))
                {
                    probePositions.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public void CutInsideObjects()
    {
        Vector3 center = transform.position;
        Vector3 scale = transform.localScale;
        Vector3 position;
        Vector3 rayPos = Vector3.zero;

        string name1, name2, name3, name4, name5;

        RaycastHit hit;

        // Inside Object Detection: Raycast all 5 sides from selected size to the probe.
        // If all directions have the same name, the probe is inside an object.
        // Down is not tested, so its easier to remove probes from objects with a hollow at bottom, like trees.
        // Removing from the end of the list to the beginning
        for (int i = probePositions.Count - 1; i >= 0; i--)
        {
            // scaling from relative to world position
            position = Vector3.Scale(probePositions[i], scale);
            position += center;

            name1 = "1";
            name2 = "2";
            name3 = "3";
            name4 = "4";
            name5 = "5";

            // Up to probe
            rayPos.Set(position.x, position.y + rayTestSize, position.z);
            if (Physics.Raycast(rayPos, Vector3.down, out hit, Vector3.Distance(rayPos, position))) name1 = hit.transform.name;
            Debug.DrawLine(position, rayPos, Color.yellow, 1);

            // Right to Probe
            rayPos.Set(position.x + rayTestSize, position.y, position.z);
            if (Physics.Raycast(rayPos, Vector3.left, out hit, Vector3.Distance(rayPos, position))) name2 = hit.transform.name;
            Debug.DrawLine(position, rayPos, Color.yellow, 1);

            // Left to Probe
            rayPos.Set(position.x - rayTestSize, position.y, position.z);
            if (Physics.Raycast(rayPos, Vector3.right, out hit, Vector3.Distance(rayPos, position))) name3 = hit.transform.name;
            Debug.DrawLine(position, rayPos, Color.yellow, 1);

            // Forward to Probe
            rayPos.Set(position.x, position.y, position.z + rayTestSize);
            if (Physics.Raycast(rayPos, Vector3.back, out hit, Vector3.Distance(rayPos, position))) name4 = hit.transform.name;
            Debug.DrawLine(position, rayPos, Color.yellow, 1);

            // Back to Probe
            rayPos.Set(position.x, position.y, position.z - rayTestSize);
            if (Physics.Raycast(rayPos, Vector3.forward, out hit, Vector3.Distance(rayPos, position))) name5 = hit.transform.name;
            Debug.DrawLine(position, rayPos, Color.yellow, 1);

            if (name1 == name2 &&
                name1 == name3 &&
                name1 == name4 &&
                name1 == name5 &&
                (hit.collider.gameObject.isStatic || !onlyStatic))
                probePositions.RemoveAt(i);
        }
    }

    public void CutFarFromObject()
    {
        Vector3 center = transform.position;
        Vector3 scale = transform.localScale;
        Vector3 position;
        Vector3 endLine = Vector3.zero;
        Vector3 startLine = Vector3.zero;

        bool hitObject;
        bool hitTest;

        RaycastHit hit;

        // Raycast all 3 axis from one side to other, and vice versa.
        // If there's at least one hit, the probe is near an object and will not be cut
        for (int i = probePositions.Count - 1; i >= 0; i--)
        {
            // Scaling from relative to world position
            position = Vector3.Scale(probePositions[i], scale);
            position += center;

            hitObject = false;

            // Down to Up and Up do Down
            startLine.Set(position.x, position.y - rayTestSize, position.z);
            endLine.Set(position.x, position.y + rayTestSize, position.z);

            hitTest = Physics.Raycast(startLine, Vector3.up, out hit, Vector3.Distance(startLine, endLine));
            hitObject = hitObject || hitTest && (hit.collider.gameObject.isStatic || !onlyStatic);

            hitTest = Physics.Raycast(endLine, Vector3.down, out hit, Vector3.Distance(endLine, startLine));
            hitObject = hitObject || hitTest && (hit.collider.gameObject.isStatic || !onlyStatic);

            Debug.DrawLine(startLine, endLine, Color.yellow, 1);

            // Left to Right and Right to Left
            startLine.Set(position.x - rayTestSize, position.y, position.z);
            endLine.Set(position.x + rayTestSize, position.y, position.z);

            hitTest = Physics.Raycast(startLine, Vector3.right, out hit, Vector3.Distance(startLine, endLine));
            hitObject = hitObject || hitTest && (hit.collider.gameObject.isStatic || !onlyStatic);

            hitTest = Physics.Raycast(endLine, Vector3.left, out hit, Vector3.Distance(endLine, startLine));
            hitObject = hitObject || hitTest && (hit.collider.gameObject.isStatic || !onlyStatic);

            Debug.DrawLine(startLine, endLine, Color.yellow, 1);

            // Back to Forward and Forward to Back
            startLine.Set(position.x, position.y, position.z - rayTestSize);
            endLine.Set(position.x, position.y, position.z + rayTestSize);

            hitTest = Physics.Raycast(startLine, Vector3.forward, out hit, Vector3.Distance(startLine, endLine));
            hitObject = hitObject || hitTest && (hit.collider.gameObject.isStatic || !onlyStatic);

            hitTest = Physics.Raycast(endLine, Vector3.back, out hit, Vector3.Distance(endLine, startLine));
            hitObject = hitObject || hitTest && (hit.collider.gameObject.isStatic || !onlyStatic);

            Debug.DrawLine(startLine, endLine, Color.yellow, 1);

            if (!hitObject) probePositions.RemoveAt(i);
        }
    }
#endif
}
