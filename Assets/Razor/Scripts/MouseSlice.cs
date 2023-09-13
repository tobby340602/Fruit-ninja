using System.Collections.Generic;
using LeastSquares;
using UnityEngine;

namespace LeastSquares.UltimateMeshSlicer
{
    public class MouseSlice : MonoBehaviour
    {

        public float separation;
        public ScreenLineRenderer lineRenderer;
        public bool addRigidbodies;

        private void OnEnable()
        {
            lineRenderer.OnLineDrawn += OnLineDrawn;
        }

        private void OnDisable()
        {
            lineRenderer.OnLineDrawn -= OnLineDrawn;
        }

        private void OnLineDrawn(Vector3 start, Vector3 end, Vector3 depth)
        {
            var planeTangent = (end - start).normalized;

            // if we didn't drag, we set tangent to be on x
            if (planeTangent == Vector3.zero)
                planeTangent = Vector3.right;

            var normalVec = Vector3.Cross(depth, planeTangent);
            SliceObjects(normalVec, (start + end) / 2);
        }

        void SliceObjects(Vector3 normal, Vector3 point)
        {
            var toSlice = GameObject.FindGameObjectsWithTag("Cuttable");
            List<GameObject> positive = new(), negative = new();

            GameObject obj;
            bool slicedAny = false;
            for (int i = 0; i < toSlice.Length; ++i)
            {
                obj = toSlice[i];
                if (obj.GetComponent<MeshFilter>() == null || !obj.activeSelf)
                    continue;
                SliceObject(normal, point, obj, positive, negative);
            }

            SeparateMeshes(positive, negative, normal);
        }

        void SliceObject(Vector3 normal, Vector3 point, GameObject obj, List<GameObject> positiveObjects,
            List<GameObject> negativeObjects)
        {
            var meshFilter = obj.GetComponent<MeshFilter>();
            var skinnedMeshRenderer = obj.GetComponent<SkinnedMeshRenderer>();
            var mesh = meshFilter != null ? (meshFilter.sharedMesh) : (skinnedMeshRenderer ? skinnedMeshRenderer.sharedMesh : null);
            if (mesh == null) return;
            
            var plane = MeshCutter.WorldSpacePlaneToMeshSpace(obj.transform, normal, point);
            var (leftMesh, rightMesh) = MeshCutter.Cut(mesh, plane, (Vector3d)obj.transform.localScale);
            if (leftMesh == null) return;
            
            negativeObjects.Add(CreateObject(leftMesh, obj));
            positiveObjects.Add(CreateObject(rightMesh, obj));
            DestroyImmediate(obj);
        }

        private GameObject CreateObject(Mesh mesh, GameObject target)
        {
            var rightObject = new GameObject(target.name + "_cut")
            {
                transform =
                {
                    position = target.transform.position,
                    rotation = target.transform.rotation,
                }
            };
            if (addRigidbodies)
            {
                rightObject.AddComponent<Rigidbody>();
                var collider = rightObject.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.sharedMesh = mesh;
            }

            rightObject.tag = "Cuttable";
            rightObject.AddComponent<MeshFilter>().mesh = mesh;
            var renderer = rightObject.AddComponent<MeshRenderer>();
            renderer.materials = target.GetComponent<MeshRenderer>().materials;
            //rightObject.transform.SetParent(target.transform);
            return rightObject;
        }

        void SeparateMeshes(Transform posTransform, Transform negTransform, Vector3 localPlaneNormal)
        {
            // Bring back normal in world space
            Vector3 worldNormal = ((Vector3)(posTransform.worldToLocalMatrix.transpose * localPlaneNormal)).normalized;

            Vector3 separationVec = worldNormal * separation;
            // Transform direction in world coordinates
            posTransform.position += separationVec;
            negTransform.position -= separationVec;
        }

        void SeparateMeshes(List<GameObject> positives, List<GameObject> negatives, Vector3 worldPlaneNormal)
        {
            int i;
            var separationVector = worldPlaneNormal * separation;

            for (i = 0; i < positives.Count; ++i)
                positives[i].transform.position += separationVector;

            for (i = 0; i < negatives.Count; ++i)
                negatives[i].transform.position -= separationVector;
        }
    }
}