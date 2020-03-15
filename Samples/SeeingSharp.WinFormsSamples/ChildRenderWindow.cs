﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.WinFormsSamples
{
    public partial class ChildRenderWindow : Form
    {
        public ChildRenderWindow()
        {
            InitializeComponent();
        }

        public void InitializeChildWindow(Scene scene, Camera3DViewPoint viewPoint)
        {
            _ctrlRenderer.Scene = scene;
            _ctrlRenderer.Camera.ApplyViewPoint(viewPoint);
        }

        public async Task SetRenderingDataAsync(SampleBase actSample)
        {
            await actSample.OnInitRenderingWindowAsync(_ctrlRenderer.RenderLoop);

            await _ctrlRenderer.RenderLoop.Register2DDrawingLayerAsync(
                new PerformanceMeasureDrawingLayer(10f));
        }

        public async Task ClearAsync()
        {
            await _ctrlRenderer.RenderLoop.Clear2DDrawingLayersAsync();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.DesignMode)
            {
                return;
            }

            this.Text = $@"{this.Text} ({Assembly.GetExecutingAssembly().GetName().Version})";

            // Register viewbox filter
            _ctrlRenderer.RenderLoop.Filters.Add(new SceneViewboxObjectFilter());
        }

        private void OnRefreshTimer_Tick(object sender, EventArgs e)
        {
            this._renderWindowControlsComponent.UpdateTargetControlStates();
        }
    }
}
