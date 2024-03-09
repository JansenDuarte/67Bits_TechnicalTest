using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extention
{
    /// <summary>
    /// Generate a randomized Vector3
    /// </summary>
    /// <param name="_valueLimit">Components limit value</param>
    /// <param name="_negativeValues">Allow negative values</param>
    /// <param name="_removeAxis">Sets one of the axis of the vector to 0</param>
    /// <returns></returns>
    public static Vector3 RandomVec3(float _valueLimit, bool _negativeValues = true, Axis _removeAxis = Axis.NILL)
    {
        Vector3 retVal = new();

        if (_negativeValues)
        {
            retVal.x = Random.Range(-_valueLimit, _valueLimit);
            retVal.y = Random.Range(-_valueLimit, _valueLimit);
            retVal.z = Random.Range(-_valueLimit, _valueLimit);
        }
        else
        {
            retVal.x = Random.Range(0f, _valueLimit);
            retVal.y = Random.Range(0f, _valueLimit);
            retVal.z = Random.Range(0f, _valueLimit);
        }

        switch (_removeAxis)
        {
            case Axis.X:
                retVal.x = 0f;
                break;
            case Axis.Y:
                retVal.y = 0f;
                break;
            case Axis.Z:
                retVal.z = 0f;
                break;
        }

        return retVal;
    }

    //DEBUG used for a few tests
    public static Vector3 AddScalar(Vector3 _v, float _scalar)
    {
        Vector3 retVal = new Vector3(_v.x + _scalar, _v.y + _scalar, _v.z + _scalar);

        return retVal;
    }

    public static Vector3 AddScalar_OnAxis(Vector3 _v, float _scalar, Axis _axis)
    {
        Vector3 retVal = new Vector3(_v.x, _v.y, _v.z);
        switch (_axis)
        {
            case Axis.X:
                retVal.x += _scalar;
                break;
            case Axis.Y:
                retVal.y += _scalar;
                break;
            case Axis.Z:
                retVal.z += _scalar;
                break;
        }

        return retVal;
    }
}

public enum Axis
{
    NILL = 0,
    X = 1,
    Y = 2,
    Z = 3
}
