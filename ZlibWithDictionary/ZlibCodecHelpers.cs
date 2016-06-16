using System;
using Ionic.Zlib;

namespace ZlibWithDictionary
{
    static class ZlibCodecHelpers
    {
        public static void AssertOk(this ZlibCodec codec, string Message, int errorCode)
        {
            if (errorCode != 0)
                throw new InvalidOperationException("Failed with " + errorCode + "; " + Message + "; " + codec.Message);
        }
    }
}
