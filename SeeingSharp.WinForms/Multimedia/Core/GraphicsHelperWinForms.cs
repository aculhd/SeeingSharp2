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

#region using

// Namespace mappings
using GDI = System.Drawing;
using D3D11 = SharpDX.Direct3D11;
using WinForms = System.Windows.Forms;

#endregion

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using Checking;
    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    public static class GraphicsHelperWinForms
    {
        /// <summary>
        /// Creates a default SwapChain for the given target control.
        /// </summary>
        /// <param name="targetControl">Target control of the swap chain.</param>
        /// <param name="device">Graphics device.</param>
        /// <param name="gfxConfig">The current graphics configuration.</param>
        internal static SharpDX.DXGI.SwapChain1 CreateSwapChainForWinForms(WinForms.Control targetControl, EngineDevice device, GraphicsViewConfiguration gfxConfig)
        {
            targetControl.EnsureNotNull(nameof(targetControl));
            device.EnsureNotNull(nameof(device));
            gfxConfig.EnsureNotNull(nameof(gfxConfig));

            // Create the swap chain description
            SharpDX.DXGI.SwapChainDescription1 swapChainDesc = new SharpDX.DXGI.SwapChainDescription1();
            if (gfxConfig.AntialiasingEnabled && device.IsStandardAntialiasingPossible)
            {
                swapChainDesc.BufferCount = 2;
                swapChainDesc.SampleDescription = device.GetSampleDescription(gfxConfig.AntialiasingQuality);
            }
            else
            {
                swapChainDesc.BufferCount = 2;
                swapChainDesc.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);
            }

            // Set common parameters
            swapChainDesc.Width = targetControl.Width;
            swapChainDesc.Height = targetControl.Height;
            swapChainDesc.Format = GraphicsHelper.DEFAULT_TEXTURE_FORMAT;
            swapChainDesc.Scaling = SharpDX.DXGI.Scaling.Stretch;
            swapChainDesc.SwapEffect = SharpDX.DXGI.SwapEffect.Discard;
            swapChainDesc.Usage = SharpDX.DXGI.Usage.RenderTargetOutput;

            // Create and return the swap chain and the render target
            return new SharpDX.DXGI.SwapChain1(
                device.FactoryDxgi, device.DeviceD3D11_1, targetControl.Handle,
                ref swapChainDesc,
                new SharpDX.DXGI.SwapChainFullScreenDescription()
                {
                    RefreshRate = new SharpDX.DXGI.Rational(60, 1),
                    Scaling = SharpDX.DXGI.DisplayModeScaling.Centered,
                    Windowed = true
                },
                null);
        }

        /// <summary>
        /// Converts a System.Drawing.Bitmap to a DirectX 11 texture object.
        /// </summary>
        /// <param name="device">Device on wich the resource should be created.</param>
        /// <param name="bitmap">The source bitmap.</param>
        internal static D3D11.Texture2D LoadTextureFromBitmap(EngineDevice device, GDI.Bitmap bitmap)
        {
            device.EnsureNotNull(nameof(device));
            bitmap.EnsureNotNull(nameof(bitmap));

            return LoadTextureFromBitmap(device, bitmap, 1);
        }

        /// <summary>
        /// Converts a System.Drawing.Bitmap to a DirectX 11 texture object.
        /// </summary>
        /// <param name="device">Device on wich the resource should be created.</param>
        /// <param name="bitmap">The source bitmap.</param>
        /// <param name="mipLevels">Total count of levels for mipmapping.</param>
        internal static D3D11.Texture2D LoadTextureFromBitmap(EngineDevice device, GDI.Bitmap bitmap, int mipLevels)
        {
            device.EnsureNotNull(nameof(device));
            bitmap.EnsureNotNull(nameof(bitmap));
            mipLevels.EnsurePositive(nameof(mipLevels));

            D3D11.Texture2D result = null;

            // Lock bitmap so it can be accessed for texture loading
            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            try
            {
                // Open a reading stream for bitmap memory
                DataRectangle dataRectangle = new DataRectangle(bitmapData.Scan0, bitmap.Width * 4);

                // Load the texture
                result = new D3D11.Texture2D(device.DeviceD3D11_1, new D3D11.Texture2DDescription()
                {
                    BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget,
                    CpuAccessFlags = D3D11.CpuAccessFlags.None,
                    Format = GraphicsHelper.DEFAULT_TEXTURE_FORMAT,
                    OptionFlags = D3D11.ResourceOptionFlags.None | D3D11.ResourceOptionFlags.GenerateMipMaps,
                    MipLevels = 0,
                    Usage = D3D11.ResourceUsage.Default,
                    Width = bitmap.Width,
                    Height = bitmap.Height,
                    ArraySize = 1,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0)
                }, new DataRectangle[] { dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle });

                // Workaround for now... auto generate mip-levels
                using (D3D11.ShaderResourceView shaderResourceView = new D3D11.ShaderResourceView(device.DeviceD3D11_1, result))
                {
                    device.DeviceImmediateContextD3D11.GenerateMips(shaderResourceView);
                }
            }
            finally
            {
                // Free bitmap-access resources
                bitmap.UnlockBits(bitmapData);
            }

            return result;
        }

        /// <summary>
        /// Loads a bitmap from the given texture. Be careful: The texture musst have CPU read access and this only matches for staging textures.
        /// </summary>
        /// <param name="device">The device on which the texture is created.</param>
        /// <param name="stagingTexture">The texture to be loaded into the bitmap.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        internal static GDI.Bitmap LoadBitmapFromStagingTexture(EngineDevice device, D3D11.Texture2D stagingTexture, int width, int height)
        {
            device.EnsureNotNull(nameof(device));
            stagingTexture.EnsureNotNull(nameof(stagingTexture));
            width.EnsurePositive(nameof(width));
            height.EnsurePositive(nameof(height));

            //Prepare target bitmap 
            GDI.Bitmap resultBitmap = new GDI.Bitmap(width, height);
            SharpDX.DataBox dataBox = device.DeviceImmediateContextD3D11.MapSubresource(stagingTexture, 0, D3D11.MapMode.Read, D3D11.MapFlags.None);
            try
            {
                //Lock bitmap so it can be accessed for texture loading
                System.Drawing.Imaging.BitmapData bitmapData = resultBitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                try
                {
                    //Copy data row by row
                    // => Rows form datasource may have more pixels because driver changes the size of textures
                    ulong rowPitch = (ulong)(width * 4);
                    for (int loopRow = 0; loopRow < height; loopRow++)
                    {
                        // Copy bitmap data
                        int rowPitchSource = dataBox.RowPitch;
                        int rowPitchDestination = width * 4;
                        SeeingSharpTools.CopyMemory(
                            dataBox.DataPointer + loopRow * rowPitchSource,
                            bitmapData.Scan0 + loopRow * rowPitchDestination,
                            rowPitch);
                    }
                }
                finally
                {
                    resultBitmap.UnlockBits(bitmapData);
                }
            }
            finally
            {
                device.DeviceImmediateContextD3D11.UnmapSubresource(stagingTexture, 0);
            }
            return resultBitmap;
        }
    }
}