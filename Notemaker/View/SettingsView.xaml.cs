using System;
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
using JayDev.Notemaker.ViewModel;
using JayDev.Notemaker.Common;

namespace JayDev.Notemaker.View
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
            bool areAllNumbersNumericChars = AreAllValidNumericChars(e.Text);
            bool isLengthOK = e.Text.Length <= 4;
            bool stopInput = (false == areAllNumbersNumericChars || false == isLengthOK);
            if (stopInput)
            {
                e.Handled = true;
            }
            base.OnPreviewTextInput(e);
        }


        /// <summary>
        /// Used for the 'Key' column, providing the user with a textbox that they can press a keyboard key into -- and we'll detect what
        /// key they pressed. Once we've detected the key, we have to STOP the change of contents of the textbox, by setting e.Handled=true.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (null != hotkeyGrid.SelectedItem)
            {
                Hotkey selected = (Hotkey)hotkeyGrid.SelectedItem;
                selected.Key = e.Key;
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

        private bool AreAllValidNumericChars(string str)
        {
            foreach (char c in str)
            {
                if (!Char.IsNumber(c)) return false;
            }

            return true;
        }
    }
}
