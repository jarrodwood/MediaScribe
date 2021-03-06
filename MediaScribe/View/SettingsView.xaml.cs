﻿using System;
using System.Collections.Generic;
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
using JayDev.MediaScribe.ViewModel;
using JayDev.MediaScribe.Common;
using System.Collections;

namespace JayDev.MediaScribe.View
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private SettingsViewModel _viewModel;


        public SettingsView(SettingsViewModel viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
            this._viewModel = viewModel;
        }

        #region Event Handlers

        /// <summary>
        /// Ensures that whatever the user types into the 'seconds' textbox, is only numeric, and not of an excessively large length.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            bool areAllNumbersNumericChars = Utility.AreAllValidNumericChars(e.Text);
            bool isLengthOK = textbox.Text.Length + e.Text.Length <= 4;
            bool stopInput = (false == areAllNumbersNumericChars || false == isLengthOK);
            if (stopInput)
            {
                e.Handled = true;
            }
            base.OnPreviewTextInput(e);
        }

        private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
        }

        /// <summary>
        /// Used for the 'Key' column, providing the user with a textbox that they can press a keyboard key into -- and we'll detect what
        /// key they pressed. Once we've detected the key, we have to STOP the change of contents of the textbox, by setting e.Handled=true.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DetermineKeyPress(e);
        }

        private void DetermineKeyPress(KeyEventArgs e)
        {
            if (null != hotkeyGrid.SelectedItem)
            {
                Hotkey selected = (Hotkey)hotkeyGrid.SelectedItem;

                selected.ModifierKey = Keyboard.Modifiers;
                //selected.ModifierKeys.Clear();
                //if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                //{
                //    selected.ModifierKeys.Add(ModifierKeys.Shift);
                //}
                //if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                //{
                //    selected.ModifierKeys.Add(ModifierKeys.Control);
                //}
                //if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                //{
                //    selected.ModifierKeys.Add(ModifierKeys.Alt);
                //}
                //if ((Keyboard.Modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
                //{
                //    selected.ModifierKeys.Add(ModifierKeys.Windows);
                //}

                switch (e.Key)
                {
                    //ignore the modifier keys
                    case Key.LeftAlt:
                    case Key.LeftCtrl:
                    case Key.LeftShift:
                    case Key.LWin:
                    case Key.RightAlt:
                    case Key.RightCtrl:
                    case Key.RightShift:
                    case Key.RWin:
                        break;
                    default:
                        selected.Key = e.Key;
                        break;
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// When the user changes the hotkey function, commit the edit... this is so that we can populate the 'parameter' column with the
        /// appropriate fields for the new hotkey function.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                if (((Hotkey)((ComboBox)sender).DataContext).Function != (HotkeyFunction)e.AddedItems[0])
                {
                    DataGrid parentGrid = UIHelpers.TryFindParent<DataGrid>((ComboBox)sender);
                    parentGrid.CommitEdit(DataGridEditingUnit.Row, true);
                }
            }
        }


        #endregion

        private void addHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            //commit any edit, so that it will pop up a new blank placeholder row down the bottom
            hotkeyGrid.CommitEdit();
            DataGridCell cell = GetCell(hotkeyGrid, hotkeyGrid.Items.Count-1, 0);
            if (cell != null)
            {
                hotkeyGrid.ScrollIntoView(hotkeyGrid.Items[hotkeyGrid.Items.Count - 1]);
                cell.Focus();
                hotkeyGrid.CurrentCell = new DataGridCellInfo(cell);
                hotkeyGrid.BeginEdit();
            }
        }


        DataGridCell GetCell(DataGrid dg, int rowIndex, int columnIndex)
        {
            var dr = dg.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            var dc = dg.Columns[columnIndex].GetCellContent(dr);
            return dc.Parent as DataGridCell;
        }


    }
}
