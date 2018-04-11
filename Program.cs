using System;
using System.IO;
using System.Security.Cryptography;

namespace cypher
{
    class Program
    {
        static void Generate()
        {
            var rsa = RSA.Create();
            Console.WriteLine("Use the following for the PUBLIC KEY (encryption)");
            Console.WriteLine(rsa.ToXmlString(false));
            Console.WriteLine("Use the following for the PRIVATE KEY (decryption)");
            Console.WriteLine(rsa.ToXmlString(true));
        }
        static void Main(string[] args)
        {
            try
            {
                var cmdOptions = new CommandOptions(args, new []{"d", "g"});
                if (cmdOptions.IsPresent("g"))
                {
                    Generate();
                    return;
                }
                var decrypt = cmdOptions.IsPresent("d");
                var key = cmdOptions.GetOne("key");
                var files = cmdOptions.Get(string.Empty);
                var input = files[0];
                var output = files[1];

                if (key.StartsWith("@"))
                {
                    // read from a file
                    key = File.ReadAllText(key.Substring(1));
                }
                var rsa = RSA.Create();
                rsa.FromXmlString(key);
                
                var aes = Aes.Create();
                if (decrypt)
                {
                    using (var i = new BinaryReader(File.OpenRead(input)))
                    {
                        // read the encrypted AES configuration
                        var aesLength = i.ReadInt32();
                        var encryptedAESConfiguration = i.ReadBytes(aesLength);
                        var aesConfiguration = rsa.Decrypt(encryptedAESConfiguration, RSAEncryptionPadding.Pkcs1);
                        var iv = new byte[aes.IV.Length];
                        Array.Copy(aesConfiguration, iv, iv.Length);
                        var aeskey = new byte[aes.Key.Length];
                        Array.Copy(aesConfiguration, iv.Length, aeskey, 0, aeskey.Length);
                        aes.IV = iv;
                        aes.Key = aeskey;
                        var decryptor = aes.CreateDecryptor();
                        using (var o = File.OpenWrite(output))
                        {
                            var buff = new byte[decryptor.InputBlockSize];
                            var outbuff = new byte[decryptor.OutputBlockSize];
                            for (int count = i.Read(buff, 0, decryptor.InputBlockSize); count > 0; count = i.Read(buff, 0, decryptor.InputBlockSize))
                            {
                                if (count < buff.Length)
                                {
                                    outbuff = decryptor.TransformFinalBlock(buff, 0, count);
                                    o.Write(outbuff, 0, outbuff.Length);
                                }
                                else
                                {
                                    var decount = decryptor.TransformBlock(buff, 0, count, outbuff, 0);
                                    o.Write(outbuff, 0, decount);
                                }
                            }
                            outbuff = decryptor.TransformFinalBlock(buff, 0, 0);
                            o.Write(outbuff, 0, outbuff.Length);
                        }
                    }

                }
                else
                {
                    aes.GenerateIV();
                    aes.GenerateKey();
                    var aesConfiguration = new byte[aes.IV.Length + aes.Key.Length];
                    // put IV and Key, one after the other
                    Array.Copy(aes.IV, aesConfiguration, aes.IV.Length);
                    Array.Copy(aes.Key, 0, aesConfiguration, aes.IV.Length, aes.Key.Length);
                    // now that we have the AES configuration, we will encrypt it with RSA
                    var encryptedAESConfiguration = rsa.Encrypt(aesConfiguration, RSAEncryptionPadding.Pkcs1);
                    // write that to the output file
                    using (var o = new BinaryWriter(File.OpenWrite(output)))
                    {
                        o.Write(encryptedAESConfiguration.Length);
                        o.Write(encryptedAESConfiguration);
                        // now write the original contents, passing it through AES
                        using (var encryptor = aes.CreateEncryptor())
                        {
                            using (var i = File.OpenRead(input))
                            {
                                var buff = new byte[encryptor.InputBlockSize];
                                var outbuff = new byte[encryptor.OutputBlockSize];
                                for (int count = i.Read(buff, 0, buff.Length); count > 0; count = i.Read(buff, 0, buff.Length))
                                {
                                    if (count < buff.Length)
                                    {
                                        outbuff = encryptor.TransformFinalBlock(buff, 0, count);
                                        o.Write(outbuff, 0, outbuff.Length);
                                    }
                                    else
                                    {
                                        var outcount = encryptor.TransformBlock(buff, 0, count, outbuff, 0);
                                        o.Write(outbuff, 0, outcount);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }
    }
}
