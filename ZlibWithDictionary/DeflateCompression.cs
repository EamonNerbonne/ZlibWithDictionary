using System;
using System.IO;
using Ionic.Zlib;

namespace ZlibWithDictionary
{
    public class DeflateCompression
    {
        /// <summary>
        /// Compress a byte array using the deflate algorithm
        /// </summary>
        /// <param name="inputData">The data to compress</param>
        /// <param name="compressionLevel">How hard to try compressing - CompressionLevel.Default (level 6) provides a good balance between performance and compression.  Higher levels can sometimes reduce compression.</param>
        /// <param name="windowSize">The size of the context window in bits (9-15 or null for default).  Larger window sizes usually increase compression but can reduce it.</param>
        /// <param name="compressionStrategy">Which compression strategy to use.  Default tends to compress best, if your data has few repetitve subsequences, you can try Filtered; if it has none (but a non-uniform symbol distribution), you can try Huffman-only.</param>
        /// <param name="dictionary">A preset dictionary of data similar to the data you will compress (or null, for none).  If you provide a dictionary, you cannot decompress without the exact same dictionary.  It is not useful to include a dictionary larger than 2^windowSize.
        /// The best dictionary is one that is contains substrings that are likely to occur in the input - the longer the matching substrings, and the more likely they occur in the input, the better.
        /// </param>
        /// <returns>The compressed data.</returns>
        public static byte[] ZlibCompressWithDictionary(byte[] inputData, byte[] dictionary, CompressionLevel compressionLevel = CompressionLevel.Default, int? windowSize = null, CompressionStrategy compressionStrategy = CompressionStrategy.Default)
        {
            const int bufferSize = 256;
            byte[] buffer = new byte[bufferSize];
            using (var ms = new MemoryStream()) {

                ZlibCodec codec = new ZlibCodec();
                codec.Strategy = compressionStrategy;
                codec.AssertOk("InitializeDeflate",
                    windowSize == null
                        ? codec.InitializeDeflate(compressionLevel)
                        : codec.InitializeDeflate(compressionLevel, windowSize.Value)
                );

                if (dictionary != null) {
                    codec.AssertOk("SetDictionary", codec.SetDictionary(dictionary));

                    var dictionaryAdler32 = ((int)Adler.Adler32(1u, dictionary, 0, dictionary.Length));
                    if (codec.Adler32 != dictionaryAdler32)
                        throw new InvalidOperationException("Impossible: codec should have an adler32 checksum fully determined by the dictionary");
                }

                codec.InputBuffer = inputData;
                codec.AvailableBytesIn = inputData.Length;
                codec.NextIn = 0;

                codec.OutputBuffer = buffer;

                while (codec.TotalBytesIn != inputData.Length) {
                    codec.AvailableBytesOut = bufferSize;
                    codec.NextOut = 0;
                    codec.AssertOk("Deflate", codec.Deflate(FlushType.None));
                    var bytesToWrite = bufferSize - codec.AvailableBytesOut;
                    ms.Write(buffer, 0, bytesToWrite);
                }

                //we've read all input bytes, but may need several flushes to write all output bytes...

                while (true) {
                    codec.AvailableBytesOut = bufferSize;
                    codec.NextOut = 0;
                    var deflateFinishErrorCode = codec.Deflate(FlushType.Finish);

                    var bytesToWrite = bufferSize - codec.AvailableBytesOut;
                    ms.Write(buffer, 0, bytesToWrite);

                    if (deflateFinishErrorCode == ZlibConstants.Z_STREAM_END)
                        break;

                    codec.AssertOk("Deflate(Finish)", deflateFinishErrorCode);
                }

                codec.AssertOk("EndDeflate", codec.EndDeflate());
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Decompressed (inflates) a compressed byte array using the Inflate algorithm.
        /// </summary>
        /// <param name="compressedData">The deflate-compressed data</param>
        /// <param name="dictionary">The dictionary originally used to compress the data, or null if no dictionary was used.</param>
        /// <returns>The uncompressed data</returns>
        public static byte[] ZlibDecompressWithDictionary(byte[] compressedData, byte[] dictionary)
        {
            using (var ms = new MemoryStream()) {
                const int bufferSize = 256;
                byte[] buffer = new byte[bufferSize];

                ZlibCodec codec = new ZlibCodec();

                codec.InputBuffer = compressedData;
                codec.NextIn = 0;
                codec.AvailableBytesIn = compressedData.Length;

                codec.AssertOk("InitializeInflate", codec.InitializeInflate());

                codec.OutputBuffer = buffer;

                while (true) {
                    codec.NextOut = 0;
                    codec.AvailableBytesOut = bufferSize;
                    var inflateReturnCode = codec.Inflate(FlushType.None);
                    var bytesToWrite = bufferSize - codec.AvailableBytesOut;
                    ms.Write(buffer, 0, bytesToWrite);

                    if (inflateReturnCode == ZlibConstants.Z_STREAM_END) {
                        break;
                    } else if (inflateReturnCode == ZlibConstants.Z_NEED_DICT && dictionary != null) {
                        //implies bytesToWrite was 0
                        var dictionaryAdler32 = ((int)Adler.Adler32(1u, dictionary, 0, dictionary.Length));
                        if (codec.Adler32 != dictionaryAdler32)
                            throw new InvalidOperationException($"Compressed data is requesting a dictionary with adler32 {codec.Adler32}, but the dictionary is actually {dictionaryAdler32}");

                        codec.AssertOk("SetDictionary", codec.SetDictionary(dictionary));
                    } else
                        codec.AssertOk("Inflate", inflateReturnCode);
                }

                codec.AssertOk("EndInflate", codec.EndInflate());
                return ms.ToArray();
            }
        }
    }
}