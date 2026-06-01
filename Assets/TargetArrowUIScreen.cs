using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetArrowUIScreen : UIScreen
{
    [SerializeField] private Image _arrow;
    private float _targetAngle = 0;


    public void SetTargetAngle(float targetAngle)
    {
        _targetAngle = targetAngle;
    }

    private void Update()
    {
        Quaternion targetRotation = Quaternion.Euler(0, 0, _targetAngle);
        _arrow.transform.rotation = targetRotation;
    }
}
