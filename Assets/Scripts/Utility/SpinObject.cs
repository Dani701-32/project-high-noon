using UnityEngine;

public class SpinObject : MonoBehaviour
{
    [SerializeField] Vector3 axis;
    [SerializeField][Range(-10, 10)] float multiplier;
    void FixedUpdate()
    {
        transform.Rotate(axis, multiplier);
    }
}
