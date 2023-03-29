using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Start.Core.Helpers
{
    public class ZipHelper
    {
        public static async Task<Dictionary<string, byte[]>> UnZip(byte[] zipFile)
        {
            var unzipData = new Dictionary<string, byte[]>();
            try
            {
                var memoryZip = new MemoryStream(zipFile);
                using var zipArchive = new ZipArchive(memoryZip, ZipArchiveMode.Read);
                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {
                    var stream = zipArchiveEntry.Open();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);

                    unzipData.Add(zipArchiveEntry.Name, memoryStream.ToArray());
                }
            }
            catch (Exception)
            {

            }

            return unzipData;
        }
        
        public static byte[] Zip(Dictionary<string, byte[]> files)
        {
            byte[] data = null;
            try
            {
                using (MemoryStream zipStream = new MemoryStream())
                {
                    using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Update, false))
                    {
                        foreach (var f in files)
                        {
                            var entry = archive.CreateEntry(f.Key, CompressionLevel.Optimal);
                            using (BinaryWriter writer = new BinaryWriter(entry.Open()))
                            {
                                writer.Write(f.Value, 0, f.Value.Length);
                                writer.Close();
                            }
                        }

                        zipStream.Position = 0;
                    }

                    data = zipStream.ToArray();
                }
            }
            catch (Exception)
            {
            }

            return data;
        }

        public static byte[] ZipWithDirectory(Dictionary<string, Dictionary<string, byte[]>> directories)
        {
            byte[] data = null;
            try
            {
                using (MemoryStream zipStream = new MemoryStream())
                {
                    using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Update, false))
                    {
                        foreach (var d in directories)
                        {
                            //archive.CreateEntry(d.Key);

                            foreach(var f in d.Value)
                            {
                                var entry = archive.CreateEntry(Path.Combine(d.Key, f.Key), CompressionLevel.Optimal);
                                using (BinaryWriter writer = new BinaryWriter(entry.Open()))
                                {
                                    writer.Write(f.Value, 0, f.Value.Length);
                                    writer.Close();
                                }
                            }
                            
                        }

                        zipStream.Position = 0;
                    }

                    data = zipStream.ToArray();
                }
            }
            catch (Exception)
            {
            }

            return data;
        }


    }
}
