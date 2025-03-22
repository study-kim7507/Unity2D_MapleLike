protoc.exe -I=./ --csharp_out=./ ./Protocol.proto 
IF ERRORLEVEL 1 PAUSE

START ../../../PacketGenerator/bin/PacketGenerator.exe ./Protocol.proto
XCOPY /Y Protocol.cs "../../../../Assets/Scripts/Packet"
XCOPY /Y Protocol.cs "../../../ServerContents/Packet"
XCOPY /Y Protocol.cs "../../../DummyClient/Packet"
XCOPY /Y ClientPacketManager.cs "../../../../Assets/Scripts/Packet"
XCOPY /Y ClientPacketManager.cs "../../../DummyClient/Packet"
XCOPY /Y ServerPacketManager.cs "../../../ServerContents/Packet"