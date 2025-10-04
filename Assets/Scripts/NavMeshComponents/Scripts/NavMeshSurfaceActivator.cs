using NavMeshPlus.Components;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshSurfaceActivator : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<NavMeshSurface>().enabled = true;
    }
}
