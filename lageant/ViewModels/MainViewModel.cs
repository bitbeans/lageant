using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Caliburn.Micro;
using Helper;
using lageant.core;
using lageant.core.Models;
using lageant.Utils;
using minisign;
using Microsoft.Win32;
using NaclKeys;
using NaclKeys.Helper;
using ProtoBuf;
using Sodium;

namespace lageant.ViewModels
{
    [Export]
    public sealed class MainViewModel : Screen
    {
        private readonly BindableCollection<Key> _keys = new BindableCollection<Key>();
        private readonly ILageantCore _lageantCore;
        private readonly IWindowManager _windowManager;

        private bool _isWorking;
        private string _mainViewError;
        private Key _selectedKey;

        /// <summary>
        ///     MainViewModel construcor on start up.
        /// </summary>
        /// <param name="windowManager">The current window manager.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        [ImportingConstructor]
        public MainViewModel(IWindowManager windowManager, IEventAggregator eventAggregator)
            : this(
                windowManager, eventAggregator, new LageantCore("lageant", 1024*1024))
        {
        }

        /// <summary>
        ///     MainViewModel construcor for XAML.
        /// </summary>
        public MainViewModel()
        {
        }

        /// <summary>
        ///     MainViewModel construcor.
        /// </summary>
        /// <param name="windowManager">The current window manager.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="lageantCore">The lageant core.</param>
        private MainViewModel(IWindowManager windowManager, IEventAggregator eventAggregator, ILageantCore lageantCore)
        {
            _windowManager = windowManager;
            DisplayName = string.Format("Lageant {0} (beta)", VersionUtilities.PublishVersion);
            MainViewError = string.Empty;
            IsWorking = false;

            _lageantCore = lageantCore;
            if (!_lageantCore.CreateServer()) return;

            Keys = new CollectionViewSource {Source = _keys};
        }

        public CollectionViewSource Keys { get; set; }

        /// <summary>
        ///     The current selected key.
        /// </summary>
        public Key SelectedKey
        {
            get { return _selectedKey; }
            set
            {
                if (value.Equals(_selectedKey)) return;
                _selectedKey = value;
                NotifyOfPropertyChange(() => SelectedKey);
            }
        }

        /// <summary>
        ///     Error on the keystore tab.
        /// </summary>
        public string MainViewError
        {
            get { return _mainViewError; }
            set
            {
                if (value.Equals(_mainViewError)) return;
                _mainViewError = value;
                NotifyOfPropertyChange(() => MainViewError);
            }
        }

        public bool IsWorking
        {
            get { return _isWorking; }
            set
            {
                if (value.Equals(_isWorking)) return;
                _isWorking = value;
                NotifyOfPropertyChange(() => IsWorking);
            }
        }

        public void OnClose(CancelEventArgs e)
        {
            e.Cancel = false;
        }

        public void GenerateNewRandomKey()
        {
            MainViewError = string.Empty;
            IsWorking = true;
            AddKey(GenerateNewKey());
            IsWorking = false;
        }

        public void RemoveKeyFromKeystore()
        {
            MainViewError = string.Empty;
            if (SelectedKey != null)
            {
                RemoveKey(SelectedKey);
            }
        }

