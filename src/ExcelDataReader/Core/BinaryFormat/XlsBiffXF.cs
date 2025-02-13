﻿using System;

namespace ExcelDataReader.Core.BinaryFormat
{
    [Flags]
    internal enum XfUsedAttributes : byte
    {
        NumberFormat = 0x01,
        Font = 0x02,
        TextStyle = 0x04,
        BorderLines = 0x08,
        BackgroundAreaStyle = 0x10,
        CellProtection = 0x20,
    }

    internal class XlsBiffXF : XlsBiffRecord
    {
        internal XlsBiffXF(byte[] bytes, uint offset, int biffVersion)
            : base(bytes, offset)
        {
            switch (Id)
            {
                case BIFFRECORDTYPE.XF_V2:
                    Font = ReadByte(0);
                    Format = ReadByte(2) & 0x3F;
                    IsLocked = (ReadByte(2) & 0x40) != 0;
                    IsHidden = (ReadByte(2) & 0x80) != 0;
                    HorizontalAlignment = (HorizontalAlignment)(ReadByte(3) & 0x07);
                    ParentCellStyleXf = 0xfff;
                    UsedAttributes =
                        XfUsedAttributes.NumberFormat | XfUsedAttributes.Font | XfUsedAttributes.TextStyle |
                        XfUsedAttributes.BorderLines | XfUsedAttributes.BackgroundAreaStyle | XfUsedAttributes.CellProtection;
                    break;
                case BIFFRECORDTYPE.XF_V3:
                    Font = ReadByte(0);
                    Format = ReadByte(1);
                    UsedAttributes = (XfUsedAttributes)(ReadByte(3) >> 2);
                    IsLocked = (ReadByte(2) & 1) != 0;
                    IsHidden = (ReadByte(2) & 2) != 0;
                    IsCellStyleXf = (ReadByte(2) & 4) != 0;
                    ParentCellStyleXf = ReadUInt16(4) >> 4;
                    HorizontalAlignment = (HorizontalAlignment)(ReadByte(4) & 0x07);
                    break;
                case BIFFRECORDTYPE.XF_V4:
                    Font = ReadByte(0);
                    Format = ReadByte(1);
                    IsLocked = (ReadByte(2) & 1) != 0;
                    IsHidden = (ReadByte(2) & 2) != 0;
                    IsCellStyleXf = (ReadByte(2) & 4) != 0;
                    ParentCellStyleXf = ReadUInt16(2) >> 4;
                    UsedAttributes = (XfUsedAttributes)(ReadByte(5) >> 2);
                    HorizontalAlignment = (HorizontalAlignment)(ReadByte(4) & 0x07);
                    break;
                default:
                    Font = ReadUInt16(0);
                    Format = ReadUInt16(2);
                    IsLocked = (ReadByte(4) & 1) != 0;
                    IsHidden = (ReadByte(4) & 2) != 0;
                    IsCellStyleXf = (ReadByte(4) & 4) != 0;
                    ParentCellStyleXf = ReadUInt16(4) >> 4;
                    HorizontalAlignment = (HorizontalAlignment)(ReadByte(6) & 0x07);
                    if (biffVersion < 8)
                    {
                        UsedAttributes = (XfUsedAttributes)(ReadByte(7) >> 2);
                    }
                    else if (biffVersion == 8)
                    {
                        IndentLevel = ReadByte(8) & 0x0F;
                        UsedAttributes = (XfUsedAttributes)(ReadByte(9) >> 2);
                    }

                    break;
            }

            // Paren 0xfff = do not inherit any cell style XF
            if (ParentCellStyleXf == 0xfff)
            {
                ParentCellStyleXf = -1;
            }

            // The font with index 4 is omitted in all BIFF versions. This means the first four
            // fonts have zero-based indexes, and the fifth font and all following fonts are 
            // referenced with one-based indexes.
            if (Font > 4)
            {
                Font--;
            }
        }

        public int Font { get; }

        public XfUsedAttributes UsedAttributes { get; }

        public int Format { get; }

        public int ParentCellStyleXf { get; }

        public bool IsCellStyleXf { get; }

        public bool IsLocked { get; }

        public bool IsHidden { get; }

        public bool ApplyAlignment { get; }

        public int IndentLevel { get; }

        public HorizontalAlignment HorizontalAlignment { get; }
    }
}
