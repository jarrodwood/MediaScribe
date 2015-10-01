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
using JayDev.MediaScribe.View.Controls;

namespace JayDev.MediaScribe.Core
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

    public class DataGridExtensions
    {
        #region ShowRowNumbersInHeader

        // Using a DependencyProperty as the backing store for ShowRowNumbersInHeader.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowRowNumbersInHeaderProperty =
            DependencyProperty.Register("ShowRowNumbersInHeader", typeof(bool), typeof(DragEnabledDataGrid), new UIPropertyMetadata(OnShowRowNumbersInHeaderChanged));


        public static void SetShowRowNumbersInHeader(DependencyObject element, Boolean value)
        {
            element.SetValue(ShowRowNumbersInHeaderProperty, value);
        }
        public static Boolean GetShowRowNumbersInHeader(DependencyObject element)
        {
            return (Boolean)element.GetValue(ShowRowNumbersInHeaderProperty);
        }


        public static void OnShowRowNumbersInHeaderChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if ((bool)args.NewValue == true)
            {
                DataGrid grid = (DataGrid)obj;
                grid.LoadingRow += new EventHandler<DataGridRowEventArgs>((sender, e) =>
                {
                    // Adding 1 to make the row count start at 1 instead of 0
                    // as pointed out by daub815
                    e.Row.Header = (e.Row.GetIndex() + 1).ToString() + " ";
                });

            }
        }

        #endregion

        #region IsDragEnabled

        //TODO: make this work!


        //public static void SetCanDrag(DependencyObject element, Boolean value)
        //{
        //    element.SetValue(CanDragProperty, value);
        //}
        //public static Boolean GetCanDrag(DependencyObject element)
        //{
        //    return (Boolean)element.GetValue(CanDragProperty);
        //}

        //// Using a DependencyProperty as the backing store for CanDrag.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty CanDragProperty =
        //    DependencyProperty.Register("CanDrag", typeof(bool), typeof(DragEnabledDataGrid), new UIPropertyMetadata(OnCanDragChanged));




        //public static void SetReadOnlySelectedItems(DependencyObject element, List<object> value)
        //{
        //    element.SetValue(ReadOnlySelectedItemsProperty, value);
        //}
        //public static List<object> GetReadOnlySelectedItems(DependencyObject element)
        //{
        //    return (List<object>)element.GetValue(ReadOnlySelectedItemsProperty);
        //}

        //// Using a DependencyProperty as the backing store for ReadOnlySelectedItems.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ReadOnlySelectedItemsProperty =
        //    DependencyProperty.Register("ReadOnlySelectedItems", typeof(List<object>), typeof(DragEnabledDataGrid), new UIPropertyMetadata(null));



        //public static void SetMoveItemsCommand(DependencyObject element, ICommand value)
        //{
        //    element.SetValue(MoveItemsCommandProperty, value);
        //}
        //public static ICommand GetMoveItemsCommand(DependencyObject element)
        //{
        //    return (ICommand)element.GetValue(MoveItemsCommandProperty);
        //}

        //// Using a DependencyProperty as the backing store for MoveItemsCommand.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty MoveItemsCommandProperty =
        //    DependencyProperty.Register("MoveItemsCommand", typeof(ICommand), typeof(DragEnabledDataGrid), new UIPropertyMetadata(null));



        //public static void SetOnDragDisplayPropertyName(DependencyObject element, string value)
        //{
        //    element.SetValue(OnDragDisplayPropertyNameProperty, value);
        //}
        //public static string GetOnDragDisplayPropertyName(DependencyObject element)
        //{
        //    return (string)element.GetValue(OnDragDisplayPropertyNameProperty);
        //}

        //// Using a DependencyProperty as the backing store for OnDragDisplayPropertyName.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty OnDragDisplayPropertyNameProperty =
        //    DependencyProperty.Register("OnDragDisplayPropertyName", typeof(string), typeof(DragEnabledDataGrid), new UIPropertyMetadata(null));


        
        //public static void OnCanDragChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        //{
        //    if ((bool)args.NewValue == true)
        //    {
        //        DataGrid grid = (DataGrid)obj;
        //        DataGridExtensions extensions = new DataGridExtensions();
        //        grid.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(extensions.DragSource_PreviewMouseLeftButtonDown);
        //        grid.PreviewMouseMove += new MouseEventHandler(extensions.DragSource_PreviewMouseMove);
        //        grid.MouseUp += new MouseButtonEventHandler(extensions.DragSource_MouseUp);
        //        grid.SelectionChanged += new SelectionChangedEventHandler(extensions.DragEnabledDataGrid_SelectionChanged);

        //    }
        //}
        


        //void DragEnabledDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    DataGrid grid = sender as DataGrid;
        //    //JDW: we have to make sure 'currentitem' is not null, because if we DELETE entries in the grid, it will automatically set
        //    //'SelectedItems' to the last item in the list (but won't colour the row as being selected)... but it leaves CurrentItem null.
        //    if (null == grid.CurrentItem)
        //    {
        //        grid.SelectedItems.Clear();
        //        grid.SetValue(ReadOnlySelectedItemsProperty, new List<object>());
        //    }
        //    else
        //    {
        //        List<object> selectedItemsList = new List<object>();
        //        foreach (var item in grid.SelectedItems)
        //        {
        //            selectedItemsList.Add(item);
        //        }
        //        grid.SetValue(ReadOnlySelectedItemsProperty, selectedItemsList);
        //    }
        //}
        
        //void DragSource_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    DataGrid grid = sender as DataGrid;
        //    if (_isClickingDownOnSelectedItem && false == IsDragging)
        //    {
        //        //JDW: if the user clicked down on an item, but DIDN'T drag... we need to select the item now.
        //        //     however, we can't use the datagrid's properties to select the item, since there's an internal flag recording the 'selected row anchor',
        //        //     which only gets cleared/reset when there's a click event in a grid cell -- not setting a selected row. this would mean if you try to do
        //        //     a shift-select, it will select from the OLD ANCHORED ROW, to the newly-selected row... instead of the currently-selected row to the newly-selected row.
        //        DataGridCell cell = UIHelpers.TryFindFromPoint<DataGridCell>((UIElement)grid, e.GetPosition(grid));
        //        MouseButtonEventArgs args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left);
        //        args.RoutedEvent = System.Windows.UIElement.MouseLeftButtonDownEvent;
        //        args.Source = sender;
        //        cell.RaiseEvent(args);
        //    }

        //    _isClickingDownOnSelectedItem = false;
        //}

        //void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
        //{
        //    DataGrid grid = sender as DataGrid;
        //    //if (grid.CanDrag) TODO: fix this. currently we're assuming if we're executing this, the user wants this functionality.
        //    {
        //        if (_isClickingDownOnSelectedItem && !IsDragging)
        //        {
        //            Point position = e.GetPosition(null);

        //            if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
        //                Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
        //            {
        //                StartDragInProcAdorner(grid, e);
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// flags whether we're currently clicking down on a selected row. this is a prerequisite to dragging.
        ///// </summary>
        //bool _isClickingDownOnSelectedItem = false;
        //void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    _startPoint = e.GetPosition(null);

        //    if (e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        DataGrid grid = sender as DataGrid;
        //        DataGridRow row = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)grid, e.GetPosition(grid));
        //        if (row != null && grid.SelectedItems.Contains(row.Item))
        //        {
        //            e.Handled = true;
        //            _isClickingDownOnSelectedItem = true;
        //        }
        //    }
        //}

        //Cursor _allOpsCursor = null;

        //void DragSource_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("DragSource_GiveFeedback " + e.Effects.ToString());

        //    if (this.DragScope == null)
        //    {
        //        try
        //        {
        //            //This loads the cursor from a stream .. 
        //            if (_allOpsCursor == null)
        //            {                        
        //                using (Stream cursorStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SimplestDragDrop.DDIcon.cur"))
        //                {
        //                    _allOpsCursor = new Cursor(cursorStream);
        //                } 
        //            }
        //            Mouse.SetCursor(_allOpsCursor);

        //            e.UseDefaultCursors = false;
        //            e.Handled = true;
        //        }
        //        finally { }
        //    }
        //    else  // This code is called when we are using a custom cursor  (either a window or adorner ) 
        //    {
                 
        //        e.UseDefaultCursors = false;
        //        e.Handled = true;
        //    }
        //}

        //Adorner _adorner = null;
        //AdornerLayer _layer;


        //FrameworkElement _dragScope;
        //public FrameworkElement DragScope
        //{
        //    get { return _dragScope; }
        //    set { _dragScope = value; }
        //}


        //List<object> _draggedItems = null;
        //private void StartDragInProcAdorner(DataGrid grid, MouseEventArgs e)
        //{

        //    // Let's define our DragScope .. In this case it is every thing inside our main window .. 
        //    DragScope = Application.Current.MainWindow.Content as FrameworkElement;
        //    System.Diagnostics.Debug.Assert(DragScope != null);

        //    // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
        //    bool previousDrop = DragScope.AllowDrop;
        //    DragScope.AllowDrop = true;            

        //    // Let's wire our usual events.. 
        //    // GiveFeedback just tells it to use no standard cursors..  

        //    GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
        //    grid.GiveFeedback += feedbackhandler;

        //    // Drag Leave is optional, but write up explains why I like it .. 
        //    DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
        //    DragScope.DragLeave += dragleavehandler;

        //    // QueryContinue Drag goes with drag leave... 
        //    QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
        //    DragScope.QueryContinueDrag += queryhandler;

        //    //Here we create our adorner.. 
        //    if (null == grid.OnDragDisplayPropertyName) throw new Exception("OnDragDisplayPropertyName must have a value to enable drag'n'drop");
        //    List<string> dragDisplayNames = new List<string>();

        //    //iterate through the 'items' collection, and check to see if each item is contained in the list of selected items. we do this because 'selecteditems' ordering is sometimes quite random...
        //    List<object> selectedItemsOrdered = new List<object>();
        //    foreach (var item in grid.Items)
        //    {
        //        if (grid.SelectedItems.Contains(item))
        //        {
        //            selectedItemsOrdered.Add(item);
        //        }
        //    }
        //    foreach (var item in selectedItemsOrdered)
        //    {
        //        dragDisplayNames.Add((string)item.GetType().GetProperty(grid.OnDragDisplayPropertyName).GetValue(item, null));
        //    }
        //    _adorner = new TextListAdorner(DragScope, dragDisplayNames);
        //    DragScope.DragOver += new DragEventHandler(DragScope_DragOver);
        //    _layer = AdornerLayer.GetAdornerLayer(DragScope as Visual);
        //    _layer.Add(_adorner);

        //    //note the selected items in a collection, for use later...
        //    _draggedItems = new List<object>();
        //    foreach (var item in grid.SelectedItems)
        //    {
        //        _draggedItems.Add(item);
        //    }

        //    IsDragging = true;
        //    _dragHasLeftScope = false; 
        //    //Finally lets drag drop 
        //    DataObject data = new DataObject(System.Windows.DataFormats.Text.ToString(), "abcd");
        //    DragDropEffects de = DragDrop.DoDragDrop(grid, data, DragDropEffects.Move);

            
        //     // Clean up our mess :) -- this code is executed when the drag'n'drop operation is completed.
        //    DragScope.AllowDrop = previousDrop;
        //    AdornerLayer.GetAdornerLayer(DragScope).Remove(_adorner);
        //    _adorner = null;




        //    //if we're dropping the dragged items onto an item that isn't in the list of dragged items...
        //    if (grid.SelectedItem == null || false == _draggedItems.Contains(grid.SelectedItem))
        //    {
        //        var targetIndex = grid.Items.IndexOf(grid.SelectedItem);
        //        //execute the command, to allow the control using the datagrid to handle moving the items
        //        grid.MoveItemsCommand.Execute(new MoveItemsCommandParameter(targetIndex, selectedItemsOrdered));
        //    }

        //    //select the dropped items
        //    grid.SelectedItems.Clear();
        //    _draggedItems.ForEach(x => grid.SelectedItems.Add(x));
        //    _draggedItems = null;

        //    grid.GiveFeedback -= feedbackhandler;
        //    DragScope.DragLeave -= dragleavehandler;
        //    DragScope.QueryContinueDrag -= queryhandler;
        //    DragScope.DragOver -= DragScope_DragOver;

        //    IsDragging = false;
        //    _isClickingDownOnSelectedItem = false;
        //}

        ///// <summary>
        ///// Used to track which grid row we're hovering over, while dragging. we will select this row, to give the user feedback on where the dragged
        ///// rows will be inserted.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void DragScope_DragOver(object sender, DragEventArgs e)
        //{
        //    //JDW: make sure the row under the grid is being selected, to give user feedback about where the rows will be inserted
        //    Point position = e.GetPosition(this);
        //    System.Diagnostics.Debug.WriteLine("DragSource_GiveFeedbackPos " + position.ToString());
        //    var row = UIHelpers.TryFindFromPoint<DataGridRow>(this, position);
        //    if (row != null) this.SelectedItem = row.Item;
        //}

        //private bool _dragHasLeftScope = false; 
        //void DragScope_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        //{
        //    if (this._dragHasLeftScope)
        //    {
        //        e.Action = DragAction.Cancel;
        //        e.Handled = true;
        //    }
        //}


        //void DragScope_DragLeave(object sender, DragEventArgs e)
        //{
        //    if (e.OriginalSource == DragScope)
        //    {
        //        Point p = e.GetPosition(DragScope);
        //        Rect r = VisualTreeHelper.GetContentBounds(DragScope);
        //        if (!r.Contains(p))
        //        {
        //            this._dragHasLeftScope = true;
        //            e.Handled = true;
        //        }
        //    }

        //}




        //private Point _startPoint;
        //private bool _isDragging;

        //public bool IsDragging
        //{
        //    get { return _isDragging; }
        //    set { _isDragging = value; }
        //} 


        #endregion
    }
}
