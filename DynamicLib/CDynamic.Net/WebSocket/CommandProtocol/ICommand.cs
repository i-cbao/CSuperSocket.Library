using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.WebSocket.CommandProtocol;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    /// <summary>
    /// 自定义命令接口 基于二进制WebSocket命令
    /// </summary>
    public interface ICommand
    {
     //   WSBinaryCommandType Command { get; }

        /// <summary>
        /// 传入命令是否能被当前命令处理
        /// </summary>
        /// <param name="command">传入的WebSocket命令</param>
        /// <returns></returns>
        bool CanExecute(WSCommandTypeBase command);

      //  WSBinaryCommandType Create(WSBinaryCommandType command);

        /// <summary>
        /// 将WebSocket命令转换为ICommand
        /// </summary>
        /// <param name="command"></param>
        ICommand Parse(WSCommandTypeBase command);

        /// <summary>
        /// 将命令值填充到指定WebSocket命令
        /// </summary>
        /// <param name="command"></param>
        void ToCommand(WSCommandTypeBase command);

        /// <summary>
        /// 将当前命令转换为Websocket命令
        /// </summary>
        /// <returns></returns>
        //WSCommandTypeBase ToCommand(IWebSocketCommandFactory commandFactory);

        /// <summary>
        /// 获取一个空命令
        /// </summary>
        /// <returns></returns>
        //WSCommandTypeBase GetEmptyCommand(IWebSocketCommandFactory commandFactory);


        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="command">请求的WebSocket命令</param>
        /// <param name="session">与命令关联的会话</param>
        /// <returns></returns>
        ICommand Execute(WSCommandTypeBase command, ExecuteCommandContext context);
    }
}
