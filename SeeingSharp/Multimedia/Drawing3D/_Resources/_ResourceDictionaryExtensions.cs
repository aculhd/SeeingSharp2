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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing3D
{
    internal static class ResourceDictionaryExtensions
    {
        /// <summary>
        /// Gets or creates the material resource for the given <see cref="GeometrySurface"/> object.
        /// </summary>
        internal static MaterialResource GetOrCreateMaterialResourceAndEnsureLoaded(this ResourceDictionary resourceDict, GeometrySurface targetSurface)
        {
            var materialResource = GetOrCreateMaterialResource(resourceDict, targetSurface);

            if (!materialResource.IsLoaded)
            {
                materialResource.LoadResource();
            }

            return materialResource;
        }

        /// <summary>
        /// Gets or creates the material resource for the given <see cref="GeometrySurface"/> object.
        /// </summary>
        internal static MaterialResource GetOrCreateMaterialResource(this ResourceDictionary resourceDict, GeometrySurface targetSurface)
        {
            var materialKey = targetSurface.Material;
            var textureKey = targetSurface.TextureKey;

            // Get the material if it is already created
            if (!materialKey.IsEmpty && resourceDict.ContainsResource(materialKey))
            {
                return resourceDict.GetResource<MaterialResource>(materialKey);
            }

            // Generate a dynamic material key
            if (materialKey.IsEmpty)
            {
                materialKey = new NamedOrGenericKey(targetSurface.MaterialProperties.GetDynamicResourceKey());
            }

            // Get the material if it is already created
            if (resourceDict.ContainsResource(materialKey))
            {
                return resourceDict.GetResource<MaterialResource>(materialKey);
            }

            if(textureKey.IsEmpty)
            {
                // Create a default material without any texture
                var result = resourceDict.AddResource(materialKey, new SimpleColoredMaterialResource());
                result.MaterialDiffuseColor = targetSurface.MaterialProperties.DiffuseColor;
                return result;
            }

            // Create texture resource if needed
            try
            {
                if (!resourceDict.ContainsResource(textureKey) &&
                    !string.IsNullOrEmpty(textureKey.NameKey))
                {
                    // Try to find and create the texture resource by its name
                    if (targetSurface.ResourceLink != null)
                    {
                        var textureResourceLink = targetSurface.ResourceLink.GetForAnotherFile(textureKey.NameKey);

                        resourceDict.AddResource(
                            textureKey,
                            new StandardTextureResource(
                                targetSurface.ResourceLink.GetForAnotherFile(textureKey.NameKey)));
                    }
                    else if (targetSurface.ResourceSourceAssembly != null)
                    {
                        var textureResourceLink = new AssemblyResourceLink(
                            targetSurface.ResourceSourceAssembly,
                            targetSurface.ResourceSourceAssembly.GetName().Name + ".Resources.Textures",
                            textureKey.NameKey);
                        if (textureResourceLink.IsValid())
                        {
                            resourceDict.AddResource(
                                textureKey,
                                new StandardTextureResource(textureResourceLink));
                        }
                        else
                        {
                            // Unable to resolve texture
                            textureKey = NamedOrGenericKey.Empty;
                        }
                    }
                    else
                    {
                        // Unable to resolve texture
                        textureKey = NamedOrGenericKey.Empty;
                    }
                }
            }
            catch(Exception ex)
            {
                GraphicsCore.PublishInternalExceptionInfo(ex, InternalExceptionLocation.LoadingTexture);
                textureKey = NamedOrGenericKey.Empty;
            }

            // Create a default textured material
            if (!textureKey.IsEmpty)
            {
                var result = resourceDict.AddResource(
                    materialKey,
                    new SimpleColoredMaterialResource(textureKey));
                result.MaterialDiffuseColor = targetSurface.MaterialProperties.DiffuseColor;
                return result;
            }
            else
            {
                var result = resourceDict.AddResource(
                    materialKey,
                    new SimpleColoredMaterialResource());
                result.MaterialDiffuseColor = targetSurface.MaterialProperties.DiffuseColor;
                return result;
            }
        }

        /// <summary>
        /// Adds a new geometry resource.
        /// </summary>
        internal static GeometryResource AddGeometry(this ResourceDictionary resourceDictionary, Geometry structure)
        {
            return resourceDictionary.AddResource(new GeometryResource(structure));
        }

        /// <summary>
        /// Adds a new geometry resource.
        /// </summary>
        internal static GeometryResource AddGeometry(this ResourceDictionary resourceDictionary, NamedOrGenericKey resourceKey, Geometry structure)
        {
            return resourceDictionary.AddResource(resourceKey, new GeometryResource(structure));
        }

        /// <summary>
        /// Adds a new geometry resource.
        /// </summary>
        internal static GeometryResource AddGeometry(this ResourceDictionary resourceDictionary, GeometryFactory objectType)
        {
            return resourceDictionary.AddResource(new GeometryResource(objectType));
        }

        /// <summary>
        /// Adds a new geometry resource.
        /// </summary>
        internal static GeometryResource AddGeometry(this ResourceDictionary resourceDictionary, NamedOrGenericKey resourceKey, GeometryFactory objectType)
        {
            return resourceDictionary.AddResource(resourceKey, new GeometryResource(objectType));
        }

        /// <summary>
        /// Adds a new text geometry with the given text.
        /// </summary>
        internal static GeometryResource AddTextGeometry(this ResourceDictionary resourceDictionary, string textToAdd)
        {
            return resourceDictionary.AddTextGeometry(textToAdd, TextGeometryOptions.Default);
        }

        /// <summary>
        /// Adds a new text geometry with the given text.
        /// </summary>
        internal static GeometryResource AddTextGeometry(this ResourceDictionary resourceDictionary, NamedOrGenericKey resourceKey, string textToAdd)
        {
            return resourceDictionary.AddTextGeometry(resourceKey, textToAdd, TextGeometryOptions.Default);
        }

        /// <summary>
        /// Adds a new text geometry with the given text.
        /// </summary>
        internal static GeometryResource AddTextGeometry(this ResourceDictionary resourceDictionary, string textToAdd, TextGeometryOptions textGeometryOptions)
        {
            var newStructure = new Geometry();
            newStructure.FirstSurface.BuildTextGeometry(textToAdd, textGeometryOptions);
            newStructure.FirstSurface.Material = textGeometryOptions.SurfaceMaterial;
            return resourceDictionary.AddGeometry(newStructure);
        }

        /// <summary>
        /// Adds a new text geometry with the given text.
        /// </summary>
        internal static GeometryResource AddTextGeometry(this ResourceDictionary resourceDictionary, NamedOrGenericKey resourceKey, string textToAdd, TextGeometryOptions textGeometryOptions)
        {
            var newStructure = new Geometry();
            newStructure.FirstSurface.BuildTextGeometry(textToAdd, textGeometryOptions);
            newStructure.FirstSurface.Material = textGeometryOptions.SurfaceMaterial;
            return resourceDictionary.AddGeometry(resourceKey, newStructure);
        }

        /// <summary>
        /// Adds a new texture resource pointing to the given texture file name.
        /// </summary>
        internal static StandardTextureResource AddTexture(this ResourceDictionary resourceDictionary, string textureFileName)
        {
            return resourceDictionary.AddResource(new StandardTextureResource(textureFileName));
        }

        /// <summary>
        /// Adds a new texture resource pointing to the given texture file name.
        /// </summary>
        internal static StandardTextureResource AddTexture(this ResourceDictionary resourceDictionary, NamedOrGenericKey resourceKey, string textureFileName)
        {
            return resourceDictionary.AddResource(resourceKey, new StandardTextureResource(textureFileName));
        }
    }
}
