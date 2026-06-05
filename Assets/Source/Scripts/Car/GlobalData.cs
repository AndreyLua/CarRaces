using KrisDevelopment.ERMG;
using System;

[Serializable]
public class GlobalData
{
    public UserInput UserInput;
    public ERMeshGen MeshGen;
    public LineFactory LineFactory;
    public GlobalData()
    {
        UserInput = new UserInput();
    }
}
