﻿using System;
using System.IO;

namespace OpenSage.Data.RefPack
{
    // Clean-room implementation based on spec here:
    // http://wiki.niotso.org/RefPack#Bitstream_specification
    public class RefPackStream : Stream
    {
        private readonly Stream _stream;
        private readonly byte[] _output;
        private int _currentOutputPosition;
        private int _nextOutputPosition;
        private bool _eof;

        public override bool CanRead => _stream?.CanRead ?? false;

        public override bool CanWrite => false;

        public override bool CanSeek => false;

        public override long Length => _output.Length;

        public override long Position
        {
            get => _currentOutputPosition;
            set => throw new NotImplementedException();
        }

        public RefPackStream(Stream compressedStream)
        {
            _stream = compressedStream;

            // Read enough of stream to get uncompressed size.

            var headerByte1 = compressedStream.ReadByte();
            if ((headerByte1 & 0b00111110) != 0b00010000)
                throw new InvalidDataException();

            var largeFilesFlag = (headerByte1 & 0b1000000) != 0;
            var compressedSizePresent = (headerByte1 & 0b00000001) != 0;

            var headerByte2 = compressedStream.ReadByte();
            if (headerByte2 != 0b11111011)
                throw new InvalidDataException();

            int readBigEndianSize()
            {
                var count = largeFilesFlag ? 4 : 3;
                var size = 0;
                for (var i = 0; i < count; i++)
                {
                    size = (size << 8) | compressedStream.ReadByte();
                }
                return size;
            }

            if (compressedSizePresent)
            {
                var compressedSize = readBigEndianSize();
            }
            var decompressedSize = readBigEndianSize();

            _output = new byte[decompressedSize];
        }

        public override void Flush()
        {
            
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var actualCount = Math.Min(count, _output.Length - _currentOutputPosition);

            while (!_eof && _currentOutputPosition + actualCount > _nextOutputPosition)
            {
                ExecuteCommand();
            }

            Array.Copy(_output, _currentOutputPosition, buffer, offset, actualCount);
            _currentOutputPosition += actualCount;

            return actualCount;
        }

        private void ExecuteCommand()
        {
            var byte1 = _stream.ReadByte();
            if ((byte1 & 0x80) == 0) // 2-byte command
            {
                Execute2ByteCommand(byte1);
            }
            else if ((byte1 & 0x40) == 0) // 3-byte command
            {
                Execute3ByteCommand(byte1);
            }
            else if ((byte1 & 0x20) == 0) // 4-byte command
            {
                Execute4ByteCommand(byte1);
            }
            else // 1-byte command
            {
                if (byte1 < 0xFC) // Ordinary 1-byte command
                {
                    Execute1ByteCommand(byte1);
                }
                else // Stop command
                {
                    Execute1ByteCommandAndStop(byte1);
                }
            }
        }

        private void Execute2ByteCommand(int byte1)
        {
            var byte2 = _stream.ReadByte();

            var proceedingDataLength = byte1 & 0x03;
            CopyProceeding(proceedingDataLength);

            var referencedDataLength = ((byte1 & 0x1C) >> 2) + 3;
            var referencedDataDistance = ((byte1 & 0x60) << 3) + byte2 + 1;
            CopyReferencedData(referencedDataLength, referencedDataDistance);
        }

        private void Execute3ByteCommand(int byte1)
        {
            var byte2 = _stream.ReadByte();
            var byte3 = _stream.ReadByte();

            var proceedingDataLength = (byte2 & 0xC0) >> 6;
            CopyProceeding(proceedingDataLength);

            var referencedDataLength = (byte1 & 0x3F) + 4;
            var referencedDataDistance = ((byte2 & 0x3F) << 8) + byte3 + 1;
            CopyReferencedData(referencedDataLength, referencedDataDistance);
        }

        private void Execute4ByteCommand(int byte1)
        {
            var byte2 = _stream.ReadByte();
            var byte3 = _stream.ReadByte();
            var byte4 = _stream.ReadByte();

            var proceedingDataLength = byte1 & 0x03;
            CopyProceeding(proceedingDataLength);

            var referencedDataLength = ((byte1 & 0x0C) << 6) + byte4 + 5;
            var referencedDataDistance = ((byte1 & 0x10) << 12) + (byte2 << 8) + byte3 + 1;
            CopyReferencedData(referencedDataLength, referencedDataDistance);
        }

        private void Execute1ByteCommand(int byte1)
        {
            var proceedingDataLength = ((byte1 & 0x1F) + 1) << 2;
            CopyProceeding(proceedingDataLength);
        }

        private void Execute1ByteCommandAndStop(int byte1)
        {
            var proceedingDataLength = byte1 & 0x03;
            CopyProceeding(proceedingDataLength);

            _eof = true;
        }

        private void CopyProceeding(int proceedingDataLength)
        {
            while (proceedingDataLength-- > 0)
            {
                _output[_nextOutputPosition++] = (byte) _stream.ReadByte();
            }
        }

        private void CopyReferencedData(int referencedDataLength, int referencedDataDistance)
        {
            var referencedDataIndex = _nextOutputPosition - referencedDataDistance;
            while (referencedDataLength-- > 0)
            {
                _output[_nextOutputPosition++] = _output[referencedDataIndex++];
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