        public async void ExportKeyToFile()
        {
            MainViewError = string.Empty;
            IsWorking = true;
            string password;
            if (SelectedKey != null)
            {
                var selectedKey = SelectedKey;
                var saveFileDialog = new SaveFileDialog
                {
                    OverwritePrompt = true,
                    AddExtension = true,
                    DefaultExt = ".lkey"
                };

                if (selectedKey.KeyId != null)
                {
                    saveFileDialog.FileName = Utilities.BinaryToHex(selectedKey.KeyId);
                }
                var result = saveFileDialog.ShowDialog();
                if (result == true)
                {
                    var filename = saveFileDialog.FileName;
                    try
                    {
                        var win = new PasswordInputViewModel {DisplayName = "Enter a new protection password"};
                        dynamic settings = new ExpandoObject();
                        settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        settings.Owner = GetView();
                        var inputOk = _windowManager.ShowDialog(win, null, settings);
                        if (inputOk == true)
                        {
                            password = win.UserPassword;
                            if (!string.IsNullOrEmpty(password))
                            {
                                if (selectedKey.KeySalt == null)
                                {
                                    selectedKey.KeySalt = SodiumCore.GetRandomBytes(32);
                                }
                                var encryptionKey = await Task.Run(() =>
                                {
                                    var tmpKey = new byte[32];
                                    try
                                    {
                                        tmpKey = PasswordHash.ScryptHashBinary(Encoding.UTF8.GetBytes(password),
                                            selectedKey.KeySalt, PasswordHash.Strength.MediumSlow, 32);
                                    }
                                    catch (OutOfMemoryException)
                                    {
                                        MainViewError = "Could not export key (out of memory).";
                                    }
                                    catch (Exception)
                                    {
                                        MainViewError = "Could not export key.";
                                    }
                                    return tmpKey;
                                }).ConfigureAwait(true);

                                selectedKey.PrivateKey = CryptoHelper.Xor(selectedKey.PrivateKey, encryptionKey);
                                using (var keyFileStream = File.OpenWrite(filename))
                                {
                                    Serializer.SerializeWithLengthPrefix(keyFileStream, selectedKey, PrefixStyle.Base128,
                                        0);
                                }
                            }
                            else
                            {
                                MainViewError = "Could not export key (missing password)";
                            }
                        }
                    }
                    catch (Exception)
                    {
                        MainViewError = "Could not export key (failure)";
                    }
                }
                else
                {
                    MainViewError = "Could not export key (missing password)";
                }
            }
            IsWorking = false;
        }

        public async void LoadKeyIntoKeystore()
        {
            MainViewError = string.Empty;
            IsWorking = true;
            string password;
            var openFileDialog = new OpenFileDialog
            {
                DefaultExt = ".lkey",
                Filter = "lkey files (.lkey)|*.lkey",
                Multiselect = false
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                var filename = openFileDialog.FileName;
                try
                {
                    var win = new PasswordInputViewModel {DisplayName = "Enter key password"};
                    dynamic settings = new ExpandoObject();
                    settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    settings.Owner = GetView();
                    var inputOk = _windowManager.ShowDialog(win, null, settings);
                    if (inputOk == true)
                    {
                        password = win.UserPassword;
                        if (!string.IsNullOrEmpty(password))
                        {
                            Key key;
                            using (var keyFileStream = File.OpenRead(filename))
                            {
                                key = Serializer.DeserializeWithLengthPrefix<Key>(keyFileStream, PrefixStyle.Base128, 0);
                            }

                            var decryptionKey = await Task.Run(() =>
                            {
                                var tmpKey = new byte[32];
                                try
                                {
                                    tmpKey =
                                        PasswordHash.ScryptHashBinary(Encoding.UTF8.GetBytes(password),
                                            key.KeySalt, PasswordHash.Strength.MediumSlow, 32);
                                }
                                catch (OutOfMemoryException)
                                {
                                    MainViewError = "Could not load key (out of memory).";
                                }
                                catch (Exception)
                                {
                                    MainViewError = "Could not load key (failure)";
                                }
                                return tmpKey;
                            }).ConfigureAwait(true);

                            key.PrivateKey = CryptoHelper.Xor(key.PrivateKey, decryptionKey);
                            if (key != null)
                            {
                                AddKey(key);
                            }
                        }
                        else
                        {
                            MainViewError = "Could not load key (missing password)";
                        }
                    }
                    else
                    {
                        MainViewError = "Could not load key (missing password)";
                    }
                }
                catch (Exception)
                {
                }
            }
            IsWorking = false;
        }

        /// <summary>
        /// Open the key calculation dialog.
        /// </summary>
        public void CalculateKey()
        {
            var win = new KeyCalculationViewModel{DisplayName = "Calculate a new key"};
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.Owner = GetView();
            var inputOk = _windowManager.ShowDialog(win, null, settings);
            if (inputOk == true)
            {
                if (win.CalculatedKey != null)
                {
                    AddKey(win.CalculatedKey);
                }
            }
        }

