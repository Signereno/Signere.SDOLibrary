using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Signere.SDOLibrary
{
    /// <summary>
    /// Object to represent a  signature
    /// </summary>
    public class Signature
    {
        /// <summary>
        /// Name of the person or organization that signed the document
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Timestamp for the signature in UTC
        /// </summary>
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// Have the signature been verified
        /// </summary>
        public bool Verified { get; set; }

        public X509Certificate2 Certificate { get; set; }
    }

    /// <summary>
    /// Object to represent a BankID signature
    /// </summary>
    public class BankIDSignature : Signature
    {
        /// <summary>
        /// What kind of BankID is used, PersonBankID, AnsattBankID or BrukerstedsBankID
        /// </summary>
        public string BankIDtype { get; set; }
        /// <summary>
        /// Name of the Bank issiuing the certificate
        /// </summary>
        public string BankName { get; set; }
        /// <summary>
        /// SDO profile type
        /// </summary>
        public string SDOProfile { get; set; }
        /// <summary>
        ///  Sign policy Description
        /// </summary>
        public string SignPolicy { get; set; }
        /// <summary>
        /// Unique id for the signer. For organization this is the MVA number, for persons this is a unique number across multiple BankIDs (not social security number).
        /// </summary>
        public string UniqueId { get; set; }
        /// <summary>
        /// Sign policy OID (urn:oid:2.16.578.1.16.4.1)
        /// </summary>
        public string SignPolicyOid { get; set; }
    }
    /// <summary>
    /// DocumentType representing different types of document that can be signed with BankID (pdf, xml and text).
    /// </summary>
    public enum DocumentType
    {
        PDF,
        TEXT,
        XML
    }

    /// <summary>
    /// Class to represent the MetaData in the SDO
    /// </summary>
    public class MetaData
    {
        /// <summary>
        /// Meta data key
        /// </summary>
        /// <remarks>MerchantDesc is reserved for the document title by BankID</remarks>
        public string Name { get; set; }
        /// <summary>
        /// Value of the key
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Description of the metadata
        /// </summary>
        public string Description { get; set; }
    }

  
    public static class BankID_Oid
    {

        public static Dictionary<string,string> Oids=new Dictionary<string, string>()
        {
            {   "2.16.578.1.16.1.6.1.1","Soft merchant certificate"},
            {   "2.16.578.1.16.1.6.2.1","HSM merchant certificate"},
            {   "2.16.578.1.16.1.9.1","NetCentric personal certificate"},            
            {   "2.16.578.1.16.1.11.2.1","NetCentric employee certificate"},
            {   "2.16.578.1.16.1.12.1.1","NetCentric Qualified personal certificate"},
            {   "2.16.578.1.16.1.13.1.1","NetCentric Qualified employee certificate"},
            {   "2.16.578.1.16.1.12.2.1","Mobile personal certificate"},
        };


    }
}