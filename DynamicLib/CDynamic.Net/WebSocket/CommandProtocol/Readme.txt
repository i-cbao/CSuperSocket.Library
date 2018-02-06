WebSocket下，基于字符串命令协议的服务端与客户端


二进制命令格式：
byte[2]				..						固定标头			0x0000
byte[16]			RequestID				命令全局唯一码		Guid
long				OccurTime				命令发送时间		DateTime
String.Data			CommandName				命令名称			String
String Data			CommandType				命令类型			String
int					ParameterCount			参数个数			int
+
String Data			ParameterName			参数名称			String
int					ParameterValueLength	参数值长度			int
byte[]				ParameterData			参数值				Byte[]


String Data类型：
int					StringLength			字符串长度			int
byte[]				StringData				字符串UTF8编码数据	byte[]


How To:
	服务端可通过实现WebSocketSessionBase子类以及对应的工厂接口ISocketSessionFactory来实现对底层数据接收与发送的控制。
	客户端可通过实现WebSocketSessionBase子类以及对应的工厂接口IWebSocketClientSessionFactory来实现对底层数据接收与发送的控制。
	
	通过向CommandClient或CommandServer中的CommandList添加自定义命令，来实现自定义的命令。