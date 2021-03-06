﻿namespace Macabre2D.UI.Models {

    using Macabre2D.Framework.Serialization;
    using Macabre2D.UI.Common;
    using System.IO;

    public class MetadataAsset : Asset {

        public MetadataAsset(string fileName) : base(fileName) {
            this.PropertyChanged += this.Self_PropertyChanged;
        }

        public MetadataAsset() : this(string.Empty) {
        }

        public bool HasChanges {
            get;
            internal set;
        }

        public string MetadataFileName {
            get {
                return $"{this.Name}{FileHelper.MetaDataExtension}";
            }
        }

        public static string GetMetadataPath(string assetPath) {
            return $"{assetPath}{FileHelper.MetaDataExtension}";
        }

        public override void Delete() {
            File.Delete(this.GetPath());
            File.Delete(this.GetMetadataPath());
            this.RaiseOnDeleted();
        }

        public string GetMetadataPath() {
            return MetadataAsset.GetMetadataPath(this.GetPath());
        }

        public void Save(Serializer serializer) {
            if (this.HasChanges) {
                try {
                    this.SaveChanges(serializer);
                    serializer.Serialize(this, this.GetMetadataPath());
                }
                finally {
                    this.HasChanges = false;
                }
            }
        }

        protected virtual void SaveChanges(Serializer serializer) {
            return;
        }

        private void Self_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            this.HasChanges = true;
        }
    }
}