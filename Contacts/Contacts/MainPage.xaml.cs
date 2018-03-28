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

using Windows.ApplicationModel.Contacts;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Popups;
using System.Diagnostics;
using Windows.UI.ViewManagement;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Contacts
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public ContactStore contactStore = null;

        public MainPage()
        {
            this.InitializeComponent();
            

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var args = e.Parameter as ProtocolActivatedEventArgs;
            // Display the result of the protocol activation if we got here as a result of being activated for a protocol.

            if (args != null)
            {
                var options = new Windows.System.LauncherOptions();
                options.DisplayApplicationPicker = true;

                options.TargetApplicationPackageFamilyName = "WhatsApp";

                string launchString = args.Uri.Scheme + ":" + args.Uri.Query;
                var launchUri = new Uri(launchString);
                await Windows.System.Launcher.LaunchUriAsync(launchUri, options);
            }
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

        private async void CreateContact_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.Contacts.Contact contact = new Windows.ApplicationModel.Contacts.Contact();
            contact.FirstName = "TestContactWhatsApp";

            Windows.ApplicationModel.Contacts.ContactEmail email = new Windows.ApplicationModel.Contacts.ContactEmail();
            email.Address = "TestContact@whatsapp.com";
            email.Kind = Windows.ApplicationModel.Contacts.ContactEmailKind.Other;
            contact.Emails.Add(email);

            Windows.ApplicationModel.Contacts.ContactPhone phone = new Windows.ApplicationModel.Contacts.ContactPhone();
            phone.Number = "4255550101";
            phone.Kind = Windows.ApplicationModel.Contacts.ContactPhoneKind.Mobile;
            contact.Phones.Add(phone);

            Windows.ApplicationModel.Contacts.ContactStore store = await
                ContactManager.RequestStoreAsync(ContactStoreAccessType.AppContactsReadWrite);

            ContactList contactList;

            IReadOnlyList<ContactList> contactLists = await store.FindContactListsAsync();

            if (0 == contactLists.Count)
                contactList = await store.CreateContactListAsync("WhatsAppContactList");
            else
                contactList = contactLists[0];

            await contactList.SaveContactAsync(contact);

            //Add Annotation
            ContactAnnotationStore annotationStore = await ContactManager.RequestAnnotationStoreAsync(ContactAnnotationStoreAccessType.AppAnnotationsReadWrite);

            ContactAnnotationList annotationList;

            IReadOnlyList<ContactAnnotationList> annotationLists = await annotationStore.FindAnnotationListsAsync();
            if (0 == annotationLists.Count)
                annotationList = await annotationStore.CreateAnnotationListAsync();
            else
                annotationList = annotationLists[0];


            ContactAnnotation annotation = new ContactAnnotation();
            annotation.ContactId = contact.Id;
            annotation.RemoteId = phone.Number; //associate the ID of a contact to an ID that your app uses internally to identify that user.

            annotation.SupportedOperations = ContactAnnotationOperations.Message |
              ContactAnnotationOperations.AudioCall |
              ContactAnnotationOperations.VideoCall |
             ContactAnnotationOperations.ContactProfile;

            
            await annotationList.TrySaveAnnotationAsync(annotation);
        }

        private async Task<Windows.ApplicationModel.Contacts.Contact> findContact(string emailAddress)
        {
            if (contactStore == null)
            {
                contactStore = await ContactManager.RequestStoreAsync();
                contactStore.ContactChanged += ContactStore_ContactChanged;
                contactStore.ChangeTracker.Enable();
            }
            

            IReadOnlyList<Windows.ApplicationModel.Contacts.Contact> contacts = null;

            contacts = await contactStore.FindContactsAsync(emailAddress);

            Windows.ApplicationModel.Contacts.Contact contact = contacts[0];
                

            return contact;
        }

        private async void OpenContact_Click(object sender, RoutedEventArgs e)
        {
            

            // Get the selection rect of the button pressed to show contact card.
            FrameworkElement element = (FrameworkElement)sender;

            Windows.UI.Xaml.Media.GeneralTransform buttonTransform = element.TransformToVisual(null);
            Windows.Foundation.Point point = buttonTransform.TransformPoint(new Windows.Foundation.Point());
            Windows.Foundation.Rect rect =
                new Windows.Foundation.Rect(point, new Windows.Foundation.Size(element.ActualWidth, element.ActualHeight));

            // helper method to find a contact just for illustrative purposes.
            Windows.ApplicationModel.Contacts.Contact contact = await findContact("TestContact@whatsapp.com");


            //Show Contact in contact Application
            FullContactCardOptions options = new FullContactCardOptions();
            options.DesiredRemainingView = ViewSizePreference.UseHalf;

            // Show the full contact card.
            ContactManager.ShowFullContactCard(contact, options);

        }

        private async void ContactStore_ContactChanged(ContactStore sender, ContactChangedEventArgs args)
        {
            //while this function is open, we won't fire additional change events
            //this allows you to operate on the changes without worrying about simultaneos trackers being open
            //holding the defferal is necessary because of async'ness 
            var defferal = args.GetDeferral();

            ContactChangeReader reader = sender.ChangeTracker.GetChangeReader();
            IReadOnlyList<ContactChange> changes = await reader.ReadBatchAsync();
            while (changes.Count != 0)
            {
                foreach (ContactChange change in changes)
                {
                    switch (change.ChangeType)
                    {
                        case ContactChangeType.Created:
                            Debug.WriteLine("[created]" + change.Contact.Id + ": " + change.Contact.DisplayName);
                            break;

                        case ContactChangeType.Deleted:
                            Debug.WriteLine("[deleted]" + change.Contact.Id + ": " + change.Contact.DisplayName);
                            break;

                        case ContactChangeType.Modified:
                            Debug.WriteLine("[modified]" + change.Contact.Id + ": " + change.Contact.DisplayName);
                            break;

                        case ContactChangeType.ChangeTrackingLost:

                            MessageDialog lostChangeTrackingWarning =
                                new MessageDialog("The system has lost the change tracking information. " +
                                    "This shouldn't happen very often, but you should handle the case. " +
                                    "The expectation is that you'll re-read all of the contacts after this.",
                                    "Change tracking lost");
                            await lostChangeTrackingWarning.ShowAsync();

                            defferal.Complete();
                            //Returning since changes are no longer going to be valid after resetting
                            //the change tracking 
                            return;

                        default:
                            //No-op on the default case for future proofing
                            break;
                    }
                }
                changes = await reader.ReadBatchAsync();
            }

            //Let the system know that we have processed all the changes so the same changes aren't surfaced again
            reader.AcceptChanges();

            defferal.Complete();

        }
    }
}
