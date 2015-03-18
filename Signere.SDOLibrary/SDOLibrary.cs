using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace Signere.SDOLibrary
{
    /// <summary>
    /// Library to parse SEID SDO files
    /// </summary>
    public class SDOLibrary : ISDOLibrary
    {
        private XmlDocument xmlDoc;
        private XmlNamespaceManager nm;
        private ContentInfo DocumentContentInfo;

        /// <summary>
        /// Load SDO from string
        /// </summary>
        /// <param name="sdoXML">SDO XML as string</param>
        public void LoadFromString(string sdoXML)
        {
            var xmlDoctmp = new XmlDocument();
            xmlDoctmp.LoadXml(sdoXML);

            SetupXML(xmlDoctmp);
        }

        /// <summary>
        /// Load SDO from filepath
        /// </summary>
        /// <param name="filepath">Filepath to the SDO file</param>
        public void LoadFromPath(string filepath)
        {
            if(!System.IO.File.Exists(filepath))
                throw new FileNotFoundException("Cannot load file",filepath);

            var xmlDoctmp = new XmlDocument();
            xmlDoctmp.Load(filepath);

            SetupXML(xmlDoctmp);
        }
      
        /// <summary>
        /// Load SDO from bytearray
        /// </summary>
        /// <param name="filedata">Byte array containing the SDO file</param>
        public void LoadFromByteArray(byte[] filedata)
        {
            var xmlDoctmp = new XmlDocument();
            xmlDoctmp.Load(new MemoryStream(filedata));

            SetupXML(xmlDoctmp);
        }

        /// <summary>
        /// Load SDO from stream
        /// </summary>
        /// <param name="stream">Stream containing the SDO</param>
        public void LoadFromStream(Stream stream) {
            var xmlDoctmp = new XmlDocument();
            xmlDoctmp.Load(stream);

            SetupXML(xmlDoctmp);
        }

      
        /// <summary>
        /// Returns the number of signatures in the SDO
        /// </summary>
        public int NumberOfSignatures
        {
            get { return xmlDoc.SelectNodes("/SDOList/SDO/SDODataPart/SignatureElement", nm).Count; }
        }

        /// <summary>
        /// Returns the document description
        /// </summary>
        public string DocumentDescription
        {
            get { return xmlDoc.SelectSingleNode("/SDOList/SDO/Metadata/ValuePair/Value", nm).InnerText; }
        }

        /// <summary>
        /// Returns the SEID SDO Version
        /// </summary>
        public string SEIDSDOVersion
        {
            get { return xmlDoc.SelectSingleNode("/SDOList/SDO/SEIDSDOVersion", nm).InnerText; }
        }

        /// <summary>
        /// Returns a signature element for the SDO Seal with name, timestamp and verfied
        /// </summary>
        public Signature SealedBy
        {
            get
            {
                var signatureNode = xmlDoc.SelectSingleNode("/SDOList/SDO/SDOSeal/SDOSignature/CMSSignatureElement/CMSSignature", nm);
                var sealNode = xmlDoc.SelectSingleNode("/SDOList/SDO/SDODataPart", nm);
                var sealData = Encoding.GetEncoding("ISO-8859-1").GetBytes(sealNode.InnerText);

                ContentInfo SealdContentInfo = new ContentInfo(sealData);
                SignedCms cms = new SignedCms(SealdContentInfo, true);
                cms.Decode(Convert.FromBase64String(signatureNode.InnerText));

                DateTime? sealedTimeStamp = GetSigningTimeFromSignedCMS(cms);
                if (sealedTimeStamp == null)
                {
                    throw new Exception("Not valid timestamp on seal");
                }

                bool verified = true;
                //verified = Verified(cms);
                string certuser = null;
                string certbank = null;
                GetCertificateStringForBankAndForUser(cms, ref certbank, ref certuser);

                return new Signature()
                    {
                        TimeStamp = sealedTimeStamp.Value,
                        Name = certuser.Name(),
                        Verified = verified,
                    };
            }
        }

        /// <summary>
        /// Returns the documenttype xml, pdf or text.
        /// </summary>
        public DocumentType DocumentType {
            get {
                var node =
                    xmlDoc.SelectSingleNode(
                        "/SDOList/SDO/SDODataPart/SignatureElement/CMSSignatureElement/SignersDocumentFormat/MimeType",
                        nm);

                if (node != null)
                    switch (node.InnerText.ToLowerInvariant()) {
                        case "application/pdf":
                            return DocumentType.PDF;
                        case "text/BIDXML":
                            return DocumentType.XML;
                        case "text/plain":
                            return DocumentType.TEXT;
                    }

                throw new Exception("Unknown documenttype");
            }
        }

        /// <summary>
        /// Returns the document that is signed as bytearray
        /// </summary>
        public byte[] SignedDocument {
            get {
                var node = xmlDoc.SelectSingleNode("/SDOList/SDO/SignedObject/SignersDocument", nm);
                if (node == null)
                    throw new Exception("Cannot find SignedDokument node in SDO");
                return Convert.FromBase64String(node.InnerText);
            }
        }

        /// <summary>
        /// Saves the signed document to the given filepath and returns the fullpath to the file
        /// </summary>
        /// <param name="filepath">Filepath to save the signed document, if none given it will be saved in temp</param>
        /// <returns>Full path to the signed document</returns>
        public string SaveSignedDocument(string filepath=null)
        {
            if (string.IsNullOrEmpty(filepath))
                filepath = System.IO.Path.GetTempPath();

            string filename = null;
            switch (DocumentType) {
                case DocumentType.PDF:
                    filename = string.Format("{0}.pdf", Guid.NewGuid().ToString("N"));
                    break;
                case DocumentType.XML:
                    filename = string.Format("{0}.xml", Guid.NewGuid().ToString("N"));
                    break;
                case DocumentType.TEXT:
                    filename = string.Format("{0}.txt", Guid.NewGuid().ToString("N"));
                    break;
            }

            string path = System.IO.Path.Combine(filepath, filename);

            File.WriteAllBytes(path, SignedDocument);

            return path;
        }

        public IEnumerable<MetaData> MetaData { get
        {
            IList<MetaData> list=new List<MetaData>();
            XmlNodeList nodelist = xmlDoc.SelectNodes("/SDOList/SDO/Metadata/ValuePair", nm);
            foreach (XmlNode item in nodelist)
            {
                var innerNode = item.SelectSingleNode("Name");
                string name = item.SelectSingleNode("Name").InnerText;
                string value = item.SelectSingleNode("Value").InnerText;
                string description = item.SelectSingleNode("Description")==null ? null: item.SelectSingleNode("Description").InnerText; 

                list.Add(new MetaData(){Description = description,Name = name,Value = value});
            }


            return list;
        }}
        public void AddMetaData(MetaData metaData)
        {
            var node = xmlDoc.SelectSingleNode("/SDOList/SDO/Metadata", nm);
            XmlNode newValuePair = xmlDoc.CreateNode(XmlNodeType.Element, "ValuePair",null);
            XmlNode nameNode = xmlDoc.CreateNode(XmlNodeType.Element, "Name", null);
            nameNode.InnerText = metaData.Name;
            XmlNode valueNode = xmlDoc.CreateNode(XmlNodeType.Element, "Value", null);
            valueNode.InnerText = metaData.Value;
            XmlNode descriptionNode = xmlDoc.CreateNode(XmlNodeType.Element, "Description", null);
            descriptionNode.InnerText = metaData.Description;


            newValuePair.AppendChild(nameNode);
            newValuePair.AppendChild(valueNode);
            newValuePair.AppendChild(descriptionNode);
            node.AppendChild(newValuePair);


        }

        public byte[] SaveSDO()
        {
            XmlDocument toSave = (XmlDocument)xmlDoc.Clone();            

            //Putting namespace back
            toSave.InnerXml = toSave.InnerXml.Replace("xmlns:XAdES=\"http://uri.etsi.org/01903/v1.2.2#\"",
                                                      "xmlns=\"http://www.npt.no/seid/xmlskjema/SDO_v1.0\" xmlns:XAdES=\"http://uri.etsi.org/01903/v1.2.2#\"");

            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter writer = new XmlTextWriter(ms,Encoding.UTF8))
                {
                    toSave.Save(writer);
                    
                }
                return ms.ToArray();
            }
        }

        public string SaveSDO(string folderpath=null)
        {
            if (string.IsNullOrEmpty(folderpath))
                folderpath = System.IO.Path.GetTempPath();

            string filename = string.Format("{0}.SDO", Guid.NewGuid().ToString("N")); ;
           
            string path = System.IO.Path.Combine(folderpath, filename);

            File.WriteAllBytes(path, SaveSDO());

            return path;
        }

        /// <summary>
        /// Returns a list of all the signatures in the SDO with name, timstamp..
        /// </summary>
        public IEnumerable<BankIDSignature> Signatures
        {
            get
            {
                IList<BankIDSignature> retur = new List<BankIDSignature>();
                XmlNodeList list = xmlDoc.SelectNodes("/SDOList/SDO/SDODataPart/SignatureElement", nm);
                foreach (XmlNode item in list)
                {
                    string sdoprofile = item.SelectSingleNode("CMSSignatureElement/SDOProfile").InnerText;
                    SignedCms cms = new SignedCms(DocumentContentInfo, true);

                    XmlNode signatureNode = item.SelectSingleNode("CMSSignatureElement/CMSSignature");
                    if (signatureNode != null)
                        cms.Decode(Convert.FromBase64String( signatureNode.InnerText));
                    var signedTime = GetSigningTimeFromSignedCMS(cms);
                    string certuser = null;
                    string certbank = null;
                    GetCertificateStringForBankAndForUser(cms, ref certbank, ref certuser);

                    string banktype = "PersonBankID";
                    bool isPersonBank = !string.IsNullOrEmpty( certuser.Organization()) && certuser.Organization().Equals("BankID");
                    if (!isPersonBank)
                    {
                        banktype = "BrukerstedsBankID";
                    }                    
                    bool verified = Verified(cms);

                    retur.Add(new BankIDSignature()
                        {
                            SDOProfile = sdoprofile,
                            SignPolicy =item.SelectSingleNode("CMSSignatureElement/SignaturePolicyIdentifier/SignaturePolicyId/SigPolicyId/XAdES:Identifier",nm).InnerText,
                            Name = certuser.Name(),
                            BankName = certbank.BankName(),
                            UniqueId = certuser.SerialNumber(),                            
                            BankIDtype = banktype,
                            TimeStamp = signedTime.Value,
                            Verified = verified,
                        });
                }

                return retur;
            }
        }

        private static bool Verified(SignedCms cms)
        {            
            try
            {
                cms.CheckSignature(true);
                //cms.CheckSignature(false);
                cms.CheckHash();                
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void SetupXML(XmlDocument xmlDoctmp) {

            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlDoctmp.InnerXml.Replace("xmlns=\"http://www.npt.no/seid/xmlskjema/SDO_v1.0\"", ""));


            nm = new XmlNamespaceManager(xmlDoc.NameTable);
            foreach (KeyValuePair<string, string> nskvp in nm.GetNamespacesInScope(XmlNamespaceScope.All)) {
                nm.AddNamespace(nskvp.Key, nskvp.Value);
            }
            nm.AddNamespace("XAdES", "http://uri.etsi.org/01903/v1.2.2#");
            DocumentContentInfo = new ContentInfo(SignedDocument);
        }

        private static void GetCertificateStringForBankAndForUser(SignedCms cms, ref string certbank, ref string certuser)
        {
            X509Certificate2 cert1 = cms.Certificates[0];
            X509Certificate2 cert2 = cms.Certificates[1];

            X509Chain chain = new X509Chain();
            chain.ChainPolicy.RevocationMode =X509RevocationMode.Online; //  X509RevocationMode.Offline;

            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 30);
            
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            
            chain.Build(cert1);
            chain.Build(cert2);
           

            if (cert1.IssuerName.Name != null && cert1.IssuerName.Name.Contains("BankID Root CA"))
            {
                certbank = cert1.SubjectName.Name;
                certuser = cert2.SubjectName.Name;
            }
            else if(cert1.IssuerName.Name != null && !cert1.IssuerName.Name.Contains("BankID Root CA"))
            {
                certbank = cert2.SubjectName.Name;
                certuser = cert1.SubjectName.Name;
            }
        }

        private static DateTime? GetSigningTimeFromSignedCMS(SignedCms cms)
        {
            for (int i = 0; i < cms.SignerInfos[0].SignedAttributes.Count; i++)
            {
                //Find the signtime attribute
                var oidvalue = cms.SignerInfos[0].SignedAttributes[i].Oid.Value;
                var friendlyname = cms.SignerInfos[0].SignedAttributes[i].Oid.FriendlyName;
                if (oidvalue.Equals("1.2.840.113549.1.9.5") && friendlyname.Equals("Signing Time"))
                {
                    {
                        var value = ((Pkcs9SigningTime) cms.SignerInfos[0].SignedAttributes[i].Values[0]).SigningTime;
                        return value;
                    }
                }
            }
            return null;
        }
     
    }
 
}