using System;
using System.Linq;
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
                    ("Navn: {0} Bank: {1} Timestamp: {2} SDOProfile: {3} UniqueId: {4} BankType: {5}", signature.Name,
                     signature.BankName, signature.TimeStamp.ToLocalTime(), signature.SDOProfile, signature.UniqueId,signature.BankIDtype);
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
        public void TestSeal() {
            SDOLibrary lib = new SDOLibrary();
            lib.LoadFromPath("Test1.sdo");

            var result = lib.SealedBy;

            Console.WriteLine ("Navn: {0}  Timestamp: {1} ", result.Name, result.TimeStamp.ToLocalTime());

            Assert.AreEqual("signere.no",result.Name);
            Assert.AreEqual(new DateTime(2013,05,27,14,44,56),result.TimeStamp.ToLocalTime());
        }

       

        
    }
}
