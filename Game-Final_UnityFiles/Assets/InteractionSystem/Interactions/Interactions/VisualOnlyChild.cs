using UnityEngine;
using UnityEngine.AI;

[ExecuteAlways]
public class VisualOnlyChild : MonoBehaviour
{
    private void Reset()
    {
        //Remove any component that could affect NavMesh
        RemoveNavMeshComponents();
    }

    private void Awake()
    {
        RemoveNavMeshComponents();
    }

    private void RemoveNavMeshComponents()
    {
        //Remove NavMeshObstacle if accidentally added
        var obstacle = GetComponent<NavMeshObstacle>();
        if (obstacle != null)
            DestroyImmediate(obstacle);


        //Remove Colliders
        var colliders = GetComponents<Collider>();
        foreach (var col in colliders)
            DestroyImmediate(col);

        //Disable Navigation Static so it doesn't affect baked NavMesh
        gameObject.isStatic = false;
    }
}