﻿#region License information
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.SampleContainer
{
    public abstract class SampleBase
    {
        public abstract Task OnStartupAsync(RenderLoop targetRenderLoop, SampleSettings settings);

        public virtual void NotifyClosed()
        {

        }

        /// <summary>
        /// Builds a floor to the given scene.
        /// </summary>
        protected void BuildStandardFloor(SceneManipulator manipulator, string sceneLayer)
        {
            SceneLayer bgLayer = manipulator.AddLayer("BACKGROUND");
            manipulator.SetLayerOrderID(bgLayer, 0);
            manipulator.SetLayerOrderID(Scene.DEFAULT_LAYER_NAME, 1);
            ResourceLink sourceBackgroundTexture = new AssemblyResourceLink(
                typeof(SampleBase),
                "Assets.Background.dds");
            ResourceLink sourceTileTexture = new AssemblyResourceLink(
                typeof(SampleBase),
                "Assets.Floor.dds");

            var resBackgroundTexture = manipulator.AddTexture(sourceBackgroundTexture);
            manipulator.Add(new FullscreenTextureObject(resBackgroundTexture), bgLayer.Name);

            // Define textures and materials
            var resTileTexture = manipulator.AddResource(() => new StandardTextureResource(sourceTileTexture));
            var resTileMaterial = manipulator.AddResource(() => new SimpleColoredMaterialResource(resTileTexture));

            // Define floor geometry
            FloorType floorType = new FloorType(new Vector2(4f, 4f), 0f);
            floorType.BottomMaterial = resTileMaterial;
            floorType.DefaultFloorMaterial = resTileMaterial;
            floorType.SideMaterial = resTileMaterial;
            floorType.SetTilemap(25, 25);

            // Add floor to scene
            var resFloorGeometry = manipulator.AddResource((() => new GeometryResource(floorType)));
            var floorObject = manipulator.AddGeneric(resFloorGeometry, sceneLayer);
        }
    }
}
