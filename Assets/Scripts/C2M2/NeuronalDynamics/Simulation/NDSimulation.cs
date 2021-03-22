﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using C2M2.NeuronalDynamics.UGX;
using UnityEditor;
using UnityEngine;
using System.Threading;
using DiameterAttachment = C2M2.NeuronalDynamics.UGX.IAttachment<C2M2.NeuronalDynamics.UGX.DiameterData>;
using MappingAttachment = C2M2.NeuronalDynamics.UGX.IAttachment<C2M2.NeuronalDynamics.UGX.MappingData>;
using TMPro;
using Math = C2M2.Utils.Math;
using C2M2.Interaction;
using C2M2.Simulation;
using C2M2.Utils.DebugUtils;
using C2M2.Utils.MeshUtils;
using Grid = C2M2.NeuronalDynamics.UGX.Grid;
using C2M2.NeuronalDynamics.Visualization.VRN;
using C2M2.NeuronalDynamics.Interaction;
using C2M2.Visualization;

namespace C2M2.NeuronalDynamics.Simulation {

    /// <summary>
    /// Provide an interface for 1D neuron-surface simulations to be visualized and interacted with
    /// </summary>
    /// <remarks>
    /// 1D Neuron surface simulations should derive from this class.
    /// </remarks>
    public abstract class NDSimulation : MeshSimulation {

        private double visualInflation = 1;
        public double VisualInflation
        {
            get { return visualInflation; }
            set
            {
                if (visualInflation != value)
                {
                    visualInflation = value;
                    if (ColliderInflation < visualInflation) ColliderInflation = visualInflation;

                    Update2DGrid();

                    VisualMesh = Grid2D.Mesh;
                    OnVisualInflationChange?.Invoke(visualInflation);
                }
            }
        }

        public delegate void OnVisualInflationChangeDelegate(double newInflation);
        public event OnVisualInflationChangeDelegate OnVisualInflationChange;

        private double colliderInflation = 1;
        public double ColliderInflation
        {
            get { return colliderInflation; }
            set
            {
                if (colliderInflation != value)
                {
                    if (value < visualInflation) return;
                    colliderInflation = value;
                    ColliderMesh = CheckMeshCache(colliderInflation);
                }
            }
        }

        private int refinementLevel = 0;
        public int RefinementLevel
        {
            get { return refinementLevel; }
            set
            {
                if (refinementLevel != value && value >= 0)
                {
                    refinementLevel = value;
                    UpdateGrid1D();
                }
            }
        }

        private Dictionary<double, Mesh> meshCache = new Dictionary<double, Mesh>();

        public NeuronClampManager ClampManager
        {
            get
            {
                return GameManager.instance.ndClampManager;
            }
        }
        public List<NeuronClamp> clamps = new List<NeuronClamp>();
        public Mutex clampMutex { get; private set; } = new Mutex();
        private static Tuple<int, double> nullClamp = new Tuple<int, double>(-1, -1);

        [Header ("1D Visualization")]
        public bool visualize1D = false;
        public Color32 color1D = Color.yellow;
        public float lineWidth1D = 0.005f;

        public GameObject controlPanel = null;

        /// <summary>
        /// Alter the precision of the color scale display
        /// </summary>
        [Tooltip("Alter the precision of the color scale display")]
        public int colorScalePrecision = 3;

        // Need mesh options for each refinement, diameter level
        [Tooltip("Name of the vrn file within Assets/StreamingAssets/NeuronalDynamics/Geometries")]
        public string vrnFileName = "test.vrn";
        private VrnReader vrnReader = null;
        private VrnReader VrnReader
        {
            get
            {
                if (vrnReader == null)
                {
                    char sl = Path.DirectorySeparatorChar;
                    if (!vrnFileName.EndsWith(".vrn")) vrnFileName = vrnFileName + ".vrn";
                    vrnReader = new VrnReader(Application.streamingAssetsPath + sl + "NeuronalDynamics" + sl + "Geometries" + sl + vrnFileName);
                }
                return vrnReader;
            }
            set { vrnReader = value; }
        }

        private Grid grid1D = null;
        public Grid Grid1D
        {
            get {
                return grid1D;
            }
            set
            {
                grid1D = value;
            }
        }

