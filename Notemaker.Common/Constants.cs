using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace JayDev.Notemaker.Common
{
    public enum PlayStatus { Stopped, Playing, Paused }

    public enum MaintenanceMode { View, Create, Edit, None }
    public enum MessageType { Navigate, RegisterReusableControl, HotkeyRegistration }
    public enum NavigateMessage { ToggleFullscreen, ListCourses, WriteCourseNotes, ReviewCourseNotes, Settings }

    public struct HotkeyMessage
    {
        public HotkeyFunction Function { get; set; }
        public Direction SeekDirection { get; set; }
        public int SeekSeconds { get; set; }
        public Color Colour { get; set; }
        public int Rating { get; set; }
    }

    #region Hotkey Enums

    public enum Direction { Back, Forward }
    public enum HotkeyFunction
    {
        [Information("Toggle fullscreen", "Toggle between fullscreen mode and normal course mode. You can also toggle fullscreen by double-clicking on the video itself.", "only on the course notes screen.")]
        ToggleFullscreen,
        [Information("Toggle pause", "Toggle pausing the video", "only on the course notes screen, and a track has been selected.")]
        TogglePause,
        //ToggleMute,
        //ChangeVolume,
        [Information("Seek", "Jumps the current play position either forward or back a given of seconds.", "only on the course notes screen.")]
        Seek,
        [Information("Set note rating", "Sets the selected note's (or notes') rating.", "only on the course notes screen.")]
        NoteRating,
        [Information("Set note start time", "Sets the selected note's (or notes') start time, to the current track's current position.", "only on the course notes screen.")]
        NoteSetStartTime,
        [Information("Note segment - bold", "You can either select a segment of text and hit this hotkey to style it bold, or you can hit the hotkey and any text you begin to type will be styled bold", "only when editing a note.")]
        NoteBold,
        [Information("Note segment - italic", "You can either select a segment of text and hit this hotkey to style it italic, or you can hit the hotkey and any text you begin to type will be styled italic", "only when editing a note.")]
        NoteItalic,
        [Information("Note segment - colour", "You can either select a segment of text and hit this hotkey to give it a specified colour, or you can hit the hotkey and any text you begin to type will be coloured", "only when editing a note.")]
        NoteColour,
        [Information("Create new note", "Begins a new note with the start time set to the current track's current position, and jumps the cursor straight to the note body so you can start typing instantly! NOTE: If you are currently editing a note, this will commit the changes first.", "only on the course notes screen.")]
        NoteEditBegin,
        [Information("Commit note edit", "Saves any changes you've made to the note that you're editing.", "only when editing a note.")]
        NoteEditCommit,
        [Information("Cancel note edit", "Cancels any changes you've made to the note that you're editing.", "only when editing a note.")]
        NoteEditCancel,
        //NoteSetEndTime,
        //NoteToggleEndTime,
    }

    #endregion

    public enum WM_Messages
    {
        WM_NULL = 0x0000, //0
        WM_CREATE = 0x0001, //1
        WM_DESTROY = 0x0002, //2
        WM_MOVE = 0x0003, //3
        WM_SIZE = 0x0005, //5
        WM_ACTIVATE = 0x0006, //6
        WM_SETFOCUS = 0x0007, //7
        WM_KILLFOCUS = 0x0008, //8
        WM_ENABLE = 0x000A, //10
        WM_SETREDRAW = 0x000B, //11
        WM_SETTEXT = 0x000C, //12
        WM_GETTEXT = 0x000D, //13
        WM_GETTEXTLENGTH = 0x000E, //14
        WM_PAINT = 0x000F, //15
        WM_CLOSE = 0x0010, //16
        WM_QUERYENDSESSION = 0x0011, //17
        WM_QUERYOPEN = 0x0013, //19
        WM_ENDSESSION = 0x0016, //22
        WM_QUIT = 0x0012, //18
        WM_ERASEBKGND = 0x0014, //20
        WM_SYSCOLORCHANGE = 0x0015, //21
        WM_SHOWWINDOW = 0x0018, //24
        WM_WININICHANGE = 0x001A, //26
        WM_DEVMODECHANGE = 0x001B, //27
        WM_ACTIVATEAPP = 0x001C, //28
        WM_FONTCHANGE = 0x001D, //29
        WM_TIMECHANGE = 0x001E, //30
        WM_CANCELMODE = 0x001F, //31
        WM_SETCURSOR = 0x0020, //32
        WM_MOUSEACTIVATE = 0x0021, //33
        WM_CHILDACTIVATE = 0x0022, //34
        WM_QUEUESYNC = 0x0023, //35
        WM_GETMINMAXINFO = 0x0024, //36
        WM_PAINTICON = 0x0026, //38
        WM_ICONERASEBKGND = 0x0027, //39
        WM_NEXTDLGCTL = 0x0028, //40
        WM_SPOOLERSTATUS = 0x002A, //42
        WM_DRAWITEM = 0x002B, //43
        WM_MEASUREITEM = 0x002C, //44
        WM_DELETEITEM = 0x002D, //45
        WM_VKEYTOITEM = 0x002E, //46
        WM_CHARTOITEM = 0x002F, //47
        WM_SETFONT = 0x0030, //48
        WM_GETFONT = 0x0031, //49
        WM_SETHOTKEY = 0x0032, //50
        WM_GETHOTKEY = 0x0033, //51
        WM_QUERYDRAGICON = 0x0037, //55
        WM_COMPAREITEM = 0x0039, //57
        WM_GETOBJECT = 0x003D, //61
        WM_COMPACTING = 0x0041, //65
        WM_COMMNOTIFY = 0x0044, //68 /* no longer suported */
        WM_WINDOWPOSCHANGING = 0x0046, //70
        WM_WINDOWPOSCHANGED = 0x0047, //71
        WM_POWER = 0x0048, //72
        WM_COPYDATA = 0x004A, //74
        WM_CANCELJOURNAL = 0x004B, //75
        WM_NOTIFY = 0x004E, //78
        WM_INPUTLANGCHANGEREQUEST = 0x0050, //80
        WM_INPUTLANGCHANGE = 0x0051, //81
        WM_TCARD = 0x0052, //82
        WM_HELP = 0x0053, //83
        WM_USERCHANGED = 0x0054, //84
        WM_NOTIFYFORMAT = 0x0055, //85
        WM_CONTEXTMENU = 0x007B, //123
        WM_STYLECHANGING = 0x007C, //124
        WM_STYLECHANGED = 0x007D, //125
        WM_DISPLAYCHANGE = 0x007E, //126
        WM_GETICON = 0x007F, //127
        WM_SETICON = 0x0080, //128
        WM_NCCREATE = 0x0081, //129
        WM_NCDESTROY = 0x0082, //130
        WM_NCCALCSIZE = 0x0083, //131
        WM_NCHITTEST = 0x0084, //132
        WM_NCPAINT = 0x0085, //133
        WM_NCACTIVATE = 0x0086, //134
        WM_GETDLGCODE = 0x0087, //135
        WM_SYNCPAINT = 0x0088, //136
        WM_NCMOUSEMOVE = 0x00A0, //160
        WM_NCLBUTTONDOWN = 0x00A1, //161
        WM_NCLBUTTONUP = 0x00A2, //162
        WM_NCLBUTTONDBLCLK = 0x00A3, //163
        WM_NCRBUTTONDOWN = 0x00A4, //164
        WM_NCRBUTTONUP = 0x00A5, //165
        WM_NCRBUTTONDBLCLK = 0x00A6, //166
        WM_NCMBUTTONDOWN = 0x00A7, //167
        WM_NCMBUTTONUP = 0x00A8, //168
        WM_NCMBUTTONDBLCLK = 0x00A9, //169
        WM_NCXBUTTONDOWN = 0x00AB, //171
        WM_NCXBUTTONUP = 0x00AC, //172
        WM_NCXBUTTONDBLCLK = 0x00AD, //173
        WM_INPUT = 0x00FF, //255
        WM_KEYFIRST = 0x0100, //256
        WM_KEYDOWN = 0x0100, //256
        WM_KEYUP = 0x0101, //257
        WM_CHAR = 0x0102, //258
        WM_DEADCHAR = 0x0103, //259
        WM_SYSKEYDOWN = 0x0104, //260
        WM_SYSKEYUP = 0x0105, //261
        WM_SYSCHAR = 0x0106, //262
        WM_SYSDEADCHAR = 0x0107, //263
        WM_UNICHAR = 0x0109, //265
        WM_KEYLAST = 0x0109, //265
        WM_KEYLASTBefore0x0501 = 0x0108, //264
        WM_IME_STARTCOMPOSITION = 0x010D, //269
        WM_IME_ENDCOMPOSITION = 0x010E, //270
        WM_IME_COMPOSITION = 0x010F, //271
        WM_IME_KEYLAST = 0x010F, //271
        WM_INITDIALOG = 0x0110, //272
        WM_COMMAND = 0x0111, //273
        WM_SYSCOMMAND = 0x0112, //274
        WM_TIMER = 0x0113, //275
        WM_HSCROLL = 0x0114, //276
        WM_VSCROLL = 0x0115, //277
        WM_INITMENU = 0x0116, //278
        WM_INITMENUPOPUP = 0x0117, //279
        WM_MENUSELECT = 0x011F, //287
        WM_MENUCHAR = 0x0120, //288
        WM_ENTERIDLE = 0x0121, //289
        WM_MENURBUTTONUP = 0x0122, //290
        WM_MENUDRAG = 0x0123, //291
        WM_MENUGETOBJECT = 0x0124, //292
        WM_UNINITMENUPOPUP = 0x0125, //293
        WM_MENUCOMMAND = 0x0126, //294
        WM_CHANGEUISTATE = 0x0127, //295
        WM_UPDATEUISTATE = 0x0128, //296
        WM_QUERYUISTATE = 0x0129, //297
        WM_CTLCOLORMSGBOX = 0x0132, //306
        WM_CTLCOLOREDIT = 0x0133, //307
        WM_CTLCOLORLISTBOX = 0x0134, //308
        WM_CTLCOLORBTN = 0x0135, //309
        WM_CTLCOLORDLG = 0x0136, //310
        WM_CTLCOLORSCROLLBAR = 0x0137, //311
        WM_CTLCOLORSTATIC = 0x0138, //312
        WM_MOUSEFIRST = 0x0200, //512
        WM_MOUSEMOVE = 0x0200, //512
        WM_LBUTTONDOWN = 0x0201, //513
        WM_LBUTTONUP = 0x0202, //514
        WM_LBUTTONDBLCLK = 0x0203, //515
        WM_RBUTTONDOWN = 0x0204, //516
        WM_RBUTTONUP = 0x0205, //517
        WM_RBUTTONDBLCLK = 0x0206, //518
        WM_MBUTTONDOWN = 0x0207, //519
        WM_MBUTTONUP = 0x0208, //520
        WM_MBUTTONDBLCLK = 0x0209, //521
        WM_MOUSEWHEEL = 0x020A, //522
        WM_XBUTTONDOWN = 0x020B, //523
        WM_XBUTTONUP = 0x020C, //524
        WM_XBUTTONDBLCLK = 0x020D, //525
        WM_MOUSELAST = 0x020D, //525
        WM_MOUSELASTBefore0x0500 = 0x020A, //522
        WM_MOUSELASTBefore0x0400 = 0x0209, //521
        WM_PARENTNOTIFY = 0x0210, //528
        WM_ENTERMENULOOP = 0x0211, //529
        WM_EXITMENULOOP = 0x0212, //530
        WM_NEXTMENU = 0x0213, //531
        WM_SIZING = 0x0214, //532
        WM_CAPTURECHANGED = 0x0215, //533
        WM_MOVING = 0x0216, //534
        WM_POWERBROADCAST = 0x0218, //536
        WM_DEVICECHANGE = 0x0219, //537
        WM_MDICREATE = 0x0220, //544
        WM_MDIDESTROY = 0x0221, //545
        WM_MDIACTIVATE = 0x0222, //546
        WM_MDIRESTORE = 0x0223, //547
        WM_MDINEXT = 0x0224, //548
        WM_MDIMAXIMIZE = 0x0225, //549
        WM_MDITILE = 0x0226, //550
        WM_MDICASCADE = 0x0227, //551
        WM_MDIICONARRANGE = 0x0228, //552
        WM_MDIGETACTIVE = 0x0229, //553
        WM_MDISETMENU = 0x0230, //560
        WM_ENTERSIZEMOVE = 0x0231, //561
        WM_EXITSIZEMOVE = 0x0232, //562
        WM_DROPFILES = 0x0233, //563
        WM_MDIREFRESHMENU = 0x0234, //564
        WM_IME_SETCONTEXT = 0x0281, //641
        WM_IME_NOTIFY = 0x0282, //642
        WM_IME_CONTROL = 0x0283, //643
        WM_IME_COMPOSITIONFULL = 0x0284, //644
        WM_IME_SELECT = 0x0285, //645
        WM_IME_CHAR = 0x0286, //646
        WM_IME_REQUEST = 0x0288, //648
        WM_IME_KEYDOWN = 0x0290, //656
        WM_IME_KEYUP = 0x0291, //657
        WM_MOUSEHOVER = 0x02A1, //673
        WM_MOUSELEAVE = 0x02A3, //675
        WM_NCMOUSEHOVER = 0x02A0, //672
        WM_NCMOUSELEAVE = 0x02A2, //674
        WM_WTSSESSION_CHANGE = 0x02B1, //689
        WM_TABLET_FIRST = 0x02c0, //704
        WM_TABLET_LAST = 0x02df, //735
        WM_CUT = 0x0300, //768
        WM_COPY = 0x0301, //769
        WM_PASTE = 0x0302, //770
        WM_CLEAR = 0x0303, //771
        WM_UNDO = 0x0304, //772
        WM_RENDERFORMAT = 0x0305, //773
        WM_RENDERALLFORMATS = 0x0306, //774
        WM_DESTROYCLIPBOARD = 0x0307, //775
        WM_DRAWCLIPBOARD = 0x0308, //776
        WM_PAINTCLIPBOARD = 0x0309, //777
        WM_VSCROLLCLIPBOARD = 0x030A, //778
        WM_SIZECLIPBOARD = 0x030B, //779
        WM_ASKCBFORMATNAME = 0x030C, //780
        WM_CHANGECBCHAIN = 0x030D, //781
        WM_HSCROLLCLIPBOARD = 0x030E, //782
        WM_QUERYNEWPALETTE = 0x030F, //783
        WM_PALETTEISCHANGING = 0x0310, //784
        WM_PALETTECHANGED = 0x0311, //785
        WM_HOTKEY = 0x0312, //786
        WM_PRINT = 0x0317, //791
        WM_PRINTCLIENT = 0x0318, //792
        WM_APPCOMMAND = 0x0319, //793
        WM_THEMECHANGED = 0x031A, //794
        WM_HANDHELDFIRST = 0x0358, //856
        WM_HANDHELDLAST = 0x035F, //863
        WM_AFXFIRST = 0x0360, //864
        WM_AFXLAST = 0x037F, //895
        WM_PENWINFIRST = 0x0380, //896
        WM_PENWINLAST = 0x038F, //911
        WM_APP = 0x8000, //32768
        WM_USER = 0x0400, //1024
        WM_HELP_Before0x0400 = 0x000c, //12
    }

    public static class Constants {

        /// <summary>
        /// used in WPF datagrid, when 'CanUserAddRows' is enabled. WPF adds a blank item to the bottom of the grid, of type
        /// MS.Internal.NamedObject... and the only way, it seems, to detect if a given row is of one of these types, is by calling
        /// .ToString() on it - which will return this string here.
        /// </summary>
        public const string NewItemPlaceholderName = "{NewItemPlaceholder}";

    }
}