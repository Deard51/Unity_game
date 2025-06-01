using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private Color gizmoColor = Color.red;
    [SerializeField] private float gizmoSize = 0.5f;
      private void OnDrawGizmos()
    {
        Color oldColor = Gizmos.color;
        
        Gizmos.color = gizmoColor;
        
        Gizmos.DrawSphere(transform.position, gizmoSize);
        
        Gizmos.color = oldColor;
    }
}