        public Vector3[] Verts1D { get { return grid1D.Mesh.vertices; } }

        private Grid grid2D = null;
        public Grid Grid2D
        {
            get
            {
                return grid2D;
            }
            set
            {
                grid2D = value;
            }
        }

        private Neuron neuron = null;
        public Neuron Neuron
        {
            get { return neuron; }
            set { neuron = value; }
        }

        private float averageDendriteRadius = 0;
        public float AverageDendriteRadius
        {
            get
            {
                if (averageDendriteRadius == 0)
                {
                    float radiusSum = 0;
                    foreach (Neuron.NodeData node in Neuron.nodes)
                    {
                        radiusSum += (float) node.NodeRadius;
                    }
                    averageDendriteRadius = radiusSum / Neuron.nodes.Count;
                }
                return averageDendriteRadius;
            }
        }

        // Stores the information from mapping in an array of structs.
        // Performs much better than using mapping directly.
        private Vert3D1DPair[] map = null;
        public Vert3D1DPair[] Map
        {
            get
            {
                if(map == null)
                {
                    map = new Vert3D1DPair[Mapping.Data.Count];
                    for(int i = 0; i < Mapping.Data.Count; i++)
                    {
                        map[i] = new Vert3D1DPair(Mapping.Data[i].Item1, Mapping.Data[i].Item2, Mapping.Data[i].Item3);
                    }
                }
                return map;
            }
        }
        private MappingInfo mapping = default;
        private MappingInfo Mapping
        {
            get
            {
                if(mapping.Equals(default(MappingInfo)))
                {
                    mapping = (MappingInfo)MapUtils.BuildMap(Grid1D, Grid2D);
                }
                return mapping;
            }
            set
            {
                mapping = (MappingInfo)MapUtils.BuildMap(Grid1D, Grid2D);
            }
        }

        private double[] scalars3D = new double[0];
        private double[] Scalars3D
        {
            get
            {
                if(scalars3D.Length == 0)
                {
                    scalars3D = new double[Mapping.Data.Count];
                }
                return scalars3D;
            }
        }

        protected override void PostSolveStep()
        {
            ApplyInteractionVals();
            void ApplyInteractionVals()
            {
                ///<c>if (clamps != null && clamps.Count > 0)</c> this if statement is where we apply voltage clamps   
                // Apply clamp values, if there are any clamps
                if (clamps.Count > 0)
                {
                    Tuple<int, double>[] clampValues = new Tuple<int, double>[clamps.Count];
                    clampMutex.WaitOne();
                    for (int i = 0; i < clamps.Count; i++)
                    {
                        if (clamps[i] != null && clamps[i].FocusVert != -1 && clamps[i].ClampLive)
                        {
                            clampValues[i] = new Tuple<int, double>(clamps[i].FocusVert, clamps[i].ClampPower);
                        }
                        else
                        {
                            clampValues[i] = nullClamp;
                        }
                    }
                    clampMutex.ReleaseMutex();
                    Set1DValues(clampValues);
                }

                // Apply raycast values
                if (raycastHits.Length > 0)
                {
                    Set1DValues(raycastHits);
                }
            }
        }

        /// <summary>
        /// Translate 1D vertex values to 3D values and pass them upwards for visualization
        /// </summary>
        /// <returns> One scalar value for each 3D vertex based on its 1D vert's scalar value </returns>
        public sealed override double[] GetValues () {
            double[] scalars1D = Get1DValues ();

            if (scalars1D == null) { return null; }
            //double[] scalars3D = new double[map.Length];
            for (int i = 0; i < Map.Length; i++) { // for each 3D point,

                // Take an weighted average using lambda
                // Equivalent to [lambda * v2 + (1 - lambda) * v1]
                double newVal = map[i].lambda * (scalars1D[map[i].v2] - scalars1D[map[i].v1]) + scalars1D[map[i].v1];

                Scalars3D[i] = newVal;
            }
            return Scalars3D;
        }

