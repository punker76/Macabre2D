﻿namespace Macabre2D.UI.Services {

    using Macabre2D.Framework;
    using Macabre2D.UI.Models.FrameworkWrappers;
    using Macabre2D.UI.ServiceInterfaces;
    using System;

    public sealed class ComponentService : NotifyPropertyChanged, IComponentService {
        private readonly ISceneService _sceneService;

        private ComponentWrapper _selectedItem;

        public ComponentService(ISceneService sceneService) {
            this._sceneService = sceneService;
        }

        public event EventHandler<ValueChangedEventArgs<ComponentWrapper>> SelectionChanged;

        public ComponentWrapper SelectedItem {
            get {
                return this._selectedItem;
            }

            set {
                var oldValue = this._selectedItem;
                if (this.Set(ref this._selectedItem, value)) {
                    this.SelectionChanged.SafeInvoke(this, new ValueChangedEventArgs<ComponentWrapper>(oldValue, value));
                }
            }
        }

        public void SelectComponent(BaseComponent component) {
            if (this._sceneService.CurrentScene is SceneWrapper scene) {
                ComponentWrapper result = null;
                foreach (var child in scene.Children) {
                    if (child.Id == component.Id) {
                        result = child;
                    }
                    else {
                        result = this.FindWrapperInChildren(child, component);
                    }

                    if (result != null) {
                        this.SelectedItem = result;
                        break;
                    }
                }
            }
        }

        private ComponentWrapper FindWrapperInChildren(ComponentWrapper parent, BaseComponent component) {
            ComponentWrapper result = null;
            foreach (var child in parent.Children) {
                if (child.Id == component.Id) {
                    result = child;
                }
                else {
                    result = this.FindWrapperInChildren(child, component);
                }

                if (result != null) {
                    break;
                }
            }

            return result;
        }
    }
}