using System.Windows.Controls;
using System.Windows.Input;

namespace lageant.Models
{
    /// <summary>
    ///     Custom ListViewItem.
    /// </summary>
    public class KeyListViewItem : ListViewItem
    {
        protected override void OnMouseEnter(MouseEventArgs e)
        {
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!IsSelected
                || Keyboard.IsKeyDown(Key.LeftShift)
                || Keyboard.IsKeyDown(Key.LeftCtrl)
                || Keyboard.IsKeyDown(Key.RightShift)
                || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                base.OnMouseLeftButtonDown(e);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (!IsSelected
                || Keyboard.IsKeyDown(Key.LeftShift)
                || Keyboard.IsKeyDown(Key.LeftCtrl)
                || Keyboard.IsKeyDown(Key.RightShift)
                || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                base.OnMouseLeftButtonUp(e);
            }

            else
            {
                base.OnMouseLeftButtonDown(e);
            }
        }
    }
}