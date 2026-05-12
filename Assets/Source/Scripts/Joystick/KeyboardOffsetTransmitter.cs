using System.Collections.Generic;
using UnityEngine;

public class KeyboardOffsetTransmitter : MonoBehaviour
{
    [SerializeField] private JoystickOffcetTransmitter _joystickOffcetTransmitter;

    private Dictionary<KeyCode, Vector2> _offsetButtonsOffsetInPair;

    private void Awake() 
    {
        _offsetButtonsOffsetInPair = new Dictionary<KeyCode, Vector2>();

        _offsetButtonsOffsetInPair[KeyCode.W] = new Vector2(0, 1);
        _offsetButtonsOffsetInPair[KeyCode.UpArrow] = new Vector2(0, 1);

        _offsetButtonsOffsetInPair[KeyCode.S] = new Vector2(0, -1);
        _offsetButtonsOffsetInPair[KeyCode.DownArrow] = new Vector2(0, -1);

        _offsetButtonsOffsetInPair[KeyCode.A] = new Vector2(-1, 0);
        _offsetButtonsOffsetInPair[KeyCode.LeftArrow] = new Vector2(-1, 0);

        _offsetButtonsOffsetInPair[KeyCode.D] = new Vector2(1, 0);
        _offsetButtonsOffsetInPair[KeyCode.RightArrow] = new Vector2(1, 0);
    }

    private void Update()
    {
        Vector2 offset = new Vector2();

        foreach (KeyCode keyCode in _offsetButtonsOffsetInPair.Keys)
            if (Input.GetKey(keyCode))
                offset += _offsetButtonsOffsetInPair[keyCode];

        offset.Normalize();

        if (offset.magnitude < 0.01f)
            offset = _joystickOffcetTransmitter.Offcet;

        FrameworkStorage.GlobalData.UserInput.JoystickOffcet = offset;
    }
}
