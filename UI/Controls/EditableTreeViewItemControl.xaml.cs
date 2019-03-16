﻿namespace Macabre2D.UI.Controls {

    using Macabre2D.UI.Common;
    using Macabre2D.UI.Models;
    using Macabre2D.UI.ServiceInterfaces;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public partial class EditableTreeViewItemControl : UserControl, INotifyPropertyChanged {

        public static readonly DependencyProperty AllowUndoProperty = DependencyProperty.Register(
            nameof(AllowUndo),
            typeof(bool),
            typeof(EditableTreeViewItemControl),
            new PropertyMetadata());

        public static readonly DependencyProperty InvalidTypesProperty = DependencyProperty.Register(
            nameof(InvalidTypes),
            typeof(IEnumerable<Type>),
            typeof(EditableTreeViewItemControl),
            new PropertyMetadata(new List<Type>()));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(EditableTreeViewItemControl),
            new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnTextChanged)));

        public static readonly DependencyProperty ValidTypesProperty = DependencyProperty.Register(
            nameof(ValidTypes),
            typeof(IEnumerable<Type>),
            typeof(EditableTreeViewItemControl),
            new PropertyMetadata(new List<Type>()));

        private readonly IUndoService _undoService;

        private bool _isEditing;

        public EditableTreeViewItemControl() {
            this._undoService = ViewContainer.Resolve<IUndoService>();
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool AllowUndo {
            get { return (bool)this.GetValue(AllowUndoProperty); }
            set { this.SetValue(AllowUndoProperty, value); }
        }

        public IEnumerable<Type> InvalidTypes {
            get { return (IEnumerable<Type>)this.GetValue(InvalidTypesProperty); }
            set { this.SetValue(InvalidTypesProperty, value); }
        }

        public bool IsEditing {
            get {
                return this._isEditing;
            }

            set {
                if (this._isEditing != value) {
                    this._isEditing = value;

                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsEditing)));
                }
            }
        }

        public string Text {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public IEnumerable<Type> ValidTypes {
            get { return (IEnumerable<Type>)this.GetValue(ValidTypesProperty); }
            set { this.SetValue(ValidTypesProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is EditableTreeViewItemControl control) {
                control._editableTextBox.Text = e.NewValue as string;
            }
        }

        private bool CanEditTreeViewItem(TreeViewItem treeViewItem) {
            return treeViewItem?.DataContext != null &&
                treeViewItem.IsSelected &&
                (this.InvalidTypes == null || !this.InvalidTypes.Contains(treeViewItem.DataContext.GetType())) &&
                (this.ValidTypes == null || !this.ValidTypes.Any() || this.ValidTypes.Contains(treeViewItem.DataContext.GetType()));
        }

        private void CommitNewText(string oldText, string newText) {
            if (this.AllowUndo) {
                var undoCommand = new UndoCommand(() => {
                    this.SetText(newText);
                }, () => {
                    this.SetText(oldText);
                });

                this._undoService.Do(undoCommand);
            }
            else {
                this.SetText(newText);
            }

            this.IsEditing = false;
        }

        private void SetText(string text) {
            this.Dispatcher.BeginInvoke((Action)(() => {
                this.Text = text;
                this._editableTextBox.Text = text;
            }));
        }

        private void TreeItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (sender is TextBox textBox && textBox.IsVisible) {
                textBox.Focus();
                textBox.SelectAll();
            }
        }

        private void TreeItem_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                this.CommitNewText(this.Text, this._editableTextBox.Text);
            }
            else if (e.Key == Key.Escape) {
                if (sender is TextBox textBox) {
                    textBox.Text = this.Text;
                    this.IsEditing = false;
                }
            }
        }

        private void TreeItem_LostFocus(object sender, RoutedEventArgs e) {
            this.IsEditing = false;
            this._editableTextBox.Text = this.Text;
        }

        private void TreeItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var treeViewItem = (e.OriginalSource as DependencyObject)?.FindAncestor<TreeViewItem>();
            if (this.CanEditTreeViewItem(treeViewItem)) {
                this._editableTextBox.Text = this.Text;
                this.IsEditing = true;
                e.Handled = true;
            }
        }
    }
}