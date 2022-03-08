using UnityEngine;

namespace ProbeGridAndCutDemoScene
{
    public class MoveObject : MonoBehaviour
    {
        Vector3 position;

        void Start()
        {
            position = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            position.x -= Time.deltaTime;
            if (position.x < -1.5f) position.x = 2.5f;
            transform.position = position;
        }
    }
}