using System;
using UnityEngine;

public class UserInput
{
    private Vector2 joystickOffcet;

    public bool IsBraking;
    public bool InputLocked { get; private set; }
    public Vector2 JoystickOffcet
    {
        get => joystickOffcet;
        set
        {
            if (!InputLocked)
                joystickOffcet = value;
        }
    }

    public event Action OnInputLockChange;

    public void ChangeInputLock(bool value)
    {
        if (InputLocked == value)
            return;

        InputLocked = value;
        joystickOffcet = Vector2.zero;
        OnInputLockChange?.Invoke();
    }

    public Vector3 CalculateWorldDirection(Camera camera)
    {
        Vector3 direction = new Vector3(JoystickOffcet.x, 0f, JoystickOffcet.y);
        return Quaternion.AngleAxis(camera.transform.eulerAngles.y, Vector3.up) * direction;
    }
}
