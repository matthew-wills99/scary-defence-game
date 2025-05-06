using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro UseText;
    [SerializeField]
    private Transform Camera;
    [SerializeField]
    private float MaxUseDistance = 5f;
    [SerializeField]
    private LayerMask UseLayers;

    public void OnUse()
    {
        if (Physics.Raycast(Camera.position, Camera.forward, out RaycastHit hit, MaxUseDistance, UseLayers))
    }
}
