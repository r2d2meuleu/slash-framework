﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlueprintManagerViewModel.cs" company="Slash Games">
//   Copyright (c) Slash Games. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BlueprintEditor.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Data;

    using MonitoredUndo;

    using Slash.GameBase.Blueprints;
    using Slash.Tools.BlueprintEditor.Logic.Annotations;
    using Slash.Tools.BlueprintEditor.Logic.Data;

    public class BlueprintManagerViewModel : INotifyPropertyChanged, IDataErrorInfo, ISupportsUndo
    {
        #region Fields

        private readonly BlueprintManager blueprintManager;

        private IEnumerable<Type> assemblyComponents;

        private ListCollectionView blueprintsView;

        private string newBlueprintId;

        #endregion

        #region Constructors and Destructors

        public BlueprintManagerViewModel(BlueprintManager blueprintManager)
        {
            this.blueprintManager = blueprintManager;

            this.Blueprints = new ObservableCollection<BlueprintViewModel>();

            IEnumerable<KeyValuePair<string, Blueprint>> blueprints = this.blueprintManager.Blueprints;
            foreach (var blueprintPair in blueprints)
            {
                this.Blueprints.Add(
                    new BlueprintViewModel(blueprintPair.Key, blueprintPair.Value)
                        {
                            AssemblyComponents = this.assemblyComponents
                        });
            }

            this.Blueprints.CollectionChanged += this.OnBlueprintsChanged;

            // Setup correct parent hierarchy.
            foreach (var blueprint in this.Blueprints.Where(blueprint => blueprint.Blueprint.ParentId != null))
            {
                this.ReparentBlueprint(blueprint.BlueprintId, blueprint.Blueprint.ParentId);
            }
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///   All available entity component types which can be added to a blueprint.
        /// </summary>
        public IEnumerable<Type> AssemblyComponents
        {
            get
            {
                return this.assemblyComponents;
            }
            set
            {
                this.assemblyComponents = value;

                // Update component table.
                InspectorComponentTable.LoadComponents();

                foreach (var blueprintViewModel in this.Blueprints)
                {
                    blueprintViewModel.AssemblyComponents = value;
                }
            }
        }

        public IEnumerable<KeyValuePair<string, Blueprint>> BlueprintPairs
        {
            get
            {
                return this.blueprintManager.Blueprints;
            }
        }

        public ObservableCollection<BlueprintViewModel> Blueprints { get; private set; }

        public ListCollectionView BlueprintsView
        {
            get
            {
                if (this.blueprintsView == null)
                {
                    this.blueprintsView = new ListCollectionView(this.Blueprints);
                    this.blueprintsView.SortDescriptions.Add(
                        new SortDescription("BlueprintId", ListSortDirection.Ascending));

                    // Don't show blueprints at root level that have parents.
                    this.blueprintsView.Filter = blueprint => ((BlueprintViewModel)blueprint).Blueprint.ParentId == null;
                }
                return this.blueprintsView;
            }
        }

        public string Error
        {
            get
            {
                return null;
            }
        }

        public string NewBlueprintId
        {
            get
            {
                return this.newBlueprintId;
            }
            set
            {
                if (Equals(value, this.newBlueprintId))
                {
                    return;
                }

                this.newBlueprintId = value;

                this.OnPropertyChanged("NewBlueprintId");
            }
        }

        #endregion

        #region Public Indexers

        public string this[string columnName]
        {
            get
            {
                string result = null;
                if (columnName == "NewBlueprintId")
                {
                    // Check if already existent.
                    if (this.blueprintManager != null && this.NewBlueprintId != null)
                    {
                        if (this.blueprintManager.ContainsBlueprint(this.NewBlueprintId))
                        {
                            result = "Blueprint id already exists.";
                        }
                    }
                }

                return result;
            }
        }

        #endregion

        #region Public Methods and Operators

        public bool CanExecuteCreateNewBlueprint()
        {
            return this.blueprintManager != null && !String.IsNullOrEmpty(this.newBlueprintId)
                   && !this.blueprintManager.ContainsBlueprint(this.newBlueprintId);
        }

        public void CreateNewBlueprint()
        {
            // Update blueprint view models.
            this.Blueprints.Add(
                new BlueprintViewModel(this.newBlueprintId, new Blueprint())
                    {
                        AssemblyComponents = this.assemblyComponents
                    });

            // Clear blueprint id.
            this.NewBlueprintId = String.Empty;
        }

        public object GetUndoRoot()
        {
            return this;
        }

        public void RemoveBlueprint(string blueprintId)
        {
            // Get blueprint to remove.
            var blueprintToRemove = this.Blueprints.First(blueprint => blueprint.BlueprintId == blueprintId);

            // Check for blueprints that derive from the blueprint to remove.
            if (blueprintToRemove.DerivedBlueprints.Count > 0)
            {
                throw new InvalidOperationException("Other blueprints depend on this one!");
            }

            this.blueprintManager.RemoveBlueprint(blueprintId);
            this.Blueprints.Remove(blueprintToRemove);

            // Remove from parent's children list, if necessary.
            if (blueprintToRemove.Blueprint.ParentId != null)
            {
                var parent =
                    this.Blueprints.First(blueprint => blueprint.BlueprintId == blueprintToRemove.Blueprint.ParentId);
                parent.DerivedBlueprints.Remove(blueprintToRemove);
            }
        }

        /// <summary>
        ///   Changes the parent of the blueprint with the specified id, updating
        ///   children collections.
        /// </summary>
        /// <param name="childId">Id of the blueprint whose parent to change.</param>
        /// <param name="newParentId">Id of the new parent blueprint.</param>
        public void ReparentBlueprint(string childId, string newParentId)
        {
            // Find blueprints.
            var childBlueprint = this.Blueprints.First(blueprint => blueprint.BlueprintId.Equals(childId));

            var oldParentBlueprintId = childBlueprint.Blueprint.ParentId;
            var oldParentBlueprint =
                this.Blueprints.FirstOrDefault(blueprint => blueprint.BlueprintId.Equals(oldParentBlueprintId));

            var newParentBlueprint = this.Blueprints.First(blueprint => blueprint.BlueprintId.Equals(newParentId));

            // Update parent.
            childBlueprint.Blueprint.ParentId = newParentBlueprint.BlueprintId;

            // Update children of old parent.
            if (oldParentBlueprint != null)
            {
                oldParentBlueprint.DerivedBlueprints.Remove(childBlueprint);
            }

            // Update children of new parent.
            newParentBlueprint.DerivedBlueprints.Add(childBlueprint);

            if (this.blueprintsView != null)
            {
                this.blueprintsView.Refresh();
            }
        }

        #endregion

        #region Methods

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnBlueprintsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            string undoMessage = null;
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // Update blueprints in blueprint manager.
                foreach (BlueprintViewModel item in e.NewItems)
                {
                    this.blueprintManager.AddBlueprint(item.BlueprintId, item.Blueprint);
                }

                if (e.NewItems.Count == 1)
                {
                    BlueprintViewModel item = (BlueprintViewModel)e.NewItems[0];
                    undoMessage = string.Format("Add blueprint '{0}'", item.BlueprintId);
                }
                else
                {
                    undoMessage = "Add blueprints";
                }

                foreach (BlueprintViewModel item in e.NewItems)
                {
                    item.Root = this;

                    // Setup correct parent hierarchy.
                    if (item.Blueprint.ParentId != null)
                    {
                        this.ReparentBlueprint(item.BlueprintId, item.Blueprint.ParentId);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // Update blueprints in blueprint manager.
                foreach (BlueprintViewModel item in e.OldItems)
                {
                    this.blueprintManager.RemoveBlueprint(item.BlueprintId);
                }

                if (e.OldItems.Count == 1)
                {
                    BlueprintViewModel item = (BlueprintViewModel)e.OldItems[0];
                    undoMessage = string.Format("Remove blueprint '{0}'", item.BlueprintId);
                }
                else
                {
                    undoMessage = "Remove blueprints";
                }

                foreach (BlueprintViewModel item in e.OldItems)
                {
                    item.Root = null;

                    // Remove from parent's children list, if necessary.
                    if (item.Blueprint.ParentId != null)
                    {
                        var parent = this.Blueprints.First(
                            blueprint => blueprint.BlueprintId == item.Blueprint.ParentId);
                        parent.DerivedBlueprints.Remove(item);
                    }
                }
            }

            // This line will log the collection change with the undo framework.
            DefaultChangeFactory.Current.OnCollectionChanged(
                this, "Blueprints", this.Blueprints, e, undoMessage ?? "Collection of Blueprints changed");
        }

        #endregion
    }
}