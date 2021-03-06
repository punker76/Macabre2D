﻿namespace Macabre2D.UI.Models.FrameworkWrappers {

    using Macabre2D.Framework;
    using Macabre2D.UI.Common;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;

    public sealed class ComponentWrapper : NotifyPropertyChanged, IHierarchical<ComponentWrapper, IParent<ComponentWrapper>> {
        internal readonly ObservableCollection<ComponentWrapper> _children = new ObservableCollection<ComponentWrapper>();
        private IParent<ComponentWrapper> _parent;

        public ComponentWrapper(BaseComponent component) {
            this.Component = component;
            this._children.CollectionChanged += this.Children_CollectionChanged;

            foreach (var child in this.Component.Children) {
                this._children.Add(new ComponentWrapper(child));
            }
        }

        public IReadOnlyCollection<ComponentWrapper> Children {
            get {
                return this._children;
            }
        }

        public BaseComponent Component { get; }

        public Guid Id {
            get {
                return this.Component.Id;
            }
        }

        public Layers Layers {
            get {
                return this.Component.Layers;
            }

            set {
                this.Component.Layers = value;
                this.RaisePropertyChanged();
            }
        }

        public string Name {
            get {
                return this.Component.Name;
            }

            set {
                // TODO: this is bad, undo service not being called
                this.Component.Name = value;
                this.RaisePropertyChanged();
            }
        }

        public IParent<ComponentWrapper> Parent {
            get {
                return this._parent;
            }

            set {
                if (value is ComponentWrapper wrapper && (wrapper.Component.Id == this.Component.Id || this.IsAncestorOf(wrapper))) {
                    return;
                }

                var originalParent = this._parent;
                if (this.Set(ref this._parent, value)) {
                    if (this._parent != null) {
                        this._parent.AddChild(this);

                        if (this._parent is ComponentWrapper componentWrapper) {
                            this.Component.Parent = componentWrapper.Component;
                        }
                        else if (this._parent is SceneWrapper sceneWrapper) {
                            this.Component.Parent = null;
                            sceneWrapper.Scene.AddChild(this.Component);
                        }
                    }

                    if (originalParent != null) {
                        originalParent.RemoveChild(this);
                    }
                }
            }
        }

        public bool AddChild(ComponentWrapper child) {
            var result = false;
            if (!this._children.Contains(child)) {
                this._children.Add(child);
                result = true;
            }

            return result;
        }

        public bool RemoveChild(ComponentWrapper child) {
            return this._children.Remove(child);
        }

        public void UpdateProperty(string pathToProperty, object newValue) {
            this.Component.SetProperty(pathToProperty, newValue);
            this.RaisePropertyChanged(pathToProperty);
        }

        private void ChildPropertyChanged(object sender, PropertyChangedEventArgs e) {
            this.RaisePropertyChanged(nameof(this.Children));
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add) {
                foreach (var newItem in e.NewItems.OfType<ComponentWrapper>()) {
                    newItem.Parent = this;
                    newItem.PropertyChanged += this.ChildPropertyChanged;
                    this.Component.AddChild(newItem.Component);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove) {
                foreach (var oldItem in e.OldItems.OfType<ComponentWrapper>()) {
                    if (oldItem.Parent == this) {
                        oldItem.Parent = null;
                        this.Component.RemoveChild(oldItem.Component);
                    }

                    oldItem.PropertyChanged -= this.ChildPropertyChanged;
                }
            }

            this.RaisePropertyChanged(nameof(this.Children));
        }

        private bool IsAncestorOf(ComponentWrapper wrapper) {
            var result = false;
            if (wrapper != null) {
                foreach (var child in this.Children) {
                    if (result) {
                        break;
                    }

                    result = child.Component.Id == wrapper.Component.Id || child.IsAncestorOf(wrapper);
                }
            }

            return result;
        }
    }
}