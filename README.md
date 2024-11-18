ZlibWithDictionary
================

Download package via nuget: [ZlibWithDictionary](http://nuget.org/packages/ZlibWithDictionary/)


This is a small wrapper around [DotNetZip](https://github.com/haf/DotNetZip.Semverd) that allows Deflate compression with preset dictionary.

Compression with dictionary is particularly useful for compressing small byte-sequences (there are rapidly diminishing returns beyond 32k) where the content being compressed follows predictable patterns (such as web-service results following a known schema or html documents).  The best dictionary is one that is contains substrings that are likely to occur in the input - the longer the matching substrings, and the more likely they occur in the input, the better.

The only two methods provided are `DeflateCompression.ZlibCompressWithDictionary` and `Deflate.ZlibDecompressWithDictionary` (intellisense explains the parameters). Usage example:

```C#
var compressed_byte_array = DeflateCompression.ZlibCompressWithDictionary(
    bytes_array_to_compress, 
    CompressionLevel.Default, 
    null /*use default window size in bits; possible values 9-15*/, 
    CompressionStrategy.Default, 
    byte_array_of_dictionary
);
var decompressed_byte_array = DeflateCompression.ZlibDecompressWithDictionary(
    compressed_byte_array,
    byte_array_of_dictionary
);
```

If you prefer to manually use the appropriate DotNetZip apis over importing this wrapper, you can peruse [the source for the DeflateCompression wrapper](https://github.com/EamonNerbonne/ZlibWithDictionary/blob/master/ZlibWithDictionary/DeflateCompression.cs).

Per the DotNetZip docs, this compression is compatible with the deflate RFC as implemented by the common [zlib library](http://zlib.net).
(Note that although gzip uses the same algorithm, it uses a slightly different and thus incompatible header.)


Unaffected by DotNetZip directory traversal vulnerability
---

[DotNetZip has a directory traversal vulnerability](https://github.com/advisories/GHSA-xhg6-9j5j-w4vf) when extracting ZipEntry. Unfortunately, [DotNetZip](https://github.com/haf/DotNetZip.Semverd) is no longer maintained and will likely not be updated.  However, code using this library (and no other DotNetZip features) is not affected because the library as far as I can tell never touches any code-paths which use ZipEntry; this library only deals with simple byte arrays and never performs any I/O. In short: the vulnerability is in the zip-archive part of DotNetZip, not the Zlib algorithmic part.
