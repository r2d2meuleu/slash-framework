﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectSettings.cs" company="Slash Games">
//   Copyright (c) Slash Games. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Slash.Tools.BlueprintEditor.Logic.Context
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;

    using Slash.GameBase.Blueprints;
    using Slash.GameBase.Components;
    using Slash.Reflection.Utils;
    using Slash.Tools.BlueprintEditor.Logic.Data;

    /// <summary>
    ///    Blueprint file which belongs to a project.
    /// </summary>
    public sealed class BlueprintFile
    {
        /// <summary>
        ///   Blueprint manager which contains the data.
        /// </summary>
        public BlueprintManager BlueprintManager { get; set; }

        /// <summary>
        ///   Path where the blueprint file is stored.
        /// </summary>
        public string Path { get; set; }
    }

    /// <summary>
    ///   Settings for the project the blueprints are edited for.
    /// </summary>
    [Serializable]
    public sealed class ProjectSettings
    {
        #region Fields

        /// <summary>
        ///   Available entity component types in the project.
        /// </summary>
        private IEnumerable<Type> entityComponentTypes;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Constructor.
        /// </summary>
        public ProjectSettings()
        {
            this.ProjectAssemblies = new List<Assembly>();
            this.BlueprintFiles = new List<BlueprintFile>();
        }

        #endregion

        #region Delegates

        public delegate void EntityComponentTypesChangedDelegate();

        #endregion

        #region Public Events

        public event EntityComponentTypesChangedDelegate EntityComponentTypesChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///   Available entity component types in the project.
        /// </summary>
        public IEnumerable<Type> EntityComponentTypes
        {
            get
            {
                return this.entityComponentTypes ?? (this.entityComponentTypes = this.CollectEntityComponentTypes());
            }
        }

        [XmlIgnore]
        public IList<BlueprintFile> BlueprintFiles { get; set; }

        /// <summary>
        ///   Wrapper for ProjectAssemblies property for xml serialization.
        /// </summary>
        [XmlArray("BlueprintFiles", Order = 1)]
        [XmlArrayItem("BlueprintFile")]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string[] BlueprintFilesSerialized
        {
            get
            {
                return this.BlueprintFiles.Select(projectAssembly => projectAssembly.Path).ToArray();
            }
            set
            {
                this.BlueprintFiles = value.Select(path => new BlueprintFile { Path = path }).ToList();
            }
        }

            /// <summary>
        ///   Wrapper for ProjectAssemblies property for xml serialization.
        /// </summary>
        [XmlArray("ProjectAssemblies", Order = 0)]
        [XmlArrayItem("ProjectAssembly")]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string[] ProjectAssembliesSerialized
        {
            get
            {
                return this.ProjectAssemblies.Select(projectAssembly => projectAssembly.Location).ToArray();
            }
            set
            {
                this.ProjectAssemblies = value.Select(ReflectionUtils.FindAssembly).ToList();
                this.entityComponentTypes = null;
                this.OnEntityComponentTypesChanged();
            }
        }

        /// <summary>
        ///   Assemblies which belong to the project.
        /// </summary>
        [XmlIgnore]
        public IList<Assembly> ProjectAssemblies { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///   Adds an assembly to use to find available types.
        /// </summary>
        /// <param name="assembly">Assembly to add to the project.</param>
        public void AddAssembly(Assembly assembly)
        {
            this.ProjectAssemblies.Add(assembly);
            this.entityComponentTypes = null;
            this.OnEntityComponentTypesChanged();
        }


        /// <summary>
        ///   Removes an assembly from the project.
        /// </summary>
        /// <param name="assembly">Assembly to remove.</param>
        public bool RemoveAssembly(Assembly assembly)
        {
            if (!this.ProjectAssemblies.Remove(assembly))
            {
                return false;
            }

            this.entityComponentTypes = null;
            this.OnEntityComponentTypesChanged();

            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Collects all entity component types from the project assemblies.
        /// </summary>
        /// <returns>All types which are inherited from IEntityComponent in the project assemblies.</returns>
        private IEnumerable<Type> CollectEntityComponentTypes()
        {
            return ComponentUtils.FindComponentTypes(this.ProjectAssemblies);
        }

        private void OnEntityComponentTypesChanged()
        {
            EntityComponentTypesChangedDelegate handler = this.EntityComponentTypesChanged;
            if (handler != null)
            {
                handler();
            }
        }

        #endregion
    }
}