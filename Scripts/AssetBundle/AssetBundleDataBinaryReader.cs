using System.IO;

namespace Tangzx.ABSystem
{
    /// <summary>
    /// 二进制文件格式说明
    /// *固定四个字节ABDB
    /// *namesCount 字符串池中字符串的个数
    /// 循环 namesCount {
    ///     *读取字符串到池中(string)
    /// }
    /// 循环 {
    ///     *名字在字符串池中的索引(int)
    ///     *短名字在字符串池中的索引(int)
    ///     *Hash在字符串池中的索引(int)
    ///     *类型(AssetBundleExportType)
    ///     *依赖文件个数M(int)
    ///     循环 M {
    ///         *依赖的AB文件名在字符串池中的索引(int)
    ///     }
    /// }
    /// </summary>
    class AssetBundleDataBinaryReader : AssetBundleDataReader
    {
        public override void Read(Stream fs)
        {
            if (fs.Length < 4) return;

            BinaryReader sr = new BinaryReader(fs);
            char[] fileHeadChars = sr.ReadChars(4);
            //读取文件头判断文件类型，ABDB 意思即 Asset-Bundle-Data-Binary
            if (fileHeadChars[0] != 'A' || fileHeadChars[1] != 'B' || fileHeadChars[2] != 'D' || fileHeadChars[3] != 'B')
                return;

            int namesCount = sr.ReadInt32();
            string[] names = new string[namesCount];
            for (int i = 0; i < namesCount; i++)
            {
                names[i] = sr.ReadString();
            }

            while (true)
            {
                if (fs.Position == fs.Length)
                    break;

                string assetName = sr.ReadString();
                string bundleName = sr.ReadString();
                string bundleHash = sr.ReadString();
               

                AssetContainerData info = new AssetContainerData();
                info.assetName = assetName;
                info.bundleName = bundleName;
                info.bundleHash = bundleHash;
                asset2bundleMap[assetName.ToLower()] = info;
            }
            sr.Close();
        }
    }
}