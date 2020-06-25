﻿using System;
using UnityEngine;

namespace C2M2.Simulation
{
    using Utils;
    using Interaction.VR;
    /// <summary>
    /// Simulation of type Vector3[] for simulating positional fields
    /// </summary>
    public abstract class PositionFieldSimulation : Simulation<Vector3[], Transform[]>
    {
        protected Transform[] transforms;
        private Timer timer;
        private int trials = 1000;
        protected override void OnAwake()
        {
            transforms = BuildVisualization();
            Vector3[] pos = new Vector3[transforms.Length];

            // Make each transform a child of this gameobject so the hierarchy isn't flooded
            for(int i = 0; i < transforms.Length; i++)
            {
                // transforms[i].parent = transform;
                // pos[i] = transforms[i].position;
            }

            Collider[] colliders = new Collider[transforms.Length];
            for(int i = 0; i < transforms.Length; i++)
            {
                colliders[i] = transforms[i].GetComponent<Collider>();
            }
            VRRaycastableColliders raycastable = gameObject.AddComponent<VRRaycastableColliders>();
            raycastable.SetSource(colliders);

            // Add custom grabbable here

            // Init interaction
        }

        protected override void UpdateVisualization(in Vector3[] simulationValues)
        {
            for (int i = 0; i < simulationValues.Length; i++)
            {
                transforms[i].localPosition = simulationValues[i];
            }
            UpdateVisChild(simulationValues);
        }
        /// <summary>
        /// Allow derived classes to implement custom visualization features
        /// </summary>
        protected virtual void UpdateVisChild(in Vector3[] simulationValues) { }
    }
}
