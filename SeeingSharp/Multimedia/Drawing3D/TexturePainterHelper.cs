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

// Some namespace mappings
using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Drawing3D
{
    #region using

    using Core;
    using Objects;
    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    internal class TexturePainterHelper
    {
        #region Resource keys
        private NamedOrGenericKey KEY_GEOMETRY = GraphicsCore.GetNextGenericResourceKey();
        private NamedOrGenericKey KEY_RENDER_PARAMETERS = GraphicsCore.GetNextGenericResourceKey();
        private NamedOrGenericKey KEY_MATERIAL = GraphicsCore.GetNextGenericResourceKey();
        #endregion

        #region Configuration
        private NamedOrGenericKey m_texture;

        #endregion

        #region Used resources
        private TextureResource m_textureResource;
        private GeometryResource m_geometryResource;
        private SpriteMaterialResource m_materialResource;
        private DefaultResources m_defaultResources;
        private ObjectRenderParameters m_renderParameters;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturePainterHelper"/> class.
        /// </summary>
        /// <param name="textureKey">The texture key.</param>
        internal TexturePainterHelper(NamedOrGenericKey textureKey)
        {
            m_texture = textureKey;
            Scaling = 1f;
            Opacity = 1f;
            AccentuationFactor = 0f;
        }

        /// <summary>
        /// Loads all resources of this texture painter object.
        /// </summary>
        /// <param name="resources">The target resource dictionary.</param>
        internal void LoadResources(ResourceDictionary resources)
        {
            // Load material
            m_materialResource = resources.GetResourceAndEnsureLoaded<SpriteMaterialResource>(
                KEY_MATERIAL,
                () => new SpriteMaterialResource(m_texture));

            // Load geometry resource
            m_geometryResource = resources.GetResourceAndEnsureLoaded<GeometryResource>(
                KEY_GEOMETRY,
                () =>
                {
                    var structure = new VertexStructure();
                    structure.FirstSurface.BuildRect4V(
                        new Vector3(-1f, -1f, 0f),
                        new Vector3(1f, -1f, 0f),
                        new Vector3(1f, 1f, 0f),
                        new Vector3(-1f, 1f, 0f),
                        Color4Ex.Transparent);
                    structure.FirstSurface.Material = KEY_MATERIAL;
                    return new GeometryResource(structure);
                });

            // Load the texture resource
            m_textureResource = resources.GetResourceAndEnsureLoaded<TextureResource>(m_texture);

            // Get default resources
            m_defaultResources = resources.GetResourceAndEnsureLoaded<DefaultResources>(
                DefaultResources.RESOURCE_KEY,
                () => new DefaultResources());

            m_renderParameters = resources.GetResourceAndEnsureLoaded<ObjectRenderParameters>(
                KEY_RENDER_PARAMETERS,
                () => new ObjectRenderParameters());
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        internal void UnloadResources()
        {
            m_geometryResource = null;
            m_textureResource = null;
            m_defaultResources = null;
            m_renderParameters = null;
            m_materialResource = null;
        }

        /// <summary>
        /// Renders the texture without using any effect.
        /// </summary>
        /// <param name="renderState">The render state to be used.</param>
        internal void RenderPlain(RenderState renderState)
        {
            // Apply rendering parameters
            m_renderParameters.UpdateValues(renderState, new CBPerObject()
            {
                AccentuationFactor = AccentuationFactor,
                BorderMultiplyer = 0f,
                BorderPart = 0f,
                Color = Vector4.Zero,
                Opacity = Opacity,
                SpriteScaling = Scaling,
                World = Matrix.Identity
            });

            // Render using current configuration
            RenderInternal(renderState);
        }

        /// <summary>
        /// Renders the texture finally.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        private void RenderInternal(RenderState renderState)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            // Apply all current rendering parameters
            m_renderParameters.Apply(renderState);

            // Render the object
            deviceContext.OutputMerger.DepthStencilState = m_defaultResources.DepthStencilStateDisableZWrites;

            if (this.AlphaBlendMode == TexturePainterAlphaBlendMode.AlphaBlend)
            {
                deviceContext.OutputMerger.BlendState = m_defaultResources.AlphaBlendingBlendState;
            }

            m_geometryResource.Render(renderState);

            if (this.AlphaBlendMode == TexturePainterAlphaBlendMode.AlphaBlend)
            {
                deviceContext.OutputMerger.BlendState = m_defaultResources.DefaultBlendState;
            }

            deviceContext.OutputMerger.DepthStencilState = m_defaultResources.DepthStencilStateDefault;
        }

        /// <summary>
        /// Gets or sets the scaling.
        /// </summary>
        public float Scaling { get; set; }

        public float AccentuationFactor { get; set; }

        public float Opacity { get; set; }

        /// <summary>
        /// Are resources loaded?
        /// </summary>
        public bool IsLoaded
        {
            get { return (m_geometryResource != null) && (m_texture != null); }
        }

        /// <summary>
        /// Gets or sets the alpha blend mode.
        /// </summary>
        public TexturePainterAlphaBlendMode AlphaBlendMode
        {
            get; set;
        }
    }
}