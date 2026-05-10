using UnityEngine;

public class JoystickUIScreen : UIScreen
{
    [SerializeField] private Transform _stick;
    [SerializeField] private Transform _stickArea;
    [SerializeField] private EventTriggerActionsHub _area;

    public Transform Stick => _stick;
    public Transform StickArea => _stickArea;
    public EventTriggerActionsHub Area => _area;
}