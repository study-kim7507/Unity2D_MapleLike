using System.Collections.Generic;

public enum ApName
{
    STR,
    DEX,
    INT,
    LUK,
}

public class AbilityPoint
{
    List<int> ap = new List<int>() { 0, 0, 0, 0 };
    
    public List<int> Ap 
    { 
        get { return ap; }
        set { value = ap; }
    }
}
