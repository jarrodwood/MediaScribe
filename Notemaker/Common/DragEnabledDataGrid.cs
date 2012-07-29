using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;

namespace JayDev.Notemaker.Common
{
    public class MoveItemsCommandParameter
    {
        public int InsertToIndex { get; set; }
        public List<object> ObjectsToInsert { get; set; }

        public MoveItemsCommandParameter(int insertToIndex, List<object> objectsToInsert)
        {
            this.InsertToIndex = insertToIndex;
            this.ObjectsToInsert = objectsToInsert;
        }
    }
    public class DragEnabledDataGrid : DataGrid
    {


        public List<object> ReadOnlySelectedItems
        {
            get { return (List<object>)GetValue(ReadOnlySelectedItemsProperty); }
            set { SetValue(ReadOnlySelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReadOnlySelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReadOnlySelectedItemsProperty =
            DependencyProperty.Register("ReadOnlySelectedItems", typeof(List<object>), typeof(DragEnabledDataGrid), new UIPropertyMetadata(null));

        

        public ICommand MoveItemsCommand
        {
            get { return (ICommand)GetValue(MoveItemsCommandProperty); }
            set { SetValue(MoveItemsCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MoveItemsCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MoveItemsCommandProperty =
            DependencyProperty.Register("MoveItemsCommand", typeof(ICommand), typeof(DragEnabledDataGrid), new UIPropertyMetadata(null));



        public string OnDragDisplayPropertyName
        {
            get { return (string)GetValue(OnDragDisplayPropertyNameProperty); }
            set { SetValue(OnDragDisplayPropertyNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OnDragDisplayPropertyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnDragDisplayPropertyNameProperty =
            DependencyProperty.Register("OnDragDisplayPropertyName", typeof(string), typeof(DragEnabledDataGrid), new UIPropertyMetadata(null));

        

        public DragEnabledDataGrid() : base() {
            
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(DragSource_PreviewMouseLeftButtonDown);
            this.PreviewMouseMove += new MouseEventHandler(DragSource_PreviewMouseMove);
            this.MouseUp += new MouseButtonEventHandler(DragSource_MouseUp);
            this.SelectionChanged += new SelectionChangedEventHandler(DragEnabledDataGrid_SelectionChanged);
        }

        void DragEnabledDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //JDW: we have to make sure 'currentitem' is not null, because if we DELETE entries in the grid, it will automatically set
            //'SelectedItems' to the last item in the list (but won't colour the row as being selected)... but it leaves CurrentItem null.
            if (null == CurrentItem)
            {
                SelectedItems.Clear();
                SetValue(ReadOnlySelectedItemsProperty, new List<object>());
            }
            else
            {
                List<object> selectedItemsList = new List<object>();
                foreach (var item in SelectedItems)
                {
                    selectedItemsList.Add(item);
                }
                SetValue(ReadOnlySelectedItemsProperty, selectedItemsList);
            }
        }
        
        void DragSource_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isClickingDownOnSelectedItem && false == IsDragging)
            {
                //JDW: if the user clicked down on an item, but DIDN'T drag... we need to select the item now.
                //     however, we can't use the datagrid's properties to select the item, since there's an internal flag recording the 'selected row anchor',
                //     which only gets cleared/reset when there's a click event in a grid cell -- not setting a selected row. this would mean if you try to do
                //     a shift-select, it will select from the OLD ANCHORED ROW, to the newly-selected row... instead of the currently-selected row to the newly-selected row.
                DataGridCell cell = UIHelpers.TryFindFromPoint<DataGridCell>((UIElement)this, e.GetPosition(this));
                MouseButtonEventArgs args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left);
                args.RoutedEvent = System.Windows.UIElement.MouseLeftButtonDownEvent;
                args.Source = sender;
                cell.RaiseEvent(args);
            }

            _isClickingDownOnSelectedItem = false;
        }

        void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isClickingDownOnSelectedItem && !IsDragging)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                     StartDragInProcAdorner(e); 
                }
            }
        }

        /// <summary>
        /// flags whether we're currently clicking down on a selected row. this is a prerequisite to dragging.
        /// </summary>
        bool _isClickingDownOnSelectedItem = false;
        void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DataGridRow row = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)this, e.GetPosition(this));
                if (row != null && SelectedItems.Contains(row.Item))
                {
                    e.Handled = true;
                    _isClickingDownOnSelectedItem = true;
                }
            }
        }

        Cursor _allOpsCursor = null;

        void DragSource_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("DragSource_GiveFeedback " + e.Effects.ToString());

            if (this.DragScope == null)
            {
                try
                {
                    //This loads the cursor from a stream .. 
                    if (_allOpsCursor == null)
                    {                        
                        using (Stream cursorStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SimplestDragDrop.DDIcon.cur"))
                        {
                            _allOpsCursor = new Cursor(cursorStream);
                        } 
                    }
                    Mouse.SetCursor(_allOpsCursor);

                    e.UseDefaultCursors = false;
                    e.Handled = true;
                }
                finally { }
            }
            else  // This code is called when we are using a custom cursor  (either a window or adorner ) 
            {
                 
                e.UseDefaultCursors = false;
                e.Handled = true;
            }
        }

        Adorner _adorner = null;
        AdornerLayer _layer;


        FrameworkElement _dragScope;
        public FrameworkElement DragScope
        {
            get { return _dragScope; }
            set { _dragScope = value; }
        }


        List<object> _draggedItems = null;
        private void StartDragInProcAdorner(MouseEventArgs e)
        {

            // Let's define our DragScope .. In this case it is every thing inside our main window .. 
            DragScope = Application.Current.MainWindow.Content as FrameworkElement;
            System.Diagnostics.Debug.Assert(DragScope != null);

            // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
            bool previousDrop = DragScope.AllowDrop;
            DragScope.AllowDrop = true;            

            // Let's wire our usual events.. 
            // GiveFeedback just tells it to use no standard cursors..  

            GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
            this.GiveFeedback += feedbackhandler;

            // Drag Leave is optional, but write up explains why I like it .. 
            DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
            DragScope.DragLeave += dragleavehandler;

            // QueryContinue Drag goes with drag leave... 
            QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
            DragScope.QueryContinueDrag += queryhandler;

            //Here we create our adorner.. 
            if(null == OnDragDisplayPropertyName) throw new Exception("OnDragDisplayPropertyName must have a value to enable drag'n'drop");
            List<string> dragDisplayNames = new List<string>();

            //iterate through the 'items' collection, and check to see if each item is contained in the list of selected items. we do this because 'selecteditems' ordering is sometimes quite random...
            List<object> selectedItemsOrdered = new List<object>();
            foreach (var item in Items)
            {
                if (SelectedItems.Contains(item))
                {
                    selectedItemsOrdered.Add(item);
                }
            }
            foreach (var item in selectedItemsOrdered)
            {
                dragDisplayNames.Add((string)item.GetType().GetProperty(OnDragDisplayPropertyName).GetValue(item, null));
            }
            _adorner = new TextListAdorner(DragScope, dragDisplayNames);
            DragScope.DragOver += new DragEventHandler(DragScope_DragOver);
            _layer = AdornerLayer.GetAdornerLayer(DragScope as Visual);
            _layer.Add(_adorner);

            //note the selected items in a collection, for use later...
            _draggedItems = new List<object>();
            foreach (var item in SelectedItems)
            {
                _draggedItems.Add(item);
            }

            IsDragging = true;
            _dragHasLeftScope = false; 
            //Finally lets drag drop 
            DataObject data = new DataObject(System.Windows.DataFormats.Text.ToString(), "abcd");
            DragDropEffects de = DragDrop.DoDragDrop(this, data, DragDropEffects.Move);

            
             // Clean up our mess :) -- this code is executed when the drag'n'drop operation is completed.
            DragScope.AllowDrop = previousDrop;
            AdornerLayer.GetAdornerLayer(DragScope).Remove(_adorner);
            _adorner = null;




            //if we're dropping the dragged items onto an item that isn't in the list of dragged items...
            if (SelectedItem == null || false == _draggedItems.Contains(SelectedItem))
            {
                var targetIndex = Items.IndexOf(SelectedItem);
                //execute the command, to allow the control using the datagrid to handle moving the items
                MoveItemsCommand.Execute(new MoveItemsCommandParameter(targetIndex, selectedItemsOrdered));
            }

            //select the dropped items
            SelectedItems.Clear();
            _draggedItems.ForEach(x => SelectedItems.Add(x));
            _draggedItems = null;

            this.GiveFeedback -= feedbackhandler;
            DragScope.DragLeave -= dragleavehandler;
            DragScope.QueryContinueDrag -= queryhandler;
            DragScope.DragOver -= DragScope_DragOver;

            IsDragging = false;
            _isClickingDownOnSelectedItem = false;
        }

        /// <summary>
        /// Used to track which grid row we're hovering over, while dragging. we will select this row, to give the user feedback on where the dragged
        /// rows will be inserted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DragScope_DragOver(object sender, DragEventArgs e)
        {
            //JDW: make sure the row under the grid is being selected, to give user feedback about where the rows will be inserted
            Point position = e.GetPosition(this);
            System.Diagnostics.Debug.WriteLine("DragSource_GiveFeedbackPos " + position.ToString());
            var row = UIHelpers.TryFindFromPoint<DataGridRow>(this, position);
            if (row != null) this.SelectedItem = row.Item;
        }

        private bool _dragHasLeftScope = false; 
        void DragScope_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (this._dragHasLeftScope)
            {
                e.Action = DragAction.Cancel;
                e.Handled = true;
            }
        }


        void DragScope_DragLeave(object sender, DragEventArgs e)
        {
            if (e.OriginalSource == DragScope)
            {
                Point p = e.GetPosition(DragScope);
                Rect r = VisualTreeHelper.GetContentBounds(DragScope);
                if (!r.Contains(p))
                {
                    this._dragHasLeftScope = true;
                    e.Handled = true;
                }
            }

        }




        private Point _startPoint;
        private bool _isDragging;

        public bool IsDragging
        {
            get { return _isDragging; }
            set { _isDragging = value; }
        } 

    }
}
