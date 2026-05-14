using System;
using UnityEngine;

public class UserInputCarControl : MonoBehaviour
{
    private void Update()
    {
               Vector2 input = Vector2.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                input += Vector2.up;

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                input -= Vector2.up;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                input += Vector2.left;

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                input += Vector2.right;

     
            if (FrameworkStorage.Inited)
            {
                FrameworkStorage.GlobalData.UserInput.JoystickOffcet = input;
            }
    
            FrameworkStorage.GlobalData.UserInput.IsBraking = Input.GetKey(KeyCode.Space);
       

    }
}
