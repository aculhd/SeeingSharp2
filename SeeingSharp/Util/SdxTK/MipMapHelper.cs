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

// This code is ported from SharpDX.Toolkit
// see: https://github.com/sharpdx/Toolkit

using System;

namespace SeeingSharp.Util.SdxTK
{
    internal static class MipMapHelper
    {
        /// <summary>
        /// Calculates the number of miplevels for a Texture 2D.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="mipLevels">A <see cref="MipMapCount"/>, set to true to calculates all mipmaps, to false to calculate only 1 miplevel, or > 1 to calculate a specific amount of levels.</param>
        /// <returns>The number of miplevels.</returns>
        public static int CalculateMipLevels(int width, int height, MipMapCount mipLevels)
        {
            if (mipLevels > 1)
            {
                var maxMips = CountMips(width, height);
                if (mipLevels > maxMips)
                {
                    throw new InvalidOperationException($"MipLevels must be <= {maxMips}");
                }
            }
            else if (mipLevels == 0)
            {
                mipLevels = CountMips(width, height);
            }
            else
            {
                mipLevels = 1;
            }
            return mipLevels;
        }

        /// <summary>
        /// Calculates the number of miplevels for a Texture 2D.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="depth">The depth of the texture.</param>
        /// <param name="mipLevels">A <see cref="MipMapCount"/>, set to true to calculates all mipmaps, to false to calculate only 1 miplevel, or > 1 to calculate a specific amount of levels.</param>
        /// <returns>The number of miplevels.</returns>
        public static int CalculateMipLevels(int width, int height, int depth, MipMapCount mipLevels)
        {
            if (mipLevels > 1)
            {
                if (!IsPow2(width) || !IsPow2(height) || !IsPow2(depth))
                {
                    throw new InvalidOperationException("Width/Height/Depth must be power of 2");
                }

                var maxMips = CountMips(width, height, depth);
                if (mipLevels > maxMips)
                {
                    throw new InvalidOperationException($"MipLevels must be <= {maxMips}");
                }
            }
            else if (mipLevels == 0)
            {
                if (!IsPow2(width) || !IsPow2(height) || !IsPow2(depth))
                {
                    throw new InvalidOperationException("Width/Height/Depth must be power of 2");
                }

                mipLevels = CountMips(width, height, depth);
            }
            else
            {
                mipLevels = 1;
            }
            return mipLevels;
        }

        private static bool IsPow2(int x)
        {
            return x != 0 && (x & (x - 1)) == 0;
        }

        private static int CountMips(int width, int height)
        {
            var mipLevels = 1;

            while (height > 1 || width > 1)
            {
                ++mipLevels;

                if (height > 1)
                {
                    height >>= 1;
                }

                if (width > 1)
                {
                    width >>= 1;
                }
            }

            return mipLevels;
        }

        private static int CountMips(int width, int height, int depth)
        {
            var mipLevels = 1;

            while (height > 1 || width > 1 || depth > 1)
            {
                ++mipLevels;

                if (height > 1)
                {
                    height >>= 1;
                }

                if (width > 1)
                {
                    width >>= 1;
                }

                if (depth > 1)
                {
                    depth >>= 1;
                }
            }

            return mipLevels;
        }
    }
}
