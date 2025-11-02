namespace SP1.Attributes;

public class ForeignEntityAttribute : Attribute
{
    public Type Foreign { get; set; }

    public ForeignEntityAttribute(Type foreign)
    {
        Foreign = foreign;
    }
}
