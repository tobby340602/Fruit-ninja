using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace LeastSquares.UltimateMeshSlicer
{
    public class MeshCutter
    {
        public struct Edge
        {
            public Vector3d vertex1;
            public Vector3d vertex2;

            public Edge(Vector3d vertex1, Vector3d vertex2)
            {
                this.vertex1 = vertex1;
                this.vertex2 = vertex2;
            }
        }

        public static Plane3d WorldSpacePlaneToMeshSpace(Transform transform, Vector3 normal, Vector3 pointOnPlane)
        {
            var scale = transform.localScale;
            transform.localScale = Vector3.one;
            var matrix = transform.worldToLocalMatrix;
            transform.localScale = scale;
            var localNormal = (Vector3d)matrix.MultiplyVector(normal);
            var localPointOnPlane = (Vector3d)matrix.MultiplyPoint(pointOnPlane);
            return new Plane3d(localNormal, localPointOnPlane);
        }
        
        private static bool AreAllVerticesOnSameSide(Vector3[] vertices, Plane3d plane, Vector3d scale)
        {
            var distance = plane.GetDistanceToPoint((Vector3d)vertices[0] * scale);
            var isOnSameSide = distance > 0;

            for (int i = 1; i < vertices.Length; i++)
            {
                distance = plane.GetDistanceToPoint((Vector3d)vertices[i] * scale);
                if ((distance > 0) != isOnSameSide)
                    return false;
            }

            return true;
        }

        public static (Mesh, Mesh) Cut(Mesh targetMesh, Plane3d plane, Vector3d scale)
        {
            var leftMesh = new Mesh();
            var rightMesh = new Mesh();

            var leftUVs = new List<Vector2>();
            var rightUVs = new List<Vector2>();
            var leftVertices = new List<Vector3d>();
            var rightVertices = new List<Vector3d>();
            var leftTriangles = new List<int>();
            var rightTriangles = new List<int>();
            
            var vertices = targetMesh.vertices;
            
            if (vertices.Length == 0 || AreAllVerticesOnSameSide(vertices, plane, scale)) return (null, null);
            
            var triangles = targetMesh.triangles;
            var uvs = targetMesh.uv.Length != 0 ? targetMesh.uv : new Vector2[vertices.Length];
            Debug.Log(targetMesh.colors.Length);
            Debug.Log(targetMesh.colors32.Length);

            var cutEdges = new List<Edge>();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                var triangleVertices = new []
                {
                    (Vector3d) vertices[triangles[i]] * scale,
                    (Vector3d) vertices[triangles[i + 1]] * scale,
                    (Vector3d) vertices[triangles[i + 2]] * scale
                };
                
                var triangleUVs = new[]
                {
                    uvs[triangles[i]],
                    uvs[triangles[i + 1]],
                    uvs[triangles[i + 2]]
                };

                bool[] isVertexOnPositiveSide =
                {
                    GetSideWithThreshold(plane, triangleVertices[0]),
                    GetSideWithThreshold(plane, triangleVertices[1]),
                    GetSideWithThreshold(plane, triangleVertices[2])
                };

                int positiveCount = Array.FindAll(isVertexOnPositiveSide, side => side).Length;
                
                if (positiveCount == 3)
                {
                    AddTriangleToMeshData(triangleVertices, triangleUVs, rightVertices, rightTriangles, rightUVs);
                }
                else if (positiveCount == 0)
                {
                    AddTriangleToMeshData(triangleVertices, triangleUVs, leftVertices, leftTriangles, leftUVs);
                }
                else
                {
                    Vector3d[] leftPart;
                    Vector3d[] rightPart;
                    Vector2[] leftInterpolatedUVs;
                    Vector2[] rightInterpolatedUVs;

                    SplitTriangle(triangleVertices, triangleUVs, plane, positiveCount, cutEdges, out leftPart, out rightPart, out leftInterpolatedUVs, out rightInterpolatedUVs);

                    AddTriangleToMeshData(leftPart, leftInterpolatedUVs, leftVertices, leftTriangles, leftUVs);
                    AddTriangleToMeshData(rightPart, rightInterpolatedUVs, rightVertices, rightTriangles, rightUVs);
                }
            }

            leftMesh.SetVertices(leftVertices.Select(V => (Vector3)V).ToArray());
            leftMesh.SetTriangles(leftTriangles, 0);
            leftMesh.SetUVs(0, leftUVs);
            leftMesh.RecalculateNormals();

            rightMesh.SetVertices(rightVertices.Select(V => (Vector3)V).ToArray());
            rightMesh.SetTriangles(rightTriangles, 0);
            rightMesh.SetUVs(0, rightUVs);
            rightMesh.RecalculateNormals();

            var leftCapMesh = CreateCapMesh(cutEdges, true);
            var rightCapMesh = CreateCapMesh(cutEdges, false);


            var transform = Matrix4x4.identity;
            var leftCombineInstances = new CombineInstance[2]
            {
                new() { mesh = leftMesh, transform = transform },
                new() { mesh = leftCapMesh, transform = transform }
            };

            var rightCombineInstances = new CombineInstance[2]
            {
                new() { mesh = rightMesh, transform = transform },
                new() { mesh = rightCapMesh, transform = transform }
            };

            var finalLeftMesh = new Mesh();
            finalLeftMesh.CombineMeshes(leftCombineInstances);
            var finalRightMesh = new Mesh();
            finalRightMesh.CombineMeshes(rightCombineInstances);

            return (finalLeftMesh, finalRightMesh);
        }

        private static void AddTriangleToMeshData(Vector3d[] inputVertices, Vector2[] inputUVs,  List<Vector3d> verticesList,
            List<int> trianglesList, List<Vector2> uvList)
        {
            if (inputVertices.Length < 3) return;
            if (inputVertices.Length == 3)
            {
                trianglesList.Add(verticesList.Count);
                verticesList.Add(inputVertices[0]);
                uvList.Add(inputUVs[0]);

                trianglesList.Add(verticesList.Count);
                verticesList.Add(inputVertices[1]);
                uvList.Add(inputUVs[1]);

                trianglesList.Add(verticesList.Count);
                verticesList.Add(inputVertices[2]);
                uvList.Add(inputUVs[2]);
            }
            else if (inputVertices.Length == 4)
            {
                // First triangle (v0, v1, v2)
                trianglesList.Add(verticesList.Count);
                verticesList.Add(inputVertices[2]);
                uvList.Add(inputUVs[2]);

                trianglesList.Add(verticesList.Count);
                verticesList.Add(inputVertices[1]);
                uvList.Add(inputUVs[1]);

                trianglesList.Add(verticesList.Count);
                verticesList.Add(inputVertices[0]);
                uvList.Add(inputUVs[0]);

                // Second triangle (v2, v1, v3)
                trianglesList.Add(verticesList.Count);
                verticesList.Add(inputVertices[3]);
                uvList.Add(inputUVs[3]);

                trianglesList.Add(verticesList.Count);
                verticesList.Add(inputVertices[2]);
                uvList.Add(inputUVs[2]);

                trianglesList.Add(verticesList.Count);
                verticesList.Add(inputVertices[0]);
                uvList.Add(inputUVs[0]);
            }
        }

        private static void SplitTriangle(Vector3d[] triangleVertices, Vector2[] triangleUVs, Plane3d plane, int positiveCount,
            List<Edge> cutEdges, out Vector3d[] leftPart, out Vector3d[] rightPart, out Vector2[] leftUVs, out Vector2[] rightUVs)
        {
            if (positiveCount == 1)
            {
                int positiveIndex = System.Array.FindIndex(triangleVertices, vertex => GetSideWithThreshold(plane, vertex));
                int prev = (positiveIndex - 1 + 3) % 3;
                int next = (positiveIndex + 1) % 3;

                var intersection1 =
                    LinePlaneIntersection(triangleVertices[positiveIndex], triangleVertices[prev], plane, out var t1);
                var intersection2 =
                    LinePlaneIntersection(triangleVertices[positiveIndex], triangleVertices[next], plane, out var t2);
                
                leftPart = new Vector3d[] { triangleVertices[next], intersection2, intersection1, triangleVertices[prev] };
                rightPart = new Vector3d[] { intersection2, intersection1, triangleVertices[positiveIndex] };
                
                var intersectionUV1 =
                    IntersectionUV(triangleUVs[positiveIndex], triangleUVs[prev], t1);
                var intersectionUV2 =
                    IntersectionUV(triangleUVs[positiveIndex], triangleUVs[next], t2);
                
                leftUVs = new Vector2[] { triangleUVs[next], intersectionUV2, intersectionUV1, triangleUVs[prev] };
                rightUVs = new Vector2[] { intersectionUV2, intersectionUV1, triangleUVs[prev] };
                
                cutEdges.Add(new Edge(intersection1, intersection2));
            }
            else
            {

                int negativeIndex = System.Array.FindIndex(triangleVertices, vertex => !GetSideWithThreshold(plane, vertex));
                int prev = (negativeIndex - 1 + 3) % 3;
                int next = (negativeIndex + 1) % 3;

                var intersection1 =
                    LinePlaneIntersection(triangleVertices[negativeIndex], triangleVertices[prev], plane, out var t1);
                var intersection2 =
                    LinePlaneIntersection(triangleVertices[negativeIndex], triangleVertices[next], plane, out var t2);

                var intersectionUV1 =
                    IntersectionUV(triangleUVs[negativeIndex], triangleUVs[prev], t1);
                var intersectionUV2 =
                    IntersectionUV(triangleUVs[negativeIndex], triangleUVs[next], t2);
                
                leftPart = new []
                {
                    intersection2, 
                    intersection1, 
                    triangleVertices[negativeIndex],
                };
                rightPart = new []
                {
                    triangleVertices[next],
                    intersection2,
                    intersection1,
                    triangleVertices[prev]
                };
                leftUVs = new []
                {
                    intersectionUV2, 
                    intersectionUV1, 
                    triangleUVs[negativeIndex],
                };
                rightUVs = new []
                {
                    triangleUVs[next],
                    intersectionUV2,
                    intersectionUV1,
                    triangleUVs[prev]
                };
                cutEdges.Add(new Edge(intersection2, intersection1));
            }
        }
        
        private static Mesh CreateCapMesh(List<Edge> cutEdges, bool flip)
        {
            var capMesh = new Mesh();
            if (cutEdges.Count == 0) return capMesh;
            var capVertices = new List<Vector3d>();
            var capTriangles = new List<int>();

            // Calculate the center point of the cut edges
            var center = Vector3d.zero;
            foreach (Edge edge in cutEdges)
            {
                center += edge.vertex1;
                center += edge.vertex2;
            }
            center /= cutEdges.Count * 2;

            // Add the center vertex
            capVertices.Add(center);

            // Iterate over the edges and create triangles that connect the center to each pair of adjacent vertices
            for (int i = 0; i < cutEdges.Count; i++)
            {
                int currentIndex = capVertices.Count;

                // Add the edge vertices to the cap mesh vertices
                capVertices.Add(cutEdges[i].vertex1);
                capVertices.Add(cutEdges[i].vertex2);

                // Add indices for a new triangle
                if (!flip)
                {
                    capTriangles.Add(0);
                    capTriangles.Add(currentIndex);
                    capTriangles.Add(currentIndex + 1);
                }
                else
                {
                    capTriangles.Add(currentIndex + 1);
                    capTriangles.Add(currentIndex);
                    capTriangles.Add(0);
                }
            }

            capMesh.vertices = capVertices.Select(V => (Vector3) V).ToArray();
            capMesh.triangles = capTriangles.ToArray();
            capMesh.RecalculateNormals();

            return capMesh;
        }

        private static Vector3d LinePlaneIntersection(Vector3d linePoint1, Vector3d linePoint2, Plane3d plane, out double t)
        {
            var direction = (linePoint2 - linePoint1);
            var planeNormal = (Vector3d)plane.normal;

            var dotNumerator = Vector3d.Dot(planeNormal, plane.distance * planeNormal - linePoint1);
            var dotDenominator = Vector3d.Dot(planeNormal, direction);
            
            t = dotNumerator / dotDenominator;
            return linePoint1 + t * direction;
        }
        
        private static Vector2 IntersectionUV(Vector2 uv1, Vector2 uv2, double t)
        {
            return new Vector2(Mathf.Lerp(uv1.x, uv2.x, (float)t), Mathf.Lerp(uv1.y, uv2.y, (float)t));
        }
        
        private static bool GetSideWithThreshold(Plane3d plane, Vector3d point, float threshold = 0.01f)
        {
            var distance = plane.GetDistanceToPoint(point);
            return distance > threshold;
        }
    }
}