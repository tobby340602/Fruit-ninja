using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LeastSquares.UltimateMeshSlicer
{
    public class EarClipping
    {
        private static bool IsEar(Vector3d v0, Vector3d v1, Vector3d v2, List<Vector3d> polygon)
        {
            if (Vector3d.Cross(v2 - v0, v1 - v0).y <= 0)
                return false;

            foreach (var vertex in polygon)
            {
                if (vertex == v0 || vertex == v1 || vertex == v2)
                    continue;

                if (PointInTriangle(vertex, v0, v1, v2))
                    return false;
            }

            return true;
        }

        private static bool PointInTriangle(Vector3d point, Vector3d v0, Vector3d v1, Vector3d v2)
        {
            var area = 0.5f * (-v1.z * v2.x + v0.z * (-v1.x + v2.x) + v0.x * (v1.z - v2.z) + v1.x * v2.z);
            var s = 1 / (2 * area) * (v0.z * v2.x - v0.x * v2.z + (v2.z - v0.z) * point.x + (v0.x - v2.x) * point.z);
            var t = 1 / (2 * area) * (v0.z * v1.x - v0.x * v1.z + (v0.z - v1.z) * point.x + (v1.x - v0.x) * point.z);

            return s > 0 && t > 0 && 1 - s - t > 0;
        }

        public static Mesh CreateCapMesh(List<MeshCutter.Edge> cutEdges)
        {
            Mesh capMesh = new Mesh();
            List<Vector3d> capVertices = new ();
            List<int> capTriangles = new List<int>();

            // Create a list of vertices from the cut edges
            List<Vector3d> cutPolygon = new ();
            foreach (MeshCutter.Edge edge in cutEdges)
            {
                if (!cutPolygon.Contains(edge.vertex1))
                    cutPolygon.Add(edge.vertex1);

                if (!cutPolygon.Contains(edge.vertex2))
                    cutPolygon.Add(edge.vertex2);
            }

            // Triangulate the cut polygon using the ear clipping algorithm
            while (cutPolygon.Count > 2)
            {
                for (int i = 0; i < cutPolygon.Count; i++)
                {
                    var v0 = cutPolygon[i];
                    var v1 = cutPolygon[(i + 1) % cutPolygon.Count];
                    var v2 = cutPolygon[(i + 2) % cutPolygon.Count];

                    if (IsEar(v0, v1, v2, cutPolygon))
                    {
                        capVertices.Add(v0);
                        capVertices.Add(v1);
                        capVertices.Add(v2);

                        int triangleIndex = capVertices.Count - 3;
                        capTriangles.Add(triangleIndex);
                        capTriangles.Add(triangleIndex + 1);
                        capTriangles.Add(triangleIndex + 2);

                        cutPolygon.RemoveAt((i + 1) % cutPolygon.Count);
                        break;
                    }
                }
            }

            capMesh.vertices = capVertices.Select(V => (Vector3) V).ToArray();
            capMesh.triangles = capTriangles.ToArray();
            capMesh.RecalculateNormals();

            return capMesh;
        }
    }
}