        public async void LoadMinisignKeyIntoKeystore()
        {
            MainViewError = string.Empty;
            IsWorking = true;
            string filename;
            string password;
            var openFileDialog = new OpenFileDialog
            {
                DefaultExt = ".key",
                Filter = "Minisign private key (.key)|*.key",
                Multiselect = false
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                filename = openFileDialog.FileName;
                try
                {
                    var win = new PasswordInputViewModel {DisplayName = "Enter minisign password"};
                    dynamic settings = new ExpandoObject();
                    settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    settings.Owner = GetView();
                    var inputOk = _windowManager.ShowDialog(win, null, settings);
                    if (inputOk == true)
                    {
                        password = win.UserPassword;
                        if (!string.IsNullOrEmpty(password))
                        {
                            var newKey = await Task.Run(() =>
                            {
                                var calculatedKey = new Key();
                                try
                                {
                                    calculatedKey.KeyType = KeyType.Minisign;
                                    var keyPair = Minisign.LoadPrivateKeyFromFile(filename, password);
                                    calculatedKey.PublicKey = keyPair.PublicKey;
                                    calculatedKey.PrivateKey = keyPair.SecretKey;
                                    calculatedKey.KeyId = keyPair.KeyId;
                                }
                                catch (OutOfMemoryException)
                                {
                                    MainViewError = "Could not load minisign key (out of memory).";
                                }
                                catch (Exception)
                                {
                                    MainViewError = "Could not load minisign key.";
                                }
                                return calculatedKey;
                            }).ConfigureAwait(true);

                            if (newKey != null)
                            {
                                if (newKey.PrivateKey != null)
                                {
                                    AddKey(newKey);
                                }
                            }
                        }
                        else
                        {
                            MainViewError = "Could not load minisign key (missing password)";
                        }
                    }
                }
                catch (Exception)
                {
                    MainViewError = "Could not load minisign key (failure)";
                }
            }
            else
            {
                MainViewError = "Could not load minisign key (missing password)";
            }
            IsWorking = false;
        }

        /// <summary>
        ///     Copy the selected key to clipboard.
        /// </summary>
        public void CopyKeyToClipboard()
        {
            MainViewError = string.Empty;
            if (SelectedKey != null)
            {
                var selectedKey = SelectedKey;
                var keyId = (selectedKey.KeyId != null) ? Utilities.BinaryToHex(selectedKey.KeyId) : "no key id";
                var publicKey = (selectedKey.PublicKey != null)
                    ? Utilities.BinaryToHex(selectedKey.PublicKey)
                    : "no public key";
                var privateKey = (selectedKey.PrivateKey != null)
                    ? Utilities.BinaryToHex(selectedKey.PrivateKey)
                    : "no private id";
                var clipboard = string.Format("Key Format: {0}\nKey ID: {1}\nPublic Key: {2}\nPrivateKey: {3}",
                    selectedKey.KeyType, keyId, publicKey,
                    privateKey);
                Clipboard.SetText(clipboard);
            }
        }


        /// <summary>
        ///     Generate a new random key.
        /// </summary>
        /// <returns>A random key.</returns>
        private static Key GenerateNewKey()
        {
            var keyPair = PublicKeyBox.GenerateKeyPair();
            var key = new Key
            {
                KeyType = KeyType.Lageant,
                PrivateKey = keyPair.PrivateKey,
                PublicKey = keyPair.PublicKey,
                KeyId = SodiumCore.GetRandomBytes(8),
                KeySalt = PasswordHash.GenerateSalt()
            };
            return key;
        }

        private void AddKey(Key key)
        {
            _lageantCore.AddKey(key);
            _keys.Add(key);
        }

        private void RemoveKey(Key key)
        {
            _lageantCore.RemoveKey(key);
            _keys.Remove(key);
        }


        private string _userPassword;
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
    }
}