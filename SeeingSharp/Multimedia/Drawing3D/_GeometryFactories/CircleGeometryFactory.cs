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

using System.Numerics;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class CircleGeometryFactory : GeometryFactory
    {
        public float Radius
        {
            get;
            set;
        }

        public float Width
        {
            get;
            set;
        }

        public float Height
        {
            get;
            set;
        }

        public int CountOfSegments
        {
            get;
            set;
        }

        public CircleGeometryFactory()
        {
            this.Radius = 2f;
            this.Width = 0.5f;
            this.Height = 0.1f;
            this.CountOfSegments = 10;
        }

        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();
            var mainSurface = result.CreateSurface();
            mainSurface.BuildCircleFullV(Vector3.Zero, this.Radius, this.Width, this.Height, this.CountOfSegments);

            return result;
        }
    }
}