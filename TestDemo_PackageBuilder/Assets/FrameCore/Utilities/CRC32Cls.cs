namespace CRC32
{
    public class CRC32Cls
    {
        protected static uint[] Crc32Table;
        //生成CRC32码表
        public static void GetCRC32Table()
        {
            uint Crc;
            Crc32Table = new uint[256];
            int i,j;
            for(i = 0;i < 256; i++)
            {
                Crc = (uint)i;
                for (j = 8; j > 0; j--)
                {
                    if ((Crc & 1) == 1)
                        Crc = (Crc >> 1) ^ 0xEDB88320;
                    else
                        Crc >>= 1;
                }
                Crc32Table[i] = Crc;
            }
        }

        //获取字符串的CRC32校验值
        public static uint GetCRC32Str(string sInputString)
        {
            //生成码表
            if(Crc32Table == null)
                GetCRC32Table();
            byte[] buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(sInputString);
            uint value = 0xffffffff;
            int len = buffer.Length;
            for (int i = 0; i < len; i++)
            {
                value = (value >> 8) ^ Crc32Table[(value & 0xFF)^ buffer[i]];
            }
            return value ^ 0xffffffff;
        }
    }
}