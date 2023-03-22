using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

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

        // Used in ProbeGridAndCutEditor.cs
        public bool somethingChanged = false;

        // List of tags that will be tested in raycastAll
        public List<string> BoundaryTags = new List<string> { "Untagged" };

        // Size of raycast from each probe
        public float rayTestSizeInsideObject = 2.5f;
        public float rayTestSizeFarObject = 1.5f;

        // Show/hide yellow lines
        public bool showYellowLines = true;

        // Used on cut by light
        public float contrast = 0.1f;

        // Light Probe Group in Editor
        LightProbeGroup probeGroup;

        // List of Light Probe Positions
        List<Vector3> probePositions = new List<Vector3>();
        public int probeCount = 0;

        // Caching variables
        RaycastHit[] hitCache = new RaycastHit[100];
        Color red = Color.red;
        Color yellow = Color.yellow;
        Vector3 zeros = Vector3.zero;
        Vector3 ones = Vector3.one;

        private void Start()
        {
            probeGroup = GetComponent<LightProbeGroup>();
            if (probeGroup == null)
            {
                probeGroup = gameObject.AddComponent<LightProbeGroup>();
                Generate();
                UpdateProbes();
            }
            probePositions = new List<Vector3>(probeGroup.probePositions);
            probeCount = probeGroup.probePositions.Length;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = red;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(zeros, ones);
        }

        public void Generate()
        {
            probePositions.Clear();
            probePositions = new List<Vector3>(probesInX * probesInY * probesInZ);

            // Position relative to parent, between 0 and 1
            Vector3 position;

            // Calculating steps on each exis, lenght between 0 and 1
            float stepX = 1f / (probesInX - 1);
            float stepY = 1f / (probesInY - 1);
            float stepZ = 1f / (probesInZ - 1);

            // Start position relative to the parent center. (Relative parent size is always 1)
            Vector3 startPosition = new Vector3(0.5f, 0.5f, 0.5f);

            // Populate probe position Array
            for (int x = 0; x < probesInX; x++)
            {
                for (int y = 0; y < probesInY; y++)
                {
                    for (int z = 0; z < probesInZ; z++)
                    {
                        position.x = startPosition.x - stepX * x;
                        position.y = startPosition.y - stepY * y;
                        position.z = startPosition.z - stepZ * z;
                        probePositions.Add(position);
                    }
                }
            }
        }

        public void CutTaggedObjects()
        {
            string currentTag;
            int hitAllLenght;

            Vector3 center = transform.position;
            Vector3 position;
            Vector3 direction;
            float distance;

            // Removing from the end of the list to the beginning
            for (int i = probePositions.Count - 1; i >= 0; i--)
            {
                position = transform.TransformPoint(probePositions[i]);
                direction = (position - center).normalized;
                distance = Vector3.Distance(center, position);

                // Trace a raycast from the center to the probe. If hit a tagged object the probe is removed from the list.
                YellowLine(center, position);
                hitAllLenght = Physics.RaycastNonAlloc(center, direction, hitCache, distance);

                for (int j = 0; j < hitAllLenght; j++)
                {
                    // Check if object is static
                    if (onlyStatic && !hitCache[j].collider.gameObject.isStatic) continue;

                    currentTag = hitCache[j].transform.gameObject.tag;
                    if (BoundaryTags.Contains(currentTag) && !currentTag.Equals("Untagged"))
                    {
                        RemoveProbeAt(i);
                        break;
                    }
                }

                // Progress Bar
                if (i % 10000 == 0)
                    EditorUtility.DisplayProgressBar("Progress", "Cutting Probes by Tags...", 1 - (float)i / probePositions.Count);
            }
            EditorUtility.ClearProgressBar();
        }

        public void CutInsideObjects()
        {
            Vector3 position;
            Vector3 target;

            // Caching Directions
            Vector3 directionUp = transform.up;
            Vector3 directionRight = transform.right;
            Vector3 directionForward = transform.forward;

            List<int> ID1, ID2, ID3, ID4, ID5;

            // Inside Object Detection: Raycast all 5 sides from selected size to the probe.
            // If all directions contains the same ID, the probe is inside an object.
            // Down is not tested, so its easier to remove probes from objects with a hollow at bottom, like trees.
            // Removing from the end of the list to the beginning
            for (int i = probePositions.Count - 1; i >= 0; i--)
            {
                position = transform.TransformPoint(probePositions[i]);

                // Up to Probe
                target = position + rayTestSizeInsideObject * directionUp;
                ID1 = HitObjectsID(target, position);

                // Right to Probe
                target = position + rayTestSizeInsideObject * directionRight;
                ID2 = HitObjectsID(target, position);

                // Left to Probe
                target = position - rayTestSizeInsideObject * directionRight;
                ID3 = HitObjectsID(target, position);

                // Forward to Probe
                target = position + rayTestSizeInsideObject * directionForward;
                ID4 = HitObjectsID(target, position);

                // Back to Probe
                target = position - rayTestSizeInsideObject * directionForward;
                ID5 = HitObjectsID(target, position);

                var intersection1 = ID1.Intersect(ID2);
                var intersection2 = ID1.Intersect(ID3);
                var intersection3 = ID1.Intersect(ID4);
                var intersection4 = ID1.Intersect(ID5);

                if (intersection1.Count() > 0 &&
                    intersection2.Count() > 0 &&
                    intersection3.Count() > 0 &&
                    intersection4.Count() > 0)
                {
                    RemoveProbeAt(i);
                }

                // Progress Bar
                if (i % 10000 == 0)
                    EditorUtility.DisplayProgressBar("Progress", "Cutting Probes Inside Objects...", 1 - (float)i / probePositions.Count);
            }
            EditorUtility.ClearProgressBar();
        }

        private List<int> HitObjectsID(Vector3 from, Vector3 to)
        {
            List<int> hitIDs = new List<int>();
            int hitNumber;

            hitNumber = Physics.RaycastNonAlloc(from, to - from, hitCache, Vector3.Distance(from, to));
            for (int i = 0; i < hitNumber; i++)
            {
                // Check if object is static
                if (!onlyStatic || hitCache[i].collider.gameObject.isStatic)
                    hitIDs.Add(hitCache[i].transform.GetInstanceID());
            }
            YellowLine(from, to);
            return hitIDs;
        }

        public void CutFarFromObject()
        {
            Vector3 position;
            Vector3 target;

            Vector3[] directionsScaled = new Vector3[]
            {
                rayTestSizeFarObject * transform.up,      // Up
                -rayTestSizeFarObject * transform.up,     // Down
                rayTestSizeFarObject * transform.right,   // Right
                -rayTestSizeFarObject * transform.right,  // Left
                rayTestSizeFarObject * transform.forward, // Forward
                -rayTestSizeFarObject * transform.forward // Back
            };

            bool hitObject;

            // Raycast all axis from one direction to center, and vice versa.
            // If there's at least one hit, the probe is near an object and will not be cut
            for (int i = probePositions.Count - 1; i >= 0; i--)
            {
                // Scaling from relative to world position
                position = transform.TransformPoint(probePositions[i]);

                hitObject = false;

                for (int j = 0; j < 6; j++)
                {
                    target = position + directionsScaled[j];
                    hitObject |= RaycastallBothWays(position, target);
                }

                // If probe hit nothing, remove
                if (!hitObject)
                {
                    RemoveProbeAt(i);
                }

                // Progress Bar
                if (i % 10000 == 0)
                    EditorUtility.DisplayProgressBar("Progress", "Cutting Probes Far From Objects...", 1 - (float)(i) / probePositions.Count);
            }
            EditorUtility.ClearProgressBar();
        }

        private bool RaycastallBothWays(Vector3 from, Vector3 to)
        {
            bool hitTest = false;
            Vector3 direction = to - from;
            float distance = Vector3.Distance(from, to);

            int hitNumber;

            // from - to
            hitNumber = Physics.RaycastNonAlloc(from, direction, hitCache, distance);
            for (int i = 0; i < hitNumber; i++)
            {
                // Check if object is static
                if (!onlyStatic || hitCache[i].collider.gameObject.isStatic)
                {
                    hitTest = true;
                    break;
                }
            }

            // to - from
            hitNumber = Physics.RaycastNonAlloc(to, -direction, hitCache, distance);
            for (int i = 0; i < hitNumber; i++)
            {
                // Check if object is static
                if (!onlyStatic || hitCache[i].collider.gameObject.isStatic)
                {
                    hitTest = true;
                    break;
                }
            }

            YellowLine(from, to);
            return hitTest;
        }

        public void CutByLight()
        {
            int probePositionsCount = probePositions.Count;

            Dictionary<Vector3, int> probePositionsOrder = new Dictionary<Vector3, int>(probePositionsCount);
            for (int i = 0; i < probePositionsCount; i++)
            {
                probePositionsOrder.Add(probePositions[i], i);
            }

            // Local coordinates
            Vector3 position;
            Vector3 target;

            // World coordinates
            Vector3 positionWorld;

            // Calculating steps on each exis
            float stepX = 1f / (probesInX - 1);
            float stepY = 1f / (probesInY - 1);
            float stepZ = 1f / (probesInZ - 1);

            // Scaled Directions in Local coordinates
            Vector3[] scaledDirections = new Vector3[]
            {
                stepY * transform.up,      // Up
                -stepY * transform.up,     // Down
                stepX * transform.right,   // Right
                -stepX * transform.right,  // Left
                stepZ * transform.forward, // Forward
                -stepZ * transform.forward // Back
            };

            // Cardinal Directions
            Vector3[] directions = new Vector3[]
            {
                Vector3.up,
                Vector3.down,
                Vector3.left,
                Vector3.right,
                Vector3.forward,
                Vector3.back
            };

            // True if probe is different from neighbors
            bool highContrast;

            // Evaluating All Probes
            Color[][] evaluatedColors = new Color[probePositionsCount][];
            for (int i = 0; i < probePositionsCount; i++)
            {
                positionWorld = transform.TransformPoint(probePositions[i]);
                evaluatedColors[i] = EvaluateProbe(positionWorld, directions);

                // Progress Bar
                if (i % 10000 == 0)
                    EditorUtility.DisplayProgressBar("Progress", "Evaluating Probes", (float)i / probePositions.Count);
            }

            // Couting from end to beginning
            for (int i = probePositionsCount - 1; i >= 0; i--)
            {
                highContrast = false;
                position = probePositions[i];

                // Test neighbor probe for each direction 
                for (int j = 0; j < 6; j++)
                {
                    target = position + scaledDirections[j];
                    if (probePositionsOrder.ContainsKey(target))
                    {
                        int targetIndex = probePositionsOrder[target];
                        highContrast = ColorContrast(evaluatedColors[i], evaluatedColors[targetIndex]);
                        if (highContrast) break;
                    }
                }

                if (!highContrast)
                {
                    RemoveProbeAt(i);
                }

                // Progress Bar
                if (i % 10000 == 0)
                    EditorUtility.DisplayProgressBar("Progress", "Cutting Probes by Light...", 1 - (float)i / probePositions.Count);
            }
            EditorUtility.ClearProgressBar();
        }

        Color[] EvaluateProbe(Vector3 position, Vector3[] directions)
        {
            SphericalHarmonicsL2 lightProbe;
            Color[] color = new Color[6];
            LightProbes.GetInterpolatedProbe(position, null, out lightProbe);
            lightProbe.Evaluate(directions, color);
            return color;
        }

        bool ColorContrast(Color[] probe1, Color[] probe2)
        {
            bool probeIsDifferent = false;

            for (int i = 0; i < 6; i++)
            {
                if (Mathf.Abs(probe1[i].r - probe2[i].r) > contrast) probeIsDifferent = true;
                if (Mathf.Abs(probe1[i].g - probe2[i].g) > contrast) probeIsDifferent = true;
                if (Mathf.Abs(probe1[i].b - probe2[i].b) > contrast) probeIsDifferent = true;
            }
            return probeIsDifferent;
        }

        public void BakeLighting()
        {
            EditorUtility.DisplayProgressBar("Please Wait", "Generating Light, the bar will not move until finished.", 0.01f);
            Lightmapping.Bake();
            EditorUtility.ClearProgressBar();
        }

        public void UpdateProbes()
        {
            EditorUtility.DisplayProgressBar("Progress", "Updating Probes, it may take a while whitout moving the bar", 1);
            probeGroup.probePositions = probePositions.ToArray();
            probeCount = probeGroup.probePositions.Length;
            EditorUtility.ClearProgressBar();
        }

        public void SimpleBake()
        {
#if UNITY_5_6 || UNITY_2017_1 || UNITY_2017_2 || UNITY_2017_3 || UNITY_2017_4
            int atlasWidth = LightmapEditorSettings.maxAtlasWidth;
            LightmapEditorSettings.maxAtlasWidth = 8;
            BakeLighting();
            LightmapEditorSettings.maxAtlasWidth = atlasWidth;
#elif UNITY_2018_1 || UNITY_2018_2 || UNITY_2018_3 || UNITY_2018_4 || UNITY_2019_1 || UNITY_2019_2 || UNITY_2019_3 || UNITY_2019_4
            int maxAtlas = LightmapEditorSettings.maxAtlasSize;
            LightmapEditorSettings.maxAtlasSize = 8;
            BakeLighting();
            LightmapEditorSettings.maxAtlasSize = maxAtlas;
#else
            LightingSettings lightSettings = Lightmapping.lightingSettings;
            int lightmapMaxSize = lightSettings.lightmapMaxSize;
            lightSettings.lightmapMaxSize = 8;
            BakeLighting();
            lightSettings.lightmapMaxSize = lightmapMaxSize;
#endif
        }

        /// <summary>
        /// Fast remove from probePositions 
        /// </summary>
        private void RemoveProbeAt(int index)
        {
            // Remove from the last position is faster
            probePositions[index] = probePositions[probePositions.Count - 1];
            probePositions.RemoveAt(probePositions.Count - 1);
        }

        private void YellowLine(Vector3 start, Vector3 end)
        {
            if (!showYellowLines) return;
            Debug.DrawLine(start, end, yellow, 1);
        }
#endif
    }
}
