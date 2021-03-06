﻿namespace Macabre2D.UI.Controls.SceneEditing {

    using Macabre2D.Framework;
    using Macabre2D.Framework.Diagnostics;
    using Macabre2D.UI.Common;
    using Macabre2D.UI.ServiceInterfaces;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;

    internal sealed class EditorCameraWrapper : NotifyPropertyChanged {
        private readonly EditorGame _editorGame;
        private readonly ISceneService _sceneService;
        private Camera _camera = new Camera();
        private int _previousScrollWheelValue = 0;
        private GridDrawer _primaryGridDrawer;
        private GridDrawer _secondaryGridDrawer;

        internal EditorCameraWrapper(EditorGame editorGame) {
            this._editorGame = editorGame;
            this._sceneService = ViewContainer.Resolve<ISceneService>();
        }

        internal Camera Camera {
            get {
                return this._camera;
            }

            private set {
                if (value == null) {
                    value = new Camera();
                }

                var oldCamera = this._camera;
                if (this.Set(ref this._camera, value)) {
                    if (this._camera != null && this._editorGame.CurrentScene != null && this._editorGame.IsInitialized) {
                        this.Initialize();
                    }

                    if (oldCamera != null) {
                        oldCamera.ViewHeightChanged -= this.Camera_ViewHeightChanged;
                    }

                    if (this._camera != null) {
                        this._camera.ViewHeightChanged += this.Camera_ViewHeightChanged;
                    }
                }
            }
        }

        internal void Draw(GameTime gameTime) {
            if (this._editorGame.ShowGrid && this._primaryGridDrawer != null && this._secondaryGridDrawer != null) {
                if (this._editorGame.CurrentScene != null) {
                    var contrastingColor = this._editorGame.CurrentScene.BackgroundColor.GetContrastingBlackOrWhite();
                    this._primaryGridDrawer.Color = new Color(contrastingColor, 60);
                    this._secondaryGridDrawer.Color = new Color(contrastingColor, 30);
                }

                this._primaryGridDrawer.Draw(gameTime, this._camera.ViewHeight);
                this._secondaryGridDrawer.Draw(gameTime, this._camera.ViewHeight);
            }
        }

        internal void Initialize() {
            this._camera.Initialize(this._editorGame.CurrentScene);
            var gridSize = this.GetGridSize();
            this._primaryGridDrawer = new GridDrawer() {
                Camera = this._camera,
                Color = new Color(255, 255, 255, 100),
                ColumnWidth = gridSize,
                LineThickness = 1f,
                RowHeight = gridSize,
                UseDynamicLineThickness = true
            };

            this._primaryGridDrawer.Initialize(this._editorGame.CurrentScene);

            var smallGridSize = gridSize / 2f;
            this._secondaryGridDrawer = new GridDrawer() {
                Camera = this._camera,
                Color = new Color(255, 255, 255, 75),
                ColumnWidth = smallGridSize,
                LineThickness = 1f,
                RowHeight = smallGridSize,
                UseDynamicLineThickness = true
            };

            this._secondaryGridDrawer.Initialize(this._editorGame.CurrentScene);
        }

        internal void RefreshCamera() {
            this.Camera = this._sceneService.CurrentScene?.SceneAsset?.Camera;
            if (this._editorGame.IsInitialized && this._editorGame.CurrentScene != null) {
                this.Camera.Initialize(this._editorGame.CurrentScene);
            }
        }

        internal void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState) {
            if (mouseState.ScrollWheelValue != this._previousScrollWheelValue) {
                var scrollViewChange = (float)(gameTime.ElapsedGameTime.TotalSeconds * (this._previousScrollWheelValue - mouseState.ScrollWheelValue)) * 5f;

                var isZoomIn = scrollViewChange < 0;
                if (isZoomIn) {
                    this.Camera.ZoomTo(mouseState.Position, scrollViewChange * -1f);
                }
                else {
                    this.Camera.ViewHeight += scrollViewChange;
                }

                this._previousScrollWheelValue = mouseState.ScrollWheelValue;
            }

            if (this._editorGame.IsFocused) {
                var movementMultiplier = (float)gameTime.ElapsedGameTime.TotalSeconds * this.Camera.ViewHeight;

                if (keyboardState.IsKeyDown(Keys.W)) {
                    this.Camera.LocalPosition += new Vector2(0f, movementMultiplier);
                }

                if (keyboardState.IsKeyDown(Keys.A)) {
                    this.Camera.LocalPosition += new Vector2(movementMultiplier * -1f, 0f);
                }

                if (keyboardState.IsKeyDown(Keys.S)) {
                    this.Camera.LocalPosition += new Vector2(0f, movementMultiplier * -1f);
                }

                if (keyboardState.IsKeyDown(Keys.D)) {
                    this.Camera.LocalPosition += new Vector2(movementMultiplier, 0f);
                }
            }
        }

        private void Camera_ViewHeightChanged(object sender, System.EventArgs e) {
            if (this._primaryGridDrawer != null) {
                var gridSize = this.GetGridSize();
                this._primaryGridDrawer.ColumnWidth = gridSize;
                this._primaryGridDrawer.RowHeight = gridSize;

                var smallGridSize = gridSize / 2f;
                this._secondaryGridDrawer.ColumnWidth = smallGridSize;
                this._secondaryGridDrawer.RowHeight = smallGridSize;
            }
        }

        private int GetGridSize() {
            var gridSize = 1;
            var currentMultiple = 4;
            while (currentMultiple < this.Camera.ViewHeight) {
                gridSize = currentMultiple / 4;
                currentMultiple = currentMultiple * 2;
            }

            return gridSize;
        }
    }
}