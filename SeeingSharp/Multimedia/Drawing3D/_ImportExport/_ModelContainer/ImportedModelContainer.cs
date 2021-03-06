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
using System.Numerics;
using System.Threading;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing3D
{
    /// <summary>
    /// Container for imported model data.
    /// </summary>
    public class ImportedModelContainer
    {
        // Static id counter 
        private static int s_maxContainerId;

        // All model data
        private int _importId;
        private ImportOptions _importOptions;
        private List<SceneObject> _objects;
        private List<ParentChildRelationship> _parentChildRelationships;
        private List<ImportedResourceInfo> _importedResources;

        // State
        private bool _isFinished;
        private bool _isValid;
        private Exception _finishException;

        public ResourceLink Source { get; }

        /// <summary>
        /// Gets a collection containing all imported objects.
        /// </summary>
        public IReadOnlyList<SceneObject> Objects => _objects;

        /// <summary>
        /// Gets the hierarchy information of the imported objects.
        /// </summary>
        public IReadOnlyList<ParentChildRelationship> ParentChildRelationships => _parentChildRelationships;

        /// <summary>
        /// Gets a collection containing all imported resources.
        /// </summary>
        public IReadOnlyList<ImportedResourceInfo> ImportedResources => _importedResources;

        /// <summary>
        /// Is loading finished?
        /// </summary>
        public bool IsFinished => _isFinished;

        /// <summary>
        /// Does this object contain a valid model?
        /// </summary>
        public bool IsValid => _isValid;

        /// <summary>
        /// An exception occurred during loading. This property may be set, when loading is finished and IsValid=false.
        /// </summary>
        public Exception FinishException => _finishException;

        /// <summary>
        /// The root object which gets generated when loading is finished successfully.
        /// All objects loaded are children of this root object.
        /// </summary>
        public ScenePivotObject RootObject { get; private set; }

        /// <summary>
        /// The bounding volume within the space of the root object.
        /// It is set when loading is finished successfully.
        /// </summary>
        public BoundingBox BoundingBox { get; private set; } = BoundingBox.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportedModelContainer" /> class.
        /// </summary>
        public ImportedModelContainer(ResourceLink source, ImportOptions importOptions)
        {
            this.Source = source;
            _importOptions = importOptions;
            _objects = new List<SceneObject>();
            _parentChildRelationships = new List<ParentChildRelationship>();
            _importedResources = new List<ImportedResourceInfo>();
            _importId = Interlocked.Increment(ref s_maxContainerId);
        }

        public void AddObject(SceneObject objectToAdd)
        {
            this.EnsureNotFinished();

            _objects.Add(objectToAdd);
        }

        public void AddParentChildRelationship(ParentChildRelationship relationship)
        {
            this.EnsureNotFinished();

            _parentChildRelationships.Add(relationship);
        }

        public void AddResource(ImportedResourceInfo resourceInfo)
        {
            this.EnsureNotFinished();

            _importedResources.Add(resourceInfo);
        }

        /// <summary>
        /// Finishes loading with the given exception.
        /// </summary>
        public void FinishLoading(Exception ex)
        {
            if (_isFinished)
            {
                throw new SeeingSharpException("ModelContainer already finished!");
            }

            _isFinished = true;
            _finishException = ex;

            _objects.Clear();
            _parentChildRelationships.Clear();
            _importedResources.Clear();

            this.RootObject = null;
            this.BoundingBox = BoundingBox.Empty;
        }

        /// <summary>
        /// Creates and adds the root for all imported scene objects.
        /// </summary>
        public void FinishLoading(BoundingBox boundingBox)
        {
            this.EnsureNotFinished();

            try
            {
                // Generic checks
                if (this.Objects.Count == 0)
                {
                    throw new SeeingSharpException("No objects imported");
                }
                if (EngineMath.EqualsWithTolerance(boundingBox.Width, 0) ||
                    EngineMath.EqualsWithTolerance(boundingBox.Height, 0) ||
                    EngineMath.EqualsWithTolerance(boundingBox.Depth, 0))
                {
                    throw new SeeingSharpException($"BoundingBox of the loaded model data seems to be empty (Width={boundingBox.Width}, Height={boundingBox.Height}, Depth={boundingBox.Height}");
                }

                // Create root for the imported object graph
                var rootObject = new ScenePivotObject();
                rootObject.TransformationType = SpacialTransformationType.ScalingTranslationEulerAngles;

                // Configure base transformation of the root object
                switch (_importOptions.ResourceCoordinateSystem)
                {
                    case CoordinateSystem.LeftHanded_UpY:
                        break;

                    case CoordinateSystem.LeftHanded_UpZ:
                        rootObject.Scaling = new Vector3(1f, -1f, 1f);
                        rootObject.RotationEuler = new Vector3(-EngineMath.RAD_90DEG, 0f, 0f);
                        break;

                    case CoordinateSystem.RightHanded_UpY:
                        rootObject.Scaling = new Vector3(1f, 1f, -1f);
                        break;

                    case CoordinateSystem.RightHanded_UpZ:
                        rootObject.Scaling = new Vector3(-1f, 1f, -1f);
                        rootObject.RotationEuler = new Vector3(EngineMath.RAD_90DEG, 0f, 0f);
                        break;
                }

                // Configure position and scaling of the root object
                if (_importOptions.FitToCube)
                {
                    var scaleFactor = Math.Min(
                        1f / boundingBox.Width,
                        Math.Min(1f / boundingBox.Height, 1f / boundingBox.Depth));
                    rootObject.Scaling *= scaleFactor;
                    rootObject.Position = new Vector3(
                        (0f - (boundingBox.Minimum.X + (boundingBox.Maximum.X - boundingBox.Minimum.X) / 2f)) *
                        scaleFactor,
                        (0f - (boundingBox.Minimum.Y + (boundingBox.Maximum.Y - boundingBox.Minimum.Y) / 2f)) *
                        scaleFactor,
                        (0f - (boundingBox.Minimum.Z + (boundingBox.Maximum.Z - boundingBox.Minimum.Z) / 2f)) *
                        scaleFactor);
                }

                // Find current root objects and assign them as child to the new root object
                foreach (var actRootObject in this.FindRootObjects())
                {
                    _parentChildRelationships.Add(
                        new ParentChildRelationship(rootObject, actRootObject));
                }

                // AddObject the object finally
                _objects.Add(rootObject);
                this.RootObject = rootObject;
                this.BoundingBox = boundingBox;

                _isValid = true;
            }
            catch (Exception ex)
            {
                this.FinishLoading(ex);
            }
            finally
            {
                _isFinished = true;
            }
        }

        /// <summary>
        /// Generates a key for a resource contained in an imported object graph.
        /// </summary>
        /// <param name="resourceClass">The type of the resource (defined by importer).</param>
        /// <param name="resourceId">The id of the resource (defined by importer)</param>
        public NamedOrGenericKey GetResourceKey(string resourceClass, string resourceId)
        {
            return new NamedOrGenericKey(
                "Imported." + _importId + "." + resourceClass + "." + resourceId);
        }

        private void EnsureNotFinished()
        {
            if (_isFinished)
            {
                throw new SeeingSharpException("ModelContainer already finished!");
            }
        }

        /// <summary>
        /// Search for root objects (objects with no parents).
        /// </summary>
        private IEnumerable<SceneObject> FindRootObjects()
        {
            foreach(var actObject in _objects)
            {
                var isRoot = true;
                foreach(var actParentChildRelationship in _parentChildRelationships)
                {
                    if (actObject == actParentChildRelationship.Child)
                    {
                        isRoot = false;
                        break;
                    }
                }

                if (isRoot)
                {
                    yield return actObject;
                }
            }
        }
    }
}