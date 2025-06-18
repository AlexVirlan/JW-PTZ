using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWPTZ.Entities
{
    public class LogData
    {
        #region Properties
        public DateTime DateTime { get; } = DateTime.Now;
        public LogType? LogType { get; set; }
        public string? File {  get; set; }
        public string? Method {  get; set; }
        public int? Line {  get; set; }
        public string? Message { get; set; }
        public string? StackTrace { get; set; }
        public string? LogInfo { get; set; }
        #endregion

        #region Constructors
        public LogData() { }
        #endregion

        #region Methods
        public override string ToString() => ToJsonString();
        public string ToJsonString(Formatting formatting = Formatting.None) => JsonConvert.SerializeObject(this, formatting);
        #endregion
    }
}
