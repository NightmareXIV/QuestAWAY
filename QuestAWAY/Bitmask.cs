using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestAWAY
{
    class Bitmask
    {
        public static bool IsBitSet(short b, byte pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static void SetBit(ref short b, byte pos)
        {
            b |= (short)(1 << pos);
        }

        public static void ResetBit(ref short b, byte pos)
        {
            b &= (short)~(1 << pos);
        }
        public static bool IsBitSet(uint b, byte pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static void SetBit(ref uint b, byte pos)
        {
            b |= (uint)(1 << pos);
        }

        public static void ResetBit(ref uint b, byte pos)
        {
            b &= (uint)~(1 << pos);
        }
    }
}
