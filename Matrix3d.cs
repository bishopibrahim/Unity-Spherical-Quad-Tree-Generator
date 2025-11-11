using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Matrix3d
{
    public double a, b, c, d, e, f, g, h, i;

    // a, b, c
    // d, e, f
    // g, h, i

    public Matrix3d(double a, double b, double c, double d, double e, double f, double g, double h, double i)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
        this.e = e;
        this.f = f;
        this.g = g;
        this.h = h;
        this.i = i;
    }

    public Matrix3d(Vector3d column1, Vector3d column2, Vector3d column3)
    {
        this.a = column1.x;
        this.b = column2.x;
        this.c = column3.x;

        this.d = column1.y;
        this.e = column2.y;
        this.f = column3.y;

        this.g = column1.z;
        this.h = column2.z;
        this.i = column3.z;
    }

    public static Matrix3d operator *(double C, Matrix3d M) // Matrix mutliplication
    {
        return new Matrix3d(C * M.a, C * M.b, C * M.c, C * M.d, C * M.e, C * M.f, C * M.g, C * M.h, C * M.i);
    }

    public static Vector3d operator *(Matrix3d M, Vector3d v) // Matrix multiplication
    {
        return new Vector3d(v.x * M.a + v.y * M.b + v.z * M.c, v.x * M.d + v.y * M.e + v.z * M.f, v.x * M.g + v.y * M.h + v.z * M.i);
    }

    public static Matrix3d AxisAngleRot(Vector3d u, double theta)
    {
        double ct = Math.Cos(theta);
        double st = Math.Sin(theta);
        double omct = 1 - ct;

        return new Matrix3d(u.x*u.x*omct + ct,      u.x*u.y*omct - u.z*st,      u.x*u.z*omct + u.y*st, 
                            u.x*u.y*omct + u.z*st,  u.y*u.y*omct+ct,            u.y*u.z*omct - u.x*st, 
                            u.x*u.z*omct - u.y*st,  u.y*u.z*omct + u.x*st,       u.z*u.z*omct + ct);
    }

    public static Matrix3d Identity()
    {
        return new Matrix3d(1, 0, 0, 0, 1, 0, 0, 0, 1);
    }
}
