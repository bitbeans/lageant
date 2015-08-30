using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;

namespace lageant.ViewModels
{
    [Export]
    public sealed class PasswordInputViewModel : Screen
    {
        private string _userPassword;

        /// <summary>
        ///     PasswordInputViewModel constructor.
        /// </summary>
        [ImportingConstructor]
        public PasswordInputViewModel()
        {
        }

        /// <summary>
        ///     The user password.
        /// </summary>
        public string UserPassword
        {
            get { return _userPassword; }
            set
            {
                if (value.Equals(_userPassword)) return;
                _userPassword = value;
                NotifyOfPropertyChange(() => UserPassword);
            }
        }

        /// <summary>
        ///     User password changed.
        /// </summary>
        /// <param name="e"></param>
        public void PasswordChanged(RoutedEventArgs e)
        {
            var t = e.Source as PasswordBox;

            if (t != null && !string.IsNullOrEmpty(t.Password))
            {
                // Bugfix: cursor position
                if (t.Password.Length == 1)
                {
                    // set the cursor position to 1
                    SetSelection(t, 1, 0);
                }
                UserPassword = t.Password;
            }
            else
            {
                UserPassword = string.Empty;
            }
        }

        /// <summary>
        ///     Sets the cursor to a given selection.
        /// </summary>
        /// <param name="passwordBox">The PasswordBox to set the selection.</param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        private static void SetSelection(PasswordBox passwordBox, int start, int length)
        {
            passwordBox.GetType()
                .GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(passwordBox, new object[] {start, length});
        }

        public void SendOk()
        {
            if (!string.IsNullOrEmpty(UserPassword))
            {
                TryClose(true);
            }
            else
            {
                TryClose(false);
            }
        }
    }
}