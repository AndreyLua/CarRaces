using System;

[Serializable]
public class GlobalData
{
    public UserInput UserInput;


    public GlobalData()
    {
        UserInput = new UserInput();
    }
}
