using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDEFReadWriteTool
{
    public class OperationResult
    {
        /// <summary>
        /// 是否成功：成功为 true，失败为 false
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// 成功时返回的对象
        /// </summary>
        public object Data { get; private set; }

        /// <summary>
        /// 操作日志
        /// </summary>
        public string Log { get; private set; }

        /// <summary>
        /// 失败时的错误代码
        /// </summary>
        public int ErrorCode { get; private set; }

        /// <summary>
        /// 失败时的错误信息
        /// </summary>
        public string ErrorMessage { get; private set; }

        private OperationResult() { }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static OperationResult Success(object data, string log)
        {
            return new OperationResult
            {
                IsSuccess = true,
                Data = data,
                Log = log,
                ErrorCode = 0,
                ErrorMessage = null
            };
        }

        public static OperationResult Success(string log)
        {
            return new OperationResult
            {
                IsSuccess = true,
                Log = log,
                ErrorCode = 0,
                ErrorMessage = null
            };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static OperationResult Failure(int errorCode, string errorMessage,string log)
        {
            return new OperationResult
            {
                IsSuccess = false,
                Data = null,
                Log = log,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
        }

        public static OperationResult Failure(int errorCode, string errorMessage)
        {
            return new OperationResult
            {
                IsSuccess = false,
                Data = null,
                Log = null,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
        }
    }
}
