using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using ICSharpCode.AvalonEdit;
using System.Windows.Documents;
using AvalonTextBox.Converters;
using System.Windows.Input;
using JayDev.Notemaker.Common;
using GalaSoft.MvvmLight.Messaging;
using Notemaker.Common;

namespace AvalonTextBox
{
    public class AvalonTextBox : TextEditor
    {
        #region Private Members

        MarkupColorizer colorizer;
        List<Section> sections;


        bool _isTextUpdatedSinceCachedMarkedupText = false;
        string _cachedMarkedupText;
        List<Inline> _cachedForTextblock;

        bool isUpdatingMarkedupTextInternally = false;

        #endregion

        #region Dependency Properties

        public string MarkedupText
        {
            get { return (string)GetValue(MarkedupTextProperty); }
            set { SetValue(MarkedupTextProperty, value); }
        }


        // Using a DependencyProperty as the backing store for MarkedupText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarkedupTextProperty =
            DependencyProperty.Register("MarkedupText", typeof(string), typeof(AvalonTextBox), new UIPropertyMetadata(new PropertyChangedCallback((a, b) =>
            {
                (a as AvalonTextBox).SetMarkedupText((string)b.NewValue);
            })));

        #endregion

        #region Constructor

        public AvalonTextBox()
            : base()
        {
            colorizer = new MarkupColorizer(sections, this.TextArea.Foreground, this);
            this.LostFocus += new RoutedEventHandler(AvalonTextBox_LostFocus);
            //we set 'ShowBoxForControlCharacters' to false, to stop a vertical gray line showing up about half way across the textbox
            this.Options.ShowBoxForControlCharacters = false;
            this.Loaded += new RoutedEventHandler(AvalonTextBox_Loaded);

        }

        //ensure that we listen to document changes when the textbox is LOADED -- this is so when we enter new notes, before actually having
        //having bound anything, it will still flag the text as changed.
        void AvalonTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            this.Document.Changed -= Document_Changed;
            this.Document.Changed += new EventHandler<DocumentChangeEventArgs>(Document_Changed);
            this.PreviewKeyDown += new KeyEventHandler(AvalonTextBox_PreviewKeyDown);
        }

        #endregion

        #region Public Methods

        public void ApplyBold()
        {
            Apply(true, false, null);
        }

        public void ApplyItalics()
        {
            Apply(false, true, null);
        }

        public void ApplyColour(Color? colour)
        {
            Apply(false, false, colour);
        }

        #endregion

        #region Protected Methods


        void SetMarkedupText(string markedupText)
        {
            if (isUpdatingMarkedupTextInternally)
                return;

            _isTextUpdatedSinceCachedMarkedupText = false;
            _cachedMarkedupText = markedupText;
            PrepareSections();

            StringBuilder strippedText = new StringBuilder();
            foreach (var section in sections)
            {
                strippedText.Append(section.Text);
            }
            this.Text = strippedText.ToString();

            colorizer.Sections = sections;
            this.TextArea.TextView.LineTransformers.Clear();
            this.TextArea.TextView.LineTransformers.Add(colorizer);
            this.Document.Changed -= Document_Changed;
            this.Document.Changed += new EventHandler<DocumentChangeEventArgs>(Document_Changed);

        }

        List<Inline> GetTextForTextblock()
        {
            _cachedForTextblock = (List<Inline>)new NoteSectionsToInlineListConverter().Convert(sections, typeof(List<Inline>), null, null);
            return new List<Inline>(_cachedForTextblock);
        }

        string GetMarkedupText()
        {
            if (_isTextUpdatedSinceCachedMarkedupText)
            {
                _cachedMarkedupText = (string)new NoteSectionsToMarkedupTextConverter().Convert(sections, typeof(string), null, null);
                _isTextUpdatedSinceCachedMarkedupText = false;
            }

            return _cachedMarkedupText;
        }

        Section ApplyHelper(Section section, bool isStyleAppliedAlready, bool toggleBold, bool toggleItalics, Color? colour)
        {
            if (isStyleAppliedAlready)
            {
                if (toggleItalics) section.Style = NoteStyle.Normal;
                if (toggleBold) section.Weight = NoteWeight.Normal;
                if (null != colour) section.Colour = ColorHelper.ApplicationDefaultTextColour;
            }
            else
            {
                if (toggleItalics) section.Style = NoteStyle.Italic;
                if (toggleBold) section.Weight = NoteWeight.Bold;
                if (null != colour) section.Colour = colour.Value;
            }
            return section;
        }

