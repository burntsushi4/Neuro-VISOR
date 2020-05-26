﻿using UnityEngine;
using System;

namespace C2M2.Simulation.Samples
{
    using Interaction;
    /// <summary>
    /// Store a scalar for each vertex on an existing MeshFilter.mesh
    /// </summary>
    /// <remarks>
    /// Good for testing interaction features
    /// </remarks>
    public class MeshSimulation : SurfaceSimulation
    {
        private double[] scalars;

        public override double[] GetValues() => scalars;
        public override void SetValues(RaycastHit hit)
        {
            Tuple<int, double>[] newValues = RaycastSimHeaterDiscrete.HitToTriangles(hit);

            foreach (Tuple<int, double> newVal in newValues)
            {
                scalars[newVal.Item1] += newVal.Item2;
            }
        }
        #region Unity Methods

        #endregion
        protected override void Solve()
        { // Do nothing, essentially
            while (true)
            {
                for (int i = 0; i < scalars.Length; i++)
                {
                }
            }
        }
        protected override Mesh BuildMesh()
        {
            MeshFilter meshf = GetComponent<MeshFilter>() ?? throw new MeshFilterNotFoundException();
            Mesh mesh = meshf.sharedMesh ?? throw new MeshNotFoundException();
            scalars = new double[mesh.vertexCount];
            return mesh;
        }
    }
}