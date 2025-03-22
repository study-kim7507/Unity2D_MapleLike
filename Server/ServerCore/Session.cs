using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    // 패킷 처리용 세션 
    // TCO 특성상 데이터가 일부만 올 수 있기 때문에 헤더사이즈 등으로 패킷이 온전히 처리됐는지 확인한다.
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        // Session클래스 에서 데이터를 받았을 때 (OnRecvCompleted())
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;
            int packetCount = 0; // 몇 개의 패킷 처리했는지 확인용

            while (true)
            {
                // 최소한 헤더는 파싱할 수 있는가?
                if (buffer.Count < HeaderSize)
                    break;

                // 패킷이 하나의 온전한 패킷으로 왔는가?
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                    break;

                // 패킷 처리 실로직 - PacketSession을 상속받은 하위 클래스에서
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));
                packetCount++;

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            //if (packetCount > 1)
            //    Console.WriteLine($"패킷 모아보내기 : {packetCount}");

            return processLen; // (온전한 패킷의) 처리된 데이터
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }


    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0; // 세션 종료 여부를 Interlocked를 통해 원자적으로 확인용 변수

        // 각 세션마다 수신용 버퍼를 생성한다.
        RecvBuffer _recvBuffer = new RecvBuffer(65535);

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        // 패킷 모아보내기에 사용
        public void Send(List<ArraySegment<byte>> sendBuffList)
        {
            if (sendBuffList.Count == 0)
                return;

            lock (_lock)
            {
                foreach (ArraySegment<byte> sendBuff in sendBuffList)
                    _sendQueue.Enqueue(sendBuff);

                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        public void Send(ArraySegment<byte> sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                // 지금 Send() 하는 이 스레드가 첫 번째이면 바로 RegisterSend() 호출
                if (_pendingList.Count == 0)
                    RegisterSend(); 
            }
        }

        public void Disconnect()
        {
            // 세션의 종료 여부를 원자적으로 확인
            // 결과가 1이라면, 이미 다른 스레드가 점유(종료)중
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            if (_disconnected == 1)
                return;

            while (_sendQueue.Count > 0)
            {
                // 큐에 모아둔 패킷을 하나로 합침(송신 데이터 모아 보내기)
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
            }
            _sendArgs.BufferList = _pendingList;

            try
            {
                bool pending = _socket.SendAsync(_sendArgs);
                // true : 보류 / false : 동기적으로 완료(바로 완료된 경우)
                // 동기적으로 바로 완료되었으면 OnSendCompleted()를 즉시 호출 
                if (pending == false)
                    OnSendCompleted(null, _sendArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterSend Failed {e}");
            }
        }

        // 호출되는 경우는 두 가지
        // 1. _socket.SendAsync(_sendArgs);가 true일 때 나중에 _sendArgs.Completed로 등록된 이벤트로 비동기적으로 호출
        // 2. _socket.SendAsync(_sendArgs);가 false(동기적으로 완료)면 직접 호출
        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        // RegisterSend ~ OnSendCompleted 과정 중에 다른 Thread의 Send()가 호출되어
                        // RegisterSend까지 못하고 _sendQueue에만 넣은 것이 있다면 여기서 RegisterSend() 호출
                        if (_sendQueue.Count > 0)
                            RegisterSend();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        void RegisterRecv()
        {
            if (_disconnected == 1)
                return;

            _recvBuffer.Clean(); // 버퍼 정리(스위핑)
            ArraySegment<byte> segment = _recvBuffer.WriteSegment; // 새로운 데이터를 쓸 수 있는 공간을 가져옴
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);
                // true : 보류 / false : 동기적으로 완료(바로 완료된 경우)
                // 동기적으로 바로 완료되었으면 ReceiveAsync()를 즉시 호출 
                if (pending == false)
                    OnRecvCompleted(null, _recvArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterRecv Failed {e}");
            }
        }

        // 호출되는 경우는 두 가지
        // 1. _socket.ReceiveAsync(_recvArgs);가 true일 때 나중에 _recvArgs.Completed로 등록된 이벤트로 비동기적으로 호출
        // 2. _socket.ReceiveAsync(_recvArgs);가 false(동기적으로 완료)면 직접 호출
        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // 수신한 데이터 양(args.BytesTransferred)만큼 Write 커서 이동
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // OnRecv() 컨텐츠 쪽으로 데이터를 넘겨주고
                    // 처리된 데이터 양을 processLen에 저장
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }

                    // 처리한 데이터 양(processLen)만큼 Read 커서 이동
                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }

        #endregion
    }
}
