﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Kexi.Common;
using Kexi.Common.MultiSelection;
using Kexi.Interfaces;
using Kexi.ItemProvider;
using Kexi.ViewModel.Item;
using Mono.Cecil;

namespace Kexi.ViewModel.Lister
{
    [Export(typeof(ReferenceLister))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ReferenceLister : BaseLister<ReferenceItem>, IHistorisationProvider
    {
        private SearchItemProvider _searchItemProvider;

        [ImportingConstructor]
        public ReferenceLister(Workspace workspace, Options options,
            CommandRepository commandRepository)
            : base(workspace, options, commandRepository)
        {
            Items = new ObservableCollection<ReferenceItem>();
            Title = PathName = ProtocolPrefix;
            PropertyChanged += ReferenceLister_PropertyChanged;

            History = new BrowsingHistory();
        }

        private FileItem Item { get; set; }

        public override ObservableCollection<Column> Columns => new ObservableCollection<Column>
        {
            new Column("", "Thumbnail", ColumnType.Image),
            new Column("Name", "DisplayName", ColumnType.Highlightable),
            new Column("Version", "Version"),
            new Column("Referenced by", "RelativePath")
        };

        public override string ProtocolPrefix => "References";

        public BrowsingHistory History { get; }

        private void ReferenceLister_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                Item = new FileItem(Path);
                History.Push(Path, Filter, GroupBy, SortHandler.CurrentSortDescription);
                PathName = Item.Name;
            }
        }

        public override async Task Refresh(bool clearFilterAndGroup = true)
        {
            Items.Clear();
            if (Item.IsContainer)
            {
                _searchItemProvider = new SearchItemProvider();
                _searchItemProvider.ItemAdded += SearchItemProvider_ItemAdded;
                await _searchItemProvider.GetItems(Path, ".dll");
            }
            else
            {
                foreach (var refItem in GetReferenceItems(Item.GetPathResolved()))
                    Items.Add(refItem);
            }

            ItemsView = new MultiSelectCollectionView<ReferenceItem>(Items);
            Title = PathName = ProtocolPrefix + "/" + Item.Name;
        }

        protected override Task<IEnumerable<ReferenceItem>> GetItems()
        {
            return null;
        }

        private void SearchItemProvider_ItemAdded(FileItem obj)
        {
            if (!Items.Any()) Workspace.FocusCurrentOrFirst();
            foreach (var i in GetReferenceItems(obj.GetPathResolved()))
                Items.Add(i);
        }

        private IEnumerable<ReferenceItem> GetReferenceItems(string path)
        {
            var rootPath = Item.IsContainer
                ? Item.GetPathResolved()
                : Item.Directory;

            var assembly = AssemblyDefinition.ReadAssembly(path);
            var baseName = assembly.Name.FullName;
            var main = assembly.MainModule;
            var refs = main.AssemblyReferences.Select(r => new ReferenceItem(path, rootPath, baseName, r));
            return refs;
        }

        public override async void DoAction(ReferenceItem item)
        {
            if (!string.IsNullOrEmpty(item?.TargetPath))
            {
                History.Push(Path, Filter, GroupBy, SortHandler.CurrentSortDescription);
                Path = item.TargetPath;
                await Refresh().ConfigureAwait(false);
            }
        }

        public override void Copy()
        {
            var selection = ItemsView.SelectedItems.Select(s => string.Join(",", s.DisplayName, s.Version, s.Assembly));
            var text = string.Join(Environment.NewLine, selection);
            Clipboard.SetText(text);
        }
    }
}