        /// <summary>
        /// Translate 3D vertex values to 1D values, and pass them downwards for interaction
        /// </summary>
        public sealed override void SetValues (RaycastHit hit) {
            // We will have 3 new index/value pairings
            Tuple<int, double>[] newValues = new Tuple<int, double>[3];
            // Translate hit triangle index so we can index into triangles array
            int triInd = hit.triangleIndex * 3;
            MeshFilter mf = hit.transform.GetComponentInParent<MeshFilter>();
            // Get mesh vertices from hit triangle
            int v1 = mf.mesh.triangles[triInd];
            int v2 = mf.mesh.triangles[triInd + 1];
            int v3 = mf.mesh.triangles[triInd + 2];
            // Attach new values to new vertices
            newValues[0] = new Tuple<int, double>(v1, raycastHitValue);
            newValues[1] = new Tuple<int, double>(v2, raycastHitValue);
            newValues[2] = new Tuple<int, double>(v3, raycastHitValue);

            SetValues (newValues);
        }
        /// <summary>
        /// Translate 3D vertex values to 1D values, and pass them downwards for interaction
        /// </summary>
        public void SetValues (Tuple<int, double>[] newValues) {
            // Reserve space for new1DValuess
            Tuple<int, double>[] new1Dvalues = new Tuple<int, double>[newValues.Length];
            // Receive values given to 3D vertices, translate them onto 1D vertices and apply values there
            for (int i = 0; i < newValues.Length; i++)
            {    
                int vert3D = newValues[i].Item1;
                double val3D = newValues[i].Item2;

                // If lambda > 0.5, the vert is closer to v2 so apply val3D there
                int vert1D = (map[vert3D].lambda > 0.5) ? map[vert3D].v2 : map[vert3D].v1;
                new1Dvalues[i] = new Tuple<int, double>(vert1D, val3D);
            }
            raycastHits = new1Dvalues;
        }

        /// <summary>
        /// Requires deived classes to know how to receive one value to add onto each 1D vert index
        /// </summary>
        /// <param name="newValues"> List of 1D vert indices and values to add onto that index. </param>
        public abstract void Set1DValues (Tuple<int, double>[] newValues);

        /// <summary>
        /// Requires derived classes to know how to make available one value for each 1D vertex
        /// </summary>
        /// <returns></returns>
        public abstract double[] Get1DValues ();

        public abstract double Get1DValue(int index1D);
        protected override void OnAwakePre()
        {
            UpdateGrid1D();
            base.OnAwakePre();
        }

        /// <summary>
        /// Read in the cell and initialize 3D/1D visualization/interaction infrastructure
        /// </summary>
        /// <returns> Unity Mesh visualization of the 3D geometry. </returns>
        /// <remarks> BuildVisualization is called by Simulation.cs,
        /// it is called after OnAwakePre and before OnAwakePost.
        /// If dryRun == true, Simulation will not call BuildVisualization. </remarks>
        protected override Mesh BuildVisualization () {
            if (!dryRun) {

                if (visualize1D) Render1DCell ();

                Update2DGrid();

                VisualMesh = Grid2D.Mesh;
                VisualMesh.Rescale (transform, new Vector3 (4, 4, 4));
                VisualMesh.RecalculateNormals ();

                // Pass blownupMesh upwards to MeshSimulation
                ColliderMesh = VisualMesh;

                InitUI ();
            }

            return VisualMesh;

            void Render1DCell () {
                Grid geom1D = Mapping.ModelGeometry;
                GameObject lines1D = gameObject.AddComponent<LinesRenderer> ().Draw (geom1D, color1D, lineWidth1D);
            }
            void InitUI () {
                // Instantiate control panel prefab, announce active simulation to buttons
                controlPanel = Resources.Load ("Prefabs/NeuronalDynamics/ControlPanel/NDControls") as GameObject;        
                controlPanel = GameObject.Instantiate(controlPanel);

                NDFeatureToggle[] toggles = controlPanel.GetComponentsInChildren<NDFeatureToggle>();
                foreach(NDFeatureToggle toggle in toggles)
                {
                    toggle.sim = this;
                }

                // Find the close button, report this simulation
                CloseNDSimulation closeButton = controlPanel.GetComponentInChildren<CloseNDSimulation>();
                if(closeButton != null)
                {
                    closeButton.sim = this;
                }

                // Find gradient display and attach our values
                GradientDisplay gradientDisplay = controlPanel.GetComponentInChildren<GradientDisplay>();
                if (gradientDisplay != null)
                {
                    gradientDisplay.sim = this;
                    if (colorLUT != null)
                    {
                        gradientDisplay.gradient = colorLUT.Gradient;
                    }
                    else if (colorLUT == null) { Debug.LogWarning("No ColorLUT found on MeshSimulation"); }

                    gradientDisplay.precision = "F" + colorScalePrecision.ToString();
                }
                else if (gradientDisplay == null) { Debug.LogWarning("No GradientDisplay found on NDControls"); }

                SimulationTimerLabel timeLabel = controlPanel.GetComponentInChildren<SimulationTimerLabel>();
                if (timeLabel != null)
                {
                    timeLabel.sim = this;
                }
            }
        }

