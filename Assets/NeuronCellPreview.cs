﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace C2M2.NeuronalDynamics.Visualization
{
    using Interaction;
    using VRN;
    using Utils.DebugUtils;
    using Utils;
    using UGX;
    using Grid = UGX.Grid;
    using DiameterAttachment = UGX.IAttachment<UGX.DiameterData>;

    /// <summary>
    /// Produces a preview of a 1D cell using LinesRenderer
    /// </summary>
    public class NeuronCellPreview : MonoBehaviour
    {
        public string vrnFileName = "null";
        public Color color;
        public LoadSimulation loader = null;
        private VrnReader vrnReader = null;

        private void Awake()
        {
            PreviewCell(vrnFileName);
            if(loader == null)
            {
                loader = GetComponentInParent<LoadSimulation>();
            }
        }
        public void PreviewCell(string vrnFileName)
        {
            char sl = Path.DirectorySeparatorChar;
            if (!vrnFileName.EndsWith(".vrn")) vrnFileName = vrnFileName + ".vrn";
            vrnReader = new VrnReader(Application.streamingAssetsPath + sl + "NeuronalDynamics" + sl + "Geometries" + sl + vrnFileName);

            string meshName1D = vrnReader.Retrieve1DMeshName();

            /// Create empty grid with name of grid in archive
            Grid grid = new Grid(new Mesh(), meshName1D);
            grid.Attach(new DiameterAttachment());

            // Read the cell
            vrnReader.ReadUGX(meshName1D, ref grid);

            Debug.Log("1D cell info:\n\tcenter: " + grid.Mesh.bounds.center + "\n\tsize:" + grid.Mesh.bounds.size);

            // Scale the parent object by 1 / max scale to make the cell fit within size (1,1,1)
            float scale = 1 / Math.Max(grid.Mesh.bounds.size);
            transform.localScale = new Vector3(scale, scale, scale);

            // Adjust center so cell mesh is centered at (0,0,0)
            transform.localPosition = -scale * grid.Mesh.bounds.center;

            // Render cells
            LinesRenderer lines = gameObject.AddComponent<LinesRenderer>();
            // (line width = scale)
            lines.Draw(grid, color, scale);
        }
        public void LoadThisCell(RaycastHit hit)
        {
            loader.vrnFileName = vrnFileName;
            loader.Load(hit);
        }
    }
}