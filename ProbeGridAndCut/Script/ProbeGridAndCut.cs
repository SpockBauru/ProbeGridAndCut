using System.Collections.Generic;
using UnityEngine;

namespace ProbeGridAndCut
{
    [ExecuteInEditMode]
    public class ProbeGridAndCut : MonoBehaviour
    {
#if UNITY_EDITOR
        // Number of probes on each axis of the grid
        public int probesInX = 5;
        public int probesInY = 5;
        public int probesInZ = 5;

        // Check if only static objects will be tested
        public bool onlyStatic = false;

        //Set editor to save if something change. Used in ProbeGridAndCutEditor.cs
        public bool somethingChanged = false;

        // List of tags that will be tested in raycastAll
        public List<string> BoundaryTags = new List<string> { "Untagged" };

        // Size of raycast from each probe
        public float rayTestSizeInsideObject = 2.5f;
        public float rayTestSizeFarObject = 1.5f;

        //Light Probe Group in Editor
        LightProbeGroup probeGroup;

        //Internal List of Light Probe Positions
        private List<Vector3> probePositions;
        public int probeCount = 0;

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
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
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
            Vector3 position;
            Vector3 direction;
            float distance;

            // Array with all hits from the center to the probe
            RaycastHit[] hitAll;

            // Removing from the end of the list to the beginning
            for (int i = probePositions.Count - 1; i >= 0; i--)
            {
                position = transform.TransformPoint(probePositions[i]);
                direction = position - center;
                distance = Vector3.Distance(center, position);

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
                position = transform.TransformPoint(probePositions[i]);

                name1 = "1";
                name2 = "2";
                name3 = "3";
                name4 = "4";
                name5 = "5";

                // Up to probe
                rayPos.Set(position.x, position.y + rayTestSizeInsideObject, position.z);
                if (Physics.Raycast(rayPos, Vector3.down, out hit, Vector3.Distance(rayPos, position)))
                    name1 = hit.transform.name;
                Debug.DrawLine(position, rayPos, Color.yellow, 1);

                // Right to Probe
                rayPos.Set(position.x + rayTestSizeInsideObject, position.y, position.z);
                if (Physics.Raycast(rayPos, Vector3.left, out hit, Vector3.Distance(rayPos, position)))
                    name2 = hit.transform.name;
                Debug.DrawLine(position, rayPos, Color.yellow, 1);

                // Left to Probe
                rayPos.Set(position.x - rayTestSizeInsideObject, position.y, position.z);
                if (Physics.Raycast(rayPos, Vector3.right, out hit, Vector3.Distance(rayPos, position)))
                    name3 = hit.transform.name;
                Debug.DrawLine(position, rayPos, Color.yellow, 1);

                // Forward to Probe
                rayPos.Set(position.x, position.y, position.z + rayTestSizeInsideObject);
                if (Physics.Raycast(rayPos, Vector3.back, out hit, Vector3.Distance(rayPos, position)))
                    name4 = hit.transform.name;
                Debug.DrawLine(position, rayPos, Color.yellow, 1);

                // Back to Probe
                rayPos.Set(position.x, position.y, position.z - rayTestSizeInsideObject);
                if (Physics.Raycast(rayPos, Vector3.forward, out hit, Vector3.Distance(rayPos, position)))
                    name5 = hit.transform.name;
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
            Vector3 position;
            Vector3 edge = Vector3.zero;

            bool hitObject;

            // Raycast all axis from one side to center, and vice versa.
            // If there's at least one hit, the probe is near an object and will not be cut
            for (int i = probePositions.Count - 1; i >= 0; i--)
            {
                // Scaling from relative to world position
                position = transform.TransformPoint(probePositions[i]);

                hitObject = false;

                // Probe to down
                edge.Set(position.x, position.y - rayTestSizeFarObject, position.z);
                hitObject = hitObject || TestCenterEdge(position, edge);

                // Probe to up
                edge.Set(position.x, position.y + rayTestSizeFarObject, position.z);
                hitObject = hitObject || TestCenterEdge(position, edge);

                // Probe to left
                edge.Set(position.x - rayTestSizeFarObject, position.y, position.z);
                hitObject = hitObject || TestCenterEdge(position, edge);

                // Probe to right
                edge.Set(position.x + rayTestSizeFarObject, position.y, position.z);
                hitObject = hitObject || TestCenterEdge(position, edge);

                // Probe to Back
                edge.Set(position.x, position.y, position.z - rayTestSizeFarObject);
                hitObject = hitObject || TestCenterEdge(position, edge);

                // Probe to Forward
                edge.Set(position.x, position.y, position.z + rayTestSizeFarObject);
                hitObject = hitObject || TestCenterEdge(position, edge);

                //If probe hit nothing, remove
                if (!hitObject) probePositions.RemoveAt(i);
            }
        }

        private bool TestCenterEdge(Vector3 center, Vector3 edge)
        {
            RaycastHit hit;
            bool hitTest;

            //from center to edge 
            hitTest = Physics.Raycast(center, (edge - center), out hit, Vector3.Distance(center, edge));
            //if is not a static object and the plugin is set to ignore non static, its a false hit
            hitTest = hitTest && (hit.collider.gameObject.isStatic || !onlyStatic);

            //from edge to center
            hitTest = hitTest || Physics.Raycast(edge, (center - edge), out hit, Vector3.Distance(edge, center));
            //if is not a static object and the plugin is set to ignore non static, its a false hit
            hitTest = hitTest && (hit.collider.gameObject.isStatic || !onlyStatic);

            Debug.DrawLine(center, edge, Color.yellow, 1);
            return hitTest;
        }
#endif
    }
}
