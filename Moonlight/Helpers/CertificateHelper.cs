using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Moonlight.Helpers
{
    public class CertificateHelper
    {
        public static X509Certificate2 GetCertificate(string subjectName,
                                                        StoreName name = StoreName.My,
                                                            StoreLocation location = StoreLocation.LocalMachine)
        {
            X509Store store = new X509Store(name, location);
            store.Open(OpenFlags.ReadOnly);

            try
            {
                var cert = store.Certificates.OfType<X509Certificate2>()
                            .FirstOrDefault(c => c.SubjectName.Name.Equals(subjectName,
                                StringComparison.OrdinalIgnoreCase));

                return (cert != null) ? new X509Certificate2(cert) : null;
            }
            finally
            {
                store.Certificates.OfType<X509Certificate2>().ToList().ForEach(c => c.Reset());
                store.Close();
            }
        }

        public static bool InstallCertificate(byte[] cert, StoreName name = StoreName.My,
                                                            StoreLocation location = StoreLocation.LocalMachine)
        {
            try
            {
                X509Store store = new X509Store(name, location);
                store.Open(OpenFlags.ReadWrite);
                var certx = new X509Certificate2(cert);
                store.Add(certx);
                store.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
