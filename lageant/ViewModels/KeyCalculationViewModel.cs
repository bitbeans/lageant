using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using lageant.core.Models;
using NaclKeys;
using NaclKeys.Helper;

namespace lageant.ViewModels
{
    [Export]
    public sealed class KeyCalculationViewModel : Screen
    {
        private string _userPassword;
        private string _userName;
        private bool _isCalculatingKey;
        private bool _bytejailFormat;
        private string _calculationError;
        private bool _curveLockFormat;
        private bool _miniLockFormat;
        private Key _calculatedKey;

        /// <summary>
        ///     KeyCalculationViewModel constructor.
        /// </summary>
        [ImportingConstructor]
        public KeyCalculationViewModel()
        {
            IsCalculatingKey = false;
            BytejailFormat = true;
            CalculationError = string.Empty;
        }

        /// <summary>
        ///     The calculated key.
        /// </summary>
        public Key CalculatedKey
        {
            get { return _calculatedKey; }
            set
            {
                if (value.Equals(_calculatedKey)) return;
                _calculatedKey = value;
                NotifyOfPropertyChange(() => CalculatedKey);
            }
        }

        public async void GenerateKeyFromInput()
        {
            CalculationError = string.Empty;
            IsCalculatingKey = true;

            var keyType = KeyType.Bytejail;
            if (BytejailFormat)
            {
                keyType = KeyType.Bytejail;
            }
            else if (CurveLockFormat)
            {
                keyType = KeyType.Curvelock;
            }
            else if (MiniLockFormat)
            {
                keyType = KeyType.Minilock;
            }

            if ((!string.IsNullOrEmpty(UserName)) && (!string.IsNullOrEmpty(UserPassword)))
            {
                if ((keyType == KeyType.Curvelock) || (keyType == KeyType.Minilock))
                {
                    if (!StringHelper.IsValidEmail(UserName))
                    {
                        CalculationError = "miniLock and CurveLock requires a valid email address as first input.";
                        IsCalculatingKey = false;
                        return;
                    }
                }
                try
                {
                    CalculatedKey = await CalculateNewKey(keyType, UserName, UserPassword);
                    TryClose(CalculatedKey != null);
                }
                catch (Exception)
                {
                    CalculationError = "Key calculation failed.";
                }
            }
            else
            {
                CalculationError = "Please enter something in both fields.";
            }
            IsCalculatingKey = false;
        }

        /// <summary>
        ///     Calculate a new key.
        /// </summary>
        /// <returns>A calculated key.</returns>
        private static async Task<Key> CalculateNewKey(KeyType keyType, string userInputOne, string userInputTwo)
        {
            var newKey = await Task.Run(() =>
            {
                var keyPair = KeyGenerator.GenerateBytejailKeyPair(userInputOne, userInputTwo);
                var key = new Key
                {
                    KeyType = keyType,
                    PrivateKey = keyPair.PrivateKey,
                    PublicKey = keyPair.PublicKey
                };
                return key;
            }).ConfigureAwait(true);
            return newKey;
        }

        /// <summary>
        ///     bytejail radio button.
        /// </summary>
        public bool BytejailFormat
        {
            get { return _bytejailFormat; }
            set
            {
                if (value.Equals(_bytejailFormat)) return;
                _bytejailFormat = value;
                NotifyOfPropertyChange(() => BytejailFormat);
            }
        }

        /// <summary>
        ///     CurveLock radio button.
        /// </summary>
        public bool CurveLockFormat
        {
            get { return _curveLockFormat; }
            set
            {
                if (value.Equals(_curveLockFormat)) return;
                _curveLockFormat = value;
                NotifyOfPropertyChange(() => CurveLockFormat);
            }
        }

        /// <summary>
        ///     MiniLock radio button.
        /// </summary>
        public bool MiniLockFormat
        {
            get { return _miniLockFormat; }
            set
            {
                if (value.Equals(_miniLockFormat)) return;
                _miniLockFormat = value;
                NotifyOfPropertyChange(() => MiniLockFormat);
            }
        }
        /// <summary>
        ///     Error on the calculation tab.
        /// </summary>
        public string CalculationError
        {
            get { return _calculationError; }
            set
            {
                if (value.Equals(_calculationError)) return;
                _calculationError = value;
                NotifyOfPropertyChange(() => CalculationError);
            }
        }

        /// <summary>
        ///     Status of the key calculation.
        /// </summary>
        public bool IsCalculatingKey
        {
            get { return _isCalculatingKey; }
            set
            {
                if (value.Equals(_isCalculatingKey)) return;
                _isCalculatingKey = value;
                NotifyOfPropertyChange(() => IsCalculatingKey);
            }
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
        ///     The user password.
        /// </summary>
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value.Equals(_userName)) return;
                _userName = value;
                NotifyOfPropertyChange(() => UserName);
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
        ///     User name changed.
        /// </summary>
        /// <param name="e"></param>
        public void UsernameChanged(RoutedEventArgs e)
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
                UserName = t.Password;
            }
            else
            {
                UserName = string.Empty;
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
                .Invoke(passwordBox, new object[] { start, length });
        }
    }
}