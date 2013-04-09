using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using GalaSoft.MvvmLight.Messaging;
using JayDev.MediaScribe.Common;
using GalaSoft.MvvmLight.Command;
using JayDev.MediaScribe.Core;

namespace JayDev.MediaScribe.ViewModel
{
    public class WindowHeaderViewModel : ViewModelBase
    {
        #region Notified Properties

        /// <summary>
        /// The <see cref="IsWritingNotes" /> property's name.
        /// </summary>
        public const string IsWritingNotesPropertyName = "IsWritingNotes";

        private bool _isWritingNotes = false;

        /// <summary>
        /// Sets and gets the IsWritingNotes property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsWritingNotes
        {
            get
            {
                return _isWritingNotes;
            }

            set
            {
                if (_isWritingNotes == value)
                {
                    return;
                }

                _isWritingNotes = value;
                RaisePropertyChanged(IsWritingNotesPropertyName);
            }
        }

        #endregion

        #region Commands

        #region ExportExcelCommand

        private RelayCommand _exportExcelCommand;

        /// <summary>
        /// Gets the ExportExcelCommand.
        /// </summary>
        public RelayCommand ExportExcelCommand
        {
            get
            {
                return _exportExcelCommand
                    ?? (_exportExcelCommand = new RelayCommand(
                                          () =>
                                          {
                                              Messenger.Default.Send<string>(string.Empty, MessageType.SaveCourseAndExportToExcel);
                                          }));
            }
        }

        #endregion


        #region ToggleFullscreenCommand

        private RelayCommand _toggleFullscreenCommand;
        public RelayCommand ToggleFullscreenCommand
        {
            get
            {
                return _toggleFullscreenCommand
                    ?? (_toggleFullscreenCommand = new RelayCommand(
                                          () =>
                                          {
                                              Messenger.Default.Send(new NavigateArgs(NavigateMessage.ToggleFullscreen, TabChangeSource.Application), MessageType.PerformNavigation);
                                          }));
            }
        }

        #endregion

        #endregion

        #region Constructor

        public WindowHeaderViewModel(UnityContainer unityContainer)
            : base(unityContainer)
        {
            Messenger.Default.Register<NavigateMessage>(this, MessageType.NavigationPerformed, HandleNavigationPerformedMessage);
        }

        public void HandleNavigationPerformedMessage(NavigateMessage message)
        {
            //need to include togglefullscreen message, for when we return to the normal window from fullscreen
            IsWritingNotes = (message == NavigateMessage.WriteCourseNotes
                || message == NavigateMessage.ToggleFullscreen);
        }

        #endregion
    }
}
