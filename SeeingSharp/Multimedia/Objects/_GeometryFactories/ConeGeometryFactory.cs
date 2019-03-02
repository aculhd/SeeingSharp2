﻿#region License information
/*
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
#endregion

using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    #region using
    #endregion

    public class ConeGeometryFactory : GeometryFactory
    {
        public override VertexStructure BuildStructure(StructureBuildOptions buildOptions)
        {
            var result = new VertexStructure();
            var mainSurface = result.CreateSurface();
            mainSurface.BuildConeFullV(Vector3.Zero, Radius, Height, CountOfSegments, Color4Ex.Transparent);

            return result;
        }

        public ConeGeometryFactory()
        {
            Radius = 0.5f;
            Height = 1f;
            CountOfSegments = 10;
        }

        public float Radius
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
    }
}