namespace GSA.OpenItems.Web
{
    using System;
    using GSA.OpenItems;

    /// <summary>
    /// Summary description for FilesBO
    /// </summary>
    public static class FilesBO
    {

        public static int AddNewFile(string FileName, int FileSize, byte[] FileData, string ContentType, string UploadUser)
        {

            if (UploadUser != "" && UploadUser.IndexOf("@") == -1)
                UploadUser = UploadUser + "@gsa.gov";

            var doc = new Document(FileName, FileSize, ContentType, FileData, UploadUser);
            return doc.DocID;

        }


    }
}