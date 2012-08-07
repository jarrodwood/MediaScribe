using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using AvalonTextBox.Converters;
using System.Windows.Media;

namespace AvalonTextBox
{
    public class BindableTextBlock : TextBlock
    {

        public string MarkedupText
        {
            get { return (string)GetValue(MarkedupTextProperty); }
            set { SetValue(MarkedupTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MarkedupText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarkedupTextProperty =
            DependencyProperty.Register("MarkedupText", typeof(string), typeof(BindableTextBlock), new UIPropertyMetadata(new PropertyChangedCallback((sender, args) => {
                if (null == args.NewValue || string.IsNullOrEmpty((string)args.NewValue))
                {
                    BindableTextBlock textBlock = sender as BindableTextBlock;
                    textBlock.Text = "Double-click here to write a note...";
                    textBlock.FontStyle = FontStyles.Italic;
                    textBlock.Opacity = 0.5;
                }
                else
                {
                    BindableTextBlock textBlock = sender as BindableTextBlock;
                    textBlock.Text = null;
                    textBlock.FontStyle = FontStyles.Normal;
                    textBlock.Opacity = 1;

                    textBlock.Inlines.Clear();
                    var inlines = new MarkedupTextToInlineListConverter().Convert(args.NewValue, typeof(List<Inline>), null, null) as List<Inline>;
                    //trying to add 'null' throws exceptions
                    if (null != inlines && inlines.Count > 0)
                    {
                        textBlock.Inlines.AddRange(inlines);
                    }
                }
            })));

        

        public IEnumerable<Inline> InlineCollection
        {
            get
            {
                return (IEnumerable<Inline>)GetValue(InlineCollectionProperty);
            }
            set
            {
                SetValue(InlineCollectionProperty, value);
            }
        }

        public static readonly DependencyProperty InlineCollectionProperty = DependencyProperty.Register(
            "InlineCollection",
            typeof(IEnumerable<Inline>),
            typeof(BindableTextBlock),
                new UIPropertyMetadata((PropertyChangedCallback)((sender, args) =>
                {
                    BindableTextBlock textBlock = sender as BindableTextBlock;

                    if (textBlock != null)
                    {
                        textBlock.Inlines.Clear();

                        IEnumerable<Inline> inlines = args.NewValue as IEnumerable<Inline>;

                        if (inlines != null)
                            textBlock.Inlines.AddRange(inlines);
                    }
                })));




        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(BindableTextBlock), new UIPropertyMetadata(OnDefaultTextChanged));



        public static void OnDefaultTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            string text = args.NewValue as string;
            if (null != text && false == string.IsNullOrEmpty(text))
            {
                BindableTextBlock textblock = (BindableTextBlock)obj;
                if (null != text && textblock.Inlines.Count == 0)
                {
                    Run run = new Run(text);
                    run.Foreground = new SolidColorBrush(Color.FromArgb(255, 180, 180, 180));
                    Italic italic = new Italic(run);
                    textblock.Inlines.Add(italic);
                }
            }
        }
    }
}