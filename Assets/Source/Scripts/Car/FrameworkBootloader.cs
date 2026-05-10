using UnityEngine;

public class FrameworkBootloader : MonoBehaviour
{
    private void Awake()
    {
        new FrameworkStorage();
    }
}