using System.Windows;
using System.Windows.Controls;

namespace lageant.Models
{
    /// <summary>
    ///     Custom ListView object.
    /// </summary>
    public class KeyListView : ListView
    {
        /// <summary>
        ///     Overwrite for GetContainerForItemOverride.
        /// </summary>
        /// <returns>A DependencyObject.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new KeyListViewItem();
        }
    }
}