using UnityEngine;

namespace ProbeGridAndCut_DemoScene
{
    public class MoveObject : MonoBehaviour
    {
        Vector3 position;
        bool forward = true;

        // Start is called before the first frame update
        void Start()
        {
            position = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (forward)
            {
                position.z -= Time.deltaTime * 10;
                if (position.z < -102f) forward = false;
            }
            else
            {
                position.z += Time.deltaTime * 100;
                if (position.z > 5f) forward = true;
            }
            transform.position = position;
        }
    }
}