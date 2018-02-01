using System;
using ICB.MiddleWare.Core;
using ICB.MiddleWare.Core.Config;
using ICB.MiddleWare.Core.Plugin;
using Microsoft.AspNetCore.Mvc;

namespace TestPlug
{
       /// <summary>
       /// 测试插件，用于测试插件的访问是否正常
       /// </summary>
    [Route("api/Test")]
    [DynamicWebApi("CA281DEB-0E27-4A73-A4D2-CC0FE4D8420F", "测试插件",Author ="zhouhuajun")]
    public class TestController : WebApiPluginBase
    {
        /// <summary>
        /// 测试get方法
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Route("action route")]
        public ResultModel Get()
        {
            ResultModel res = new ResultModel();
            res.data = "ok";
            res.state = true;
            res.msg = "测试成功！";
            return res;
        }
        /// <summary>
        /// 测试post提交,返回提交值
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        [HttpPost]
        //[Route("action route")]
        public ResultModel Post(string postData)
        {
            ResultModel res = new ResultModel();
            res.data = "ok";
            res.state = true;
            res.msg = "传入的数据为："+postData;
            return res;
        }
    }
      
}
