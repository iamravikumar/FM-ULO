using System;

//Bit of a placeholder right now.
public interface IZoneFinder
{
    int ZoneId { get; }
}

public class zoneFinder : IZoneFinder
{
    public int ZoneId
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}