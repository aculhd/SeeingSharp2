﻿#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SeeingSharp
{
    public static class Vector4Ex
    {
        public static bool EqualsWithTolerance(Vector4 left, Vector4 right, float tolerance = EngineMath.TOLERANCE_FLOAT_POSITIVE)
        {
            return
                EngineMath.EqualsWithTolerance(left.X, right.X, tolerance: tolerance) &&
                EngineMath.EqualsWithTolerance(left.Y, right.Y, tolerance: tolerance) &&
                EngineMath.EqualsWithTolerance(left.Z, right.Z, tolerance: tolerance) &&
                EngineMath.EqualsWithTolerance(left.W, right.W, tolerance: tolerance);
        }

        public static float GetValue(Vector4 vector, int index)
        {
            switch (index)
            {
                case 1: return vector.X;
                case 2: return vector.Y;
                case 3: return vector.Z;
                case 4: return vector.W;
                default: throw new ArgumentException("Invalid index!");
            }
        }

        public static void SetValue(Vector4 vector, int index, float value)
        {
            switch (index)
            {
                case 1: vector.X = value; break;
                case 2: vector.Y = value; break;
                case 3: vector.Z = value; break;
                case 4: vector.W = value; break;
                default: throw new ArgumentException("Invalid index!");
            }
        }
    }
}
