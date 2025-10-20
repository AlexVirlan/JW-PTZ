using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWPTZ.Entities
{
    public class MethodResponse
    {
        #region Properties
        public bool Success { get; set; }
        public string? Message { get; set; }

        public Exception? Exception { get; set; }

        #region Static properties
        [JsonIgnore]
        public static MethodResponse Successful { get; } = new MethodResponse(success: true);

        [JsonIgnore]
        public static MethodResponse Unsuccessful { get; } = new MethodResponse(success: false);
        #endregion
        #endregion

        #region Constructors
        public MethodResponse() { }

        public MethodResponse(bool success)
        {
            Success = success;
        }

        public MethodResponse(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public MethodResponse(Exception exception)
        {
            Success = false;
            Message = exception.Message;
            Exception = exception;
        }
        #endregion

        #region Methods
        public static MethodResponse SuccessfulWithMessage(string message)
        {
            return new MethodResponse(success: true, message: message);
        }

        public static MethodResponse UnsuccessfulWithMessage(string message)
        {
            return new MethodResponse(success: false, message: message);
        }

        public string ToJson(bool indented = false)
        {
            Formatting formatting = indented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(this, formatting);
        }
        #endregion
    }
}
