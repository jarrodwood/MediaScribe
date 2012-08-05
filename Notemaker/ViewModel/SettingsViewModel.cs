using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JayDev.Notemaker.Model;
using GalaSoft.MvvmLight.Command;
using JayDev.Notemaker.Common;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;
using LibMPlayerCommon;
using System.Windows.Threading;

namespace JayDev.Notemaker.ViewModel
{
    public class SettingsViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        #region Private Members

        private CourseRepository _repo;
        private Dispatcher _uiDispatcher;

        #endregion

        #region Commands

        #region NavigateCommand

        private RelayCommand<NavigateMessage> _navigateCommand;
        public RelayCommand<NavigateMessage> NavigateCommand
        {
            get
            {
                return _navigateCommand
                    ?? (_navigateCommand = new RelayCommand<NavigateMessage>(
                                          (NavigateMessage message) =>
                                          {
                                              //JDW: if we're navigating from the list view, we have no context course.
                                              Messenger.Default.Send(new NavigateArgs(message), MessageType.Navigate);
                                          }));
            }
        }

        #endregion

        #endregion

        #region Notified Properties

        #region AreCoursesExisting

        //todo: make dynamic.
        public bool AreSettingsExisting { get { return true; } }

        #endregion

        public NavigateMessage CurrentPage
        {
            get
            {
                return NavigateMessage.Settings;
            }
        }

        #endregion

        #region Constructor

        public SettingsViewModel(CourseRepository repo)
        {
            _repo = repo;
            _uiDispatcher = Dispatcher.CurrentDispatcher;
        }

        #endregion
    }
}
