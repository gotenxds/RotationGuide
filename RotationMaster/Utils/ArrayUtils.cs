using System;

namespace RotationMaster.Utils;

public static class ArrayUtils
{
    public static T[] Remove<T>(this T[] inputArray, T itemToRemove) 
    {
        var indexToRemove = Array.IndexOf(inputArray, itemToRemove);
        
        if (indexToRemove < 0) 
        {
            return inputArray;
        }
        
        var tempArray = new T[inputArray.Length - 1];
        Array.Copy(inputArray, 0, tempArray, 0, indexToRemove);
        Array.Copy(inputArray, indexToRemove + 1, tempArray, indexToRemove, inputArray.Length - indexToRemove - 1);
        
        return tempArray;
    }
}
