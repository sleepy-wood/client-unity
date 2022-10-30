using System;

/// <summary>
/// Attribute to apply to ISproutProcessor classes.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SproutProcessorAttribute : Attribute
{
    public readonly int id;

    public SproutProcessorAttribute (int id) {
        this.id = id;
    }
}