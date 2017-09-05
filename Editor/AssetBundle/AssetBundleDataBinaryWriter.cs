using System.Collections.Generic;
using System.IO;

namespace Tangzx.ABSystem
{
    public class AssetBundleDataBinaryWriter : AssetBundleDataWriter
    {
        public virtual void SaveMap(Stream stream, AssetTarget[] targets)
        {
            BinaryWriter sw = new BinaryWriter(stream);
            //写入文件头判断文件类型用，ABDB 意思即 Asset-Bundle-Data-Binary
            sw.Write(new char[] { 'A', 'B', 'D', 'B' });

            //写入详细信息
            for (int i = 0; i < targets.Length; i++)
            {
                AssetTarget target = targets[i];
                //debug name
                sw.Write(target.assetPath);
                //bundle name
                sw.Write(target.bundleName);
                //hash
                sw.Write(target.bundleCrc);
            }
            sw.Close();
        }

        public  void SaveDeps(Stream stream, BundleTarget[] targets)
        {
            BinaryWriter sw = new BinaryWriter(stream);
            //写入文件头判断文件类型用，ABDB 意思即 Asset-Bundle-Data-Binary
            sw.Write(new char[] { 'A', 'B', 'D', 'D' });

            

            //写入文件名池
            List<string> listBundleNames = new List<string>();
            sw.Write(targets.Length);
            for (int i = 0; i < targets.Length; i++)
            {
                sw.Write(targets[i].Name);
            }
        }
    }
}