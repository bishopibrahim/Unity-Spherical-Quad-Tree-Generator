using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class Vector3d
{
    public double x, y, z;

    public Vector3d(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3d(Vector3 v)
    {
        this.x = (double)v.x;
        this.y = (double)v.y;
        this.z = (double)v.z;
    }

    public Vector3d normalized
    {
        get {return this.normalize();}
        set {;}
    }

    public double magnitude
    {
        get {return System.Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);}
        set {;}
    }

    // Operators
    public static Vector3d operator +(Vector3d a, Vector3d b) 
    {
        return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3d operator -(Vector3d a, Vector3d b)
    {
        return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Vector3d operator *(Vector3d v, double d)
    {
        return new Vector3d(v.x * d, v.y * d, v.z * d);
    }

    public static Vector3d operator *(double d, Vector3d v)
    {
        return new Vector3d(v.x * d, v.y * d, v.z * d);
    }

    // Dot product
    public static double operator *(Vector3d v1, Vector3d v2)
    {
        return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
    }

    // Implicit conversions

    public static implicit operator Vector3d(Vector3 v)
    {
        return new Vector3d((double)v.x, (double)v.y, (double)v.z);
    }

    public Vector3d normalize()
    {
        double reciprocalMagnitude = 1.0d / (System.Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z));
        return reciprocalMagnitude * this;
    }

    public Vector3 ToVector3()
    {
        return new Vector3((float)this.x, (float)this.y, (float)this.z);
    }
    
    public Vector3d Copy()
    {
        return new Vector3d(this.x, this.y, this.z);
    }

    public string ToString()
    {
        return this.ToVector3().ToString();
    }

    public Vector3d Cross(Vector3d v2)
    {
        double iV = this.y * v2.z - this.z * v2.y; // Det 1
        double jV = this.x * v2.z - this.z * v2.x; // Det 2
        double kV = this.x * v2.y - this.y * v2.x; // Det 3
        return new Vector3d(iV,-1 * jV,kV);
    }

}
