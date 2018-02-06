using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Core.Runtime;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    public class DefaultWebSocketCommandFactory : IWebSocketCommandFactory
    {
        public MessageContentType ContentType { get; protected set; }


        private Dictionary<MessageContentType, IWebSocketCommandFactory> commandFactoryDic = new Dictionary<MessageContentType, IWebSocketCommandFactory>()
        {
            {MessageContentType.Binary, new DefaultWebSocketCommandFactory(MessageContentType.Binary)},
            {MessageContentType.Text, new DefaultWebSocketCommandFactory(MessageContentType.Text)}
        };

        public DefaultWebSocketCommandFactory(MessageContentType contentType)
        {
            ContentType = contentType;
        }

        #region IWebSocketCommandFactory 成员

        public WSCommandTypeBase CreateCommand()
        {
            if (ContentType == MessageContentType.Text)
            {
                return new WSCommandType();
            }
            else if (ContentType == MessageContentType.Binary)
            {
                return new WSBinaryCommandType();
            }

            throw new NotSupportedException("不支持的命令格式：" + ContentType.ToString());
        }

        public IWebSocketCommandFactory GetCommandFactory(MessageReceivedEventArgs args)
        {
            if (args.ContentType == MessageContentType.Text)
            {
                return commandFactoryDic[MessageContentType.Text];
            }
            else if (args.ContentType == MessageContentType.Binary)
            {
                return commandFactoryDic[MessageContentType.Binary];
            }

            throw new NotSupportedException("不支持的命令格式：" + ContentType.ToString());
        }

        public WSCommandTypeBase GetCommand(MessageReceivedEventArgs args)
        {
            WSCommandTypeBase command = null;
            switch (ContentType)
            {
                case MessageContentType.Text:
                    command = SerializationUtility.ToObject<WSCommandType>(args.Content);
                    break;
                case MessageContentType.Binary:
                    command = BinaryCommandTypeSerializer.ToCommandType(args.Data);
                    break;
            }

            return command;
        }


        public void SetReplyCommandData(WSCommandTypeBase command, MessageReceivedEventArgs args)
        {
            if (ContentType == MessageContentType.Text)
            {
                args.ResponseContent = SerializationUtility.ToXmlString(command);
            }
            else if (ContentType == MessageContentType.Binary)
            {
                args.ResponseData = BinaryCommandTypeSerializer.ToBinary(command as WSBinaryCommandType);
            }

            throw new NotSupportedException("不支持的命令格式：" + ContentType.ToString());
        }
        #endregion
    }
}
