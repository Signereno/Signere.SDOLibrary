using System;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;

namespace Signere.SDOLibrary.Test
{
    [TestFixture]
    public class SDOLibraryTestPDF
    {
        [Test]
        public void TestDescription()
        {
            SDOLibrary lib=new SDOLibrary();
            lib.LoadFromPath("Test1.sdo");
            Assert.AreEqual("Test paa multisignatur",lib.DocumentDescription);
        }

        [Test]
        public void TestSEIDSDOVersion() {
            SDOLibrary lib = new SDOLibrary();
            lib.LoadFromPath("Test1.sdo");
            Assert.AreEqual("1.0", lib.SEIDSDOVersion);
        }

        [Test]
        public void TestDocumentType() {
            SDOLibrary lib = new SDOLibrary();
            lib.LoadFromPath("Test1.sdo");            
            Assert.AreEqual(DocumentType.PDF, lib.DocumentType);
        }

        [Test]
        public void TestSaveFile() {
            SDOLibrary lib = new SDOLibrary();
            lib.LoadFromPath("Test1.sdo");
            var filepath=lib.SaveSignedDocument(@"C:\");
            Console.WriteLine(filepath);
            Assert.IsTrue(System.IO.File.Exists(filepath));
            System.IO.File.Delete(filepath);
        }

        [Test]
        public void TestSaveFile_file3()
        {
            SDOLibrary lib = new SDOLibrary();
            lib.LoadFromPath("test3_corrup_document.sdo");
            var filepath = lib.SaveSignedDocument(@"C:\");
            Console.WriteLine(filepath);
            Assert.IsTrue(System.IO.File.Exists(filepath));
            System.IO.File.Delete(filepath);
        }

        [Test]
        public void TestSaveFileWithoutPath() {
            SDOLibrary lib = new SDOLibrary();
            lib.LoadFromPath("Test1.sdo");
            var filepath = lib.SaveSignedDocument();
            Console.WriteLine(filepath);
            Assert.IsTrue(System.IO.File.Exists(filepath));
            System.IO.File.Delete(filepath);
        }

        [Test]
        public void TestCompareFileAndByteArray() {
            SDOLibrary lib = new SDOLibrary();
            lib.LoadFromPath("Test1.sdo");
            var filepath = lib.SaveSignedDocument(@"C:\");
            var bytes = System.IO.File.ReadAllBytes(filepath);
            Assert.AreEqual(bytes,lib.SignedDocument);
            Assert.IsTrue(System.IO.File.Exists(filepath));
            System.IO.File.Delete(filepath);
        }

        [Test]
        public void TestSaveSDO() {
            SDOLibrary lib = new SDOLibrary();
            lib.LoadFromPath("Test1.sdo");
            string filepath = lib.SaveSDO(@"C:\");
            Console.WriteLine(filepath);
            System.IO.File.Delete(filepath);
        }

        [Test]
        public void TestAddMetaData()
        {
            SDOLibrary lib = new SDOLibrary();
            lib.LoadFromPath("Test1.sdo");     
            lib.AddMetaData(new MetaData()
                {
                    Description = "Signere description",
                    Name = "DocumentDescription",
                    Value = "Nice Document"
                });

            foreach (MetaData metaData in lib.MetaData) {
                Console.WriteLine("Name: {0} Value: {1} Description: {2}", metaData.Name, metaData.Value, metaData.Description);
            }


            Console.WriteLine(lib.SaveSDO(@"C:\"));
            Assert.AreEqual(2, lib.MetaData.Count());
            Assert.IsTrue(lib.MetaData.Any(x => x.Name.Equals("DocumentDescription") && x.Value.Equals("Nice Document")));
            
        }

        [Test]
        public void TestMetaData()
        {
            SDOLibrary lib = new SDOLibrary();
            lib.LoadFromPath("Test1.sdo");

            foreach (MetaData metaData in lib.MetaData)
            {
                Console.WriteLine("Name: {0} Value: {1} Description: {2}",metaData.Name,metaData.Value,metaData.Description);
            }
            Assert.AreEqual(1,lib.MetaData.Count());
            Assert.AreEqual("MerchantDesc", lib.MetaData.FirstOrDefault().Name);
            Assert.AreEqual("Test paa multisignatur", lib.MetaData.FirstOrDefault().Value);
            Assert.IsNull(lib.MetaData.FirstOrDefault().Description);
        }


        [Test]
        public void TestSignatures() {
            SDOLibrary lib = new SDOLibrary();
            lib.LoadFromPath("Test1.sdo");

            var result=lib.Signatures;

            foreach (BankIDSignature signature in result)
            {
                Console.WriteLine
                    ("Navn: {0} Bank: {1} Timestamp: {2} SDOProfile: {3} UniqueId: {4} BankType: {5} Oid: {6}", signature.Name,
                     signature.BankName, signature.TimeStamp.ToLocalTime(), signature.SDOProfile, signature.UniqueId,signature.BankIDtype,signature.SignPolicy);
                Console.WriteLine(signature.Certificate);
            }
            Assert.AreEqual(3,result.Count());
            Assert.AreEqual(3,lib.NumberOfSignatures);

            Assert.IsTrue(result.Any(x =>
                    x.Name.Equals("Synnevåg, Rune") &&
                    x.BankName.Equals("Bankenes ID-tjeneste") &&
                    x.UniqueId.Equals("9578-5993-4-1617343") &&
                    x.TimeStamp.ToLocalTime().Equals(new DateTime(2013, 05, 27, 14, 44, 02)) 
                ),"Error 1");
            Assert.IsTrue(result.Any(x => 
                x.Name.Equals("signere.no") && 
                x.BankName.Equals("DnB NOR") && 
                x.UniqueId.Equals("998303168") &&
                x.TimeStamp.ToLocalTime().Equals(new DateTime(2013, 05, 27, 14, 41, 57))
                ), "Error 2");
            Assert.IsTrue(result.Any(x =>
                x.Name.Equals("FØLLESDAL, ESBEN ANDRE") && 
                x.BankName.Equals("Skandiabanken") &&
                x.UniqueId.Equals("9578-5994-4-629257") &&
                x.TimeStamp.ToLocalTime().Equals(new DateTime(2013, 05, 27, 14, 41, 37))
                ), "Error 3");
            
            Assert.AreEqual(result.Count(),result.Count(x=>x.Verified),"Not all signatures are verified");

            Assert.IsTrue(result.Any(x => x.GetFirstNameAndLastName().FirstName.Equals("Rune") && x.GetFirstNameAndLastName().LastName.Equals("Synnevåg")));
            Assert.IsTrue(result.Any(x => x.GetFirstNameAndLastName().FirstName.Equals("Esben Andre") && x.GetFirstNameAndLastName().LastName.Equals("Føllesdal")));
        }

        [Test]
        public void TestSignatures_should_not_be_verified()
        {
            SDOLibrary lib = new SDOLibrary();
            lib.LoadFromPath("test3_corrup_document.sdo");

            var result = lib.Signatures;

            foreach (BankIDSignature signature in result)
            {
                Assert.IsFalse(signature.Verified);
            }
        }

     

        [Test]
        public void TestSeal() {
            SDOLibrary lib = new SDOLibrary();
            lib.LoadFromPath("Test1.sdo");

            var result = lib.SealedBy;

            Console.WriteLine ("Navn: {0}  Timestamp: {1} verified: {2}", result.Name, result.TimeStamp.ToLocalTime(),result.Verified);
            Console.WriteLine(result.Certificate);
            Assert.AreEqual("signere.no",result.Name);
            Assert.IsTrue(result.Verified);
            Assert.AreEqual(new DateTime(2013,05,27,14,44,56),result.TimeStamp.ToLocalTime());
        }

        [Test]
        public void TestSeal_not_valid()
        {
            SDOLibrary lib = new SDOLibrary();
            lib.LoadFromPath("test2_corrupt_seal.sdo");

            var result = lib.SealedBy;

            Console.WriteLine("Navn: {0}  Timestamp: {1} verified: {2}", result.Name, result.TimeStamp.ToLocalTime(), result.Verified);
            Console.WriteLine(result.Certificate);
            Assert.AreEqual("signere.no", result.Name);
            Assert.IsFalse(result.Verified);
            Assert.AreEqual(new DateTime(2013, 05, 27, 14, 44, 56), result.TimeStamp.ToLocalTime());
        }


        [Test]
        public void Sha256()
        {
            string data= "<SDODataPart><SignatureElement><CMSSignatureElement><SDOProfile>SEID-SDO-Basic-V</SDOProfile><SignaturePolicyIdentifier><SignaturePolicyId><SigPolicyId><XAdES:Identifier>urn:oid:2.16.578.1.16.4.1</XAdES:Identifier></SigPolicyId></SignaturePolicyId></SignaturePolicyIdentifier><SignersDocumentFormat><MimeType>application/pdf</MimeType></SignersDocumentFormat><HashedData><ds:DigestMethod Algorithm=\"SHA-256\"/><ds:DigestValue>npdb34ArZat9l3P1aCbt3ZmCwW4OO36bNG7PxUEhhDY=</ds:DigestValue></HashedData><CMSSignature>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</CMSSignature><UserCertificateAndRevocationData><RevocationValues><XAdES:OCSPValues><XAdES:EncapsulatedOCSPValue>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</XAdES:EncapsulatedOCSPValue></XAdES:OCSPValues></RevocationValues></UserCertificateAndRevocationData></CMSSignatureElement></SignatureElement><SignatureElement><CMSSignatureElement><SDOProfile>SEID-SDO-Basic-V</SDOProfile><SignaturePolicyIdentifier><SignaturePolicyId><SigPolicyId><XAdES:Identifier>urn:oid:2.16.578.1.16.4.1</XAdES:Identifier></SigPolicyId></SignaturePolicyId></SignaturePolicyIdentifier><SignersDocumentFormat><MimeType>application/pdf</MimeType></SignersDocumentFormat><HashedData><ds:DigestMethod Algorithm=\"SHA-256\"/><ds:DigestValue>npdb34ArZat9l3P1aCbt3ZmCwW4OO36bNG7PxUEhhDY=</ds:DigestValue></HashedData><CMSSignature>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</CMSSignature><UserCertificateAndRevocationData><RevocationValues><XAdES:OCSPValues><XAdES:EncapsulatedOCSPValue>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</XAdES:EncapsulatedOCSPValue></XAdES:OCSPValues></RevocationValues></UserCertificateAndRevocationData></CMSSignatureElement></SignatureElement><SignatureElement><CMSSignatureElement><SDOProfile>SEID-SDO-Basic-V</SDOProfile><SignaturePolicyIdentifier><SignaturePolicyId><SigPolicyId><XAdES:Identifier>urn:oid:2.16.578.1.16.4.1</XAdES:Identifier></SigPolicyId></SignaturePolicyId></SignaturePolicyIdentifier><SignersDocumentFormat><MimeType>application/pdf</MimeType></SignersDocumentFormat><HashedData><ds:DigestMethod Algorithm=\"SHA-256\"/><ds:DigestValue>npdb34ArZat9l3P1aCbt3ZmCwW4OO36bNG7PxUEhhDY=</ds:DigestValue></HashedData><CMSSignature>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</CMSSignature><UserCertificateAndRevocationData><RevocationValues><XAdES:OCSPValues><XAdES:EncapsulatedOCSPValue>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</XAdES:EncapsulatedOCSPValue></XAdES:OCSPValues></RevocationValues></UserCertificateAndRevocationData></CMSSignatureElement></SignatureElement></SDODataPart>";
            Console.WriteLine(data.Length);
            Console.WriteLine(Convert.ToBase64String(new SHA256Managed().ComputeHash(System.Text.Encoding.GetEncoding(1252).GetBytes(data))));


        }

        
    }
}
