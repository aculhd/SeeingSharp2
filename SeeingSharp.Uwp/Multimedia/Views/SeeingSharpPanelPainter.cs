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

using System;
using System.Threading;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Input;
using SeeingSharp.Util;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Views
{
    // Using SwapChainBackgroundPanel to render to the background of the WinRT app
    //  see http://msdn.microsoft.com/en-us/library/windows/apps/hh825871.aspx
    public class SeeingSharpPanelPainter : ISeeingSharpPainter, IDisposable, IInputEnabledView, IRenderLoopHost
    {
        private const double MIN_PIXEL_SIZE_HEIGHT = 100.0;
        private const double MIN_PIXEL_SIZE_WIDTH = 100.0;

        // SwapChainBackgroundPanel local members
        private SwapChainPanelWrapper _targetPanel;
        private Size _lastRefreshTargetSize;
        private bool _compositionScaleChanged;
        private DateTime _lastSizeChange;

        // Resources from Direct3D 11
        private SwapChain1 _swapChain;
        private D3D11.Texture2D _backBuffer;
        private D3D11.Texture2D _backBufferMultisampled;
        private D3D11.Texture2D _depthBuffer;
        private D3D11.RenderTargetView _renderTargetView;
        private D3D11.DepthStencilView _renderTargetDepth;

        /// <summary>
        /// Gets the current 3D scene.
        /// </summary>
        public Scene Scene
        {
            get => this.RenderLoop.Scene;
            set => this.RenderLoop.SetScene(value);
        }

        /// <summary>
        /// Gets or sets the current 3D camera.
        /// </summary>
        public Camera3DBase Camera
        {
            get => this.RenderLoop.Camera;
            set => this.RenderLoop.Camera = value;
        }

        /// <summary>
        /// Gets current renderloop object.
        /// </summary>
        public RenderLoop RenderLoop { get; }

        public GraphicsViewConfiguration Configuration => this.RenderLoop.Configuration;

        /// <summary>
        /// Discard rendering?
        /// </summary>
        public bool DiscardRendering
        {
            get => this.RenderLoop.DiscardRendering;
            set => this.RenderLoop.DiscardRendering = value;
        }

        /// <summary>
        /// Should we detach automatically if the TargetPanel gets unloaded?
        /// </summary>
        public bool DetachOnUnload { get; set; }

        /// <summary>
        /// Is this painter attached to any panel?
        /// </summary>
        public bool IsAttachedToGui => _targetPanel != null;

        /// <summary>
        /// Gets the current pixel size of the target panel.
        /// </summary>
        public Size2 PixelSize => this.GetTargetRenderPixelSize();

        public Size2 ActualSize => new Size2((int)_targetPanel.ActualWidth, (int)_targetPanel.ActualHeight);

        /// <summary>
        /// Gets or sets the clear color for the 3D view.
        /// </summary>
        public Color ClearColor
        {
            get
            {
                var clearColor = this.RenderLoop.ClearColor;
                return SeeingSharpUwpUtil.UIColorFromColor4(ref clearColor);
            }
            set => this.RenderLoop.ClearColor = SeeingSharpUwpUtil.Color4FromUIColor(ref value);
        }

        public Panel TargetPanel => _targetPanel?.Panel;

        /// <summary>
        /// True if the control is connected with the main rendering loop.
        /// False if something went wrong.
        /// </summary>
        public bool IsOperational => this.RenderLoop.IsOperational;

        public CoreDispatcher Dispatcher => _targetPanel?.Dispatcher;

        /// <summary>
        /// Does the target control have focus?
        /// (Return true here if rendering runs, because in WinRT we are every time at fullscreen)
        /// </summary>
        bool IInputEnabledView.Focused => this.RenderLoop.IsRegisteredOnMainLoop;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpPanelPainter" /> class.
        /// </summary>
        public SeeingSharpPanelPainter()
        {
            _lastSizeChange = DateTime.MinValue;

            // Create the RenderLoop object
            this.RenderLoop = new RenderLoop(SynchronizationContext.Current, this)
            {
                ClearColor = Color4.Transparent
            };

            this.RenderLoop.Internals.CallPresentInUIThread = true;
            this.DetachOnUnload = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpPanelPainter"/> class.
        /// </summary>
        /// <param name="targetPanel">The target panel.</param>
        public SeeingSharpPanelPainter(SwapChainBackgroundPanel targetPanel)
            : this()
        {
            this.Attach(targetPanel);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpPanelPainter"/> class.
        /// </summary>
        /// <param name="targetPanel">The target panel.</param>
        public SeeingSharpPanelPainter(SwapChainPanel targetPanel)
            : this()
        {
            this.Attach(targetPanel);
        }

        /// <summary>
        /// Attaches the renderer to the given SwapChainBackgroundPanel.
        /// </summary>
        /// <param name="targetPanel">The target panel to attach to.</param>
        public void Attach(SwapChainPanel targetPanel)
        {
            this.AttachInternal(new SwapChainPanelWrapper(targetPanel));
        }

        /// <summary>
        /// Attaches the renderer to the given SwapChainBackgroundPanel.
        /// </summary>
        /// <param name="targetPanel">The target panel to attach to.</param>
        public void Attach(SwapChainBackgroundPanel targetPanel)
        {
            this.AttachInternal(new SwapChainPanelWrapper(targetPanel));
        }

        /// <summary>
        /// Detaches the renderer from the current target panel.
        /// </summary>
        public void Detach()
        {
            try
            {
                if (_targetPanel == null) { return; }

                // Clear view resources
                this.RenderLoop.Internals.UnloadViewResources();
                this.RenderLoop.DeregisterRenderLoop();

                // Clear event registrations
                _targetPanel.SizeChanged -= this.OnTargetPanel_SizeChanged;
                _targetPanel.Loaded -= this.OnTargetPanel_Loaded;
                _targetPanel.Unloaded -= this.OnTargetPanel_Unloaded;
                _targetPanel.CompositionScaleChanged -= this.OnTargetPanel_CompositionScaleChanged;

                // Clear created references
                if (_targetPanel != null)
                {
                    _targetPanel.SwapChain = null;
                    SeeingSharpUtil.SafeDispose(ref _targetPanel);
                }
            }
            catch (Exception ex)
            {
                throw new SeeingSharpException($"Error while detaching from view: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Detach();
        }

        /// <summary>
        /// Called when the size of the target panel has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Windows.UI.Xaml.SizeChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnTargetPanelThrottled_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (!GraphicsCore.IsLoaded)
                {
                    return;
                }

                // Ignore event, if nothing has changed..
                var actSize = this.GetTargetRenderPixelSize();

                if ((int)_lastRefreshTargetSize.Width == actSize.Width &&
                    (int)_lastRefreshTargetSize.Height == actSize.Height)
                {
                    return;
                }

                this.UpdateRenderLoopViewSize();
            }
            catch (Exception ex)
            {
                throw new SeeingSharpException($"Error during resize of the view: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Attaches the renderer to the given SwapChainBackgroundPanel.
        /// </summary>
        /// <param name="targetPanel">The target panel to attach to.</param>
        private void AttachInternal(SwapChainPanelWrapper targetPanel)
        {
            if (_targetPanel != null) { throw new InvalidOperationException("Unable to attach to new SwapChainBackgroundPanel: Renderer is already attached to another one!"); }

            _lastRefreshTargetSize = new Size(0.0, 0.0);
            _targetPanel = targetPanel;

            _targetPanel.SizeChanged += this.OnTargetPanel_SizeChanged;
            _targetPanel.Loaded += this.OnTargetPanel_Loaded;
            _targetPanel.Unloaded += this.OnTargetPanel_Unloaded;
            _targetPanel.CompositionScaleChanged += this.OnTargetPanel_CompositionScaleChanged;

            this.UpdateRenderLoopViewSize();

            // Define unloading behavior
            if (VisualTreeHelper.GetParent(_targetPanel.Panel) != null)
            {
                this.RenderLoop.RegisterRenderLoop();
            }
        }

        /// <summary>
        /// Gets the current target pixel size for the render panel.
        /// </summary>
        private Size2 GetTargetRenderPixelSize()
        {
            if (_targetPanel == null) { return new Size2((int)MIN_PIXEL_SIZE_WIDTH, (int)MIN_PIXEL_SIZE_HEIGHT); }

            var currentWidth = _targetPanel.ActualWidth * _targetPanel.CompositionScaleX;
            var currentHeight = _targetPanel.ActualHeight * _targetPanel.CompositionScaleY;

            return new Size2(
                (int)(currentWidth > MIN_PIXEL_SIZE_WIDTH ? currentWidth : MIN_PIXEL_SIZE_WIDTH),
                (int)(currentHeight > MIN_PIXEL_SIZE_HEIGHT ? currentHeight : MIN_PIXEL_SIZE_HEIGHT));
        }

        /// <summary>
        /// Update current configured view size on the render loop.
        /// </summary>
        private void UpdateRenderLoopViewSize()
        {
            var viewSize = this.GetTargetRenderPixelSize();
            this.RenderLoop.Camera.SetScreenSize(viewSize.Width, viewSize.Height);
            this.RenderLoop.SetCurrentViewSize(
                viewSize.Width,
                viewSize.Height);
        }

        /// <summary>
        /// Called when the target panel gets unloaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnTargetPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            this.RenderLoop.DeregisterRenderLoop();

            // Trigger detach if requested
            if (this.DetachOnUnload)
            {
                this.Detach();
            }
        }

        /// <summary>
        /// Called when the target panel gets loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnTargetPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.RenderLoop.IsRegisteredOnMainLoop)
            {
                this.RenderLoop.RegisterRenderLoop();
            }
        }

        /// <summary>
        /// Called when the size of the target panel has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void OnTargetPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!GraphicsCore.IsLoaded)
            {
                return;
            }

            //Resize render target only on greater size changes
            var viewSize = this.GetTargetRenderPixelSize();
            var resizeFactorWidth = (double)viewSize.Width > _lastRefreshTargetSize.Width ? viewSize.Width / _lastRefreshTargetSize.Width : _lastRefreshTargetSize.Width / viewSize.Width;
            var resizeFactorHeight = (double)viewSize.Height > _lastRefreshTargetSize.Height ? viewSize.Height / _lastRefreshTargetSize.Height : _lastRefreshTargetSize.Height / viewSize.Height;

            if (resizeFactorWidth > 1.3 || resizeFactorHeight > 1.3)
            {
                this.UpdateRenderLoopViewSize();
            }

            _lastSizeChange = DateTime.UtcNow;
        }

        /// <summary>
        /// Some configuration like
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTargetPanel_CompositionScaleChanged(object sender, EventArgs e)
        {
            this.UpdateRenderLoopViewSize();

            _compositionScaleChanged = true;
        }

        /// <summary>
        /// Disposes all loaded view resources.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_DisposeViewResources(EngineDevice engineDevice)
        {
            if (_targetPanel != null)
            {
                _targetPanel.SwapChain = null;
            }

            _renderTargetDepth = SeeingSharpUtil.DisposeObject(_renderTargetDepth);
            _depthBuffer = SeeingSharpUtil.DisposeObject(_depthBuffer);
            _renderTargetView = SeeingSharpUtil.DisposeObject(_renderTargetView);
            _backBuffer = SeeingSharpUtil.DisposeObject(_backBuffer);
            _backBufferMultisampled = SeeingSharpUtil.DisposeObject(_backBufferMultisampled);
            _swapChain = SeeingSharpUtil.DisposeObject(_swapChain);
        }

        /// <summary>
        /// Create all view resources.
        /// </summary>
        Tuple<D3D11.Texture2D, D3D11.RenderTargetView, D3D11.Texture2D, D3D11.DepthStencilView, RawViewportF, Size2, DpiScaling> IRenderLoopHost.OnRenderLoop_CreateViewResources(EngineDevice engineDevice)
        {
            _backBufferMultisampled = null;

            var viewSize = this.GetTargetRenderPixelSize();

            // Create the SwapChain and associate it with the SwapChainBackgroundPanel
            _swapChain = GraphicsHelperUwp.CreateSwapChainForComposition(engineDevice, viewSize.Width, viewSize.Height, this.RenderLoop.Configuration);
            _targetPanel.SwapChain = _swapChain;
            _compositionScaleChanged = true;

            // Get the backbuffer from the SwapChain
            _backBuffer = D3D11.Resource.FromSwapChain<D3D11.Texture2D>(_swapChain, 0);

            // Define the render target (in case of multisample an own render target)
            D3D11.Texture2D backBufferForRenderloop = null;
            if (this.RenderLoop.Configuration.AntialiasingEnabled)
            {
                _backBufferMultisampled = GraphicsHelper.Internals.CreateRenderTargetTexture(engineDevice, viewSize.Width, viewSize.Height, this.RenderLoop.Configuration);
                _renderTargetView = new D3D11.RenderTargetView(engineDevice.Internals.DeviceD3D11_1, _backBufferMultisampled);
                backBufferForRenderloop = _backBufferMultisampled;
            }
            else
            {
                _renderTargetView = new D3D11.RenderTargetView(engineDevice.Internals.DeviceD3D11_1, _backBuffer);
                backBufferForRenderloop = _backBuffer;
            }

            //Create the depth buffer
            _depthBuffer = GraphicsHelper.Internals.CreateDepthBufferTexture(engineDevice, viewSize.Width, viewSize.Height, this.RenderLoop.Configuration);
            _renderTargetDepth = new D3D11.DepthStencilView(engineDevice.Internals.DeviceD3D11_1, _depthBuffer);

            //Define the viewport for rendering
            var viewPort = GraphicsHelper.Internals.CreateDefaultViewport(viewSize.Width, viewSize.Height);
            _lastRefreshTargetSize = new Size(viewSize.Width, viewSize.Height);

            var dpiScaling = new DpiScaling
            {
                DpiX = (float)(96.0 * _targetPanel.CompositionScaleX),
                DpiY = (float)(96.0 * _targetPanel.CompositionScaleY)
            };

            return Tuple.Create(backBufferForRenderloop, _renderTargetView, _depthBuffer, _renderTargetDepth, viewPort, viewSize, dpiScaling);
        }

        /// <summary>
        /// Called when RenderLoop object checks whether it is possible to render.
        /// </summary>
        bool IRenderLoopHost.OnRenderLoop_CheckCanRender(EngineDevice engineDevice)
        {
            if (_targetPanel == null) { return false; }
            if (_targetPanel.ActualWidth <= 0) { return false; }
            if (_targetPanel.ActualHeight <= 0) { return false; }
            if (_targetPanel.Visibility != Visibility.Visible) { return false; }

            return true;
        }

        void IRenderLoopHost.OnRenderLoop_PrepareRendering(EngineDevice engineDevice)
        {
            if (_targetPanel == null) { return; }
            if (this.RenderLoop == null) { return; }

            // Update swap chain scaling (only relevant for SwapChainPanel targets)
            //  see https://www.packtpub.com/books/content/integrating-direct3d-xaml-and-windows-81
            if (this.RenderLoop.Camera != null &&
                _swapChain != null)
            {
                if (_compositionScaleChanged &&
                    _targetPanel.CompositionRescalingNeeded)
                {
                    _compositionScaleChanged = false;
                    var swapChain2 = _swapChain.QueryInterfaceOrNull<SwapChain2>();

                    if (swapChain2 != null)
                    {
                        try
                        {
                            var inverseScale = new RawMatrix3x2
                            {
                                M11 = 1.0f / (float)_targetPanel.CompositionScaleX,
                                M22 = 1.0f / (float)_targetPanel.CompositionScaleY
                            };

                            swapChain2.MatrixTransform = inverseScale;
                        }
                        finally
                        {
                            swapChain2.Dispose();
                        }
                    }
                }
            }

            // Handle throttled resizing of view resources
            if (_lastSizeChange != DateTime.MinValue &&
                DateTime.UtcNow - _lastSizeChange > SeeingSharpConstantsUwp.THROTTLED_VIEW_RECREATION_TIME_ON_RESIZE)
            {
                _lastSizeChange = DateTime.MinValue;
                this.UpdateRenderLoopViewSize();
            }
        }

        /// <summary>
        /// Called when RenderLoop wants to present its results.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_Present(EngineDevice engineDevice)
        {
            // Copy contents of the backbuffer if in multisampling mode
            if (_backBufferMultisampled != null)
            {
                engineDevice.Internals.DeviceImmediateContextD3D11.ResolveSubresource(_backBufferMultisampled, 0, _backBuffer, 0, GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT);
            }

            // Present all rendered stuff on screen
            // First parameter indicates synchronization with vertical blank
            //  see http://msdn.microsoft.com/en-us/library/windows/desktop/bb174576(v=vs.85).aspx
            //  see example http://msdn.microsoft.com/en-us/library/windows/apps/hh825871.aspx
            _swapChain.Present(1, PresentFlags.None);
        }

        /// <summary>
        /// Called when RenderLoop has finished rendering.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_AfterRendering(EngineDevice engineDevice)
        {

        }
    }
}