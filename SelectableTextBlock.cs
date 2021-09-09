/*
 * Copyright (C) 2021 Mike Wagner
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
namespace Wagner.Common.UI.Controls
{
    /// <summary>
    /// The delegate to use for handlers that receive TextSelectedEventArgs
    /// </summary>
    public delegate void TextSelectedEventHandler(object sender, TextSelectedEventArgs e);
    public class TextSelectedEventArgs : RoutedEventArgs
    {
        private TextRange _selectedTextRange;

        public TextSelectedEventArgs(TextRange selectedTextRange)
        {
            this._selectedTextRange = selectedTextRange;
        }
        public TextPointer StartSelectPosition => this._selectedTextRange.Start;
        public TextPointer EndSelectPosition => this._selectedTextRange.End;
        public TextRange SelectedTextRange { get => this._selectedTextRange; set => this._selectedTextRange = value; }
        public string SelectedText => this._selectedTextRange.Text;
        public bool IsEmpty => this._selectedTextRange.IsEmpty;
    }
    public class SelectableTextBlock : TextBlock
    {
        private bool _isSelecting;
        private TextPointer _startSelectPosition;
        private TextPointer _endSelectPosition;
        private TextRange _selectedTextRange = null;
        #region Dependency Propery Defaults
        // Defaults used in the DependencyPropertys
        protected static readonly bool DoubleClickSelectWordEnabledDefault = true;
        protected static readonly bool TripleClickSelectAllEnabledDefault = true;
        protected static readonly bool UseTextCursorEnabledDefault = true;
        protected static readonly Brush SelectionHighlightBrushDefault = new SolidColorBrush(Color.FromRgb(51, 153, 255));
        protected static readonly bool HighlightWhileSelectingEnabledDefault = true;
        protected static readonly Cursor TextCursorDefault = Cursors.IBeam;
        #endregion
        public static readonly DependencyProperty UseTextCursorEnabledProperty = DependencyProperty.Register(nameof(UseTextCursorEnabled), typeof(bool), typeof(SelectableTextBlock), new PropertyMetadata(UseTextCursorEnabledDefault));
        public static readonly DependencyProperty SelectionHighlightBrushProperty = DependencyProperty.Register(nameof(SelectionHighlightBrush), typeof(Brush), typeof(SelectableTextBlock), new PropertyMetadata(SelectionHighlightBrushDefault));
        public static readonly DependencyProperty HighlightWhileSelectingEnabledProperty = DependencyProperty.Register(nameof(HighlightWhileSelectingEnabled), typeof(bool), typeof(SelectableTextBlock), new PropertyMetadata(HighlightWhileSelectingEnabledDefault));
        public static readonly DependencyProperty TextCursorProperty = DependencyProperty.Register(nameof(TextCursor), typeof(Cursor), typeof(SelectableTextBlock), new PropertyMetadata(TextCursorDefault));
        public static readonly DependencyProperty DoubleClickSelectWordEnabledProperty = DependencyProperty.Register(nameof(DoubleClickSelectWordEnabled), typeof(bool), typeof(SelectableTextBlock), new PropertyMetadata(DoubleClickSelectWordEnabledDefault));
        public static readonly DependencyProperty TripleClickSelectAllEnabledProperty = DependencyProperty.Register(nameof(TripleClickSelectAllEnabled), typeof(bool), typeof(SelectableTextBlock), new PropertyMetadata(TripleClickSelectAllEnabledDefault));

        public SelectableTextBlock() : base()
        {
            this.MouseLeftButtonDown += new MouseButtonEventHandler(OnLeftMouseDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(OnLeftMouseUp);
            this.MouseMove += new MouseEventHandler(OnMouseMove);
        }
        public event TextSelectedEventHandler OnTextSelected;

        /// <summary>
        /// The selected TextRange.
        /// </summary>
        public TextRange SelectedText { get => this._selectedTextRange; set => this._selectedTextRange = value; }
        /// <summary>
        /// Controls whether the cursor chould change when over text.
        /// </summary>
        public bool UseTextCursorEnabled
        {
            get { return (bool)GetValue(UseTextCursorEnabledProperty); }
            set { SetValue(UseTextCursorEnabledProperty, value); }
        }
        /// <summary>
        /// The background brush to use for selected text.
        /// </summary>
        public Brush SelectionHighlightBrush
        {
            get { return (Brush)GetValue(SelectionHighlightBrushProperty); }
            set { SetValue(SelectionHighlightBrushProperty, value); }
        }
        /// <summary>
        /// Change the background of text being selected.
        /// </summary>
        public bool HighlightWhileSelectingEnabled
        {
            get { return (bool)GetValue(HighlightWhileSelectingEnabledProperty); }
            set { SetValue(HighlightWhileSelectingEnabledProperty, value); }
        }
        /// <summary>
        /// The cursor to display when over text.
        /// </summary>
        public Cursor TextCursor
        {
            get { return (Cursor)GetValue(TextCursorProperty); }
            set { SetValue(TextCursorProperty, value); }
        }
        /// <summary>
        /// Controls whether double clicking selects the word clicked.
        /// </summary>
        public bool DoubleClickSelectWordEnabled
        {
            get { return (bool)GetValue(DoubleClickSelectWordEnabledProperty); }
            set { SetValue(DoubleClickSelectWordEnabledProperty, value); }
        }
        /// <summary>
        /// Controls whether triple clicking selects all the text.
        /// </summary>
        public bool TripleClickSelectAllEnabled
        {
            get { return (bool)GetValue(TripleClickSelectAllEnabledProperty); }
            set { SetValue(TripleClickSelectAllEnabledProperty, value); }
        }

        /// <summary>
        /// Changes the cursor type when over text.
        /// </summary>
        private void UpdateCursor(TextPointer textPointerClosestToCursor)
        {
            if (UseTextCursorEnabled)
            {
                if (textPointerClosestToCursor.Parent is Run run && run.IsMouseOver)
                {
                    Cursor = TextCursor;
                }
                else
                {
                    Cursor = null; //default cursor
                }
            }
        }
        /// <summary>
        /// Updates the selection as the mouse moves.
        /// </summary>
        private void UpdateSelection(TextPointer newEndSelectPosition)
        {
            if (this._endSelectPosition != null)
            {
                TextRange newTextRange;
                TextRange clearTextRange;
                if (this._startSelectPosition.CompareTo(this._endSelectPosition) == this._endSelectPosition.CompareTo(newEndSelectPosition))
                {
                    newTextRange = new TextRange(this._startSelectPosition, this._endSelectPosition.GetPositionAtOffset(this._endSelectPosition.CompareTo(this._startSelectPosition)));
                    newTextRange.ApplyPropertyValue(TextElement.BackgroundProperty, SelectionHighlightBrush);
                }
                else
                {
                    clearTextRange = new TextRange(this._endSelectPosition, newEndSelectPosition);
                    ClearTextRange(clearTextRange);
                }
            }
            this._endSelectPosition = newEndSelectPosition;
        }
        /// <summary>
        /// Tests if a char matches the \w character class.
        /// </summary>
        /// <param name="c">The char to test</param>
        /// <returns>True if a char matches the \w character class.</returns>
        private bool IsWordCharacter(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }
        /// <summary>
        /// Raises the OnTextSelected event
        /// </summary>
        private void RaiseOnTextSelected()
        {
            if (OnTextSelected != null)
            {
                TextSelectedEventArgs textSelectedEventArgs = new TextSelectedEventArgs(this._selectedTextRange);
                OnTextSelected.Invoke(this, textSelectedEventArgs);
            }

        }
        protected virtual void OnLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            ClearSelection();
            Point mouseDownPoint = e.GetPosition(this);
            if (e.ClickCount == 1)
            {
                this._startSelectPosition = this.GetPositionFromPoint(mouseDownPoint, true);
                this._isSelecting = true;
            }
            else if (e.ClickCount == 2 && DoubleClickSelectWordEnabled)
            {
                SelectDoubleClickedWord(mouseDownPoint);
            }
            else if (e.ClickCount == 3 && TripleClickSelectAllEnabled)
            {
                SelectAll();
            }
            
        }
        protected virtual void ClearSelection()
        {
            ClearTextRange(this._selectedTextRange);
            this._selectedTextRange = null;
        }
        /// <summary>
        /// Resets the background of the TextRange to the TextBlock background.
        /// </summary>
        protected virtual void ClearTextRange(TextRange textRange)
        {
            if (textRange != null)
            {
                textRange.ApplyPropertyValue(TextElement.BackgroundProperty, this.Background);
            }
        }
        protected virtual void OnLeftMouseUp(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (this._startSelectPosition == null || !this._isSelecting)
            {
                this._isSelecting = false;
                return;
            }

            Point mouseUpPoint = e.GetPosition(this);
            this._endSelectPosition = this.GetPositionFromPoint(mouseUpPoint, true);
            if (this._startSelectPosition.CompareTo(this._endSelectPosition) == 0)
            {
                this._startSelectPosition = null;
                this._endSelectPosition = null;
                this._isSelecting = false;
                return;
            }

            this._selectedTextRange = new TextRange(this._startSelectPosition, this._endSelectPosition);


            RaiseOnTextSelected();
            this._startSelectPosition = null;
            this._endSelectPosition = null;
            this._isSelecting = false;
        }
        protected virtual void OnMouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            TextPointer endSelectPosition = this.GetPositionFromPoint(e.GetPosition(this), true);
            UpdateCursor(endSelectPosition);
            if (e.LeftButton == MouseButtonState.Released)
            {
                this._isSelecting = false;
            }
            else if (this._isSelecting && (this._endSelectPosition == null || endSelectPosition.CompareTo(this._endSelectPosition) != 0))
            {
                UpdateSelection(endSelectPosition);
            }
        }
        /// <summary>
        /// Selects the word clicked. Methodology was to get the character clicked and expand in both directions
        /// until either the end of the string or a non-word character [^\w] is hit.
        /// This is not straighforward because formatting symbols are stored with text in the TextBlock internals.
        /// I'm sure this can be better implemented by someone who understands why 90% of all TextPointer and TextRange members are internal.
        /// </summary>
        protected virtual void SelectDoubleClickedWord(Point mousePoint)
        {
            TextPointer wordStartBoundary = this.GetPositionFromPoint(mousePoint, true);
            TextPointer wordEndBoundary = this.GetPositionFromPoint(mousePoint, true);
            TextRange textRange = new TextRange(wordStartBoundary, wordEndBoundary);
            while (wordStartBoundary.GetPositionAtOffset(-1) != null && (textRange.Text.Length == 0 || IsWordCharacter(textRange.Text.First())))
            {
                wordStartBoundary = wordStartBoundary.GetPositionAtOffset(-1);
                textRange = new TextRange(wordStartBoundary, wordEndBoundary);
            }
            wordStartBoundary = wordStartBoundary.GetPositionAtOffset(1); //Fix overshoot
            while (wordEndBoundary.GetPositionAtOffset(1) != null && (textRange.Text.Length == 0 || IsWordCharacter(textRange.Text.Last())))
            {
                wordEndBoundary = wordEndBoundary.GetPositionAtOffset(1);
                textRange = new TextRange(wordStartBoundary, wordEndBoundary);
            }
            wordEndBoundary = wordEndBoundary.GetPositionAtOffset(-1); //Fix overshoot
            textRange = new TextRange(wordStartBoundary, wordEndBoundary);
            this._startSelectPosition = wordStartBoundary;
            this._endSelectPosition = wordEndBoundary;
            this._selectedTextRange = textRange;
            textRange.ApplyPropertyValue(TextElement.BackgroundProperty, SelectionHighlightBrush);
            RaiseOnTextSelected();
        }
        /// <returns>what I thought TextBlock.Text would return.</returns>
        public string GetText()
        {
            TextRange textRange = new TextRange(this.ContentStart, this.ContentEnd);
            return textRange.Text;
        }
        /// <summary>
        /// Selects all the text.
        /// </summary>
        public virtual void SelectAll()
        {
            this._startSelectPosition = this.ContentStart;
            this._endSelectPosition = this.ContentEnd;
            this._selectedTextRange = new TextRange(this.ContentStart, this.ContentEnd);
            this._selectedTextRange.ApplyPropertyValue(TextElement.BackgroundProperty, SelectionHighlightBrush);
            RaiseOnTextSelected();
        }
    }
}
