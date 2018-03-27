using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WinRtContactManager = Windows.ApplicationModel.Contacts.ContactManager;
using WinRtContact = Windows.ApplicationModel.Contacts.Contact;
using WinRtPhoneNumber = Windows.ApplicationModel.Contacts.ContactPhone;
using WinRtPhoneNumberKind = Windows.ApplicationModel.Contacts.ContactPhoneKind;
using WinRtAccount = Windows.ApplicationModel.Contacts.ContactConnectedServiceAccount;
using WinRtEmail = Windows.ApplicationModel.Contacts.ContactEmail;
using WinRtEmailKind = Windows.ApplicationModel.Contacts.ContactEmailKind;
using WinRtAddress = Windows.ApplicationModel.Contacts.ContactAddress;
using WinRtAddressKind = Windows.ApplicationModel.Contacts.ContactAddressKind;
using WinRtJobInfo = Windows.ApplicationModel.Contacts.ContactJobInfo;
using WinRtDate = Windows.ApplicationModel.Contacts.ContactDate;
using WinRtDateKind = Windows.ApplicationModel.Contacts.ContactDateKind;
using System.Diagnostics;

namespace Contacts
{
    namespace ContactClasses
    {
        public partial class Account
        {
            public StorageKind Kind { get; internal set; }
            public string Name { get; internal set; }
        }

        public partial class ContactAddress
        {
            public IEnumerable<Account> Accounts { get; internal set; }
            public AddressKind Kind { get; internal set; }
            //public CivicAddress PhysicalAddress { get; internal set; }
        }

        public partial class ContactEmailAddress
        {
            public IEnumerable<Account> Accounts { get; internal set; }
            public string EmailAddress { get; internal set; }
            public EmailAddressKind Kind { get; internal set; }
        }

        public partial class ContactPhoneNumber
        {
            public IEnumerable<Account> Accounts { get; internal set; }
            public PhoneNumberKind Kind { get; internal set; }
            public string PhoneNumber { get; internal set; }
        }

        public partial class ContactCompanyInformation
        {
            public IEnumerable<Account> Accounts { get; internal set; }
            public string CompanyName { get; internal set; }
            public string JobTitle { get; internal set; }
            public string OfficeLocation { get; internal set; }
            public string YomiCompanyName { get; internal set; }
        }

        public partial class CompleteName
        {
            public string FirstName { get; internal set; }
            public string LastName { get; internal set; }
            public string MiddleName { get; internal set; }
            public string Nickname { get; internal set; }
            public string Suffix { get; internal set; }
            public string Title { get; internal set; }
            public string YomiFirstName { get; internal set; }
            public string YomiLastName { get; internal set; }
        }

        public static class Extensions
        {
            public static IEnumerable<Q> SafeSelect<P, Q>(this IEnumerable<P> source, Func<P, Q> map)
            {
                if (source == null)
                    return null;
                return source.Select(map);
            }

            public static IEnumerable<P> SafeWhere<P>(this IEnumerable<P> source, Func<P, bool> map)
            {
                if (source == null)
                    return null;
                return source.Where(map);
            }

            public static Q WrapIfNonNull<P, Q>(this P p, Func<P, Q> map)
            {
                if (p == null)
                    return default(Q);
                return map(p);
            }
        }

        public interface Contact
        {
            IEnumerable<Account> Accounts { get; }

            string DisplayName { get; }

            CompleteName CompleteName { get; }

            IEnumerable<DateTime> Birthdays { get; }

            IEnumerable<ContactPhoneNumber> PhoneNumbers { get; }

            IEnumerable<ContactEmailAddress> EmailAddresses { get; }

            IEnumerable<ContactAddress> Addresses { get; }

            IEnumerable<ContactCompanyInformation> Companies { get; }

            IEnumerable<string> Websites { get; }

            IEnumerable<string> Notes { get; }

            // TODO: this API sucks.
            Stream GetPicture();

            string GetIdentifier();
        }

        // NB: this must be binary compatible with Microsoft.Phone.UserData.PhoneNumberKind, because
        // we store these values in the database.
        //
        public enum PhoneNumberKind
        {
            Mobile = 0,
            Home = 1,
            Work = 2,
            Company = 3,
            Pager = 4,
            HomeFax = 5,
            WorkFax = 6,

            // NB: Values outside the "legacy" ones are safe to put after this point:
            Other = 0xffff,    // Introduced by WinRT
        };

        public enum EmailAddressKind
        {
            Personal = 0,
            Work = 1,
            Other = 2,
        }

        public enum AddressKind
        {
            Home = 0,
            Work = 1,
            Other = 2,
        }

        public enum StorageKind
        {
            Phone = 0,
            WindowsLive = 1,
            Outlook = 2,
            Facebook = 3,
            Other = 4,
        }

        public class AddressBookSearchArgs : EventArgs
        {
            public IEnumerable<Contact> Results;
        }