        // TODO: Obsolete
        private Mesh CheckMeshCache(double inflation)
        {
            if (!meshCache.ContainsKey(inflation) || meshCache[inflation] == null)
            {
                string meshName = VrnReader.Retrieve2DMeshName(inflation);

                Grid grid = new Grid(new Mesh(), meshName);
                VrnReader.ReadUGX(meshName, ref grid);
                meshCache[inflation] = grid.Mesh;
            }
            return meshCache[inflation];
        }

        public void SwitchColliderMesh (double inflation) {
            inflation = Math.Clamp (inflation, 1, 5);
            ColliderInflation = inflation;
        }

        public void SwitchMesh (double inflation) {
            inflation = Math.Clamp (inflation, 1, 5);
            VisualInflation = inflation;
        }

        private void UpdateGrid1D()
        {
            string meshName1D = VrnReader.Retrieve1DMeshName(RefinementLevel);
            /// Create empty grid with name of grid in archive
            Grid1D = new Grid(new Mesh(), meshName1D);
            Grid1D.Attach(new DiameterAttachment());

            VrnReader.ReadUGX(meshName1D, ref grid1D);

            Neuron = new Neuron(grid1D);
        }
        private void Update2DGrid()
        {
            /// Retrieve mesh names from archive
            string meshName2D = VrnReader.Retrieve2DMeshName(VisualInflation);

            /// Empty 2D grid which stores geometry + mapping data
            Grid2D = new Grid(new Mesh(), meshName2D);
            Grid2D.Attach(new MappingAttachment());
            VrnReader.ReadUGX(meshName2D, ref grid2D);
        }

        public int GetNearestPoint(RaycastHit hit)
        {
            if (mf == null) return -1;

            // Get 3D mesh vertices from hit triangle
            int triInd = hit.triangleIndex * 3;
            int v1 = mf.mesh.triangles[triInd];
            int v2 = mf.mesh.triangles[triInd + 1];
            int v3 = mf.mesh.triangles[triInd + 2];

            // Find 1D verts belonging to these 3D verts
            int[] verts1D = new int[]
            {
                Map[v1].v1, Map[v1].v2,
                Map[v2].v1, Map[v2].v2,
                Map[v3].v1, Map[v3].v2
            };
            Vector3 localHitPoint = transform.InverseTransformPoint(hit.point);

            float nearestDist = float.PositiveInfinity;
            int nearestVert1D = -1;
            foreach (int vert in verts1D)
            {
                float dist = Vector3.Distance(localHitPoint, Verts1D[vert]);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestVert1D = vert;
                }
            }

            return nearestVert1D;
        }
    }

    /// <summary>
    /// Stores two 1D indices and a lambda value for a 3D vertex
    /// </summary>
    /// <remarks>
    /// Lambda is a value between 0 and 1. A lambda value greater than 0.5 implies that the 3D vert lies closer to v2.
    /// A lambda value of 0 would imply that the 3D vert lies directly over v1,
    /// and a lambda of 1 implies that it lies completely over v2.
    /// </remarks>
    public struct Vert3D1DPair
    {
        public int v1 { get; private set; }
        public int v2 { get; private set; }
        public double lambda { get; private set; }

        public Vert3D1DPair(int v1, int v2, double lambda)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.lambda = lambda;
        }
        public override string ToString()
        {
            return "v1: " + v1 + "\nv2: " + v2 + "\nlambda: " + lambda;
        }
    }
}
