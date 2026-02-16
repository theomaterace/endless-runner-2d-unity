using UnityEngine;

public class LoopingGround : MonoBehaviour
{
    [SerializeField] private Transform otherSegment;
    [SerializeField] private float segmentWidth = 20f;
    [SerializeField] private float resetX = -20f;

    private void Update()
    {
        if (transform.position.x <= resetX)
        {
            transform.position = new Vector3(
                otherSegment.position.x + segmentWidth,
                transform.position.y,
                transform.position.z
            );
        }
    }
}