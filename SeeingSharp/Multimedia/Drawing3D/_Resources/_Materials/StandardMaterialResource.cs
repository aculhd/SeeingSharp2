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

using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class StandardMaterialResource : MaterialResource
    {
        // Resource keys
        private static readonly NamedOrGenericKey s_resKeyVertexShader = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_resKeyPixelShader = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_resKeyPixelShaderOrtho = GraphicsCore.GetNextGenericResourceKey();
        private readonly NamedOrGenericKey _keyConstantBuffer = GraphicsCore.GetNextGenericResourceKey();

        // Some configuration
        private Color4 _materialDiffuseColor;
        private float _clipFactor;
        private float _maxClipDistance;
        private float _addToAlpha;
        private bool _adjustTextureCoordinates;
        private bool _cbPerMaterialDataChanged;
        private bool _useVertexColors;
        private float _borderPart;
        private float _borderMultiplier;

        // Resource members
        private TextureResource _textureResource;
        private VertexShaderResource _vertexShader;
        private PixelShaderResource _pixelShader;
        private PixelShaderResource _pixelShaderOrtho;
        private TypeSafeConstantBufferResource<CBPerMaterial> _cbPerMaterial;
        private DefaultResources _defaultResources;

        /// <summary>
        /// Gets the key of the texture resource.
        /// </summary>
        public NamedOrGenericKey TextureKey { get; }

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => _vertexShader != null;

        /// <summary>
        /// Gets or sets the ClipFactor.
        /// Pixel are clipped up to an alpha value defined by this Clipfactor within the pixel shader.
        /// </summary>
        public float ClipFactor
        {
            get => _clipFactor;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_clipFactor, value))
                {
                    _clipFactor = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum distance on which to apply pixel clipping (defined by ClipFactor property).
        /// </summary>
        public float MaxClipDistance
        {
            get => _maxClipDistance;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_maxClipDistance, value))
                {
                    _maxClipDistance = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        /// <summary>
        /// Interpolate texture coordinate based on xy-scaling.
        /// </summary>
        public bool AdjustTextureCoordinates
        {
            get => _adjustTextureCoordinates;
            set
            {
                if (_adjustTextureCoordinates != value)
                {
                    _adjustTextureCoordinates = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        /// <summary>
        /// Needed for video rendering (Frames from the MF SourceReader have alpha always to zero).
        /// </summary>
        public float AddToAlpha
        {
            get => _addToAlpha;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_addToAlpha, value))
                {
                    _addToAlpha = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        public Color4 MaterialDiffuseColor
        {
            get => _materialDiffuseColor;
            set
            {
                if (_materialDiffuseColor != value)
                {
                    _materialDiffuseColor = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        public bool UseVertexColors
        {
            get => _useVertexColors;
            set
            {
                if (_useVertexColors != value)
                {
                    _useVertexColors = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        public float BorderPart
        {
            get => _borderPart;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_borderPart, value))
                {
                    _borderPart = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        public float BorderMultiplier
        {
            get => _borderMultiplier;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_borderMultiplier, value))
                {
                    _borderMultiplier = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardMaterialResource"/> class.
        /// </summary>
        /// <param name="textureKey">The name of the texture to be rendered.</param>
        /// <param name="enableShaderGeneratedBorder">Enable drawing of borders which are generated by the pixel shader?</param>
        public StandardMaterialResource(NamedOrGenericKey textureKey = new NamedOrGenericKey(), bool enableShaderGeneratedBorder = false)
        {
            this.TextureKey = textureKey;
            _maxClipDistance = 1000f;
            _adjustTextureCoordinates = false;
            _addToAlpha = 0f;
            _materialDiffuseColor = Color4.White;
            _useVertexColors = true;

            if(enableShaderGeneratedBorder){ this.EnableShaderGeneratedBorder(); }
            else{ this.DisableShaderGeneratedBorder(); }
        }

        /// <summary>
        /// Enables a shader generated border.
        /// </summary>
        public void EnableShaderGeneratedBorder(float borderThickness = 1f)
        {
            this.BorderMultiplier = 50f;
            this.BorderPart = 0.01f * borderThickness;
        }

        /// <summary>
        /// Disables shader generated border.
        /// </summary>
        public void DisableShaderGeneratedBorder()
        {
            this.BorderMultiplier = 0f;
            this.BorderPart = 0f;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            // Load all required shaders and constant buffers
            _vertexShader = resources.GetResourceAndEnsureLoaded(
                s_resKeyVertexShader,
                () => GraphicsHelper.Internals.GetVertexShaderResource(device, "Common", "CommonVertexShader"));
            _pixelShader = resources.GetResourceAndEnsureLoaded(
                s_resKeyPixelShader,
                () => GraphicsHelper.Internals.GetPixelShaderResource(device, "Common", "CommonPixelShader"));
            _pixelShaderOrtho = resources.GetResourceAndEnsureLoaded(
                s_resKeyPixelShaderOrtho,
                () => GraphicsHelper.Internals.GetPixelShaderResource(device, "Common", "CommonPixelShader.Ortho"));
            _cbPerMaterial = resources.GetResourceAndEnsureLoaded(
                _keyConstantBuffer,
                () => new TypeSafeConstantBufferResource<CBPerMaterial>());
            _cbPerMaterialDataChanged = true;

            // Get a reference to default resource object
            _defaultResources = resources.GetResourceAndEnsureLoaded<DefaultResources>(DefaultResources.RESOURCE_KEY);

            //Load the texture if any configured.
            if (!this.TextureKey.IsEmpty)
            {
                // Get texture resource
                _textureResource = resources.GetResourceAndEnsureLoaded<TextureResource>(this.TextureKey);
            }
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _vertexShader = null;
            _pixelShader = null;
            _pixelShaderOrtho = null;
            _textureResource = null;
            _cbPerMaterial = null;
        }

        /// <summary>
        /// Generates the requested input layout.
        /// </summary>
        /// <param name="device">The device on which to create the input layout.</param>
        /// <param name="inputElements">An array of InputElements describing vertex input structure.</param>
        internal override D3D11.InputLayout GetInputLayout(EngineDevice device, D3D11.InputElement[] inputElements)
        {
            return _vertexShader.GetInputLayout(device, inputElements);
        }

        /// <summary>
        /// Applies the material to the given render state.
        /// </summary>
        /// <param name="renderState">Current render state</param>
        /// <param name="previousMaterial">The previously applied material.</param>
        internal override void Apply(RenderState renderState, MaterialResource previousMaterial)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;
            var isResourceSameType =
                previousMaterial != null &&
                previousMaterial.ResourceType == this.ResourceType;

            // Apply local shader configuration
            if (_cbPerMaterialDataChanged)
            {
                _cbPerMaterial.SetData(
                    deviceContext,
                    new CBPerMaterial
                    {
                        ClipFactor = _clipFactor,
                        MaxClipDistance = _maxClipDistance,
                        Texture0Factor = _textureResource != null ? 1f : 0f,
                        AdjustTextureCoordinates = _adjustTextureCoordinates ? 1f : 0f,
                        AddToAlpha = _addToAlpha,
                        MaterialDiffuseColor = _materialDiffuseColor,
                        DiffuseColorFactor = _useVertexColors ? 0f : 1f,
                        BorderPart = _borderPart,
                        BorderMultiplier = _borderMultiplier
                    });
                _cbPerMaterialDataChanged = false;
            }

            // Set shaders, sampler and constants
            if (!isResourceSameType)
            {
                deviceContext.PixelShader.SetSampler(0, _defaultResources.GetSamplerState(TextureSamplerQualityLevel.Low));

                deviceContext.VertexShader.Set(_vertexShader.VertexShader);
                if(renderState.Camera.IsOrthopraphicInternal){ deviceContext.PixelShader.Set(_pixelShaderOrtho.PixelShader); }
                else{ deviceContext.PixelShader.Set(_pixelShader.PixelShader); }
            }
            deviceContext.PixelShader.SetConstantBuffer(3, _cbPerMaterial.ConstantBuffer);
            deviceContext.VertexShader.SetConstantBuffer(3, _cbPerMaterial.ConstantBuffer);

            // Set texture resource (if set)
            if (_textureResource != null &&
                renderState.ViewInformation.ViewConfiguration.ShowTexturesInternal)
            {
                deviceContext.PixelShader.SetShaderResource(0, _textureResource.TextureView);
            }
            else
            {
                deviceContext.PixelShader.SetShaderResource(0, null);
            }
        }
    }
}
