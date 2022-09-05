// 
// Copyright (c) 2022 DotNetBrightener.

using System;

namespace DotNetBrightener.CryptoEngine.Loaders;

public interface IRSAKeysLoader
{
    /// <summary>
    ///     The name of the RSA Keys Loader
    /// </summary>
    string LoaderName { get; }

    /// <summary>
    ///     Load the keys pair (public and private key) for RSA Encryption, or initialize them if not exists
    /// </summary>
    /// <returns>
    ///     The <seealso cref="Tuple"/> of the public and private key.
    /// </returns>
    Tuple<string, string> LoadOrInitializeKeyPair();
}