namespace LeastSquares.UltimateMeshSlicer
{
    public struct Plane3d
    {
        public Vector3d normal;
        public double distance;

        public Plane3d(Vector3d normal, Vector3d point)
        {
            this.normal = normal.normalized;
            this.distance = Vector3d.Dot(normal, point);
        }
        

        public double GetDistanceToPoint(Vector3d point)
        {
            return Vector3d.Dot(normal, point) - distance;
        }

        public override string ToString()
        {
            return $"Plane(Normal: {normal.ToString()}, Distance: {distance:F4})";
        }
    }

}