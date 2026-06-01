using KrisDevelopment.ERMG;
using UnityEngine;

public class FrameworkBootloader : MonoBehaviour
{
    private void Awake()
    {
        new FrameworkStorage();

        FrameworkStorage.GlobalData.MeshGen = FindAnyObjectByType<ERMeshGen>();
    }
}