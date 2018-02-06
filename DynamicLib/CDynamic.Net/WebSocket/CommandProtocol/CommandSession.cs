using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.WebSocket;
using System.Threading;
using Dynamic.Core.Runtime;
using System.Diagnostics;
using Dynamic.Core.Log;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    public delegate void CommandResponse(CommandItemBase command );

    public delegate void CommandCallback<T>(T command) where T : CommandBase, new();

    public class CommandSession : EventArgs
    {
        public WebSocketSessionBase Session { get; set; }

        public DateTime LastRequestTime { get; set; }

        public List<CommandItemBase> Commands { get; private set; }

      //  public MessageContentType ContentType { get; set; }
        public ICommandParser CommandParser { get; set; }

        private ILogger logger = LoggerManager.GetLogger("CommandSession");

        public Object Context { get; set; }

        public object SyncLocker = new object();

        public CommandSession()
        {
            Commands = new List<CommandItemBase>();
        }


        #region SendCommand
        public void SendCommand(WSCommandTypeBase command)
        {
            SendCommand(command, null);
        }

        public virtual void SendCommand(WSCommandTypeBase command, CommandResponse callback)
        {
            if (command == null)
                return;
            command.RequestID = Guid.NewGuid();
            Session.LastRequestTime = DateTime.Now;
            lock (SyncLocker)
            {
                Commands.Add(new CommandItem<WSCommandTypeBase>() { CommandRequest = command, CommandResponse = null, RetryCount = 0, ResponseCallback = callback, RequestTime = DateTime.Now, IsSync = false });
            }
            logger.Trace("发送命令：{0}", command.CommandName);
            Session.SendMessage(command.ToBinary(), command.TransferEncoder);
        }


        public virtual CommandItem<WSCommandTypeBase> SendCommandSync(WSCommandTypeBase command,  TimeSpan timeout)
        {
            if (command == null)
                return null;
            command.RequestID = Guid.NewGuid();
            Session.LastRequestTime = DateTime.Now;

            CommandItem<WSCommandTypeBase> ci = new CommandItem<WSCommandTypeBase>()
            {
                CommandRequest = command,
                CommandResponse = null,
                RetryCount = 0,
                ResponseCallback = null,
                RequestTime = DateTime.Now,
                IsSync = true,
                Wait = new System.Threading.AutoResetEvent(false)
            };
            lock (SyncLocker)
            {
                Commands.Add(ci);
            }

            logger.Trace("发送命令：{0}", command.CommandName);
            Session.SendMessage(command.ToBinary(), command.TransferEncoder);


            ci.Wait.WaitOne(timeout);

            return ci;
        }


        #endregion


        #region ICommand
        public void SendCommand(ICommand command)
        {
            SendCommand(command, (CommandResponse)null);
        }

        public virtual void SendCommand(ICommand command, CommandResponse callback)
        {
            if (command == null)
                return;
            WSCommandTypeBase wsCommand = CommandParser.Create();
            command.ToCommand(wsCommand);
            wsCommand.RequestID = Guid.NewGuid();
            Session.LastRequestTime = DateTime.Now;
            lock (SyncLocker)
            {
                Commands.Add(new CommandItem<WSCommandTypeBase>() { CommandRequest = wsCommand, CommandResponse = null, RetryCount = 0, ResponseCallback = callback, RequestTime = DateTime.Now, IsSync = false });
            }
            logger.Trace("发送命令：{0}", wsCommand.CommandName);
            Session.SendMessage(CommandParser.ToBinary(wsCommand), CommandParser.TransferEncoder);
        }

        public virtual void SendCommand<T>(ICommand command, CommandCallback<T> callback)
            where T : CommandBase, new()
        {
            SendCommand(command, new CommandResponse((ci) =>
            {
                if (callback != null && ci != null )
                {
                    CommandItem<WSCommandTypeBase> actualCi = ci as CommandItem<WSCommandTypeBase>;
                    if (actualCi != null && actualCi.CommandResponse != null)
                    {
                        T responseCmd = new T();
                        responseCmd.LoadCommand(actualCi.CommandResponse);
                        callback(responseCmd);
                    }
                }
            }));
        }

        public virtual void SendReplyCommand(ICommand command, Guid requestGuid)
        {
            if (command == null)
                return;
            WSCommandTypeBase wsCommand = CommandParser.Create();
            command.ToCommand(wsCommand);
            wsCommand.RequestID = requestGuid;
           
            Session.SendMessage(CommandParser.ToBinary(wsCommand), CommandParser.TransferEncoder);
        }

        public virtual void SendReplyCommand(WSCommandTypeBase command)
        {
            if (command == null)
                return;

            Session.SendMessage(CommandParser.ToBinary(command), CommandParser.TransferEncoder);
        }


        public virtual CommandItem<WSCommandTypeBase> SendCommandSync(ICommand command, TimeSpan timeout)
        {
            if (command == null)
                return null;
            WSCommandTypeBase wsCommand = CommandParser.Create();
            command.ToCommand(wsCommand);
            
            wsCommand.RequestID = Guid.NewGuid();
            Session.LastRequestTime = DateTime.Now;

            CommandItem<WSCommandTypeBase> ci = new CommandItem<WSCommandTypeBase>()
            {
                CommandRequest = wsCommand,
                CommandResponse = null,
                RetryCount = 0,
                ResponseCallback = null,
                RequestTime = DateTime.Now,
                IsSync = true,
                Wait = new System.Threading.AutoResetEvent(false)
            };
            lock (SyncLocker)
            {
                Commands.Add(ci);
            }

            logger.Trace("发送命令：{0}", wsCommand.CommandName);
            bool isSended = Session.SendMessage(CommandParser.ToBinary(wsCommand), CommandParser.TransferEncoder);

            if (!isSended)
            {
                return ci;
            }

            ci.Wait.WaitOne(timeout);

            return ci;
        }


        public virtual T SendCommandSync<T>(ICommand command, TimeSpan timeout) where T : CommandBase, new()
        {
            CommandItem<WSCommandTypeBase> response = SendCommandSync(command, timeout);
            if (response != null && response.CommandResponse != null)
            {
                CommandBase responseCommand = new T();
                responseCommand.LoadCommand(response.CommandResponse);
                return responseCommand as T;
            }

            return null;
        }
        #endregion

        #region TextCommand
        //public void SendCommand(WSCommandType command)
        //{
        //    SendCommand(command, MessageContentType.Text, null);
        //}

        //public void SendCommand(WSCommandType command, CommandResponse callback)
        //{
        //    //if (command == null)
        //    //    return;
        //    //command.RequestID = Guid.NewGuid();
        //    //Session.LastRequestTime = DateTime.Now;
        //    //lock (lockerObj)
        //    //{
        //    //    Commands.Add(new CommandItem<WSCommandType>() { CommandRequest = command, CommandResponse = null, RetryCount = 0, ResponseCallback = callback, RequestTime = DateTime.Now, IsSync = false });
        //    //}
        //    //string commandText = SerializationUtility.ToXmlString(command);
        //    Session.SendMessage((WSCommandTypeBase)command, MessageContentType.Text, callback);
        //}

      
       // public CommandItem<WSCommandType> SendCommandSync(WSCommandType command, TimeSpan timeout)
       // {
            //if (command == null)
            //    return null;
            //command.RequestID = Guid.NewGuid();
            //Session.LastRequestTime = DateTime.Now;

            //CommandItem<WSCommandType> ci = new CommandItem<WSCommandType>()
            //{
            //    CommandRequest = command,
            //    CommandResponse = null,
            //    RetryCount = 0,
            //    ResponseCallback = null,
            //    RequestTime = DateTime.Now,
            //    IsSync = true,
            //    Wait = new System.Threading.AutoResetEvent(false)
            //};
            //lock (lockerObj)
            //{
            //    Commands.Add(ci);
            //}
            //string commandText = SerializationUtility.ToXmlString(command);
            //Session.SendMessage(commandText);
            

            //ci.Wait.WaitOne(timeout);

            //return ci;
      //  }
        #endregion


        #region BinaryCommand
        //public void SendCommand(WSBinaryCommandType command)
        //{
        //    SendCommand(command, null);
        //}

        //public void SendCommand(WSBinaryCommandType command, CommandResponse callback)
        //{
        //    if (command == null)
        //        return;
        //    command.RequestID = Guid.NewGuid();
        //    Session.LastRequestTime = DateTime.Now;
        //    lock (lockerObj)
        //    {
        //        Commands.Add(new CommandItem<WSBinaryCommandType>() { CommandRequest = command, CommandResponse = null, RetryCount = 0, ResponseCallback = callback, RequestTime = DateTime.Now, IsSync = false });
        //    }
        //     byte[] commandData = BinaryCommandTypeSerializer.ToBinary(command);
        //    Session.SendMessage(commandData);
        //}

        //public CommandItem<WSBinaryCommandType> SendCommandSync(WSBinaryCommandType command, TimeSpan timeout)
        //{
        //    if (command == null)
        //        return null;
        //    command.RequestID = Guid.NewGuid();
        //    Session.LastRequestTime = DateTime.Now;

        //    CommandItem<WSBinaryCommandType> ci = new CommandItem<WSBinaryCommandType>()
        //    {
        //        CommandRequest = command,
        //        CommandResponse = null,
        //        RetryCount = 0,
        //        ResponseCallback = null,
        //        RequestTime = DateTime.Now,
        //        IsSync = true,
        //        Wait = new System.Threading.AutoResetEvent(false)
        //    };
        //    lock (lockerObj)
        //    {
        //        Commands.Add(ci);
        //    }
        //    byte[] commandData = BinaryCommandTypeSerializer.ToBinary(command);
        //    Session.SendMessage(commandData);


        //    ci.Wait.WaitOne(timeout);

        //    return ci;
        //}
        #endregion

        public void ClearRequestCommand(DateTime beforeTime)
        {
            if (Commands == null)
                return;

            lock (SyncLocker)
            {
                Commands.Where(x => x.RequestTime < beforeTime).ToList().All(c =>
                {
                    Debug.WriteLine("移除超时命令：" + c.RequestTime.ToString());
                    Commands.Remove(c);
                    if (c.ResponseCallback != null)
                    {
                        c.ResponseCallback(c);
                    }
                    return true;
                });
            }
            
        }
    }

    public abstract class CommandItemBase
    {
        public bool IsSync { get; set; }

        internal AutoResetEvent Wait { get; set; }

        public DateTime RequestTime { get; set; }

        public int RetryCount { get; set; }

        public CommandResponse ResponseCallback { get; set; }
    }

    public class CommandItem<T> : CommandItemBase
    {
        public T CommandRequest { get; set; }

        public T CommandResponse { get; set; }

    }

    //public class BinaryCommandItem
    //{
    //}

}
