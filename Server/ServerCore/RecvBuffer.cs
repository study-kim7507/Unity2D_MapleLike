using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize { get { return _writePos - _readPos; } } // 지금 가지고 있는 데이터 사이즈
        public int FreeSize { get { return _buffer.Count - _writePos; } } // 지금 쓸 수 있는(더 저장할 수 있는) 데이터 사이즈

        // 읽을 수 있는 데이터의 범위 반환
        public ArraySegment<byte> ReadSegment
        {
            // _buffer.Offset + _readPos: 읽을 데이터가 시작되는 위치
            // DataSize : 읽을 데이터의 크기
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }

        // 새로운 데이터를 쓸 수 있는 공간의 범위 반환
        public ArraySegment<byte> WriteSegment
        {
            // _buffer.Offset + _readPos: 읽을 데이터가 시작되는 위치
            // FreeSize : 쓸 수 있는 데이터의 크기
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        // 버퍼 영역을 관리 (스위핑) 해주는 함수
        // RegisterRecv()에서 데이터를 받을 때마다 실행된다.
        public void Clean()
        {
            int dataSize = DataSize; // DataSize : 아직 처리되지 않은 Data

            // 남은 데이터가 없으면 read, write offset을 0으로 초기화
            if (dataSize == 0)
            {
                _readPos = _writePos = 0;
            }
            // 남은 데이터가 있다면
            else
            {
                // 남은 데이터를 _buffer의 첫 부분에 옮긴 후, 
                // read offset은 처음 위치부터 읽기 시작하도록,
                // write offset은 남은 데이터의 끝 부분에 위치 시켜 다음 데이터를 받을 준비
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
                return false;

            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
                return false;

            _writePos += numOfBytes;
            return true;
        }
    }
}