        public class WinRtContacts
        {
            private async Task<AddressBookSearchArgs> GetContactByIdAsync(string id)
            {
                var contacts = await WinRtContactManager.RequestStoreAsync();
                var result = await contacts.GetContactAsync(id);
                return WrapContacts(result);
            }

            public async Task<AddressBookSearchArgs> GetContactByPhoneNumberAsync(string number)
            {
                var contacts = await WinRtContactManager.RequestStoreAsync();

                // ISSUE: On 8.1 we cannot search by phone number, only by all fields in the contact.
                // on 10 we should use GetContactReader to specify a phone number match
                //
                number = new string(number.Where(c => Char.IsDigit(c)).ToArray());
                IEnumerable<WinRtContact> result = await contacts.FindContactsAsync(number);
                return WrapContacts(result);
            }

            public async Task<AddressBookSearchArgs> GetAllContactsAsync()
            {
                var contacts = await WinRtContactManager.RequestStoreAsync();
                if (contacts == null)
                {
                    Debug.WriteLine("contacts-ContactsManager returned null!");
                    return ErrorResult();
                }
                var result = await contacts.FindContactsAsync();
                if (result == null)
                {
                    Debug.WriteLine("contacts-FindContactsAsync returned null!");
                    return ErrorResult();
                }
                return WrapContacts(result);
            }

            private AddressBookSearchArgs ErrorResult()
            {
                return new AddressBookSearchArgs() { Results = new Contact[0] };
            }

            private AddressBookSearchArgs WrapContacts(IEnumerable<WinRtContact> args)
            {
                return new AddressBookSearchArgs() { Results = (args ?? new WinRtContact[0]).Where(a => a != null).Select(WrapContact) };
            }
            private AddressBookSearchArgs WrapContacts(params WinRtContact[] args)
            {
                return WrapContacts(args.AsEnumerable());
            }
            private Contact WrapContact(WinRtContact contact)
            {
                return contact != null ? new ContactImplWinRt(contact) : null;
            }

            private class ContactImplWinRt : Contact
            {
                private WinRtContact contact;

                public ContactImplWinRt(WinRtContact contact)
                {
                    this.contact = contact;
                }

                public string GetIdentifier()
                {
                    return contact.Id;
                }

                public IEnumerable<Account> Accounts { get { return contact.ConnectedServiceAccounts.SafeSelect(AccountImpl.Create).SafeWhere(a => a != null); } }

                public string DisplayName { get { return contact.DisplayName; } }

                public CompleteName CompleteName { get { return CompleteNameImpl.Create(contact); } }

                public IEnumerable<DateTime> Birthdays
                {
                    get
                    {
                        return contact.ImportantDates.SafeSelect(
                            w => (w?.Kind == WinRtDateKind.Birthday) ? ContactDateToDateTime(w) : null
                        ).SafeWhere(w => w != null).SafeSelect(a => a.Value);
                    }
                }

                public IEnumerable<ContactPhoneNumber> PhoneNumbers { get { return contact.Phones.SafeSelect(ContactPhoneNumberImpl.Create).SafeWhere(a => a != null); } }

                public IEnumerable<ContactEmailAddress> EmailAddresses { get { return contact.Emails.SafeSelect(ContactEmailAddressImpl.Create).SafeWhere(a => a != null); } }

                public IEnumerable<ContactAddress> Addresses { get { return contact.Addresses.SafeSelect(ContactAddressImpl.Create).SafeWhere(a => a != null); } }

                public IEnumerable<ContactCompanyInformation> Companies { get { return contact.JobInfo.SafeSelect(ContactCompanyImpl.Create).SafeWhere(a => a != null); } }

                public IEnumerable<string> Websites
                {
                    get
                    {
                        return contact.Websites.SafeSelect(w => w?.Uri?.OriginalString).SafeWhere(w => w != null);
                    }
                }
                public IEnumerable<string> Notes { get { if (contact.Notes == null) return null; return new[] { contact.Notes }; } }

                public Stream GetPicture()
                {
                    //var winRtStream = contact.Thumbnail;
                    //if (winRtStream == null)
                    //    return null;

                    //// XXX this is extremely ugly, but all of our callers want synchronous I/O right now.
                    //Stream r = null;
                    //var streamObs = winRtStream.OpenReadAsync().AsTask().ToObservable().SubscribeOn(Scheduler.ThreadPool);
                    //Action @throw = null;
                    //streamObs.Run(
                    //    s => r = s.AsStream(),
                    //    ex => @throw = ex.GetRethrowAction()
                    // );
                    //if (@throw != null)
                    //{
                    //    r.SafeDispose();
                    //    r = null;
                    //    @throw();
                    //}
                    return null;
                }

