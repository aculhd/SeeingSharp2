﻿/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/

using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace SeeingSharp.UwpSamples.Controls
{
    public sealed partial class PropertyGrid : UserControl
    {
        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register(nameof(SelectedObject), typeof(object), typeof(PropertyGrid), new PropertyMetadata(null, OnSelectedObjectChanged));

        private PropertyGridViewModel _propertyGridVM;

        public object SelectedObject
        {
            get => this.GetValue(SelectedObjectProperty);
            set => this.SetValue(SelectedObjectProperty, value);
        }

        public PropertyGrid()
        {
            this.InitializeComponent();

            _propertyGridVM = new PropertyGridViewModel();
            GridMain.DataContext = _propertyGridVM;
        }

        private static void OnSelectedObjectChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eArgs)
        {
            if (!(sender is PropertyGrid propGrid)) { return; }

            propGrid._propertyGridVM.SelectedObject = eArgs.NewValue;
            propGrid.UpdatePropertiesView();
        }

        private void UpdatePropertiesView()
        {
            GridMain.Children.Clear();

            var lstProperties = new List<ConfigurablePropertyMetadata>(_propertyGridVM.PropertyMetadata);
            lstProperties.Sort((left, right) => left.CategoryName.CompareTo(right.CategoryName));
            var lstPropertyCategories = lstProperties
                .Select(actProperty => actProperty.CategoryName)
                .Distinct()
                .ToList();

            // Define rows
            GridMain.RowDefinitions.Clear();
            var rowCount = lstProperties.Count + lstPropertyCategories.Count;
            for (var loop = 0; loop < rowCount; loop++)
            {
                GridMain.RowDefinitions.Add(new RowDefinition { Height = new GridLength(45.0) });
            }
            this.Height = rowCount * 45d;

            // Create all controls
            var actRowIndex = 0;
            var actCategory = string.Empty;
            foreach (var actProperty in _propertyGridVM.PropertyMetadata)
            {
                if (actProperty.CategoryName != actCategory)
                {
                    actCategory = actProperty.CategoryName;

                    var txtHeader = new TextBlock
                    {
                        Text = actCategory
                    };

                    txtHeader.SetValue(Grid.RowProperty, (double)actRowIndex);
                    txtHeader.SetValue(Grid.ColumnSpanProperty, 2d);
                    txtHeader.SetValue(Grid.ColumnProperty, 0d);
                    txtHeader.Margin = new Thickness(5d, 5d, 5d, 5d);
                    txtHeader.VerticalAlignment = VerticalAlignment.Bottom;
                    txtHeader.FontWeight = FontWeights.Bold;
                    GridMain.Children.Add(txtHeader);

                    var rect = new Windows.UI.Xaml.Shapes.Rectangle
                    {
                        Height = 2d,
                        Fill = new SolidColorBrush(Colors.Black),
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(5d, 5d, 5d, 0d)
                    };

                    rect.SetValue(Grid.RowProperty, (double)actRowIndex);
                    rect.SetValue(Grid.ColumnSpanProperty, 2d);
                    rect.SetValue(Grid.ColumnProperty, 0d);
                    GridMain.Children.Add(rect);

                    actRowIndex++;
                }

                var ctrlText = new TextBlock
                {
                    Text = actProperty.PropertyDisplayName
                };

                ctrlText.SetValue(Grid.RowProperty, (double)actRowIndex);
                ctrlText.SetValue(Grid.ColumnProperty, 0d);
                ctrlText.Margin = new Thickness(5d, 5d, 50d, 5d);
                ctrlText.VerticalAlignment = VerticalAlignment.Center;
                GridMain.Children.Add(ctrlText);

                FrameworkElement ctrlValueEdit = null;

                switch (actProperty.ValueType)
                {
                    case PropertyValueType.Bool:
                        var ctrlCheckBox = new CheckBox();

                        ctrlCheckBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding
                        {
                            Path = new PropertyPath(nameof(actProperty.ValueAccessor)),
                            Mode = BindingMode.TwoWay,
                            Source = actProperty,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        });

                        ctrlValueEdit = ctrlCheckBox;
                        break;

                    case PropertyValueType.String:
                        var ctrlTextBox = new TextBox();
                        ctrlTextBox.SetBinding(TextBox.TextProperty, new Binding
                        {
                            Path = new PropertyPath(nameof(actProperty.ValueAccessor)),
                            Mode = BindingMode.TwoWay,
                            Source = actProperty,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        });
                        ctrlTextBox.Width = 200d;
                        ctrlValueEdit = ctrlTextBox;
                        break;

                    case PropertyValueType.Enum:
                        var ctrlComboBox = new ComboBox();
                        ctrlComboBox.ItemsSource = actProperty.GetEnumMembers();
                        ctrlComboBox.SetBinding(Selector.SelectedItemProperty, new Binding
                        {
                            Path = new PropertyPath(nameof(actProperty.ValueAccessor)),
                            Mode = BindingMode.TwoWay,
                            Source = actProperty,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        });
                        ctrlComboBox.Width = 200d;
                        ctrlValueEdit = ctrlComboBox;
                        break;
                }

                if (ctrlValueEdit != null)
                {
                    ctrlValueEdit.Margin = new Thickness(0d, 0d, 5d, 0d);
                    ctrlValueEdit.VerticalAlignment = VerticalAlignment.Center;
                    ctrlValueEdit.SetValue(Grid.RowProperty, (double)actRowIndex);
                    ctrlValueEdit.SetValue(Grid.ColumnProperty, 1d);
                    GridMain.Children.Add(ctrlValueEdit);
                }

                actRowIndex++;
            }
        }
    }
}
