﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.SampleContainer.Basics3D._05_ParentChild
{
    [SampleDescription(
        "Parent/Child", 5, nameof(SeeingSharp.SampleContainer.Basics3D),
        sampleImageFileName: "PreviewImage.png")]
    public class ParentChildSample : SampleBase
    {
        /// <summary>
        /// Called when the sample has to startup.
        /// </summary>
        /// <param name="targetRenderLoop">The target render loop.</param>
        public override async Task OnStartupAsync(RenderLoop targetRenderLoop)
        {
            targetRenderLoop.EnsureNotNull(nameof(targetRenderLoop));

            // Build dummy scene
            Scene scene = targetRenderLoop.Scene;
            Camera3DBase camera = targetRenderLoop.Camera as Camera3DBase;

            await targetRenderLoop.Scene.ManipulateSceneAsync((manipulator) =>
            {
                // Create floor
                base.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create pallet geometry resource
                CubeType cubeType = new CubeType();
                var resCubeGeometry = manipulator.AddResource<GeometryResource>(
                    () => new GeometryResource(cubeType));

                //********************************
                // Create parent object
                GenericObject cubeObject = manipulator.AddGeneric(resCubeGeometry);
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

                //********************************
                // Create first level childs
                GenericObject actChild = manipulator.AddGeneric(resCubeGeometry);
                actChild.Position = new Vector3(-2f, 0f, 0f);
                actChild.Scaling = new Vector3(0.5f, 0.5f, 0.5f);
                manipulator.AddChild(cubeObject, actChild);

                actChild = manipulator.AddGeneric(resCubeGeometry);
                actChild.Position = new Vector3(0f, 0f, 2f);
                actChild.Scaling = new Vector3(0.5f, 0.5f, 0.5f);
                manipulator.AddChild(cubeObject, actChild);

                //********************************
                // Create second level parent/child relationships
                GenericObject actSecondLevelParent = manipulator.AddGeneric(resCubeGeometry);
                actSecondLevelParent.Position = new Vector3(3f, 0f, 0f);
                actSecondLevelParent.Scaling = new Vector3(0.8f, 0.8f, 0.8f);
                actSecondLevelParent.Color = Color4Ex.BlueColor;
                actSecondLevelParent.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => actSecondLevelParent.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();
                manipulator.AddChild(cubeObject, actSecondLevelParent);

                GenericObject actSecondLevelChild = manipulator.AddGeneric(resCubeGeometry);
                actSecondLevelChild.Position = new Vector3(1f, 0f, 0f);
                actSecondLevelChild.Scaling = new Vector3(0.4f, 0.4f, 0.4f);
                manipulator.AddChild(actSecondLevelParent, actSecondLevelChild);

                actSecondLevelChild = manipulator.AddGeneric(resCubeGeometry);
                actSecondLevelChild.Position = new Vector3(-1f, 0f, 0f);
                actSecondLevelChild.Scaling = new Vector3(0.4f, 0.4f, 0.4f);
                manipulator.AddChild(actSecondLevelParent, actSecondLevelChild);

                actSecondLevelChild = manipulator.AddGeneric(resCubeGeometry);
                actSecondLevelChild.Position = new Vector3(0f, 0f, 1f);
                actSecondLevelChild.Scaling = new Vector3(0.4f, 0.4f, 0.4f);
                manipulator.AddChild(actSecondLevelParent, actSecondLevelChild);
            });

            // Configure camera
            camera.Position = new Vector3(5f, 5f, 5f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            targetRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
        }
    }
}