        void Apply(bool toggleBold, bool toggleItalics, Color? colour)
        {
            _isTextUpdatedSinceCachedMarkedupText = true;

            //if the selection length is 0, toggle the style on the selection. otherwise, create a new, empty section with the appropriate
            //styling.
            if (this.SelectionLength > 0)
            {
                int position = 0;
                int firstAppliceableSectionIndex = -1;
                int firstAppliceableSectionTextChangeFromIndex = -1;
                int numberAffectedSections = 0;
                for (int i = 0; i < sections.Count; i++)
                {
                    Section currentSection = sections[i];
                    //if we're not at the position, keep on going through the list until we are.
                    if (this.SelectionStart >= position + currentSection.Text.Length)
                    {
                        position += currentSection.Text.Length;
                    }
                    //if we ARE at the position...
                    else
                    {
                        //if this is the first section affected by the change, note it.
                        if (firstAppliceableSectionIndex == -1)
                        {
                            firstAppliceableSectionIndex = i;
                            firstAppliceableSectionTextChangeFromIndex = this.SelectionStart - position;
                        }

                        //if this is the last section affected by the change, note it.
                        if (this.SelectionStart + this.SelectionLength <= position + currentSection.Text.Length)
                        {
                            numberAffectedSections = (i - firstAppliceableSectionIndex) + 1;
                            break;
                        }

                        position += currentSection.Text.Length;
                    }
                }

                //now to do the processing. if the style is already applied on ALL the elements, we remove it. if it's on none or some, we
                //apply the style to all.
                bool isStyleAppliedToAll = true;
                for (int q = 0; q < numberAffectedSections; q++)
                {
                    Section currentSection = sections[firstAppliceableSectionIndex + q];
                    if ((toggleBold && currentSection.Weight != NoteWeight.Bold)
                        || (toggleItalics && currentSection.Style != NoteStyle.Italic)
                        || (null != colour && currentSection.Colour != colour))
                    {
                        isStyleAppliedToAll = false;
                        break;
                    }
                }

                int currentSectionIndex = firstAppliceableSectionIndex;
                int changeLengthLeft = this.SelectionLength;
                //loop through the affected sections... NOTE: we don't iterate through the collection, because we'll likely be inserting new
                //entries as we go and it'll be difficult to track.
                for (int q = 0; q < numberAffectedSections; q++)
                {
                    Section currentSection = sections[currentSectionIndex];
                    bool isFirstSection = q == 0;
                    bool isLastSection = q == numberAffectedSections - 1;


                    //if this is the FIRST section and the change only applies to part of it... we'll snip off the affected part of this
                    //section and create a new one, trim the old section to get rid of the affected part... and we'll apply the new stylings
                    //to the new section a little further down.
                    if (isFirstSection && firstAppliceableSectionTextChangeFromIndex > 0)
                    {
                        Section newSection = currentSection;
                        newSection.Text = newSection.Text.Substring(firstAppliceableSectionTextChangeFromIndex);

                        currentSection.Text = currentSection.Text.Substring(0, firstAppliceableSectionTextChangeFromIndex);
                        sections[currentSectionIndex] = currentSection;

                        sections.Insert(currentSectionIndex + 1, newSection);
                        currentSectionIndex++;
                        currentSection = newSection;
                    }

                    //if it's before the last section, or IS the last section and the change is applicable to the entire section... then
                    //make the changes accordingly.
                    if (false == isLastSection || changeLengthLeft == currentSection.Text.Length)
                    {
                        currentSection = ApplyHelper(currentSection, isStyleAppliedToAll, toggleBold, toggleItalics, colour);
                        sections[currentSectionIndex] = currentSection;
                        changeLengthLeft -= currentSection.Text.Length;
                    }
                    //else, it's the last section and the change applies to only part of it.
                    else
                    {
                        //create a new section which will be inserted after the current section, without the style changes.
                        Section newSection = currentSection;
                        newSection.Text = newSection.Text.Substring(changeLengthLeft);

                        currentSection.Text = currentSection.Text.Substring(0, changeLengthLeft);
                        currentSection = ApplyHelper(currentSection, isStyleAppliedToAll, toggleBold, toggleItalics, colour);
                        sections[currentSectionIndex] = currentSection;

                        //insert the new section into the collection, after the current section.
                        sections.Insert(currentSectionIndex + 1, newSection);
                        //increment the current section index, to reflect the new section we just made.
                        currentSectionIndex++;

                        changeLengthLeft -= currentSection.Text.Length;
                    }

                    //increment the current selection index, so that we move onto the next section.
                    currentSectionIndex++;
                }
            }
            else
            {
                int position = 0;
                for (int i = 0; i < sections.Count; i++)
                {
                    Section currentSection = sections[i];

                    //if the start of the selection doesn't fall within this section, keep on going through the sections until it does.
                    if (this.SelectionStart > position + currentSection.Text.Length)
                    {
                        position += currentSection.Text.Length;
                    }
                    //else, if it does fall somewhere within this section...
                    else
                    {
                        bool isStyleAlreadyApplied;
                        //if the selection starts after the beginning... figure out where, and split the section so we can style from the
                        //selection point onward
                        int indexOfSectionWithCaret = i;
                        if (this.CaretOffset > position)
                        {
                            //if it's at the END of the section... and there's been nothing selected, and there's an empty section next...
                            //we've already split the section before, and we're stacking up style changes. so, apply the style to the blank
                            //section, and leave.
                            if (this.SelectionLength == 0 && this.SelectionStart == position + currentSection.Text.Length
                                && i < sections.Count - 1 && sections[i + 1].Text.Length == 0)
                            {
                                Section stackingSection = sections[i + 1];
                                isStyleAlreadyApplied = (toggleBold && stackingSection.Weight == NoteWeight.Bold)
                                            || (toggleItalics && stackingSection.Style == NoteStyle.Italic)
                                            || (null != colour && stackingSection.Colour == colour);
                                stackingSection = ApplyHelper(stackingSection, isStyleAlreadyApplied, toggleBold, toggleItalics, colour);
                                sections[i + 1] = stackingSection;
                                return;
                            }
                            //if it's somewhere in the middle of the section (or at the end, but we're not stacking style changes), then
                            //split the sections up as needed
                            else
                            {
                                Section sectionBefore = currentSection;
                                sectionBefore.Text = sectionBefore.Text.Substring(0, this.CaretOffset - position);
                                sections[indexOfSectionWithCaret] = sectionBefore;
                                currentSection.Text = currentSection.Text.Substring(this.CaretOffset - position);
                                sections.Insert(indexOfSectionWithCaret + 1, currentSection);
                                indexOfSectionWithCaret++;
                            }
                        }

                        if (currentSection.Text.Length > 0)
                        {
                            Section sectionAfter = currentSection;
                            sections.Insert(indexOfSectionWithCaret + 1, sectionAfter);
                        }


                        currentSection.Text = string.Empty;
                        isStyleAlreadyApplied = (toggleBold && currentSection.Weight == NoteWeight.Bold)
                                    || (toggleItalics && currentSection.Style == NoteStyle.Italic)
                                    || (null != colour && currentSection.Colour == colour);
                        currentSection = ApplyHelper(currentSection, isStyleAlreadyApplied, toggleBold, toggleItalics, colour);
                        sections[indexOfSectionWithCaret] = currentSection;
                        break;
                    }
                }
            }

            bool userChangedStylingsAtCaret = this.SelectionLength == 0;
            //JDW: only merge sections if we've been changing the stylings of chunks of sections. this is because if the user creates a new,
            //empty section with a different style... we need to be able to insert into there.
            if (false == userChangedStylingsAtCaret)
            {
                for (int i = sections.Count - 1; i >= 1; i--)
                {
                    if (sections[i].Colour == sections[i - 1].Colour
                        && sections[i].Style == sections[i - 1].Style
                        && sections[i].Weight == sections[i - 1].Weight)
                    {
                        Section mergedSection = sections[i - 1];
                        mergedSection.Text += sections[i].Text;
                        sections.RemoveAt(i);
                        sections[i - 1] = mergedSection;
                    }
                }
            }

            this.TextArea.TextView.Redraw();
        }

