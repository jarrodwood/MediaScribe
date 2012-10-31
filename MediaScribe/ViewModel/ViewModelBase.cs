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
    }
}
