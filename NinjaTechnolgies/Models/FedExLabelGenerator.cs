using NinjaTechnolgies.FedExShipServiceWebReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NinjaTechnolgies.Models
{
    public class FedExLabelGenerator
    {
        static Party ReturnParty = new Party()
        {
            Contact = new Contact()
            {
                CompanyName = "Ninja Technologies",
                PersonName = "Benjamin Ballard",
                PhoneNumber = "4058885800"
            },
            Address = new Address()
            {
                StreetLines = new string[1] { "401 Woodhollow Trl" },
                City = "Edmond",
                StateOrProvinceCode = "OK",
                PostalCode = "73012",
                CountryCode = "US"
            },
            AccountNumber = "510087984"
        };

        public static ProcessShipmentRequest CreateRequest(ContactInfo contact)
        {
            return CreateRequest(contact.CompanyName, contact.ContactName, contact.Address1, contact.Address2, contact.City, contact.State, contact.Zip, contact.Phone, contact.RefID);
        }

        static ProcessShipmentRequest CreateRequest(string CompanyName, string ContactName, string Address1, string Address2, string City, string State, string Zip, string Phone, string Ref)
        {
            var request = new ProcessShipmentRequest();
            //todo add credintials from config
            request.WebAuthenticationDetail = new WebAuthenticationDetail()
            {
                UserCredential = new WebAuthenticationCredential()
                {
                    Key = "",
                    Password = ""
                }
            };

            request.ClientDetail = new ClientDetail()
            {
                AccountNumber = "",
                MeterNumber = ""
            };

            request.TransactionDetail = new TransactionDetail()
            {
                CustomerTransactionId = "***Ground Domestic Ship Request***"
            };

            request.Version = new VersionId();

            //set shipment details
            request.RequestedShipment = new RequestedShipment()
            {
                ShipTimestamp = DateTime.Now,
                ServiceType = ServiceType.FEDEX_GROUND,
                PackagingType = PackagingType.YOUR_PACKAGING,
                PackageCount = "1"
            };

            //set sender details
            request.RequestedShipment.Shipper = new Party()
            {
                Contact = new Contact()
                {
                    PersonName = ContactName,
                    CompanyName = CompanyName,
                    PhoneNumber = Phone
                },
                Address = new Address()
                {
                    StreetLines = new string[2] { Address1, Address2 },
                    City = City,
                    StateOrProvinceCode = State,
                    PostalCode = Zip,
                    CountryCode = "US"
                }
            };

            //set recipient details
            request.RequestedShipment.Recipient = ReturnParty;

            //set payment details
            request.RequestedShipment.ShippingChargesPayment = new Payment()
            {
                PaymentType = PaymentType.SENDER,
                Payor = new Payor()
                {
                    ResponsibleParty = ReturnParty
                }
            };

            //set label deails
            request.RequestedShipment.LabelSpecification = new LabelSpecification()
            {
                ImageType = ShippingDocumentImageType.PDF,
                ImageTypeSpecified = true,
                LabelFormatType = LabelFormatType.COMMON2D,
                LabelStockType = LabelStockType.PAPER_85X11_BOTTOM_HALF_LABEL
            };

            request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[1]{
                new RequestedPackageLineItem()
                {
                    SequenceNumber = "1",
                    Weight = new Weight()
                    {
                        Value = 2.0M,
                        Units = WeightUnits.LB
                    },
                     CustomerReferences = new CustomerReference[1]
                     {
                         new CustomerReference(){
                             CustomerReferenceType = CustomerReferenceType.CUSTOMER_REFERENCE,
                             Value = Ref
                         }
                     }
                }
            };

            request.RequestedShipment.SpecialServicesRequested = new ShipmentSpecialServicesRequested()
            {
                ReturnShipmentDetail = new ReturnShipmentDetail()
                {
                    ReturnType = ReturnType.PRINT_RETURN_LABEL,
                },
                SpecialServiceTypes = new ShipmentSpecialServiceType[1] { 
                    ShipmentSpecialServiceType.RETURN_SHIPMENT
                }
            };
            /*
            request.RequestedShipment.ShippingDocumentSpecification = new ShippingDocumentSpecification()
            {
                ReturnInstructionsDetail = new ReturnInstructionsDetail()
                {
                    Format = new ShippingDocumentFormat()
                    {
                        ImageType = ShippingDocumentImageType.PDF,
                        ProvideInstructions = true
                    }
                },
                ShippingDocumentTypes = new RequestedShippingDocumentType[2]{
                    RequestedShippingDocumentType.RETURN_INSTRUCTIONS,
                    RequestedShippingDocumentType.LABEL
                }
            };
            */
            return request;
        }
    }
}