        void PrepareSections()
        {
            sections = (List<Section>)new MarkedupTextToNoteSectionsConverter().Convert(GetMarkedupText(), typeof(List<Section>), null, null);
        }

        void Assert(bool condition, string errorMessage)
        {
            if (condition)
                throw new Exception(errorMessage);
        }

        #endregion

        #region Event Handlers

        void AvalonTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            isUpdatingMarkedupTextInternally = true;
            MarkedupText = GetMarkedupText();
            isUpdatingMarkedupTextInternally = false;
        }

        void Document_Changed(object sender, DocumentChangeEventArgs e)
        {
            _isTextUpdatedSinceCachedMarkedupText = true;
            Assert(e.OffsetChangeMap.Count > 1, "uhh... we have more than one change?");

            if (e.RemovalLength > 0)
            {
                int charactersToRemove = e.RemovalLength;
                int position = 0;
                for (int i = 0; i < sections.Count; i++)
                {
                    Section currentSection = sections[i];
                    //if we're not at the position, keep on going through the list until we are.
                    if (e.Offset > position + currentSection.Text.Length)
                    {
                        position += currentSection.Text.Length;
                    }
                    //if we ARE at the position, remove the text and leave.
                    else
                    {
                        int deleteFromSectionIndex = e.Offset - position;
                        //this section is only so long. does the removal stretch into future sections? if so, how many characters into
                        //future sections?
                        int removalOverflow = charactersToRemove - (currentSection.Text.Length - deleteFromSectionIndex);
                        int removalLengthForSection = Math.Min(charactersToRemove, currentSection.Text.Length - deleteFromSectionIndex);
                        currentSection.Text = currentSection.Text.Remove(deleteFromSectionIndex, removalLengthForSection);
                        if (currentSection.Text.Length > 0)
                        {
                            sections[i] = currentSection;
                        }
                        else
                        {
                            //if it's a section in the middle of the text, and the last bits of text from it have been removed, get rid of
                            //the section. the only time we can have an empty section, is JUST after the user instructs one to be created.
                            //if they don't immediately populate it (i.e. start typing into a different section, or remove text from
                            //anywhere), it gets deleted again.
                            sections.RemoveAt(i);
                            //since we've removed the section, we have to decrement the index again
                            i--;
                        }

                        charactersToRemove -= removalLengthForSection;
                        //we've already begin the removals - the next section, start removing from the very beginning.
                        position = e.Offset;

                        //if there's nothing more to remove, get outta here.
                        if (charactersToRemove <= 0)
                            break;
                    }
                }
            }

            if (e.InsertionLength > 0)
            {
                int position = 0;
                for (int i = 0; i < sections.Count; i++)
                {
                    Section currentSection = sections[i];
                    //if we're not at the position, keep on going through the list until we are.
                    if (e.Offset > position + currentSection.Text.Length)
                    {
                        position += currentSection.Text.Length;
                    }
                    //if we're at the END of the section, and the next section's empty... the user probably created a new style and
                    //wants to insert the text into that.
                    else if (e.Offset == position + currentSection.Text.Length
                        && i < sections.Count - 1
                        && sections[i + 1].Text.Length == 0)
                    {
                        currentSection = sections[i + 1];
                        currentSection.Text = e.InsertedText.Text;
                        sections[i + 1] = currentSection;
                        break;
                    }
                    //if we're supposed to insert somewhere within the current section, inser the text and leave.
                    else
                    {
                        int insertToSectionIndex = e.Offset - position;
                        currentSection.Text = currentSection.Text.Insert(insertToSectionIndex, e.InsertedText.Text);
                        sections[i] = currentSection;
                        break;
                    }
                }
            }

            //remove any left-over empty sections after changes.
            for (int i = sections.Count - 1; i >= 0; i--)
            {
                if (sections[i].Text.Length == 0)
                {
                    sections.RemoveAt(i);
                }
            }
            //merge same-styled sections after changes
            for (int i = sections.Count - 1; i >= 1; i--)
            {
                if (sections[i].Colour == sections[i - 1].Colour
                    && sections[i].Style == sections[i - 1].Style
                    && sections[i].Weight == sections[i - 1].Weight)
                {
                    Section mergedSection = sections[i - 1];
                    mergedSection.Text += sections[i].Text;
                    sections.RemoveAt(i);
                    sections[i - 1] = mergedSection;
                }
            }
        }

        void AvalonTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var matches = HotkeyManager.CheckHotkey(e);

            if (null != matches && matches.Count > 0)
            {
                foreach (var match in matches)
                {
                    switch (match.Function)
                    {
                        case HotkeyFunction.NoteColour:
                            ApplyColour(match.Colour);
                            break;
                        case HotkeyFunction.NoteItalic:
                            ApplyItalics();
                            break;
                        case HotkeyFunction.NoteBold:
                            ApplyBold();
                            break;
                    }

                    e.Handled = true;
                }
            }
            //switch (e.Key)
            //{
            //    case Key.D1:
            //        e.Handled = true;
            //        ApplyColour(Colors.GreenYellow);
            //        break;
            //    case Key.D2:
            //        e.Handled = true;
            //        ApplyColour(Colors.Indigo);
            //        break;
            //    case Key.D3:
            //        e.Handled = true;
            //        ApplyColour(ColorHelper.ApplicationDefaultTextColour);
            //        break;
            //    case Key.D4:
            //        e.Handled = true;
            //        ApplyBold();
            //        break;
            //    case Key.D5:
            //        e.Handled = true;
            //        ApplyItalics();
            //        break;
            //}
        }

        #endregion
    }
}
