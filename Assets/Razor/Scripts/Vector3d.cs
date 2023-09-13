using System;
using UnityEngine;

namespace LeastSquares.UltimateMeshSlicer
{
    public struct Vector3d
    {
        public double x, y, z;

        public Vector3d(Vector3 v) : this(v.x, v.y, v.z)
        {

        }

        public Vector3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static double Dot(Vector3d v1, Vector3d v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        public static explicit operator Vector3d(Vector3 v)
        {
            return new Vector3d(v.x, v.y, v.z);
        }
        
        public static explicit operator Vector3(Vector3d v)
        {
            return new Vector3((float)v.x, (float)v.y, (float)v.z);
        }

        public static Vector3d operator +(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3d operator -(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3d operator *(double d, Vector3d a)
        {
            return new Vector3d(a.x * d, a.y * d, a.z * d);
        }
        
        public static Vector3d operator *(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector3d Cross(Vector3d v1, Vector3d v2)
        {
            double crossX = v1.y * v2.z - v1.z * v2.y;
            double crossY = v1.z * v2.x - v1.x * v2.z;
            double crossZ = v1.x * v2.y - v1.y * v2.x;

            return new Vector3d(crossX, crossY, crossZ);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3d v)
            {
                return this.x == v.x && this.y == v.y && this.z == v.z;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }
        
        
        public static bool operator ==(Vector3d v1, Vector3d v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(Vector3d v1, Vector3d v2)
        {
            return !v1.Equals(v2);
        }
        
        public static Vector3d operator /(Vector3d a, double d)
        {
            if (d == 0)
            {
                throw new DivideByZeroException("Cannot divide a Vector3d by zero.");
            }

            return new Vector3d(a.x / d, a.y / d, a.z / d);
        }
        
        public override string ToString()
        {
            return $"({x:F4}, {y:F4}, {z:F4})";
        }

        public Vector3d normalized => (1.0 / magnitude) * new Vector3d(x, y, z);

        public double magnitude => Math.Sqrt(x * x + y * y + z * z);

        public static readonly Vector3d zero = new (0, 0, 0);
    }
}