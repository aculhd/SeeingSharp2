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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    /// <summary>
    /// A Direct2D texture which performs it's rendering only
    /// on load time. So Draw2D is called only once per device!
    /// Use this class for static textures, backgrounds, etc.
    /// </summary>
    public class Direct2DOneTimeRenderTextureResource : TextureResource
    {
        // Configuration
        private Custom2DDrawingLayer _drawingLayer;
        private BrushResource _fillBrush;
        private int _width;
        private int _height;

        // Resources for Direct3D
        private D3D11.Texture2D _renderTargetTexture;
        private D3D11.ShaderResourceView _renderTargetTextureView;

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => _renderTargetTexture != null;

        /// <summary>
        /// Gets the texture object.
        /// </summary>
        internal override D3D11.Texture2D Texture => _renderTargetTexture;

        /// <summary>
        /// Gets a ShaderResourceView targeting the texture.
        /// </summary>
        internal override D3D11.ShaderResourceView TextureView => _renderTargetTextureView;

        /// <summary>
        /// Gets the size of the texture array.
        /// 1 for normal textures.
        /// 6 for cubemap textures.
        /// </summary>
        public override int ArraySize => 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Direct2DOneTimeRenderTextureResource"/> class.
        /// </summary>
        /// <param name="drawingLayer">The drawing layer.</param>
        /// <param name="height">The width of the generated texture.</param>
        /// <param name="width">The height of the generated texture.</param>
        public Direct2DOneTimeRenderTextureResource(Custom2DDrawingLayer drawingLayer, int width, int height)
        {
            drawingLayer.EnsureNotNull(nameof(drawingLayer));
            width.EnsurePositiveOrZero(nameof(width));
            height.EnsurePositiveOrZero(nameof(height));

            _drawingLayer = drawingLayer;
            _width = width;
            _height = height;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Direct2DOneTimeRenderTextureResource"/> class.
        /// </summary>
        /// <param name="fillBrush">The brush which gets used when painting the texture on load time.</param>
        /// <param name="height">The width of the generated texture.</param>
        /// <param name="width">The height of the generated texture.</param>
        public Direct2DOneTimeRenderTextureResource(BrushResource fillBrush, int width, int height)
        {
            fillBrush.EnsureNotNull(nameof(fillBrush));
            width.EnsurePositiveOrZero(nameof(width));
            height.EnsurePositiveOrZero(nameof(height));

            _fillBrush = fillBrush;
            _width = width;
            _height = height;
        }

        /// <summary>
        /// Loads all resource.
        /// </summary>
        /// <param name="device">The device on which to load all resources.</param>
        /// <param name="resources">The current ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _renderTargetTexture = GraphicsHelper.Internals.CreateRenderTargetTexture(
                device, _width, _height, new GraphicsViewConfiguration { AntialiasingEnabled = false });
            _renderTargetTextureView = new D3D11.ShaderResourceView(device.DeviceD3D11_1, _renderTargetTexture);

            // Create resources for rendering on the texture
            using (var overlayRenderer = new Direct2DOverlayRenderer(
                device,
                _renderTargetTexture,
                _width, _height,
                DpiScaling.Default))
            {
                var graphics2D = new Graphics2D(device, overlayRenderer.RenderTarget2D, new Size2F(_width, _height));

                // Start drawing
                overlayRenderer.BeginDraw();
                try
                {
                    if (_drawingLayer != null)
                    {
                        _drawingLayer.Draw2DInternal(graphics2D);
                    }
                    else if (_fillBrush != null)
                    {
                        graphics2D.FillRectangle(
                            new RectangleF(0f, 0f, _width, _height),
                            _fillBrush);
                    }
                }
                finally
                {
                    overlayRenderer.EndDraw();
                }
            }
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        /// <param name="device">The device on which the resources where loaded.</param>
        /// <param name="resources">The current ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            SeeingSharpUtil.SafeDispose(ref _renderTargetTextureView);
            SeeingSharpUtil.SafeDispose(ref _renderTargetTexture);
        }
    }
}
