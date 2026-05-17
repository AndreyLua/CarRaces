using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickOffcetTransmitter : MonoBehaviour
{
    [SerializeField] private float _maxStickDistance;

    private JoystickUIScreen _screen;
    public Vector2 Offcet => (!_screen.Stick.gameObject.activeSelf || !_screen.Area.gameObject.activeSelf) ? Vector2.zero 
        : ((Vector2)_screen.Stick.transform.position - _stickPivot) / CalculateRealMaxStickDistance();

    private Vector2 _stickPivot;

    private void Awake()
    {
        _screen = UIScreenRepository.GetScreen<JoystickUIScreen>();
        _stickPivot = _screen.Stick.position;

        OnJoystickDisable(null);
   
        _screen.Area.OnPointerDownEvent += OnJoystickEnable;
        _screen.Area.OnDraggingEvent += Dragging;
        _screen.Area.OnPointerUpEvent += OnJoystickDisable;

        FrameworkStorage.GlobalData.UserInput.OnInputLockChange += OnInputLockChange;
    }

    private void OnInputLockChange()
    {
        _screen.Area.gameObject.SetActive(!FrameworkStorage.GlobalData.UserInput.InputLocked);
        OnJoystickDisable(null);
    }

    private void OnJoystickEnable(PointerEventData eventData)
    {
        List<RaycastResult> result = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, result);

        if (result.Count > 1)
            return;

        _screen.Stick.gameObject.SetActive(true);
        _screen.StickArea.gameObject.SetActive(true);
        _stickPivot = eventData.position;
        _screen.Stick.position = _stickPivot;
        _screen.StickArea.position = _screen.Stick.position;
    }

    private void Dragging(PointerEventData eventData)
    {
        float distance = Vector2.Distance(eventData.position, _stickPivot);
        if (distance <= CalculateRealMaxStickDistance())
            _screen.Stick.position = eventData.position;
        else 
            _screen.Stick.position = _stickPivot + (eventData.position - _stickPivot).normalized * CalculateRealMaxStickDistance();

        FrameworkStorage.GlobalData.UserInput.JoystickOffcet = Offcet;
    }

    private void OnJoystickDisable(PointerEventData eventData)
    {
        _screen.Stick.gameObject.SetActive(false);
        _screen.StickArea.gameObject.SetActive(false);
        _screen.Stick.position = _stickPivot;
       FrameworkStorage.GlobalData.UserInput.JoystickOffcet = Vector2.zero;
    }

    private float CalculateRealMaxStickDistance()
    {
        return _maxStickDistance * (Camera.main.pixelWidth / 1080f);
    }
}
