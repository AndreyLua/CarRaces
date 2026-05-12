using UnityEngine;

public class KeyboardCarUserInput : MonoBehaviour
{
    private void Update()
    {
        FrameworkStorage.GlobalData.UserInput.IsBraking = Input.GetKeyDown(KeyCode.Space);
    }
}