﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Amoeba.Properties;
using Library.Net.Amoeba;
using Library.Net;
using Library.Security;
using System.Collections.ObjectModel;

namespace Amoeba.Windows
{
    /// <summary>
    /// UploadWindow.xaml の相互作用ロジック
    /// </summary>
    partial class UploadListWindow : Window
    {
        private AmoebaManager _amoebaManager;

        private ObservableCollection<UploadListViewItem> _filePaths = new ObservableCollection<UploadListViewItem>();
        private bool _isShare = false;

        public UploadListWindow(IEnumerable<string> filePaths, bool isShare, AmoebaManager amoebaManager)
        {
            _amoebaManager = amoebaManager;

            var list = filePaths.ToList();
            list.Sort();

            foreach (var item in list)
            {
                _filePaths.Add(new UploadListViewItem() { Path = item });
            }

            _isShare = isShare;

            var digitalSignatureCollection = new List<object>();
            digitalSignatureCollection.Add(new ComboBoxItem() { Content = "" });
            digitalSignatureCollection.AddRange(Settings.Instance.Global_DigitalSignatureCollection.Select(n => new DigitalSignatureComboBoxItem(n)).ToArray());

            InitializeComponent();

            using (FileStream stream = new FileStream(System.IO.Path.Combine(App.DirectoryPaths["Icons"], "Amoeba.ico"), FileMode.Open))
            {
                this.Icon = BitmapFrame.Create(stream);
            }

            if (Settings.Instance.Global_UploadKeywords.Count >= 1) _keywordsComboBox1.Text = Settings.Instance.Global_UploadKeywords[0];
            if (Settings.Instance.Global_UploadKeywords.Count >= 2) _keywordsComboBox2.Text = Settings.Instance.Global_UploadKeywords[1];
            if (Settings.Instance.Global_UploadKeywords.Count >= 3) _keywordsComboBox3.Text = Settings.Instance.Global_UploadKeywords[2];

            _keywordsComboBox1.Items.Add(new ComboBoxItem() { Content = "" });
            _keywordsComboBox2.Items.Add(new ComboBoxItem() { Content = "" });
            _keywordsComboBox3.Items.Add(new ComboBoxItem() { Content = "" });

            foreach (var item in Settings.Instance.Global_SearchKeywords) _keywordsComboBox1.Items.Add(new ComboBoxItem() { Content = item });
            foreach (var item in Settings.Instance.Global_SearchKeywords) _keywordsComboBox2.Items.Add(new ComboBoxItem() { Content = item });
            foreach (var item in Settings.Instance.Global_SearchKeywords) _keywordsComboBox3.Items.Add(new ComboBoxItem() { Content = item });

            _listView.ItemsSource = _filePaths;

            _signatureComboBox.ItemsSource = digitalSignatureCollection;

            var index = Settings.Instance.Global_DigitalSignatureCollection.IndexOf(Settings.Instance.Global_UploadDigitalSignature);
            _signatureComboBox.SelectedIndex = index + 1;
        }

        private void _listViewDeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var uploadItems = _listView.SelectedItems;
            if (uploadItems == null) return;

            foreach (var item in uploadItems.Cast<UploadListViewItem>().ToArray())
            {
                _filePaths.Remove(item);
            }
        }

        private void _okButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            var keywords = new KeywordCollection();
            if (!string.IsNullOrWhiteSpace(_keywordsComboBox1.Text)) keywords.Add(_keywordsComboBox1.Text);
            if (!string.IsNullOrWhiteSpace(_keywordsComboBox2.Text)) keywords.Add(_keywordsComboBox2.Text);
            if (!string.IsNullOrWhiteSpace(_keywordsComboBox3.Text)) keywords.Add(_keywordsComboBox3.Text);
            keywords = new KeywordCollection(new HashSet<string>(keywords));
            var digitalSignatureComboBoxItem = _signatureComboBox.SelectedItem as DigitalSignatureComboBoxItem;
            DigitalSignature digitalSignature = digitalSignatureComboBoxItem == null ? null : digitalSignatureComboBoxItem.Value;

            Settings.Instance.Global_UploadDigitalSignature = digitalSignature;

            if (!_isShare)
            {
                foreach (var item in _filePaths)
                {
                    _amoebaManager.Upload(item.Path,
                        item.Name,
                        keywords,
                        null,
                        digitalSignature,
                        3);
                }
            }
            else
            {
                foreach (var item in _filePaths)
                {
                    _amoebaManager.Share(item.Path,
                        item.Name,
                        keywords,
                        null,
                        digitalSignature,
                        3);
                }
            }

            Settings.Instance.Global_UploadKeywords.Clear();
            Settings.Instance.Global_UploadKeywords.AddRange(keywords);
        }

        private void _cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private class UploadListViewItem
        {
            private string _path = null;
            private string _name = null;

            public UploadListViewItem()
            {
            }

            public string Path
            {
                get
                {
                    return _path;
                }
                set
                {
                    _path = value;
                    _name = System.IO.Path.GetFileName(_path);
                }
            }

            public string Name
            {
                get
                {
                    return _name;
                }
            }
        }
    }
}
