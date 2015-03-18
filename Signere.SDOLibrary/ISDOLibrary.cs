using System.Collections.Generic;
using System.IO;

namespace Signere.SDOLibrary
{
    public interface ISDOLibrary
    {
        /// <summary>
        /// Load SDO from string
        /// </summary>
        /// <param name="sdoXML">SDO XML as string</param>
        void LoadFromString(string sdoXML);

        /// <summary>
        /// Load SDO from filepath
        /// </summary>
        /// <param name="filepath">Filepath to the SDO file</param>
        void LoadFromPath(string filepath);

        /// <summary>
        /// Load SDO from bytearray
        /// </summary>
        /// <param name="filedata">Byte array containing the SDO file</param>
        void LoadFromByteArray(byte[] filedata);

        /// <summary>
        /// Load SDO from stream
        /// </summary>
        /// <param name="stream">Stream containing the SDO</param>
        void LoadFromStream(Stream stream);

        /// <summary>
        /// Returns the number of signatures in the SDO
        /// </summary>
        int NumberOfSignatures { get; }

        /// <summary>
        /// Returns the document description
        /// </summary>
        string DocumentDescription { get; }

        /// <summary>
        /// Returns the SEID SDO Version
        /// </summary>
        string SEIDSDOVersion { get; }

        /// <summary>
        /// Returns a signature element for the SDO Seal with name, timestamp and verfied
        /// </summary>
        Signature SealedBy { get; }

        /// <summary>
        /// Returns the documenttype xml, pdf or text.
        /// </summary>
        DocumentType DocumentType { get; }

        /// <summary>
        /// Returns the document that is signed as bytearray
        /// </summary>
        byte[] SignedDocument { get; }

        /// <summary>
        /// Returns a list of all the signatures in the SDO with name, timstamp..
        /// </summary>
        IEnumerable<BankIDSignature> Signatures { get; }

        /// <summary>
        /// Saves the signed document to the given filepath and returns the fullpath to the file
        /// </summary>
        /// <param name="filepath">Filepath to save the signed document, if none given it will be saved in temp</param>
        /// <returns>Full path to the signed document</returns>
        string SaveSignedDocument(string filepath=null);

        /// <summary>
        /// Returns a list of all the metdata in the SDO       
        /// </summary>
        /// <returns></returns>
        IEnumerable<MetaData> MetaData { get; }

        /// <summary>
        /// Adds metadata to the SDO
        /// </summary>
        /// <param name="metaData"></param>
        void AddMetaData(MetaData metaData);

        /// <summary>
        /// Returns the SDO as a byte array, use when you add metadata and want the new SDO 
        /// </summary>
        /// <returns>The SDO as a byte array</returns>
        byte[] SaveSDO();

        /// <summary>
        /// Saves the SDO to the given folder
        /// </summary>
        /// <param name="folderpath">Path to the folder where you want to save the SDO</param>
        /// <returns>Returns full filepath to the saved file.</returns>
        string SaveSDO(string folderpath);
    }
}