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

using SeeingSharp.Checking;
using SharpDX.DXGI;

namespace SeeingSharp.Multimedia.Core
{
    public static class GraphicsHelperUwp
    {
        /// <summary>
        /// Creates the SwapChain object that is used on WinRT platforms.
        /// </summary>
        /// <param name="device">The device on which to create the SwapChain.</param>
        /// <param name="width">Width of the screen in pixels.</param>
        /// <param name="height">Height of the screen in pixels.</param>
        /// <param name="gfxConfig">Current graphics configuration.</param>
        internal static SwapChain1 CreateSwapChainForComposition(EngineDevice device, int width, int height, GraphicsViewConfiguration gfxConfig)
        {
            device.EnsureNotNull(nameof(device));
            width.EnsurePositiveOrZero(nameof(width));
            height.EnsurePositiveOrZero(nameof(height));
            gfxConfig.EnsureNotNull(nameof(gfxConfig));

            var desc = new SwapChainDescription1
            {
                Width = width,
                Height = height,
                Format = GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT,
                Stereo = false,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.BackBuffer | Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipSequential,
                AlphaMode = gfxConfig.AlphaEnabledSwapChain ? AlphaMode.Premultiplied : AlphaMode.Ignore
            };

            //Creates the swap chain for XAML composition
            return new SwapChain1(device.Internals.FactoryDxgi, device.Internals.DeviceD3D11_1, ref desc);
        }
    }
}