using System;

/// <summary>
/// Attribute to apply to SproutLabEditor implementations.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SproutLabEditorImplAttribute : Attribute
{
    public readonly int order;

    public SproutLabEditorImplAttribute (int order) {
        this.order = order;
    }
}