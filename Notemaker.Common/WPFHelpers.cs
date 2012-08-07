using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace JayDev.MediaScribe.Common
{
  /// <summary>
  /// Common UI related helper methods.
  /// </summary>
  public static class UIHelpers
  {

    #region find parent

    /// <summary>
    /// Finds a parent of a given item on the visual tree.
    /// </summary>
    /// <typeparam name="T">The type of the queried item.</typeparam>
    /// <param name="child">A direct or indirect child of the
    /// queried item.</param>
    /// <returns>The first parent item that matches the submitted
    /// type parameter. If not matching item can be found, a null
    /// reference is being returned.</returns>
    public static T TryFindParent<T>(DependencyObject child)
      where T : DependencyObject
    {
      //get parent item
      DependencyObject parentObject = GetParentObject(child);

      //we've reached the end of the tree
      if (parentObject == null) return null;

      //check if the parent matches the type we're looking for
      T parent = parentObject as T;
      if (parent != null)
      {
        return parent;
      }
      else
      {
        //use recursion to proceed with next level
        return TryFindParent<T>(parentObject);
      }
    }


    /// <summary>
    /// This method is an alternative to WPF's
    /// <see cref="VisualTreeHelper.GetParent"/> method, which also
    /// supports content elements. Do note, that for content element,
    /// this method falls back to the logical tree of the element.
    /// </summary>
    /// <param name="child">The item to be processed.</param>
    /// <returns>The submitted item's parent, if available. Otherwise
    /// null.</returns>
    public static DependencyObject GetParentObject(DependencyObject child)
    {
      if (child == null) return null;
      ContentElement contentElement = child as ContentElement;

      if (contentElement != null)
      {
        DependencyObject parent = ContentOperations.GetParent(contentElement);
        if (parent != null) return parent;

        FrameworkContentElement fce = contentElement as FrameworkContentElement;
        return fce != null ? fce.Parent : null;
      }

      //if it's not a ContentElement, rely on VisualTreeHelper
      return VisualTreeHelper.GetParent(child);
    }

    #endregion







    /// <summary>
    /// Tries to locate a given item within the visual tree,
    /// starting with the dependency object at a given position. 
    /// </summary>
    /// <typeparam name="T">The type of the element to be found
    /// on the visual tree of the element at the given location.</typeparam>
    /// <param name="reference">The main element which is used to perform
    /// hit testing.</param>
    /// <param name="point">The position to be evaluated on the origin.</param>
    public static T TryFindFromPoint<T>(UIElement reference, Point point)
      where T : DependencyObject
    {
      DependencyObject element = reference.InputHitTest(point)
                                   as DependencyObject;
      if (element == null) return null;
      else if (element is T) return (T)element;
      else return TryFindParent<T>(element);
    }



    public static T GetVisualChild<T>(Visual parent) where T : Visual
    {
        T child = default(T);
        int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < numVisuals; i++)
        {
            Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
            child = v as T;
            if (child == null)
            {
                child = GetVisualChild<T>(v);
            }
            if (child != null)
            {
                break;
            }
        }
        return child;
    }
  }
}
