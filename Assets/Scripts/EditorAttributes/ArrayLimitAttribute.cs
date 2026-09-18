using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class ArrayLimitAttribute : PropertyAttribute
{
    public int MaxSize { get; private set; }
    public ArrayLimitAttribute(int maxSize) => MaxSize = maxSize;
}
