using Contacts.ContactClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Contacts
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void GetAllContact_Click(object sender, RoutedEventArgs e)
        {

            var contacts = new WinRtContacts();
            var listContacts = await contacts.GetAllContactsAsync();
            lbContact.Items.Clear();
            foreach (var item in listContacts.Results)
            {
                lbContact.Items.Add(item.DisplayName);
            }
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            var contacts = new WinRtContacts();
            var adress = await contacts.GetContactByPhoneNumberAsync(tbNumber.Text);
            lbContact.Items.Clear();
            foreach (var item in adress.Results)
            {
                lbContact.Items.Add(item.DisplayName);
            }
        }
    }
}
