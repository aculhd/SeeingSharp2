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

using System.ComponentModel;
using System.Numerics;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Primitives3D._01_Cube
{
    [SampleDescription(
        "Cube", 1, nameof(Primitives3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Primitives3D/_01_Cube",
        typeof(CubeSampleSettings))]
    public class CubeSample : Primitive3DSampleBase
    {
        protected override Mesh CreateMesh(SceneManipulator manipulator, SampleSettings sampleSettings, NamedOrGenericKey resMaterial)
        {
            var castedSettings = (CubeSampleSettings) sampleSettings;

            var resGeometry = manipulator.AddResource(
                device => new GeometryResource(
                    new CubeGeometryFactory
                    {
                        Width = castedSettings.Width,
                        Height = castedSettings.Height,
                        Depth = castedSettings.Depth
                    }));

            var result = new Mesh(resGeometry, resMaterial);
            result.Position = new Vector3(0f, 0.5f + castedSettings.Height / 2f, 0f);
            return result;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class CubeSampleSettings : Primitive3DSampleSettings
        {
            private float _width = 1f;
            private float _height = 1f;
            private float _depth = 1f;

            [Category(CATEGORY_NAME)]
            public float Width
            {
                get => _width;
                set
                {
                    if (!EngineMath.EqualsWithTolerance(_width, value))
                    {
                        _width = value;
                        this.RaiseRecreateRequest();
                    }
                }
            }

            [Category(CATEGORY_NAME)]
            public float Height
            {
                get => _height;
                set
                {
                    if (!EngineMath.EqualsWithTolerance(_height, value))
                    {
                        _height = value;
                        this.RaiseRecreateRequest();
                    }
                }
            }

            [Category(CATEGORY_NAME)]
            public float Depth
            {
                get => _depth;
                set
                {
                    if (!EngineMath.EqualsWithTolerance(_depth, value))
                    {
                        _depth = value;
                        this.RaiseRecreateRequest();
                    }
                }
            }
        }
    }
}