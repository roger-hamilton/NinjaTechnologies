using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Xml;
using System.Threading.Tasks;
using System.Data;
using NinjaTechnolgies.MethodIntegration;

namespace NinjaTechnolgies.Models
{
    public class ContactInfo
    {
        public string ContactName { get; set; }
        public string CompanyName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string ContryCode { get; set; }
        public string Phone { get; set; }
        public string RefID { get; set; }
    }

    public class MethodData
    {
        public static void Test(string username,string password)
        {
            Service service = new Service();
            string xml = service.MethodAPIFieldListV2(CompanyAccount, username, password, "", "DocumentLibrary");
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            var rows = doc.DocumentElement.ChildNodes[0].ChildNodes;
        }
        #region StateAbbr
        static Dictionary<string, string> StateAbbr = new Dictionary<string, string>()
        {
            {"alabama",	"AL"},
            {"alaska",	"AK"},
            {"arizona",	"AZ"},
            {"arkansas",	"AR"},
            {"california",	"CA"},
            {"colorado",	"CO"},
            {"connecticut",	"CT"},
            {"delaware",	"DE"},
            {"florida",	"FL"},
            {"georgia",	"GA"},
            {"hawaii",	"HI"},
            {"idaho",	"ID"},
            {"illinois",	"IL"},
            {"indiana",	"IN"},
            {"iowa",	"IA"},
            {"kansas",	"KS"},
            {"kentucky",	"KY"},
            {"louisiana",	"LA"},
            {"maine",	"ME"},
            {"maryland",	"MD"},
            {"massachusetts",	"MA"},
            {"michigan",	"MI"},
            {"minnesota",	"MN"},
            {"mississippi",	"MS"},
            {"missouri",	"MO"},
            {"montana",	"MT"},
            {"nebraska",	"NE"},
            {"nevada",	"NV"},
            {"new hampshire",	"NH"},
            {"new jersey",	"NJ"},
            {"new mexico",	"NM"},
            {"new york",	"NY"},
            {"north carolina",	"NC"},
            {"north dakota",	"ND"},
            {"ohio",	"OH"},
            {"oklahoma",	"OK"},
            {"oregon",	"OR"},
            {"pennsylvania",	"PA"},
            {"rhode island",	"RI"},
            {"south carolina",	"SC"},
            {"south dakota",	"SD"},
            {"tennessee",	"TN"},
            {"texas",	"TX"},
            {"utah",	"UT"},
            {"vermont",	"VT"},
            {"virginia",	"VA"},
            {"washington",	"WA"},
            {"west virginia",	"WV"},
            {"wisconsin",	"WI"},
            {"wyoming",	"WY"},
        };
        #endregion
        static string CompanyAccount = "NinjaTechnologies";

        public static bool ValidLogin(string username,string password)
        {
            Service MethodService = new Service();
            string reply = null;
            int retries = 5;
            int attemt = 0;
            while (attemt++ < retries)
            {
                reply = MethodService.MethodAPIFieldListV2(CompanyAccount, username, password, "", "Vendor");
                if (reply.IndexOf("Failure. The API could not validate your log in.") < 0)
                    return true;
            }

            return false;
        }

        public static ContactInfo GetContact(string RefID,string username,string password)
        {

            string Fields = string.Join(",",
                "FirstName",
                "LastName",
                "CompanyName",
                "VendorAddressAddr1",
                "VendorAddressAddr2",
                "VendorAddressCity",
                "VendorAddressState",
                "VendorAddressPostalCode",
                "VendorAddressCountry",
                "Phone",
                "DirectPhone"
            );
            string WhereClause = "RecordID=" + RefID;

            Service MethodService = new Service();

            DataSet RecordInfo = null;

            int retries = 5;
            int attemt = 0;
            while(attemt++<retries && RecordInfo == null)
            {
                MethodService.MethodAPISelect_DataSetV2(CompanyAccount, username, password, "", ref RecordInfo, "Vendor", Fields, WhereClause, "", "", "");
            }
            if (RecordInfo == null)
                return null;

            var record = RecordInfo.Tables["Record"].Rows[0];

            ContactInfo contact = new ContactInfo()
            {
                ContactName = (record["FirstName"] as string) + " " + (record["LastName"] as string),
                CompanyName = record["CompanyName"] as string,
                Address1 = record["VendorAddressAddr1"] as string,
                Address2 = record["VendorAddressAddr2"] as string,
                City = record["VendorAddressCity"] as string,
                State = record["VendorAddressState"] as string,
                Zip = record["VendorAddressPostalCode"] as string,
                ContryCode = record["VendorAddressCountry"] as string,
                Phone = record["Phone"] as string,
                RefID = RefID
            };


            if (contact.Phone == null || contact.Phone == "")
                contact.Phone = record["DirectPhone"] as string;

            if (contact.Phone == null || contact.Phone == "")
                contact.Phone = "0000000000";

            if (contact.ContryCode == null || contact.ContryCode == "")
                contact.ContryCode = "US";
            if(contact.State.Length>2)
            {
                var StateCode = StateAbbr[contact.State.ToLower()];
                if (StateCode != null)
                    contact.State = StateCode;
            }

            return contact;
        }

        public static string WritePDFToDocumentStore(string username, string password,string filename, byte[] data)
        {
            Service MethodService = new Service();
            DateTime now = DateTime.Now;
            XmlDocument doc = new XmlDocument();
            
            //number of times to try to reconnect
            int retries = 10;

            int attempts = 0;

            string FullFileName = filename+".pdf";

            string InsertReply = "";
            attempts = 0;

            while (attempts++ < retries && InsertReply.IndexOf("Success") < 0)
                InsertReply = MethodService.MethodAPIInsertV2(CompanyAccount, username, password, "",
                    "DocumentLibrary", new string[] { "AssignedTo", "CreatedBy", "CreatedDate", "DocumentFileName",
                    "Description","IsActive", "IsForInternalUse", "LastModifiedBy", "LastModifiedDate", "Name" },
                    new string[] { username, username, now.ToString(), FullFileName,
                    "Label Created From FedEX Service","Yes", "No", username, now.ToString(), FullFileName});

            if (InsertReply.IndexOf("Success") < 0)
                return null;
                    
            doc.LoadXml(InsertReply);

            var recordID = doc.DocumentElement.Attributes["RecordID"].Value;
            string UploadReply = "";
            attempts = 0;
            while (attempts++ < retries && UploadReply.IndexOf("Success") < 0)
                UploadReply = MethodService.MethodAPIUpdateFile(CompanyAccount, username, password, "",
                    "DocumentLibrary", new string[] { "Document" }, data, FullFileName, recordID);

            if (UploadReply.IndexOf("Success") < 0)
                return null;
            
            return FullFileName;
        }
    }
}