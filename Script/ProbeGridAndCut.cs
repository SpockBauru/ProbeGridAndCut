using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ProbeGridAndCut : MonoBehaviour
{
    //External Box just for refrence
    private Bounds Box = new Bounds();

    //Number of probes on each axis of the box
    [HideInInspector]
    public int probesInX = 5;
    [HideInInspector]
    public int probesInY = 5;
    [HideInInspector]
    public int probesInZ = 5;

    //Max size of raycast from each probe
    [HideInInspector]
    public float ObjectMaxSize = 1f;

    //List of tags that will be tested in raycastAll
    [HideInInspector]
    public List<string> BoundaryTags = new List<string> { "Empty      " };

    //Position relative to parent, between 0 and 1
    private Vector3 position;
    private Vector3 startPosition;


    //Lenght relative to parent, between 0 and 1
    private float stepX;
    private float stepY;
    private float stepZ;

    LightProbeGroup probeGroup;
    [HideInInspector]
    public List<Vector3> probePositions;

    void Start()
    {
        probeGroup = GetComponent<LightProbeGroup>();
        probePositions = new List<Vector3>(probeGroup.probePositions);
    }

    public void Generate()
    {
        probePositions.Clear();

        //Fool proof (I always do that...)
        if (probesInX < 2) probesInX = 2;
        if (probesInY < 2) probesInY = 2;
        if (probesInZ < 2) probesInZ = 2;

        //Calculating steps on each exis, lenght between 0 and 1
        stepX = 1f / (probesInX - 1);
        stepY = 1f / (probesInY - 1);
        stepZ = 1f / (probesInZ - 1);

        //Start position relative to the parent center. (Relative parent size is always 1)
        startPosition.Set(-0.5f, -0.5f, -0.5f);

        //Recursive probe position table
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

        //Place probes on all positions
        probeGroup.probePositions = probePositions.ToArray();
    }

    public void CutTaggedObjects()
    {
        Vector3 center = transform.position;
        Vector3 scale = transform.localScale;
        Vector3 position;
        Vector3 direction;
        float distance;

        RaycastHit[] hitAll;

        //List<string> tags = new List<string>(BoundaryTags);

        for (int i = probePositions.Count - 1; i >= 0; i--)
        {
            //scaling from relative to world position
            position = Vector3.Scale(probePositions[i], scale);
            position += center;

            //distance and direction for the Raycast
            distance = Vector3.Distance(center, position);
            direction = position - center;

            //trace a raycast from the center to the probe. If the hit a tagged object the probe is removed from the list.
            hitAll = Physics.RaycastAll(center, direction, distance);
            for (int j = 0; j < hitAll.Length; j++)
            {
                if (BoundaryTags.Contains(hitAll[j].transform.gameObject.tag))
                {
                    probePositions.RemoveAt(i);
                    break;
                }
            }
        }

        //Update Light Probe Group
        probeGroup.probePositions = probePositions.ToArray();
    }

    public void CutInsideObjects()
    {
        Vector3 center = transform.position;
        Vector3 scale = transform.localScale;
        Vector3 position, rayPos=Vector3.zero;

        string name1, name3, name4, name5, name6;

        RaycastHit hit;

        for (int i = probePositions.Count - 1; i >= 0; i--)
        {
            //scaling from relative to world position
            position = Vector3.Scale(probePositions[i], scale);
            position += center;

            name1 = "1";
            name3 = "3";
            name4 = "4";
            name5 = "5";
            name6 = "6";

            //Raycast all 6 sides from the box boudaries to the probe. if all directions have the same name, the probe is inside an object.
            rayPos.Set(position.x, position.y + ObjectMaxSize, position.z);
            if (Physics.Raycast(rayPos, Vector3.down, out hit, Vector3.Distance(rayPos, position))) name1 = hit.transform.name;
            Debug.DrawLine(position, rayPos, Color.yellow, 1);

            rayPos.Set(position.x + ObjectMaxSize, position.y, position.z);
            if (Physics.Raycast(rayPos, Vector3.left, out hit, Vector3.Distance(rayPos, position))) name3 = hit.transform.name;
            Debug.DrawLine(position, rayPos, Color.yellow, 1);

            rayPos.Set(position.x - ObjectMaxSize, position.y, position.z);
            if (Physics.Raycast(rayPos, Vector3.right, out hit, Vector3.Distance(rayPos, position))) name4 = hit.transform.name;
            Debug.DrawLine(position, rayPos, Color.yellow, 1);

            rayPos.Set(position.x, position.y, position.z + ObjectMaxSize);
            if (Physics.Raycast(rayPos, Vector3.back, out hit, Vector3.Distance(rayPos, position))) name5 = hit.transform.name;
            Debug.DrawLine(position, rayPos, Color.yellow, 1);

            rayPos.Set(position.x, position.y, position.z - ObjectMaxSize);
            if (Physics.Raycast(rayPos, Vector3.forward, out hit, Vector3.Distance(rayPos, position))) name6 = hit.transform.name;
            Debug.DrawLine(position, rayPos, Color.yellow, 1);

            if (name1 == name3 &&
                name1 == name4 &&
                name1 == name5 &&
                name1 == name6) probePositions.RemoveAt(i);
        }

        //Update Light Probe Group
        probeGroup.probePositions = probePositions.ToArray();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Box.center, Box.extents);
    }

    private void Update()
    {
        Box.center = transform.position;
        Box.extents = transform.localScale;
    }
}