                private DateTime? ContactDateToDateTime(WinRtDate c)
                {
                    try
                    {
                        var date = new DateTime();

                        if (c.Year != null)
                        {
                            date = date.AddYears((int)c.Year - 1);
                        }
                        if (c.Day != null)
                        {
                            date = date.AddDays((int)c.Day - 1);
                        }
                        if (c.Month != null)
                        {
                            date = date.AddMonths((int)c.Month - 1);
                        }

                        return date;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message + ":Failed parsing contact date");
                    }
                    return null;
                }
                private class ContactCompanyImpl : ContactCompanyInformation
                {
                    public static ContactCompanyInformation Create(WinRtJobInfo a)
                    {
                        if (a == null)
                            return null;

                        return new ContactCompanyInformation()
                        {
                            CompanyName = a.CompanyName,
                            JobTitle = a.Title,
                            OfficeLocation = a.Office,
                            YomiCompanyName = a.CompanyYomiName,
                            Accounts = new Account[0] //TODO
                        };
                    }
                }

                private class AccountImpl : Account
                {
                    public static Account Create(WinRtAccount a)
                    {
                        if (a == null)
                            return null;

                        return new Account()
                        {
                            Name = a.ServiceName
                            //TODO Kind is not added yet
                        };
                    }
                }

                private class ContactAddressImpl : ContactEmailAddress
                {
                    public static ContactAddress Create(WinRtAddress a)
                    {
                        if (a == null)
                            return null;

                        return new ContactAddress()
                        {
                            //PhysicalAddress = new CivicAddress()
                            //{
                            //    AddressLine1 = a.StreetAddress,
                            //    City = a.Locality,
                            //    CountryRegion = a.Country,
                            //    PostalCode = a.PostalCode,
                            //    StateProvince = a.Region
                            //},
                            Kind = ConvertAddressKind(a.Kind),
                            Accounts = new Account[0]  //TODO
                        };
                    }

                    private static AddressKind ConvertAddressKind(WinRtAddressKind a)
                    {
                        switch (a)
                        {
                            case WinRtAddressKind.Home:
                                return AddressKind.Home;
                            case WinRtAddressKind.Work:
                                return AddressKind.Work;
                            case WinRtAddressKind.Other:
                            default:
                                return AddressKind.Other;
                        }
                    }
                }

                private class ContactEmailAddressImpl : ContactEmailAddress
                {
                    public static ContactEmailAddress Create(WinRtEmail a)
                    {
                        if (a == null)
                            return null;

                        return new ContactEmailAddress()
                        {
                            EmailAddress = a.Address,
                            Kind = ConvertEmailKind(a.Kind),
                            Accounts = new Account[0]  //TODO
                        };
                    }

                    private static EmailAddressKind ConvertEmailKind(WinRtEmailKind a)
                    {
                        switch (a)
                        {
                            case WinRtEmailKind.Personal:
                                return EmailAddressKind.Personal;
                            case WinRtEmailKind.Work:
                                return EmailAddressKind.Work;
                            case WinRtEmailKind.Other:
                            default:
                                return EmailAddressKind.Other;
                        }
                    }
                }
                private class CompleteNameImpl : CompleteName
                {
                    public static CompleteName Create(WinRtContact a)
                    {
                        return new CompleteName()
                        {
                            FirstName = a.FirstName,
                            LastName = a.LastName,
                            MiddleName = a.MiddleName,
#if false
                            Nickname = a.Nickname,
                            Suffix = a.Suffix,
                            Title = a.Title,
#endif
                            YomiFirstName = a.YomiGivenName,
                            YomiLastName = a.YomiFamilyName
                        };
                    }
                }

                private class ContactPhoneNumberImpl : ContactPhoneNumber
                {
                    public static ContactPhoneNumber Create(WinRtPhoneNumber a)
                    {
                        if (a == null)
                            return null;

                        return new ContactPhoneNumber()
                        {
                            Accounts = new Account[0],
                            Kind = ConvertPhoneNumberKind(a.Kind),
                            PhoneNumber = a.Number,
                        };
                    }
                    private static PhoneNumberKind ConvertPhoneNumberKind(WinRtPhoneNumberKind a)
                    {
                        switch (a)
                        {
                            case WinRtPhoneNumberKind.Home:
                                return PhoneNumberKind.Home;
                            case WinRtPhoneNumberKind.Mobile:
                                return PhoneNumberKind.Mobile;
                            case WinRtPhoneNumberKind.Work:
                                return PhoneNumberKind.Work;
                            case WinRtPhoneNumberKind.Other:
                            default:
                                return PhoneNumberKind.Other;
                        }
                    }
                }
            }

            #region IAddressBook wrappers
            //public IObservable<AddressBookSearchArgs> GetContactById(string id)
            //{
            //    return GetContactByIdAsync(id).ToObservable();
            //}
            //public IObservable<AddressBookSearchArgs> GetContactByPhoneNumber(string number)
            //{
            //    return GetContactByPhoneNumberAsync(number).ToObservable();
            //}
            //public IObservable<AddressBookSearchArgs> GetAllContacts()
            //{
            //    return GetAllContactsAsync().ToObservable();
            //}
            #endregion
        }
    }
}
