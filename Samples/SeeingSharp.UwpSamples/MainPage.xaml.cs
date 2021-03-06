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
using System.Collections.Generic;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.UwpSamples
{
    public sealed partial class MainPage : Page
    {
        private SampleBase _actSample;
        private SampleSettings _actSampleSettings;
        private SampleMetadata _actSampleInfo;
        private bool _isChangingSample;

        private List<ChildRenderPage> _childPages;
        private object _childPagesLock;

        public MainPage()
        {
            this.InitializeComponent();

            _childPages = new List<ChildRenderPage>();
            _childPagesLock = new object();

            // Manipulate titlebar
            //  see https://social.msdn.microsoft.com/Forums/windows/en-US/08462adc-a7ba-459f-9d2b-32a14c7a7de1/uwp-how-to-change-the-text-of-the-application-title-bar?forum=wpdevelop
            var package = Package.Current;
            var appName = package.DisplayName;
            TextAppTitle.Text = $@"{appName} - Main window - {Assembly.GetExecutingAssembly().GetName().Version}";
            Window.Current.SetTitleBar(TextAppTitleRow);

            this.Loaded += this.OnLoaded;
        }

        /// <summary>
        /// Applies the given sample.
        /// </summary>
        private async void ApplySample(SampleMetadata sampleInfo, SampleSettings sampleSettings)
        {
            if (DesignMode.DesignModeEnabled) { return; }
            if (_isChangingSample) { return; }

            _isChangingSample = true;
            try
            {
                if (_actSampleInfo == sampleInfo) { return; }

                // Clear previous sample
                if (_actSampleInfo != null)
                {
                    await CtrlSwapChain.RenderLoop.Scene.ManipulateSceneAsync(manipulator =>
                    {
                        manipulator.Clear(true);
                    });
                    await CtrlSwapChain.RenderLoop.Clear2DDrawingLayersAsync();
                    CtrlSwapChain.RenderLoop.ObjectFilters.Clear();

                    foreach (var actChildWindow in _childPages)
                    {
                        await actChildWindow.ClearAsync();
                    }

                    _actSample.OnSampleClosed();
                }
                if (_actSampleSettings != null)
                {
                    _actSampleSettings.RecreateRequest -= this.OnSampleSettings_RecreateRequest;
                }

                // Reset members
                _actSample = null;
                _actSampleSettings = null;
                _actSampleInfo = null;

                // Apply new sample
                if (sampleInfo != null)
                {
                    var sampleObject = sampleInfo.CreateSampleObject();
                    await sampleObject.OnStartupAsync(CtrlSwapChain.RenderLoop, sampleSettings);
                    await sampleObject.OnInitRenderingWindowAsync(CtrlSwapChain.RenderLoop);
                    await sampleObject.OnReloadAsync(CtrlSwapChain.RenderLoop, sampleSettings);
                    
                    foreach (var actChildWindow in _childPages)
                    {
                        await actChildWindow.SetRenderingDataAsync(sampleObject);
                    }

                    _actSample = sampleObject;
                    _actSampleSettings = sampleSettings;
                    _actSampleInfo = sampleInfo;

                    if (_actSampleSettings != null)
                    {
                        _actSampleSettings.RecreateRequest += this.OnSampleSettings_RecreateRequest;
                    }

                    await CtrlSwapChain.RenderLoop.Register2DDrawingLayerAsync(
                        new PerformanceMeasureDrawingLayer(130f, CtrlSwapChain.ViewInformation));
                }

                // Wait for next finished rendering
                await CtrlSwapChain.RenderLoop.WaitForNextFinishedRenderAsync();

                await CtrlSwapChain.RenderLoop.WaitForNextFinishedRenderAsync();
            }
            finally
            {
                _isChangingSample = false;
            }
        }

        private async void OnSampleSettings_RecreateRequest(object sender, EventArgs e)
        {
            var sample = _actSample;
            var sampleSettings = _actSampleSettings;
            if (sample == null)
            {
                return;
            }
            if (sampleSettings == null)
            {
                return;
            }

            await sample.OnReloadAsync(CtrlSwapChain.RenderLoop, sampleSettings);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled) { return; }

            //this.na = $@"{this.Title} ({Assembly.GetExecutingAssembly().GetName().Version})";

            var sampleRepo = new SampleRepository();
            sampleRepo.LoadSampleData();

            var viewModel = this.DataContext as MainWindowViewModel;
            viewModel?.LoadSampleData(sampleRepo, CtrlSwapChain.RenderLoop);

            // Start update loop
            CtrlSwapChain.RenderLoop.Configuration.AlphaEnabledSwapChain = true;
            CtrlSwapChain.RenderLoop.PrepareRender += (innerSender, eArgs) =>
            {
                var actSample = _actSample;
                actSample?.Update();
            };
        }

        private void OnSelectedSampleChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled) { return; }
            if (!(this.DataContext is MainWindowViewModel viewModel)) { return; }

            var selectedSample = viewModel.SelectedSample;
            if (selectedSample == null) { return; }

            this.ApplySample(selectedSample.SampleMetadata, viewModel.SampleSettings);
        }

        private async void OnCmdNewChildWindow_Click(object sender, RoutedEventArgs e)
        {
            // Get current scene and camera viewpoint
            var currentScene = CtrlSwapChain.Scene;
            var currentViewPoint = CtrlSwapChain.Camera.GetViewPoint();

            // Create the child window and the page
            // Be careful: the child view has it's own UI thread. See https://docs.microsoft.com/en-us/windows/uwp/design/layout/application-view
            var newView = CoreApplication.CreateNewView();
            var newViewId = 0;
            ChildRenderPage childPage = null;
            var hostDispatcher = this.Dispatcher;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Hide default title bar.
                var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.ExtendViewIntoTitleBar = true;

                childPage = new ChildRenderPage();
                childPage.InitializeChildWindow(currentScene, currentViewPoint);

                Window.Current.Content = childPage;
                Window.Current.Activate();
                Window.Current.VisibilityChanged += async (o, args) =>
                {
                    // 'Visible == false' means that the user closed the window
                    if (args.Visible == false)
                    {
                        // Set content to null --> This ensures that SeeingSharp gets the unloaded event from the view internally
                        Window.Current.Content = null;

                        // Notify the host that the ChildRenderPage was removed
                        await hostDispatcher.RunAsync(
                            CoreDispatcherPriority.Normal,
                            () => _childPages.Remove(childPage));

                        // Wait some loop passes to ensure that all references to this additional view are cleared
                        await GraphicsCore.Current.MainLoop.WaitForNextPassedLoopAsync();
                        await GraphicsCore.Current.MainLoop.WaitForNextPassedLoopAsync();
                        await GraphicsCore.Current.MainLoop.WaitForNextPassedLoopAsync();
                        
                        // Finally close the window
                        Window.Current.Close();
                    }
                };

                newViewId = ApplicationView.GetForCurrentView().Id;
            });

            // Show the child window
            var viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
            if (viewShown)
            {
                // Register the child page
                _childPages.Add(childPage);

                // We can call this one in the current thread
                // Should be thread save
                await childPage.SetRenderingDataAsync(_actSample);
            }
        }
    }
}