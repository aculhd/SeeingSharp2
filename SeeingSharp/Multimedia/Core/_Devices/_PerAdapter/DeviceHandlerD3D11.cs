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
using SeeingSharp.Util;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using D3D = SharpDX.Direct3D;

namespace SeeingSharp.Multimedia.Core
{
    // Overview Feature levels:
    //http://msdn.microsoft.com/en-us/library/windows/desktop/ff476876(v=vs.85).aspx

    // Information on WARP
    //http://msdn.microsoft.com/en-us/library/windows/desktop/gg615082(v=vs.85).aspx#capabilities

    /// <summary>
    /// All initialization logic for the D3D11 device
    /// </summary>
    public class DeviceHandlerD3D11
    {
        // Resources from Direct3D11 api
        private Adapter1 m_dxgiAdapter;
        private D3D11.Device1 m_device1;
        private D3D11.Device3 m_device3;
        private D3D11.DeviceContext m_immediateContext;
        private D3D11.DeviceContext3 m_immediateContext3;

        // Parameters of created device
        private D3D11.DeviceCreationFlags m_creationFlags;
        private D3D.FeatureLevel m_featureLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceHandlerD3D11"/> class.
        /// </summary>
        internal DeviceHandlerD3D11(DeviceLoadSettings deviceLoadSettings, Adapter1 dxgiAdapter)
        {
            m_dxgiAdapter = dxgiAdapter;

            // Define possible create flags
            var createFlagsBgra = D3D11.DeviceCreationFlags.BgraSupport;
            var createFlagsNormal = D3D11.DeviceCreationFlags.None;
            if (deviceLoadSettings.DebugEnabled)
            {
                createFlagsBgra |= D3D11.DeviceCreationFlags.Debug;
                createFlagsNormal |= D3D11.DeviceCreationFlags.Debug;
            }

            // Define all steps on which we try to initialize Direct3D
            var initParameterQueue =
                new List<Tuple<D3D.FeatureLevel, D3D11.DeviceCreationFlags, HardwareDriverLevel>>();

            // Define all tries for hardware initialization
            initParameterQueue.Add(Tuple.Create(
                D3D.FeatureLevel.Level_11_1, createFlagsBgra, HardwareDriverLevel.Direct3D11));
            initParameterQueue.Add(Tuple.Create(
                D3D.FeatureLevel.Level_11_0, createFlagsBgra, HardwareDriverLevel.Direct3D11));
            initParameterQueue.Add(Tuple.Create(
                D3D.FeatureLevel.Level_10_0, createFlagsBgra, HardwareDriverLevel.Direct3D10));
            initParameterQueue.Add(Tuple.Create(
                D3D.FeatureLevel.Level_9_3, createFlagsBgra, HardwareDriverLevel.Direct3D9_3));
            initParameterQueue.Add(Tuple.Create(
                D3D.FeatureLevel.Level_9_2, createFlagsBgra, HardwareDriverLevel.Direct3D9_2));
            initParameterQueue.Add(Tuple.Create(
                D3D.FeatureLevel.Level_9_1, createFlagsBgra, HardwareDriverLevel.Direct3D9_1));
            initParameterQueue.Add(Tuple.Create(
                D3D.FeatureLevel.Level_10_0, createFlagsNormal, HardwareDriverLevel.Direct3D10));
            initParameterQueue.Add(Tuple.Create(
                 D3D.FeatureLevel.Level_9_3, createFlagsNormal, HardwareDriverLevel.Direct3D9_3));
            initParameterQueue.Add(Tuple.Create(
                 D3D.FeatureLevel.Level_9_2, createFlagsNormal, HardwareDriverLevel.Direct3D9_2));
            initParameterQueue.Add(Tuple.Create(
                 D3D.FeatureLevel.Level_9_1, createFlagsNormal, HardwareDriverLevel.Direct3D9_1));

            // Try to create the device, each defined configuration step by step
            foreach (var (actFeatureLevel, actCreateFlags, actDriverLevel) in initParameterQueue)
            {
                try
                {
                    // Try to create the device using current parameters
                    using (var device = new D3D11.Device(dxgiAdapter, actCreateFlags, actFeatureLevel))
                    {
                        m_device1 = device.QueryInterface<D3D11.Device1>();
                        m_device3 = SeeingSharpUtil.TryExecute(() => m_device1.QueryInterface<D3D11.Device3>());

                        if(m_device3 != null)
                        {
                            m_immediateContext3 = m_device3.ImmediateContext3;
                        }
                    }

                    // Device successfully created, save all parameters and break this loop
                    m_featureLevel = actFeatureLevel;
                    m_creationFlags = actCreateFlags;
                    this.DriverLevel = actDriverLevel;
                    break;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            // Throw exception on failure
            if (m_device1 == null)
            {
                throw new SeeingSharpGraphicsException("Unable to initialize d3d11 device!");
            }

            // Get immediate context from the device
            m_immediateContext = m_device1.ImmediateContext;
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        public void UnloadResources()
        {
            m_immediateContext = SeeingSharpUtil.DisposeObject(m_immediateContext);
            m_immediateContext3 = SeeingSharpUtil.DisposeObject(m_immediateContext3);
            m_device1 = SeeingSharpUtil.DisposeObject(m_device1);
            m_device3 = SeeingSharpUtil.DisposeObject(m_device3);

            m_creationFlags = D3D11.DeviceCreationFlags.None;
            m_featureLevel = D3D.FeatureLevel.Level_11_0;
        }

        /// <summary>
        /// Is the hardware Direct3D 10 or upper?
        /// </summary>
        public bool IsDirect3D10OrUpperHardware =>
            m_featureLevel == D3D.FeatureLevel.Level_10_0 ||
            m_featureLevel == D3D.FeatureLevel.Level_10_1 ||
            m_featureLevel == D3D.FeatureLevel.Level_11_0 ||
            m_featureLevel == D3D.FeatureLevel.Level_11_1;

        /// <summary>
        /// Is the hardware Direct3D 11 or upper?
        /// </summary>
        public bool IsDirect3D11OrUpperHardware =>
            m_featureLevel == D3D.FeatureLevel.Level_11_0 ||
            m_featureLevel == D3D.FeatureLevel.Level_11_1;

        /// <summary>
        /// Gets the native pointer to the device object.
        /// </summary>
        public IntPtr DeviceNativePointer => m_device1.NativePointer;

        /// <summary>
        /// Is device successfully initialized?
        /// </summary>
        public bool IsInitialized => m_device1 != null;

        /// <summary>
        /// Gets a short description containing info about the created device.
        /// </summary>
        public string DeviceModeDescription
        {
            get
            {
                if (m_device1 == null) { return "None"; }

                return m_dxgiAdapter + " - " + m_featureLevel + (this.IsDirect2DTextureEnabled ? " - Bgra" : " - No Bgra");
            }
        }

        /// <summary>
        /// Gets the driver level.
        /// </summary>
        public HardwareDriverLevel DriverLevel { get; }

        /// <summary>
        /// Are Direct2D textures possible?
        /// </summary>
        public bool IsDirect2DTextureEnabled => (m_creationFlags & D3D11.DeviceCreationFlags.BgraSupport) == D3D11.DeviceCreationFlags.BgraSupport;

        /// <summary>
        /// Gets current feature level.
        /// </summary>
        internal D3D.FeatureLevel FeatureLevel => m_featureLevel;

        /// <summary>
        /// Gets the Direct3D 11 device.
        /// </summary>
        internal D3D11.Device1 Device1 => m_device1;

        internal D3D11.Device3 Device3 => m_device3;

        /// <summary>
        /// Gets the immediate context.
        /// </summary>
        internal D3D11.DeviceContext ImmediateContext => m_immediateContext;

        /// <summary>
        /// Gets the immediate context.
        /// </summary>
        internal D3D11.DeviceContext3 ImmediateContext3 => m_immediateContext3;
    }
}