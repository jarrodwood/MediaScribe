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
        private const string DEFAULT_TITLE = "MediaScribe";

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

        #region WindowTitle

        /// <summary>
        /// The <see cref="WindowTitle" /> property's name.
        /// </summary>
        public const string WindowTitlePropertyName = "WindowTitle";

        private string _windowTitle;

        /// <summary>
        /// Sets and gets the WindowTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WindowTitle
        {
            get
            {
                return _windowTitle;
            }

            set
            {
                if (_windowTitle == value)
                {
                    return;
                }

                _windowTitle = value;
                RaisePropertyChanged(WindowTitlePropertyName);
            }
        }

        #endregion WindowTitle

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
            Messenger.Default.Register<SetWindowTitleMessage>(this, HandleSetWindowTitleMessage);

            Messenger.Default.Send<SetWindowTitleMessage>(new SetWindowTitleMessage()
            {
                Mode = SetWindowTitleMode.ResetToDefaultTitle
            });
        }

        public void HandleNavigationPerformedMessage(NavigateMessage message)
        {
            //need to include togglefullscreen message, for when we return to the normal window from fullscreen
            IsWritingNotes = (message == NavigateMessage.WriteCourseNotes
                || message == NavigateMessage.ToggleFullscreen);
        }

        public void HandleSetWindowTitleMessage(SetWindowTitleMessage message)
        {
            switch (message.Mode)
            {
                case SetWindowTitleMode.AppendToDefaultTitle:
                    WindowTitle = string.Format("{0} - {1}", DEFAULT_TITLE, message.Text);
                    break;
                case SetWindowTitleMode.ReplaceTitle:
                    WindowTitle = message.Text;
                    break;
                case SetWindowTitleMode.ResetToDefaultTitle:
                    WindowTitle = DEFAULT_TITLE;
                    break;
                default:
                    throw new Exception("unknown SetWindowTitleMode: " + message.Mode);
            }
        }

        #endregion
    }
}
