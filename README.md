ZlibWithDictionary
================

Download package via nuget: [ZlibWithDictionary](http://nuget.org/packages/ZlibWithDictionary/)


This is a plain wrapper around [DotNetZip](https://github.com/haf/DotNetZip.Semverd) that allows Deflate compression with preset dictionary.
This is particularly useful for compressing small byte-sequences (diminishing returns beyond 32k) where the content being compressed follows predictable patterns such a web-service results following a known schema or html documents.
The best dictionary is one that is contains substrings that are likely to occur in the input - the longer the matching substrings, and the more likely they occur in the input, the better.
The only two methods provided are `DeflateCompression.ZlibCompressWithDictionary` and `Deflate.ZlibDecompressWithDictionary` (intellisense explains the parameters).
Per the DotNetZip docs, this compression is compatible with the deflate RFC as implemented by the common [zlib library](http://zlib.net).
(Note that although gzip uses the same algorithm, it uses a slightly different and thus incompatible header.)
