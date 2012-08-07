﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using System.Windows.Media;
using JayDev.Notemaker.View.Controls;
using System.Windows.Media.Animation;

namespace JayDev.Notemaker.ViewModel
{
    public class ViewModelBase : GalaSoft.MvvmLight.ViewModelBase
    {
        private RelayCommand _showAboutDialog;

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
                                                  Owner = Application.Current.MainWindow
                                              };

                                              DoubleAnimation animFadeIn = new DoubleAnimation();
                                              animFadeIn.From = 0;
                                              animFadeIn.To = 1;
                                              animFadeIn.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                                              about.BeginAnimation(Window.OpacityProperty, animFadeIn);
                                              about.ShowDialog();

                                              //darkwindow.Close();
                                          }));
            }
        }
    }
}
