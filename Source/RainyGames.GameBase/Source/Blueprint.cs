﻿// -----------------------------------------------------------------------
// <copyright file="Blueprint.cs" company="Rainy Games">
// Copyright (c) Rainy Games. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace RainyGames.GameBase
{
    using System;
    using System.Collections.Generic;
    using RainyGames.Collections.AttributeTables;

    /// <summary>
    /// Blueprint for creating an entity with a specific set of components
    /// and initial attribute values.
    /// </summary>
    public class Blueprint
    {
        #region Constructors and Destructors

        /// <summary>
        /// Constructs a new blueprint without any components or data.
        /// </summary>
        public Blueprint()
        {
            this.ComponentTypes = new List<Type>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Collection of types of components to add to entities created with
        /// this blueprint.
        /// </summary>
        public List<Type> ComponentTypes { get; set; }

        /// <summary>
        /// Data for initializing the components of entities created with this
        /// blueprint.
        /// </summary>
        public IAttributeTable AttributeTable { get; set; }

        #endregion
    }
}