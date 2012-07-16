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
using GalaSoft.MvvmLight.Command;
using System.Diagnostics;
using System.Timers;

using JayDev.Notemaker.Common;
using System.ComponentModel;
using GalaSoft.MvvmLight.Messaging;

namespace JayDev.Notemaker.View.Controls
{
    /// <summary>
    /// Interaction logic for VideoControl.xaml
    /// </summary>
    public partial class VideoControl : UserControl
    {
        public double? AspectRatio
        {
            get { return (double?)GetValue(AspectRatioProperty); }
            set { SetValue(AspectRatioProperty, value); }
        }
        // Using a DependencyProperty as the backing store for AspectRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AspectRatioProperty =
            DependencyProperty.Register("AspectRatio", typeof(double?), typeof(VideoControl), new UIPropertyMetadata(null));

        




        public delegate void DoubleClickHandler();
        public event DoubleClickHandler OnDoubleClick;

        private const double defaultAspectRatio = 1.7777d;//16:9



        public IntPtr VideoPanelPointer
        {
            get { return (IntPtr)GetValue(VideoPanelPointerProperty); }
            set { SetValue(VideoPanelPointerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VideoPanelPointer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VideoPanelPointerProperty =
            DependencyProperty.Register("VideoPanelPointer", typeof(IntPtr), typeof(VideoControl), new UIPropertyMetadata(null));

        


        public bool IsShowingVideo
        {
            get { return (bool)GetValue(IsShowingVideoProperty); }
            set { SetValue(IsShowingVideoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsShowingVideo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShowingVideoProperty =
            DependencyProperty.Register("IsShowingVideo", typeof(bool), typeof(VideoControl), new UIPropertyMetadata(true));




        #region PlayPauseCommand

        public static readonly DependencyProperty PlayPauseCommandqProperty =
            DependencyProperty.Register("PlayPauseCommandq", typeof(ICommand), typeof(VideoControl));

        /// <summary>
        /// Gets the PlayCommand.
        /// </summary>
        public ICommand PlayPauseCommandq
        {
            get { return (ICommand)GetValue(PlayPauseCommandqProperty); }
            set { SetValue(PlayPauseCommandqProperty, value); }
        }

        #endregion


        public VideoControl()
        {
            InitializeComponent();
           

            //this.play.VideoExited += new MplayerEventHandler(play_VideoExited);

            //// Set fullscreen
            //if (fullScreen == true && (this.WindowState != FormWindowState.Maximized))
            //{
            //    this.ToggleFormFullScreen();
            //}

            //// start playing mmediately
            //if (playNow == true && this.filePath != "")
            //{
            //    btnPlay_Click(new object(), new EventArgs());
            //}

            var mPlayerWPFControl1 = MediaPlayerWPFDisplayControl.Instance;
            this.videoPlaceholder.Content = mPlayerWPFControl1;
            mPlayerWPFControl1.OnDoubleClick += new MediaPlayerWPFDisplayControl.DoubleClickHandler(mPlayerWPFControl1_OnDoubleClick);

            //Ensure that when the video panel pointer property of the native win32 control is updated, we update our reference to it.
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(MediaPlayerWPFDisplayControl.VideoPanelHandlePropertyProperty, typeof(MediaPlayerWPFDisplayControl));
            if (dpd != null)
            {
                dpd.AddValueChanged(mPlayerWPFControl1, delegate
                {
                    VideoPanelPointer = mPlayerWPFControl1.VideoPanelHandleProperty;
                });
            }
        }


        void mPlayerWPFControl1_OnDoubleClick(MediaPlayerWPFDisplayControl.DoubleClickEventArgs args)
        {

            Messenger.Default.Send(NavigateMessage.ToggleFullscreen, MessageType.Navigate);
            //PlayPauseCommandq.Execute(null);
        }


        public enum VideoControlMode { Embedded, Collapsed, Fullscreen }

        public void SetMode(VideoControlMode mode)
        {
            if (mode == VideoControlMode.Embedded)
            {
                ControlPanel.Visibility = System.Windows.Visibility.Visible;
            }
            else if (mode == VideoControlMode.Fullscreen)
            {
                ControlPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetPanelSizeForAspectRatio(e.NewSize.Width, e.NewSize.Height);
        }

        private void SetPanelSizeForAspectRatio()
        {
            SetPanelSizeForAspectRatio(this.ActualWidth, this.ActualHeight);
        }
        private void SetPanelSizeForAspectRatio(double newWidth, double newHeight)
        {
                double aspectRatio = AspectRatio ?? defaultAspectRatio;
                //this.play.ForceAspectRatio(this.panelVideo.Width, this.panelVideo.Height);
                double width = newWidth;
                double height = newHeight - this.ControlPanel.ActualHeight;
                double correctedWidth = -1;
                double correctedHeight = -1;
                float panelAspectRatio = (float)width / (float)height;
                if (panelAspectRatio >= aspectRatio)
                {
                    correctedHeight = height;
                    correctedWidth = Convert.ToInt32((float)height * aspectRatio);
                    MediaPlayerWPFDisplayControl.Instance.Width = correctedWidth;
                    MediaPlayerWPFDisplayControl.Instance.Height = correctedHeight;
                }
                else
                {
                    correctedWidth = width;
                    correctedHeight = Convert.ToInt32((float)width / aspectRatio);
                    MediaPlayerWPFDisplayControl.Instance.Height = correctedHeight;
                    MediaPlayerWPFDisplayControl.Instance.Width = correctedWidth;
                }
        }


        //private void play_VideoExited(object sender, MplayerEvent e)
        //{
        //    timer1.Stop();
        //    this.ResetTime();
        //}



        //private void btnPlay_Click(object sender, EventArgs e)
        //{

        //    if (this.filePath == String.Empty || this.filePath == null)
        //    {

        //        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        //        {
        //            this.filePath = openFileDialog1.FileName;
        //        }
        //        else
        //        {
        //            MessageBox.Show("You must select a video file.", "Select a file", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //            return;
        //        }
        //    }

        //    _videoSettings = new Discover(this.filePath);

        //    this.play.Play(this.filePath);

        //    Discover file = new Discover(this.filePath);
        //    lblVideoLength.Text = TimeConversion.ConvertTimeHHMMSS(file.Length);

        //    this.currentTime = play.CurrentPosition();

        //    timer1.Start();

        //}

        //private void btnStop_Click(object sender, EventArgs e)
        //{
        //    if (this.play != null)
        //    {
        //        this.play.Stop();

        //        this.ResetTime();
        //        timer1.Stop();

        //    }
        //}


        //private void btnLoadFile_Click(object sender, EventArgs e)
        //{
        //    if (openFileDialog1.ShowDialog() == DialogResult.OK)
        //    {
        //        this.filePath = openFileDialog1.FileName;
        //    }
        //}

        //private void timer1_Tick(object sender, EventArgs e)
        //{

        //    this.SetTime(1);


        //    float videoLength = (float)this.play.CurrentPlayingFileLength();
        //    if (videoLength == 0f)
        //    {
        //        return;
        //    }

        //    int percent = (int)(((float)this.currentTime / videoLength) * 100);

        //    if (percent >= 100)
        //    {
        //        percent = 100;
        //        timer1.Stop();
        //    }

        //    if (this.trackBarMousePushedDown == false)
        //    {
        //        trackBar1.Value = percent;
        //    }

        //}


        //private void SetTime(int timeInSecondsAdded)
        //{
        //    this.Invoke((MethodInvoker)delegate
        //    {
        //        this.currentTime += timeInSecondsAdded;
        //        lblVideoPosition.Text = TimeConversion.ConvertTimeHHMMSS(this.currentTime);
        //    });


        //}

        //private void ResetTime()
        //{
        //    this.Invoke((MethodInvoker)delegate
        //    {
        //        this.currentTime = 0;
        //        lblVideoPosition.Text = TimeConversion.ConvertTimeHHMMSS(this.currentTime);
        //        trackBar1.Value = 0;
        //    });
        //}

        //private void btnPause_Click(object sender, EventArgs e)
        //{
        //    this.play.Pause();

        //    if (timer1.Enabled)
        //    {
        //        timer1.Stop();
        //    }
        //    else
        //    {
        //        timer1.Start();
        //    }

        //}

        //private void btnFastforward_Click(object sender, EventArgs e)
        //{
        //    this.play.Seek(60, Seek.Relative);
        //    this.SetTime(60);
        //}

        //private void btnRewind_Click(object sender, EventArgs e)
        //{
        //    this.play.Seek(-60, Seek.Relative);
        //    this.SetTime(-60);
        //}

        //private void trackBar1_ValueChanged(object sender, EventArgs e)
        //{

        //}

        //private void trackBar1_MouseDown(object sender, MouseEventArgs e)
        //{
        //    trackBarMousePushedDown = true;
        //}

        //private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        //{

        //    int length = this.play.CurrentPlayingFileLength();
        //    if (length == 0)
        //    {
        //        return;
        //    }

        //    int percentNew = trackBar1.Value;
        //    int newPositionInSeconds = (int)(((float)percentNew / 100.0f) * (float)length);
        //    int changeInSeconds = newPositionInSeconds - this.currentTime;

        //    this.play.Seek(changeInSeconds, Seek.Relative);
        //    this.SetTime(changeInSeconds);

        //    trackBarMousePushedDown = false;
        //}

        //private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        //{

        //    if ((e.KeyChar.ToString().ToLower() == Keys.F.ToString().ToLower()) || (e.KeyChar == (char)Keys.F11))
        //    {
        //        this.ToggleFormFullScreen();
        //        //this.play.ToggleFullScreen(); // it is already toggled to fullscreen on the control it is attached to.  
        //    }
        //}


        //private System.Windows.Forms.FormBorderStyle _border = FormBorderStyle.Sizable;
        //private FormWindowState _windowstate = FormWindowState.Normal;
        //private void ToggleFormFullScreen()
        //{
        //    if (this.WindowState == FormWindowState.Maximized)
        //    {
        //        this.FormBorderStyle = this._border;
        //        this.WindowState = this._windowstate;
        //        this.panel1.Show();
        //    }
        //    else
        //    {
        //        this._border = this.FormBorderStyle;
        //        this._windowstate = this.WindowState;

        //        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        //        this.WindowState = FormWindowState.Maximized;

        //        this.panel1.Hide();
        //    }


        //}

        //private void panelVideo_DoubleClick(object sender, EventArgs e)
        //{
        //    this.ToggleFormFullScreen();
        //}



        //protected override bool ProcessKeyPreview(ref System.Windows.Forms.Message m)
        //{
        //    switch (m.WParam.ToInt32())
        //    {
        //        // TODO: Fix so move commands are sent to correct carousel
        //        case (int)Keys.Right:

        //            this.play.Seek(10, Seek.Relative);
        //            this.SetTime(10);
        //            break;
        //        case (int)Keys.Left:
        //            this.play.Seek(-10, Seek.Relative);
        //            this.SetTime(-10);
        //            break;
        //        case (int)Keys.Up:
        //            this.play.Seek(60, Seek.Relative);
        //            this.SetTime(60);
        //            break;
        //        case (int)Keys.Down:
        //            this.play.Seek(-60, Seek.Relative);
        //            this.SetTime(-60);
        //            break;
        //    }

        //    return false;
        //}

        //private void MainForm_Resize(object sender, EventArgs e)
        //{
        //    //this.play.ForceAspectRatio(this.panelVideo.Width, this.panelVideo.Height);
        //    int width = this.Width;
        //    int height = this.Height - this.panel1.Height;
        //    int correctedWidth = -1;
        //    int correctedHeight = -1;
        //    float panelAspectRatio = (float)width / (float)height;
        //    if (panelAspectRatio >= this.play.AspectRatio)
        //    {
        //        correctedHeight = height;
        //        correctedWidth = Convert.ToInt32((float)height * this.play.AspectRatio);
        //        this.panelVideo.Width = correctedWidth;
        //    }
        //    else
        //    {
        //        correctedWidth = width;
        //        correctedHeight = Convert.ToInt32((float)width / this.play.AspectRatio);
        //        this.panelVideo.Height = correctedHeight;
        //    }
        //}


    }
}
