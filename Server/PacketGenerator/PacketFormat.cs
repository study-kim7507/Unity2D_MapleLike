using System;
using System.Collections.Generic;
using System.Text;

namespace PacketGenerator
{
    class PacketFormat
    {
        // {0} 패킷 등록
        public static string managerFormat =
@"using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance {{ get {{ return _instance; }} }}
	#endregion

	PacketManager()
	{{
		Register();
	}}

	// ushort : 패킷 아이디, Action<> : 지금 수신한 이 패킷은 어떤 패킷인가?
	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	// ushort : 패킷 아이디, Action<> : _onRecv의 패킷 처리 로직은 무엇인가?
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();
		
	public Action<PacketSession, IMessage, ushort> CustomHandler {{ get; set; }}

	// Register()는 멀티스레드가 개입 되기 전 호출되어야함
	public void Register()
	{{{0}
	}}

	// 지금 수신한 이 패킷을 딕셔너리에서 찾고
	// 해당 패킷을 Invoke(여기서는 MakePacket())함
	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}}

	// OnRecvPacket()로부터 Invoke로 호출 되어
	// 이 패킷의 처리 로직을 Invoke(여기서는 PacketHandler.cs의 각 처리 로직)
	// 유니티는 메인스레드에서 패킷을 처리해야 하기 때문에 분기가 나뉘어진 모습
	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
		
		// 클라이언트(유니티)에서 사용되는 분기
		if (CustomHandler != null)
		{{
			CustomHandler.Invoke(session, pkt, id);
		}}
		// 서버에서 사용되는 분기
		else
		{{
			Action<PacketSession, IMessage> action = null;
			if (_handler.TryGetValue(id, out action))
				action.Invoke(session, pkt);
		}}
	}}

	// 유니티에서 큐에 담아둔 해당 패킷의 처리 로직이 무엇인지
	// PacketHandler.cs로 유도해주는 함수 
	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}}
}}";

        // {0} MsgId
        // {1} 패킷 이름
        public static string managerRegisterFormat =
@"		
		_onRecv.Add((ushort)MsgId.{0}, MakePacket<{1}>);
		_handler.Add((ushort)MsgId.{0}, PacketHandler.{1}Handler);";

    }
}
