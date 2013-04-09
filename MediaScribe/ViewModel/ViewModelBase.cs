using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using System.Windows.Media;
using JayDev.MediaScribe.View.Controls;
using System.Windows.Media.Animation;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using JayDev.MediaScribe.View;
using JayDev.MediaScribe.Common;
using JayDev.MediaScribe.Core;

namespace JayDev.MediaScribe.ViewModel
{
    public class ViewModelBase : GalaSoft.MvvmLight.ViewModelBase
    {
        private RelayCommand _showAboutDialog;
        protected Dispatcher _uiDispatcher;
        protected IUnityContainer _container;
        protected IController _controller;

        /// <summary>
        /// Gets the ShowAboutDialog.
        /// </summary>
        public RelayCommand ShowAboutDialog
        {
            get
            {
                return _showAboutDialog
                    ?? (_showAboutDialog = new RelayCommand(
                                          () =>
                                          {
                                              ////TODO: no windows in viewmodel
                                              About about = new About()
                                              {
                                                  WindowStyle = WindowStyle.None,
                                                  AllowsTransparency = true,
                                                  Background = Brushes.Transparent,
                                                  ShowInTaskbar = false,
                                                  ResizeMode = ResizeMode.NoResize,
                                                  WindowStartupLocation = WindowStartupLocation.CenterOwner,
                                                  Owner = System.Windows.Application.Current.MainWindow
                                              };

                                              DoubleAnimation animFadeIn = new DoubleAnimation();
                                              animFadeIn.From = 0;
                                              animFadeIn.To = 1;
                                              animFadeIn.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                                              about.BeginAnimation(Window.OpacityProperty, animFadeIn);
                                              about.ShowDialog();
                                          }));
            }
        }

        public ViewModelBase(UnityContainer unityContainer) : base()
        {
            _uiDispatcher = Dispatcher.CurrentDispatcher;
            if (null != unityContainer)
            {
                _container = unityContainer;
                _controller = _container.Resolve<IController>();
            }
        }

        public virtual void HandleWindowKeypress(object sender, System.Windows.Input.KeyEventArgs e)
        {
        }

        public virtual void EnteringViewModel() { }

        public virtual void LeavingViewModel() { }

        internal void ExportCourseToExcel(Course selectedCourse)
        {
            //TODO: refactor so we don't use dialogs in viewmodels
            Microsoft.Win32.SaveFileDialog saveFileDialog1 = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.DefaultExt = "xslx";
            // Adds a extension if the user does not
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.InitialDirectory = Convert.ToString(Environment.SpecialFolder.MyDocuments);
            saveFileDialog1.Filter = "Excel Spreadsheet|*.xlsx";
            saveFileDialog1.FileName = string.Format("Exported Notes for {0} - {1}.xlsx", selectedCourse.Name, DateTime.Now.ToString("dd-MM-yyyy HH.mm.ss"));
            saveFileDialog1.Title = "Save Exported Notes";

            if (saveFileDialog1.ShowDialog() == true)
            {
                try
                {
                    using (System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile())
                    {
                        XslsExporter exporter = new XslsExporter();
                        exporter.CreateSpreadsheet(fs, selectedCourse.Tracks.ToList(), selectedCourse.Notes.ToList());

                        fs.Close();
                    }

                    var openResult = System.Windows.MessageBox.Show(System.Windows.Application.Current.MainWindow, "Export successful! Would you like to open the file?", "Open exported file confirmation", System.Windows.MessageBoxButton.YesNo);
                    if (openResult == System.Windows.MessageBoxResult.Yes)
                    {
                        System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
                        info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
                        info.FileName = saveFileDialog1.FileName;
                        var process = System.Diagnostics.Process.Start(info);
                    }
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(System.Windows.Application.Current.MainWindow, "Error exporting :( - " + e.ToString());
                }
            }
        }
    }
}
