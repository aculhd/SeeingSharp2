﻿/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Basics3D._05_Direct2DTextureAnimated
{
    [SampleDescription(
        "Direct2D Texture 2", 5, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics3D/_05_Direct2DTextureAnimated",
        typeof(SampleSettingsWith3D))]
    public class Direct2DTextureAnimatedSample : SampleBase
    {
        private SolidBrushResource _animatedRectBrush;
        private SolidBrushResource _solidBrush;
        private TextFormatResource _textFormat;

        public override async Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            // Whole animation takes x milliseconds
            const float animationMillis = 3000f;

            // 2D rendering is made here
            _solidBrush = new SolidBrushResource(Color4.Gray);
            _animatedRectBrush = new SolidBrushResource(Color4.RedColor);

            var d2DDrawingLayer = new Custom2DDrawingLayer(graphics =>
            {
                // Draw the background
                var d2DRectangle = new RectangleF(10, 10, 236, 236);
                graphics.Clear(Color4.LightBlue);
                graphics.FillRoundedRectangle(
                    d2DRectangle, 30, 30,
                    _solidBrush);

                // Recalculate current location of the red rectangle on each frame
                var currentLocation = (float)(DateTime.UtcNow - DateTime.UtcNow.Date).TotalMilliseconds % animationMillis / animationMillis;
                var rectPos = this.GetAnimationLocation(currentLocation, 165f, 165f);
                graphics.FillRectangle(
                    new RectangleF(
                        20f + rectPos.x,
                        20f + rectPos.y,
                        50f, 50f),
                    _animatedRectBrush);
            });

            // Build 3D scene
            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Create floor
                this.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Define Direct2D texture resource
                var resD2DTexture = manipulator.AddResource(
                    device => new Direct2DTextureResource(d2DDrawingLayer, 256, 256));
                var resD2DMaterial = manipulator.AddStandardMaterialResource(resD2DTexture);

                // Create cube geometry resource
                var resGeometry = manipulator.AddResource(
                    device => new GeometryResource(new CubeGeometryFactory()));

                // Create cube object
                var cubeMesh = new Mesh(resGeometry, resD2DMaterial);
                cubeMesh.Color = Color4.GreenColor;
                cubeMesh.YPos = 0.5f;
                cubeMesh.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => cubeMesh.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();
                manipulator.AddObject(cubeMesh);
            });
        }

        public override Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            var camera = mainOrChildRenderLoop.Camera;

            // ConfigureLoading camera
            camera.Position = new Vector3(3f, 3f, 3f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());

            // Add object filter for viewbox culling
            mainOrChildRenderLoop.ObjectFilters.Add(new SceneViewboxObjectFilter());

            return Task.FromResult<object>(null);
        }

        public override void OnSampleClosed()
        {
            base.OnSampleClosed();

            SeeingSharpUtil.SafeDispose(ref _solidBrush);
            SeeingSharpUtil.SafeDispose(ref _animatedRectBrush);
            SeeingSharpUtil.SafeDispose(ref _textFormat);
        }

        public (float x, float y) GetAnimationLocation(float procentualLoc, float maxWidth, float maxHeight)
        {
            var xPos = 0f;
            var yPos = 0f;
            var currentLineLoc = procentualLoc % 0.25f / 0.25f;
            if (procentualLoc < 0.25f)
            {
                xPos = maxWidth * currentLineLoc;
                yPos = 0f;
            }
            else if (procentualLoc < 0.5f)
            {
                xPos = maxWidth;
                yPos = maxHeight * currentLineLoc;
            }
            else if (procentualLoc < 0.75f)
            {
                xPos = maxWidth - maxWidth * currentLineLoc;
                yPos = maxHeight;
            }
            else
            {
                xPos = 0f;
                yPos = maxHeight - maxHeight * currentLineLoc;
            }

            return (xPos, yPos);
        }
    }
}
