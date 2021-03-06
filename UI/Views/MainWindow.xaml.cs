﻿namespace Macabre2D.UI.Views {

    using Macabre2D.UI.Common;
    using Macabre2D.UI.ServiceInterfaces;
    using Macabre2D.UI.ViewModels;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    public partial class MainWindow {
        private IDialogService _dialogService;

        public MainWindow(MainWindowViewModel viewModel, IDialogService dialogService) {
            this.DataContext = viewModel;
            this.InitializeComponent();
            this._dialogService = dialogService;
        }

        internal TabControl MainTabControl {
            get {
                return this._contentSwitcher.TrueContent.GetChildOfType<TabControl>();
            }
        }

        protected override void OnClosing(CancelEventArgs e) {
            var saveResult = this._dialogService.ShowSaveDiscardCancelDialog();
            if (saveResult == Models.SaveDiscardCancelResult.Cancel) {
                e.Cancel = true;
            }
            else {
                base.OnClosing(e);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void MenuItem_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            // We don't want double clicking a menu item to cause the title bar double click event.
            e.Handled = true;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Cast<TabItem>().FirstOrDefault() is TabItem tabItem && tabItem.Tag is TabTypes type && this.DataContext is MainWindowViewModel viewModel) {
                viewModel.SelectedTabType = type;
            }
        }
    }
}