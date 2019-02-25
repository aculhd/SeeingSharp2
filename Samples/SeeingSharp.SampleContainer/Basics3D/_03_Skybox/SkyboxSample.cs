﻿#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

namespace SeeingSharp.SampleContainer.Basics3D._03_Skybox
{
    #region using

    using System;
    using System.Threading.Tasks;
    using Checking;
    using Multimedia.Components;
    using Multimedia.Core;
    using Multimedia.Drawing3D;
    using Multimedia.Objects;
    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    [SampleDescription(
        "Skybox", 3, nameof(SeeingSharp.SampleContainer.Basics3D),
        sampleImageFileName: "PreviewImage.png",
        sourceCodeUrl: "https://github.com/RolandKoenig/SeeingSharp2/tree/master/_Samples/SeeingSharp.SampleContainer/Basics3D/_03_Skybox")]
    public class SkyboxSample : SampleBase
    {
        /// <summary>
        /// Called when the sample has to startup.
        /// </summary>
        public override async Task OnStartupAsync(RenderLoop targetRenderLoop, SampleSettings settings)
        {
            targetRenderLoop.EnsureNotNull(nameof(targetRenderLoop));

            // Build dummy scene
            Scene scene = targetRenderLoop.Scene;
            Camera3DBase camera = targetRenderLoop.Camera as Camera3DBase;

            await scene.ManipulateSceneAsync((manipulator) =>
            {
                // Create floor
                this.BuildStandardFloor(manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create cube geometry resource
                CubeType cubeType = new CubeType();
                var resPalletGeometry = manipulator.AddResource<GeometryResource>(
                    () => new GeometryResource(cubeType));

                // Create cube object
                GenericObject cubeObject = manipulator.AddGeneric(resPalletGeometry);
                cubeObject.Color = Color4Ex.GreenColor;
                cubeObject.Position = new Vector3(0f, 0.5f, 0f);
                cubeObject.EnableShaderGeneratedBorder();
                cubeObject.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => cubeObject.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();

                var resSkyboxTexture = manipulator.AddTexture(
                    new AssemblyResourceLink(
                       this.GetType(),
                        "SkyBox.dds"));

                // Create the skybox on a new layer
                manipulator.AddLayer("Skybox");
                SkyboxObject skyboxObject = new SkyboxObject(resSkyboxTexture);
                manipulator.Add(skyboxObject, "Skybox");
            });

            // Configure camera
            camera.Position = new Vector3(3f, 1f, 3f);
            camera.Target = new Vector3(0f, 1.2f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            targetRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
        }
    }
}
