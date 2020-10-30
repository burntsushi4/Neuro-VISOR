﻿using UnityEngine;
using C2M2.Visualization;
using C2M2.Utils.MeshUtils;

namespace C2M2.Simulation
{
    using Utils;
    using Interaction.VR;
    /// <summary>
    /// Simulation of type double[] for visualizing scalar fields on mesh surfaces
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public abstract class MeshSimulation : Simulation<double[], Mesh, VRRaycastableMesh, VRGrabbableMesh>
    {
        #region Variables

        /// <summary>
        /// Gradient for coloring each surface point based on their scalar values
        /// </summary>
        public Gradient gradient;

        /// <summary>
        /// Lookup table for more efficient color calculations on the gradient
        /// </summary>
        public LUTGradient colorLUT { get; private set; } = null;

        public LUTGradient.ExtremaMethod extremaMethod = LUTGradient.ExtremaMethod.GlobalExtrema;
        [Tooltip("Must be set if extremaMethod is set to GlobalExtrema")]
        public float globalMax = float.NegativeInfinity;
        [Tooltip("Must be set if extremaMethod is set to GlobalExtrema")]
        public float globalMin = float.PositiveInfinity;

        private Mesh visualMesh = null;
        public Mesh VisualMesh
        {
            get
            {
                return visualMesh;
            }
            protected set
            {
                //if (value == null) return;
                visualMesh = value;

                var mf = GetComponent<MeshFilter>();
                if (mf == null) gameObject.AddComponent<MeshFilter>();
                if (GetComponent<MeshRenderer>() == null)
                    gameObject.AddComponent<MeshRenderer>().sharedMaterial = GameManager.instance.vertexColorationMaterial;
                mf.sharedMesh = visualMesh;
            }
        }
        private Mesh colliderMesh = null;
        public Mesh ColliderMesh
        {
            get { return colliderMesh; }
            protected set
            {
                //if (value == null) return;
                colliderMesh = value;

                var cont = GetComponent<VRRaycastableMesh>();
                if (cont == null) { cont = gameObject.AddComponent<VRRaycastableMesh>(); }
                cont.SetSource(colliderMesh);
            }
        }

        private MeshFilter mf;
        private MeshRenderer mr;
        #endregion

        /// <summary>
        /// Update vertex colors based on simulation values
        /// </summary>
        private void UpdateVisualization(in float[] scalars3D)
        {
            Color32[] newCols = colorLUT.Evaluate(scalars3D);
            if(newCols != null)
            {
                mf.mesh.colors32 = newCols;
            }
        }
        protected override void UpdateVisualization(in double[] newValues) => UpdateVisualization(newValues.ToFloat());

        protected override void OnAwakePost(Mesh viz)
        {
            if (!dryRun)
            {
                InitRenderer();
                InitColors();
                InitInteraction();
            }
            return;

            void InitRenderer()
            {
                // Safe check for existing MeshFilter, MeshRenderer
                mf = GetComponent<MeshFilter>();
                if (mf == null) mf = gameObject.AddComponent<MeshFilter>();
                mf.sharedMesh = viz;

                mr = GetComponent<MeshRenderer>();
                if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();

                // Ensure the renderer has a vertex coloring material     
                mr.material = GameManager.instance.vertexColorationMaterial;
            }
            void InitColors()
            {
                // Initialize the color lookup table
                colorLUT = gameObject.AddComponent<LUTGradient>();
                colorLUT.Gradient = gradient;
                colorLUT.extremaMethod = extremaMethod;
                if (extremaMethod == LUTGradient.ExtremaMethod.GlobalExtrema)
                {
                    colorLUT.globalMax = globalMax;
                    colorLUT.globalMin = globalMin;
                }
            }
            void InitInteraction()
            {
                VRRaycastableMesh raycastable = gameObject.AddComponent<VRRaycastableMesh>();
                if (ColliderMesh != null) raycastable.SetSource(ColliderMesh);
                else raycastable.SetSource(viz);

                gameObject.AddComponent<VRGrabbableMesh>();
            }
        }
    }
}