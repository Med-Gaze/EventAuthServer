using System;
using System.Collections.Generic;
using System.Text;

namespace med.common.library.security
{
    public interface ICryptography
    {
        string Encrypt(string plainText);
        string Decrypt(string plainText);
    }
}
