using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Broccoli.BroccoEditor
{
    public class PreviewRenderUtility : UnityEditor.PreviewRenderUtility {
        public void DrawMeshB(Mesh mesh, Matrix4x4 matrix, Material mat, int subMeshIndex)
        {
            DrawMeshB(mesh, matrix, mat, subMeshIndex, null, null, false);
        }

        public void DrawMeshB(Mesh mesh, Matrix4x4 matrix, Material mat, int subMeshIndex, MaterialPropertyBlock customProperties)
        {
            DrawMeshB(mesh, matrix, mat, subMeshIndex, customProperties, null, false);
        }

        public void DrawMeshB(Mesh mesh, Matrix4x4 m, Material mat, int subMeshIndex, MaterialPropertyBlock customProperties, Transform probeAnchor, bool useLightProbe)
        {
            var quat = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
            var pos = m.GetColumn(3);
            var scale = new Vector3(
                m.GetColumn(0).magnitude,
                m.GetColumn(1).magnitude,
                m.GetColumn(2).magnitude
            );

            DrawMeshB(mesh, pos, scale, quat, mat, subMeshIndex, customProperties, probeAnchor, useLightProbe);
        }

        public void DrawMeshB(Mesh mesh, Vector3 pos, Quaternion rot, Material mat, int subMeshIndex)
        {
            DrawMeshB(mesh, pos, rot, mat, subMeshIndex, null, null, false);
        }

        public void DrawMeshB(Mesh mesh, Vector3 pos, Quaternion rot, Material mat, int subMeshIndex, MaterialPropertyBlock customProperties)
        {
            DrawMeshB(mesh, pos, rot, mat, subMeshIndex, customProperties, null, false);
        }

        public void DrawMeshB(Mesh mesh, Vector3 pos, Quaternion rot, Material mat, int subMeshIndex, MaterialPropertyBlock customProperties, Transform probeAnchor)
        {
            DrawMeshB(mesh, pos, rot, mat, subMeshIndex, customProperties, probeAnchor, false);
        }

        public void DrawMeshB(Mesh mesh, Vector3 pos, Quaternion rot, Material mat, int subMeshIndex, MaterialPropertyBlock customProperties, Transform probeAnchor, bool useLightProbe)
        {
            DrawMeshB(mesh, pos, Vector3.one, rot, mat, subMeshIndex, customProperties, probeAnchor, useLightProbe);
        }

        public void DrawMeshB(Mesh mesh, Vector3 pos, Vector3 scale, Quaternion rot, Material mat, int subMeshIndex, MaterialPropertyBlock customProperties, Transform probeAnchor, bool useLightProbe)
        {
            Graphics.DrawMesh(mesh, Matrix4x4.TRS(pos, rot, scale), mat, 1, camera, subMeshIndex, customProperties, UnityEngine.Rendering.ShadowCastingMode.TwoSided, true, probeAnchor, true);
        }
    }
}